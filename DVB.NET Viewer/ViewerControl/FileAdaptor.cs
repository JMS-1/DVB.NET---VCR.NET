using System;
using System.Windows.Forms;

using JMS.DVB.DirectShow.AccessModules;


namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Verbindet die Anwendung mit einem Transport Stream in einer Datei.
    /// </summary>
    public class FileAdaptor : TSAdaptor, IFileReplay
    {
        /// <summary>
        /// N�chste gew�nschte Position.
        /// </summary>
        private double? m_NextPosition = null;

        /// <summary>
        /// Zeitpunkt, an dem die Position zuletzt ver�ndert wurde.
        /// </summary>
        private DateTime m_LastPositionChange = DateTime.MinValue;

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz.
        /// </summary>
        /// <param name="main">Die zugeh�rige Anwendung.</param>
        /// <param name="path">Voller Pfad zu einer Datei.</param>
        public FileAdaptor( IViewerSite main, string path )
            : base( main )
        {
            // Connect to file player
            SetAccessor( new FileAccessor( path ) );

            // Finished
            OnConnected();

            // Configure keys
            RegisterReplayKeys( main, this );
        }

        private bool IsNextPositionValid
        {
            get
            {
                // Not at al
                if (!m_NextPosition.HasValue) return false;

                // Get delta
                TimeSpan delta = DateTime.UtcNow - m_LastPositionChange;

                // Attach to site
                IGeneralInfo info = Parent as IGeneralInfo;

                // Respect OSD display time
                if (null == info) return (delta.TotalSeconds < 5);

                // Only for some seconds
                return (delta.TotalSeconds < (5 + info.OSDLifeTime));
            }
        }

        /// <summary>
        /// Wird verwendet, um die Position in der Datei zu verschieben.
        /// </summary>
        public override void StartRecording()
        {
            // Stop sending data
            Accessor.Stop();

            // Stop displaying data
            Accessor.StopGraph();

            // Must change position)
            if (IsNextPositionValid)
            {
                // Set it
                Accessor.Position = m_NextPosition.Value;

                // Forget it
                m_NextPosition = null;
            }

            // Start again
            RestartAudio( true );
        }

        /// <summary>
        /// Ermittelt die aktuelle Senderliste.
        /// </summary>
        public override void LoadStations()
        {
            // Reset
            Favorites.ClearAll();

            // Load audio
            LoadAudio();
        }

        /// <summary>
        /// W�hlt einen anderen Sender.
        /// </summary>
        /// <param name="context">Senderbeschreibung abh�ngig vom Zugriffsmodul.</param>
        /// <returns>Sendername mit ausgew�hlter Tonspur oder <i>null</i>.</returns>
        public override string SetStation( object context )
        {
            // Do nothing
            return null;
        }

        /// <summary>
        /// W�hlt eine andere Tonspur.
        /// </summary>
        /// <param name="audio">Voller Name der Tonspur oder <i>null</i> f�r die
        /// bevorzugte Tonspur.</param>
        /// <returns>Sendername mit ausgew�hlter Tonspur oder <i>null</i>.</returns>
        public override string SetAudio( string audio )
        {
            // Forward
            return SetAudio( audio, false );
        }

        /// <summary>
        /// Meldet den Sender, der beim Starten der Anwendung ausgew�hlt werden soll.
        /// </summary>
        protected override string DefaultStation
        {
            get
            {
                // Report
                return null;
            }
        }

        /// <summary>
        /// Meldet die Tonspur, die beim Starten der Anwendung ausgew�hlt werden soll.
        /// </summary>
        protected override string DefaultAudio
        {
            get
            {
                // Report
                return null;
            }
        }

        /// <summary>
        /// Meldet alle auf dem aktuellen Portal verf�gbaren NVOD Dienste.
        /// </summary>
        public override ServiceItem[] Services
        {
            get
            {
                // Forward
                return new ServiceItem[0];
            }
        }

        /// <summary>
        /// W�hlte einen NVOD Dienst des aktuellen Portals zur Anzeige.
        /// </summary>
        /// <param name="service">Name des Dienstes.</param>
        /// <returns>Name des Dienstes mit aktueller Tonspur oder <i>null</i>.</returns>
        public override string SetService( ServiceItem service )
        {
            // Do nothing
            return null;
        }

        /// <summary>
        /// Meldet den Namen des aktuellen Senders.
        /// </summary>
        protected override string StationName
        {
            get
            {
                // Forward
                return Accessor.Name;
            }
        }

        /// <summary>
        /// Das verwendet Dateizugriffsmodul.
        /// </summary>
        public new FileAccessor Accessor
        {
            get
            {
                // Report
                return (FileAccessor) base.Accessor;
            }
        }

        /// <summary>
        /// Beendet die Nutzung dieser Instanz.
        /// </summary>
        protected override void OnDispose()
        {
        }

        /// <summary>
        /// Konfiguriert die Tastatur f�r das Abspielen einer Datei.
        /// </summary>
        /// <param name="viewer">Die zugeh�rige Anzeigeeinheit.</param>
        /// <param name="replay">Die Komponente, die f�r das Auslesen der Datei zust�ndig ist.</param>
        public static void RegisterReplayKeys( IViewerSite viewer, IFileReplay replay )
        {
            // Register all keys to move in file
            viewer.SetKeyHandler( Keys.Subtract, () => replay.MovePosition( -0.001 ) );
            viewer.SetKeyHandler( Keys.PageDown, () => replay.MovePosition( -0.01 ) );
            viewer.SetKeyHandler( Keys.PageUp, () => replay.MovePosition( +0.01 ) );
            viewer.SetKeyHandler( (Keys) 189, () => replay.MovePosition( -0.001 ) );
            viewer.SetKeyHandler( (Keys) 187, () => replay.MovePosition( +0.001 ) );
            viewer.SetKeyHandler( Keys.Add, () => replay.MovePosition( +0.001 ) );
            viewer.SetKeyHandler( Keys.F3, () => replay.MovePosition( 0 ) );
            viewer.SetKeyHandler( Keys.D0, () => replay.Position = 0.0 );
            viewer.SetKeyHandler( Keys.D1, () => replay.Position = 0.1 );
            viewer.SetKeyHandler( Keys.D2, () => replay.Position = 0.2 );
            viewer.SetKeyHandler( Keys.D3, () => replay.Position = 0.3 );
            viewer.SetKeyHandler( Keys.D4, () => replay.Position = 0.4 );
            viewer.SetKeyHandler( Keys.D5, () => replay.Position = 0.5 );
            viewer.SetKeyHandler( Keys.D6, () => replay.Position = 0.6 );
            viewer.SetKeyHandler( Keys.D7, () => replay.Position = 0.7 );
            viewer.SetKeyHandler( Keys.D8, () => replay.Position = 0.8 );
            viewer.SetKeyHandler( Keys.D9, () => replay.Position = 0.9 );
        }

        /// <summary>
        /// Eine Senderauswahl wird nicht unterst�tzt.
        /// </summary>
        public override Keys? StationListKey
        {
            get
            {
                // None
                return null;
            }
        }

        /// <summary>
        /// Die Auswahl von Diensten wird nicht unterst�tzt.
        /// </summary>
        public override Keys? ServiceListKey
        {
            get
            {
                // None
                return null;
            }
        }

        /// <summary>
        /// Der Men�punkt f�r die Aufzeichnung wird f�r die Auswahl einer Position in der
        /// Datei mi�braucht.
        /// </summary>
        public override Keys? RecordingKey
        {
            get
            {
                // Report
                return Keys.Add;
            }
        }

        /// <summary>
        /// Der Men�punkt f�r die Aufzeichnung wird f�r die Auswahl einer Position in der
        /// Datei mi�braucht.
        /// </summary>
        public override string RecordingText
        {
            get
            {
                // Report
                return Properties.Resources.Context_FilePosition;
            }
        }

        /// <summary>
        /// Meldet, ob beim �ndern der Direct Show Filter ein Neustart des
        /// Graphen unterst�tzt wird.
        /// </summary>
        public override bool CanRestartGraph
        {
            get
            {
                // Report
                return false;
            }
        }

        #region IFileReplay Members

        void IFileReplay.MovePosition( double delta )
        {
            // Load once
            if ((0 == delta) || !IsNextPositionValid) m_NextPosition = Accessor.Position;

            // Move it
            Position = m_NextPosition.Value + delta;
        }

        double IFileReplay.Position
        {
            set
            {
                // Forward
                Position = value;
            }
        }

        private double Position
        {
            set
            {
                // Move it and remember time
                m_NextPosition = Math.Max( 0, Math.Min( 1, value ) );
                m_LastPositionChange = DateTime.UtcNow;

                // Show
                Parent.ShowPositionInFile( m_NextPosition.Value );
            }
        }

        #endregion
    }
}
