using JMS.DVB;
using JMS.DVB.CardServer;
using JMS.DVBVCR.RecordingService.Persistence;
using JMS.DVBVCR.RecordingService.Status;
using JMS.DVBVCR.RecordingService.Win32Tools;
using System;
using System.Collections.Generic;
using System.Threading;


namespace JMS.DVBVCR.RecordingService.Requests
{
    /// <summary>
    /// Beschreibt Zugriffe auf eine DVB.NET Hardware.
    /// </summary>
    public abstract class CardServerProxy
    {
        /// <summary>
        /// Der zugehörige <i>DVB.NET Card Server</i>.
        /// </summary>
        protected ServerImplementation Server { get; private set; }

        /// <summary>
        /// Meldet den zugehörigen Zustand des Geräteprofils.
        /// </summary>
        public ProfileState ProfileState { get; private set; }

        /// <summary>
        /// Die Daten zur aktuellen Gesamtaufzeichnung - diese dient vor allem als Repräsentant für
        /// die aktuelle Aktion auf dem Gerät.
        /// </summary>
        protected VCRRecordingInfo Representative { get; private set; }

        /// <summary>
        /// Alle Daten für die Ausführung der Erweiterungen.
        /// </summary>
        protected Dictionary<string, string> ExtensionEnvironment { get; private set; }

        /// <summary>
        /// Verhindert, dass der Rechner während der Aufzeichnung in den Schlafzustand wechselt.
        /// </summary>
        private IDisposable m_HibernateBlock;

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz.
        /// </summary>
        /// <param name="state">Der Zustands des zugehörigen Geräteprofils.</param>
        /// <param name="primary">Die primäre Aufzeichnung, auf Grund derer der Aufzeichnungsprozeß aktiviert wurde.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Profil angegeben.</exception>
        protected CardServerProxy( ProfileState state, VCRRecordingInfo primary )
        {
            // Validate
            if (state == null)
                throw new ArgumentNullException( "state" );

            // Finish setup of fields and properties
            RequestFinished = new ManualResetEvent( false );

            // Remember
            Representative = primary.Clone();
            ProfileState = state;

            // Make sure that we report the start
            m_start = primary;
        }

        /// <summary>
        /// Meldet den Namen des aktuellen Geräteprofils.
        /// </summary>
        public string ProfileName => ProfileState.ProfileName;

        /// <summary>
        /// Meldet die anzahl der gerade aktiven Aufzeichnungen.
        /// </summary>
        public virtual int NumberOfActiveRecordings => 1;
        
        /// <summary>
        /// Wird aufgerufen, wenn eine Bindung an ein Geräteprofil vorgenommen wird.
        /// </summary>
        public void Activate()
        {
            // Block against hibernation
            using (m_HibernateBlock)
                m_HibernateBlock = PowerManager.StartForbidHibernation();
        }

        /// <summary>
        /// Wird aufgerufen, wenn eine Bindung an ein Geräteprofil aufgehoben wird.
        /// </summary>
        public void Deactivate()
        {
            // Release hibernation block
            using (m_HibernateBlock)
                m_HibernateBlock = null;
        }

        /// <summary>
        /// Erzeugt eine Beschreibung der mit diesem Zugriff verbundenen Aufzeichnungen.
        /// </summary>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public FullInfo CreateFullInformation() => CreateFullInformation( false );

        /// <summary>
        /// Erzeugt eine Beschreibung der mit diesem Zugriff verbundenen Aufzeichnungen.
        /// </summary>
        /// <param name="finalCall">Es handelt sich hier um den abschliessenden Aufruf.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        private FullInfo CreateFullInformation( bool finalCall )
        {
            // Create
            var info = new FullInfo { Recording = Representative.Clone(), CanStream = false };

            // Complete
            info.EndsAt = info.Recording.EndsAt;

            // Fill depending on the current task
            OnFillInformation( info, finalCall, m_state );

            // Report
            return info;
        }

        /// <summary>
        /// Erzeugt eine Beschreibung der mit diesem Zugriff verbundenen Aufzeichnungen.
        /// </summary>
        /// <param name="finalCall">Es handelt sich hier um den abschliessenden Aufruf.</param>
        /// <param name="info">Die bereits erzeugte Beschreibung.</param>
        /// <param name="state">Der aktuelle Zustand.</param>
        protected abstract void OnFillInformation( FullInfo info, bool finalCall, ServerInformation state );

