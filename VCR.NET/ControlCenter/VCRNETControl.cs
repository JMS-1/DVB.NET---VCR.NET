using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;


namespace VCRControlCenter
{
    /// <summary>
    /// Die farbliche Hinterlegung des Symbols im <i>System Tray</i> von Windows
    /// beschreibt den Zustand des jeweiligen VCR.NET Recording Service.
    /// </summary>
    public enum TrayColors
    {
        /// <summary>
        /// Der Zustand wurde noch nicht abgefragt.
        /// </summary>
        Unknown,

        /// <summary>
        /// Zumindest ein DVB.NET Geräteprofil ist in Benutzung.
        /// </summary>
        Blue,

        /// <summary>
        /// Kein DVB.NET Geräteprofil ist in Benutzung aber es gibt geplante Aufzeichnungen,
        /// von denen keine in den nächsten 5 Minuten beginnt.
        /// </summary>
        Green,

        /// <summary>
        /// Der aktuelle Zustand des VCR.NET Recording Service konnte nicht ermittelt werden.
        /// </summary>
        Red,

        /// <summary>
        /// Kein DVB.NET Geräteprofil ist in Benutzung und es gibt auch keine geplante Aufzeichnungen.
        /// </summary>
        Standard,

        /// <summary>
        /// Kein DVB.NET Geräteprofil ist in Benutzung aber es gibt geplante Aufzeichnungen,
        /// von denen mindestens eine in den nächsten 5 Minuten beginnt.
        /// </summary>
        Yellow
    };

    public partial class VCRNETControl : Form
    {
        private class CultureItem
        {
            public readonly CultureInfo Info;

            public CultureItem( CultureInfo info )
            {
                // Remember
                Info = info;
            }

            public override string ToString()
            {
                // Report
                return Info.NativeName;
            }
        }

        private class StreamInfo
        {
            public readonly Guid UniqueIdentifier;
            public readonly bool Disconnect;
            public readonly string Station;
            public readonly string Profile;
            public readonly int Index;
            public readonly int Port;

            public StreamInfo( string profile, int port, string station, Guid uniqueIdentifier, int index )
            {
                // Remember
                UniqueIdentifier = uniqueIdentifier;
                Disconnect = (port < 0);
                Station = station;
                Profile = profile;
                Index = index;
                Port = port;
            }
        }

        private static int[] m_HibernateDelays = { 120, 90, 60, 45, 30, 15, 10, 5, -5, -10, -15, -30, -45, -60, -90, -120 };

        private static object m_FileLock = new object();

        private Dictionary<TrayColors, Icon> m_TrayIcons = new Dictionary<TrayColors, Icon>();
        private Properties.Settings m_Settings = Properties.Settings.Default;
        private List<NotifyIcon> m_Icons = new List<NotifyIcon>();
        private DateTime m_BlockHibernate = DateTime.MaxValue;
        private HibernateDialog m_Hibernation = null;
        private bool m_QueryEndSession = false;
        private NotifyIcon m_Active = null;
        private Image m_Connected;
        private string m_TopMenu;

