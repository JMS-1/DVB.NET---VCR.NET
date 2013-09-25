using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

using JMS.DVB.Editors;
using JMS.DVB.Administration.SourceScanner;


namespace JMS.DVB.Administration.ProfileManager
{
    /// <summary>
    /// Die Pflege der Daten eines Geräteprofils.
    /// </summary>
    public partial class ProfileDialog : UserControl, IPlugInControl
    {
        /// <summary>
        /// Die zugehörige Erweiterung.
        /// </summary>
        private ProfileEditor m_PlugIn;

        /// <summary>
        /// Eine Methode, mit der das Administrationswerkzeug zur
        /// Aktualisierung der Schaltflächen aufgefordert werden kann.
        /// </summary>
        private IPlugInUISite m_Site;

        /// <summary>
        /// Alle Sondereinstellungen für Aufzeichnungen.
        /// </summary>
        private Dictionary<string, string> m_RecordingSettings;

        /// <summary>
        /// Erzeugt eine neue Steuerung.
        /// </summary>
        /// <param name="editor">Die Informationen zum zu pflegenden Geräteprofil.</param>
        /// <param name="site">Die Arbeitsumgebung der Erweiterung - üblicherweise das DVB.NET
        /// Administrationswerkzeug.</param>
        public ProfileDialog( ProfileEditor editor, IPlugInUISite site )
        {
            // Remember
            m_PlugIn = editor;
            m_Site = site;

            // Fill data from designer
            InitializeComponent();

            // Get the source group type of the profile
            Type groupType = m_PlugIn.Profile.GetGroupType();

            // Load static data
            if (groupType == typeof( SatelliteGroup ))
            {
                // Set default selection
                selType.Items.Add( new HardwareTypeItem { DisplayName = Properties.Resources.Type_Standard, Type = "JMS.DVB.StandardSatelliteHardware, JMS.DVB.HardwareAbstraction" } );
                selType.Items.Add( new HardwareTypeItem { DisplayName = Properties.Resources.Type_Legacy, Type = "JMS.DVB.Provider.Legacy.DVBSLegacy, JMS.DVB.Provider.Legacy" } );
            }
            else if (groupType == typeof( CableGroup ))
            {
                // Set default selection
                selType.Items.Add( new HardwareTypeItem { DisplayName = Properties.Resources.Type_Standard, Type = "JMS.DVB.StandardCableHardware, JMS.DVB.HardwareAbstraction" } );
                selType.Items.Add( new HardwareTypeItem { DisplayName = Properties.Resources.Type_Legacy, Type = "JMS.DVB.Provider.Legacy.DVBCLegacy, JMS.DVB.Provider.Legacy" } );
            }
            else if (groupType == typeof( TerrestrialGroup ))
            {
                // Set default selection
                selType.Items.Add( new HardwareTypeItem { DisplayName = Properties.Resources.Type_Standard, Type = "JMS.DVB.StandardTerrestrialHardware, JMS.DVB.HardwareAbstraction" } );
                selType.Items.Add( new HardwareTypeItem { DisplayName = Properties.Resources.Type_Legacy, Type = "JMS.DVB.Provider.Legacy.DVBTLegacy, JMS.DVB.Provider.Legacy" } );
            }
            else
            {
                // Fallback
                selType.Items.Add( new HardwareTypeItem { DisplayName = string.Empty, Type = null } );
            }

            // Reset sharing selection
            selShare.Items.Add( Properties.Resources.UseFrom_Self );

            // List of all profiles which we can choose from
            var profiles = new List<string>();

            // Get the current reference
            string from = m_PlugIn.Profile.UseSourcesFrom;
            try
            {
                // Just test all
                foreach (Profile profile in JMS.DVB.ProfileManager.AllProfiles)
                {
                    // Enter
                    m_PlugIn.Profile.UseSourcesFrom = profile.Name;

                    // Test
                    if (null != m_PlugIn.Profile.LeafProfile)
                        profiles.Add( profile.Name );
                }
            }
            finally
            {
                // Must always reset
                m_PlugIn.Profile.UseSourcesFrom = from;
            }

            // Sort list
            profiles.Sort();

            // Add list to selection
            selShare.Items.AddRange( profiles.ToArray() );

            // Select current
            if (string.IsNullOrEmpty( from ))
            {
                // Select the first entry
                selShare.SelectedIndex = 0;
            }
            else
            {
                // Select the indicated entry
                selShare.SelectedItem = from;
            }

            // Load recording settings
            m_RecordingSettings = RecordingSettings.ExtractSettings( m_PlugIn.Profile );

            // We are valid if the sharing is configured correctly (does not point to a dead entry)
            m_PlugIn.IsValid = (null != selShare.SelectedItem);
        }