        /// <summary>
        /// Verändert den Endzeitpunkt der Aufzeichnung.
        /// </summary>
        /// <param name="streamIdentifier">Die eindeutige Kennung des betroffenen Datenstroms.</param>
        /// <param name="newEndTime">Der neue Endzeitpunkt.</param>
        /// <param name="disableHibernation">Gesetzt, wenn der Übergang in den Schlafzustand deaktiviert werden soll.</param>
        public virtual void ChangeEndTime( Guid streamIdentifier, DateTime newEndTime, bool disableHibernation )
        {
            // Not us
            if (streamIdentifier != Representative.ScheduleUniqueID.Value)
                return;

            // Disable hibernation
            if (disableHibernation)
                Representative.DisableHibernation = true;

            // Send to planner
            if (ProfileState.Collection.ChangeEndTime( streamIdentifier, newEndTime ))
                Representative.EndsAt = newEndTime;
        }

        /// <summary>
        /// Markiert eine Aufzeichnung so, dass sie für weitere Planungen nicht mehr vor 
        /// einem bestimmten Zeitpunkt berücksichtigt wird.
        /// </summary>
        /// <param name="scheduleIdentifier">Die eindeutige Identifikation des betroffenen Datenstroms.</param>
        public virtual void SetRestartThreshold( Guid? scheduleIdentifier )
        {
        }

        #region Algorithmus der eigentlichen Aufzeichnung

        /// <summary>
        /// Alle aktiven Aufzeichnungen.
        /// </summary>
        private readonly Dictionary<Guid, VCRRecordingInfo> m_active = new Dictionary<Guid, VCRRecordingInfo>();

        /// <summary>
        /// Gesetzt, wenn die Aufzeichnungsgruppe vorzeitig beendet wurde.
        /// </summary>
        private volatile bool m_aborted;

        /// <summary>
        /// Ein separater <see cref="Thread"/>, der die Ausführung steuert.
        /// </summary>
        private Thread m_worker;

        /// <summary>
        /// Gesetzt, solange die Aufzeichnung läuft.
        /// </summary>
        private volatile bool m_running = true;

        /// <summary>
        /// Wird aktiviert, wenn eine Bearbeitung einer zusätzlichen Anforderung stattfinden soll.
        /// </summary>
        private readonly AutoResetEvent m_wakeUp = new AutoResetEvent( false );

        /// <summary>
        /// Wird gesetzt, sobald der Aufzeichnungsthread beendet ist.
        /// </summary>
        public ManualResetEvent RequestFinished { get; }

        /// <summary>
        /// Gesetzt, während dieser Zugriff beendet wird.
        /// </summary>
        public bool IsShuttingDown => !m_running;

        /// <summary>
        /// Erzeugt einen passenden Aufzeichnungsprozess.
        /// </summary>
        /// <returns>Der gewünschte Aufzeichnungsprozess.</returns>
        private ServerImplementation CreateCardServerProxy()
        {
            // Create the server
            if (VCRConfiguration.Current.UseExternalCardServer)
                return ServerImplementation.CreateOutOfProcess();
            else
                return ServerImplementation.CreateInMemory();
        }

        /// <summary>
        /// Startet einen separaten Thread, der die Ausführung steuert.
        /// </summary>
        public void Start()
        {
            // Make sure that we notify the planner in all situations
            try
            {
                // Report
                Tools.ExtendedLogging( "Starting Recording on {0}", ProfileName );

                // Once only
                if (m_worker != null)
                    throw new InvalidOperationException( "Request already started" );

                // Attach us to the state
                if (!ProfileState.BeginRequest( this, IsRealRecording ))
                {
                    // May be a zapping request which is overruled
                    ConfirmPendingRequest( true );
                }
                else
                {
                    // Create thread and start it - if this works we are no longer responsible for the plan request
                    m_worker = new Thread( Run ) { Name = $"Card Server Proxy Thread for {ProfileName}" };
                    m_worker.Start();
                }
            }
            catch (Exception)
            {
                // Something went very wrong
                ConfirmPendingRequest( true );

                // Forward
                throw;
            }
        }