        public VCRNETControl( string[] args )
        {
            // Report
            Log( "VCC [2013/06/01] starting up" );

            // Autostart service
            if (m_Settings.AutoStartService)
                if (!ThreadPool.QueueUserWorkItem( StartService ))
                    StartService( null );

            // This is us
            Type me = GetType();

            // Pattern start
            string prefix = me.Namespace + ".TrayIcons.";
            string suffix = ".ICO";

            // Load tray icons
            foreach (string resname in me.Assembly.GetManifestResourceNames())
                if (resname.StartsWith( prefix ))
                    if (resname.EndsWith( suffix ))
                    {
                        // Icon
                        string iconname = resname.Substring( prefix.Length, resname.Length - prefix.Length - suffix.Length );

                        // Load
                        using (Stream stream = me.Assembly.GetManifestResourceStream( resname ))
                        {
                            // Load the icon
                            Icon icon = new Icon( stream );

                            // Check for debugger
                            if (Debugger.IsAttached)
                                using (var bmp = icon.ToBitmap())
                                {
                                    // Change
                                    using (var gc = Graphics.FromImage( bmp ))
                                    using (var color = new SolidBrush( Color.Black ))
                                    {
                                        // Paint
                                        gc.FillRectangle( color, 0, 15, bmp.Width, 5 );
                                    }

                                    // Free icon
                                    icon.Dispose();

                                    // Reload
                                    icon = Icon.FromHandle( bmp.GetHicon() );
                                }

                            // Get the related color
                            var colorIndex = (TrayColors) Enum.Parse( typeof( TrayColors ), iconname, true );

                            // Add to map
                            m_TrayIcons[colorIndex] = icon;
                        }
                    }

            // Load picture
            using (var stream = me.Assembly.GetManifestResourceStream( me.Namespace + ".Icons.Connected.ico" ))
                m_Connected = Image.FromStream( stream );

            // Startup
            InitializeComponent();

            // Load the top menu string
            m_TopMenu = mnuDefault.Text;

            // Correct IDE problems (destroyed when language changes)
            selHibernate.Value = 5;
            selHibernate.Minimum = 1;
            selHibernate.Maximum = 30;
            selInterval.Value = 10;
            selInterval.Minimum = 5;
            selInterval.Maximum = 300;
            selPort.Maximum = ushort.MaxValue;
            selPort.Value = 80;
            selPort.Minimum = 1;
            selStreamPort.Maximum = ushort.MaxValue;
            selStreamPort.Value = 2910;
            selStreamPort.Minimum = 1;
            lstServers.View = View.Details;
            errorMessages.SetIconAlignment( txArgs, ErrorIconAlignment.MiddleLeft );
            errorMessages.SetIconAlignment( txMultiCast, ErrorIconAlignment.MiddleLeft );

            // Make sure that handle exists
            CreateHandle();

            // Load servers
            if (null != m_Settings.Servers)
                foreach (var setting in m_Settings.Servers)
                    lstServers.Items.Add( setting.View );

            // Load settings
            ckAutoStart.Checked = m_Settings.AutoStartService;
            ckHibernate.Checked = (m_Settings.HibernationDelay > 0);
            selHibernate.Value = ckHibernate.Checked ? (int) m_Settings.HibernationDelay : 5;
            txMultiCast.Text = m_Settings.MulticastIP;
            selStreamPort.Value = m_Settings.MinPort;
            selDelay.Value = m_Settings.StartupDelay;
            txArgs.Text = m_Settings.ViewerArgs;
            txViewer.Text = m_Settings.Viewer;

            // The default
            CultureItem selectedItem = null;

            // Fill language list
            foreach (var info in CultureInfo.GetCultures( CultureTypes.NeutralCultures ))
            {
                // Skip all sub-languages
                if (info.NativeName.IndexOf( '(' ) >= 0) continue;

                // Create
                var item = new CultureItem( info );

                // Add to map
                selLanguage.Items.Add( item );

                // Remember default
                if (Equals( m_Settings.Language, info.TwoLetterISOLanguageName )) selectedItem = item;
            }

            // Copy over
            selLanguage.SelectedItem = selectedItem;

            // Update GUI
            CheckLocal();
            CheckStream();

            // No local service
            frameLocal.Enabled = false;

            // See if local service exists
            try
            {
                // Attach to registry
                using (var vcr = Registry.LocalMachine.OpenSubKey( @"SYSTEM\CurrentControlSet\Services\VCR.NET Service" ))
                    if (null != vcr)
                    {
                        // Load path
                        string image = (string) vcr.GetValue( "ImagePath" );
                        if (!string.IsNullOrEmpty( image ))
                        {
                            // Correct
                            if (image.StartsWith( "\"" ) && image.EndsWith( "\"" ) && (image.Length > 1))
                            {
                                // Remove quotes
                                image = image.Substring( 1, image.Length - 2 ).Replace( "\"\"", "\"" );
                            }
                        }

                        // Attach to configuration file
                        var config = new FileInfo( image + ".config" );
                        if (config.Exists)
                        {
                            // Load DOM
                            XmlDocument dom = new XmlDocument();
                            dom.Load( config.FullName );

                            // Check for port
                            var portNode = (XmlElement) dom.SelectSingleNode( "configuration/appSettings/add[@key='TCPPort']" );
                            var port = ushort.Parse( portNode.GetAttribute( "value" ) );

                            // At least there is a valid local server
                            frameLocal.Enabled = true;

                            // Check for defaulting
                            if (lstServers.Items.Count < 1)
                            {
                                // Create a new entry
                                var settings =
                                    new PerServerSettings
                                    {
                                        ServerName = "localhost",
                                        RefreshInterval = 10,
                                        RunExtensions = true,
                                        ServerPort = port,
                                    };

                                // Add
                                m_Settings.Servers = new PerServerSettings[] { settings };

                                // Try add
                                try
                                {
                                    // Save
                                    m_Settings.Save();

                                    // Add to list
                                    lstServers.Items.Add( settings.View );
                                }
                                catch
                                {
                                    // Reset
                                    m_Settings.Servers = null;
                                }
                            }
                        }
                    }
            }
            catch
            {
                // Ignore any error
            }

            // Install tray icons
            CreateTrayIcons();

            // Prepare buttons
            lstServers_SelectedIndexChanged( lstServers, EventArgs.Empty );
        }

        private PerServerSettings CurrentServer
        {
            get
            {
                // Nothing to do
                if (null == m_Active) return null;

                // Get the index of the icon clicked
                int index = m_Icons.IndexOf( m_Active );
                if (index < 0) return null;

                // Validate
                if (index >= lstServers.Items.Count) return null;

                // Attach to the related server view
                var view = (PerServerSettings.PerServerView) lstServers.Items[index];

                // Report server
                return view.Settings;
            }
        }

        private void CreateTrayIcons()
        {
            // Disable timer
            tickProcess.Enabled = false;

            // Disable hibernation
            HideHibernation();

            // Reset all
            for (int n = components.Components.Count; n-- > 0; )
            {
                // Read it
                NotifyIcon icon = components.Components[n] as NotifyIcon;
                if (null == icon) continue;

                // Shutdown
                icon.Dispose();

                // Forget
                components.Remove( icon );
            }

            // Clear internal state
            m_Icons.Clear();
            m_Active = null;

            // Check mode
            if (lstServers.Items.Count < 1)
            {
                // Create dummy
                NotifyIcon icon = CreateTrayIcon( Properties.Resources.NoServers );

                // Set color
                SetNotifyIcon( icon, TrayColors.Red );

                // Register double click 
                icon.DoubleClick += mnuSettings_Click;
            }
            else
            {
                // All servers
                foreach (PerServerSettings.PerServerView view in lstServers.Items)
                {
                    // Force fast recheck
                    view.ResetProcessing();

                    // Attach to server
                    PerServerSettings server = view.Settings;

                    // Create
                    NotifyIcon icon = CreateTrayIcon( string.Format( Properties.Resources.ServerTrayText, server.ServerName, server.ServerPort ) );

                    // Register double click
                    icon.DoubleClick += mnuDefault_Click;
                }

                // Enable timer
                tickProcess.Enabled = true;
            }
        }

