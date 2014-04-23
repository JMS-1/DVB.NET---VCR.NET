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
            public ushort Identifier { get; private set; }

            /// <summary>
            /// Die Art der Daten zu diese Datenstrom.
            /// </summary>
            public StreamTypes StreamType { get; private set; }

            /// <summary>
            /// Der aktuelle Empfänger für die Daten dieses Datenstroms.
            /// </summary>
            public Action<byte[], int, int> Consumer { get; private set; }

            /// <summary>
            /// Beschreibt die Anmeldung eines einzelnen Verbrauchers.
            /// </summary>
            private class _Registration
            {
                /// <summary>
                /// Die eindeutige Kennung des Verbrauchers.
                /// </summary>
                public Guid UniqueIdentifier { get; private set; }

                /// <summary>
                /// Die Methode, an die alle Daten geleitet werden sollen.
                /// </summary>
                public Action<byte[], int, int> Sink { get; private set; }

                /// <summary>
                /// Meldet oder legt fest, ob dieser Verbracuher Daten empfangen soll.
                /// </summary>
                public volatile bool IsActive;

                /// <summary>
                /// Erzeugt eine neue Anmeldung.
                /// </summary>
                /// <param name="consumer">Der neu anzumeldende Verbraucher.</param>
                /// <exception cref="ArgumentNullException">Es wurde keine Methode zum Datenempfang angegeben.</exception>
                public _Registration( Action<byte[], int, int> consumer )
                {
                    // Validate
                    if (null == consumer)
                        throw new ArgumentNullException( "consumer" );

                    // Remember
                    UniqueIdentifier = Guid.NewGuid();
                    Sink = consumer;
                }
            }

            /// <summary>
            /// Alle bekannten Verbraucher.
            /// </summary>
            private volatile _Registration[] m_Consumers;

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
                m_Consumers = new _Registration[] { new _Registration( callback ) };

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
                _Registration reg = new _Registration( callback );

                // Load
                List<_Registration> consumers = new List<_Registration>( m_Consumers );

                // Append
                consumers.Add( reg );

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
                List<_Registration> consumers = new List<_Registration>( m_Consumers );

                // Find
                int i = consumers.FindIndex( c => c.UniqueIdentifier.Equals( uniqueIdentifier ) );

                // Not known
                if (i < 0)
                    return null;

                // Attach to the registration
                _Registration consumer = consumers[i];

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
                _Registration consumer = this[uniqueIdentifier];

                // Not known
                if (null == consumer)
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
            private _Registration this[Guid uniqueIdentifier]
            {
                get
                {
                    // Find it
                    int i = Array.FindIndex( m_Consumers, c => c.UniqueIdentifier.Equals( uniqueIdentifier ) );

                    // Not known
                    if (i < 0)
                        return null;
                    else
                        return m_Consumers[i];
                }
            }

            /// <summary>
            /// Ermittelt, ob ein bestimmter Verbraucher gerade Daten empfängt.
            /// </summary>
            /// <param name="uniqueIdentifier">Die eindeutige Kennung des Verbrauchers.</param>
            /// <returns>Gesetzt, wenn der Verbraucher Daten empfängt. Ist der angegebene Verbraucher nicht
            /// bekannt, so wird <i>null</i> gemeldet.</returns>
            public bool? GetConsumerState( Guid uniqueIdentifier )
            {
                // Find it
                _Registration consumer = this[uniqueIdentifier];

                // Ask it
                if (null == consumer)
                    return null;
                else
                    return consumer.IsActive;
            }

            /// <summary>
            /// Meldet die Anzahl der Verbraucher dieses Datenstroms, die gerade Daten empfangen.
            /// </summary>
            public int ActiveConsumerCount
            {
                get
                {
                    // Just sum up
                    return m_Consumers.Count( r => r.IsActive );
                }
            }

            /// <summary>
            /// Verteilt den Datenblock an alle angeschlossenen Verbraucher.
            /// </summary>
            /// <param name="data">Der Speicherbereich mit den Nutzdaten.</param>
            /// <param name="offset">Das erste zu nutzende Byte.</param>
            /// <param name="length">Die Anzahl der zu nutzenden Bytes.</param>
            private void Dispatcher( byte[] data, int offset, int length )
            {
                // Load the array
                _Registration[] consumers = m_Consumers;

                // Forward to all
                foreach (_Registration consumer in consumers)
                    if (consumer.IsActive)
                        consumer.Sink( data, offset, length );
            }
        }

        /// <summary>
        /// Gesetzt, sobald einmal <see cref="Dispose"/> aufgerufen wurde.
        /// </summary>
        private bool m_IsDisposed = false;

        /// <summary>
        /// Gesetzt, während <see cref="OnDispose"/> ausgeführt wird.
        /// </summary>
        private bool m_IsDisposing = false;

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
        private Dictionary<ushort, StreamInformation> m_StreamsByPID = new Dictionary<ushort, StreamInformation>();

        /// <summary>
        /// Alle Datenströme, geordnet nach der automatisch vergebenen eindeutigen Kennung.
        /// </summary>
        private Dictionary<Guid, StreamInformation> m_StreamsById = new Dictionary<Guid, StreamInformation>();

        /// <summary>
        /// Synchronisiert den Zugriff auf die internen Datenstrukturen.
        /// </summary>
        protected object InstanceSynchronizer { get; private set; }

        /// <summary>
        /// Ermittelt die aktuelle <i>Network Information Table</i>.
        /// </summary>
        private CancellableTask<NIT[]> m_NITReader;

        /// <summary>
        /// Die Hintergrundaufgabe zum Auslesen der Netzwerkinformationen.
        /// </summary>
        public Task<NIT[]> LocationInformationReader { get { return m_NITReader; } }

        /// <summary>
        /// Ermittelt die aktuelle <i>Program Association Table</i>.
        /// </summary>
        private CancellableTask<PAT[]> m_PATReader;

        /// <summary>
        /// Die Hintergrundaufgabe zum Auslesen der Dienstbelegung.
        /// </summary>
        public Task<PAT[]> AssociationTableReader { get { return m_PATReader; } }

        /// <summary>
        /// Ermittelt die aktuelle <i>Service Description Table</i>.
        /// </summary>
        private CancellableTask<SDT[]> m_SDTReader;

        /// <summary>
        /// Die Hintergrundaufgabe zum Auslesen der Dienstabelle.
        /// </summary>
        public Task<SDT[]> ServiceTableReader { get { return m_SDTReader; } }

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
        private DateTime m_TuneTime = DateTime.MinValue;

        /// <summary>
        /// Alle Empfänger von Daten der Programmzeitschrift.
        /// </summary>
        private volatile Action<EIT>[] m_ProgramGuideConsumers = { };

        /// <summary>
        /// Die eindeutige Kennung für die Anmeldung der Verbraucher für die Programmzeitschrift.
        /// </summary>
        private Guid m_ProgramGuideIdentifier;

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
            if (null == profile)
                throw new ArgumentNullException( "profile" );

            // Remember
            Profile = profile;

            // Copy settings
            EffectivePipeline = new List<PipelineItem>( profile.Pipeline.Select( p => new PipelineItem { SupportedOperations = p.SupportedOperations, ComponentType = p.ComponentType } ) );
            EffectiveDeviceAspects = new List<DeviceAspect>( Profile.DeviceAspects.Select( a => new DeviceAspect { Aspekt = a.Aspekt, Value = a.Value } ) );
            EffectiveProfileParameters = new List<ProfileParameter>( Profile.Parameters.Select( p => new ProfileParameter( p.Name, p.Value ) ) );

            // Create helper
            InstanceSynchronizer = new object();

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
                throw new ArgumentNullException( "callback" );

            // Create clone of current
            var consumers = new List<Action<EIT>>( m_ProgramGuideConsumers );

            // Append
            consumers.Add( callback );

            // Put back
            m_ProgramGuideConsumers = consumers.ToArray();

            // Get the state of the overall receiption
            var epgState = GetConsumerState( m_ProgramGuideIdentifier );

            // Must register
            if (!epgState.HasValue)
                m_ProgramGuideIdentifier = this.AddConsumer<EIT>( ProgramGuide );

            // Must start
            if (!epgState.GetValueOrDefault( false ))
                SetConsumerState( m_ProgramGuideIdentifier, true );
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
                throw new ArgumentNullException( "callback" );

            // Create clone of current
            var consumers = new List<Action<EIT>>( m_ProgramGuideConsumers );

            // Remove
            consumers.Remove( callback );

            // Put back
            m_ProgramGuideConsumers = consumers.ToArray();
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
            // Load current state of list
            var receivers = m_ProgramGuideConsumers;

            // Process
            foreach (Action<EIT> receiver in receivers)
                try
                {
                    // Send
                    receiver( table );
                }
                catch
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
                ResetReader( ref m_NITReader );
                ResetReader( ref m_SDTReader );
                ResetReader( ref m_PATReader );
                m_groupReader = null;

                // Erase list of EPG receivers
                m_ProgramGuideConsumers = new Action<EIT>[0];

                // List of active consumers
                List<StreamInformation> consumers;

                // Be safe
                lock (InstanceSynchronizer)
                {
                    // Remember
                    consumers = new List<StreamInformation>( m_StreamsByPID.Values );

                    // Wipe out
                    m_StreamsByPID.Clear();
                    m_StreamsById.Clear();
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
                m_TuneTime = DateTime.UtcNow;
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
                ResetReader( ref m_NITReader, this.GetTableAsync<NIT> );

            // Restart core reader
            ResetReader( ref m_PATReader, this.GetTableAsync<PAT> );
            ResetReader( ref m_SDTReader, this.GetTableAsync<SDT> );

            // Restart group reader
            m_groupReader =
                Task.Run( () =>
                {
                    // Requires PAT
                    var patReader = AssociationTableReader;
                    if (patReader == null)
                        return null;

                    // Requires SDT
                    var sdtReader = ServiceTableReader;
                    if (sdtReader == null)
                        return null;

                    // Wait and Construct
                    return sdtReader.Result.ToGroupInformation( patReader.Result );
                } );
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
        public LocationInformation GetLocationInformation( int timeout )
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
        /// <returns>Die gewünschten Informationen oder <i>null.</i></returns>
        public GroupInformation GetGroupInformation( int timeout )
        {
            // Load reader
            var groupReader = GroupReader;
            if (groupReader == null)
                return null;
            else if (!groupReader.Wait( timeout ))
                return null;
            else
                return groupReader.Result;
        }

        /// <summary>
        /// Ermittelt die Datenstromkennung der SI Tabelle PMT zu einer Quelle.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Die Datenstromkennung oder <i>null</i>, wenn die Quelle auf
        /// der aktuellen Quellgruppe (Transponder) nicht angeboten wird.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
        public ushort? GetServicePMT( SourceIdentifier source )
        {
            // Validate
            if (source == null)
                throw new ArgumentNullException( "source" );

            // Get the current group information
            var groupInfo = this.GetGroupInformation();
            if (groupInfo == null)
                return null;

            // See if group exists
            if (groupInfo.Sources.Find( s => source.Equals( s ) ) == null)
                return null;

            // Direct read of result is possible - we already have the group information available
            var patReader = AssociationTableReader;
            if (patReader == null)
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
                if (m_StreamsByPID.TryGetValue( stream, out info ))
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
                    m_StreamsByPID[stream] = info;
                }

                // Add to lookup map
                m_StreamsById[consumerId] = info;
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
            Guid correctionId = Guid.Empty;

            // Must be synchronized
            lock (InstanceSynchronizer)
            {
                // Get the corresponding PID information
                if (!m_StreamsById.TryGetValue( consumerId, out info ))
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
                    m_StreamsById.Remove( consumerId );
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
                if (m_StreamsById.TryGetValue( consumerId, out info ))
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
            Dictionary<StreamTypes, bool> match = null;

            // Fill
            if (types != null)
            {
                // Create
                match = new Dictionary<StreamTypes, bool>();

                // All in
                foreach (var type in types)
                    match[type] = true;
            }

            // Result
            List<ushort> all = new List<ushort>();

            // Process
            lock (InstanceSynchronizer)
                foreach (var info in m_StreamsByPID.Values)
                    if ((match == null) || match.ContainsKey( info.StreamType ))
                        if (info.ActiveConsumerCount > 0)
                            all.Add( info.Identifier );

            // Report
            return all.ToArray();
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
        public bool HasConsumerRestriction
        {
            get
            {
                // Report
                return Restrictions.ConsumerLimit.HasValue && (Restrictions.ConsumerLimit.Value < 200);
            }
        }

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
        protected virtual bool UsesLegacyPipeline
        {
            get
            {
                // This is the default
                return true;
            }
        }

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

        #region IDisposable Members

        /// <summary>
        /// Meldet, ob die Ressourcen dieser Instanz bereits freigegeben wurden.
        /// </summary>
        protected bool IsDisposed
        {
            get
            {
                // Report
                return m_IsDisposed;
            }
        }

        /// <summary>
        /// Meldet, ob gerade die <see cref="Dispose"/> Methode ausgeführt wird.
        /// </summary>
        protected bool IsDisposing
        {
            get
            {
                // Report
                return m_IsDisposing;
            }
        }

        /// <summary>
        /// Gibt alle mit dieser Instanz verbundenen Ressourcen frei.
        /// </summary>
        protected abstract void OnDispose();

        /// <summary>
        /// Initialisiert eine Hintergrundaufgabe.
        /// </summary>
        /// <typeparam name="TResultType">Die Art des Ergebnisses der Aufgabe.</typeparam>
        /// <param name="reader">Die aktuelle Hintergrundaufgabe.</param>
        /// <param name="factory">Optional die Methode zur Neuinitialisierung.</param>
        private void ResetReader<TResultType>( ref CancellableTask<TResultType> reader, Func<CancellableTask<TResultType>> factory = null ) where TResultType : class
        {
            // Wipe out
            var previous = Interlocked.Exchange( ref reader, null );

            // Stop it
            if (previous != null)
                previous.Cancel();

            // New one
            if (factory != null)
                reader = factory();
        }

        /// <summary>
        /// Gibt alle mit dieser Instanz verbundenen Ressourcen frei.
        /// </summary>
        public void Dispose()
        {
            // Once only
            if (!m_IsDisposed)
                try
                {
                    // Get flag
                    m_IsDisposing = true;

                    // All readers
                    ResetReader( ref m_NITReader );
                    ResetReader( ref m_PATReader );
                    ResetReader( ref m_SDTReader );
                    m_groupReader = null;

                    // Forward
                    OnDispose();
                }
                finally
                {
                    // Did it
                    m_IsDisposing = false;
                    m_IsDisposed = true;
                }
        }

        #endregion
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