        /// <summary>
        /// Fordert zum Beenden der Aufzeichnung auf und wartet auf den Abschluss.
        /// </summary>
        /// <param name="waitOnFinish">Gesetzt, wenn auf den Abschluss gewartet werden soll.</param>
        public void Stop( bool waitOnFinish = true )
        {
            // Report
            Tools.ExtendedLogging( "External Stop requested for {0}", ProfileName );

            // Set flag - keep order of assignment!
            m_aborted = true;
            m_running = false;

            // Wakeup call
            m_wakeUp.Set();

            // Wait for thread to finish
            if (waitOnFinish)
                m_worker.Join();
        }

        /// <summary>
        /// Ergänzt eine weitere Aufzeichnung.
        /// </summary>
        /// <param name="recording">Die Daten der neuen Aufzeichnung.</param>
        public virtual void Start( VCRRecordingInfo recording )
        {
            // Validate
            if (recording == null)
                throw new ArgumentNullException( "recording" );

            // Remember synchronized
            lock (m_active)
                if ((m_start == null) && !m_stop.HasValue)
                    m_start = recording;

            // Do the wakeup call
            m_wakeUp.Set();
        }

        /// <summary>
        /// Beendet eine einzelne Teilaufzeichnung.
        /// </summary>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der Teilaufzeichnung.</param>
        public void Stop( Guid scheduleIdentifier )
        {
            // Must synchronize
            lock (m_active)
                if ((m_start == null) && !m_stop.HasValue)
                    m_stop = scheduleIdentifier;

            // Do the wakeup call
            m_wakeUp.Set();
        }

        /// <summary>
        /// Prüft, ob es sich um eine reguläre Aufzeichnung handelt.
        /// </summary>
        protected virtual bool IsRealRecording => true;

        /// <summary>
        /// Die Art dieser Aufzeichnung.
        /// </summary>
        protected abstract string TypeName { get; }

        /// <summary>
        /// Startet die eigentlich Aufzeichnung in einer abgesicherten Umgebung.
        /// </summary>
        private void Run()
        {
            // Be fully safe
            try
            {
                // Create raw environment
                var coreEnvironment =
                    new Dictionary<string, string>
                    {
                        { "%wakeupprofile%", ProfileState.WakeUpRequired ? "1" : "0" },
                        { "%dvbnetprofile%", ProfileName },
                    };

                // Be fully safe
                try
                {
                    // Log it
                    VCRServer.Log( LoggingLevel.Schedules, Properties.Resources.RecordingStarted, TypeName );

                    // Report
                    Tools.ExtendedLogging( "Started Recording Control Thread for {0}", ProfileName );

                    // Fire all extensions
                    Tools.RunSynchronousExtensions( "BeforeProfileAccess", coreEnvironment );

                    // Use it
                    using (Server = CreateCardServerProxy())
                    {
                        // Check mode
                        var mustWakeUp = ProfileState.WakeUpRequired;
                        if (mustWakeUp)
                        {
                            // Log
                            VCRServer.Log( LoggingLevel.Full, Properties.Resources.RestartMessage );

                            // Report
                            Tools.ExtendedLogging( "Will restart Hardware for {0} if Restart Device is configured in Profile", ProfileName );
                        }

                        // Start synchronously
                        ServerImplementation.EndRequest(
                            Server.BeginSetProfile
                                (
                                    ProfileName,
                                    mustWakeUp,
                                    VCRConfiguration.Current.DisablePCRFromH264Generation,
                                    VCRConfiguration.Current.DisablePCRFromMPEG2Generation
                                ) );

                        // Report
                        Tools.ExtendedLogging( "Card Server is up and running" );

                        // Remember time
                        Representative.PhysicalStart = DateTime.UtcNow;

                        // Create fresh environment and fire extensions - with no files so far
                        FireRecordingStartedExtensions( ExtensionEnvironment = Representative.GetReplacementPatterns() );

                        // Time to allow derived class to start up
                        OnStart();

                        // Process idle loop - done every second if not interrupted earlier
                        for (IAsyncResult<ServerInformation> stateRequest = null; ; m_wakeUp.WaitOne( m_running ? 1000 : 100 ))
                        {
                            // First check for state request
                            if (stateRequest != null)
                            {
                                // No yet done
                                if (!stateRequest.IsCompleted)
                                    continue;

                                // Process the state
                                OnNewStateAvailable( m_state = stateRequest.Result );

                                // No longer waiting for the next state
                                stateRequest = null;
                            }

                            // See if we are busy in the derived class
                            if (HasPendingServerRequest)
                                continue;

                            // Process actions - no asynchronous operations allowed in current version
                            ProcessActions();

                            // Make sure that scheduler knows we accepted the request
                            ConfirmPendingRequest();

                            // If we are still idle fire a new state request
                            if (!HasPendingServerRequest)
                                if (m_running)
                                    stateRequest = Server.BeginGetState();
                                else
                                    break;
                        }

                        // Time to let derived class shut down properly
                        OnStop();

                        // Run final state update before protocol entry is created
                        if (m_state != null)
                            OnNewStateAvailable( m_state );

                        // Update recording data for the very last time
                        var info = CreateFullInformation( true );
                        if (info != null)
                            Representative = info.Recording;

                        // Set real send
                        Representative.EndsAt = DateTime.UtcNow;

                        // Set end state
                        ExtensionEnvironment["%Aborted%"] = m_aborted ? "1" : "0";

                        // Process extensions
                        FireRecordingFinishedExtensions( ExtensionEnvironment );

                        // No need for further wakeups
                        ProfileState.WakeUpRequired = false;
                    }
                }
                catch (Exception e)
                {
                    // Report
                    VCRServer.Log( e );

                    // Try to make sure that job is not restarted
                    try
                    {
                        // Update
                        SetRestartThreshold( null );
                    }
                    catch (Exception ex)
                    {
                        // Report
                        VCRServer.Log( ex );
                    }
                }
                finally
                {
                    // Detach from server - is already disposed and shut down
                    Server = null;

                    // Fire all extensions
                    Tools.RunSynchronousExtensions( "AfterProfileAccess", coreEnvironment );

                    // Detach from profile
                    ProfileState.EndRequest( this );

                    // Write recording
                    ProfileState.Server.JobManager.CreateLogEntry( Representative );

                    // May go to sleep after job is finished
                    ProfileState.Server.ReportRecordingDone( Representative.DisableHibernation, IsRealRecording );

                    // Check for next job on all profiles
                    ProfileState.Collection.BeginNewPlan();

                    // Report
                    Tools.ExtendedLogging( "Recording finished for {0}", ProfileName );

                    // Log it
                    VCRServer.Log( LoggingLevel.Schedules, Properties.Resources.RecordingFinished, TypeName );
                }
            }
            catch (Exception e)
            {
                // Report
                VCRServer.Log( e );
            }
            finally
            {
                // Make sure that even in case of error we do a full notification
                ConfirmPendingRequest( true );

                // Always fire event
                RequestFinished.Set();
            }
        }

