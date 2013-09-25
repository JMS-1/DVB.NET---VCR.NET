using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;
using System.Xml;
using JMS.DVB;


namespace JMS.DVBVCR.InstallerActions
{
    /// <summary>
    /// Bietet dem Anwender bei der Erstinstallation die Auswahl eines Geräteprofils an, das
    /// der VCR.NET Recording Service nutzen darf.
    /// </summary>
    public partial class ProfileInstaller : Form
    {
        /// <summary>
        /// Die aktuelle Konfiguration des VCR.NET.
        /// </summary>
        private XmlDocument m_Configuration;

        /// <summary>
        /// Erzeugt das Hauptfenster zur Auswahl.
        /// </summary>
        /// <param name="configuration">Die aktuell gepflegte Konfiguration.</param>
        public ProfileInstaller( XmlDocument configuration )
        {
            // Remember
            m_Configuration = configuration;

            // Set up self
            InitializeComponent();
        }

        /// <summary>
        /// Bereitet die Anzeige des Dialogs vor.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ProfileInstaller_Load( object sender, EventArgs e )
        {
            // See if there profile is already assigned
            var profiles = (XmlElement) m_Configuration.SelectSingleNode( "configuration/appSettings/add[@key='Profiles']" );

            // Already set
            if (!string.IsNullOrEmpty( profiles.GetAttribute( "value" ) ))
            {
                // Discard
                DialogResult = DialogResult.Cancel;

                // Done
                Close();

                // Stop here
                return;
            }

            // Set as top level
            TopLevel = true;
            TopMost = true;

            // To top
            BringToFront();

            // Load all lists
            Reload();
        }

        /// <summary>
        /// Prüft, welche visuellen Elemente in welcher Form nutzbar sind.
        /// </summary>
        private void CheckButton()
        {
            // Disable
            cmdTemplate.Enabled = false;

            // No template selected
            if (selTemplate.SelectedIndex < 0)
                return;

            // No name entered
            if (string.IsNullOrEmpty( txProfileName.Text ))
                return;

            // Invalid
            if (txProfileName.Text.IndexOfAny( Path.GetInvalidFileNameChars().Union( Path.GetInvalidPathChars() ).ToArray() ) >= 0)
                return;

            // Name already exists
            foreach (string profile in selProfile.Items)
                if (string.Equals( profile, txProfileName.Text, StringComparison.InvariantCultureIgnoreCase ))
                    return;

            // Can use it
            cmdTemplate.Enabled = true;
        }

        /// <summary>
        /// Füllt alle Auswahllisten.
        /// </summary>
        private void Reload()
        {
            // Reset manager list
            ProfileManager.Refresh();

            // Load last selection
            var lastProfile = selProfile.SelectedItem as string;

            // Clear lists
            selTemplate.Items.Clear();
            selProfile.Items.Clear();

            // Update GUI
            cmdUse.Enabled = false;

            // Fill it
            selProfile.Items.AddRange( ProfileManager.AllProfiles.Select( p => p.Name ).ToArray() );

            // Get our context
            var type = GetType();

            // Parts
            var prefix = type.Namespace + ".ProfileTemplates.";
            var suffix = "." + ProfileManager.ProfileExtension;

            // Create list of templates
            selTemplate.Items.AddRange(
                type
                    .Assembly
                    .GetManifestResourceNames()
                    .Where( r => r.StartsWith( prefix ) )
                    .Where( r => r.EndsWith( suffix ) )
                    .Select( r => r.Substring( prefix.Length, r.Length - prefix.Length - suffix.Length ) )
                    .ToArray() );

            // Finish
            CheckButton();

            // Reload
            if (lastProfile != null)
            {
                // Select it
                selProfile.SelectedItem = lastProfile;

                // Choose
                selProfile_SelectionChangeCommitted( selProfile, EventArgs.Empty );
            }
        }

        /// <summary>
        /// Die Auswahl einer Profilvorlage wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selTemplate_SelectionChangeCommitted( object sender, EventArgs e )
        {
            // UpdateGUI
            CheckButton();
        }

        /// <summary>
        /// Der Name des neu anzulegenden Geräteprofils wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void txProfileName_TextChanged( object sender, EventArgs e )
        {
            // UpdateGUI
            CheckButton();
        }