        private NotifyIcon CreateTrayIcon( string text )
        {
            // Create new
            NotifyIcon icon = new NotifyIcon( components );

            // Remember
            m_Icons.Add( icon );

            // Configure
            icon.ContextMenuStrip = trayMenu;
            icon.Visible = true;
            icon.Text = text;

            // Time to set the icon
            SetNotifyIcon( icon, TrayColors.Standard );

            // Connect events
            icon.MouseDown += SetActiveTray;

            // Report
            return icon;
        }

        private void SetActiveTray( object sender, MouseEventArgs e )
        {
            // Remember
            m_Active = (NotifyIcon) sender;
        }

        private void mnuClose_Click( object sender, EventArgs e )
        {
            // Terminate application loop
            Application.Exit();
        }

        protected override void OnClosing( CancelEventArgs e )
        {
            // Check mode
            if (!m_QueryEndSession)
            {
                // Abort
                e.Cancel = true;

                // Hide self
                Hide();
            }

            // Base
            base.OnClosing( e );
        }

        /// <summary>
        /// Wird beim Öffnen des Kontextmenüs aktiviert und füllt die dynamischen Menüeinträge.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void trayMenu_Opening( object sender, CancelEventArgs e )
        {
            // Reset
            mnuDefault.Text = m_TopMenu;

            // Check mode
            var hasHibernate = (m_Settings.HibernationDelay > 0);
            var hasServers = (lstServers.Items.Count > 0);

            // Enable/disable all
            mnuHibernateSep.Visible = hasHibernate;
            mnuHibernate.Visible = hasHibernate;
            mnuOpenJobList.Enabled = hasServers;
            mnuLiveConnect.Enabled = hasServers;
            mnuCurrent.Enabled = hasServers;
            mnuDefault.Enabled = hasServers;
            mnuNewJob.Enabled = hasServers;
            mnuAdmin.Enabled = hasServers;
            mnuEPG.Enabled = hasServers;

            // Update fonts
            mnuSettings.Font = new Font( mnuSettings.Font, hasServers ? FontStyle.Regular : FontStyle.Bold );
            mnuDefault.Font = new Font( mnuDefault.Font, hasServers ? FontStyle.Bold : FontStyle.Regular );

            // Reset watch list
            mnuLiveConnect.DropDownItems.Clear();
            mnuCurrent.DropDownItems.Clear();

            // See if there are servers configured
            if (hasServers)
            {
                // Find the server
                PerServerSettings settings = CurrentServer;
                if (null != settings)
                {
                    // Change top menu
                    mnuDefault.Text += string.Format( " ({0})", settings.ServerName );

                    // No viewer
                    if (!string.IsNullOrEmpty( m_Settings.Viewer ))
                    {
                        // See if this is the DVB.NET / VCR.NET Viewer or an equivalent application
                        bool usesViewer = (!string.IsNullOrEmpty( m_Settings.ViewerArgs ) && m_Settings.ViewerArgs.Contains( "{2}" ));

                        // All profiles
                        foreach (ProfileInfo profile in settings.View.Profiles)
                        {
                            // Attach to the last known recording
                            var current = profile.CurrentRecordings;
                            if (current == null)
                            {
                                // Only if we can use the dvbnet:// protocol
                                if (usesViewer)
                                {
                                    // Create the startup agrument for the DVB.NET Viewer
                                    Uri uri = new Uri( settings.EndPoint );

                                    // Add template
                                    ToolStripItem live = mnuLiveConnect.DropDownItems.Add( profile.Profile );

                                    // Attach recording information
                                    live.Tag = string.Format( "dvbnet://*{0}/{1}/0/Live", uri.Authority, Uri.EscapeDataString( profile.Profile ) );

                                    // Connect event
                                    live.Click += LiveConnect;
                                }

                                // Next
                                continue;
                            }

                            // Port counter
                            var port = m_Settings.MinPort;

                            // See if streaming is possible
                            foreach (var stream in current)
                                if (stream.streamIndex >= 0)
                                {
                                    // Add template
                                    var added = mnuCurrent.DropDownItems.Add( string.Format( Properties.Resources.StationFormat, stream.name, stream.device, string.IsNullOrEmpty( stream.streamTarget ) ? port.ToString() : stream.streamTarget ).Replace( "&", "&&" ) );

                                    // Attach management
                                    added.Image = string.IsNullOrEmpty( stream.streamTarget ) ? null : m_Connected;
                                    added.Click += StartStopStreaming;
                                    added.Tag = stream;

                                    // Next port
                                    stream.ReservedPort = port++;
                                }
                        }
                    }
                }
            }

            // Disable live connect
            mnuLiveConnect.Visible = (mnuLiveConnect.DropDownItems.Count > 0);

            // Hibernate control
            if (hasHibernate)
            {
                // Clear sub items from last call
                while (mnuHibernate.DropDownItems.Count > 1) mnuHibernate.DropDownItems.RemoveAt( 1 );

                // Get the reference time
                DateTime now = DateTime.UtcNow, zero = now;

                // Reset
                if (m_BlockHibernate < now) m_BlockHibernate = DateTime.MaxValue;

                // Check mode
                if (m_BlockHibernate < DateTime.MaxValue)
                {
                    // See if hibernate delay is active			
                    mnuHibernateReset.Visible = true;

                    // Set the text
                    mnuHibernateReset.Text = string.Format( Properties.Resources.HibernateReset, m_BlockHibernate.ToLocalTime() );

                    // Add separator
                    mnuHibernate.DropDownItems.Add( new ToolStripSeparator() );

                    // Use as reference time
                    zero = m_BlockHibernate;
                }
                else
                {
                    // Do not show
                    mnuHibernateReset.Visible = false;
                }

                // Show options
                foreach (int delay in m_HibernateDelays)
                {
                    // Get the time
                    DateTime check = zero.AddMinutes( delay );

                    // Not possible
                    if (check <= now) break;

                    // Create entry
                    ToolStripMenuItem item = new ToolStripMenuItem();

                    // Fill it
                    item.Text = string.Format( Properties.Resources.HibernateN, check.ToLocalTime() );
                    item.Click += new EventHandler( SetHibernateDelay );
                    item.Tag = check;

                    // Remember it
                    mnuHibernate.DropDownItems.Add( item );
                }
            }
        }