        #endregion

        #region Zustandsverwaltung der Implementierungsklassen

        /// <summary>
        /// Der aktuelle Zustand des <i>Card Servers</i>.
        /// </summary>
        private volatile ServerInformation m_state;

        /// <summary>
        /// Die nächste Aufzeichnung die gestartet werden soll.
        /// </summary>
        private VCRRecordingInfo m_start;

        /// <summary>
        /// Die nächste Aufzeichnung, die beendet werden soll.
        /// </summary>
        private Guid? m_stop;

        /// <summary>
        /// Wird einmalig aufgerufen, sobald die Verbindung zum externen Aufzeichnungsprozeß hergestellt wurde.
        /// </summary>
        protected abstract void OnStart();

        /// <summary>
        /// Wird aufgerufen, sobald ein neuer Zustand vom Aufzeichnungsprozeß bereit steht.
        /// </summary>
        /// <param name="state">Der neue Zustand.</param>
        protected abstract void OnNewStateAvailable( ServerInformation state );

        /// <summary>
        /// Beginnt eine neue Aufzeichnung.
        /// </summary>
        /// <param name="recording">Die Daten der Aufzeichung.</param>
        protected virtual void OnStartRecording( VCRRecordingInfo recording )
        {
        }

        /// <summary>
        /// Beendete eine Aufzeichnung.
        /// </summary>
        /// <param name="scheduleIdentifier">Die zu beendene Aufzeichung.</param>
        protected virtual void OnEndRecording( Guid scheduleIdentifier )
        {
        }

        /// <summary>
        /// Wird einmalig aufgerufen, sobald die Verbindung zum externen Aufzeichnungsprozeß beendet wurde.
        /// </summary>
        protected abstract void OnStop();

