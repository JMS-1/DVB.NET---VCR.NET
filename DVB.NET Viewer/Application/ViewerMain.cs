using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using DVBNETViewer.Dialogs;
using JMS.DVB;
using JMS.DVB.DirectShow;
using JMS.DVB.Viewer;


namespace DVBNETViewer
{
    /// <summary>
    /// Hauptfenster des DVB.NET Viewers zur Steuerung eines BDA DirectShow Graphen
    /// für die Anzeige und einer Senderlistenverwaltung mit Favoritenfunktionalität.
    /// </summary>
    public partial class ViewerMain : Form, IChannelInfo, IRemoteInfo, ILocalInfo, IStreamInfo, IGeneralInfo
    {
        /// <summary>
        /// Zeigt kurz die Maus an.
        /// </summary>
        private class _Cursor : IDisposable
        {
            /// <summary>
            /// Erzeugt eine neue Steuerung.
            /// </summary>
            public _Cursor()
            {
                // Show the cursor            
                if (Properties.Settings.Default.HideCursor)
                    Cursor.Show();
            }

            /// <summary>
            /// Verbirgt die Maus wieder.
            /// </summary>
            public void Dispose()
            {
                // Back to normal
                if (Properties.Settings.Default.HideCursor)
                    Cursor.Hide();
            }
        }

        /// <summary>
        /// Wird während der Startphase und Reinitialisierung aktiviert.
        /// </summary>
        private bool m_Starting = true;

        /// <summary>
        /// Operationsmodus der Anwendung.
        /// </summary>
        private StartupModes m_Mode;

        /// <summary>
        /// Parmeter für den Operationsmodus.
        /// </summary>
        private string[] m_Arguments;

        /// <summary>
        /// Vorgegebener Name des VCR.NET Servers im Protokollmodus.
        /// </summary>
        private string m_FixedServer = null;

        /// <summary>
        /// Das zu verwendende Geräteprofil.
        /// </summary>
        private Profile Profile;

        /// <summary>
        /// Der Name eines Geräteprofils.
        /// </summary>
        private string m_Profile;

        /// <summary>
        /// Erzeugt das Hauptfenster, wenn die Anwendung ohne Zugriff auf die
        /// lokale Hardware gestartet wird.
        /// </summary>
        /// <param name="mode">Operationsmodus der Anwendung.</param>
        /// <param name="args">Parameter zum Operationsmodus der Anwendung.</param>
        public ViewerMain( StartupModes mode, params string[] args )
            : this( null, mode, args )
        {
        }

        /// <summary>
        /// Erzeugt das Hauptfenster im DVB.NET Modus mit direktem Zugriff auf
        /// eine lokale Hardware.
        /// </summary>
        /// <param name="profile">Die zu verwendene lokale DVB.NET Hardware.</param>
        public ViewerMain( Profile profile )
            : this( profile, StartupModes.LocalDVB )
        {
        }

        /// <summary>
        /// Erzeugt as Hauptfenster.
        /// </summary>
        /// <param name="profile">Die zu verwendene lokale DVB.NET Hardware.</param>
        /// <param name="mode">Operationsmodus der Anwendung.</param>
        /// <param name="args">Parameter zum Operationsmodus der Anwendung.</param>
        public ViewerMain( Profile profile, StartupModes mode, params string[] args )
        {
            // Remember
            m_Arguments = args;
            Profile = profile;
            m_Mode = mode;

            // Create components
            InitializeComponent();

            // Attach to viewer
            IViewerSite viewer = (IViewerSite) theViewer;

            // Register additional keys - to be kept we must do this before SetSite fixes the map
            viewer.SetKeyHandler( Keys.J, ProcessFullScreen );
            viewer.SetKeyHandler( Keys.End, Close );

            // Connect viewer control to configuration
            theViewer.SetSite( this );

            // Prepare to show
            SetBounds();
        }