        /// <summary>
        /// Startet das aktuelle Anzeigeprogramm mit einem bestimmten Geräteprofil.
        /// </summary>
        /// <param name="sender">Der ausgelöste Menüpunkt.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void LiveConnect( object sender, EventArgs e )
        {
            // Be safe
            try
            {
                // Attach to menu item
                ToolStripItem menu = (ToolStripItem) sender;

                // Get the corresponding parameter
                string dvbnetURL = (string) menu.Tag;

                // Attach to server
                Process.Start( m_Settings.Viewer, dvbnetURL );
            }
            catch
            {
                // Ignore for now
            }
        }

        void SetHibernateDelay( object sender, EventArgs e )
        {
            // Convert
            ToolStripMenuItem item = (ToolStripMenuItem) sender;

            // Just update
            m_BlockHibernate = (DateTime) item.Tag;
        }

        private void OpenBrowser( string page )
        {
            // Attach to the related server
            var settings = CurrentServer;
            if (settings != null)
                Process.Start( string.Format( "{0}/{1}", settings.EndPoint, page ) );
        }

        private void mnuDefault_Click( object sender, EventArgs e )
        {
            // Open browser
            OpenBrowser( "default.html" );
        }

        private void mnuSettings_Click( object sender, EventArgs e )
        {
            // Show self
            Show();

            // To top
            BringToFront();
        }

        private void UpdateGUI()
        {
            // Forward
            UpdateGUI( null, EventArgs.Empty );
        }

        private PerServerSettings FindServer( string server )
        {
            // Process
            foreach (PerServerSettings.PerServerView view in lstServers.Items)
                if (0 == string.Compare( view.Settings.ServerName, server, true ))
                    return view.Settings;

            // Not found
            return null;
        }

        private void UpdateGUI( object sender, EventArgs e )
        {
            // Disable all
            cmdAdd.Enabled = false;
            cmdDelete.Enabled = false;
            cmdUpdate.Enabled = false;

            // Get the server name
            string serverName = txServer.Text;
            if (string.IsNullOrEmpty( serverName )) return;

            // Find server
            PerServerSettings server = FindServer( serverName );
            if (null == server)
            {
                // At least we can add it
                cmdAdd.Enabled = true;

                // Done
                return;
            }

            // At least we can delete it
            cmdDelete.Enabled = true;

            // See if this is an equal entry
            if (serverName.Equals( server.ServerName ))
                if (selInterval.Value == server.RefreshInterval)
                    if (selPort.Value == server.ServerPort)
                        if (ckExtensions.Checked == server.RunExtensions)
                            return;

            // And finally we could update
            cmdUpdate.Enabled = true;
        }

        private void cmdAdd_Click( object sender, EventArgs e )
        {
            // Create a brand new item
            PerServerSettings settings = new PerServerSettings();

            // Initialize it
            settings.ServerName = txServer.Text;
            settings.RefreshInterval = (int) selInterval.Value;
            settings.ServerPort = (ushort) selPort.Value;
            settings.RunExtensions = ckExtensions.Checked;

            // Update view
            settings.View.Refresh();

            // Create the update list
            List<PerServerSettings> list = new List<PerServerSettings>();

            // Existing
            foreach (PerServerSettings.PerServerView view in lstServers.Items)
                list.Add( view.Settings );

            // the new one
            list.Add( settings );

            // Old value
            PerServerSettings[] oldSettings = m_Settings.Servers;

            // Update
            m_Settings.Servers = list.ToArray();

            // Try save
            try
            {
                // Store to disk
                m_Settings.Save();

                // Unselect
                foreach (PerServerSettings.PerServerView view in lstServers.Items)
                    view.Selected = false;

                // Select
                settings.View.Selected = true;

                // Update in memory
                lstServers.Items.Add( settings.View );

                // Show
                settings.View.EnsureVisible();

                // Refresh icon list
                CreateTrayIcons();
            }
            catch (Exception ex)
            {
                // Back
                m_Settings.Servers = oldSettings;

                // Report
                MessageBox.Show( this, ex.Message, string.Format( Properties.Resources.AddServerTitle, settings.ServerName ) );
            }
        }

