using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;

namespace JMS.DVB.Administration.ProfileManager
{
    /// <summary>
    /// Dialog zur Bestätigung des endgültigen Löschens eines Geräteprofils.
    /// </summary>
    public partial class DeleteConfirmation : UserControl, IPlugInControl
    {
        /// <summary>
        /// Die zugehörige Erweiterung.
        /// </summary>
        private ProfileDeleter m_PlugIn;

        /// <summary>
        /// Eine Methode, mit der das Administrationswerkzeug zur
        /// Aktualisierung der Schaltflächen aufgefordert werden kann.
        /// </summary>
        private IPlugInUISite m_Site;

        /// <summary>
        /// Erzeugt eine neue Steuerung.
        /// </summary>
        /// <param name="deleter">Die Informationen zum zu löschenden Geräteprofil.</param>
        /// <param name="site">Die Arbeitsumgebung der Erweiterung - üblicherweise das DVB.NET
        /// Administrationswerkzeug.</param>
        public DeleteConfirmation( ProfileDeleter deleter, IPlugInUISite site )
        {
            // Remember
            m_PlugIn = deleter;
            m_Site = site;

            // Overwrite designer stuff
            InitializeComponent();
        }

        /// <summary>
        /// Bereitet die Anzeige des Dialogs vor.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void DeleteConfirmation_Load( object sender, EventArgs e )
        {
            // Simple texts
            lbProfile.Text = string.Format( lbProfile.Text, m_PlugIn.Profile.Name );
            lbBackup.Text = string.Format( lbBackup.Text, JMS.DVB.ProfileManager.ProfilePath.FullName );

            // All profiles referencing this one
            Profile[] dependants = Array.FindAll( JMS.DVB.ProfileManager.AllProfiles, p => (0 == string.Compare( p.UseSourcesFrom, m_PlugIn.Profile.Name, true )) );

            // Update text
            lbInUse.Text = string.Format( lbInUse.Text, string.Join( ", ", Array.ConvertAll( dependants, p => p.Name ) ) );

            // Finish
            lbInUse.Visible = (dependants.Length > 0);
            ckBackup.Visible = !lbInUse.Visible;
            lbBackup.Visible = ckBackup.Visible;

            // Position
            if (lbInUse.Visible)
                lbInUse.Top = ckBackup.Top;
        }

        #region IPlugInControl Members

        /// <summary>
        /// Meldet, ob die Ausführung der Aufgabe abgebrochen werden kann.
        /// </summary>
        bool IPlugInControl.CanCancel
        {
            get
            {
                // Never
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
            return true;
        }

        /// <summary>
        /// Führt die Aufgabe aus.
        /// </summary>
        /// <returns>Gesetzt, wenn die Aufgabe synchron abgeschlossen wurde.</returns>
        bool IPlugInControl.Start()
        {
            // Start processing
            try
            {
                // Check mode
                if (ckBackup.Checked)
                {
                    // Create suffix derived from current time
                    string now = DateTime.Now.ToString( "yyyyMMddHHmmss" );

                    // Create the full name
                    FileInfo backup = new FileInfo( Path.ChangeExtension( m_PlugIn.Profile.ProfilePath.FullName, now ) );

                    // Make sure that it does not exist
                    backup.Delete();

                    // Just rename
                    m_PlugIn.Profile.ProfilePath.MoveTo( backup.FullName );
                }
                else
                {
                    // The easy part
                    m_PlugIn.Profile.Delete();
                }

                // Refresh internal list
                JMS.DVB.ProfileManager.Refresh();

                // Delete selection
                m_Site.SelectProfile( null );
            }
            catch (Exception e)
            {
                // Report
                MessageBox.Show( this, e.Message, Properties.Resources.DeleteProfile_Error );
            }

            // Did it
            return true;
        }

        #endregion
    }
}
