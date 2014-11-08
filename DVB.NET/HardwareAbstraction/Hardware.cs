using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JMS.DVB.DeviceAccess;
using JMS.DVB.SI;


namespace JMS.DVB
{
    /// <summary>
    /// Basisklasse zur Implementierung von DVB.NET Hardwareabstraktionen.
    /// </summary>
    public abstract partial class Hardware : IDisposable
    {
        /// <summary>
        /// Der Parametername für den Anzeigenamen des Empfängerfilters. Dieser wird im 
        /// Allgemeinen ignoriert, wenn <see cref="Aspect_TunerMoniker"/> verfügbar ist.
        /// </summary>
        public const string Aspect_TunerName = "TunerFriendlyName";

        /// <summary>
        /// Der Parametername für den eindeutigen Namen des Empfängerfilters. Diese Angabe
        /// ist optional, wird aber empfohlen.
        /// </summary>
        public const string Aspect_TunerMoniker = "TunerMoniker";

        /// <summary>
        /// Der Parametername für den Anzeigenamen des Filter, über den die Rohdaten in den Graphen
        /// geleitet werden. Diese Angabe kann je nach Gerät optional sein. Sie wird im Allgemeinen
        /// ignoriert, wenn <see cref="Aspect_CaptureMoniker"/> gesetzt ist.
        /// </summary>
        public const string Aspect_CaptureName = "CaptureFriendlyName";

        /// <summary>
        /// Der Parametername für den eindeutigen Namen des Filter, über den die Rohdaten in den Graphen
        /// geleitet werden. Diese Angabe kann je nach Gerät optional sein.
        /// </summary>
        public const string Aspect_CaptureMoniker = "CaptureMoniker";

        /// <summary>
        /// Der Parameter für den eindeutigen Namens des Gerätes, dass nach dem Aufwachen aus dem Schlafzustand
        /// reaktiviert werden soll.
        /// </summary>
        public const string Parameter_WakeupMoniker = "WakeupDeviceMoniker";

        /// <summary>
        /// Der Parametername für den Anzeigenamen des Gerätes, dass nach dem Aufwachen aus dem Schlafzustand
        /// reaktiviert werden soll.
        /// </summary>
        public const string Parameter_WakeupDevice = "WakeupDevice";

        /// <summary>
        /// Der Parametername für die Einstellung, ob ein Gerät nach dem Aufwachen aus dem Schlafzustand
        /// reaktiviert werden soll.
        /// </summary>
        public const string Parameter_EnableWakeup = "ResetAfterWakeup";

        /// <summary>
        /// Der Parametername für die Einstellung, ob während einer Aktualisierung der Liste der Quellen bei
        /// einem Fehlversuch die Entschlüsselung neu initialisiert werden sollen - einige Module benötigen
        /// dies zur korrekten Resynchronisation.
        /// </summary>
        public const string Parameter_EnableCIDuringScan = "EnableDecryptionForSourceScan";

        /// <summary>
        /// Mit diesem Schalter kann ausgewählt werden, in welchem Umfang Manipulationen der Verbraucher
        /// protokolliert werden sollen.
        /// </summary>
        public static readonly BooleanSwitch ConsumerTraceSwitch = new BooleanSwitch( "ConsumerTrace", "Reports raw Operations on PID Consumers" );

        /// <summary>
        /// Dieser Schalter erlaubt es, dass Umschalten von Quellgruppen (Transponder) zu protokollieren.
        /// </summary>
        public static readonly BooleanSwitch TunerTraceSwitch = new BooleanSwitch( "TunerTrace", "Reports Selection of Source Groups" );

        /// <summary>
        /// Enthält alle Daten zu einem registrierten Datenstrom.
        /// </summary>
        protected class StreamInformation
        {
            /// <summary>
            /// Die eindeutige Nummer (PID) des Datenstroms.
            /// </summary>
            public readonly ushort Identifier;

            /// <summary>
            /// Die Art der Daten zu diese Datenstrom.
            /// </summary>
            public readonly StreamTypes StreamType;

            /// <summary>
            /// Der aktuelle Empfänger für die Daten dieses Datenstroms.
            /// </summary>
            public readonly Action<byte[], int, int> Consumer;

            /// <summary>
            /// Beschreibt die Anmeldung eines einzelnen Verbrauchers.
            /// </summary>
            private class StreamRegistration
            {
                /// <summary>
                /// Die eindeutige Kennung des Verbrauchers.
                /// </summary>
                public readonly Guid UniqueIdentifier = Guid.NewGuid();

                /// <summary>
                /// Die Methode, an die alle Daten geleitet werden sollen.
                /// </summary>
                public readonly Action<byte[], int, int> Sink;

                /// <summary>
                /// Meldet oder legt fest, ob dieser Verbracuher Daten empfangen soll.
                /// </summary>
                public volatile bool IsActive;

                /// <summary>
                /// Erzeugt eine neue Anmeldung.
                /// </summary>
                /// <param name="consumer">Der neu anzumeldende Verbraucher.</param>
                /// <exception cref="ArgumentNullException">Es wurde keine Methode zum Datenempfang angegeben.</exception>
                public StreamRegistration( Action<byte[], int, int> consumer )
                {
                    // Validate
                    if (consumer == null)
                        throw new ArgumentException( "data sink missing", "consumer" );

                    // Remember
                    Sink = consumer;
                }
            }

            /// <summary>
            /// Alle bekannten Verbraucher.
            /// </summary>
            private volatile StreamRegistration[] m_Consumers;