        /// <summary>
        /// Füllt die Anzeige mit den Daten des aktuellen Geräteprofils.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ProfileDialog_Load( object sender, EventArgs e )
        {
            // Attach to the profile
            Profile profile = m_PlugIn.Profile;

            // Name
            txName.Text = profile.Name;

            // Find the hardware entry
            foreach (HardwareTypeItem item in selType.Items)
                if (Equals( item.Type, profile.HardwareType ))
                {
                    // Choose it
                    selType.SelectedItem = item;

                    // Done
                    break;
                }

            // Must create new
            if (null == selType.SelectedItem)
                if (!string.IsNullOrEmpty( profile.HardwareType ))
                {
                    // Create item
                    HardwareTypeItem tempItem = new HardwareTypeItem { Type = profile.HardwareType };

                    // Remember it
                    selType.Items.Add( tempItem );

                    // Select it
                    selType.SelectedItem = tempItem;
                }

            // Flags
            ckDisableEPG.Checked = m_PlugIn.Profile.DisableProgramGuide;

            // Change profile type
            var satProfile = m_PlugIn.Profile as SatelliteProfile;
            if (null != satProfile)
            {
                // Show DVB-S2 selection
                ckDVBS2.Checked = satProfile.DisableS2Groups;
                ckDVBS2.Visible = true;
            }

            // Load edit control
            selType_SelectionChangeCommitted( selType, EventArgs.Empty );

            // Finish
            UpdateGUI();
        }

        /// <summary>
        /// Die Auswahl der Implementierung wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selType_SelectionChangeCommitted( object sender, EventArgs e )
        {
            // Clear all
            foreach (Control control in pnlEditor.Controls)
                control.Dispose();

            // Reset
            pnlEditor.Controls.Clear();

            // Report to extension
            m_PlugIn.CurrentEditor = null;

            // Get the selection
            var item = (HardwareTypeItem) selType.SelectedItem;
            if (null == item)
                return;
            if (null == item.Type)
                return;

            // Create the control
            var editor = HardwareEditorAttribute.CreateEditor( item.Type );

            // Initialize it
            editor.Profile = m_PlugIn.Profile;

            // Change type
            Control editorControl = (Control) editor;

            // Configure it
            editorControl.Dock = DockStyle.Fill;
            editorControl.Visible = true;

            // Put it in place
            pnlEditor.Controls.Add( editorControl );

            // Report to extension
            m_PlugIn.CurrentEditor = editor;
        }

        /// <summary>
        /// Prüft bei Veränderungen an dem Namen des Profils, ob dieser 
        /// gültig ist.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void txName_TextChanged( object sender, EventArgs e )
        {
            // Forward
            UpdateGUI();
        }

        /// <summary>
        /// Die Geräteimplementierung wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selType_SelectedIndexChanged( object sender, EventArgs e )
        {
            // Forward
            UpdateGUI();
        }

        /// <summary>
        /// Aktualisiert die Anzeige nach einer Änderung.
        /// </summary>
        private void UpdateGUI()
        {
            // Assume not valid
            m_PlugIn.IsValid = false;

            // Always update
            try
            {
                // Get the selection
                HardwareTypeItem item = (HardwareTypeItem) selType.SelectedItem;
                if (null == item)
                    return;
                if (null == item.Type)
                    return;

                // No name at all
                if (string.IsNullOrEmpty( txName.Text ))
                    return;

                // Check for invalid file name characters
                if (txName.Text.IndexOfAny( Path.GetInvalidFileNameChars() ) >= 0)
                    return;

                // If name changes make sure that there is no other profile with the same name
                Profile other = JMS.DVB.ProfileManager.FindProfile( txName.Text );
                if (null != other)
                    if (other != m_PlugIn.Profile)
                        return;

                // We allow it if the source selection is valid
                m_PlugIn.IsValid = (null != selShare.SelectedItem);
            }
            finally
            {
                // Update GUI
                m_Site.UpdateGUI();
            }
        }

