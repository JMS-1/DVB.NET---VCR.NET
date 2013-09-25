using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;


namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Kommunziert mit einem VCR.NET Server im LIVE Modus.
    /// </summary>
    public class LiveConnector : VCRConnector
    {
        /// <summary>
        /// Menüelement mit Aufzeichnungsdauer.
        /// </summary>
        private class DurationOption
        {
            /// <summary>
            /// Dauer in Minuten.
            /// </summary>
            public readonly int Duration;

            /// <summary>
            /// Zugehörige verbindungsinstanz.
            /// </summary>
            public readonly LiveConnector Connector;

            /// <summary>
            /// Erzeugt einen neuen Menüeintrag.
            /// </summary>
            /// <param name="connector">Zugehörige Verbindung.</param>
            /// <param name="duration">Aufzeichnungsdauer in Minuten.</param>
            public DurationOption( LiveConnector connector, int duration )
            {
                // Remember all
                Connector = connector;
                Duration = duration;
            }

            /// <summary>
            /// Aufruf weiterleiten.
            /// </summary>
            public void Process()
            {
                // Forward
                Connector.StartRecording( Duration );
            }
        }

        /// <summary>
        /// Vorschläge für die Aufzeichnungsdauer.
        /// </summary>
        private static int[] Durations = { 5, 10, 15, 30, 45, 60, 75, 90, 120, 150, 180, 210, 240 };

        /// <summary>
        /// Aktuelle Senderliste zur Auflösung der NVOD Dienste.
        /// </summary>
        private readonly Dictionary<SourceIdentifier, VCRNETRestProxy.Source> Sources = new Dictionary<SourceIdentifier, VCRNETRestProxy.Source>();

        /// <summary>
        /// Aktueller Sender.
        /// </summary>
        private VCRNETRestProxy.Source CurrentSource = null;

        /// <summary>
        /// Name des aktuellen NVOD Dienstes.
        /// </summary>
        private string CurrentService = null;

        /// <summary>
        /// Last status we checked.
        /// </summary>
        private volatile VCRNETRestProxy.Status m_LastStatus = null;

        /// <summary>
        /// Die Basisadresse des Dienstes.
        /// </summary>
        private readonly string m_serverRoot;

        /// <summary>
        /// Erzeugt eine neue Kommunikationsinstanz.
        /// </summary>
        /// <param name="adaptor">Zugehörige Verbindung zur Anwendung.</param>
        public LiveConnector( VCRAdaptor adaptor )
            : base( adaptor )
        {
            // Load REST base address
            m_serverRoot = adaptor.EndPoint + "/zapping/";

            // Be safe
            try
            {
                // Start LIVE mode
                OnProfileChanged();
            }
            catch
            {
                // For now any error is ignored
            }
        }

        /// <summary>
        /// Ermittelt die aktuelle Senderliste.
        /// </summary>
        public override void LoadStations()
        {
            // Allow restriction on channels
            Favorites.EnableFavorites();

            // Reset all
            CurrentSource = null;
            CurrentService = null;
            Sources.Clear();

            // Check free TV mode
            var free = Adaptor.ChannelInfo.FreeTV;
            var pay = Adaptor.ChannelInfo.PayTV;

            // Check service type
            var radio = Adaptor.ChannelInfo.UseRadio;
            var tv = Adaptor.ChannelInfo.UseTV;

            // Get radio and video separatly
            for (int radioFlag = 2; radioFlag-- > 0; )
                foreach (var source in VCRNETRestProxy.ReadSourcesSync( m_serverRoot, Profile, radioFlag == 0, radioFlag == 1 ))
                {
                    // Add anything to map
                    Sources[SourceIdentifier.Parse( source.source )] = source;

                    // Check type
                    if (radioFlag == 1)
                    {
                        // See if radio is allowed
                        if (tv && !radio)
                            continue;
                    }
                    else
                    {
                        // See if TV is allowed
                        if (radio && !tv)
                            continue;
                    }

                    // Check encryption
                    if (source.encrypted)
                    {
                        // Only if encrypted is allowed
                        if (free && !pay)
                            continue;
                    }
                    else
                    {
                        // Only if free is allowed
                        if (pay && !free)
                            continue;
                    }

                    // Process
                    Favorites.AddChannel( source.nameWithProvider, source );
                }

            // Finished
            Favorites.FillChannelList();
        }

        /// <summary>
        /// Beendet die Nutzung dieser Kommunikationsinstanz endgültig.
        /// </summary>
        protected override void OnDispose()
        {
            // Release network ressources
            VCRNETRestProxy.DisconnectSync( m_serverRoot, Profile );
        }

        /// <summary>
        /// Wählt einen neuen Sender.
        /// </summary>
        /// <param name="context">Der zu wählende Sender.</param>
        /// <returns>Name des neuen Senders inklsuive der gewählten Tonspur
        /// oder <i>null</i>.</returns>
        public override string SetStation( object context )
        {
            // Stop sending data
            Accessor.Stop();

            // Restart videotext caching from scratch
            Adaptor.VideoText.Deactivate( true );

            // Change type
            var source = (VCRNETRestProxy.Source) context;

            // Change the program
            VCRNETRestProxy.TuneSync( m_serverRoot, Profile, source.source );

            // Remember
            CurrentSource = source;
            CurrentService = null;

            // Store to settings
            return (Adaptor.RemoteInfo.VCRStation = source.nameWithProvider);
        }

        /// <summary>
        /// Ermittelt zu einem VCR.NET Sender den vollen Namen inklusive Anbieter.
        /// </summary>
        /// <param name="channel">Ein Sender.</param>
        /// <returns>Der volle Name des Senders.</returns>
        private static string GetStationName( VCRNETRestProxy.Source channel )
        {
            // None
            if (null == channel)
                return null;
            else
                return channel.nameWithProvider;
        }

        /// <summary>
        /// Ermittelt den Namen des aktuellen Senders.
        /// </summary>
        public override string StationName
        {
            get
            {
                // Forward
                if (string.IsNullOrEmpty( CurrentService ))
                    return GetStationName( CurrentSource );
                else
                    return CurrentService;
            }
        }

        /// <summary>
        /// Ermittelt die Liste der auf dem aktuellen Portal verfügbaren
        /// NVOD Dienste.
        /// </summary>
        public override ServiceItem[] Services
        {
            get
            {
                // The list
                var result =
                     VCRNETRestProxy
                        .GetStatusSync( m_serverRoot, Profile )
                        .services
                        .Select( service => new ServiceItem( SourceIdentifier.Parse( service.source ), service.nameWithIndex ) )
                        .ToList();

                // Order
                result.Sort();

                // Report
                return result.ToArray();
            }
        }

        /// <summary>
        /// Wählt einen NVOD Dienst aus.
        /// </summary>
        /// <param name="service">Der gewünschte Dienst.</param>
        /// <returns>Name des Dienstes inklusive der aktuellen Tonspur oder
        /// <i>null</i>, wenn eine Aktivierung des Dienstes nicht möglich war.</returns>
        public override string SetService( ServiceItem service )
        {
            // Must be a known channel
            VCRNETRestProxy.Source source;
            if (!Sources.TryGetValue( service.Identifier, out source ))
                return null;

            // Stop sending data
            Accessor.Stop();

            // Tune
            VCRNETRestProxy.TuneSync( m_serverRoot, Profile, SourceIdentifier.ToString( service.Identifier ) );

            // Got the portal
            if (service.Index == 0)
            {
                // Reset
                CurrentService = null;

                // Done
                return source.nameWithProvider;
            }

            // Got a real service
            CurrentService = service.Name;

            // Done
            return CurrentService;
        }

        /// <summary>
        /// Prüft, ob der zugehörige VCR.NET noch im LIVE Modus Daten an die
        /// aktuelle Anwendung sendet. Ist das nicht der Fall, wird mit in
        /// zur Anzeige der aktuellen Aufzeichnung gewechselt.
        /// </summary>
        public override void KeepAlive()
        {
            // Read the status
            var status = m_LastStatus;

            // Clear it
            m_LastStatus = null;

            // Not connected to us
            if (status != null)
                if (!StringComparer.InvariantCultureIgnoreCase.Equals( status.target, Adaptor.Target ))
                {
                    // Switch mode
                    Adaptor.StartWatch( null );

                    // Done
                    return;
                }

            // Restart request	
            VCRNETRestProxy.GetStatus( m_serverRoot, Profile, newState => m_LastStatus = newState, null );
        }

        /// <summary>
        /// Beim Wechsel des Profils wird die Verbindung zum VCR.NET gelöst.
        /// </summary>
        public override void OnProfileChanging()
        {
            // Forget the status
            m_LastStatus = null;

            // Stop LIVE mode
            VCRNETRestProxy.DisconnectSync( m_serverRoot, Profile );
        }

        /// <summary>
        /// Stellt eine neue Verbindung zu VCR.NET her.
        /// </summary>
        public override void OnProfileChanged()
        {
            // Start LIVE mode
            VCRNETRestProxy.ConnectSync( m_serverRoot, Profile, Adaptor.Target );
        }

        /// <summary>
        /// Bietet den Start einer Aufzeichnung an.
        /// </summary>
        public override void StartRecording()
        {
            // Attach to list
            var selection = Adaptor.Parent.ScratchComboBox;

            // Reset
            selection.Items.Clear();

            // Fill
            for (var i = 0; i < Durations.Length; )
            {
                // Create item holder
                var option = new DurationOption( this, Durations[i++] );

                // Create text
                var name = string.Format( Properties.Resources.RecordingItem, option.Duration, DateTime.UtcNow.AddMinutes( option.Duration ).ToLocalTime() );

                // Register
                selection.Items.Add( new OptionDisplay( name, option.Process ) );
            }

            // Show up
            Adaptor.Parent.ShowList( Properties.Resources.RecordingStartHeadLine, 0, OSDShowMode.Other );
        }

        /// <summary>
        /// Startet eine Aufzeichnung im VCR.NET auf dem aktuellen Sender.
        /// </summary>
        /// <param name="duration"></param>
        private void StartRecording( int duration )
        {
            // Read the status
            var status = VCRNETRestProxy.GetStatusSync( m_serverRoot, Profile );

            // Not connected to us
            if (!StringComparer.InvariantCultureIgnoreCase.Equals( status.target, Adaptor.Target ))
                return;

            // Configure the new schedule
            var schedule =
                new VCRNETRestProxy.Schedule
                {
                    lastDay = new DateTime( 2999, 12, 31 ),
                    name = "Gestartet vom DVB.NET Viewer",
                    firstStart = DateTime.UtcNow,
                    duration = duration,
                };

            // Configure the new job
            var job = new VCRNETRestProxy.Job
            {
                sourceName = CurrentSource.nameWithProvider,
                name = "Manuelle Aufzeichnung",
                withSubtitles = true,
                withVideotext = true,
                includeDolby = true,
                allLanguages = true,
                device = Profile,
            };

            // Send it
            VCRNETRestProxy.CreateNewSync( Adaptor.EndPoint, job, schedule );
        }

        /// <summary>
        /// Meldet, dass bei einer LIVE Verbindung Sender und Tonspur bei
        /// Veränderung in den Einstellungen zu vermerken sind.
        /// </summary>
        public override bool UpdateSettings { get { return true; } }
    }
}