            /// <summary>
            /// Erzeugt eine neue Dateninstanz.
            /// </summary>
            /// <param name="stream">Die eindeutige Nummer (PID) des Datenstroms.</param>
            /// <param name="type">Die Art der Daten zu diesem Datenstrom.</param>
            /// <param name="callback">Die Methode, die beim Eintreffen von Daten aktiviert werden soll.</param>
            /// <param name="primaryIdentifier">Die eindeutige Kennung der ersten Registrierung eines Verbrauchers.</param>
            public StreamInformation( ushort stream, StreamTypes type, Action<byte[], int, int> callback, out Guid primaryIdentifier )
            {
                // Register consumer
                m_Consumers = new[] { new StreamRegistration( callback ) };

                // Report generated identifier
                primaryIdentifier = m_Consumers[0].UniqueIdentifier;

                // Remember
                Consumer = Dispatcher;
                Identifier = stream;
                StreamType = type;
            }

            /// <summary>
            /// Meldet einen weiteren Verbraucher an.
            /// </summary>
            /// <param name="callback">Die Methode, die beim Eintreffen von Daten aktiviert werden soll.</param>
            /// <returns>Eine eindeutige Kennung für den Verbraucher.</returns>
            public Guid AddConsumer( Action<byte[], int, int> callback )
            {
                // Create new
                var reg = new StreamRegistration( callback );

                // Load
                var consumers = new List<StreamRegistration>( m_Consumers ) { reg };

                // Push back
                m_Consumers = consumers.ToArray();

                // Report
                return reg.UniqueIdentifier;
            }

            /// <summary>
            /// Entfernt einen Verbraucher.
            /// </summary>
            /// <param name="uniqueIdentifier">Die bei <see cref="AddConsumer"/> vergebene eindeutige Kennung.</param>
            /// <returns>Der Rückgabewert ist nicht gesetzt, wenn der letzt aktive Verbraucher entfernt wurde - ein 
            /// gesetzter Wert wird nie gemeldet. Eine <i>null</i> bedeutet, dass sich am Gesamtzustand dieses Datenstroms
            /// nichts verändert hat.</returns>
            public bool? RemoveConsumer( Guid uniqueIdentifier )
            {
                // Create helper
                var consumers = new List<StreamRegistration>( m_Consumers );

                // Find
                var i = consumers.FindIndex( c => c.UniqueIdentifier.Equals( uniqueIdentifier ) );
                if (i < 0)
                    return null;

                // Attach to the registration
                var consumer = consumers[i];

                // Correct
                consumers.RemoveAt( i );

                // Push back
                m_Consumers = consumers.ToArray();

                // No change possible
                if (!consumer.IsActive)
                    return null;

                // We are still active
                if (ActiveConsumerCount > 0)
                    return null;

                // Better stop us now
                return false;
            }

            /// <summary>
            /// Verändert den Empfang für einen einzelnen Verbraucher.
            /// </summary>
            /// <param name="uniqueIdentifier">Die eindeutige Kennung des Verbrauchers.</param>
            /// <param name="active">Gesetzt, wenn der Empfang aktiviert werden soll.</param>
            /// <returns>Meldet, ob sich der Gesamtzustand für diesen Datenstrom verändert hat. Ist
            /// die Rückgabe gesetzt, so sollte nun der Datenstrom aktiviert werden. Ist er nicht
            /// gesetzt, so kann eine Deaktivierung stattfinden. Bei <i>null</i> ist keine Veränderung
            /// notwendig.</returns>
            public bool? SetConsumerState( Guid uniqueIdentifier, bool active )
            {
                // Find it
                var consumer = this[uniqueIdentifier];

                // Not known
                if (consumer == null)
                    return null;

                // Is in the indicated state
                if (consumer.IsActive == active)
                    return null;

                // Change the state
                consumer.IsActive = active;

                // See if we are switching our operation mode
                switch (ActiveConsumerCount)
                {
                    case 0: if (!active) return false; break;
                    case 1: if (active) return true; break;
                }

                // Seems as if nothing changed
                return null;
            }

            /// <summary>
            /// Ermittelt einen Verbraucher.
            /// </summary>
            /// <param name="uniqueIdentifier">Die eindeutige Kennung des Verbrauchers.</param>
            /// <returns>Der gewünschte Verbraucher oder <i>null</i>, wenn dieser nicht bekannt ist.</returns>
            private StreamRegistration this[Guid uniqueIdentifier] { get { return m_Consumers.FirstOrDefault( consumer => consumer.UniqueIdentifier.Equals( uniqueIdentifier ) ); } }

            /// <summary>
            /// Ermittelt, ob ein bestimmter Verbraucher gerade Daten empfängt.
            /// </summary>
            /// <param name="uniqueIdentifier">Die eindeutige Kennung des Verbrauchers.</param>
            /// <returns>Gesetzt, wenn der Verbraucher Daten empfängt. Ist der angegebene Verbraucher nicht
            /// bekannt, so wird <i>null</i> gemeldet.</returns>
            public bool? GetConsumerState( Guid uniqueIdentifier )
            {
                // Find it
                var consumer = this[uniqueIdentifier];
                if (consumer == null)
                    return null;
                else
                    return consumer.IsActive;
            }

            /// <summary>
            /// Meldet die Anzahl der Verbraucher dieses Datenstroms, die gerade Daten empfangen.
            /// </summary>
            public int ActiveConsumerCount { get { return m_Consumers.Count( consumer => consumer.IsActive ); } }