        private void lstServers_SelectedIndexChanged( object sender, EventArgs e )
        {
            // Single select only
            if (1 != lstServers.SelectedItems.Count)
            {
                // Reste
                txServer.Text = (lstServers.Items.Count < 1) ? "VCRServer" : null;
                selInterval.Value = 10;
                selPort.Value = 80;
                ckExtensions.Checked = true;
            }
            else
            {
                // Attach to the selected item
                PerServerSettings.PerServerView view = (PerServerSettings.PerServerView) lstServers.SelectedItems[0];
                PerServerSettings settings = view.Settings;

                // Load all
                txServer.Text = settings.ServerName;
                selPort.Value = settings.ServerPort;
                selInterval.Value = settings.RefreshInterval;
                ckExtensions.Checked = settings.RunExtensions;
            }

            // Refresh all
            UpdateGUI();
        }

        private void cmdDelete_Click( object sender, EventArgs e )
        {
            // Create a brand new item
            PerServerSettings settings = FindServer( txServer.Text );
            if (null == settings) return;

            // Create the update list
            List<PerServerSettings> list = new List<PerServerSettings>();

            // Existing
            foreach (PerServerSettings.PerServerView view in lstServers.Items)
                if (view.Settings != settings)
                    list.Add( view.Settings );

            // Old value
            PerServerSettings[] oldSettings = m_Settings.Servers;

            // Update
            m_Settings.Servers = list.ToArray();

            // Try save
            try
            {
                // Store to disk
                m_Settings.Save();

                // Update in memory
                lstServers.Items.Remove( settings.View );

                // Update GUI
                lstServers_SelectedIndexChanged( lstServers, EventArgs.Empty );

                // Refresh icon list
                CreateTrayIcons();
            }
            catch (Exception ex)
            {
                // Back
                m_Settings.Servers = oldSettings;

                // Report
                MessageBox.Show( this, ex.Message, string.Format( Properties.Resources.DelServerTitle, settings.ServerName ) );
            }
        }

        private void cmdUpdate_Click( object sender, EventArgs e )
        {
            // Create a brand new item
            PerServerSettings settings = FindServer( txServer.Text );
            if (null == settings) return;

            // Remember
            bool extensions = settings.RunExtensions;
            int interval = settings.RefreshInterval;
            string server = settings.ServerName;
            ushort port = settings.ServerPort;

            // Update
            settings.RefreshInterval = (int) selInterval.Value;
            settings.RunExtensions = ckExtensions.Checked;
            settings.ServerPort = (ushort) selPort.Value;
            settings.ServerName = txServer.Text;

            // Try save
            try
            {
                // Store to disk
                m_Settings.Save();

                // Update view
                settings.View.Refresh();

                // Update GUI
                lstServers_SelectedIndexChanged( lstServers, EventArgs.Empty );

                // Refresh icon list
                CreateTrayIcons();
            }
            catch (Exception ex)
            {
                // Back
                settings.RunExtensions = extensions;
                settings.RefreshInterval = interval;
                settings.ServerName = server;
                settings.ServerPort = port;

                // Report
                MessageBox.Show( this, ex.Message, string.Format( Properties.Resources.UpdateServerTitle, settings.ServerName ) );
            }
        }

        private void mnuAdmin_Click( object sender, EventArgs e )
        {
            // Jump in
            OpenBrowser( "default.html#admin" );
        }

        private void mnuOpenJobList_Click( object sender, EventArgs e )
        {
            // Jump in
            OpenBrowser( "default.html#plan" );
        }

        /// <summary>
        /// Ruft in der zugehörigen Web Anwendung die Anzeige der aktuellen Aufzeichnungen auf.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void mnuCurrent_Click( object sender, EventArgs e )
        {
            // Jump in
            OpenBrowser( "default.html#current" );
        }

        private void CheckLocal()
        {
            // Forward
            CheckLocal( null, EventArgs.Empty );
        }

        private void CheckLocal( object sender, EventArgs e )
        {
            // Disable / enable
            selHibernate.Enabled = ckHibernate.Checked;

            // Disable update
            cmdUpdateAll.Enabled = false;

            // Test all flags
            if (m_Settings.AutoStartService == ckAutoStart.Checked)
            {
                // Get the delay
                int delay = ckHibernate.Checked ? (int) selHibernate.Value : 0;

                // Compare
                if (m_Settings.HibernationDelay == delay) return;
            }

            // Can update
            cmdUpdateAll.Enabled = true;
        }

        private string CurrentLanguage
        {
            get
            {
                // Attach to selected language
                CultureItem culture = (CultureItem) selLanguage.SelectedItem;

                // Report
                return (null == culture) ? string.Empty : culture.Info.TwoLetterISOLanguageName;
            }
        }

