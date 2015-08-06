using System;
using System.Linq;
using System.Drawing;
using System.Security;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using JMS.DVB.EPG;
using JMS.DVB.Favorites;
using JMS.DVB.DirectShow;
using JMS.DVB.TS.VideoText;
using JMS.DVB.DirectShow.UI;
using JMS.DVB.DirectShow.RawDevices;


namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Dieses .NET <see cref="Control"/> ist die Basis des DVB.NET / VCR.NET
    /// Viewers. Es selbst oder Varianten können in verschiedenen Umgebungen
    /// eingesetzt werden.
    /// </summary>
    public partial class ViewerControl : UserControl, IOSDSite, IViewerSite, IChannelInfo, IRemoteInfo, ILocalInfo, IStreamInfo, IGeneralInfo
    {
        /// <summary>
        /// Hilfsinstanz zur Anzeige einer Überblendung.
        /// </summary>
        private class _OSDText : IDisposable
        {
            /// <summary>
            /// Die Konstruktion des Inhaltes.
            /// </summary>
            public OSDText Builder { get; private set; }

            /// <summary>
            /// Die zugehörige Anzeigeeinheit.
            /// </summary>
            private ViewerControl m_Viewer;

            /// <summary>
            /// Die Art des Inhalts der angezeigten Daten.
            /// </summary>
            private OSDShowMode m_Mode;

            /// <summary>
            /// Erzeugt eine neue Hilfsinstanz zur Anzeige einer Überblendung.
            /// </summary>
            /// <param name="viewer">Die zugehörige Anzeigeeinheit.</param>
            /// <param name="builder">Die Konstruktionseinheit.</param>
            /// <param name="mode">Die Art des Inhalts.</param>
            public _OSDText( ViewerControl viewer, OSDText builder, OSDShowMode mode )
            {
                // Remember
                Builder = builder;
                m_Viewer = viewer;
                m_Mode = mode;
            }

            #region IDisposable Members

            /// <summary>
            /// Beendet die Nutzung dieser Instanz und zeigt die aktuelle Zusammenstellung an.
            /// </summary>
            public void Dispose()
            {
                // Nothing to do
                if (Builder == null)
                    return;

                // Forward
                using (Builder)
                    Builder = null;

                // Update
                m_Viewer.OSDShown( m_Mode );
            }

            #endregion
        }

        /// <summary>
        /// Zeigt an, dass kein OSD aktiv ist.
        /// </summary>
        private static readonly DateTime OSDOffTime = DateTime.MaxValue;

        /// <summary>
        /// Zeigt an, dass der Videotext aktiv ist.
        /// </summary>
        private static readonly DateTime TTXOnTime = DateTime.MaxValue.AddSeconds( -1 );

        /// <summary>
        /// Prüft, ob eine bestimmte Taste gedrückt ist.
        /// </summary>
        /// <param name="virtualKey">Zu prüfende Taste.</param>
        /// <returns>Ergebnis der Prüfung.</returns>
        [DllImport( "user32.dll" )]
        [SuppressUnmanagedCodeSecurity]
        private static extern short GetAsyncKeyState( int virtualKey );

        /// <summary>
        /// Die Senderverwaltung mit Favoritenfähigkeit.
        /// </summary>
        private ChannelSelector m_FavoriteManager;

        /// <summary>
        /// Zeitpunkt, an dem das aktuelle OSD deaktiviert werden soll.
        /// </summary>
        private DateTime m_OSDOff = OSDOffTime;

        /// <summary>
        /// Aktuelle Auswahlliste im OSD.
        /// </summary>
        private ComboBox m_CurrentList = null;

        /// <summary>
        /// Überschrift der aktuellen Auswahlliste im OSD.
        /// </summary>
        private string m_CurrentHead = null;

        /// <summary>
        /// Verwaltung des Zugriffs auf den DVB Transport Stream.
        /// </summary>
        private Adaptor m_CurrentAdaptor;

        /// <summary>
        /// Gesetzt, wenn die aktuelle Auswahl bestätigt wurde.
        /// </summary>
        private bool m_CanSelect = false;

        /// <summary>
        /// Überwachung des eingehenden Transport Stream zur Erkennung eines
        /// fehlenden Signals.
        /// </summary>
        private long m_LastBytes = 0;

        /// <summary>
        /// Verwaltet die Einstellungen zur Senderliste.
        /// </summary>
        private IChannelInfo m_ChannelInfo = null;

        /// <summary>
        /// Verwaltet die Einstellungen für eine VCR.NET Verbindung.
        /// </summary>
        private IRemoteInfo m_RemoteInfo = null;

        /// <summary>
        /// Verwaltet die Einstellungen für die Nutzung einer lokalen DVB.NET Hardware.
        /// </summary>
        private ILocalInfo m_LocalInfo = null;

        /// <summary>
        /// Verwaltet die Einstellungen für den Netzwerkversand.
        /// </summary>
        private IStreamInfo m_StreamInfo = null;

        /// <summary>
        /// Verwaltet die allgemeinen Einstellungen.
        /// </summary>
        private IGeneralInfo m_GeneralInfo;

        /// <summary>
        /// Einstellungen der Anwendung selbst.
        /// </summary>
        private List<OptionDisplay> m_GlobalOptions = new List<OptionDisplay>();

        /// <summary>
        /// Gesetzt, wenn kein Signal vorhanden ist.
        /// </summary>
        private bool m_NoSignal = false;

        /// <summary>
        /// Saved key handlers
        /// </summary>
        private Dictionary<Keys, BDAWindow.KeyProcessor> m_Handlers;

        /// <summary>
        /// Gesetzt, wenn die letzte OSD Anzeige eine Liste war.
        /// </summary>
        private bool m_LastOSDWasList = false;

        /// <summary>
        /// Gesetzt, wenn der letzte angezeigte Balken eine Dateiposition war.
        /// </summary>
        private bool m_LastProgressWasFile = false;

        /// <summary>
        /// Aktueller Stand einer Videotext Seiteneingabe.
        /// </summary>
        private int m_TTXPageBuilder = 0;

        /// <summary>
        /// Gesetzt, wenn eine Videotextseite angezeigt wird.
        /// </summary>
        private OSDShowMode m_OSDShowMode = OSDShowMode.Nothing;

        /// <summary>
        /// Vermerkt, wann zuletzt eine Eingabe für eine Videotext Seite erfolgt ist.
        /// </summary>
        private DateTime m_TTXLastKey = DateTime.MinValue;

        /// <summary>
        /// Alle Zahlen auf der aktuellen Videotextseite.
        /// </summary>
        private DigitManager m_TTXDigits;

        /// <summary>
        /// Steuert die Anzeige von Überlagerungen.
        /// </summary>
        private OverlayWindow m_Overlay;

        /// <summary>
        /// Empfängt Befehle von einer Fernsteuerung.
        /// </summary>
        private RawInputSink m_RCReceiver;

        /// <summary>
        /// Die Konfiguration der Fenrbedienung.
        /// </summary>
        private RCSettings m_RCSettings = RCSettings.LoadOrDefault();

        /// <summary>
        /// Der Zeitpunkt, an dem letztmalig ein Befehl der Fernsteuerung empfangen wurde.
        /// </summary>
        private DateTime m_LastRC = DateTime.MinValue;

        /// <summary>
        /// Die bisher gesammelten Fernsteuercodes.
        /// </summary>
        private List<MappingItem> m_CurrentRC = new List<MappingItem>();

        /// <summary>
        /// Die zuletzt angezeigte Videotextseite.
        /// </summary>
        private TTXPage m_CurrentTTXPage;

        /// <summary>
        /// Die aktuelle Eingabe einer Videotextseite.
        /// </summary>
        private string m_PendingTTXPage;

        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        public ViewerControl()
        {
            // Create components
            InitializeComponent();

            // Create favorite manager
            m_FavoriteManager = new ChannelSelector( true );

            // Connect to events
            m_FavoriteManager.ChannelSelected += ChannelSelected;
            m_FavoriteManager.ServiceSelected += ServiceSelected;
            m_FavoriteManager.TrackSelected += TrackSelected;

            // Configure shortcuts
            LoadKeys();

            // React on the mouse wheel
            MouseWheel += MouseWheelChanged;
        }

        /// <summary>
        /// Reagiert auf die Bewegung des Mausrades.
        /// </summary>
        /// <param name="sender">Ignoriert.</param>
        /// <param name="e">Informationen zur Veränderung.</param>
        private void MouseWheelChanged( object sender, MouseEventArgs e )
        {
            // See if we should control the volume
            if (m_LastOSDWasList)
            {
                // Check for change
                if (e.Delta > 0)
                    ListUp( Keys.Up );
                else if (e.Delta < 0)
                    ListDown( Keys.Down );
            }
            else if (m_LastProgressWasFile)
            {
                // Check for change
                if (e.Delta > 0)
                    ExecuteKey( Keys.Add );
                else if (e.Delta < 0)
                    ExecuteKey( Keys.Subtract );
            }
            else
            {
                // Check for change
                if (e.Delta > 0)
                    VolumeUp( Keys.Right );
                else if (e.Delta < 0)
                    VolumeDown( Keys.Left );
            }
        }

        /// <summary>
        /// Verbindet das Control mit dem Zugriff auf die Konfiguration.
        /// </summary>
        /// <param name="settings">Ermöglicht den Zugriff auf die Einstellungen.</param>
        public void SetSite( object settings )
        {
            // Attach to all interfaces provided
            m_GeneralInfo = settings as IGeneralInfo;
            m_ChannelInfo = settings as IChannelInfo;
            m_StreamInfo = settings as IStreamInfo;
            m_RemoteInfo = settings as IRemoteInfo;
            m_LocalInfo = settings as ILocalInfo;

            // Load initial settings
            m_Handlers = new Dictionary<Keys, BDAWindow.KeyProcessor>( directShow.KeyProcessors );
        }

        /// <summary>
        /// Verbindet die Anzeige mit einem Adaptor.
        /// </summary>
        /// <param name="adaptor">Die zu verwendende Quelle.</param>
        public void Initialize( Adaptor adaptor )
        {
            // Remember
            m_CurrentAdaptor = adaptor;

            // Connect
            m_CurrentAdaptor.VideoText.OnPageAvailable += ShowTTXPage;

            // Configure graph
            directShow.UseCyberlink = GeneralInfo.UseCyberlinkCodec;
            directShow.MPEG2Decoder = GeneralInfo.MPEG2Decoder;
            directShow.H264Decoder = GeneralInfo.H264Decoder;
            directShow.MP2Decoder = GeneralInfo.MP2Decoder;
            directShow.AC3Decoder = GeneralInfo.AC3Decoder;

            // Initialize DirectShow window
            directShow.SetAccessor( m_CurrentAdaptor.Accessor );

            // Set the volume
            directShow.Volume = GeneralInfo.Volume;

            // Load picture parameters
            var parameters = directShow.PictureParameters;

            // Overwrite from settings
            if (null != parameters)
            {
                // Update
                GeneralInfo.SetPictureParameters( parameters );

                // Send to window
                directShow.PictureParameters = parameters;
            }

            // Start timer
            signalTest.Enabled = true;

            // Finish initialisation
            Restart( true );
        }

        /// <summary>
        /// Mit dem Drücken der rechten Maustaste wird das Kontextmenü geöffnet.
        /// </summary>
        /// <param name="key">Taste, die der Anwender betätigt hat.</param>
        private void RightClick( Keys key )
        {
            // Must have adaptor
            if (null == m_CurrentAdaptor) return;

            // Liste aufsetzen
            selScratch.Items.Clear();

            // Funktionen, die in jedem Operationsmodus zur Verfügung stehen
            selScratch.Items.Add( new OptionDisplay( Properties.Resources.Context_Fullscreen, () => ExecuteKey( Keys.J ) ) );
            selScratch.Items.Add( new OptionDisplay( Properties.Resources.Context_Volume, () => VolumeChange( 0 ) ) );
            selScratch.Items.Add( " " );
            selScratch.Items.Add( new OptionDisplay( Properties.Resources.Context_EPG, () => ExecuteKey( Keys.F1 ) ) );
            if (m_CurrentAdaptor.TTXAvailable) selScratch.Items.Add( new OptionDisplay( Properties.Resources.Context_VideoText, () => ExecuteKey( Keys.F2 ) ) );

            // Optionale Listen
            Keys? station = m_CurrentAdaptor.StationListKey, audio = m_CurrentAdaptor.TrackListKey, nvod = m_CurrentAdaptor.ServiceListKey, rec = m_CurrentAdaptor.RecordingKey, tshift = m_CurrentAdaptor.TimeShiftKey;
            string recText = m_CurrentAdaptor.RecordingText;

            // Alle Listen
            if (station.HasValue) selScratch.Items.Add( new OptionDisplay( Properties.Resources.Context_Channels, () => ExecuteKey( station.Value ) ) );
            if (audio.HasValue) selScratch.Items.Add( new OptionDisplay( Properties.Resources.Context_Audio, () => ExecuteKey( audio.Value ) ) );
            if (nvod.HasValue) selScratch.Items.Add( new OptionDisplay( Properties.Resources.Context_Service, () => ExecuteKey( nvod.Value ) ) );

            // Trenner
            selScratch.Items.Add( " " );

            // Aufzeichnung
            if (rec.HasValue) selScratch.Items.Add( new OptionDisplay( recText, () => ExecuteKey( rec.Value ) ) );
            if (tshift.HasValue) selScratch.Items.Add( new OptionDisplay( Properties.Resources.Context_TimeShift, () => ExecuteKey( tshift.Value ) ) );

            // Funktionen, die in jedem Operationsmodus zur Verfügung stehen
            selScratch.Items.Add( new OptionDisplay( Properties.Resources.Context_Options, () => ExecuteKey( (Keys) 219 ) ) );
            selScratch.Items.Add( " " );
            selScratch.Items.Add( new OptionDisplay( Properties.Resources.Context_Quit, () => ExecuteKey( Keys.End ) ) );

            // Anzeigen
            ShowList( Properties.Resources.ContextTitle, selScratch, 0, OSDShowMode.ContextMenu );
        }

        /// <summary>
        /// Führt die Aktion zu einer Taste aus.
        /// </summary>
        /// <param name="key">Die gwünschte Taste.</param>
        private void ExecuteKey( Keys key )
        {
            // Load
            BDAWindow.KeyProcessor processor;
            if (directShow.KeyProcessors.TryGetValue( key, out processor )) processor( key );
        }

        /// <summary>
        /// Die Senderverwaltung meldet hierüber, dass ein neuer NVOD Dienst ausgewählt wurde.
        /// </summary>
        /// <param name="selector">Die Senderverwaltung.</param>
        /// <param name="serviceName">Der Name des neuen NVOD Dienstes.</param>
        /// <param name="context">Das <see cref="ServiceItem"/> zum NVOD Dienst.</param>
        private void ServiceSelected( ChannelSelector selector, string serviceName, object context )
        {
            // Be safe
            try
            {
                // Forward
                ShowName( m_CurrentAdaptor.SetService( (ServiceItem) context ) );
            }
            catch (Exception ex)
            {
                // Report
                ShowError( ex );
            }
        }

        /// <summary>
        /// Die Senderverwaltung meldet hierüber, dass eine neue Tonspur ausgewählt wurde.
        /// </summary>
        /// <param name="selector">Die Senderverwaltung.</param>
        /// <param name="audioName">Der Name der gewünschten Tonspur.</param>
        private void TrackSelected( ChannelSelector selector, string audioName )
        {
            // Be safe
            try
            {
                // Forward
                ShowName( m_CurrentAdaptor.SetAudio( audioName ) );
            }
            catch (Exception ex)
            {
                // Report
                ShowError( ex );
            }
        }

        /// <summary>
        /// Erzeugt ein OSD mit dem Namen des aktuellen Senders inklusive der gewählten
        /// Tonspur.
        /// </summary>
        /// <param name="fullName">Anzuzeigender Name oder <i>null</i>, wenn keine Anzeige
        /// erfolgen soll.</param>
        private void ShowName( string fullName )
        {
            // Nothing to show
            if (!string.IsNullOrEmpty( fullName )) ShowMessage( fullName, Properties.Resources.NameTitle, true );
        }

        /// <summary>
        /// Die Senderverwaltung meldet hierüber, dass ein neuer Sender ausgewählt wurde.
        /// </summary>
        /// <param name="selector">Die Senderverwaltung.</param>
        /// <param name="channelName">Der Anzeigename des Sender.</param>
        /// <param name="context">Eine beliebige Instanz zu einem Sender, wie sie im aktuellen
        /// <see cref="Adaptor"/> verstanden wird.</param>
        private void ChannelSelected( ChannelSelector selector, string channelName, object context )
        {
            // Be safe
            try
            {
                // Process
                ShowName( m_CurrentAdaptor.SetStation( context ) );
            }
            catch (Exception ex)
            {
                // Report
                ShowError( ex );
            }
        }

        /// <summary>
        /// Erzeugt die Belegung der Tasten für die einzelnen Funktionen.
        /// </summary>
        /// <remarks>
        /// Die Belegung ist in der aktuellen Version fest codiert. In späteren Varianten
        /// soll es möglich sein, zumindest einen Ausschnitt der Tastenkürzel frei definieren
        /// zu können.
        /// </remarks>
        private void LoadKeys()
        {
            // Keys mapped to core implementation
            directShow.KeyProcessors[Keys.Subtract] = PreviousChannel;
            directShow.KeyProcessors[Keys.PageDown] = ListPageDown;
            directShow.KeyProcessors[(Keys) 189] = PreviousChannel;
            directShow.KeyProcessors[(Keys) 191] = StartRecording;
            directShow.KeyProcessors[Keys.RButton] = RightClick;
            directShow.KeyProcessors[Keys.LButton] = LeftClick;
            directShow.KeyProcessors[Keys.K] = ShowChannelList;
            directShow.KeyProcessors[Keys.M] = ShowServiceList;
            directShow.KeyProcessors[Keys.PageUp] = ListPageUp;
            directShow.KeyProcessors[Keys.NumPad0] = ProcessAs;
            directShow.KeyProcessors[Keys.NumPad1] = ProcessAs;
            directShow.KeyProcessors[Keys.NumPad2] = ProcessAs;
            directShow.KeyProcessors[Keys.NumPad3] = ProcessAs;
            directShow.KeyProcessors[Keys.NumPad4] = ProcessAs;
            directShow.KeyProcessors[Keys.NumPad5] = ProcessAs;
            directShow.KeyProcessors[Keys.NumPad6] = ProcessAs;
            directShow.KeyProcessors[Keys.NumPad7] = ProcessAs;
            directShow.KeyProcessors[Keys.NumPad8] = ProcessAs;
            directShow.KeyProcessors[Keys.NumPad9] = ProcessAs;
            directShow.KeyProcessors[(Keys) 187] = NextChannel;
            directShow.KeyProcessors[(Keys) 219] = ShowOptions;
            directShow.KeyProcessors[Keys.Escape] = EscapeKey;
            directShow.KeyProcessors[Keys.D0] = ForwardTTXKey;
            directShow.KeyProcessors[Keys.D1] = ForwardTTXKey;
            directShow.KeyProcessors[Keys.D2] = ForwardTTXKey;
            directShow.KeyProcessors[Keys.D3] = ForwardTTXKey;
            directShow.KeyProcessors[Keys.D4] = ForwardTTXKey;
            directShow.KeyProcessors[Keys.D5] = ForwardTTXKey;
            directShow.KeyProcessors[Keys.D6] = ForwardTTXKey;
            directShow.KeyProcessors[Keys.D7] = ForwardTTXKey;
            directShow.KeyProcessors[Keys.D8] = ForwardTTXKey;
            directShow.KeyProcessors[Keys.D9] = ForwardTTXKey;
            directShow.KeyProcessors[Keys.F3] = ShowPosition;
            directShow.KeyProcessors[Keys.Add] = NextChannel;
            directShow.KeyProcessors[Keys.L] = ShowAudioList;
            directShow.KeyProcessors[Keys.Left] = VolumeDown;
            directShow.KeyProcessors[Keys.Right] = VolumeUp;
            directShow.KeyProcessors[Keys.Down] = ListDown;
            directShow.KeyProcessors[Keys.Return] = Select;
            directShow.KeyProcessors[Keys.Enter] = Select;
            directShow.KeyProcessors[Keys.A] = ForwardKey;
            directShow.KeyProcessors[Keys.B] = ForwardKey;
            directShow.KeyProcessors[Keys.C] = ForwardKey;
            directShow.KeyProcessors[Keys.D] = ForwardKey;
            directShow.KeyProcessors[Keys.E] = ForwardKey;
            directShow.KeyProcessors[Keys.F] = ForwardKey;
            directShow.KeyProcessors[Keys.G] = ForwardKey;
            directShow.KeyProcessors[Keys.H] = ForwardKey;
            directShow.KeyProcessors[Keys.I] = ForwardKey;
            directShow.KeyProcessors[Keys.N] = ServiceKey;
            directShow.KeyProcessors[Keys.O] = ServiceKey;
            directShow.KeyProcessors[Keys.P] = ServiceKey;
            directShow.KeyProcessors[Keys.Q] = ServiceKey;
            directShow.KeyProcessors[Keys.R] = ServiceKey;
            directShow.KeyProcessors[Keys.S] = ServiceKey;
            directShow.KeyProcessors[Keys.U] = ServiceKey;
            directShow.KeyProcessors[Keys.V] = ServiceKey;
            directShow.KeyProcessors[Keys.W] = ServiceKey;
            directShow.KeyProcessors[Keys.X] = ServiceKey;
            directShow.KeyProcessors[Keys.Y] = ServiceKey;
            directShow.KeyProcessors[Keys.Z] = ServiceKey;
            directShow.KeyProcessors[Keys.F1] = ShowEPG;
            directShow.KeyProcessors[Keys.F2] = ShowTTX;
            directShow.KeyProcessors[Keys.Up] = ListUp;
        }

        /// <summary>
        /// Interpretiert eine Taste um.
        /// </summary>
        /// <param name="key">Die tatsächlich gedrückte Taste.</param>
        private void ProcessAs( Keys key )
        {
            // Remap
            switch (key)
            {
                case Keys.NumPad0: key = Keys.D0; break;
                case Keys.NumPad1: key = Keys.D1; break;
                case Keys.NumPad2: key = Keys.D2; break;
                case Keys.NumPad3: key = Keys.D3; break;
                case Keys.NumPad4: key = Keys.D4; break;
                case Keys.NumPad5: key = Keys.D5; break;
                case Keys.NumPad6: key = Keys.D6; break;
                case Keys.NumPad7: key = Keys.D7; break;
                case Keys.NumPad8: key = Keys.D8; break;
                case Keys.NumPad9: key = Keys.D9; break;
                default: return;
            }

            // Load
            BDAWindow.KeyProcessor processor = directShow.KeyProcessors[key];

            // Run
            if (processor != null)
                processor( key );
        }

        /// <summary>
        /// Die linke Masutaste wurde gedrückt, was als Auswahl in der aktuellen Liste interpretiert wird.
        /// </summary>
        /// <param name="key">Die vom Anwender betätigte Taste.</param>
        private void LeftClick( Keys key )
        {
            // Check for consistency and process
            if (OSDActive)
                if (m_LastOSDWasList)
                {
                    // Select the entry
                    Select( Keys.Enter );
                }
                else if (m_LastProgressWasFile)
                {
                    // File movements are triggered using the record key
                    ExecuteKey( (Keys) 191 );
                }
                else if (IsVideoTextActive)
                {
                    // Not possible
                    if (m_TTXDigits == null)
                        return;

                    // Get relative position in videotext window
                    PointF? ttxHit = m_Overlay.TeleTextHit;

                    // None
                    if (!ttxHit.HasValue)
                        return;

                    // Find the page
                    int? page = m_TTXDigits.GetPageAt( ttxHit.Value );

                    // Use if
                    if (page.HasValue)
                    {
                        // Reset page collector
                        m_TTXLastKey = DateTime.MinValue;
                        m_PendingTTXPage = null;

                        // Select the page
                        m_CurrentAdaptor.VideoText.CurrentPage = page;
                    }
                }
        }

        /// <summary>
        /// Zeigt EPG Informationen an, falls vorhanden.
        /// </summary>
        /// <param name="key">Vom Anwender gedrückte Taste.</param>
        private void ShowEPG( Keys key )
        {
            // End display
            HideOSD();

            // Forward
            if (m_CurrentAdaptor != null)
                m_CurrentAdaptor.ShowEPG();
        }

        /// <summary>
        /// Zeigt die Videotext Seite 100 an.
        /// </summary>
        /// <param name="key">Vom Anwender gedrückte Taste.</param>
        private void ShowTTX( Keys key )
        {
            // Check cuurent state
            var showingText = (m_OSDShowMode == OSDShowMode.Videotext);

            // End display
            HideOSD();

            // Forward
            if (!showingText)
                if (null != m_CurrentAdaptor)
                    if (m_CurrentAdaptor.TTXAvailable)
                    {
                        // Reset builder
                        m_PendingTTXPage = null;

                        // Choose page
                        m_CurrentAdaptor.VideoText.CurrentPage = 100;
                    }
        }

        /// <summary>
        /// Meldet, ob gerade eine Videotext Seite angezeigt wird.
        /// </summary>
        public bool IsVideoTextActive
        {
            get
            {
                // Report
                return (m_OSDOff == TTXOnTime);
            }
        }

        /// <summary>
        /// Prüft, ob eine Taste im Rahmen der aktuellen Videotext Seite verarbeitet
        /// werden muss.
        /// <seealso cref="PreprocessKey"/>
        /// <seealso cref="ForwardKey"/>
        /// </summary>
        /// <param name="key">Die Taste, die der Anwender gedrückt hat.</param>
        private void ForwardTTXKey( Keys key )
        {
            // Forward if not eaten up by TTX
            if (!PreprocessKey( key )) ForwardKey( key );
        }

        /// <summary>
        /// Führt eine Taste mit einer besonderen Bedeutung für den Videotext aus.
        /// </summary>
        /// <param name="key">Die gedrückte Taste.</param>
        /// <returns>Gesetzt, wenn die Taste verarbeitet wurde.</returns>
        public bool PreprocessKey( Keys key )
        {
            // See if this is the END key
            if (Keys.End == key) return IsRecording;

            // TTX not active
            if (!IsVideoTextActive) return false;

            // Check key
            if ((key < Keys.D0) || (key > Keys.D9))
            {
                // The delta
                int page;

                // Move down or up
                if ((Keys.Subtract == key) || ((Keys) 189 == key))
                {
                    // Down
                    page = -1;
                }
                else if ((Keys.Add == key) || ((Keys) 187 == key))
                {
                    // Up
                    page = +1;
                }
                else
                {
                    // Done
                    return false;
                }

                // Inconsistent state - do nothing
                if (!m_CurrentAdaptor.VideoText.CurrentPage.HasValue)
                    return true;

                // Calculate
                page += m_CurrentAdaptor.VideoText.CurrentPage.Value;

                // Skip if out of bounds
                if ((page < 100) || (page > 899))
                    return true;

                // Use next
                m_TTXPageBuilder = page;
            }
            else
            {
                // Clear key if too old
                TimeSpan delta = DateTime.UtcNow - m_TTXLastKey;

                // At most three seconds
                if (delta.TotalSeconds > 3) m_TTXPageBuilder = 0;

                // Advance
                m_TTXPageBuilder *= 10;
                m_TTXPageBuilder += key - Keys.D0;

                // Create a string from the page
                if ((m_TTXPageBuilder > 0) && (m_TTXPageBuilder < 100))
                    m_PendingTTXPage = string.Format( "{0}??", m_TTXPageBuilder ).Substring( 0, 3 );
                else
                    m_PendingTTXPage = null;

                // Refresh
                if (m_CurrentTTXPage != null)
                    ShowTTXPage( m_CurrentTTXPage );

                // Check mode
                if ((m_TTXPageBuilder < 100) || (m_TTXPageBuilder > 899))
                {
                    // Wait for the next key
                    m_TTXLastKey = DateTime.UtcNow;

                    // Wait for next
                    return true;
                }
            }

            // Reset
            m_TTXLastKey = DateTime.MinValue;
            m_PendingTTXPage = null;

            // Select the page
            m_CurrentAdaptor.VideoText.CurrentPage = m_TTXPageBuilder;

            // Do nothing
            return true;
        }

        /// <summary>
        /// Increase volume.
        /// </summary>
        /// <param name="key">Ignored.</param>
        private void VolumeUp( Keys key )
        {
            // Forward
            VolumeChange( +0.01 );
        }

        /// <summary>
        /// Decrease volume.
        /// </summary>
        /// <param name="key">Ignored.</param>
        private void VolumeDown( Keys key )
        {
            // Forward
            VolumeChange( -0.01 );
        }

        /// <summary>
        /// Beginnt oder beendet eine Aufzeichnung.
        /// </summary>
        /// <param name="key"></param>
        private void StartRecording( Keys key )
        {
            // Be safe
            try
            {
                // Forward
                m_CurrentAdaptor.StartRecording();
            }
            catch (Exception ex)
            {
                // Report
                ShowError( ex );
            }
        }

        /// <summary>
        /// Meldet, ob gerade eine Aufzeichnung läuft.
        /// </summary>
        /// <remarks>
        /// Ist das der Fall, so erfolgt eine entsprechende Anzeige als
        /// Warnung.
        /// </remarks>
        private bool IsRecording
        {
            get
            {
                // No
                if (!m_CurrentAdaptor.IsRecording) return false;

                // Report
                ShowMessage( string.Format( Properties.Resources.RecordingActive, m_CurrentAdaptor.RecordedBytes / 1024 ), Properties.Resources.RecordingTitle, true );

                // Yes
                return true;
            }
        }

        /// <summary>
        /// Meldet einen Tastendruck an die Senderverwaltung, wenn keine Aufzeichnung
        /// aktiv ist.
        /// </summary>
        /// <param name="key">Aktuelle Eingabe.</param>
        private void ForwardKey( Keys key )
        {
            // Not while recording
            if (IsRecording) return;

            // Hide any overlay
            HideOSD();

            // Send
            m_FavoriteManager.AnalyseStandardKey( key );
        }

        /// <summary>
        /// Aktiviert über die Senderverwaltung den nächsten Sender, wenn keine
        /// Aufzeichnung aktiv ist.
        /// </summary>
        /// <param name="key">Aktuelle Eingabe.</param>
        private void NextChannel( Keys key )
        {
            // Check for videotext
            if (PreprocessKey( key )) return;

            // Not while recording
            if (IsRecording) return;

            // Hide any overlay
            HideOSD();

            // Forward
            m_FavoriteManager.AnalyseStandardKey( '+' );
        }

        /// <summary>
        /// Aktiviert über die Senderverwaltung den vorherigen Sender, wenn keine
        /// Aufzeichnung aktiv ist.
        /// </summary>
        /// <param name="key">Aktuelle Eingabe.</param>
        private void PreviousChannel( Keys key )
        {
            // Check for videotext
            if (PreprocessKey( key )) return;

            // Not while recording
            if (IsRecording) return;

            // Hide any overlay
            HideOSD();

            // Forward
            m_FavoriteManager.AnalyseStandardKey( '-' );
        }

        /// <summary>
        /// Deaktiviert das OSD.
        /// </summary>
        /// <param name="key">Aktuelle Eingabe.</param>
        private void EscapeKey( Keys key )
        {
            // Hide
            HideOSD();
        }

        /// <summary>
        /// Wählt den in der aktuellen Auswahlliste aktiven Eintrag aus und
        /// führt die entsprechende Funktionalität aus.
        /// </summary>
        /// <remarks>
        /// Im Allgemeinen erfolgt eine Weitergabe an die Senderverwaltung.
        /// </remarks>
        /// <param name="key">Aktuelle Eingabe.</param>
        private void Select( Keys key )
        {
            // Not while recording
            if (IsRecording) return;

            // Already did it
            if (!m_CanSelect) return;

            // Only once
            m_CanSelect = false;

            // No list active
            if (null == m_CurrentList) return;

            // Hide OSD
            HideOSD();

            // Back to volume control mode
            m_LastProgressWasFile = false;
            m_LastOSDWasList = false;

            // Be safe
            try
            {
                // Call local configuration
                if ((selOptions == m_CurrentList) || (selScratch == m_CurrentList))
                {
                    // Get the current selection
                    OptionDisplay option = m_CurrentList.SelectedItem as OptionDisplay;

                    // Call
                    if (null != option) option.Option();

                    // Done
                    return;
                }

                // Restart timer
                signalTest.Enabled = false;
                signalTest.Enabled = true;

                // Hide signal control
                m_NoSignal = false;

                // Update GUI
                Application.DoEvents();

                // Process
                m_FavoriteManager.FinishSelection( m_CurrentList );
            }
            catch (Exception ex)
            {
                // Report
                ShowError( ex );
            }
        }

        /// <summary>
        /// Verschiebt den aktiven Eintrag der aktuellen Auswahlliste im OSD.
        /// </summary>
        /// <param name="delta">Gibt an, wie die Verschiebung durchzuführen ist.</param>
        private void MoveListIndex( int delta )
        {
            // Do nothing
            if (m_CurrentList == null)
                return;
            if (m_OSDShowMode == OSDShowMode.Videotext)
                return;

            // Be safe
            try
            {
                // Update
                m_CurrentList.SelectedIndex = Math.Max( 0, Math.Min( m_CurrentList.Items.Count - 1, m_CurrentList.SelectedIndex + delta ) );

                // Show up
                ShowList( 0, m_OSDShowMode );
            }
            catch
            {
                // Ignore any error
            }
        }

        /// <summary>
        /// Aktiviert in der aktuellen Auswahlliste im OSD den vorherigen Eintrag,
        /// wenn keine Aufzeichnung aktiv ist.
        /// </summary>
        /// <param name="key">Aktuelle Eingabe</param>
        private void ListUp( Keys key )
        {
            // Not while recording
            if (IsRecording) return;

            // Process
            MoveListIndex( -1 );
        }

        /// <summary>
        /// Aktiviert in der aktuellen Auswahlliste im OSD den nächsten Eintrag,
        /// wenn keine Aufzeichnung aktiv ist.
        /// </summary>
        /// <param name="key">Aktuelle Eingabe</param>
        private void ListDown( Keys key )
        {
            // Not while recording
            if (IsRecording) return;

            // Process
            MoveListIndex( +1 );
        }

        /// <summary>
        /// Aktiviert in der aktuellen Auswahlliste im OSD einen Eintrag auf
        /// der vorherigen Seite, wenn keine Aufzeichnung aktiv ist.
        /// </summary>
        /// <remarks>
        /// In der aktuellen Implementierung wird der aktive Eintrag um eine Seite
        /// zum Anfang der Auswahlliste hin verschoben.
        /// </remarks>
        /// <param name="key">Aktuelle Eingabe</param>
        private void ListPageUp( Keys key )
        {
            // Not while recording
            if (IsRecording) return;

            // Process
            MoveListIndex( -m_Overlay.MaximumNumberOfLines );
        }

        /// <summary>
        /// Aktiviert in der aktuellen Auswahlliste im OSD einen Eintrag auf
        /// der nächsten Seite, wenn keine Aufzeichnung aktiv ist.
        /// </summary>
        /// <remarks>
        /// In der aktuellen Implementierung wird der aktive Eintrag um eine Seite
        /// zum Ende der Auswahlliste hin verschoben.
        /// </remarks>
        /// <param name="key">Aktuelle Eingabe</param>
        private void ListPageDown( Keys key )
        {
            // Not while recording
            if (IsRecording) return;

            // Process
            MoveListIndex( +m_Overlay.MaximumNumberOfLines );
        }

        /// <summary>
        /// Zeigt die aktuelle Auswahlliste im OSD an.
        /// </summary>
        /// <param name="minShow">Enthält die Liste weniger Einträge als diese
        /// Zahl, so wird das OSD nicht angezeigt.</param>
        /// <param name="mode">Die Art der angezeigten Daten.</param>
        private void ShowList( int minShow, OSDShowMode mode )
        {
            // Too few items in list
            if (m_CurrentList.Items.Count < minShow)
            {
                // Forget
                m_CurrentHead = null;
                m_CurrentList = null;

                // Done
                return;
            }

            // Get the minimum to show
            int lowIndex = Math.Max( 0, m_CurrentList.SelectedIndex - m_Overlay.MaximumNumberOfLines / 2 );
            int highIndex = Math.Min( m_CurrentList.Items.Count, lowIndex + m_Overlay.MaximumNumberOfLines );

            // Start OSD
            using (var osd = CreateTextOverlay( m_Overlay.MaximumNumberOfLines, m_CurrentHead, mode ))
            {
                // Normal lines
                for (int i = lowIndex; i < highIndex; ++i)
                    osd.Builder.WriteLine( i == m_CurrentList.SelectedIndex, m_CurrentList.Items[i].ToString() );

                // Bug to feature to use empty line
                if (highIndex < m_CurrentList.Items.Count)
                    osd.Builder.WriteLine( "..." );
            }

            // Set
            m_LastOSDWasList = true;
        }

        /// <summary>
        /// Setzt eine Auswahlliste als die aktuelle Auswahlliste und zeigt sie
        /// im OSD an.
        /// <seealso cref="ShowList(int, OSDShowMode)"/>
        /// </summary>
        /// <param name="headline">Gewünschte Überschrift.</param>
        /// <param name="selection">Die neue aktuelle Auswahlliste.</param>
        /// <param name="minShow">Enthält die Liste weniger als diese Anzahl von
        /// Einträgen, so wird sie nicht angezeigt.</param>
        /// <param name="mode">Die Art der angezeigten Daten.</param>
        public void ShowList( string headline, ComboBox selection, int minShow, OSDShowMode mode )
        {
            // Double means off
            if (m_OSDShowMode == mode)
            {
                // Discard
                HideOSD();
            }
            else
            {
                // Reset
                m_CurrentList = selection;
                m_CurrentHead = headline;

                // Forward
                ShowList( minShow, mode );
            }
        }

        /// <summary>
        /// Ist keine Aufzeichnung aktiv, so wird die Liste der verfügbaren Konfigurationsoptionen
        /// zur aktuellen Auswahlliste im OSD und diese anzeigt.
        /// </summary>
        /// <param name="key">Aktuelle Eingabe.</param>
        private void ShowOptions( Keys key )
        {
            // Not while recording
            if (IsRecording) return;

            // Process
            ShowList( Properties.Resources.OptionListHeadLine, selOptions, 1, OSDShowMode.ContextMenu );
        }

        /// <summary>
        /// Ist keine Aufzeichnung aktiv, so wird die Senderliste
        /// zur aktuellen Auswahlliste im OSD und diese anzeigt.
        /// </summary>
        /// <param name="key">Aktuelle Eingabe.</param>
        private void ShowChannelList( Keys key )
        {
            // Not while recording
            if (IsRecording) return;

            // Process
            ShowList( Properties.Resources.ChannelListHeadLine, m_FavoriteManager.ChannelList, 1, OSDShowMode.SourceList );
        }

        /// <summary>
        /// Wählt einen NVOD Dienst über sein Kürzel aus, wenn keine Aufzeichnung aktiv ist.
        /// </summary>
        /// <param name="key">Aktuelle Eingabe.</param>
        private void ServiceKey( Keys key )
        {
            // Not while recording
            if (IsRecording) return;

            // Load list
            if (LoadServiceList()) m_FavoriteManager.AnalyseStandardKey( key );
        }

        /// <summary>
        /// Ist keine Aufzeichnung aktiv, so wird die Liste der im aktiven Portal aktuell verfügbaren
        /// NVOD Dienste erzeugt und diese angezeigt.
        /// </summary>
        /// <param name="key">Aktuelle Eingabe.</param>
        private void ShowServiceList( Keys key )
        {
            // Not while recording
            if (IsRecording) return;

            // Load list
            if (LoadServiceList()) ShowList( Properties.Resources.ServiceListHeadLine, m_FavoriteManager.ServiceList, 2, OSDShowMode.Services );
        }

        /// <summary>
        /// Ermittelt die Liste der auf dem aktuellen Portal verfügbaren NVOD Dienste und überträgt
        /// diese an die Senderverwaltung.
        /// </summary>
        /// <returns>Gesetzt, wenn mindestens ein NVOD Dienst angeboten wird.</returns>
        private bool LoadServiceList()
        {
            // Be safe
            try
            {
                // Time to load services
                m_FavoriteManager.ClearService();

                // All we know
                foreach (ServiceItem service in m_CurrentAdaptor.Services)
                {
                    // Add to list
                    m_FavoriteManager.AddService( service.ToString(), service );
                }

                // Finished
                m_FavoriteManager.FillServiceList();

                // Force load
                return m_FavoriteManager.ServiceList.Enabled;
            }
            catch (Exception ex)
            {
                // Report
                ShowError( ex );

                // Do not proceed
                return false;
            }
        }

        /// <summary>
        /// Ist keine Aufzeichnung aktiv, so wird die Liste der verfügbaren Tonspuren
        /// zur aktuellen Auswahlliste im OSD und diese anzeigt.
        /// </summary>
        /// <param name="key">Aktuelle Eingabe.</param>
        private void ShowAudioList( Keys key )
        {
            // Not while recording
            if (IsRecording) return;

            // Maybe delayed load is required
            if (null != m_CurrentAdaptor) m_CurrentAdaptor.LoadTracks();

            // Process
            ShowList( Properties.Resources.AudioListHeadLine, m_FavoriteManager.AudioList, 2, OSDShowMode.AudioTracks );
        }

        /// <summary>
        /// Zeigt die aktuelle zeitliche Position.
        /// </summary>
        /// <param name="key">Aktuelle Eingabe.</param>
        private void ShowPosition( Keys key )
        {
            // Turn off display
            HideOSD();

            // No adaptor active
            if (null == m_CurrentAdaptor) return;

            // See if there is any EPG data
            EventEntry current = m_CurrentAdaptor.CurrentEntry;

            // None
            if (null == current) return;

            // No entry
            if (TimeSpan.Zero == current.Duration) return;

            // See where we are
            TimeSpan pos = DateTime.UtcNow - current.StartTime;

            // Ups, not yet started
            if (pos.TotalMilliseconds <= 0) return;

            // See if we are in range
            if (pos > current.Duration) pos = current.Duration;

            // Get the relative position
            ShowPosition( pos.TotalMilliseconds / current.Duration.TotalMilliseconds, string.Format( Properties.Resources.EPGPosition, (int) (pos.TotalMinutes + 0.5), (int) (current.Duration.TotalMinutes + 0.5) ) );
        }

        /// <summary>
        /// Ermittelt die Liste der verfügbaren Sender und überträgt diese an die
        /// Senderverwaltung. Dann wird die Anwendung erstmalig gestartet oder
        /// Reinitialisert.
        /// </summary>
        /// <param name="applicationStart">Gesetzt, wenn die Anwendung in der Initialisierungsphase ist.</param>
        private void Restart( bool applicationStart )
        {
            // Configure graph
            directShow.UseCyberlink = GeneralInfo.UseCyberlinkCodec;
            directShow.MPEG2Decoder = GeneralInfo.MPEG2Decoder;
            directShow.AudioVideoDelay = GeneralInfo.AVDelay;
            directShow.H264Decoder = GeneralInfo.H264Decoder;
            directShow.MP2Decoder = GeneralInfo.MP2Decoder;
            directShow.AC3Decoder = GeneralInfo.AC3Decoder;

            // Forget lists
            m_CurrentList = null;

            // Reset favorite manager
            m_FavoriteManager.ClearAll();

            // Be safe
            try
            {
                // Process all stations
                m_CurrentAdaptor.LoadStations();

                // Load defaults
                ShowName( m_CurrentAdaptor.LoadDefaults( applicationStart ) );
            }
            catch (Exception ex)
            {
                // Report
                ShowError( ex );
            }
        }

        /// <summary>
        /// Prüft, ob die Anzeigezeit des OSD abgelaufen ist und deaktiviert dieses
        /// wenn nötig.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void osdOff_Tick( object sender, EventArgs e )
        {
            // Not yet
            if (DateTime.UtcNow < m_OSDOff) return;

            // Switch off
            HideOSD();
        }

        /// <summary>
        /// Prüft, ob das OSD gerade angezeigt wird.
        /// </summary>
        public bool OSDActive
        {
            get
            {
                // Report
                return (m_OSDOff != OSDOffTime);
            }
        }

        /// <summary>
        /// Prüft periodisch, ob noch Daten empfangen werden. Ist das nicht mehr der
        /// Fall, werden der Vollbildmodus deaktiviert und ein Fehler angezeigt.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void signalTest_Tick( object sender, EventArgs e )
        {
            // Be safe
            try
            {
                // Load current tick counter
                int tick = ((int?) signalTest.Tag) ?? 10;

                // Run the keep alive test
                m_CurrentAdaptor.KeepAlive( --tick > 0 );

                // Store back
                signalTest.Tag = (0 == tick) ? 10 : tick;

                // Previous
                long before = m_LastBytes;

                // Current
                m_LastBytes = directShow.BytesReceived;

                // Check mode
                bool noNewData = (m_LastBytes == before);

                // Check
                if (noNewData == m_NoSignal) return;

                // Change
                m_NoSignal = noNewData;
            }
            catch (Exception ex)
            {
                // Report
                ShowError( ex );
            }
        }

        /// <summary>
        /// Hierüber meldet der DirectShow Graph, dass die Lautstärke verändert wurde.
        /// Es erfolgt eine entsprechende Visualisierung im OSD.
        /// </summary>
        /// <param name="delta">Change of volume.</param>
        private void VolumeChange( double delta )
        {
            // Not possible
            if (null == m_GeneralInfo) return;

            // Process
            double volume = Math.Min( 1, Math.Max( 0, m_GeneralInfo.Volume + delta ) );

            // No change at all
            if (volume == m_GeneralInfo.Volume)
                if (delta != 0)
                    return;

            // Update
            m_GeneralInfo.Volume = volume;

            // Send
            directShow.Volume = volume;

            // Report
            using (var osd = CreateTextOverlay( 1, string.Format( Properties.Resources.VolumeTitle, (int) (volume * 100) ), OSDShowMode.Volume ))
            {
                // Fill
                osd.Builder.ShowProgress( volume, true );
            }

            // Remember what we are doing
            m_LastProgressWasFile = false;
        }

        /// <summary>
        /// Zeigt eine Fehlermeldung im OSD an.
        /// </summary>
        /// <param name="e">Der aufgetretene Fehler.</param>
        public void ShowError( Exception e )
        {
            // Show message
            ShowMessage( e.Message, Properties.Resources.ErrorTitle, false );
        }

        /// <summary>
        /// Zeigt eine Videotext Seite an.
        /// </summary>
        /// <param name="page"></param>
        private void ShowTTXPage( TTXPage page )
        {
            // Sychnronize
            if (InvokeRequired)
            {
                // Execute
                BeginInvoke( new TTXParser.PageHandler( ShowTTXPage ), page );

                // Done
                return;
            }

            // Safe create
            try
            {
                // Attach current next number
                page.Feedback = m_PendingTTXPage;

                // Create it
                if (!m_Overlay.ShowPage( page, out m_TTXDigits ))
                {
                    // Ups - failed
                    HideOSD();
                }
                else
                {
                    // Remember
                    m_CurrentTTXPage = page;

                    // Report
                    OSDShown( OSDShowMode.Videotext );

                    // Nearly (no) automatic termination of OSD
                    m_OSDOff = TTXOnTime;
                }
            }
            catch (Exception e)
            {
                // Report error
                ShowError( e );

                // Done
                return;
            }
        }

        #region IOSDSite Members

        /// <summary>
        /// Aktiviert das OSD.
        /// </summary>
        /// <remarks>
        /// Ein echtes OSD wird nur im Vollbildmodus verwendet.
        /// </remarks>
        /// <param name="bitmap">Im OSD anzuzeigende Daten.</param>
        /// <param name="left">Relative Position des OSD (linker Rand).</param>
        /// <param name="top">Relative Position des OSD (oberer Rand).</param>
        /// <param name="right">Relative Position des OSD (rechter Rand).</param>
        /// <param name="bottom">Relative Position des OSD (unterer Rand).</param>
        /// <param name="alpha">Transparenz des OSD. Befindet sich die Anwendung nicht im
        /// Vollbildmodus, wird dieser Parameter ignoriert und das OSD ist undurchsichtig.</param>
        /// <param name="transparent">Optional durchsichtige Farbe.</param>
        /// <param name="mode">Die Art der angezeigten Daten.</param>
        private void ShowOSD( Bitmap bitmap, double left, double top, double right, double bottom, double? alpha, Color? transparent, OSDShowMode mode )
        {
            // Update
            m_Overlay.ShowOverlay( bitmap, left, top, right, bottom, alpha, transparent );

            // Report
            OSDShown( mode );
        }

        /// <summary>
        /// Bestätigt die Anzeige einer Überblendung.
        /// </summary>
        /// <param name="mode">Die Art der angezeigten Daten.</param>
        private void OSDShown( OSDShowMode mode )
        {
            // Reset
            m_OSDShowMode = (mode == OSDShowMode.Nothing) ? OSDShowMode.Other : mode;
            m_LastProgressWasFile = false;
            m_LastOSDWasList = false;

            // Disable video text display
            if (mode != OSDShowMode.Videotext)
                m_CurrentAdaptor.VideoText.CurrentPage = null;

            // Allow selection - at least the user saw something
            m_CanSelect = true;

            // Start the OSD timer
            m_OSDOff = DateTime.UtcNow.AddSeconds( GeneralInfo.OSDLifeTime );
        }

        /// <summary>
        /// Erzeugt einen neuen Überlagerungstext.
        /// </summary>
        /// <param name="lines">Die gewünschte Anzahl von Zeilen.</param>
        /// <param name="headline">Die gewünschte Überschrift.</param>
        /// <param name="mode">Die Art der angezeigten Daten.</param>
        /// <returns>Die Steuerungseinheit.</returns>
        private _OSDText CreateTextOverlay( int lines, string headline, OSDShowMode mode )
        {
            // Forward
            return new _OSDText( this, m_Overlay.CreateTextOverlay( lines, headline, true ), mode );
        }

        /// <summary>
        /// Aktiviert das OSD.
        /// </summary>
        /// <remarks>
        /// Ein echtes OSD wird nur im Vollbildmodus verwendet.
        /// </remarks>
        /// <param name="bitmap">Im OSD anzuzeigende Daten.</param>
        /// <param name="left">Relative Position des OSD (linker Rand).</param>
        /// <param name="top">Relative Position des OSD (oberer Rand).</param>
        /// <param name="right">Relative Position des OSD (rechter Rand).</param>
        /// <param name="bottom">Relative Position des OSD (unterer Rand).</param>
        /// <param name="alpha">Transparenz des OSD. Befindet sich die Anwendung nicht im
        /// Vollbildmodus, wird dieser Parameter ignoriert und das OSD ist undurchsichtig.</param>
        /// <param name="transparent">Optional durchsichtige Farbe.</param>
        /// <param name="mode">Die Art der angezeigten Daten.</param>
        void IOSDSite.Show( Bitmap bitmap, double left, double top, double right, double bottom, double? alpha, Color? transparent, OSDShowMode mode )
        {
            // Forward
            ShowOSD( bitmap, left, top, right, bottom, alpha, transparent, mode );
        }

        /// <summary>
        /// Gesetzt, wenn die Anzeige als echtes Overlay erfolgen soll
        /// </summary>
        public bool UsesOverlay
        {
            get
            {
                // Report
                return ((GetAsyncKeyState( 16 ) >= 0) && (directShow.VideoBytesReceived > 0));
            }
        }

        /// <summary>
        /// Gesetzt, wenn die Anzeige als echtes Overlay erfolgen soll
        /// </summary>
        bool IOSDSite.UsesOverlay
        {
            get
            {
                // Forward
                return UsesOverlay;
            }
        }

        /// <summary>
        /// Entfernt das OSD.
        /// </summary>
        private void HideOSD()
        {
            // Disable video text display
            m_CurrentAdaptor.VideoText.CurrentPage = null;

            // Forward
            HideOSD( true );
        }

        /// <summary>
        /// Entfernt das OSD.
        /// </summary>
        /// <param name="hideControls">Gesetzt, wenn all visuellen OSD Element ausgeblendet werden sollen.</param>
        private void HideOSD( bool hideControls )
        {
            // Clear reset timer
            m_OSDOff = OSDOffTime;

            // Check
            if (hideControls)
            {
                // Put off
                m_Overlay.Hide();

                // Reset mode
                m_OSDShowMode = OSDShowMode.Nothing;
            }

            // Back to volume control mode
            m_LastProgressWasFile = false;
            m_LastOSDWasList = false;
        }

        /// <summary>
        /// Entfernt das OSD.
        /// </summary>
        void IOSDSite.Hide()
        {
            // Forward
            HideOSD();
        }

        #endregion

        #region ViewerControl Members

        /// <summary>
        /// Meldet die aktuelle Verwaltungsinstanz der Favoriten.
        /// </summary>
        ChannelSelector IViewerSite.FavoriteManager
        {
            get
            {
                // Report
                return m_FavoriteManager;
            }
        }

        /// <summary>
        /// Zeit eine einzeilige Nachricht im OSD an.
        /// </summary>
        /// <param name="message">Die Nachricht.</param>
        /// <param name="headline">Überschrift für das Nachrichtenfeld.</param>
        /// <param name="realOSD">Gesetzt, wenn das echte OSD verwendet werden soll.</param>
        private void ShowMessage( string message, string headline, bool realOSD )
        {
            // Report
            using (var osd = CreateTextOverlay( 1, headline, OSDShowMode.Other ))
            {
                // Set mode
                if (!realOSD)
                    osd.Builder.DisableOverlay();

                // Fill
                osd.Builder.WriteLine( message );
            }

            // Forward to title
            if (Equals( headline, Properties.Resources.NameTitle )) GeneralInfo.SetWindowTitle( message );
        }

        /// <summary>
        /// Zeit eine einzeilige Nachricht im OSD an.
        /// </summary>
        /// <param name="message">Die Nachricht.</param>
        /// <param name="headline">Überschrift für das Nachrichtenfeld.</param>
        /// <param name="realOSD">Gesetzt, wenn das echte OSD verwendet werden soll.</param>
        void IViewerSite.ShowMessage( string message, string headline, bool realOSD )
        {
            // Forward
            ShowMessage( message, headline, realOSD );
        }

        object IViewerSite.Invoke( Delegate method, params object[] args )
        {
            // Forward to .NET
            return Invoke( method, args );
        }

        /// <summary>
        /// Set die Behandung der Tastatureingabe auf den ursprünglichen Zustand zurück.
        /// </summary>
        void IViewerSite.ResetKeyHandlers()
        {
            // Clear
            directShow.KeyProcessors.Clear();

            // Reload from initial state
            foreach (KeyValuePair<Keys, BDAWindow.KeyProcessor> map in m_Handlers)
            {
                // Reinstall
                directShow.KeyProcessors[map.Key] = map.Value;
            }
        }

        ComboBox IViewerSite.ScratchComboBox
        {
            get
            {
                // Report
                return selScratch;
            }
        }

        void IViewerSite.ShowList( string headline, int minShow, OSDShowMode mode )
        {
            // Forward
            ShowList( headline, selScratch, minShow, mode );
        }

        void IViewerSite.ResetOptions()
        {
            // Clear all
            selOptions.Items.Clear();

            // Load all the global ones
            foreach (OptionDisplay option in m_GlobalOptions) selOptions.Items.Add( option );
        }

        /// <summary>
        /// Liest oder verändert die Parameter des Videobildes.
        /// </summary>
        PictureParameters IViewerSite.PictureParameters
        {
            get
            {
                // Forward
                return directShow.PictureParameters;
            }
            set
            {
                // Forward
                directShow.PictureParameters = value;
            }
        }

        void IViewerSite.FillOptions()
        {
            // Reset global options
            m_GlobalOptions.Clear();

            // Load to global options
            foreach (OptionDisplay option in selOptions.Items) m_GlobalOptions.Add( option );

            // Forward
            m_CurrentAdaptor.FillOptions();
        }

        /// <summary>
        /// Meldet, ob beim Ändern der Direct Show Filter ein Neustart des
        /// Graphen unterstützt wird.
        /// </summary>
        bool IViewerSite.CanRestartGraph
        {
            get
            {
                // Forward
                return m_CurrentAdaptor.CanRestartGraph;
            }
        }

        void IViewerSite.AddOption( OptionDisplay option )
        {
            // Store to list
            selOptions.Items.Add( option );
        }

        Adaptor IViewerSite.CurrentAdaptor
        {
            get
            {
                // Report
                return m_CurrentAdaptor;
            }
        }

        /// <summary>
        /// Zeigt die Maus an.
        /// </summary>
        /// <returns>Steuereinheit zum erneuten Verbergen der Maus.</returns>
        public IDisposable ShowCursor()
        {
            // Forward
            return (m_GeneralInfo != null) ? m_GeneralInfo.ShowCursor() : null;
        }

        void IViewerSite.Restart()
        {
            // Forward
            Restart( false );
        }

        /// <summary>
        /// Zeigt die aktuelle Position an.
        /// </summary>
        /// <param name="percentage">Die relative Position zwischen 0 und 1.</param>
        /// <param name="headline">Überschreift zur Anzeige.</param>
        private void ShowPosition( double percentage, string headline )
        {
            // Report
            using (var osd = CreateTextOverlay( 1, string.Format( headline, (int) (100 * percentage) ), OSDShowMode.Position ))
            {
                // Fill
                osd.Builder.ShowProgress( percentage, false );
            }
        }

        /// <summary>
        /// Zeigt die aktuelle Position in einer Datei an.
        /// </summary>
        /// <param name="percentage">Die relative Position zwischen 0 und 1.</param>
        void IViewerSite.ShowPositionInFile( double percentage )
        {
            // Display
            ShowPosition( percentage, Properties.Resources.FileTitle );

            // Remember what we are doing
            m_LastProgressWasFile = true;
        }

        /// <summary>
        /// Meldet ein Tastaturbefehl an.
        /// </summary>
        /// <param name="key">Gewünschte Taste.</param>
        /// <param name="handler">Zugehörige Bearbeitungsroutine.</param>
        void IViewerSite.SetKeyHandler( Keys key, ViewerKeyStrokeCallback handler )
        {
            // Check mode
            if (null == handler)
                directShow.KeyProcessors.Remove( key );
            else
                directShow.KeyProcessors[key] = test => { if (!PreprocessKey( test )) handler(); };
        }

        /// <summary>
        /// Meldet, ob Radiosender in der Senderliste erscheinen sollen.
        /// </summary>
        bool IChannelInfo.UseRadio
        {
            get
            {
                // Forward
                return (null == m_ChannelInfo) ? true : m_ChannelInfo.UseRadio;
            }
        }

        /// <summary>
        /// Meldet, ob verschlüsselte Sender in der Senderliste erscheinen sollen.
        /// </summary>
        bool IChannelInfo.PayTV
        {
            get
            {
                // Forward
                return (null == m_ChannelInfo) ? true : m_ChannelInfo.PayTV;
            }
        }

        /// <summary>
        /// Meldet, ob unverschlüsselte Sender in der Senderliste erscheinen sollen.
        /// </summary>
        bool IChannelInfo.FreeTV
        {
            get
            {
                // Forward
                return (null == m_ChannelInfo) ? true : m_ChannelInfo.FreeTV;
            }
        }

        /// <summary>
        /// Meldet, ob Fernsehsender in der Senderliste erscheinen sollen.
        /// </summary>
        bool IChannelInfo.UseTV
        {
            get
            {
                // Forward
                return (null == m_ChannelInfo) ? true : m_ChannelInfo.UseTV;
            }
        }

        string ILocalInfo.LocalStation
        {
            get
            {
                // Forward
                return (null == m_LocalInfo) ? null : m_LocalInfo.LocalStation;
            }
            set
            {
                // Change & save
                if (null != m_LocalInfo) m_LocalInfo.LocalStation = value;
            }
        }

        string ILocalInfo.LocalAudio
        {
            get
            {
                // Forward
                return (null == m_LocalInfo) ? null : m_LocalInfo.LocalAudio;
            }
            set
            {
                // Change & save
                if (null != m_LocalInfo) m_LocalInfo.LocalAudio = value;
            }
        }

        string ILocalInfo.RecordingDirectory
        {
            get
            {
                // Forward
                return (null == m_LocalInfo) ? null : m_LocalInfo.RecordingDirectory;
            }
            set
            {
                // Change & save
                if (null != m_LocalInfo) m_LocalInfo.RecordingDirectory = value;
            }
        }

        string IRemoteInfo.VCRStation
        {
            get
            {
                // Forward
                return (null == m_RemoteInfo) ? null : m_RemoteInfo.VCRStation;
            }
            set
            {
                // Change & save
                if (null != m_RemoteInfo) m_RemoteInfo.VCRStation = value;
            }
        }

        string IRemoteInfo.VCRAudio
        {
            get
            {
                // Forward
                return (null == m_RemoteInfo) ? null : m_RemoteInfo.VCRAudio;
            }
            set
            {
                // Change & save
                if (null != m_RemoteInfo) m_RemoteInfo.VCRAudio = value;
            }
        }

        string IRemoteInfo.VCRProfile
        {
            get
            {
                // Forward
                return (null == m_RemoteInfo) ? null : m_RemoteInfo.VCRProfile;
            }
            set
            {
                // Change & save
                if (null != m_RemoteInfo) m_RemoteInfo.VCRProfile = value;
            }
        }

        string IStreamInfo.BroadcastIP
        {
            get
            {
                // Forward
                return (null == m_StreamInfo) ? null : m_StreamInfo.BroadcastIP;
            }
        }

        ushort IStreamInfo.BroadcastPort
        {
            get
            {
                // Forward
                return (null == m_StreamInfo) ? (ushort) 0 : m_StreamInfo.BroadcastPort;
            }
        }

        Uri IRemoteInfo.ServerUri
        {
            get
            {
                // Forward
                return (null == m_RemoteInfo) ? null : m_RemoteInfo.ServerUri;
            }
        }

        #endregion

        #region IGeneralInfo Members

        private IGeneralInfo GeneralInfo
        {
            get
            {
                // Report
                return (IGeneralInfo) this;
            }
        }

        int IGeneralInfo.OSDLifeTime
        {
            get
            {
                // Forward
                return (null == m_GeneralInfo) ? 5 : m_GeneralInfo.OSDLifeTime;
            }
        }

        double IGeneralInfo.Volume
        {
            get
            {
                // Forward
                return (null == m_GeneralInfo) ? 1 : m_GeneralInfo.Volume;
            }
            set
            {
                // Forward
                if (null != m_GeneralInfo) m_GeneralInfo.Volume = value;
            }
        }

        void IGeneralInfo.LeaveFullScreen()
        {
            // Forward
            if (null != m_GeneralInfo) m_GeneralInfo.LeaveFullScreen();
        }


        bool IGeneralInfo.UseCyberlinkCodec { get { return ((null == m_GeneralInfo) || m_GeneralInfo.UseCyberlinkCodec); } }

        /// <summary>
        /// Gesetzt, wenn die Fernsteuerung verwendet werden soll.
        /// </summary>
        public bool UseRemoteControl { get { return ((null == m_GeneralInfo) || m_GeneralInfo.UseRemoteControl); } }

        int IGeneralInfo.AVDelay { get { return (null == m_GeneralInfo) ? 500 : m_GeneralInfo.AVDelay; } }

        string IGeneralInfo.H264Decoder { get { return (null == m_GeneralInfo) ? null : m_GeneralInfo.H264Decoder; } }

        string IGeneralInfo.MPEG2Decoder { get { return (null == m_GeneralInfo) ? null : m_GeneralInfo.MPEG2Decoder; } }

        string IGeneralInfo.AC3Decoder { get { return (null == m_GeneralInfo) ? null : m_GeneralInfo.AC3Decoder; } }

        string IGeneralInfo.MP2Decoder { get { return (null == m_GeneralInfo) ? null : m_GeneralInfo.MP2Decoder; } }


        void IGeneralInfo.SetPictureParameters( PictureParameters parameters )
        {
            // Forward
            if (null != m_GeneralInfo) m_GeneralInfo.SetPictureParameters( parameters );
        }


        void IGeneralInfo.SetWindowTitle( string title )
        {
            // Forward
            if (null != m_GeneralInfo) m_GeneralInfo.SetWindowTitle( title );
        }

        #endregion

        /// <summary>
        /// Wird beim Starten ausgelöst.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ViewerControl_Load( object sender, EventArgs e )
        {
            // Install overlay
            m_Overlay = new OverlayWindow( this ) { Owner = FindForm() };

            // Connect message sink
            m_Overlay.OnGotMessage += ForwardOSDMessage;
            m_Overlay.MouseWheel += MouseWheelChanged;

            // See if we should remote control      
            if (UseRemoteControl)
                if (m_RCSettings.Mappings.Length > 0)
                {
                    // Install remote control
                    m_RCReceiver = RawInputSink.Create( Handle );

                    // Connect
                    m_RCReceiver.SetReceiver( ForwardRCMessage );
                }
        }

        /// <summary>
        /// Bearbeitet eine Windows Meldung über eine Benutzereingabe.
        /// </summary>
        /// <param name="m">Die zu bearbeitende Meldung</param>
        protected override void WndProc( ref System.Windows.Forms.Message m )
        {
            // Pre process
            if (m_RCReceiver != null)
                if (m_RCReceiver.ProcessMessage( ref m ))
                    return;

            // Forward
            base.WndProc( ref m );
        }

        /// <summary>
        /// Nimmt einen Befehl von der Fernsteuerung entgegen.
        /// </summary>
        /// <param name="item">Der empfangene Code.</param>
        private void ForwardRCMessage( MappingItem item )
        {
            // Be safe
            try
            {
                // None
                if (item == null)
                    return;

                // Reset
                if ((DateTime.UtcNow - m_LastRC).TotalSeconds >= 0.2)
                {
                    // Forget all
                    m_LastRC = DateTime.UtcNow;
                    m_CurrentRC.Clear();
                }

                // Collect
                m_CurrentRC.Add( item );

                // Load mapping
                var command = m_RCSettings[m_CurrentRC.ToArray()];
                if (!command.HasValue)
                    return;

                // Remap to keys
                switch (command.Value)
                {
                    case InputKey.SourceDown: directShow.ProcessKey( Keys.Subtract ); break;
                    case InputKey.PageDown: directShow.ProcessKey( Keys.PageDown ); break;
                    case InputKey.VolumeDown: directShow.ProcessKey( Keys.Left ); break;
                    case InputKey.VolumeUp: directShow.ProcessKey( Keys.Right ); break;
                    case InputKey.Information: directShow.ProcessKey( Keys.L ); break;
                    case InputKey.PageUp: directShow.ProcessKey( Keys.PageUp ); break;
                    case InputKey.ListDown: directShow.ProcessKey( Keys.Down ); break;
                    case InputKey.Menu: directShow.ProcessKey( Keys.RButton ); break;
                    case InputKey.SourceUp: directShow.ProcessKey( Keys.Add ); break;
                    case InputKey.Enter: directShow.ProcessKey( Keys.Enter ); break;
                    case InputKey.Pause: directShow.ProcessKey( (Keys) 191 ); break;
                    case InputKey.ListUp: directShow.ProcessKey( Keys.Up ); break;
                    case InputKey.Digit0: directShow.ProcessKey( Keys.D0 ); break;
                    case InputKey.Digit1: directShow.ProcessKey( Keys.D1 ); break;
                    case InputKey.Digit2: directShow.ProcessKey( Keys.D2 ); break;
                    case InputKey.Digit3: directShow.ProcessKey( Keys.D3 ); break;
                    case InputKey.Digit4: directShow.ProcessKey( Keys.D4 ); break;
                    case InputKey.Digit5: directShow.ProcessKey( Keys.D5 ); break;
                    case InputKey.Digit6: directShow.ProcessKey( Keys.D6 ); break;
                    case InputKey.Digit7: directShow.ProcessKey( Keys.D7 ); break;
                    case InputKey.Digit8: directShow.ProcessKey( Keys.D8 ); break;
                    case InputKey.Digit9: directShow.ProcessKey( Keys.D9 ); break;
                    case InputKey.Guide: directShow.ProcessKey( Keys.F1 ); break;
                    case InputKey.Text: directShow.ProcessKey( Keys.F2 ); break;
                    case InputKey.Off: directShow.ProcessKey( Keys.End ); break;
                    case InputKey.Mute: directShow.ProcessKey( Keys.F3 ); break;
                    case InputKey.List: directShow.ProcessKey( Keys.K ); break;
                }
            }
            catch
            {
                // Silent eat it up
            }
        }

        /// <summary>
        /// Leitet Eingaben weiter.
        /// </summary>
        /// <param name="m">Die Daten zur aktuellen Benutzereingabe.</param>
        private void ForwardOSDMessage( ref Message m )
        {
            // Check operation
            switch (m.Msg)
            {
                case 0x0101: directShow.ProcessKey( (Keys) m.WParam ); break;
                case 0x0202: directShow.ProcessKey( Keys.LButton ); break;
                case 0x0205: directShow.ProcessKey( Keys.RButton ); break;
            }
        }
    }
}