            /// <summary>
            /// Verteilt den Datenblock an alle angeschlossenen Verbraucher.
            /// </summary>
            /// <param name="data">Der Speicherbereich mit den Nutzdaten.</param>
            /// <param name="offset">Das erste zu nutzende Byte.</param>
            /// <param name="length">Die Anzahl der zu nutzenden Bytes.</param>
            private void Dispatcher( byte[] data, int offset, int length )
            {
                // Load the array
                var consumers = m_Consumers;

                // Forward to all
                foreach (var consumer in consumers)
                    if (consumer.IsActive)
                        consumer.Sink( data, offset, length );
            }
        }

        /// <summary>
        /// Gesetzt, sobald einmal <see cref="Dispose"/> aufgerufen wurde.
        /// </summary>
        private bool m_disposed = false;

        /// <summary>
        /// Gesetzt, während <see cref="OnDispose"/> ausgeführt wird.
        /// </summary>
        private bool m_disposing = false;

        /// <summary>
        /// Liest oder setzt das zugeordnete Geräteprofil.
        /// </summary>
        public Profile Profile { get; private set; }

        /// <summary>
        /// Der aktuell verwendete Ursprung.
        /// </summary>
        public GroupLocation CurrentLocation { get; private set; }

        /// <summary>
        /// Die aktuell verwendete Quellgruppe.
        /// </summary>
        public SourceGroup CurrentGroup { get; private set; }

        /// <summary>
        /// Alle Datenströme geordnet nach der Datenstromkennung (PID).
        /// </summary>
        private Dictionary<ushort, StreamInformation> m_streamsByPID = new Dictionary<ushort, StreamInformation>();

        /// <summary>
        /// Alle Datenströme, geordnet nach der automatisch vergebenen eindeutigen Kennung.
        /// </summary>
        private Dictionary<Guid, StreamInformation> m_streamsById = new Dictionary<Guid, StreamInformation>();

        /// <summary>
        /// Synchronisiert den Zugriff auf die internen Datenstrukturen.
        /// </summary>
        protected readonly object InstanceSynchronizer = new object();

        /// <summary>
        /// Ermittelt die aktuelle <i>Network Information Table</i>.
        /// </summary>
        private CancellableTask<NIT[]> m_networkInformationReader;

        /// <summary>
        /// Die Hintergrundaufgabe zum Auslesen der Netzwerkinformationen.
        /// </summary>
        public Task<NIT[]> LocationInformationReader { get { return m_networkInformationReader; } }

        /// <summary>
        /// Ermittelt die aktuelle <i>Program Association Table</i>.
        /// </summary>
        private CancellableTask<PAT[]> m_associationReader;

        /// <summary>
        /// Die Hintergrundaufgabe zum Auslesen der Dienstbelegung.
        /// </summary>
        public Task<PAT[]> AssociationTableReader { get { return m_associationReader; } }

        /// <summary>
        /// Ermittelt die aktuelle <i>Service Description Table</i>.
        /// </summary>
        private CancellableTask<SDT[]> m_serviceReader;

        /// <summary>
        /// Die Hintergrundaufgabe zum Auslesen der Dienstabelle.
        /// </summary>
        public Task<SDT[]> ServiceTableReader { get { return m_serviceReader; } }

        /// <summary>
        /// Die zuletzt ermittelten Daten zur aktuellen Quellgruppe (Transponder).
        /// </summary>
        private Task<GroupInformation> m_groupReader;

        /// <summary>
        /// Die Hintergrundaufgabe zum Auslesen der Informationen zur Quellgruppe.
        /// </summary>
        public Task<GroupInformation> GroupReader { get { return m_groupReader; } }

        /// <summary>
        /// Der Zeitpunkt der letzten Auswahl einer Quellgruppe (Transponder).
        /// </summary>
        private DateTime m_lastTuneTime = DateTime.MinValue;

        /// <summary>
        /// Alle Empfänger von Daten der Programmzeitschrift.
        /// </summary>
        private volatile Action<EIT> m_programGuideConsumers;

        /// <summary>
        /// Die eindeutige Kennung für die Anmeldung der Verbraucher für die Programmzeitschrift.
        /// </summary>
        private Guid m_programGuideIdentifier;

        /// <summary>
        /// Die zugehörigen Einschränkungen des Gerätes.
        /// </summary>
        public HardwareRestriction Restrictions { get; private set; }

        /// <summary>
        /// Initialisiert die Basisklasse.
        /// </summary>
        /// <param name="profile">Das zugeordnete Geräteprofil.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Geräteprofil angegeben.</exception>
        internal Hardware( Profile profile )
        {
            // Validate
            if (profile == null)
                throw new ArgumentException( "profile missing", "profile" );

            // Remember
            Profile = profile;

            // Copy settings
            EffectivePipeline = new List<PipelineItem>( profile.Pipeline.Select( p => new PipelineItem { SupportedOperations = p.SupportedOperations, ComponentType = p.ComponentType } ) );
            EffectiveDeviceAspects = new List<DeviceAspect>( Profile.DeviceAspects.Select( a => new DeviceAspect { Aspekt = a.Aspekt, Value = a.Value } ) );
            EffectiveProfileParameters = new List<ProfileParameter>( Profile.Parameters.Select( p => new ProfileParameter( p.Name, p.Value ) ) );

            // Load the restrictions
            Restrictions = Profile.Restrictions ?? new HardwareRestriction();
        }