        /// <summary>
        /// Positioniert das Hauptfenster der Anwendung.
        /// </summary>
        private void SetBounds()
        {
            // Remember current mode
            bool starting = m_Starting;

            // With reset
            try
            {
                // Avoid messing with the saved bounds 
                m_Starting = true;

                // Check mode
                if (Properties.Settings.Default.FullScreen)
                {
                    // Manual position
                    StartPosition = FormStartPosition.Manual;

                    // Full screen simulation
                    FormBorderStyle = FormBorderStyle.None;
                    Bounds = Screen.PrimaryScreen.Bounds;
                    ControlBox = false;
                    TopMost = true;
                }
                else
                {
                    // Get bounds
                    Rectangle rect = Properties.Settings.Default.Location;

                    // Overwrite
                    if ((rect.Width > 0) && (rect.Height > 0))
                    {
                        // Manual position
                        StartPosition = FormStartPosition.Manual;

                        // Force
                        DesktopBounds = rect;
                    }

                    // Disable full screen simulation
                    FormBorderStyle = FormBorderStyle.Sizable;
                    ControlBox = true;
                    TopMost = false;
                }
            }
            finally
            {
                // Reset
                m_Starting = starting;
            }
        }

        /// <summary>
        /// Durch den Anwender angeforderte Umschaltung des Vollbildmodus.
        /// </summary>
        private void ProcessFullScreen()
        {
            // Change
            Properties.Settings.Default.FullScreen = !Properties.Settings.Default.FullScreen;
            Properties.Settings.Default.Save();

            // Attach to viewer
            IOSDSite osd = (IOSDSite) theViewer;

            // Hide any overlay
            osd.Hide();

            // Put us in position
            SetBounds();
        }

        /// <summary>
        /// Startet die Anzeige der Anwendung.
        /// <seealso cref="ReloadChannels"/>
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ViewerMain_Load( object sender, EventArgs e )
        {
            // Finish
            if (Properties.Settings.Default.HideCursor)
                Cursor.Hide();

            // Ask for configuration
            if (Properties.Settings.Default.FirstStart)
            {
                // Show configuration
                if (!ShowGlobalOptions( null ))
                {
                    // Done
                    Close();

                    // Leave
                    return;
                }

                // Not again
                Properties.Settings.Default.FirstStart = false;
                Properties.Settings.Default.Save();
            }

            // Delay start streaming
            tickStart.Enabled = true;

            // Make us active
            Activate();
        }

        /// <summary>
        /// Aktiviert den Datenstrom leicht zeitverzögert, so dass der Viewer in jedem Fall
        /// angezeigt wird.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void tickStart_Tick( object sender, EventArgs e )
        {
            // Once only
            tickStart.Enabled = false;

            // Remember
            string oldText = Text, newText = oldText + Properties.Resources.Connecting;

            // Show progress
            Text = newText;

            // Make sure that text is shown
            Application.DoEvents();

            // The adaptor to use
            Adaptor adaptor;

            // Check mode
            switch (m_Mode)
            {
                case StartupModes.LocalDVB: adaptor = new DeviceAdpator( Profile, theViewer ); break;
                case StartupModes.RemoteVCR: adaptor = new VCRAdaptor( theViewer ); break;
                case StartupModes.ConnectTCP: adaptor = new SlaveAdaptor( theViewer, m_Arguments[0] ); break;
                case StartupModes.PlayLocalFile: adaptor = new FileAdaptor( theViewer, m_Arguments[0] ); break;
                case StartupModes.PlayRemoteFile: m_FixedServer = m_Arguments[1]; adaptor = new VCRAdaptor( theViewer, m_Arguments[0] ); break;
                case StartupModes.WatchOrTimeshift: adaptor = CreateWatch( m_Arguments[0].Split( '/' ) ); break;
                default: throw new ArgumentException( m_Mode.ToString(), "m_Mode" );
            }

            // Startup control
            theViewer.Initialize( adaptor );

            // Create configuration list
            ResetOptions();

            // Reset title
            if (Equals( Text, newText ))
                Text = oldText;

            // Up and running
            m_Starting = false;
        }

        /// <summary>
        /// Erzeugt einen Adaptor zum Betrachten einer aktuellen Aufzeichnung - dieser
        /// Aufruf wird über das <i>dvbnet://</i> Protokoll angestossen.
        /// </summary>
        /// <param name="parts">URL Teile nach dem Protokollnamen.</param>
        /// <returns>Ein geeignet konfigurierter Adaptor.</returns>
        private Adaptor CreateWatch( string[] parts )
        {
            // Remember
            m_FixedServer = parts[0];
            m_Profile = parts[1];

            // Create
            return new VCRAdaptor( theViewer, int.Parse( parts[2] ), 0 == string.Compare( parts[3], "TimeShift", true ) );
        }