        private void cmdUpdateAll_Click( object sender, EventArgs e )
        {
            // Old values
            bool autoStart = m_Settings.AutoStartService;
            int delay = m_Settings.HibernationDelay;

            // Update the all
            m_Settings.HibernationDelay = ckHibernate.Checked ? (int) selHibernate.Value : 0;
            m_Settings.AutoStartService = ckAutoStart.Checked;

            // Try save
            try
            {
                // Store to disk
                m_Settings.Save();

                // Did it
                cmdUpdateAll.Enabled = false;
            }
            catch (Exception ex)
            {
                // Back
                m_Settings.AutoStartService = autoStart;
                m_Settings.HibernationDelay = delay;

                // Report
                MessageBox.Show( this, ex.Message, Properties.Resources.UpdateTitle );
            }
        }

        private void selLanguage_SelectionChangeCommitted( object sender, EventArgs e )
        {
            // Old values
            string language = m_Settings.Language;

            // Update the all
            m_Settings.Language = CurrentLanguage;

            // Try save
            try
            {
                // Store to disk
                m_Settings.Save();
            }
            catch (Exception ex)
            {
                // Back
                m_Settings.Language = language;

                // Report
                MessageBox.Show( this, ex.Message, Properties.Resources.UpdateLanguage );
            }
        }

        private void tickProcess_Tick( object sender, EventArgs e )
        {
            // Report
            Log( "Timer elapsed" );

            // Check all servers
            foreach (PerServerSettings.PerServerView view in lstServers.Items)
            {
                // Attach to settings
                PerServerSettings settings = view.Settings;

                // Skip
                if (view.IsProcessing)
                {
                    // Report
                    Log( "{0}:{1} is still processing, last processed {2}", settings.ServerName, settings.ServerPort, view.LastProcessed );

                    // Next
                    continue;
                }

                // Calculate next processing time
                DateTime next = view.LastProcessed.AddSeconds( settings.RefreshInterval );

                // Not yet
                if (next >= DateTime.UtcNow)
                {
                    // Report
                    Log( "{0}:{1} will delay until {2}", settings.ServerName, settings.ServerPort, next );

                    // Next
                    continue;
                }

                // Create processing item
                ProcessingItem item = new ProcessingItem( view, this );

                // Run it
                if (ThreadPool.QueueUserWorkItem( item.CheckServer ))
                {
                    // Report
                    Log( "{0}:{1} processing enqueued", settings.ServerName, settings.ServerPort );
                }
                else
                {
                    // Report
                    Log( "{0}:{1} processing started manually", settings.ServerName, settings.ServerPort );

                    // Run manually
                    item.CheckServer( null );
                }
            }
        }

        private delegate bool ReportPattern( ProcessingItem controller, TrayColors state, bool mustHibernate, bool pendingExtensions );

        internal bool ProcessStateAndCheckHibernation( ProcessingItem controller, TrayColors state, bool mustHibernate, bool pendingExtensions )
        {
            // Load view
            var view = controller.View;

            // Report
            Log( "{0}:{1} report received for {2}", view.Settings.ServerName, view.Settings.ServerPort, state );

            // May be (normally) on the wrong thread
            if (InvokeRequired)
            {
                // Forward
                return (bool) Invoke( new ReportPattern( ProcessStateAndCheckHibernation ), controller, state, mustHibernate, pendingExtensions );
            }

            // Get the previous state
            TrayColors oldState = TrayColors.Unknown;

            // Find the view
            int index = lstServers.Items.IndexOf( view );
            if (index >= 0)
                if (index < m_Icons.Count)
                {
                    // Attach to the icon
                    var tray = m_Icons[index];

                    // Find
                    foreach (var test in m_TrayIcons)
                        if (ReferenceEquals( test.Value, tray.Icon ))
                        {
                            // Remember
                            oldState = test.Key;

                            // Stop
                            break;
                        }

                    // Set the new state
                    SetNotifyIcon( tray, state );
                }

            // Prepare next loop
            view.EndProcessing();

            // Process transition
            return GetHibernationDialogPoppedUp( controller, oldState, state, mustHibernate, pendingExtensions );
        }