        /// <summary>
        /// Stellt den Empfang auf eine bestimmte Quellgruppe eines Ursprungs ein.
        /// </summary>
        /// <param name="location">Der gewünschte Ursprung.</param>
        /// <param name="group">Die gewünschte Quellgruppe.</param>
        protected abstract void OnSelectGroup( GroupLocation location, SourceGroup group );

        /// <summary>
        /// Meldet einen weiteren Empfänger für die Daten der Programmzeitschrift an.
        /// </summary>
        /// <param name="callback">Der Empfänger der Daten.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Empfänger angegeben.</exception>
        public void AddProgramGuideConsumer( Action<EIT> callback )
        {
            // Validate
            if (callback == null)
                throw new ArgumentException( "no program guide sink", "callback" );
            else
                m_programGuideConsumers += callback;

            // Get the state of the overall receiption
            var epgState = GetConsumerState( m_programGuideIdentifier );

            // Must register
            if (!epgState.HasValue)
                m_programGuideIdentifier = this.AddConsumer<EIT>( ProgramGuide );

            // Must start
            if (!epgState.GetValueOrDefault( false ))
                SetConsumerState( m_programGuideIdentifier, true );
        }

        /// <summary>
        /// Meldet einen Empfänger für die Daten der Programmzeitschrift ab.
        /// </summary>
        /// <param name="callback">Der Empfänger der Daten.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Empfänger angegeben.</exception>
        public void RemoveProgramGuideConsumer( Action<EIT> callback )
        {
            // Validate
            if (callback == null)
                throw new ArgumentNullException( "program guide sink missing", "callback" );
            else
                m_programGuideConsumers -= callback;
        }

        /// <summary>
        /// Aktiviert die Entschlüsselung einer Liste von Quellen.
        /// </summary>
        /// <param name="sources">Die gewünschten Quellen.</param>
        public virtual void Decrypt( params SourceIdentifier[] sources )
        {
            // Not here
            throw new NotSupportedException();
        }

        /// <summary>
        /// Bereitet einen Sendersuchlauf vor.
        /// </summary>
        public virtual void PrepareSourceScan()
        {
            // Silently discard
        }

        /// <summary>
        /// Nimmt eine Tabelle der Programmzeitschrift entgegen und verteilt diese
        /// an die angemeldeten Empfänger.
        /// </summary>
        /// <param name="table">Die aktuelle Tabelle.</param>
        private void ProgramGuide( EIT table )
        {
            // Process
            var consumers = m_programGuideConsumers;
            if (consumers != null)
                try
                {
                    consumers( table );
                }
                catch (Exception)
                {
                    // Ignore any error
                }
        }

        /// <summary>
        /// Prüft, ob die angegebene Quellgruppe (Transponder) angesteuert werden kann.
        /// </summary>
        /// <param name="group">Eine Quellgruppe zur Prüfung.</param>
        /// <returns>Gesetzt, wenn die Quellgruppe angesteuert werden kann.</returns>
        protected abstract bool OnCanHandle( SourceGroup group );

        /// <summary>
        /// Prüft, ob die angegebene Quellgruppe (Transponder) angesteuert werden kann.
        /// </summary>
        /// <param name="group">Eine Quellgruppe zur Prüfung.</param>
        /// <returns>Gesetzt, wenn die Quellgruppe angesteuert werden kann.</returns>
        public bool CanHandle( SourceGroup group )
        {
            // Check mode
            return (group != null) && OnCanHandle( group );
        }

        /// <summary>
        /// Stellt den Empfang auf eine bestimmte Quellgruppe eines Ursprungs ein.
        /// </summary>
        /// <remarks>Der Aufruf dieser Methode darf niemals auf mehreren Threads
        /// gleichzeitig erfolgen.</remarks>
        /// <param name="location">Der gewünschte Ursprung.</param>
        /// <param name="group">Die gewünschte Quellgruppe.</param>
        public void SelectGroup( GroupLocation location, SourceGroup group )
        {
            // Report
            if (TunerTraceSwitch.Enabled)
                Trace.WriteLine( string.Format( Properties.Resources.Trace_Tuner_Request, location, group ), TunerTraceSwitch.DisplayName );

            // Always use clones
            location = location.CloneLocation();
            group = group.CloneGroup();

            // Stop all current
            if ((CurrentLocation != null) || (CurrentGroup != null))
            {
                // Report
                if (ConsumerTraceSwitch.Enabled)
                    Trace.WriteLine( Properties.Resources.Trace_Consumer_StoppingAll, ConsumerTraceSwitch.DisplayName );

                // See if provide has a fast way to stop all streams
                bool allStopped = OnStopAll();

                // Stop all table readers
                ResetReader( ref m_networkInformationReader );
                ResetReader( ref m_associationReader );
                ResetReader( ref m_serviceReader );
                m_groupReader = null;

                // Erase list of EPG receivers
                m_programGuideConsumers = null;

                // List of active consumers
                StreamInformation[] consumers;

                // Be safe
                lock (InstanceSynchronizer)
                {
                    // Remember
                    consumers = m_streamsByPID.Values.ToArray();

                    // Wipe out
                    m_streamsByPID.Clear();
                    m_streamsById.Clear();
                }

                // Forward to all streams
                if (!allStopped)
                    foreach (var consumer in consumers)
                        if (consumer.ActiveConsumerCount > 0)
                            OnStop( consumer );
            }

            // See if something changed
            if (!Equals( location, CurrentLocation ) || !Equals( group, CurrentGroup ))
            {
                // Report
                if (TunerTraceSwitch.Enabled)
                    Trace.WriteLine( Properties.Resources.Trace_Tuner_Hardware, TunerTraceSwitch.DisplayName );

                // Activate hardware
                OnSelectGroup( location, group );

                // Remember
                CurrentLocation = location;
                CurrentGroup = group;

                // Mark time
                m_lastTuneTime = DateTime.UtcNow;
            }

            // Restart all readers
            ResetInformationReaders( true );
        }

