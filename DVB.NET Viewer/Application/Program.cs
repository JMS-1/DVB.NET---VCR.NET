using System;
using System.IO;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using JMS.DVB;
using JMS.DVB.Viewer;
using JMS.DVB.DirectShow.UI;
using JMS.DVB.DirectShow.RawDevices;
using System.Reflection;


namespace DVBNETViewer
{
    /// <summary>
    /// Die Startroutine für den DVB.NET / VCR.NET Viewer.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Wird hier zum Abmelden des Anwenders verwendet.
        /// </summary>
        /// <param name="flags">Die Art der Abmeldung.</param>
        /// <param name="reason">Optional ein Grund, falls der Rechner heruntergefahren wird.</param>
        /// <returns>Gesetzt, wenn die Operation erfolgreich war.</returns>
        [DllImport( "user32.dll" )]
        private static extern bool ExitWindowsEx( UInt32 flags, UInt32 reason );

        /// <summary>
        /// Installiert die Laufzeitumgebung.
        /// </summary>
        static Program()
        {
            // Activate dynamic loading
            RunTimeLoader.Startup();
        }

        [STAThread]
        static void Main( string[] args )
        {
            // Be safe
            try
            {
                // Check settings
                var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                if (!version.Equals( Properties.Settings.Default.Version ))
                {
                    // Upgrade
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.Version = version;
                    Properties.Settings.Default.Save();
                }

                // Prepare
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault( false );

                // Force priority
                Process.GetCurrentProcess().PriorityClass = Properties.Settings.Default.Priority;

                // Check start mode
                string startMode = ((null == args) || (args.Length < 1)) ? null : args[0];

                // Apply language
                UserProfile.ApplyLanguage();

                // See how we should work
                if (Equals( startMode, "/Reset" ))
                {
                    // Ask user
                    if (DialogResult.Yes != MessageBox.Show( Properties.Resources.ResetSettings, Properties.Resources.Confirmation, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2 )) return;

                    // Process
                    Properties.Settings.Default.Reset();
                    Properties.Settings.Default.Save();
                }
                if (Equals( startMode, "/LearnRC" ))
                {
                    // Run RC configuration
                    RCSettings.Edit( RCSettings.ConfigurationFile ).Dispose();
                }
                else
                {
                    // Check OSD mode
                    if (Properties.Settings.Default.ManualOSDLegacyMode)
                        OverlayWindow.UseLegacyOverlay = Properties.Settings.Default.OSDLegacyMode;

                    // Form to start
                    ViewerMain form = null;

                    // Test parameter
                    if (!string.IsNullOrEmpty( startMode ))
                        if (Equals( startMode, "/VCR" ))
                        {
                            // Create in VCR LIVE / CURRENT mode
                            form = new ViewerMain( StartupModes.RemoteVCR );
                        }
                        else if (startMode.ToLower().StartsWith( "dvbnet://" ))
                        {
                            // Start with the server part
                            startMode = startMode.Substring( 9 );

                            // See if this is a regular start using the URL protocol
                            bool startedByProtocol = !startMode.StartsWith( "*" );

                            // Must be the control center
                            if (startedByProtocol)
                            {
                                // Prepare for special decoding
                                byte[] tmp = new byte[startMode.Length];

                                // Copy by byte
                                for (int i = tmp.Length; i-- > 0; )
                                    tmp[i] = (byte) startMode[i];

                                // Retrieve
                                startMode = Encoding.UTF8.GetString( tmp );
                            }
                            else
                            {
                                // Just cut off the control character
                                startMode = startMode.Substring( 1 );
                            }

                            // Just correct for URL stuff                           
                            startMode = Uri.UnescapeDataString( startMode.Replace( '+', ' ' ) );

                            // See if this is a file replay
                            int file = startMode.ToLower().IndexOf( "/play=" );
                            if (file < 0)
                            {
                                // Create in VCR CURRENT mode
                                form = new ViewerMain( StartupModes.WatchOrTimeshift, startMode );
                            }
                            else
                            {
                                // Get server and file name
                                string server = startMode.Substring( 0, file );
                                string path = startMode.Substring( file + 6 );

                                // Replay
                                form = new ViewerMain( StartupModes.PlayRemoteFile, path, server );
                            }
                        }
                        else if (startMode.StartsWith( "/VCR=" ))
                        {
                            // Create in VCR REPLY mode
                            form = new ViewerMain( StartupModes.PlayRemoteFile, startMode.Substring( 5 ), null );
                        }
                        else if (startMode.StartsWith( "/TCP=" ))
                        {
                            // Create in STREAMING SLAVE mode
                            form = new ViewerMain( StartupModes.ConnectTCP, startMode.Substring( 5 ) );
                        }
                        else if (startMode.StartsWith( "/FILE=" ))
                        {
                            // Create in LOCAL REPLAY mode
                            form = new ViewerMain( StartupModes.PlayLocalFile, startMode.Substring( 6 ) );
                        }

                    // Local mode
                    if (form != null)
                    {
                        // Run the application
                        Application.Run( form );
                    }
                    else
                    {
                        // Ask for the profile
                        var profile = UserProfile.Profile;
                        if (profile != null)
                            using (HardwareManager.Open())
                                Application.Run( new ViewerMain( profile ) );
                    }
                }
            }
            catch (Exception e)
            {
                // Report as is
                MessageBox.Show( e.ToString() );

                // Terminate
                Environment.Exit( 1 );
            }

            // If we are running as the users shell log off
            using (var key = Registry.CurrentUser.OpenSubKey( @"Software\Microsoft\Windows NT\CurrentVersion\Winlogon" ))
                if (key != null)
                    try
                    {
                        // Load shell
                        var shell = key.GetValue( "Shell" ) as string;
                        if (shell != null)
                        {
                            // Remove quotes
                            if (shell.Length >= 2)
                                if (shell.StartsWith( "\"" ))
                                    if (shell.EndsWith( "\"" ))
                                        shell = shell.Substring( 1, shell.Length - 2 ).Replace( "\"\"", "\"" );

                            // Clip
                            shell = shell.Trim();

                            // See what's left
                            if (!string.IsNullOrEmpty( shell ))
                            {
                                // Attach to file
                                var file1 = new FileInfo( shell );
                                var file2 = new FileInfo( Application.ExecutablePath );

                                // Check 
                                if (string.Equals( file1.FullName, file2.FullName, StringComparison.InvariantCultureIgnoreCase ))
                                    ExitWindowsEx( 0x10, 0 );
                            }
                        }
                    }
                    catch
                    {
                        // Ignore any error
                    }
        }
    }
}