        /// <summary>
        /// Der Anwender möchte eine Profilvorlage verwenden.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdTemplate_Click( object sender, EventArgs e )
        {
            // Be safe
            try
            {
                // Get our context
                var type = GetType();

                // Full name
                var resName = string.Format( "{0}.ProfileTemplates.{1}.{2}", type.Namespace, selTemplate.SelectedItem, ProfileManager.ProfileExtension );

                // Open and read
                using (var xml = type.Assembly.GetManifestResourceStream( resName ))
                {
                    // Load from stream
                    var profile = ProfileManager.LoadProfile( xml );

                    // Get the directory
                    var profileDir = ProfileManager.ProfilePath;

                    // Make sure that directory exists
                    if (!profileDir.Exists)
                        profileDir.Create();

                    // Store it
                    profile.Save( new FileInfo( Path.Combine( profileDir.FullName, txProfileName.Text + "." + ProfileManager.ProfileExtension ) ) );
                }

                // Reload
                Reload();

                // Select
                selProfile.SelectedItem = txProfileName.Text;

                // Update GUI
                selProfile_SelectionChangeCommitted( selProfile, EventArgs.Empty );
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, string.Format( Properties.Resources.CreateProfile, txProfileName.Text ), ex.Message );
            }
        }

        /// <summary>
        /// Der Anwender möchte ein neues Geräteprofil anlegen.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdNewProfile_Click( object sender, EventArgs e )
        {
            // Be safe
            try
            {
                // Attach to the administration tool
                var admin = new FileInfo( Path.Combine( RunTimeLoader.RootDirectory.FullName, "DVBNETAdmin.exe" ) );

                // Prepare to start it
                var start =
                    new ProcessStartInfo
                        {
                            Arguments = "/task=JMS.DVB.Administration.ProfileManager.NewProfile",
                            WorkingDirectory = admin.Directory.FullName,
                            WindowStyle = ProcessWindowStyle.Normal,
                            FileName = admin.FullName,
                            LoadUserProfile = true,
                            UseShellExecute = true
                        };

                // Start it
                using (var process = Process.Start( start ))
                {
                    // Wait for the window to come up
                    process.WaitForInputIdle();

                    // Hide self
                    Visible = false;

                    // Do it
                    Application.DoEvents();

                    // Finish
                    process.WaitForExit();
                }

                // Get all current profiles
                var profiles = new Dictionary<string, bool>( ProfileManager.ProfileNameComparer );

                // Fill
                foreach (string profile in selProfile.Items)
                    profiles[profile] = true;

                // Refresh
                Reload();

                // Select the first not in list
                foreach (string profile in selProfile.Items)
                    if (!profiles.ContainsKey( profile ))
                    {
                        // Select
                        selProfile.SelectedItem = profile;

                        // Update
                        selProfile_SelectionChangeCommitted( selProfile, EventArgs.Empty );

                        // Done
                        break;
                    }
            }
            catch
            {
            }
            finally
            {
                // Show up
                Visible = true;
            }
        }

        /// <summary>
        /// Der Anwender hat ein existierendes Geräteprofil ausgewählt.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selProfile_SelectionChangeCommitted( object sender, EventArgs e )
        {
            // Set button
            cmdUse.Enabled = (selProfile.SelectedIndex >= 0);
        }

        /// <summary>
        /// Der Anwender möchte ein existierendes Geräteprofil verwenden.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdUse_Click( object sender, EventArgs e )
        {
            // Load the name
            var profile = (string) selProfile.SelectedItem;

            // Find XML tag to update
            var profiles = (XmlElement) m_Configuration.SelectSingleNode( "configuration/appSettings/add[@key='Profiles']" );

            // Do the update
            profiles.SetAttribute( "value", profile );

            // Get the name
            var profilePath = new FileInfo( Path.Combine( ProfileManager.ProfilePath.FullName, profile + "." + ProfileManager.ProfileExtension ) );

            // Make sure that adminstrator access is defined
            try
            {
                // Load the current rights
                var sec = profilePath.GetAccessControl();

                // This is administrator access
                var admins = new SecurityIdentifier( WellKnownSidType.BuiltinAdministratorsSid, null );

                // Set if update is needed
                var adminRights = false;

                // Scan all current rights
                foreach (FileSystemAccessRule testRule in sec.GetAccessRules( true, true, typeof( SecurityIdentifier ) ))
                    if (adminRights = Equals( testRule.IdentityReference, admins ))
                        break;

                // Add the access
                if (!adminRights)
                {
                    // Create rule
                    var rule = new FileSystemAccessRule( admins, FileSystemRights.FullControl, AccessControlType.Allow );

                    // Apply rule
                    sec.SetAccessRule( rule );

                    // Save to file system
                    profilePath.SetAccessControl( sec );
                }

                // Done
                Close();
            }
            catch
            {
            }
        }
    }
}