        /// <summary>
        /// Stellt sicher, dass im Folgenden die Informationen über die Quellen dieser
        /// Quellegruppe (Transponder) neu ermittelt werden.
        /// </summary>
        public void ResetInformationReaders()
        {
            // Forward
            ResetInformationReaders( false );
        }

        /// <summary>
        /// Stellt sicher, dass im Folgenden die Informationen über die Quellen dieser
        /// Quellegruppe (Transponder) neu ermittelt werden.
        /// </summary>
        /// <param name="includeNetworkInformation">Gesetzt, wenn auch die Netzwerkinformationen
        /// neu angefordert werden sollen.</param>
        private void ResetInformationReaders( bool includeNetworkInformation )
        {
            // See if there is a group active
            if (CurrentGroup == null)
                return;

            // Restart network information reader
            if (includeNetworkInformation)
                ResetReader( ref m_networkInformationReader, this.GetTableAsync<NIT> );

            // Restart core reader
            var patReader = ResetReader( ref m_associationReader, this.GetTableAsync<PAT> );
            var sdtReader = ResetReader( ref m_serviceReader, this.GetTableAsync<SDT> );

            // Restart group reader
            m_groupReader = Task.Run( () => sdtReader.Result.ToGroupInformation( patReader.Result ) );
        }

        /// <summary>
        /// Stellt den Empfang auf die Quellgruppe der bezeichneten Quelle ein.
        /// </summary>
        /// <param name="source">Die Informationen zur gewünschten Quelle.</param>
        public void SelectGroup( SourceSelection source )
        {
            // Forward
            SelectGroup( (source == null) ? null : source.Location, (source == null) ? null : source.Group );
        }

        /// <summary>
        /// Ermittelt die Informationen zum aktuellen Ursprung.
        /// </summary>
        /// <param name="timeout">Die maximale Wartezeit seit Auswahl der Quellgruppe
        /// (Transponder) in Millisekunden.</param>
        /// <returns>Die gewünschten Informationen oder <i>null.</i></returns>
        public LocationInformation GetLocationInformation( int timeout = 5000 )
        {
            // Just forward
            var reader = LocationInformationReader;
            if (reader == null)
                return null;
            else if (reader.Wait( timeout ))
                return reader.Result.ToLocationInformation( this );
            else
                return null;
        }

        /// <summary>
        /// Ermittelt die Informationen zur aktuellen Quellgruppe (Transponder).
        /// </summary>
        /// <param name="timeout">Die maximale Wartezeit seit Auswahl der Quellgruppe
        /// (Transponder) in Millisekunden.</param>
        /// <param name="cancel">Optional eine Abbruchsteuerung.</param>
        /// <returns>Die gewünschten Informationen oder <i>null.</i></returns>
        public GroupInformation GetGroupInformation( int timeout = 5000, CancellationToken? cancel = null )
        {
            // Load reader
            var groupReader = GroupReader;
            if (groupReader == null)
                return null;
            else if (!groupReader.CancellableWait( cancel ?? CancellationToken.None, timeout ))
                return null;
            else
                return groupReader.Result;
        }

        /// <summary>
        /// Ermittelt die Datenstromkennung der SI Tabelle PMT zu einer Quelle.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <param name="cancel">Optional eine Abbruchsteuerung.</param>
        /// <returns>Die Datenstromkennung oder <i>null</i>, wenn die Quelle auf
        /// der aktuellen Quellgruppe (Transponder) nicht angeboten wird.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
        public ushort? GetServicePMT( SourceIdentifier source, CancellationToken? cancel = null )
        {
            // Validate
            if (source == null)
                throw new ArgumentNullException( "source" );

            // Get the current group information
            var groupInfo = GetGroupInformation( cancel: cancel );
            if (groupInfo == null)
                return null;

            // See if group exists
            if (!groupInfo.Sources.Any( source.Equals ))
                return null;

            // Direct read of result is possible - we already have the group information available
            var patReader = AssociationTableReader;
            if (patReader == null)
                return null;
            else if (!patReader.CancellableWait( cancel ?? CancellationToken.None ))
                return null;
            else
                return patReader.Result.FindService( source.Service );
        }