        /// <summary>
        /// Prüft, ob weitere Anfragen an den Aufzeichnungsprozeß ausstehen. Es kann immer nur eine Anfrage
        /// aktiv sein.
        /// </summary>
        protected abstract bool HasPendingServerRequest { get; }

        /// <summary>
        /// Bestätigt die Bearbeitung des zuletzt erhaltenen Aufzeichnungsauftrags.
        /// </summary>
        /// <param name="notifyOnly">Gesetzt, wenn nur eine Benachrichtigung ausgelöst werden soll.</param>
        private void ConfirmPendingRequest( bool notifyOnly = false )
        {
            // Current settings
            VCRRecordingInfo start = null;
            Guid scheduleIdentifier;

            // Protect against change and check mode
            lock (m_active)
                if (m_start != null)
                {
                    // Load the current value
                    start = m_start;
                    scheduleIdentifier = start.ScheduleUniqueID.Value;

                    // Reset it
                    m_start = null;

                    // Add to map
                    m_active.Add( scheduleIdentifier, start );
                }
                else if (m_stop.HasValue)
                {
                    // Load the current value
                    scheduleIdentifier = m_stop.Value;

                    // Reset it
                    m_stop = null;

                    // Remove from to map
                    m_active.Remove( scheduleIdentifier );

                    // We did it
                    if (m_active.Count < 1)
                        m_running = false;
                }
                else
                {
                    // We are idle - nothing more to do
                    return;
                }

            // Check mode
            var isStart = (start != null);

            // Notify derived class if we are not in shutdown mode
            if (!notifyOnly)
                if (isStart)
                    OnStartRecording( start );
                else
                    OnEndRecording( scheduleIdentifier );

            // Inform scheduler outside the lock
            ProfileState.Collection.ConfirmOperation( scheduleIdentifier, isStart );
        }

        #endregion

        #region Aufrufe an den Aufzeichnungsprozeß

        /// <summary>
        /// Eine leere Liste von Aktionen.
        /// </summary>
        private static readonly Action[] s_NoActions = { };

        /// <summary>
        /// Eine Liste von Arbeitsaufträgen, die abgearbeitet werden sollen.
        /// </summary>
        private List<Action> m_Requests = new List<Action>();

        /// <summary>
        /// Führt alle ausstehenden Aktionen aus.
        /// </summary>
        protected void ProcessActions()
        {
            // Just execute all
            foreach (var action in DequeueActions())
                action();
        }

        /// <summary>
        /// Meldet alle ausstehenden Aktionen.
        /// </summary>
        /// <returns>Die Liste der ausstehenden Aktionen.</returns>
        private Action[] DequeueActions()
        {
            // Load
            lock (m_Requests)
                if (m_Requests.Count < 1)
                {
                    // This is the normal case so keep it simple
                    return s_NoActions;
                }
                else
                {
                    // Load 
                    var requests = m_Requests.ToArray();

                    // Clear
                    m_Requests.Clear();

                    // Report
                    return requests;
                }
        }

        /// <summary>
        /// Beendet einen asynchronen Aufruf. Im Fall einer Ausnahme wird diese nur protokolliert
        /// und der Aufruf als abgeschlossen angenommen.
        /// </summary>
        /// <typeparam name="TAsync">Die Art des Aufrufs.</typeparam>
        /// <param name="request">Der aktive Aufruf.</param>
        /// <param name="traceFormat">Meldung bei Abschluss des Aufrufs. Ist dieser Parameter <i>null</i>, so
        /// wartet die Methode auf den Abschluss des asynchronen Aufrufs.</param>
        /// <param name="traceArgs">Parameter zum Aufbau der Meldung.</param>
        /// <returns>Gesetzt, wenn der Aufruf abgeschlossen wurde oder bereits war. Ansonsten
        /// ist der Aufruf weiterhin aktiv.</returns>
        protected bool WaitForEnd<TAsync>( ref TAsync request, string traceFormat = null, params object[] traceArgs ) where TAsync : class, IAsyncResult
        {
            // No request at all
            var pending = request;
            if (pending == null)
                return true;

            // Request still busy - calling this method with only a single parameter will do a synchronous call
            if (traceFormat != null)
                if (!pending.IsCompleted)
                    return false;

            // Synchronize
            try
            {
                // Fetch result
                ServerImplementation.EndRequest( pending );

                // Report
                if (!string.IsNullOrEmpty( traceFormat ))
                    Tools.ExtendedLogging( traceFormat, traceArgs );
            }
            catch (Exception e)
            {
                // Report only!
                VCRServer.Log( LoggingLevel.Errors, "Failed Operation for '{0}': {1}", ProfileName, e.Message );
            }
            finally
            {
                // Forget
                request = null;
            }

            // Stopped it
            return true;
        }

