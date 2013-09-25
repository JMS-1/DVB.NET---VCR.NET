extern alias oldVersion;

using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using JMS.DVB.Provider.Legacy;
using System.Collections.Generic;

using legacy = oldVersion::JMS;

namespace JMS.DVB.Administration.ProfileManager
{
    /// <summary>
    /// Die Benutzerschnittstelle zur Konvertierung älterer (vor 3.5.1) Geräteprofile
    /// in das neue Format.
    /// </summary>
    public partial class ProfileConversion : UserControl, IPlugInControl
    {
        /// <summary>
        /// Die zugehörige Erweiterung.
        /// </summary>
        private ProfileUpgrade m_PlugIn;

        /// <summary>
        /// Eine Methode, mit der das Administrationswerkzeug zur
        /// Aktualisierung der Schaltflächen aufgefordert werden kann.
        /// </summary>
        private IPlugInUISite m_Site;

        /// <summary>
        /// Erzeugt ein neues visuelles Element.
        /// </summary>
        /// <param name="upgrader">Die Informationen zum zu pflegenden Geräteprofil.</param>
        /// <param name="site">Die Arbeitsumgebung der Erweiterung - üblicherweise das DVB.NET
        /// Administrationswerkzeug.</param>
        public ProfileConversion( ProfileUpgrade upgrader, IPlugInUISite site )
        {
            // Remember
            m_PlugIn = upgrader;
            m_Site = site;

            // Overload designer stuff
            InitializeComponent();
        }

        /// <summary>
        /// Bereitet die Anzeige vor.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ProfileConversion_Load( object sender, EventArgs e )
        {
            // Reset extension
            m_PlugIn.CanProcess = false;

            // Find all profiles
            foreach (legacy.ChannelManagement.DeviceProfile profile in legacy.ChannelManagement.DeviceProfile.SystemProfiles)
            {
                // Just add
                ListViewItem item = lstProfiles.Items.Add( profile.Name );

                // Configure
                item.Tag = profile;
            }

            // Set width
            lstProfiles.Columns[0].AutoResize( ColumnHeaderAutoResizeStyle.ColumnContent );

            // Prepare
            UpdateGUI();
        }

        /// <summary>
        /// Es wurde ein Gräteprofil aktiviert oder deaktiviert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void lstProfiles_ItemChecked( object sender, ItemCheckedEventArgs e )
        {
            // Forward
            UpdateGUI();
        }

        /// <summary>
        /// Die Bestätigung zum Überschreiben vorhandener Profile wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ckConfirmOverwrite_CheckedChanged( object sender, EventArgs e )
        {
            // Forward
            if (ckConfirmOverwrite.Visible)
                UpdateGUI();
        }

        /// <summary>
        /// Aktualisiert die Oberflächenelemente.
        /// </summary>
        private void UpdateGUI()
        {
            // See if confirmation should be shown
            bool showConfirm = false;

            // Process all
            foreach (ListViewItem item in lstProfiles.CheckedItems)
                if (null != JMS.DVB.ProfileManager.FindProfile( item.Text ))
                {
                    // Remember
                    showConfirm = true;

                    // Done
                    break;
                }

            // Check mode
            if (showConfirm)
            {
                // Must show
                if (!ckConfirmOverwrite.Visible)
                {
                    // Reset
                    ckConfirmOverwrite.Checked = false;

                    // Show
                    ckConfirmOverwrite.Visible = true;
                    lbConfirmOverwrite.Visible = true;
                }

                // Enable on checked
                m_PlugIn.CanProcess = ckConfirmOverwrite.Checked;
            }
            else
            {
                // Just hide
                ckConfirmOverwrite.Visible = false;
                lbConfirmOverwrite.Visible = false;

                // Enable if anything is selected
                m_PlugIn.CanProcess = (lstProfiles.CheckedItems.Count > 0);
            }

            // Forward
            m_Site.UpdateGUI();
        }

        #region IPlugInControl Members

        /// <summary>
        /// Prüft, ob eine Ausführung möglich ist.
        /// </summary>
        /// <returns>Gesetzt, wenn <see cref="IPlugInControl.Start"/> aufgerufen werden darf.</returns>
        bool IPlugInControl.TestStart()
        {
            // Yes, we can
            return true;
        }

        /// <summary>
        /// Meldet, ob die Ausführung dieser Aufgabe abgebrochen werden kann.
        /// </summary>
        bool IPlugInControl.CanCancel
        {
            get
            {
                // No
                return false;
            }
        }

        /// <summary>
        /// Führt die zugehörige Aufgabe aus.
        /// </summary>
        /// <returns>Gesetzt, wenn die Aufgabe erfolgreich abgeschlossen werden konnte.</returns>
        bool IPlugInControl.Start()
        {
            // Process all
            for (int i = 0; i < lstProfiles.CheckedItems.Count; ++i)
            {
                // Load the item
                ListViewItem item = lstProfiles.CheckedItems[i];

                // Save process
                try
                {
                    // Run conversion
                    Profile profile = ProfileTools.Convert( (legacy.ChannelManagement.DeviceProfile) item.Tag );

                    // Save to disk
                    profile.MakePermanent();
                }
                catch (Exception e)
                {
                    // Construct
                    string message = string.Format( Properties.Resources.Convert_AskUser, e.Message ).Replace( @"\r\n", "\r\n" );
                    string title = string.Format( Properties.Resources.Convert_ErrorTitle, item.Text );

                    // Ask user
                    DialogResult aw = MessageBox.Show( this, message, title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2 );

                    // Stop it all
                    if (DialogResult.Abort == aw)
                        break;

                    // Try again
                    if (DialogResult.Retry == aw)
                        --i;
                }
            }

            // Reload all - at least for cleanup
            DVB.ProfileManager.Refresh();

            // Did it
            return true;
        }

        #endregion
    }
}