        /// <summary>
        /// Meldet einen Verbraucher für den Inhalt eines bestimmten Datenstroms an.
        /// <seealso cref="SetConsumerState"/>
        /// </summary>
        /// <param name="stream">Die eindeutige Nummer (PID) des Datenstroms in der aktiven
        /// <see cref="SourceGroup"/>.</param>
        /// <param name="type">Die Art der Daten im Datenstrom.</param>
        /// <param name="consumer">Der Empfänger für die Nutzdaten. Die Angabe von <i>null</i>
        /// wird zur Abmeldung verwendet.</param>
        /// <returns>Die eindeutige Kennung des neuen Verbrauchers. Diese Kennung wird benutzt, um
        /// den Datenempfang zu aktivieren, deaktivieren oder den Verbraucher wieder abzumelden.</returns>
        public Guid AddConsumer( ushort stream, StreamTypes type, Action<byte[], int, int> consumer )
        {
            // Report
            if (ConsumerTraceSwitch.Enabled)
                Trace.WriteLine( string.Format( Properties.Resources.Trace_Consumer_Register, stream, type ), ConsumerTraceSwitch.DisplayName );

            // Unique identifier of the new registration
            Guid consumerId;

            // Must synchronize
            lock (InstanceSynchronizer)
            {
                // Load the stream information
                StreamInformation info;
                if (m_streamsByPID.TryGetValue( stream, out info ))
                {
                    // Report
                    if (ConsumerTraceSwitch.Enabled)
                        Trace.WriteLine( Properties.Resources.Trace_Consumer_RegisterReuse, ConsumerTraceSwitch.DisplayName );

                    // Just add to existing registration
                    consumerId = info.AddConsumer( consumer );
                }
                else
                {
                    // Create a brand new one
                    info = new StreamInformation( stream, type, consumer, out consumerId );

                    // Remember it
                    m_streamsByPID[stream] = info;
                }

                // Add to lookup map
                m_streamsById[consumerId] = info;
            }

            // Report
            if (ConsumerTraceSwitch.Enabled)
                Trace.WriteLine( string.Format( Properties.Resources.Trace_Consumer_Identifier, consumerId ), ConsumerTraceSwitch.DisplayName );

            // Done
            return consumerId;
        }

        /// <summary>
        /// Aktiviert den Datenempfang für einen bestimmten Datenstrom.
        /// </summary>
        /// <param name="stream">Die Informationen zum betroffenen Datenstrom.</param>
        protected abstract void OnStart( StreamInformation stream );

        /// <summary>
        /// Deaktiviert den Datenempfang für einen bestimmten Datenstrom.
        /// </summary>
        /// <param name="stream">Die Informationen zum betroffenen Datenstrom.</param>
        protected abstract void OnStop( StreamInformation stream );

        /// <summary>
        /// Deaktiviert den Datenempfang unmittelbar bevor eine neue Quellgruppe aktiviert wird.
        /// </summary>
        /// <returns>Gesetzt, wenn die Implementierung die Deaktivierung vollständig ausgeführt hat.</returns>
        protected virtual bool OnStopAll()
        {
            // Not done
            return false;
        }

        /// <summary>
        /// Aktiviert oder deaktiviert den Empfang von Nutzdaten auf einem bestimmten
        /// Datenstrom.
        /// <seealso cref="AddConsumer"/>
        /// </summary>
        /// <param name="consumerId">Die eindeutige Kennung des betroffenen Verbrauchers.</param>
        /// <param name="running">Gesetzt, wenn der Empfang aktiviert werden soll. Ansonsten
        /// wird der Empfang der Nutzdaten sobald als möglich beendet und bei <i>null</i>
        /// die Registrierung vollständig entfernt.</param>
        public void SetConsumerState( Guid consumerId, bool? running )
        {
            // The internal information
            StreamInformation info;

            // Reports is state has been changed
            bool? newState;

            // Correction if activating the stream failes
            var correctionId = Guid.Empty;

            // Must be synchronized
            lock (InstanceSynchronizer)
            {
                // Get the corresponding PID information
                if (!m_streamsById.TryGetValue( consumerId, out info ))
                {
                    // Report
                    if (ConsumerTraceSwitch.Enabled)
                        Trace.WriteLine( string.Format( Properties.Resources.Trace_Consumer_NoConsumer, consumerId ), ConsumerTraceSwitch.DisplayName );

                    // Done
                    return;
                }

                // See what should be changed
                if (running.HasValue)
                {
                    // Report
                    if (ConsumerTraceSwitch.Enabled)
                        Trace.WriteLine( string.Format( Properties.Resources.Trace_Consumer_ChangeState, info.Identifier, running.Value, consumerId ), ConsumerTraceSwitch.DisplayName );

                    // Just change
                    newState = info.SetConsumerState( consumerId, running.Value );

                    // Remember
                    if (running.Value)
                        correctionId = consumerId;
                }
                else
                {
                    // Report
                    if (ConsumerTraceSwitch.Enabled)
                        Trace.WriteLine( string.Format( Properties.Resources.Trace_Consumer_RemoveConsumer, info.Identifier, consumerId ), ConsumerTraceSwitch.DisplayName );

                    // Try to process
                    newState = info.RemoveConsumer( consumerId );

                    // Forget it
                    m_streamsById.Remove( consumerId );
                }
            }

            // Process
            if (newState.HasValue)
            {
                // Report
                if (ConsumerTraceSwitch.Enabled)
                    Trace.WriteLine( string.Format( Properties.Resources.Trace_Consumer_FilterChange, info.Identifier, newState.Value ), ConsumerTraceSwitch.DisplayName );

                // Process
                if (newState.Value)
                    try
                    {
                        // Safe start
                        OnStart( info );
                    }
                    catch
                    {
                        // Correct not to count active filters wrong
                        if (correctionId != Guid.Empty)
                            lock (InstanceSynchronizer)
                                info.SetConsumerState( correctionId, false );

                        // Forward error
                        throw;
                    }
                else
                    OnStop( info );
            }
        }

