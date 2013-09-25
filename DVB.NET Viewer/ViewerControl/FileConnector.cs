using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;


namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Verbindet die Anwendung mit einem TCP/IP UDP Strom, der einen Transport Stream
    /// sendet.
    /// </summary>
    public class FileConnector : VCRConnector, IFileReplay
    {
        /// <summary>
        /// Signatur zum Aufruf von <see cref="StartRecording"/>.
        /// </summary>
        private delegate void StartRecordingCallback();

        /// <summary>
        /// Per Aufruf angeforderte Anzahl von Bytes.
        /// </summary>
        private readonly int ChunkSize = 2000000;

        /// <summary>
        /// Die VCR.NET Aufzeichnungsdatei.
        /// </summary>
        private string m_Path;

        /// <summary>
        /// Name der Teilaufzeichnung.
        /// </summary>
        private string m_Name;

        /// <summary>
        /// Ursprünglicher Name der Teilaufzeichnung.
        /// </summary>
        private string m_OriginalName;

        /// <summary>
        /// Aktuelle Position in der Datei.
        /// </summary>
        private long m_Position = 0;

        /// <summary>
        /// Gesamte Größe der Datei - diese kann sich verändern, wenn es sich um
        /// eine laufende Aufzeichnung handelt.
        /// </summary>
        private long? m_Length = null;

        /// <summary>
        /// Neue Position beim Abspielen.
        /// </summary>
        private double? m_NextPosition = null;

        /// <summary>
        /// Zeitpunkt, an dem die Position zuletzt verändert wurde.
        /// </summary>
        private DateTime m_LastPositionChange = DateTime.MinValue;

        /// <summary>
        /// Zeitpunkt, zu dem letztmalig ein Neustart durchgeführt wurde.
        /// </summary>
        private DateTime m_LastStart = DateTime.MinValue;

        /// <summary>
        /// Erzwingt eine neue Position beim nächsten Zugriff.
        /// </summary>
        private double? m_Move = null;

        /// <summary>
        /// Gesetzt während eines asynchronen Aufrufs.
        /// </summary>
        private bool m_Waiting = false;

        /// <summary>
        /// Gesetzt, sobald die Nutzung dieser Instanz beendet wurde.
        /// </summary>
        private bool m_Disposed = false;

        /// <summary>
        /// Minimale Anzahl von Sekunden zwischen Neustarts der Anzeige.
        /// </summary>
        private readonly int RestartInterval = 2;

        /// <summary>
        /// Zeitgrenze zur Erkennung, ob die Zeitstempel fehlerhaft sind.
        /// </summary>
        private readonly long RestartTrigger = -20000000;

        /// <summary>
        /// Maximaler Füllgrad des Zwischenspeichers.
        /// </summary>
        private readonly long MaximumAheadTime = 10000000;

        /// <summary>
        /// Die einzelnen Dateien bei Aufzeichnungen eines Senders mit Regionalfenster.
        /// </summary>
        private string[] m_Paths;

        /// <summary>
        /// Erzeugt eine Kommunikationseinheit.
        /// </summary>
        /// <param name="adaptor">Verbindung zur aktuellen Anwendung.</param>
        /// <param name="path">Volller Pfad einer VCR.NET Aufzeichnungsdatei.</param>
        /// <param name="name">Name der Teilaufzeichnung.</param>
        /// <param name="paths">Alle Dateien zur Teilaufzeichnung.</param>
        public FileConnector( VCRAdaptor adaptor, string path, string name, string[] paths )
            : base( adaptor )
        {
            // Remember
            m_Name = string.IsNullOrEmpty( name ) ? Path.GetFileNameWithoutExtension( path ) : name;
            m_OriginalName = m_Name;
            m_Paths = paths;
            m_Path = path;

            // Load dynamic data
            var interval = ConfigurationManager.AppSettings["RestartInterval"];
            if (!string.IsNullOrEmpty( interval ))
                RestartInterval = int.Parse( interval );
            var trigger = ConfigurationManager.AppSettings["RestartTrigger"];
            if (!string.IsNullOrEmpty( trigger ))
                RestartTrigger = long.Parse( trigger ) * -10000000;
            var ahead = ConfigurationManager.AppSettings["MaximumAheadTime"];
            if (!string.IsNullOrEmpty( ahead ))
                MaximumAheadTime = long.Parse( ahead ) * 10000000;
            var chunk = ConfigurationManager.AppSettings["ChunkSize"];
            if (!string.IsNullOrEmpty( chunk ))
                ChunkSize = int.Parse( chunk );
        }

        /// <summary>
        /// Ermittelt die Liste der verfügbaren Aufzeichnungen.
        /// </summary>
        public override void LoadStations()
        {
            // Nothing to do
            if (m_Paths.Length < 2)
                return;

            // Forbid restriction on channels
            Favorites.DisableFavorites();

            // Create the format
            var format = new string( '0', m_Paths.Length.ToString().Length );

            // Load it
            for (var i = 0; i < m_Paths.Length; ++i)
            {
                // Remember
                Favorites.AddChannel( string.Format( Properties.Resources.PartialRecording, (1 + i).ToString( format ) ), m_Paths[i] );
            }

            // Finish
            Favorites.FillChannelList();
        }

        /// <summary>
        /// NVOD Dienste werden auf diesem Wege grundsätzlich nicht angeboten.
        /// </summary>
        public override ServiceItem[] Services { get { return new ServiceItem[0]; } }

        /// <summary>
        /// Ein NVOD Dienst kann auf diesem Wege nicht ausgewählt werden.
        /// </summary>
        /// <param name="service">Name des gewünschten NVOD Dienstes.</param>
        /// <returns>Wirft immer eine <see cref="NotSupportedException"/>.</returns>
        public override string SetService( ServiceItem service )
        {
            // None 
            return null;
        }

        /// <summary>
        /// Wählt eine Aufzeichnung aus.
        /// </summary>
        /// <param name="context">Die gewünschte Aufzeichnung.</param>
        /// <returns>Aktuelle Aufzeichnung samt aktiver Tonspur oder <i>null</i>.</returns>
        public override string SetStation( object context )
        {
            // Check mode
            var path = context as string;
            if (!string.IsNullOrEmpty( path ))
            {
                // Stop playing
                Accessor.Stop();

                // Reset all
                m_Name = Path.GetFileNameWithoutExtension( path );
                m_LastPositionChange = DateTime.MinValue;
                m_LastStart = DateTime.MinValue;
                m_NextPosition = null;
                m_Waiting = false;
                m_Length = null;
                m_Position = 0;
                m_Move = null;
                m_Path = path;

                // Restart
                StartRecording();
            }
            else
            {
                // Start streaming
                Adaptor.RestartAudio( false );
            }

            // None 
            return null;
        }

        /// <summary>
        /// Meldet den Namen der aktiven Aufzeichnung.
        /// </summary>
        public override string StationName { get { return m_Name; } }

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        protected override void OnDispose()
        {
            // Protect
            m_Disposed = true;
        }

        /// <summary>
        /// Meldet, dass bei Verbindung an eine Aufzeichnung Sender und Tonspur nicht
        /// in den Einstellungen zu vermerken sind.
        /// </summary>
        public override bool UpdateSettings { get { return false; } }

        /// <summary>
        /// Wird gemeldet, sobald neue Daten bereit stehen.
        /// </summary>
        /// <param name="bytes">Die aktuelle Größe der Datei.</param>
        private void DataAvailable( long bytes )
        {
            // Be safe
            try
            {
                // Update
                m_Length = bytes;

                // Adjust
                m_Position += ChunkSize;

                // Correct
                if (m_Position > m_Length.Value)
                    m_Position = m_Length.Value;
            }
            finally
            {
                // Did it
                m_Waiting = false;
            }
        }

        /// <summary>
        /// Wird aktiviert, wenn das Zugriffsmodul Daten anfordert.
        /// </summary>
        public override void OnWaitData()
        {
            // Already done
            if (m_Disposed)
                return;

            // Still calling?
            if (m_Waiting)
                return;

            // Test for move request
            if (m_Move.HasValue)
            {
                // Take it
                if (m_Length.HasValue)
                    m_Position = (long) (m_Move.Value * m_Length.Value);

                // Forget
                m_Move = null;
            }
            else
            {
                // See how internal timing is currently set
                var offset = Accessor.StreamTimeOffset;
                if (offset.HasValue)
                    if (offset.Value < RestartTrigger)
                    {
                        // Try restart
                        if (!RestartRecording())
                            return;
                    }
                    else if (offset.Value >= MaximumAheadTime)
                    {
                        // Do not feed any more data - for now
                        return;
                    }
            }

            // Split off
            var parts = Adaptor.Target.Split( ':' );

            // Request will start right now
            m_Waiting = true;

            // Force server to send data
            VCRNETRestProxy.RequestFilePart( Adaptor.EndPoint, m_Path, m_Position, ChunkSize, parts[0], ushort.Parse( parts[1] ), DataAvailable, null );
        }

        private bool IsNextPositionValid
        {
            get
            {
                // Not at al
                if (!m_NextPosition.HasValue)
                    return false;

                // Get delta
                var delta = DateTime.UtcNow - m_LastPositionChange;

                // Attach to site
                var info = Adaptor.Parent as IGeneralInfo;

                // Respect OSD display time
                if (info == null)
                    return (delta.TotalSeconds < 5);

                // Only for some seconds
                return (delta.TotalSeconds < (5 + info.OSDLifeTime));
            }
        }

        private bool RestartRecording()
        {
            // Only once per two seconds
            var delta = DateTime.UtcNow - m_LastStart;
            if (delta.TotalSeconds >= RestartInterval)
                try
                {
                    // Lock out
                    m_LastStart = DateTime.MaxValue;

                    // Process
                    Adaptor.Parent.Invoke( new StartRecordingCallback( StartRecording ) );

                    // Did it
                    return true;
                }
                catch
                {
                    // Ignore any error
                }

            // Skipped
            return false;
        }

        /// <summary>
        /// Wird verwendet, um die Position in der Datei zu verändern.
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
                // Force update on next request
                m_Move = m_NextPosition;

                // Forget it
                m_NextPosition = null;
            }

            // Remember
            m_LastStart = DateTime.UtcNow;

            // Forward
            Adaptor.RestartAudio( true );
        }

        /// <summary>
        /// Trägt zusätzliche Konfigurationeinstellung ein.
        /// </summary>
        public override void FillOptions()
        {
            // Forward
            base.FillOptions();

            // Configure general replay keys
            FileAdaptor.RegisterReplayKeys( Adaptor.Parent, this );

            // Configure private keys
            Adaptor.Parent.SetKeyHandler( Keys.Multiply, () => Adaptor.StartWatch( m_OriginalName ) );
        }

        /// <summary>
        /// Das Geräteprofil wurde gewechselt.
        /// </summary>
        public override void OnProfileChanged()
        {
            // Forward to base
            base.OnProfileChanged();

            // Restart - try LIVE
            Adaptor.StartLIVE();
        }

        /// <summary>
        /// Eine Senderauswahl wird nicht unterstützt.
        /// </summary>
        public override Keys? StationListKey { get { return (m_Paths.Length < 2) ? null : base.StationListKey; } }

        /// <summary>
        /// Die Auswahl von Diensten wird nicht unterstützt.
        /// </summary>
        public override Keys? ServiceListKey { get { return null; } }

        /// <summary>
        /// Der Menüpunkt für die Aufzeichnung wird für die Auswahl einer Position in der
        /// Datei mißbraucht.
        /// </summary>
        public override Keys? RecordingKey { get { return Keys.Add; } }

        /// <summary>
        /// Der Menüpunkt für die Aufzeichnung wird für die Auswahl einer Position in der
        /// Datei mißbraucht.
        /// </summary>
        public override string RecordingText { get { return Properties.Resources.Context_FilePosition; } }

        /// <summary>
        /// Meldet die Taste zum Umschalten in den TimeShift Modus.
        /// </summary>
        public override Keys? TimeShiftKey { get { return Keys.Multiply; } }

        /// <summary>
        /// Meldet, ob beim Ändern der Direct Show Filter ein Neustart des
        /// Graphen unterstützt wird.
        /// </summary>
        public override bool CanRestartGraph { get { return false; } }

        #region IFileReplay Members

        void IFileReplay.MovePosition( double delta )
        {
            // Not possible
            if (!m_Length.HasValue)
                return;

            // Load once
            if ((delta == 0) || !IsNextPositionValid)
                if (m_Length.Value < 1)
                    m_NextPosition = 1.0;
                else
                    m_NextPosition = m_Position * 1.0 / m_Length.Value;

            // Move it
            Position = m_NextPosition.Value + delta;
        }

        double IFileReplay.Position { set { Position = value; } }

        private double Position
        {
            set
            {
                // Move it and remember time stamp
                m_NextPosition = Math.Max( 0, Math.Min( 1, value ) );
                m_LastPositionChange = DateTime.UtcNow;

                // Show
                Adaptor.Parent.ShowPositionInFile( m_NextPosition.Value );
            }
        }

        #endregion
    }
}
