using System;
using System.Windows.Forms;


namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Verbindet die Anwendung mit einem TCP/IP UDP Strom, der einen Transport Stream
    /// sendet.
    /// </summary>
    public class SlaveAdaptor : UDPAdaptor
    {
        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz.
        /// </summary>
        /// <param name="main">Die zugehörige Anwendung.</param>
        /// <param name="endpoint">Eine TCP/IP Adresse, an der ein
        /// Transport Stream entgegen genommen werden kann.</param>
        public SlaveAdaptor( IViewerSite main, string endpoint )
            : base( main )
        {
            // Split
            string[] parts = endpoint.Split( ':' );

            // Connect to stream
            Connect( parts[0], ushort.Parse( parts[1] ), null );

            // No control needed
            DisconnectWaiter();
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
        /// Wählt einen anderen Sender.
        /// </summary>
        /// <param name="context">Senderbeschreibung abhängig vom Zugriffsmodul.</param>
        /// <returns>Sendername mit ausgewählter Tonspur oder <i>null</i>.</returns>
        public override string SetStation( object context )
        {
            // Do nothing
            return null;
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
            return SetAudio( audio, false );
        }

        /// <summary>
        /// Meldet den Sender, der beim Starten der Anwendung ausgewählt werden soll.
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
        /// Meldet die Tonspur, die beim Starten der Anwendung ausgewählt werden soll.
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
        /// Meldet alle auf dem aktuellen Portal verfügbaren NVOD Dienste.
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
        /// Wählte einen NVOD Dienst des aktuellen Portals zur Anzeige.
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
                return Properties.Resources.PlayStream;
            }
        }

        /// <summary>
        /// Beendet die Nutzung dieser Instanz.
        /// </summary>
        protected override void OnDispose()
        {
        }

        /// <summary>
        /// Restart the stream completly.
        /// </summary>
        public override void StartRecording()
        {
            // Stop sending data
            Accessor.Stop();

            // Stop displaying data
            Accessor.StopGraph();

            // Restart videotext caching from scratch
            VideoText.Deactivate( true );

            // Start again
            RestartAudio( true );
        }

        /// <summary>
        /// Eine Senderauswahl wird nicht unterstützt.
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
        /// Die Auswahl von Diensten wird nicht unterstützt.
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
        /// Die Aufzeichnungstaste wird zum Neuaufbau der Graphen wiederverwendet, hier
        /// wird ein entsprechender Text für das Kontextmenü gemeldet.
        /// </summary>
        public override string RecordingText
        {
            get
            {
                // Report
                return Properties.Resources.Context_Refresh;
            }
        }
    }
}