        /// <summary>
        /// Meldet den aktuellen Empfang von Nutzdaten auf einem bestimmten Datenstrom.
        /// </summary>
        /// <param name="consumerId">Die eindeutige Kennung des betroffenen Verbrauchers.</param>
        /// <returns>Gesetzt, wenn der Datenstrom Nutzdaten empfängt ansonsten <i>null</i>,
        /// wenn der Datenstrom überhaupt nicht behannt ist.</returns>
        public bool? GetConsumerState( Guid consumerId )
        {
            // Overall synchronisation
            lock (InstanceSynchronizer)
            {
                // Get the corresponding PID information
                StreamInformation info;
                if (m_streamsById.TryGetValue( consumerId, out info ))
                    return info.GetConsumerState( consumerId );
            }

            // Not known at all
            return null;
        }

        /// <summary>
        /// Reinitialisiert das mit dieser Hardware verbundene Windows Gerät.
        /// </summary>
        public virtual void ResetWakeupDevice()
        {
        }

        /// <summary>
        /// Meldet, für welche Datenströme Verbraucher aktiv sind.
        /// </summary>
        /// <param name="types">Liste der zu prüfenden Arten.</param>
        /// <returns>Die Liste aller Datenströme mit mindestens einem aktivem Verbraucher.</returns>
        public ushort[] GetActiveStreams( params StreamTypes[] types )
        {
            // Create map
            var matcher = (types == null) ? null : new HashSet<StreamTypes>( types );

            // Process
            lock (InstanceSynchronizer)
                return
                    m_streamsByPID
                        .Values
                        .Where( info => (matcher == null) || matcher.Contains( info.StreamType ) )
                        .Where( info => info.ActiveConsumerCount > 0 )
                        .Select( info => info.Identifier )
                        .ToArray();
        }

        /// <summary>
        /// Meldet, für welche Datenströme Verbraucher aktiv sind.
        /// </summary>
        /// <returns>Die Liste aller Datenströme mit mindestens einem aktivem Verbraucher.</returns>
        public ushort[] GetActiveStreams()
        {
            // Forward
            return GetActiveStreams( StreamTypes.Video, StreamTypes.Audio, StreamTypes.VideoText, StreamTypes.SubTitle, StreamTypes.UnknownPES );
        }

        /// <summary>
        /// Meldet, ob die Anzahl gleichzeitiger Verbraucher beschränkt ist. Für die meiste Hardware 
        /// besteht eine solche Begrenzung nicht.
        /// </summary>
        public bool HasConsumerRestriction { get { return Restrictions.ConsumerLimit.HasValue && (Restrictions.ConsumerLimit.Value < 200); } }

        /// <summary>
        /// Ermittelt die aktuellen Daten zum empfangenen Signal.
        /// </summary>
        /// <param name="signal">Die vorbereitete Informationsstruktur.</param>
        protected virtual void OnGetSignal( SignalInformation signal )
        {
        }

        /// <summary>
        /// Meldet, ob der Pipelinemechanismus von DVB.NET 3.9 verwendet werden soll.
        /// </summary>
        protected virtual bool UsesLegacyPipeline { get { return true; } }

        /// <summary>
        /// Meldet die aktuellen technischen Daten zum empfangenen Signal oder <i>null</i>,
        /// wenn die Hardware das Auslesen der Signalstärke nicht unterstützt.
        /// </summary>
        public SignalInformation CurrentSignal
        {
            get
            {
                // None
                if (!Restrictions.ProvidesSignalInformation)
                    return null;

                // Create
                var info = new SignalInformation();

                // Fill
                OnGetSignal( info );

                // Report
                return info;
            }
        }

        /// <summary>
        /// Initialisiert eine Hintergrundaufgabe.
        /// </summary>
        /// <typeparam name="TResultType">Die Art des Ergebnisses der Aufgabe.</typeparam>
        /// <param name="reader">Die aktuelle Hintergrundaufgabe.</param>
        /// <param name="factory">Optional die Methode zur Neuinitialisierung.</param>
        /// <returns>Die neue Hintergrundaufgabe.</returns>
        private CancellableTask<TResultType> ResetReader<TResultType>( ref CancellableTask<TResultType> reader, Func<CancellableTask<TResultType>> factory = null ) where TResultType : class
        {
            // Wipe out
            var previous = Interlocked.Exchange( ref reader, null );

            // Stop it
            if (previous != null)
                previous.Cancel();

            // New one
            if (factory != null)
                reader = factory();

            // Report
            return reader;
        }

        /// <summary>
        /// Meldet, ob die Ressourcen dieser Instanz bereits freigegeben wurden.
        /// </summary>
        protected bool IsDisposed { get { return m_disposed; } }

        /// <summary>
        /// Meldet, ob gerade die <see cref="Dispose"/> Methode ausgeführt wird.
        /// </summary>
        protected bool IsDisposing { get { return m_disposing; } }

        /// <summary>
        /// Gibt alle mit dieser Instanz verbundenen Ressourcen frei.
        /// </summary>
        protected abstract void OnDispose();

        /// <summary>
        /// Gibt alle mit dieser Instanz verbundenen Ressourcen frei.
        /// </summary>
        public void Dispose()
        {
            // Once only
            if (m_disposed)
                return;
            if (m_disposing)
                return;

            // Do it
            m_disposing = true;
            try
            {
                // All readers
                ResetReader( ref m_networkInformationReader );
                ResetReader( ref m_associationReader );
                ResetReader( ref m_serviceReader );
                m_groupReader = null;

                // Forward
                OnDispose();
            }
            finally
            {
                // Did it
                m_disposing = false;
                m_disposed = true;
            }
        }
    }