        private bool GetHibernationDialogPoppedUp( ProcessingItem controller, TrayColors oldState, TrayColors newState, bool mustHibernate, bool pendingExtensions )
        {
            // Do the check
            bool? dialogState = GetHibernationDialogState( controller, oldState, newState, mustHibernate, pendingExtensions );

            // Just don't mess around with the hibernation dialog
            if (!dialogState.HasValue)
                return false;

            // Dialog should not be shown
            if (!dialogState.Value)
                HideHibernation();

            // Actually we feel responsible but optimize a bit to avoid too many callbacks
            return mustHibernate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        /// <param name="mustHibernate"></param>
        /// <param name="pendingExtensions"></param>
        /// <returns>Gesetzt, wenn der Dialog geöffnet wurde und inaktiv, wenn er geschlossen wurde. <i>null</i> zeigt an,
        /// dass die Kontrolle über den Schlafzustand nicht hier erfolgt.</returns>
        private bool? GetHibernationDialogState( ProcessingItem controller, TrayColors oldState, TrayColors newState, bool mustHibernate, bool pendingExtensions )
        {
            // Only if watching local server
            if (!frameLocal.Enabled)
                return null;

            // Only if this is the local server (do not hide hibernation if active)
            var view = controller.View;
            if (!view.Settings.IsLocal)
                return null;

            // Can not hibernate
            if (m_Settings.HibernationDelay < 1)
            {
                // Dialog should not be shown
                HideHibernation();

                // Not responsible
                return null;
            }

            // See if hibernation is temporarily disabled
            if (m_BlockHibernate < DateTime.MaxValue)
                if (DateTime.UtcNow <= m_BlockHibernate)
                    return false;

            // Only if server is now idle
            if (TrayColors.Standard != newState)
                if (TrayColors.Green != newState)
                    return false;

            // See if hibernation dialog has run through
            if (null != m_Hibernation)
                if (m_Hibernation.MustHibernateNow)
                {
                    // See if there are pending extensions
                    if (!pendingExtensions)
                    {
                        // Hide it
                        m_Hibernation.Hide();

                        // Initiate hibernation
                        try
                        {
                            // Do the web call
                            VCRNETRestProxy.TryHibernate( controller.EndPoint );
                        }
                        catch
                        {
                            // Ignore any error
                        }
                    }

                    // No change required
                    return null;
                }
                else if (m_Hibernation.Visible)
                    return null;

            // Only if we come from recording
            if (!mustHibernate)
                return false;

            // Release
            if (null != m_Hibernation)
            {
                // Shutdown
                m_Hibernation.Dispose();

                // Forget
                m_Hibernation = null;
            }

            // Create dialog
            m_Hibernation = new HibernateDialog();

            // Initialize and show up
            m_Hibernation.Owner = this;
            m_Hibernation.Start( m_Settings.HibernationDelay );
            m_Hibernation.Show();

            // Hibernation dialog is running
            return true;
        }

        private void StartService( object state )
        {
            // Be safe
            try
            {
                // Attach to service manager
                using (ServiceController service = new ServiceController( "VCR.NET Service" ))
                    if (ServiceControllerStatus.Running != service.Status)
                    {
                        // Start it
                        service.Start();

                        // Synchronize
                        service.WaitForStatus( ServiceControllerStatus.Running );
                    }
            }
            catch
            {
                // Ignore any error
            }
        }

        private void mnuNewJob_Click( object sender, EventArgs e )
        {
            // Jump in
            OpenBrowser( "default.html#edit" );
        }

        private void mnuEPG_Click( object sender, EventArgs e )
        {
            // Jump in
            OpenBrowser( "default.html#guide" );
        }

        private void HideHibernation()
        {
            // Process
            if (null != m_Hibernation)
                if (!m_Hibernation.IsClosed)
                    m_Hibernation.Visible = false;
        }

        protected override void WndProc( ref System.Windows.Forms.Message m )
        {
            // Check for power management request
            if (0x0218 == m.Msg) HideHibernation();

            // Set flag
            m_QueryEndSession = (0x0011 == m.Msg);

            // Forward to base class
            base.WndProc( ref m );
        }

        private void CheckStream()
        {
            // Disable button
            cmdStreaming.Enabled = false;

            // Pre-check
            if (!string.IsNullOrEmpty( errorMessages.GetError( txArgs ) )) return;
            if (!string.IsNullOrEmpty( errorMessages.GetError( txMultiCast ) )) return;

            // Check mode
            if (Equals( txViewer.Text, m_Settings.Viewer ))
                if (Equals( txArgs.Text, m_Settings.ViewerArgs ))
                    if (Equals( txMultiCast.Text, m_Settings.MulticastIP ))
                        if (m_Settings.MinPort == (int) selStreamPort.Value)
                            return;

            // Can save
            cmdStreaming.Enabled = true;
        }

        private void cmdAppChooser_Click( object sender, EventArgs e )
        {
            // Load
            fileChooser.FileName = txViewer.Text;

            // Open chooser
            if (DialogResult.OK != fileChooser.ShowDialog( this )) return;

            // Load
            txViewer.Text = fileChooser.FileName;

            // Update
            CheckStream();
        }

        private void txArgs_Validating( object sender, CancelEventArgs e )
        {
            try
            {
                // Clear error
                errorMessages.SetError( txArgs, null );

                // Test
                string.Format( txArgs.Text, string.Empty, 0, "dvbnet://" );

                // Update GUI
                CheckStream();
            }
            catch (Exception ex)
            {
                // Show error
                errorMessages.SetError( txArgs, ex.Message );

                // Update GUI
                cmdStreaming.Enabled = false;

                // Forbid
                e.Cancel = true;
            }
        }

        private void txMultiCast_Validating( object sender, CancelEventArgs e )
        {
            // Clear
            errorMessages.SetError( txMultiCast, null );

            // Convert
            try
            {
                // Empty is allowed
                if (!string.IsNullOrEmpty( txMultiCast.Text ))
                {
                    // Convert
                    IPAddress test = IPAddress.Parse( txMultiCast.Text );
                    byte test0 = test.GetAddressBytes()[0];

                    // Check it
                    if ((test0 < 224) || (test0 > 239)) throw new FormatException( Properties.Resources.Multicast );
                }

                // Update GUI
                CheckStream();
            }
            catch (Exception ex)
            {
                // Update
                errorMessages.SetError( txMultiCast, ex.Message );

                // Update GUI
                cmdStreaming.Enabled = false;

                // Forbid
                e.Cancel = true;
            }
        }

        private void cmdStreaming_Click( object sender, EventArgs e )
        {
            // Old values
            string viewer = m_Settings.Viewer;
            string args = m_Settings.ViewerArgs;
            string ip = m_Settings.MulticastIP;
            ushort port = m_Settings.MinPort;

            // Update the all
            m_Settings.MinPort = (ushort) selStreamPort.Value;
            m_Settings.MulticastIP = txMultiCast.Text;
            m_Settings.ViewerArgs = txArgs.Text;
            m_Settings.Viewer = txViewer.Text;

            // Try save
            try
            {
                // Store to disk
                m_Settings.Save();

                // Did it
                cmdStreaming.Enabled = false;
            }
            catch (Exception ex)
            {
                // Back
                m_Settings.ViewerArgs = args;
                m_Settings.MulticastIP = ip;
                m_Settings.Viewer = viewer;
                m_Settings.MinPort = port;

                // Report
                MessageBox.Show( this, ex.Message, Properties.Resources.UpdateTitle );
            }
        }

        /// <summary>
        /// Wird aufgerufen, um den Netzwerkversand einer Aufzeichnung zu manipulieren.
        /// </summary>
        /// <param name="sender">Der zugehörige Menüpunkt.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void StartStopStreaming( object sender, EventArgs e )
        {
            // Attach to server
            var settings = CurrentServer;
            if (settings == null)
                return;

            // Attach to item
            var item = (ToolStripDropDownItem) sender;
            if (item == null)
                return;

            // Get the target information
            var info = item.Tag as VCRNETRestProxy.Current;
            if (info == null)
                return;

            // Be safe
            try
            {
                // The target
                string target = null;
                if (string.IsNullOrEmpty( info.streamTarget ))
                    if (!string.IsNullOrEmpty( m_Settings.MulticastIP ))
                    {
                        // Use multicast addressing
                        target = string.Format( "*{0}:{1}", m_Settings.MulticastIP, info.ReservedPort );
                    }
                    else if (settings.IsLocal)
                    {
                        // Construct
                        target = string.Format( "localhost:{0}", info.ReservedPort );
                    }
                    else
                    {
                        // Construct
                        target = string.Format( "{0}:{1}", Dns.GetHostName(), info.ReservedPort );
                    }

                // See if DVB.NET Viewer URL is used
                bool usesViewer = (!string.IsNullOrEmpty( m_Settings.ViewerArgs ) && m_Settings.ViewerArgs.Contains( "{2}" ));

                // Process
                if ((target == null) || !usesViewer)
                    VCRNETRestProxy.SetStreamTarget( settings.EndPoint, info.device, info.source, info.referenceId.Value, target );

                // Done
                if (!string.IsNullOrEmpty( info.streamTarget ))
                    return;

                // Create protocol for direct start 
                var uri = new Uri( settings.EndPoint );
                var dvbnetURL = string.Format( "dvbnet://*{0}/{1}/{2}/Live", uri.Authority, Uri.EscapeDataString( info.device ), info.streamIndex );

                // Run
                Process.Start( m_Settings.Viewer, string.Format( m_Settings.ViewerArgs, string.IsNullOrEmpty( m_Settings.MulticastIP ) ? string.Empty : m_Settings.MulticastIP, info.ReservedPort, dvbnetURL ) );
            }
            catch (Exception)
            {
                // Ignore for now
            }
        }

        private void SetNotifyIcon( NotifyIcon icon, TrayColors state )
        {
            // Store
            icon.Icon = m_TrayIcons[state];

            // Report
            Log( "{0} := {1}", icon.Text, state );
        }

        public static void Log( string message )
        {
            // Synchronize access
            lock (m_FileLock)
            {
                // Read the logging path
                string logPath = (string) ConfigurationManager.AppSettings["IconLogPath"];
                if (string.IsNullOrEmpty( logPath )) return;

                // Be safe
                try
                {
                    // Create text
                    byte[] text = Encoding.GetEncoding( 1252 ).GetBytes( string.Format( "{0} {1}\r\n", DateTime.Now, message ) );

                    // Attach to the file
                    using (FileStream stream = new FileStream( logPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read ))
                    {
                        // Move to end
                        stream.Seek( 0, SeekOrigin.End );

                        // Send
                        stream.Write( text, 0, text.Length );
                    }
                }
                catch
                {
                    // Last chance - ignore any error
                }
            }
        }

        public static void Log( string format, params object[] args )
        {
            // Forward
            Log( string.Format( format, args ) );
        }

        private void selDelay_ValueChanged( object sender, EventArgs e )
        {
            // Old value
            int delay = m_Settings.StartupDelay;

            // Update
            m_Settings.StartupDelay = (int) selDelay.Value;

            // Try save
            try
            {
                // Store to disk
                m_Settings.Save();
            }
            catch (Exception ex)
            {
                // Back
                m_Settings.StartupDelay = delay;

                // Report
                MessageBox.Show( this, ex.Message, Properties.Resources.UpdateLanguage );
            }
        }

        private void mnuHibernateReset_Click( object sender, EventArgs e )
        {
            // Reset
            m_BlockHibernate = DateTime.MaxValue;
        }

        private void mnuHibernateServer_Click( object sender, EventArgs e )
        {
            // Attach to the related server
            var settings = CurrentServer;
            if (settings != null)
                if (MessageBox.Show( string.Format( Properties.Resources.Hibernate, settings.ServerName ), Properties.Resources.HibernateTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2 ) == DialogResult.Yes)
                    try
                    {
                        VCRNETRestProxy.TryHibernate( settings.EndPoint );
                    }
                    catch
                    {
                    }
        }
    }
}