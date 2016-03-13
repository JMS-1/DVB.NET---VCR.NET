using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using JMS.DVB.Administration.ProfileManager;
using JMS.DVB.Algorithms;
using JMS.DVB.TS;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Implementierung eines <i>Card Servers</i>, der alle Befehle direkt in der Anwendung
    /// ausführt.
    /// </summary>
    public partial class InMemoryCardServer : ServerImplementation
    {
        /// <summary>
        /// Wird als Rückgabewert verwendet, wenn eine Operation verzögert ausgeführt werden soll.
        /// </summary>
        private static readonly object DelayedOperationTag = new object();

        /// <summary>
        /// Das gerade verwendete Geräteprofil.
        /// </summary>
        public Profile Profile { get; private set; }

        /// <summary>
        /// Die Wartezeit zwischen zwei Prüfungen auf den Gesamtdatenstrom.
        /// </summary>
        private TimeSpan m_GroupWatchDogInterval = TimeSpan.FromSeconds( 15 );

        /// <summary>
        /// Die Wartezeit zwischen zwei Prüfungen auf einem entschlüsselten Nutzdatenstrom.
        /// </summary>
        private TimeSpan m_DecryptionWatchDogInterval = TimeSpan.FromSeconds( 10 );

        /// <summary>
        /// Die Wartezeit zwischen zwei Prüfungen auf eine Änderung der Senderdaten.
        /// </summary>
        private TimeSpan m_RetestWatchDogInterval = TimeSpan.FromSeconds( 0 );

        /// <summary>
        /// Der <see cref="Thread"/>, auf dem die Augaben ausgeführt werden.
        /// </summary>
        private Thread m_Thread;

        /// <summary>
        /// Ein <see cref="Thread"/>, der eine periodische Aktivierung auslöst.
        /// </summary>
        private Thread m_IdleThread;

        /// <summary>
        /// Gesetzt, so lange die Bearbeitung aktiv ist.
        /// </summary>
        private volatile bool m_Running = true;

        /// <summary>
        /// Wird ausgelöst, wenn ein neuer Auftrag bearbeitet werden soll.
        /// </summary>
        private AutoResetEvent m_Trigger = new AutoResetEvent( false );

        /// <summary>
        /// Verwaltet alle aktiven Quellen.
        /// </summary>
        public Dictionary<SourceIdenfierWithKey, ActiveStream> Streams { get; private set; }

        /// <summary>
        /// Wird gesetzt, wenn <see cref="m_IdleThread"/> beendet werden soll.
        /// </summary>
        private ManualResetEvent m_IdleDone = new ManualResetEvent( false );

        /// <summary>
        /// Die auszuführende Operation.
        /// </summary>
        private volatile Func<Hardware, object> m_Operation;

        /// <summary>
        /// Ermittelt Daten zu den NVOD Diensten.
        /// </summary>
        private ServiceParser m_ServiceParser;

        /// <summary>
        /// Meldet oder legt fest, wann das letzte Mal Daten zur aktuellen Quellegruppe
        /// empfangen wurden.
        /// </summary>
        private DateTime m_LastGroupInfoTime = DateTime.UtcNow;

        /// <summary>
        /// Zählt, wie oft ein Neustart der aktuellen Gruppe ausgeführt wurde.
        /// </summary>
        public int GroupRestart { get; private set; }

        /// <summary>
        /// Wird aufgerufen, wenn die Leerlauffunktion aktiviert wurde.
        /// </summary>
        public event Action<InMemoryCardServer, Hardware> OnAfterIdleProcessing;

        /// <summary>
        /// Erzeugt eine neue Implementierung.
        /// </summary>
        internal InMemoryCardServer()
        {
            // Create helpers
            Streams = new Dictionary<SourceIdenfierWithKey, ActiveStream>();
        }

        /// <summary>
        /// Führt die eintreffenden Aufträge aus.
        /// </summary>
        private void WorkerThread( object reset )
        {
            // Always use the configured language
            UserProfile.ApplyLanguage();

            // Use hardware manager
            using (HardwareManager.Open())
            {
                // Device to use
                Hardware device;

                // Try to attach profile
                try
                {
                    // Open it
                    device = HardwareManager.OpenHardware( Profile );

                    // Should reset
                    if ((bool) reset)
                        device.ResetWakeupDevice();

                    // Report success
                    ActionDone( null, null );
                }
                catch (Exception e)
                {
                    // Report error
                    ActionDone( e, null );

                    // Did it
                    return;
                }

                // Loop
                for (var lastIdle = DateTime.UtcNow; m_Running; )
                {
                    // Wait for new item to process
                    m_Trigger.WaitOne();

                    // Load operation
                    var operation = m_Operation;

                    // Clear
                    m_Operation = null;

                    // Process
                    if (m_Running)
                        if (operation != null)
                            try
                            {
                                // Process
                                var result = operation( device );

                                // See if there is at least one active source with a program guide running
                                var withGuide = Streams.Values.FirstOrDefault( stream => stream.IsProgamGuideActive.GetValueOrDefault( false ) );

                                // Check for service parser operation
                                if (m_ServiceParser == null)
                                {
                                    // Can be activated it at least one source with active program guide
                                    if (withGuide != null)
                                        try
                                        {
                                            // Create
                                            m_ServiceParser = new ServiceParser( Profile, withGuide.Manager.Source );
                                        }
                                        catch
                                        {
                                            // Ignore any error
                                            m_ServiceParser = null;
                                        }
                                }
                                else
                                {
                                    // Only allowed if there is at least one source with active program guide
                                    if (withGuide == null)
                                        DisableServiceParser();
                                }

                                // Done
                                if (!ReferenceEquals( result, DelayedOperationTag ))
                                    ActionDone( null, result );
                            }
                            catch (Exception e)
                            {
                                // Report
                                ActionDone( e, null );
                            }

                    // See if idle processing should be started
                    if (m_Running)
                        if ((DateTime.UtcNow - lastIdle).TotalSeconds >= 5)
                        {
                            // Run
                            OnIdle( device );

                            // Reset
                            lastIdle = DateTime.UtcNow;
                        }
                }

                // Reset EPG flag
                EPGProgress = null;

                // Clear EPG list to preserve memory
                m_EPGItems.Clear();

                // Reset PSI scan flag
                using (var scanner = m_Scanner)
                {
                    // Forget all
                    m_ScanProgress = -1;
                    m_Scanner = null;
                }

                // Just be safe
                try
                {
                    // Final cleanup
                    RemoveAll();
                }
                catch
                {
                    // Ignore any error
                }
            }
        }

        /// <summary>
        /// Prüft, ob auf allen Datenströmen etwas empfangen wird.
        /// </summary>
        /// <param name="device">Die verwendete Hardware.</param>
        private void TestForRestart( Hardware device )
        {
            // No active group
            if (device.CurrentGroup == null)
                return;

            // Get the group information
            var info = device.GetGroupInformation( 15000 );
            if (info == null)
            {
                // See if we are already out of retries
                if (GroupRestart >= 3)
                    return;

                // See if we are waiting long enough
                if ((DateTime.UtcNow - m_LastGroupInfoTime) <= m_GroupWatchDogInterval)
                    return;

                // Prepare to restart all streams
                foreach (var stream in Streams.Values)
                    try
                    {
                        // Stop it
                        stream.Close();
                    }
                    catch
                    {
                        // Ignore any error
                    }

                // Count the restart
                m_LastGroupInfoTime = DateTime.UtcNow;
                GroupRestart += 1;

                // Get the current settings
                var location = device.CurrentLocation;
                var group = device.CurrentGroup;

                // Detach from source group
                device.SelectGroup( null, null );

                // Reactivate source group
                device.SelectGroup( location, group );

                // Done - code following will try to restart all streams
                return;
            }

            // Remember time
            m_LastGroupInfoTime = DateTime.UtcNow;

            // Check for any decrypted channel
            foreach (var stream in Streams.Values)
                stream.TestDecryption( m_DecryptionWatchDogInterval );
        }

        /// <summary>
        /// Wird periodisch ausgelöst.
        /// </summary>
        /// <param name="device">Das aktuell verwendete DVB.NET Gerät.</param>
        private void OnIdle( Hardware device )
        {
            // Force reload of group information if receiving sources
            if (Streams.Count > 0)
            {
                // Make sure, hat stream information is reloaded
                device.ResetInformationReaders();

                // Test for stream restart
                TestForRestart( device );

                // Just recheck all streams
                foreach (var stream in Streams.Values)
                    try
                    {
                        // Retest configuration
                        stream.Refresh( m_RetestWatchDogInterval );
                    }
                    catch
                    {
                        // Ignore any error
                    }
            }

            // Process EPG
            CollectProgramGuide( device );

            // Process scan
            CheckScanAbort();

            // Check for extensions
            var onIdle = OnAfterIdleProcessing;
            if (onIdle != null)
                onIdle( this, device );
        }

        /// <summary>
        /// Liest eine verfeinerte Einstellung für Aufzeichnungen.
        /// </summary>
        /// <param name="name">Der Name des Parameters.</param>
        /// <param name="interval">Der aktuell verwendete Defaultwert.</param>
        /// <param name="zeroMapping">Der Wert der zu verwenden ist, wenn der Parameter deaktiviert wurde.</param>
        /// <returns></returns>
        private void ReadRecordingParameter( string name, ref TimeSpan interval, TimeSpan zeroMapping )
        {
            // Load the setting
            var setting = Profile.GetParameter( name );

            // Read the value
            uint seconds;
            if (!uint.TryParse( setting, out seconds ))
                return;

            // Load
            if (seconds < 1)
                interval = zeroMapping;
            else
                interval = TimeSpan.FromSeconds( seconds );
        }

        /// <summary>
        /// Aktiviert die Nutzung eines DVB.NET Geräteprofils.
        /// </summary>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <param name="reset">Gesetzt, wenn das zugehörige Windows Gerät neu initialisiert werden soll.</param>
        /// <param name="disablePCRFromH264">Wird gesetzt um zu verhindern, dass die Systemzeit (PCR) aus
        /// dem H.264 Bildsignal ermittelt wird, da dieser Mechanismus hochgradig unsicher ist.</param>
        /// <param name="disablePCRFromMPEG2">Wird gesetzt um zu verhindern, dass die Systemzeit (PCR) aus
        /// dem MPEG2 Bildsignal ermittelt wird, da dieser Mechanismus hochgradig unsicher ist.</param>
        /// <exception cref="CardServerException">Es existiert kein Geräteprofil mit dem
        /// angegebenen Namen.</exception>
        protected override void OnAttachProfile( string profileName, bool reset, bool disablePCRFromH264, bool disablePCRFromMPEG2 )
        {
            // Overwrite flags
            Manager.DisablePCRForSDTV = disablePCRFromMPEG2;
            Manager.DisablePCRForHDTV = disablePCRFromH264;

            // Load the profile
            Profile = ProfileManager.FindProfile( profileName );

            // Validate
            if (Profile == null)
                CardServerException.Throw( new NoProfileFault( profileName ) );

            // Load special overwrites
            ReadRecordingParameter( RecordingSettings.DecryptWatchDogIntervalName, ref m_DecryptionWatchDogInterval, TimeSpan.MaxValue );
            ReadRecordingParameter( RecordingSettings.PSIWatchDogIntervalName, ref m_RetestWatchDogInterval, m_RetestWatchDogInterval );
            ReadRecordingParameter( RecordingSettings.StreamWatchDogIntervalName, ref m_GroupWatchDogInterval, TimeSpan.MaxValue );

            // Create the thread
            m_Thread = new Thread( WorkerThread ) { Name = "DVB.NET Card Server Worker Thread", Priority = ThreadPriority.AboveNormal };
            m_Thread.SetApartmentState( ApartmentState.STA );
            m_Thread.Start( reset );

            // Create the idle thread
            m_IdleThread = new Thread( Idle ) { Name = "DVB.NET Card Server Idle Thread" };
            m_IdleThread.Start();
        }

        /// <summary>
        /// Löst in einem festen Intervall die Bearbeitung aus.
        /// </summary>
        private void Idle()
        {
            // Just wait
            while (!m_IdleDone.WaitOne( 2500, false ))
                m_Trigger.Set();
        }

        /// <summary>
        /// Bereitet eine Anfrage zur Ausführung vor.
        /// </summary>
        /// <param name="operation">Die gewünschte Aktion.</param>
        private void Start( Func<Hardware, object> operation )
        {
            // Remember
            m_Operation = operation;

            // Process
            m_Trigger.Set();
        }

        /// <summary>
        /// Bereitet eine Anfrage zur Ausführung vor.
        /// </summary>
        /// <param name="operation">Die gewünschte Aktion.</param>
        private void Start( Action<Hardware> operation )
        {
            // Forward
            Start( device => { operation( device ); return null; } );
        }

        /// <summary>
        /// Bereitet eine Anfrage zur Ausführung vor.
        /// </summary>
        /// <param name="operation">Die gewünschte Aktion.</param>
        private void Start( Func<object> operation )
        {
            // Forward
            Start( device => { return operation(); } );
        }

        /// <summary>
        /// Bereitet eine Anfrage zur Ausführung vor.
        /// </summary>
        /// <param name="operation">Die gewünschte Aktion.</param>
        private void Start( Action operation )
        {
            // Forward
            Start( device => { operation(); return null; } );
        }

        /// <summary>
        /// Wählt eine Quellgruppe an.
        /// </summary>
        /// <param name="device">Die zu verwendende Hardware.</param>
        /// <param name="selection">Die gewünschte Quellgruppe.</param>
        private void SelectGroup( Hardware device, SourceSelection selection )
        {
            // Check mode
            if (EPGProgress.HasValue)
                CardServerException.Throw( new EPGActiveFault() );
            if (m_ScanProgress >= 0)
                CardServerException.Throw( new SourceUpdateActiveFault() );

            // Stop all current recordings
            RemoveAll();

            // Forward
            device.SelectGroup( selection.Location, selection.Group );

            // Time to reset counters
            m_LastGroupInfoTime = DateTime.UtcNow;
            GroupRestart = 0;
        }

        /// <summary>
        /// Wählt eine bestimmte Quellgruppe (Transponder) an.
        /// </summary>
        /// <param name="selection">Die Beschreibung einer Quelle, deren Gruppe aktiviert werden soll.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Quellgruppe angegeben.</exception>
        /// <exception cref="CardServerException">Es wird bereits eine Anfrage ausgeführt.</exception>
        protected override void OnSelect( SourceSelection selection )
        {
            // Prepare operation            
            Start( device => { SelectGroup( device, selection ); } );
        }

        /// <summary>
        /// Ermittelt den aktuellen Zustand des Servers.
        /// </summary>
        /// <param name="device">Die verwendete DVB.NET Hardware.</param>
        /// <returns>Die gewünschten Daten.</returns>
        private ServerInformation CreateState( Hardware device )
        {
            // Create the current selection
            var selection =
                new SourceSelection
                {
                    Location = device.CurrentLocation,
                    Source = new SourceIdentifier(),
                    Group = device.CurrentGroup,
                    ProfileName = Profile.Name
                };

            // Create the full monty
            var info =
                new ServerInformation
                {
                    HasGroupInformation = (device.GetGroupInformation( 15000 ) != null),
                    ProgramGuideProgress = EPGProgress,
                    Selection = selection.SelectionKey
                };

            // Finish EPG collection
            if (info.ProgramGuideProgress.HasValue)
                info.CurrentProgramGuideItems = m_EPGItemCount;

            // Read scan progress
            int scan = m_ScanProgress;
            if (scan >= 0)
            {
                // Fill
                info.UpdateSourceCount = m_ScanSources;
                info.UpdateProgress = scan / 1000.0;
            }

            // All all streams
            info.Streams.AddRange( Streams.Values.Select( stream => stream.CreateInformation() ) );

            // Attach to NVOD service list
            var services = (m_ServiceParser == null) ? null : m_ServiceParser.ServiceMap;
            if (services != null)
                info.Services = services.Select( service =>
                    new ServiceInformation
                    {
                        Service = new SourceIdentifier( service.Key ),
                        UniqueName = service.Value
                    } ).ToArray();

            // Report
            return info;
        }

        /// <summary>
        /// Ermittelt den aktuellen Zustand des <i>Card Servers</i>.
        /// </summary>
        protected override void OnGetState()
        {
            // Process
            Start( device => CreateState( device ) );
        }

        /// <summary>
        /// Aktiviert eine einzelne Quelle für den <i>Zapping Modus</i>.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <param name="target">Die Netzwerkadresse, an die alle Daten versendet werden sollen.</param>
        protected override void OnSetZappingSource( SourceSelection source, string target )
        {
            // Prepare operation            
            Start( device =>
                {
                    // The next stream identifier to use
                    short nextStreamIdentifier = 0;
                    if (Streams.Count > 0)
                        nextStreamIdentifier = Streams.Values.First().Manager.NextStreamIdentifier;

                    // Activate the source group - will terminate all active streams
                    SelectGroup( device, source );

                    // Create optimizer and stream selector
                    var optimizer = new StreamSelectionOptimizer();
                    var streams =
                        new StreamSelection
                        {
                            AC3Tracks = { LanguageMode = LanguageModes.All },
                            MP2Tracks = { LanguageMode = LanguageModes.All },
                            SubTitles = { LanguageMode = LanguageModes.All },
                            ProgramGuide = true,
                            Videotext = true,
                        };

                    // Prepare to optimize
                    optimizer.Add( source, streams );

                    // See how many we are allowed to start
                    optimizer.Optimize();

                    // Create
                    var stream = new ActiveStream( Guid.Empty, source.Open( optimizer.GetStreams( 0 ) ), streams, null );
                    try
                    {
                        // Configure streaming target
                        stream.Manager.StreamingTarget = target;

                        // Attach next stream identifier
                        stream.Manager.NextStreamIdentifier = nextStreamIdentifier;

                        // See if we have to connect an optimizer for restarts
                        if (device.HasConsumerRestriction)
                            stream.EnableOptimizer( source );

                        // Try to start
                        stream.Refresh( m_RetestWatchDogInterval );

                        // Load
                        Streams.Add( stream.SourceKey, stream );
                    }
                    catch
                    {
                        // Cleanup
                        stream.Dispose();

                        // Forward
                        throw;
                    }

                    // Report state
                    return CreateState( device );
                } );
        }

        /// <summary>
        /// Ermittelt eine aktive Quelle.
        /// </summary>
        /// <param name="source">Die eindeutige Kennung der Quelle.</param>
        /// <returns>Der 0-basierte laufende Index der Quelle oder <i>-1</i>, 
        /// wenn diese nicht in Benutzung ist.</returns>
        public ActiveStream FindSource( SourceIdenfierWithKey source )
        {
            // Find the source
            ActiveStream stream;
            if (Streams.TryGetValue( source, out stream ))
                return stream;
            else
                return null;
        }

        /// <summary>
        /// Stellt den Empfang für eine Quelle ein.
        /// </summary>
        /// <param name="source">Die betroffene Quelle.</param>
        /// <param name="uniqueIdentifier">Der eindeutige Name der Quelle.</param>
        protected override void OnRemoveSource( SourceIdentifier source, Guid uniqueIdentifier )
        {
            // Prepare operation
            Start( () =>
                {
                    // Find the source
                    var stream = FindSource( new SourceIdenfierWithKey( uniqueIdentifier, source ) );
                    if (stream == null)
                        CardServerException.Throw( new NoSourceFault( source ) );

                    // Stop all activity on the source
                    using (stream)
                        Streams.Remove( stream.SourceKey );
                } );
        }

        /// <summary>
        /// Verändert den Netzwerkversand für eine aktive Quelle.
        /// </summary>
        /// <param name="source">Die Auswahl der Quelle.</param>
        /// <param name="uniqueIdentifier">Der eindeutige Name der Quelle.</param>
        /// <param name="target">Die Daten zum Netzwerkversand.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
        protected override void OnSetStreamTarget( SourceIdentifier source, Guid uniqueIdentifier, string target )
        {
            // Prepare operation
            Start( () =>
            {
                // Find the source
                var stream = FindSource( new SourceIdenfierWithKey( uniqueIdentifier, source ) );
                if (stream == null)
                    CardServerException.Throw( new NoSourceFault( source ) );

                // Process
                stream.Manager.StreamingTarget = target;
            } );
        }

        /// <summary>
        /// Stellt den Empfang für alle Quellen ein.
        /// </summary>
        protected override void OnRemoveAllSources()
        {
            // Dispatch
            Start( () =>
                {
                    // Check mode
                    if (EPGProgress.HasValue)
                        CardServerException.Throw( new EPGActiveFault() );
                    if (m_ScanProgress >= 0)
                        CardServerException.Throw( new SourceUpdateActiveFault() );

                    // Forward
                    RemoveAll();
                } );
        }

        /// <summary>
        /// Lädt eine Bibliothek mit Erweiterungen.
        /// </summary>
        /// <param name="actionAssembly">Der Binärcode der Bibliothek.</param>
        /// <param name="symbols">Informationen zum Debuggen der Erweiterung.</param>
        /// <returns>Steuereinheit zur Synchronisation des Aufrufs.</returns>
        protected override void OnLoadExtensions( byte[] actionAssembly, byte[] symbols )
        {
            // Load the assembly
            var assembly = Assembly.Load( actionAssembly, symbols );

            // Load types - server side
            Request.AddTypes( assembly );

            // Attach loader
            AppDomain.CurrentDomain.AssemblyResolve += ( sender, args ) =>
                {
                    // Check names
                    if (Equals( args.Name, assembly.FullName ))
                        return assembly;
                    else
                        return null;
                };

            // Report success
            ActionDone( null, null );
        }

        /// <summary>
        /// Führt eine Erweiterungsoperation aus.
        /// </summary>
        /// <param name="actionType">Die Klasse, von der aus die Erweiterungsmethode abgerufen werden kann.</param>
        /// <param name="parameters">Optionale Parameter zur Ausführung.</param>
        protected override void OnCustomAction<TInput, TOutput>( string actionType, TInput parameters )
        {
            // Process
            Start( device =>
                {
                    // Resolve the type
                    var type = Type.GetType( actionType, false );
                    if (type == null)
                        CardServerException.Throw( new NoSuchActionFault( actionType ) );

                    // Create the instance of the type
                    var customAction = Activator.CreateInstance( type, new object[] { this } ) as CustomAction<TInput, TOutput>;
                    if (customAction == null)
                        CardServerException.Throw( new NoSuchActionFault( actionType ) );

                    // Process
                    return customAction.Execute( device, parameters );
                } );
        }

        /// <summary>
        /// Aktiviert den Empfang einer Quelle.
        /// </summary>
        /// <param name="sources">Informationen zu den zu aktivierenden Quellen.</param>
        protected override void OnAddSources( ReceiveInformation[] sources )
        {
            // Prepare operation
            Start( device =>
                {
                    // Check mode
                    if (EPGProgress.HasValue)
                        CardServerException.Throw( new EPGActiveFault() );
                    if (m_ScanProgress >= 0)
                        CardServerException.Throw( new SourceUpdateActiveFault() );

                    // Force reload of group information to be current
                    device.ResetInformationReaders();

                    // Create optimizer
                    var optimizer = new StreamSelectionOptimizer();

                    // Source backmap
                    var infos = new Dictionary<SourceIdenfierWithKey, ReceiveInformation>();

                    // Pre-Test
                    foreach (var info in sources)
                    {
                        // It's not allowed to activate a source twice
                        var key = new SourceIdenfierWithKey( info.UniqueIdentifier, info.Selection.Source );
                        if (FindSource( key ) != null)
                            CardServerException.Throw( new SourceInUseFault( info.Selection.Source ) );

                        // Remember
                        infos.Add( key, info );

                        // Prepare to optimize
                        optimizer.Add( info.Selection, info.Streams );
                    }

                    // See how many we are allowed to start
                    var allowed = optimizer.Optimize();

                    // Streams to activate
                    var newStreams = new List<ActiveStream>();
                    try
                    {
                        // Process all
                        for (int i = 0; i < allowed; ++i)
                        {
                            // Attach to the source
                            var current = sources[i];
                            var source = current.Selection;
                            var key = new SourceIdenfierWithKey( current.UniqueIdentifier, source.Source );

                            // Create the stream manager
                            var manager = source.Open( optimizer.GetStreams( i ) );

                            // Attach file size mapper
                            manager.FileBufferSizeChooser = infos[key].GetFileBufferSize;

                            // Create
                            var stream = new ActiveStream( key.UniqueIdentifier, manager, current.Streams, current.RecordingPath );

                            // Remember
                            newStreams.Add( stream );

                            // See if we have to connect an optimizer for restarts
                            if (device.HasConsumerRestriction)
                                stream.EnableOptimizer( source );

                            // Try to start
                            stream.Refresh( m_RetestWatchDogInterval );
                        }

                        // Loaded all
                        newStreams.ForEach( stream => Streams.Add( stream.SourceKey, stream ) );

                        // Generate response
                        try
                        {
                            // Create all
                            return newStreams.Select( stream => stream.CreateInformation() ).ToArray();
                        }
                        finally
                        {
                            // No need to clean up
                            newStreams.Clear();
                        }
                    }
                    finally
                    {
                        // Cleanup
                        newStreams.ForEach( stream => stream.Dispose() );
                    }
                } );
        }

        /// <summary>
        /// Beendet den Empfang auf allen Quellen.
        /// </summary>
        private void RemoveAll()
        {
            // With cleanup
            try
            {
                // For all
                foreach (var stream in Streams.Values)
                    try
                    {
                        // Just forward
                        stream.Dispose();
                    }
                    catch
                    {
                        // Ignore any error
                    }
            }
            finally
            {
                // Reset list
                Streams.Clear();
            }

            // Shutdown parser, too
            DisableServiceParser();
        }

        /// <summary>
        /// Beendet die Auswertung der programmzeitschrift für die NVOD Dienste.
        /// </summary>
        private void DisableServiceParser()
        {
            // Stop service parser
            if (m_ServiceParser != null)
                try
                {
                    // Shut down
                    m_ServiceParser.Disable();
                }
                catch
                {
                    // Ignore any error
                }
                finally
                {
                    // Forget
                    m_ServiceParser = null;
                }
        }

        /// <summary>
        /// Beendet die Nutzung dieser Instanz.
        /// </summary>
        protected override void OnDispose()
        {
            // Flag termination
            m_Running = false;

            // Wake up call
            m_IdleDone.Set();
            m_Trigger.Set();

            // Stop the threads
            var idleThread = Interlocked.Exchange( ref m_IdleThread, null );
            if (idleThread != null)
                idleThread.Join();

            var thread = Interlocked.Exchange( ref m_Thread, null );
            if (thread != null)
                thread.Join();

            // Forward
            base.OnDispose();
        }
    }
}