        /// <summary>
        /// Führt eine Aktion auf dem Aufzeichnungsthread aus.
        /// </summary>
        /// <param name="action">Die gewünschte Aktion.</param>
        protected void EnqueueActionAndWait( Action action ) => EnqueueActionAndWait<object>( () => { action(); return null; } );

        /// <summary>
        /// Führt eine Aktion auf dem Aufzeichnungsthread aus.
        /// </summary>
        /// <typeparam name="TResult">Die Art des Ergebnisses der Aktion.</typeparam>
        /// <param name="action">Die gewünschte Aktion.</param>
        /// <returns>Das Ergebnis der Aktion.</returns>
        protected TResult EnqueueActionAndWait<TResult>( Func<TResult> action )
        {
            // Validate
            TResult result = default( TResult );
            if (action == null)
                return result;

            // Create a synchronisation instance
            using (var done = new ManualResetEvent( false ))
            {
                // Error to report
                var error = default( Exception );

                // Add the request
                lock (m_Requests)
                    m_Requests.Add( () =>
                    {
                        // Safe processing
                        try
                        {
                            // Report
                            Tools.ExtendedLogging( "Processing Action on {0}", ProfileName );

                            // Process
                            result = action();
                        }
                        catch (Exception e)
                        {
                            // Report
                            Tools.ExtendedLogging( "Action fails with {0}", e );

                            // Remember
                            error = e;
                        }
                        finally
                        {
                            // Report
                            Tools.ExtendedLogging( "Notifying Initiator of Action Completition" );

                            // Always trigger
                            done.Set();
                        }
                    } );

                // Trigger execution
                m_wakeUp.Set();

                // Create wait handle array
                WaitHandle[] wait = { done, RequestFinished };
                if (WaitHandle.WaitAny( wait ) == 0)
                    if (error != null)
                        throw error;
                    else
                        return result;
                else
                    return default( TResult );
            }
        }

        #endregion

        #region Aufruf von Erweiterungen

        /// <summary>
        /// Ergänzt die Umgebungsvariablen mit den angegebenen Aufzeichnungsdateien.
        /// </summary>
        /// <param name="environment">Die aktuellen Umgebungsvariablen.</param>
        /// <param name="prefix">Die Art der Dateien.</param>
        /// <param name="files">Die gewünschten Dateien.</param>
        protected void AddFilesToEnvironment( Dictionary<string, string> environment, string prefix, IEnumerable<FileInformation> files )
        {
            // Reset index
            int i = 0;

            // Process all
            foreach (var file in files)
            {
                // Report
                environment[$"%{prefix}File{++i}%"] = file.Path;

                // Create the name
                string formatName = $"%{prefix}Format{i}%";

                // Check mode
                switch (file.VideoType)
                {
                    case VideoTypes.H264: environment[formatName] = "27"; break;
                    case VideoTypes.MPEG2: environment[formatName] = "2"; break;
                    case VideoTypes.NoVideo: environment[formatName] = "255"; break;
                    default: environment[formatName] = string.Empty; break;
                }
            }

            // Set count
            environment[$"%{prefix}Files%"] = i.ToString();
        }

        /// <summary>
        /// Aktiviert eine VCR.NET Erweiterung.
        /// </summary>
        /// <param name="environment">Die Umgebungsvariablen für die Erweiterung.</param>
        protected void FireRecordingStartedExtensions( Dictionary<string, string> environment ) => ProfileState.Server.ExtensionProcessManager.AddWithCleanup( "RecordingStarted", environment );

        /// <summary>
        /// Aktiviert eine VCR.NET Erweiterung.
        /// </summary>
        /// <param name="environment">Die Umgebungsvariablen für die Erweiterung.</param>
        protected void FireRecordingFinishedExtensions( Dictionary<string, string> environment ) => ProfileState.Server.ExtensionProcessManager.AddWithCleanup( "RecordingFinished", environment );

        #endregion
    }
}