        /// <summary>
        /// Zeigt die erweiterten Einstellungen für Aufzeichnungen an.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdRecordings_Click( object sender, EventArgs e )
        {
            // Just show
            using (var dialog = new RecordingSettings( m_RecordingSettings ))
                dialog.ShowDialog( this );
        }

        #region IPlugInControl Members

        /// <summary>
        /// Beginnt mit der Ausführung der Aufgabe.
        /// </summary>
        /// <returns>Gesetzt, wenn die Aufgabe synchron abgeschlossen wurde.</returns>
        bool IPlugInControl.Start()
        {
            // Set the driver type
            HardwareTypeItem item = (HardwareTypeItem) selType.SelectedItem;
            if (null == item)
                m_PlugIn.Profile.HardwareType = null;
            else
                m_PlugIn.Profile.HardwareType = item.Type;

            // See if we are using DVB-S
            SatelliteProfile satProfile = m_PlugIn.Profile as SatelliteProfile;
            if (null != satProfile)
                satProfile.DisableS2Groups = ckDVBS2.Checked;

            // Copy device data back to profile
            m_PlugIn.CurrentEditor.UpdateProfile();

            // Copy flags
            m_PlugIn.Profile.DisableProgramGuide = ckDisableEPG.Checked;

            // Copy our data back to profile
            if (0 == selShare.SelectedIndex)
                m_PlugIn.Profile.UseSourcesFrom = null;
            else
                m_PlugIn.Profile.UseSourcesFrom = (string) selShare.SelectedItem;

            // Clear out recording parameters
            m_PlugIn.Profile.Parameters.RemoveAll( p => !string.IsNullOrEmpty( p.Name ) && m_RecordingSettings.ContainsKey( p.Name ) );

            // Add all
            m_PlugIn.Profile.Parameters.AddRange( m_RecordingSettings.Where( p => !string.IsNullOrEmpty( p.Value ) ).Select( p => new ProfileParameter( p.Key, p.Value ) ) );

            // Check for rename
            if (0 == string.Compare( m_PlugIn.Profile.Name, txName.Text, true ))
            {
                // Just put to disk
                m_PlugIn.Profile.Save();
            }
            else
            {
                // Attach to the current file position
                FileInfo oldPath = m_PlugIn.Profile.ProfilePath;

                // Make the new path
                FileInfo newPath = new FileInfo( Path.Combine( oldPath.DirectoryName, txName.Text + oldPath.Extension ) );

                // Delete it silently
                newPath.Delete();

                // Update file state
                oldPath.Refresh();

                // See if files are the same
                bool isSameFile = !oldPath.Exists;

                // Restart the profile manager
                DVB.ProfileManager.Refresh();

                // Try to save
                m_PlugIn.Profile.Save( newPath );

                // Delete the old path if we are not renaming in place
                if (!isSameFile)
                    oldPath.Delete();

                // Make it current
                m_Site.SelectProfile( txName.Text );
            }

            // See if we should configure the scan locations
            if (string.IsNullOrEmpty( m_PlugIn.Profile.UseSourcesFrom ))
                if (m_PlugIn.Profile.ScanLocations.Count < 1)
                    m_Site.SelectNextPlugIn( typeof( Configuration ) );

            // Did it
            return true;
        }

        /// <summary>
        /// Meldet, ob eine Aufgabe unterbrochen werden kann.
        /// </summary>
        bool IPlugInControl.CanCancel
        {
            get
            {
                // We can
                return false;
            }
        }

        /// <summary>
        /// Prüft, ob eine Ausführung möglich ist.
        /// </summary>
        /// <returns>Gesetzt, wenn <see cref="IPlugInControl.Start"/> aufgerufen werden darf.</returns>
        bool IPlugInControl.TestStart()
        {
            // Yes, we can
            if (m_PlugIn.IsValid)
                if (null == m_PlugIn.CurrentEditor)
                    return true;
                else if (m_PlugIn.CurrentEditor.IsValid)
                    return true;

            // Ask user
            switch (MessageBox.Show( this, Properties.Resources.EditProfile_Invalid, m_PlugIn.DisplayName, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2 ))
            {
                case DialogResult.Yes: return true;
                default: return false;
            }
        }

        #endregion
    }
}
