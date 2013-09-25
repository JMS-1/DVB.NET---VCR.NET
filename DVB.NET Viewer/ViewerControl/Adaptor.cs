using System;
using System.Text;
using System.Windows.Forms;
using JMS.DVB.DirectShow;
using JMS.DVB.Favorites;
using JMS.DVB.TS.VideoText;


namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Diese Basisklasse abstrahiert die Quelle eines Transport Streams.
    /// </summary>
    public abstract class Adaptor : IDisposable
    {
        /// <summary>
        /// Zugehörige Anwendung.
        /// </summary>
        public readonly IViewerSite Parent;

        /// <summary>
        /// Gesetzt, während die gespeicherten Vorschlagswerte für Sender und
        /// Tonspur geladen werden.
        /// </summary>
        private bool m_LoadingDefaults = false;

        /// <summary>
        /// Gesetzt, wenn beim nächsten Aufruf der Programmzeitschrift der aktuelle
        /// Eintrag angezeigt werden soll.
        /// </summary>
        private bool m_ShowCurrentEntry = true;

        /// <summary>
        /// Aktuelle EPG Informationen.
        /// </summary>
        private EPG.EventEntry m_CurrentEntry = null;

        /// <summary>
        /// EPG Informationen zur nächsten Sendung.
        /// </summary>
        private EPG.EventEntry m_NextEntry = null;

        /// <summary>
        /// Synchronisiert den Zugriff auf die EPG Informationen.
        /// </summary>
        private object m_EITLock = new object();

        /// <summary>
        /// Das Zugriffmodul, über das der Transport Stream in die Anwendung gelenkt wird.
        /// </summary>
        private AccessModule m_Accessor;

        /// <summary>
        /// Verwaltet den Videotext.
        /// </summary>
        private PageManager m_TTX = new PageManager();

        /// <summary>
        /// Initialisiert eine Verwaltungsinstanz.
        /// </summary>
        /// <param name="main">Zugehörige Anwendung.</param>
        protected Adaptor( IViewerSite main )
        {
            // Remember
            Parent = main;
        }

        /// <summary>
        /// Der Sender, der beim vorherigen Beenden der Anwendung aktiv war.
        /// </summary>
        protected abstract string DefaultStation { get; }

        /// <summary>
        /// Die Tonspur, die beim vorherigen Beenden der Anwendungs aktiv war.
        /// </summary>
        protected abstract string DefaultAudio { get; }

        /// <summary>
        /// Anzeigename des aktuellen Senders - bei Verwendung von NVOD Diensten
        /// handelt es sich um den Namen des Portals.
        /// </summary>
        protected abstract string StationName { get; }

        /// <summary>
        /// Wird beim Beenden der zugehörigen Anwendung zur Freigabe aller Ressourcen aufgerufen.
        /// </summary>
        protected abstract void OnDispose();

        /// <summary>
        /// Startet eine neue Aufzeichnung.
        /// </summary>
        public abstract void StartRecording();

        /// <summary>
        /// Neuen NVOD Dienst auswählen.
        /// </summary>
        /// <param name="service"></param>
        /// <returns>Anzeigename von Dienst und aktiver Tonspue oder <i>null</i>.</returns>
        public abstract string SetService( ServiceItem service );

        /// <summary>
        /// Meldet die Anzahl der aufgezeichneten Bytes.
        /// </summary>
        public virtual long RecordedBytes { get { return 0; } }

        /// <summary>
        /// Meldet, ob gerade eine Aufzeichnung läuft.
        /// </summary>
        public virtual bool IsRecording { get { return false; } }

        /// <summary>
        /// Neuen Sender auswählen - Tonspur und NVOD Dienst werden auf die
        /// Voreinstellung zurück gesetzt.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Anzeigename von Sender und aktiver Tonspur oder <i>null</i>.</returns>
        public abstract string SetStation( object context );

        /// <summary>
        /// Alle zum aktuellen Portal verfügbaren NVOD Dienste melden.
        /// </summary>
        public abstract ServiceItem[] Services { get; }

        /// <summary>
        /// Neue Tonspur auswählen.
        /// </summary>
        /// <param name="audio">Die gewünschte Tonspur.</param>
        /// <returns>Anzeigename von Sender und aktiver Tonspur oder <i>null</i>.</returns>
        public abstract string SetAudio( string audio );

        /// <summary>
        /// Lädt die Senderliste.
        /// </summary>
        public abstract void LoadStations();

        /// <summary>
        /// Lädt die Liste der Tonspuren.
        /// </summary>
        public virtual void LoadTracks()
        {
        }

        /// <summary>
        /// Periodischer Aufruf der Anwendung, der zur Prüfung der Funktionsfähigkeit der
        /// Transport Stream Quelle genutzt werden kann.
        /// </summary>
        /// <param name="fine">Gesetzt für den Aufruf im Sekundenrythmus.</param>
        public virtual void KeepAlive( bool fine )
        {
        }

        /// <summary>
        /// Ergänzt optional zusätzliche Konfigurationsmöglichkeiten.
        /// </summary>
        /// <remarks>
        /// Jeder Eintrag ist von der Art <see cref="OptionDisplay"/>.
        /// </remarks>
        public virtual void FillOptions()
        {
        }

        /// <summary>
        /// Meldet, ob beim Ändern der Direct Show Filter ein Neustart des
        /// Graphen unterstützt wird.
        /// </summary>
        public virtual bool CanRestartGraph { get { return true; } }

        /// <summary>
        /// Lädt beim Starten der Anwendung den zuletzt verwendeten Sender und die
        /// zugehörige Tonspur.
        /// </summary>
        /// <param name="applicationStart">Gesetzt, wenn die Anwendung gerade startet.</param>
        /// <returns>Aktueller Sender mit aktiver tonspur oder <i>null</i>.</returns>
        public virtual string LoadDefaults( bool applicationStart )
        {
            // Load
            var station = DefaultStation;
            var audio = DefaultAudio;

            // Validate
            if (string.IsNullOrEmpty( station ))
                return null;

            // Starting to load defauls
            m_LoadingDefaults = true;
            try
            {
                // Select and forward
                if (Favorites.SelectChannel( station ))
                    return SetAudio( audio );
            }
            finally
            {
                // Reset
                m_LoadingDefaults = false;
            }

            // Nothing
            return null;
        }

        /// <summary>
        /// Meldet, ob die aktuelle Veränderung an Sender und Tonspur zum Laden der
        /// Vorgabewerte dient.
        /// </summary>
        public bool LoadingDefaults { get { return m_LoadingDefaults; } }

        /// <summary>
        /// Verbindet ein Zugriffsmodul mit dieser Verwaltungsinstanz.
        /// </summary>
        /// <param name="accessor">Ein Transport Stream Zugriffsmodul.</param>
        protected void SetAccessor( AccessModule accessor )
        {
            // Remember
            m_Accessor = accessor;
        }

        /// <summary>
        /// Meldet das aktuelle Zugriffsmodul.
        /// </summary>
        public AccessModule Accessor { get { return m_Accessor; } }

        /// <summary>
        /// Zeigt eine Meldung im OSD an.
        /// </summary>
        /// <param name="message">Die Meldung.</param>
        /// <param name="headline">Die Überschrift zur Meldung.</param>
        /// <param name="realOSD">Gesetzt, wenn das echte OSD verwendet werden soll.</param>
        public void ShowMessage( string message, string headline, bool realOSD )
        {
            // Forward
            Parent.ShowMessage( message, headline, realOSD );
        }

        /// <summary>
        /// Meldet die ktuelle Senderverwaltung.
        /// </summary>
        public ChannelSelector Favorites { get { return Parent.FavoriteManager; } }

        /// <summary>
        /// Prüft, ob die EPG Informationen zum aktuellen Sender und der laufenden Sendung
        /// gehören und vermerkt diese dann.
        /// </summary>
        /// <param name="section">Die EPG Informationen.</param>
        /// <param name="current">Kennung des aktuellen Senders.</param>
        protected void ProcessEPG( EPG.Section section, SourceIdentifier current )
        {
            // Test station
            if (current == null)
                return;

            // Test section
            if (section == null)
                return;
            if (!section.IsValid)
                return;

            // Test table
            var eit = section.Table as EPG.Tables.EIT;
            if (eit == null)
                return;
            if (!eit.IsValid)
                return;

            // Test identification
            if (current.Service != eit.ServiceIdentifier)
                return;
            if (current.Network != eit.OriginalNetworkIdentifier)
                return;
            if (current.TransportStream != eit.TransportStreamIdentifier)
                return;

            // Check mode
            bool gotCurrent = false, gotNext = false;

            // Test state
            foreach (var entry in eit.Entries)
            {
                // Check for current entry
                if (!gotCurrent)
                    if (entry.Status == EPG.EventStatus.Running)
                    {
                        // Remember
                        CurrentEntry = entry;
                        gotCurrent = true;

                        // Done
                        if (gotNext)
                            break;

                        // Next
                        continue;
                    }

                // Check for next entry
                if (!gotNext)
                    if (entry.Status == EPG.EventStatus.NotRunning)
                    {
                        // Remember
                        NextEntry = entry;
                        gotNext = true;

                        // Done
                        if (gotCurrent)
                            break;
                    }
            }
        }

        /// <summary>
        /// Zeigt die aktuellen EPG Informationen an.
        /// </summary>
        public void ShowEPG()
        {
            // Try the active entry
            var entry = m_ShowCurrentEntry ? CurrentEntry : NextEntry;
            if (entry == null)
            {
                // Switch mode
                m_ShowCurrentEntry = !m_ShowCurrentEntry;

                // Try the alternate entry
                entry = m_ShowCurrentEntry ? CurrentEntry : NextEntry;
                if (entry == null)
                    return;
            }

            // Switch mode for next call
            m_ShowCurrentEntry = !m_ShowCurrentEntry;

            // Get data
            var start = entry.StartTime.ToLocalTime();
            var end = (entry.StartTime + entry.Duration).ToLocalTime();

            // Full text
            var fulltext = new StringBuilder();

            // Title
            string name = string.Format( "#{0}", entry.EventIdentifier ), shortText = null;
            foreach (var descriptor in entry.Descriptors)
            {
                // Short
                var test = descriptor as EPG.Descriptors.ShortEvent;
                if (test != null)
                {
                    // Replace
                    if (!string.IsNullOrEmpty( test.Name ))
                        name = test.Name;
                    if (!string.IsNullOrEmpty( test.Language ))
                        name = string.Format( "{0} ({1})", name, test.Language );

                    // Remember
                    shortText = test.Text;

                    // Next
                    continue;
                }

                // Long
                var descr = descriptor as EPG.Descriptors.ExtendedEvent;
                if (descr != null)
                    fulltext.Append( descr.Text );
            }

            // Create headline
            var head = string.Format( "{3}{0:HH:mm}-{1:HH:mm} {2}", start, end, name, m_ShowCurrentEntry ? "* " : string.Empty );

            // Get the scratch selection list
            var scratch = Parent.ScratchComboBox;

            // Clear it
            scratch.Items.Clear();

            // Show episode and separate
            if (SendEPGToList( shortText, scratch ))
                if (fulltext.Length > 0)
                    scratch.Items.Add( " " );

            // Show description
            SendEPGToList( fulltext.ToString(), scratch );

            // Show
            Parent.ShowList( head, 0, OSDShowMode.ProgramGuide );
        }

        /// <summary>
        /// Zerlegt eine Zeichenkette in Worte und bildet diese dann auf eine Liste ab.
        /// </summary>
        /// <param name="data">Die zu zerlegenden Daten.</param>
        /// <param name="list">Die Liste, die tatsächlich angezeigt wird.</param>
        private bool SendEPGToList( string data, ComboBox list )
        {
            // Nothing to do
            if (string.IsNullOrEmpty( data ))
                return false;

            // Check size
            var count = list.Items.Count;

            // Remove CRLF
            data = data.Replace( "\r", string.Empty );

            // Split in lines
            foreach (var line in data.Split( '\n' ))
                if (string.IsNullOrEmpty( line ))
                    list.Items.Add( " " );
                else
                    for (var pos = 0; pos < line.Length; )
                    {
                        // Next position
                        var next = Math.Min( pos + 65, line.Length );

                        // Find separator backward if the rest doesn't fit
                        var sep = (next < line.Length) ? line.LastIndexOf( ' ', next - 1, next ) : next;

                        // Not in range
                        if (sep < pos)
                        {
                            // Find separator forward
                            sep = line.IndexOf( ' ', next );

                            // All of it
                            if (sep < 0)
                                sep = line.Length;
                        }

                        // Get part
                        if (sep > pos)
                            list.Items.Add( line.Substring( pos, sep - pos ) );

                        // Next to process
                        pos = sep + 1;
                    }

            // At least added one
            return (list.Items.Count > 0);
        }

        /// <summary>
        /// Liest oder setzt den aktuellen EPG Eintrag.
        /// </summary>
        public virtual EPG.EventEntry CurrentEntry
        {
            get
            {
                // Report locked
                lock (m_EITLock)
                    return m_CurrentEntry;
            }
            set
            {
                // Change locked
                lock (m_EITLock)
                    m_CurrentEntry = value;
            }
        }

        /// <summary>
        /// Beim nächsten Aufruf der Programmzeitschrift wird der aktuelle Eintrag angezeigt.
        /// </summary>
        public void ShowCurrentEntry()
        {
            // Set flag
            m_ShowCurrentEntry = true;
        }

        /// <summary>
        /// Liest oder setzt den EPG Eintrag für die nächste Sendung.
        /// </summary>
        public virtual EPG.EventEntry NextEntry
        {
            get
            {
                // Report locked
                lock (m_EITLock)
                    return m_NextEntry;
            }
            set
            {
                // Change locked
                lock (m_EITLock)
                    m_NextEntry = value;
            }
        }

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
        /// Meldet die Verwaltungsinstanz der Videotext Seiten.
        /// </summary>
        public PageManager VideoText { get { return m_TTX; } }

        /// <summary>
        /// Meldet, ob ein Videotext Signal vorliegt.
        /// </summary>
        public abstract bool TTXAvailable { get; }

        /// <summary>
        /// Übermittelt Videotext Daten zur Analyse.
        /// </summary>
        /// <param name="isStart">Gesetzt, wenn der PES Kopf enthalten ist.</param>
        /// <param name="buffer">Speicher für die Videotext Daten.</param>
        /// <param name="offset">Position des ersten Bytes der Videotext Daten.</param>
        /// <param name="length">Anzahl der Bytes an Videotext Daten.</param>
        /// <param name="pts">Aktueller Zeitstempel.</param>
        protected void AnalyseVideoText( bool isStart, byte[] buffer, int offset, int length, long pts )
        {
            // Forward
            m_TTX.AddPayload( isStart, buffer, offset, length, pts );
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Verwaltungsinstanz endgültig und gibt alle
        /// verbundenen Ressourcen frei.
        /// </summary>
        public void Dispose()
        {
            // Cleanup self
            using (m_Accessor)
                m_Accessor = null;

            // Forward
            OnDispose();
        }

        #endregion
    }
}