        /// <summary>
        /// Zeigt die Konfiguration der Senderverwaltung, nachdem der Vollbildmodus 
        /// deaktiviert wurde.
        /// </summary>
        private void ShowFavoriteSettings()
        {
            // Show cursor
            using (ShowCursor())
            {
                // Attach to viewer
                IViewerSite viewer = (IViewerSite) theViewer;

                // Show configuration dialog
                viewer.FavoriteManager.ShowConfiguration( this );
            }
        }

        /// <summary>
        /// Lädt die Liste der Konfigurationsoptionen neu.
        /// </summary>
        public void ResetOptions()
        {
            // Attach to viewer
            IViewerSite viewer = (IViewerSite) theViewer;

            // Wipe out
            viewer.ResetOptions();

            // Fill configuration
            viewer.AddOption( new OptionDisplay( Properties.Resources.OptionGlobalSettings, () => ShowGlobalOptions( viewer ) ) );
            viewer.AddOption( new OptionDisplay( Properties.Resources.OptionFavorites, ShowFavoriteSettings ) );

            // Be safe
            try
            {
                // Fill conditional configuration
                viewer.FillOptions();
            }
            catch (Exception ex)
            {
                // Report
                viewer.ShowMessage( ex.Message, Properties.Resources.ErrorTitle, false );
            }
        }

