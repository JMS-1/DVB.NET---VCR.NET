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
    /// Die Benutzerschnittstelle zum Anlegen eines neuen Geräteprofils.
    /// </summary>
    public partial class ProfileCreator : UserControl, IPlugInControl
    {
        /// <summary>
        /// Die zugehörige Erweiterung.
        /// </summary>
        private NewProfile m_PlugIn;

        /// <summary>
        /// Eine Methode, mit der das Administrationswerkzeug zur
        /// Aktualisierung der Schaltflächen aufgefordert werden kann.
        /// </summary>
        private IPlugInUISite m_Site;

        /// <summary>
        /// Erzeugt eine neue Steuerung.
        /// </summary>
        /// <param name="plugIn">Die Informationen zum neuen Geräteprofil.</param>
        /// <param name="site">Die Arbeitsumgebung der Erweiterung - üblicherweise das DVB.NET
        /// Administrationswerkzeug.</param>
        public ProfileCreator( NewProfile plugIn, IPlugInUISite site )
        {
            // Remember
            m_PlugIn = plugIn;
            m_Site = site;

            // Copy from designer
            InitializeComponent();

            // Reset
            m_PlugIn.CanProcess = false;
        }

        /// <summary>
        /// Wird aufgerufen, wenn sich der Name des neuen Geräteprofils ändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void txName_TextChanged( object sender, EventArgs e )
        {
            // Forward
            UpdateGUI();
        }

        /// <summary>
        /// Aktualisiert die Anzeige.
        /// </summary>
        private void UpdateGUI()
        {
            // Reset
            m_PlugIn.CanProcess = false;

            // Must have a unique name and at least one option selected
            if (!string.IsNullOrEmpty( txName.Text ))
                if (txName.Text.IndexOfAny( Path.GetInvalidFileNameChars() ) < 0)
                    if (null == JMS.DVB.ProfileManager.FindProfile( txName.Text ))
                        if (optSatellite.Checked || optCable.Checked || optTerrestrial.Checked)
                            m_PlugIn.CanProcess = true;

            // Forward
            m_Site.UpdateGUI();
        }

        /// <summary>
        /// Die Auswahl der Art des Geräteprofils wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void optSatellite_CheckedChanged( object sender, EventArgs e )
        {
            // Forward
            UpdateGUI();
        }

        #region IPlugInControl Members

        /// <summary>
        /// Meldet, ob die Ausführung der Aufgabe abgebrochen werden kann.
        /// </summary>
        bool IPlugInControl.CanCancel
        {
            get
            {
                // We just create the profile
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
            // Be safe
            try
            {
                // New profile to use
                Profile newProfile = null;

                // Check mode
                if (optSatellite.Checked)
                {
                    // Just create
                    newProfile = new SatelliteProfile();
                }
                else if (optCable.Checked)
                {
                    // Create
                    newProfile = new CableProfile();

                    // Add placeholder location
                    newProfile.Locations.Add( new CableLocation() );
                }
                else if (optTerrestrial.Checked)
                {
                    // Create
                    newProfile = new TerrestrialProfile();

                    // Add placeholder location
                    newProfile.Locations.Add( new TerrestrialLocation() );
                }

                // Get the full path
                FileInfo path = new FileInfo( Path.Combine( JMS.DVB.ProfileManager.ProfilePath.FullName, txName.Text + "." + JMS.DVB.ProfileManager.ProfileExtension ) );

                // Store it
                newProfile.Save( path );

                // Update managers list
                DVB.ProfileManager.Refresh();

                // Prepare for selection
                m_Site.SelectProfile( txName.Text );

                // Auto-start configuration
                m_Site.SelectNextPlugIn( typeof( ProfileEditor ) );
            }
            catch (Exception e)
            {
                // Report
                MessageBox.Show( this, e.Message, Properties.Resources.NewProfile_Error );
            }

            // Done
            return true;
        }

        #endregion
    }
}
