using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using JMS.DVB.DirectShow.AccessModules;


namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Verbindet die Anwendung mit einer VCR.NET Recording Service.
    /// </summary>
    public class VCRAdaptor : UDPAdaptor
    {
        /// <summary>
        /// Der aktuelle Zugriffsmoduls.
        /// </summary>
        private VCRConnector m_CurrentConnector;

        /// <summary>
        /// Das verwendete Profil.
        /// </summary>
        private string m_Profile;

        /// <summary>
        /// Zugriff auf die Informationen zur Senderliste.
        /// </summary>
        public readonly IChannelInfo ChannelInfo;

        /// <summary>
        /// Zugriff auf die Konfiguration für den Netzwerkempfang.
        /// </summary>
        public readonly IStreamInfo StreamInfo;

        /// <summary>
        /// Der Aufrufpunkt für die Web Dienste.
        /// </summary>
        public string EndPoint
        {
            get
            {
                // Construct Url
                var uri = RemoteInfo.ServerUri.AbsoluteUri;

                // Create endpoint
                return uri.Substring( 0, uri.LastIndexOf( '/' ) );
            }
        }

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz.
        /// </summary>
        /// <param name="main">Die zugehörige Anwendung.</param>
        public VCRAdaptor( IViewerSite main )
            : this( main, default( string ) )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz.
        /// </summary>
        /// <param name="main">Die zugehörige Anwendung.</param>
        /// <param name="streamIndex">Teilaufzeichnung, die betrachtet werden soll.</param>
        /// <param name="timeshift">Gesetzt, wenn der Timeshift Modus aktiviert werden soll.</param>
        public VCRAdaptor( IViewerSite main, int streamIndex, bool timeshift )
            : base( main )
        {
            // Connect to alternate interfaces
            ChannelInfo = (IChannelInfo) main;
            StreamInfo = (IStreamInfo) main;

            // Remember
            m_Profile = RemoteInfo.VCRProfile;

            // Use default
            if (string.IsNullOrEmpty( m_Profile ))
                m_Profile = "*";

            // Construct Url
            var uri = RemoteInfo.ServerUri;

            // Connect to stream
            Connect( StreamInfo.BroadcastIP, StreamInfo.BroadcastPort, uri.Host );

            // Find all current activities
            var activities = VCRNETRestProxy.GetActivitiesForProfile( EndPoint, Profile );
            if (activities.Count < 1)
                StartLIVE( true );
            else
            {
                // Find the activity
                var current = activities.FirstOrDefault( activity => activity.streamIndex == streamIndex );
                if (current == null)
                    StartWatch( null, true );
                else if (timeshift && string.IsNullOrEmpty( StreamInfo.BroadcastIP ) && (current.files.Length > 0))
                    StartReplay( current.files[0], current.name, current, true );
                else
                    StartWatch( string.Format( "dvbnet:{0}", streamIndex ), true );
            }
        }

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz.
        /// </summary>
        /// <param name="main">Die zugehörige Anwendung.</param>
        /// <param name="replayPath">Pfad zu einer VCR.NET Aufzeichnungsdatei.</param>
        public VCRAdaptor( IViewerSite main, string replayPath )
            : base( main )
        {
            // Connect to alternate interfaces
            ChannelInfo = (IChannelInfo) main;
            StreamInfo = (IStreamInfo) main;

            // Remember
            m_Profile = RemoteInfo.VCRProfile;

            // Use default
            if (string.IsNullOrEmpty( m_Profile ))
                m_Profile = "*";

            // Construct Url
            Uri uri = RemoteInfo.ServerUri;

            // Connect to stream
            Connect( StreamInfo.BroadcastIP, StreamInfo.BroadcastPort, uri.Host );

            // Check startup mode
            if (string.IsNullOrEmpty( replayPath ))
            {
                // Special if LIVE is active
                var current = VCRNETRestProxy.GetFirstActivityForProfile( EndPoint, Profile );
                if (current != null)
                    if (!current.IsActive)
                        current = null;
                    else if ("LIVE".Equals( current.name ))
                        current = null;

                // Start correct access module
                if (current == null)
                    StartLIVE( true );
                else
                    StartWatch( null, true );
            }
            else
            {
                // Start remote file replay
                StartReplay( replayPath, null, null, true );
            }
        }

        /// <summary>
        /// Ermittelt die aktuelle Senderliste.
        /// </summary>
        public override void LoadStations()
        {
            // Forward as is
            m_CurrentConnector.LoadStations();
        }

        /// <summary>
        /// Entfernt das aktuelle Zugriffsmodul.
        /// </summary>
        /// <param name="startup">Für den ersten Aufruf gesetzt.</param>
        private void DestroyConnector( bool startup )
        {
            // Stop all
            if (!startup)
                try
                {
                    // Attach to accessor
                    var accessor = Accessor;
                    if (accessor != null)
                    {
                        // Stop sending data
                        accessor.Stop();

                        // Stop graph
                        accessor.StopGraph();
                    }

                    // Restart videotext caching from scratch
                    VideoText.Deactivate( true );
                }
                catch
                {
                    // Ignore any error - maybe we are disposing the instance
                }

            // Forget
            using (m_CurrentConnector)
                m_CurrentConnector = null;
        }

        /// <summary>
        /// Beendet die Nutzung dieser Instanz.
        /// </summary>
        protected override void OnDispose()
        {
            // Check connector
            DestroyConnector( false );
        }

        /// <summary>
        /// Wählt einen anderen Sender.
        /// </summary>
        /// <param name="context">Senderbeschreibung abhängig vom Zugriffsmodul.</param>
        /// <returns>Sendername mit ausgewählter Tonspur oder <i>null</i>.</returns>
        public override string SetStation( object context )
        {
            // Forward
            if (m_CurrentConnector.SetStation( context ) == null)
                return null;

            // Reset EPG display
            ShowCurrentEntry();

            // Restart videotext caching from scratch
            VideoText.Deactivate( true );

            // Forget EPG data collected so far
            CurrentEntry = null;
            NextEntry = null;

            // Reset audio selection
            return RestartAudio( false );
        }

        /// <summary>
        /// Wählte eine andere Tonspur.
        /// </summary>
        /// <param name="audio">Voller Name der Tonspur oder <i>null</i> für die
        /// bevorzugte Tonspur.</param>
        /// <returns>Sendername mit ausgewählter Tonspur oder <i>null</i>.</returns>
        public override string SetAudio( string audio )
        {
            // Forward
            return SetAudio( audio, m_CurrentConnector.UpdateSettings );
        }

        /// <summary>
        /// Meldet den Sender, der beim Starten der Anwendung ausgewählt werden soll.
        /// </summary>
        protected override string DefaultStation { get { return m_CurrentConnector.DefaultStation; } }

        /// <summary>
        /// Meldet die Tonspur, die beim Starten der Anwendung ausgewählt werden soll.
        /// </summary>
        protected override string DefaultAudio { get { return m_CurrentConnector.DefaultAudio; } }

        /// <summary>
        /// Meldet alle auf dem aktuellen Portal verfügbaren NVOD Dienste.
        /// </summary>
        public override ServiceItem[] Services { get { return m_CurrentConnector.Services; } }

        /// <summary>
        /// Wählte einen NVOD Dienst des aktuellen Portals zur Anzeige.
        /// </summary>
        /// <param name="service">Name des Dienstes.</param>
        /// <returns>Name des Dienstes mit aktueller Tonspur oder <i>null</i>.</returns>
        public override string SetService( ServiceItem service )
        {
            // Forward
            if (m_CurrentConnector.SetService( service ) == null)
                return null;
            else
                return RestartAudio( false );
        }

        /// <summary>
        /// Prüft, ob die Verbindung zum VCR.NET Recording Service in der aktuellen
        /// Form aufrecht gehalten werden kann.
        /// </summary>
        /// <param name="fine">Gesetzt für den Aufruf im Sekundenrythmus.</param>
        public override void KeepAlive( bool fine )
        {
            // Forward
            if (!fine)
                m_CurrentConnector.KeepAlive();
        }

        /// <summary>
        /// Trägt zusätzliche Konfigurationsoptionen in die Liste aller Konfigurationseinträge ein.
        /// </summary>
        public override void FillOptions()
        {
            // Request all
            var profiles = VCRNETRestProxy.GetProfilesSync( EndPoint ).Select( profile => profile.name ).ToArray();

            // Process all
            foreach (var profile in profiles)
            {
                // Format to use
                string format = Properties.Resources.OptionProfile;

                // See if this is active
                if (string.IsNullOrEmpty( Profile ) && ProfileManager.ProfileNameComparer.Equals( profiles[0], profile ))
                    format = Properties.Resources.OptionProfileActive;
                else if (ProfileManager.ProfileNameComparer.Equals( Profile, profile ))
                    format = Properties.Resources.OptionProfileActive;
                else
                    format = Properties.Resources.OptionProfile;

                // Register - Clone() is important since anonymous delegate would bind profile to the last iteration value for ALL items
                Parent.AddOption( new OptionDisplay( string.Format( format, profile ), () => ChangeProfile( profile ) ) );
            }

            // Forward
            m_CurrentConnector.FillOptions();
        }

        /// <summary>
        /// Meldet den Namen des aktuellen Senders.
        /// </summary>
        protected override string StationName { get { return m_CurrentConnector.StationName; } }

        /// <summary>
        /// Meldet das aktuelle Profil.
        /// </summary>
        public string Profile { get { return m_Profile; } }

        /// <summary>
        /// Verändert das aktuelle VCR.NET Geräteprofil.
        /// </summary>
        /// <remarks>
        /// Erfolgt eine Auswahl des aktuellen Profils, so wird die Verbindung zum
        /// VCR.NET erneut aufgebaut.
        /// </remarks>
        /// <param name="profile">Das zu verwendende Profil.</param>
        private void ChangeProfile( string profile )
        {
            // Fire
            m_CurrentConnector.OnProfileChanging();

            // Change
            m_Profile = profile;

            // Save changes
            RemoteInfo.VCRProfile = Profile;

            // Fire
            m_CurrentConnector.OnProfileChanged();

            // Reload all
            ChannelListChanged();
        }

        /// <summary>
        /// Lädt die Senderliste neu.
        /// </summary>
        public void ChannelListChanged()
        {
            // Reload options
            Parent.ResetKeyHandlers();
            Parent.ResetOptions();
            Parent.FillOptions();

            // Reload channels
            Parent.Restart();

            // Reload current audio map
            LoadAudio();
        }

        /// <summary>
        /// Aktiviert eine LIVE Verbindung zum VCR.NET.
        /// </summary>
        public void StartLIVE()
        {
            // Forward
            StartLIVE( false );
        }

        /// <summary>
        /// Startet die Wiedergabe einer Aufzeichnungsdatei.
        /// </summary>
        /// <param name="path">Voller Pfad einer VCR.NET Aufzeichnungsdatei.</param>
        /// <param name="name">Name der Teilaufzeichnung.</param>
        public void StartReplay( string path, string name )
        {
            // Forward
            StartReplay( path, name, null );
        }

        /// <summary>
        /// Startet die Wiedergabe einer Aufzeichnungsdatei.
        /// </summary>
        /// <param name="path">Voller Pfad einer VCR.NET Aufzeichnungsdatei.</param>
        /// <param name="name">Name der Teilaufzeichnung.</param>
        /// <param name="recording">Detailinformationen zur aktuellen Aufzeichnung.</param>
        public void StartReplay( string path, string name, VCRNETRestProxy.Current recording )
        {
            // Forward
            StartReplay( path, name, recording, false );
        }

        /// <summary>
        /// Startet die Wiedergabe einer Aufzeichnungsdatei.
        /// </summary>
        /// <param name="path">Voller Pfad einer VCR.NET Aufzeichnungsdatei.</param>
        /// <param name="name">Name der Teilaufzeichnung.</param>
        /// <param name="recording">Detailinformationen zur aktuellen Aufzeichnung.</param>
        /// <param name="startup">Während des Starts der Anwendung gesetzt.</param>
        private void StartReplay( string path, string name, VCRNETRestProxy.Current recording, bool startup )
        {
            // Shut down
            DestroyConnector( startup );

            // All files
            var files = new List<string>();

            // Try to count number of files
            if (!string.IsNullOrEmpty( path ))
            {
                // Add self
                files.Add( path );

                // See if there are more
                if (recording != null)
                    if (recording.files != null)
                        if (path.ToLower().EndsWith( ".ts" ))
                        {
                            // Get prefix
                            var prefix = path.Substring( 0, path.Length - 3 ) + " - ";

                            // Search all
                            foreach (var test in recording.files)
                                if (test.ToLower().EndsWith( ".ts" ))
                                    if (string.Compare( test, 0, prefix, 0, prefix.Length, true ) == 0)
                                        files.Add( test );
                        }
            }

            // Restart
            m_CurrentConnector = new FileConnector( this, path, name, files.ToArray() );

            // Done on first call
            if (startup)
                return;

            // Reset all
            ChannelListChanged();

            // Reload all
            ShowMessage( RestartAudio( false ), Properties.Resources.NameTitle, true );
        }

        /// <summary>
        /// Aktiviert eine Verbindung zum VCR.NET Recording Service.
        /// </summary>
        /// <param name="startup">Während des Starts der Anwendung gesetzt.</param>
        private void StartLIVE( bool startup )
        {
            // Shut down
            DestroyConnector( startup );

            // Restart
            m_CurrentConnector = new LiveConnector( this );

            // Reload all
            if (!startup)
                ChannelListChanged();
        }

        /// <summary>
        /// Verbindet sich mit der laufenden Aufzeichnung im VCR.NET Recording Service.
        /// </summary>
        /// <param name="startWith">Auszuwählende Aufzeichnung.</param>
        public void StartWatch( string startWith )
        {
            // Forward
            StartWatch( startWith, false );
        }

        /// <summary>
        /// Verbindet sich mit der laufenden Aufzeichnung im VCR.NET Recording Service.
        /// </summary>
        /// <param name="startup">Während des Starts der Anwendung gesetzt.</param>
        /// <param name="startWith">Auszuwählende Aufzeichnung.</param>
        private void StartWatch( string startWith, bool startup )
        {
            // Shut down
            DestroyConnector( startup );

            // Restart
            m_CurrentConnector = new CurrentConnector( this, startWith );

            // Reload all
            if (!startup)
                ChannelListChanged();
        }

        /// <summary>
        /// Beginnt eine neue Aufzeichnung.
        /// </summary>
        public override void StartRecording()
        {
            // Forward
            m_CurrentConnector.StartRecording();
        }

        /// <summary>
        /// Wird aufgerufen, wenn das Zugriffsmodul Daten anfordert.
        /// </summary>
        /// <param name="endPoint">Das anfordernde Zugriffsmodul.</param>
        protected override void OnWaitData( TransportStreamReceiver endPoint )
        {
            // Load connector
            var connector = m_CurrentConnector;
            if (connector != null)
                connector.OnWaitData();
        }

        /// <summary>
        /// Speicert die zuletzt verwendete Tonspur.
        /// </summary>
        protected override string LastAudio
        {
            set
            {
                // Store
                if (RemoteInfo != null)
                    RemoteInfo.VCRAudio = value;
            }
        }

        /// <summary>
        /// Taste zum Anzeigen der Liste aller Dienste.
        /// </summary>
        public override Keys? ServiceListKey { get { return (m_CurrentConnector == null) ? base.ServiceListKey : m_CurrentConnector.ServiceListKey; } }

        /// <summary>
        /// Taste zur Anzeige der Tonspuren.
        /// </summary>
        public override Keys? TrackListKey { get { return (m_CurrentConnector == null) ? base.TrackListKey : m_CurrentConnector.TrackListKey; } }

        /// <summary>
        /// Teste zum Anzeigen der Senderliste.
        /// </summary>
        public override Keys? StationListKey { get { return (m_CurrentConnector == null) ? base.StationListKey : m_CurrentConnector.StationListKey; } }

        /// <summary>
        /// Taste zum Beginnen oder Beenden einer Aufzeichnung.
        /// </summary>
        public override Keys? RecordingKey { get { return (m_CurrentConnector == null) ? base.RecordingKey : m_CurrentConnector.RecordingKey; } }

        /// <summary>
        /// Taste zum Umschalten des TimeShift Modus.
        /// </summary>
        public override Keys? TimeShiftKey { get { return (m_CurrentConnector == null) ? base.TimeShiftKey : m_CurrentConnector.TimeShiftKey; } }

        /// <summary>
        /// Text für den Menüeintrag zum Starten einer Aufzeichnung.
        /// </summary>
        public override string RecordingText { get { return (m_CurrentConnector == null) ? base.RecordingText : m_CurrentConnector.RecordingText; } }

        /// <summary>
        /// Meldet, ob ein Videotext Signal vorliegt.
        /// </summary>
        public override bool TTXAvailable { get { return (m_CurrentConnector != null) && m_CurrentConnector.TTXAvailable; } }

        /// <summary>
        /// Meldet, ob beim Ändern der Direct Show Filter ein Neustart des
        /// Graphen unterstützt wird.
        /// </summary>
        public override bool CanRestartGraph { get { return (m_CurrentConnector == null) ? base.CanRestartGraph : m_CurrentConnector.CanRestartGraph; } }
    }
}