    /// <summary>
    /// Implementierungshilfe für Geräte einer bestimmten Art.
    /// </summary>
    /// <typeparam name="TProfileType">Die genaue Art des Geräteprofils.</typeparam>
    /// <typeparam name="TLocationType">Die Art des Ursprungs.</typeparam>
    /// <typeparam name="TGroupType">Die Art der Quellgruppe.</typeparam>
    public abstract class Hardware<TProfileType, TLocationType, TGroupType> : Hardware
        where TProfileType : Profile
        where TLocationType : GroupLocation
        where TGroupType : SourceGroup
    {
        /// <summary>
        /// Initialisiert eine neue Geräteimplementierung.
        /// </summary>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        protected Hardware( TProfileType profile )
            : base( profile )
        {
        }

        /// <summary>
        /// Meldet das zugeordnete Geräteprofil.
        /// </summary>
        public new TProfileType Profile { get { return (TProfileType) base.Profile; } }

        /// <summary>
        /// Meldet den aktuell verwendeten Ursprung.
        /// </summary>
        public new TLocationType CurrentLocation { get { return (TLocationType) base.CurrentLocation; } }

        /// <summary>
        /// Meldet die aktuelle Quellgruppe.
        /// </summary>
        public new TGroupType CurrentGroup { get { return (TGroupType) base.CurrentGroup; } }

        /// <summary>
        /// Stellt den Empfang auf eine bestimmte Quellgruppe eines Ursprungs ein.
        /// </summary>
        /// <param name="location">Der gewünschte Ursprung.</param>
        /// <param name="group">Die gewünschte Quellgruppe.</param>
        public void SelectGroup( TLocationType location, TGroupType group )
        {
            // Forward
            base.SelectGroup( location, group );
        }

        /// <summary>
        /// Prüft, ob die angegebene Quellgruppe (Transponder) angesteuert werden kann.
        /// </summary>
        /// <param name="group">Eine Quellgruppe zur Prüfung.</param>
        /// <returns>Gesetzt, wenn die Quellgruppe angesteuert werden kann.</returns>
        protected override sealed bool OnCanHandle( SourceGroup group )
        {
            // Check type
            if (group.GetType() != typeof( TGroupType ))
                return false;
            else
                return OnCanHandle( (TGroupType) group );
        }

        /// <summary>
        /// Prüft, ob die angegebene Quellgruppe (Transponder) angesteuert werden kann.
        /// </summary>
        /// <param name="group">Eine Quellgruppe zur Prüfung.</param>
        /// <returns>Gesetzt, wenn die Quellgruppe angesteuert werden kann.</returns>
        protected virtual bool OnCanHandle( TGroupType group )
        {
            // Default is yes
            return true;
        }

        /// <summary>
        /// Prüft, ob die angegebene Quellgruppe (Transponder) angesteuert werden kann.
        /// </summary>
        /// <param name="group">Eine Quellgruppe zur Prüfung.</param>
        /// <returns>Gesetzt, wenn die Quellgruppe angesteuert werden kann.</returns>
        public bool CanHandle( TGroupType group )
        {
            // Forward
            return (group != null) && OnCanHandle( group );
        }

        /// <summary>
        /// Stellt den Empfang auf eine bestimmte Quellgruppe eines Ursprungs ein.
        /// </summary>
        /// <param name="location">Der gewünschte Ursprung.</param>
        /// <param name="group">Die gewünschte Quellgruppe.</param>
        protected abstract void OnSelect( TLocationType location, TGroupType group );

        /// <summary>
        /// Stellt den Empfang auf eine bestimmte Quellgruppe eines Ursprungs ein.
        /// </summary>
        /// <param name="location">Der gewünschte Ursprung.</param>
        /// <param name="group">Die gewünschte Quellgruppe.</param>
        protected override sealed void OnSelectGroup( GroupLocation location, SourceGroup group )
        {
            // Forward
            OnSelect( (TLocationType) location, (TGroupType) group );
        }
    }

    /// <summary>
    /// Beschreibt eine Implementierung für die Ansteuerung eines DVB-S(2) Gerätes.
    /// </summary>
    public abstract class SatelliteHardware : Hardware<SatelliteProfile, SatelliteLocation, SatelliteGroup>
    {
        /// <summary>
        /// Initialisiert diese Geräteimplementierungsinstanz.
        /// </summary>
        /// <param name="profile">Das zugeordnete Geräteprofil.</param>
        protected SatelliteHardware( SatelliteProfile profile )
            : base( profile )
        {
        }
    }

    /// <summary>
    /// Beschreibt eine Implementierung für die Ansteuerung eines DVB-C Gerätes.
    /// </summary>
    public abstract class CableHardware : Hardware<CableProfile, CableLocation, CableGroup>
    {
        /// <summary>
        /// Initialisiert diese Geräteimplementierungsinstanz.
        /// </summary>
        /// <param name="profile">Das zugeordnete Geräteprofil.</param>
        protected CableHardware( CableProfile profile )
            : base( profile )
        {
        }
    }

    /// <summary>
    /// Beschreibt eine Implementierung für die Ansteuerung eines DVB-T Gerätes.
    /// </summary>
    public abstract class TerrestrialHardware : Hardware<TerrestrialProfile, TerrestrialLocation, TerrestrialGroup>
    {
        /// <summary>
        /// Initialisiert diese Geräteimplementierungsinstanz.
        /// </summary>
        /// <param name="profile">Das zugeordnete Geräteprofil.</param>
        protected TerrestrialHardware( TerrestrialProfile profile )
            : base( profile )
        {
        }
    }
}
