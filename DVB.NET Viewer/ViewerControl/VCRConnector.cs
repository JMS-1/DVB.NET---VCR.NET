using System;
using System.Windows.Forms;
using JMS.DVB.DirectShow.AccessModules;
using JMS.DVB.Favorites;


namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Abstrahiert die Verbindung zu einem VCR.NET Server.
    /// </summary>
    public abstract class VCRConnector : IDisposable
    {
        /// <summary>
        /// Die Verbindung zur aktuellen Anwendung.
        /// </summary>
        public readonly VCRAdaptor Adaptor;

        /// <summary>
        /// Initialisiert die Instanz.
        /// </summary>
        /// <param name="adaptor">Verbindung zur aktuellen Anwendung.</param>
        protected VCRConnector( VCRAdaptor adaptor )
        {
            // Remember
            Adaptor = adaptor;
        }

        /// <summary>
        /// Wählte einen NVOD Dienst.
        /// </summary>
        /// <param name="service">Name des NVO Dienstes.</param>
        /// <returns>Name des Dienstes und der aktuellen Tonspur oder <i>null</i>.</returns>
        public abstract string SetService( ServiceItem service );

        /// <summary>
        /// Verändert den angezeigten Sender.
        /// </summary>
        /// <param name="context">Neuer Sender.</param>
        /// <returns>Name des neuen Senders und der aktiven Tonspur oder <i>null</i>.</returns>
        public abstract string SetStation( object context );

        /// <summary>
        /// Liste der auf dem aktuellen Portal verfügbaren NVOD Dienste.
        /// </summary>
        public abstract ServiceItem[] Services { get; }

        /// <summary>
        /// Meldet den aktuellen Sendernamen.
        /// </summary>
        public abstract string StationName { get; }

        /// <summary>
        /// Lädt die Liste der verfügbaren Sender.
        /// </summary>
        public abstract void LoadStations();

        /// <summary>
        /// Meldet, ob Veränderungen ans Sender oder Tonspur in den Einstellungen vermerkt werden sollen.
        /// </summary>
        public abstract bool UpdateSettings { get; }

        /// <summary>
        /// Wird aktiviert, wenn Daten vom Zugriffsmodul angefordert werden.
        /// </summary>
        public virtual void OnWaitData()
        {
            // No longer needed
            Adaptor.DisconnectWaiter();
        }

        /// <summary>
        /// Wird aktiviert, bevor das VCR.NET Geräteprofil gewechselt wird.
        /// </summary>
        public virtual void OnProfileChanging()
        {
        }

        /// <summary>
        /// Wird aktiviert, nachdem das VCR.NET Geräteprofil verändert wurde.
        /// </summary>
        public virtual void OnProfileChanged()
        {
        }

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        protected abstract void OnDispose();

        /// <summary>
        /// Prüft, ob die aktuelle Verbindung zum VCR.NET Recording Service
        /// aufrecht gehalten werden kann.
        /// </summary>
        public virtual void KeepAlive()
        {
        }

        /// <summary>
        /// Fügt zusätzliche Optionen in die Liste der Konfigurationsoptionen
        /// ein.
        /// </summary>
        public virtual void FillOptions()
        {
        }

        /// <summary>
        /// Beginnt oder beendet eine Aufzeichung.
        /// </summary>
        public virtual void StartRecording()
        {
        }

        /// <summary>
        /// Meldet die aktuelle Senderverwaltung.
        /// </summary>
        protected ChannelSelector Favorites { get { return Adaptor.Favorites; } }

        /// <summary>
        /// Meldet die aktuelle Verbindung zum eingehenden Transport Stream.
        /// </summary>
        protected TransportStreamReceiver Accessor { get { return (TransportStreamReceiver) Adaptor.Accessor; } }

        /// <summary>
        /// Meldet, ob ein Videotext Signal vorliegt.
        /// </summary>
        public bool TTXAvailable { get { return (Accessor != null) && Accessor.TTXAvailable; } }

        /// <summary>
        /// Liefert das gerade verwendete VCR.NET Geräteprofil.
        /// </summary>
        protected string Profile { get { return Adaptor.Profile; } }

        /// <summary>
        /// Zeigt eine Nachricht im OSD an.
        /// </summary>
        /// <param name="message">Der Text der Nachricht.</param>
        /// <param name="headline">Die Überschrift zur Meldung.</param>
        /// <param name="realOSD">Gesetzt, wenn das echte OSD verwendet werden soll.</param>
        public void ShowMessage( string message, string headline, bool realOSD )
        {
            // Forward
            Adaptor.ShowMessage( message, headline, realOSD );
        }

        /// <summary>
        /// Zeigt eine formatierte Nachricht im OSD an.
        /// </summary>
        /// <param name="format">Format für die Nachricht.</param>
        /// <param name="headline">Die Überschrift zur Meldung.</param>
        /// <param name="realOSD">Gesetzt, wenn das echte OSD verwendet werden soll.</param>
        /// <param name="args">Parameter zur Erzeugung der Nachricht aus dem Format.</param>
        public void ShowMessage( string format, string headline, bool realOSD, params object[] args )
        {
            // Forward
            ShowMessage( string.Format( format, args ), headline, realOSD );
        }

        /// <summary>
        /// Meldet den Sender, der beim Starten der Anwendung ausgewählt werden soll.
        /// </summary>
        public virtual string DefaultStation { get { return Adaptor.RemoteInfo.VCRStation; } }

        /// <summary>
        /// Meldet die Tonspur, die beim Starten der Anwendung ausgewählt werden soll.
        /// </summary>
        public virtual string DefaultAudio { get { return Adaptor.RemoteInfo.VCRAudio; } }

        /// <summary>
        /// Meldete die Taste, mit der die Senderliste angezeigt wird.
        /// </summary>
        public virtual Keys? StationListKey { get { return Keys.K; } }

        /// <summary>
        /// Meldete die Taste, mit der die Liste der Tonspuren angezeigt wird.
        /// </summary>
        public virtual Keys? TrackListKey { get { return Keys.L; } }

        /// <summary>
        /// Meldete die Taste, mit der die Liste der Dienste angezeigt wird.
        /// </summary>
        public virtual Keys? ServiceListKey { get { return Keys.M; } }

        /// <summary>
        /// Meldete die Taste, mit der eine Aufzeichnung gestartet oder beendet wird.
        /// </summary>
        public virtual Keys? RecordingKey { get { return (Keys) 191; } }

        /// <summary>
        /// Meldet den Text für die Aktion zum Starten und Beenden einer Aufzeichnung.
        /// </summary>
        public virtual string RecordingText { get { return Properties.Resources.Context_Record; } }

        /// <summary>
        /// Taste zum an- und abschalten des TimeShift Modus.
        /// </summary>
        public virtual Keys? TimeShiftKey { get { return null; } }

        /// <summary>
        /// Meldet, ob beim Ändern der Direct Show Filter ein Neustart des
        /// Graphen unterstützt wird.
        /// </summary>
        public virtual bool CanRestartGraph { get { return true; } }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// <seealso cref="OnDispose"/>
        /// </summary>
        public void Dispose()
        {
            // Forward
            OnDispose();
        }

        #endregion
    }
}