        /// <summary>
        /// Vermerkt veränderung an Größe und Position der Anwendung in den Benutzereinstellungen,
        /// so dass die Anwendung beim nächsten Start genau so angezeigt werden kann.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ViewerMain_SizeChanged( object sender, EventArgs e )
        {
            // Do not update while starting
            if (m_Starting) return;

            // Never in fullscreen mode
            if (Properties.Settings.Default.FullScreen) return;

            // Load bounds
            Rectangle rect = DesktopBounds;

            // Correct
            if (rect.Left < 0) rect.Offset( -rect.Left, 0 );
            if (rect.Top < 0) rect.Offset( 0, -rect.Top );

            // Remember all
            Properties.Settings.Default.Location = rect;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Zeigt die Einstellungen der Anwendung an.
        /// </summary>
        /// <param name="viewer">Die zugehörige Darstellungsinstanz.</param>
        private bool ShowGlobalOptions( IViewerSite viewer )
        {
            // Show configuration dialog
            using (ShowCursor())
            using (ProgramSettings dialog = new ProgramSettings( viewer ))
                if (DialogResult.OK == dialog.ShowDialog( this ))
                {
                    // Update
                    Properties.Settings.Default.Save();

                    // Change priority
                    Process.GetCurrentProcess().PriorityClass = Properties.Settings.Default.Priority;

                    // Done on first call
                    if (null == viewer) return true;

                    // Load mode
                    ProgramSettings.ChangeTypes changeType = dialog.ChangeType;

                    // Update
                    if (ProgramSettings.ChangeTypes.Picture == changeType)
                        if (!viewer.CanRestartGraph)
                            changeType = ProgramSettings.ChangeTypes.Application;

                    // Check mode
                    switch (changeType)
                    {
                        case ProgramSettings.ChangeTypes.Application:
                            {
                                // Report to user
                                MessageBox.Show( this, Properties.Resources.RequireRestart, Properties.Resources.OptionGlobalSettings );

                                // We can do no more
                                break;
                            }
                        case ProgramSettings.ChangeTypes.Picture:
                            {
                                // Switch off OSD
                                IOSDSite osd = (IOSDSite) theViewer;
                                osd.Hide();

                                // Restart picture
                                viewer.Restart();

                                // Done
                                break;
                            }
                    }

                    // Accepted
                    return true;
                }

            // Not accepted
            return false;
        }

        /// <summary>
        /// Zeigt die Maus an.
        /// </summary>
        /// <returns>Verbirgt die Maus wieder, wenn die Operation abgeschlossen ist.</returns>
        public IDisposable ShowCursor()
        {
            // Create helper
            return new _Cursor();
        }


        #region IChannelInfo Members

        /// <summary>
        /// Gesetzt, wenn Radiosender in der Senderliste erscheinen sollen.
        /// </summary>
        bool IChannelInfo.UseRadio
        {
            get
            {
                // Forward
                return Properties.Settings.Default.UseRadio;
            }
        }

        /// <summary>
        /// Gesetzt, wenn verschlüsselte Sender in der Senderliste erscheinen sollen.
        /// </summary>
        bool IChannelInfo.PayTV
        {
            get
            {
                // Forward
                return Properties.Settings.Default.PayTV;
            }
        }

        /// <summary>
        /// Gesetzt, wenn unverschlüsselte Sender in der Senderliste erscheinen sollen.
        /// </summary>
        bool IChannelInfo.FreeTV
        {
            get
            {
                // Forward
                return Properties.Settings.Default.FreeTV;
            }
        }

        /// <summary>
        /// Gesetzt, wenn Fernsehsender in der Senderliste erscheinen sollen.
        /// </summary>
        bool IChannelInfo.UseTV
        {
            get
            {
                // Forward
                return Properties.Settings.Default.UseTV;
            }
        }

        #endregion

        #region ILocalInfo

        /// <summary>
        /// Liest oder verändert den Namen des zuletzt verwendeten Senders bei Zugriff auf eine lokale
        /// DVB.NET Hardware.
        /// </summary>
        string ILocalInfo.LocalStation
        {
            get
            {
                // Forward
                return Properties.Settings.Default.LocalStation;
            }
            set
            {
                // Change & save
                Properties.Settings.Default.LocalStation = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Liest oder verändert den Namen der zuletzt verwendeten Tonspur bei Zugriff auf
        /// eine lokale DVB.NET Hardware.
        /// </summary>
        string ILocalInfo.LocalAudio
        {
            get
            {
                // Forward
                return Properties.Settings.Default.LocalAudio;
            }
            set
            {
                // Change & save
                Properties.Settings.Default.LocalAudio = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Liest oder setzt das Aufzeichnungsverzeichnis bei Verwendung einer lokalen
        /// DVB.NET Hardware.
        /// </summary>
        string ILocalInfo.RecordingDirectory
        {
            get
            {
                // Forward
                return Properties.Settings.Default.RecordingDirectory;
            }
            set
            {
                // Change & save
                Properties.Settings.Default.RecordingDirectory = value;
                Properties.Settings.Default.Save();
            }
        }

        #endregion

        #region IRemoteInfo

        /// <summary>
        /// Liest oder verändert den Namen des zuletzt verwendeten Senders im
        /// VCR.NET LIVE Modus.
        /// </summary>
        string IRemoteInfo.VCRStation
        {
            get
            {
                // Forward
                return Properties.Settings.Default.VCRStation;
            }
            set
            {
                // Change & save
                Properties.Settings.Default.VCRStation = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Liest oder verändert den Namen der zuletzt verwendeten Tonspur im
        /// VCR.NET LIVE Modus.
        /// </summary>
        string IRemoteInfo.VCRAudio
        {
            get
            {
                // Forward
                return Properties.Settings.Default.VCRAudio;
            }
            set
            {
                // Change & save
                Properties.Settings.Default.VCRAudio = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Liest oder verändert das zuletzt verwendete VCR.NET Geräteprofil.
        /// </summary>
        string IRemoteInfo.VCRProfile
        {
            get
            {
                // Override is active
                if (null != m_Profile)
                    return m_Profile;
                else
                    return Properties.Settings.Default.VCRProfile;
            }
            set
            {
                // Deactive override
                m_Profile = null;

                // Change & save
                Properties.Settings.Default.VCRProfile = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Meldet die <see cref="Uri"/> zum VCR.NET Recording Service.
        /// </summary>
        Uri IRemoteInfo.ServerUri
        {
            get
            {
                // Override is active
                if (null != m_FixedServer) return new Uri( string.Format( "http://{0}/VCR.NET/VCRServer.asmx", m_FixedServer ) );

                // Forward
                return new Uri( Properties.Settings.Default.DVBNETViewer_FullServer_VCR30Server );
            }
        }

        #endregion

        #region IStreamInfo

        /// <summary>
        /// Meldet die Netzwerkadresse für den Versand eines Transport Streams. Es
        /// muss sich um eine TCP/IP Multicast Adresse handeln.
        /// </summary>
        /// <remarks>
        /// Diese Adresse wird in zwei Fällen verwendet. Im Betrieb mit einer lokalen
        /// DVB.NET Hardware wird jede Aufzeichnung an diese Adresse (mit dem TCP/IP Port
        /// <see cref="IStreamInfo.BroadcastPort"/>) versendet. Bei Verbindung mit
        /// einem VCR.NET Recording Service wird dieser aufgefordert, die Transport
        /// Stream an die angegebene Adresse zu senden. Im LIVE Modus oder bei Betrachtung
        /// einer laufende Aufzeichnungen können andere Empfänger so mithören.
        /// </remarks>
        string IStreamInfo.BroadcastIP
        {
            get
            {
                // Forward
                return Properties.Settings.Default.BroadcastIP;
            }
        }

        /// <summary>
        /// TCP/IP Port, an dem ein Transport Stream entgegen genommen wird - bei
        /// Verwendung einer lokalen DVB.NET Hardware ist dies ein Teil der Zieladresse,
        /// an die eine Aufzeichnungsdatei geschickt wird.
        /// </summary>
        ushort IStreamInfo.BroadcastPort
        {
            get
            {
                // Forward
                return Properties.Settings.Default.BroadcastPort;
            }
        }

        #endregion

        #region IGeneralInfo Members

        /// <summary>
        /// Meldet die maximal erlaubte Anzeigezeit für ein OSD in Sekunden.
        /// </summary>
        int IGeneralInfo.OSDLifeTime
        {
            get
            {
                // Report
                return Properties.Settings.Default.OSDLifeTime;
            }
        }

        /// <summary>
        /// Liest oder setzt die aktuelle Lautstärke in % von 0.0 bis 1.0.
        /// </summary>
        double IGeneralInfo.Volume
        {
            get
            {
                // Report
                return Properties.Settings.Default.Volume;
            }
            set
            {
                // Change & Save
                Properties.Settings.Default.Volume = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Deaktiviert den Vollbildmodus.
        /// </summary>
        void IGeneralInfo.LeaveFullScreen()
        {
            // Forward
            LeaveFullScreen();
        }

        /// <summary>
        /// Deaktiviert den Vollbildmodus.
        /// </summary>
        private void LeaveFullScreen()
        {
            // Nothing to do
            if (!Properties.Settings.Default.FullScreen) return;

            // Leave fullscreen mode
            ProcessFullScreen();

            // Wait a bit
            Application.DoEvents();
        }


        /// <summary>
        /// Meldet, ob der Cyberlink / PowerDVD Codec für H.264 aktiviert werden soll.
        /// </summary>
        bool IGeneralInfo.UseCyberlinkCodec { get { return Properties.Settings.Default.UseCyberlinkCodec; } }

        bool IGeneralInfo.UseRemoteControl { get { return Properties.Settings.Default.UseRemote; } }

        string IGeneralInfo.H264Decoder { get { return Properties.Settings.Default.H264Decoder; } }

        string IGeneralInfo.MPEG2Decoder { get { return Properties.Settings.Default.MPEG2Decoder; } }

        string IGeneralInfo.AC3Decoder { get { return Properties.Settings.Default.AC3Decoder; } }

        string IGeneralInfo.MP2Decoder { get { return Properties.Settings.Default.MP2Decoder; } }

        int IGeneralInfo.AVDelay { get { return Properties.Settings.Default.AVDelay; } }

        void IGeneralInfo.SetPictureParameters( PictureParameters parameters )
        {
            // Copy over if necessary
            if (Properties.Settings.Default.OverwriteVideoSettings)
            {
                // Set all
                SetVideoParameter( parameters.Brightness, Properties.Settings.Default.VideoBrightness );
                SetVideoParameter( parameters.Saturation, Properties.Settings.Default.VideoSaturation );
                SetVideoParameter( parameters.Hue, Properties.Settings.Default.VideoHue );
                SetVideoParameter( parameters.Contrast, Properties.Settings.Default.VideoContrast );
            }
        }

        private void SetVideoParameter( PictureParameters.ParameterSet parameter, float value )
        {
            // Store but keep in range
            parameter.Value = Math.Max( parameter.Minimum, Math.Min( parameter.Maximum, value ) );
        }

        void IGeneralInfo.SetWindowTitle( string title )
        {
            // Do as requested
            Text = title;
        }

        #endregion
    }
}