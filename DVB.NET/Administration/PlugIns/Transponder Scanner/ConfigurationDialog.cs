using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using JMS.DVB.Algorithms;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace JMS.DVB.Administration.SourceScanner
{
    /// <summary>
    /// Die Steuerung des Sendersuchlaufs.
    /// </summary>
    public partial class ConfigurationDialog : UserControl, IPlugInControl
    {
        /// <summary>
        /// Die zugehörige Erweiterung.
        /// </summary>
        private Configuration m_PlugIn;

        /// <summary>
        /// Eine Methode, mit der das Administrationswerkzeug zur
        /// Aktualisierung der Schaltflächen aufgefordert werden kann.
        /// </summary>
        private IPlugInUISite m_Site;

        /// <summary>
        /// Alle bisher verwendeten Konfigurationen für DVB-S.
        /// </summary>
        private List<ScanTemplate<SatelliteLocation>> m_SatTemplates = new List<ScanTemplate<SatelliteLocation>>();

        /// <summary>
        /// Erzeugt eine neue Steuerung.
        /// </summary>
        /// <param name="configuration">Die Konfiguration für diesen Suchlauf.</param>
        /// <param name="site">Die Arbeitsumgebung der Erweiterung - üblicherweise das DVB.NET
        /// Administrationswerkzeug.</param>
        public ConfigurationDialog( Configuration configuration, IPlugInUISite site )
        {
            // Remember
            m_PlugIn = configuration;
            m_Site = site;

            // Fill data from designer
            InitializeComponent();

            // Forward
            selGroups.LoadLocations( m_PlugIn.Profile );
        }

        /// <summary>
        /// Bereitet die Benutzerschnittstelle vor.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ConfigurationDialog_Load( object sender, EventArgs e )
        {
            // Set up the name of the selected profile
            lbProfile.Text = string.Format( lbProfile.Text, m_PlugIn.Profile.Name );

            // Disable DiSEqC selection
            pnlDiSEqC.Visible = false;

            // Get the type of the source groups
            Type groupType = m_PlugIn.Profile.GetGroupType();
            if (null == groupType)
                return;

            // See if it is satellite
            if (!typeof( SatelliteGroup ).IsAssignableFrom( groupType ))
            {
                // Template to use
                ScanTemplate scan;

                // Create new
                if (typeof( CableGroup ).IsAssignableFrom( groupType ))
                {
                    // Create for DVB-C
                    scan = new ScanTemplate<CableLocation> { Location = new CableLocation() };
                }
                else if (typeof( TerrestrialGroup ).IsAssignableFrom( groupType ))
                {
                    // Create for DVB-T
                    scan = new ScanTemplate<TerrestrialLocation> { Location = new TerrestrialLocation() };
                }
                else
                {
                    // Actually this is strange
                    return;
                }

                // Preset
                if (m_PlugIn.Profile.ScanLocations.Count > 0)
                    scan.ScanLocations.AddRange( ((ScanTemplate) m_PlugIn.Profile.ScanLocations[0]).ScanLocations );

                // First only
                selGroups.SetSelection( scan );

                // Done
                return;
            }

            // Enable DiSEqC input
            pnlDiSEqC.Visible = true;

            // Do not use DiSEqC
            selMode.SelectedIndex = 0;

            // Check mode
            foreach (ScanTemplate<SatelliteLocation> scanLocation in m_PlugIn.Profile.ScanLocations)
                if (null != scanLocation.Location)
                {
                    // Get the type of the location selection
                    DiSEqCLocations selection = scanLocation.Location.LNB;

                    // No DiSEqC
                    if (DiSEqCLocations.None == selection)
                        break;

                    // Simple (burst) mode
                    if ((DiSEqCLocations.BurstOn == selection) || (DiSEqCLocations.BurstOff == selection))
                    {
                        // Choose
                        selMode.SelectedIndex = 1;

                        // Done
                        break;
                    }

                    // DiSEqC 1.0
                    if ((DiSEqCLocations.DiSEqC1 == selection) || (DiSEqCLocations.DiSEqC2 == selection) || (DiSEqCLocations.DiSEqC3 == selection) || (DiSEqCLocations.DiSEqC4 == selection))
                    {
                        // Choose
                        selMode.SelectedIndex = 2;

                        // Done
                        break;
                    }
                }

            // Finsih selection
            selMode_SelectionChangeCommitted( selMode, EventArgs.Empty );
        }

        /// <summary>
        /// Die Auswahl des DiSEqC Modus wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selMode_SelectionChangeCommitted( object sender, EventArgs e )
        {
            // Get the selection
            int selection = selMode.SelectedIndex;

            // Clear
            selDish.Items.Clear();

            // Load
            if (selection > 0)
                for (int i = 1, imax = 2 * selection; i <= imax; ++i)
                    selDish.Items.Add( i );

            // Show or hide
            pnlLNB.Visible = (selDish.Items.Count > 0);

            // Select the first
            if (pnlLNB.Visible)
                selDish.SelectedIndex = 0;

            // Finish
            selDish_SelectionChangeCommitted( selDish, EventArgs.Empty );
        }

        /// <summary>
        /// Meldet die aktuelle Auswahl des Ursprungs.
        /// </summary>
        private DiSEqCLocations CurrentLocation
        {
            get
            {
                // Get the mode
                if (selMode.SelectedIndex == 1)
                    switch (selDish.SelectedIndex)
                    {
                        case 0: return DiSEqCLocations.BurstOff;
                        case 1: return DiSEqCLocations.BurstOn;
                    }
                else if (selMode.SelectedIndex == 2)
                    switch (selDish.SelectedIndex)
                    {
                        case 0: return DiSEqCLocations.DiSEqC1;
                        case 1: return DiSEqCLocations.DiSEqC2;
                        case 2: return DiSEqCLocations.DiSEqC3;
                        case 3: return DiSEqCLocations.DiSEqC4;
                    }

                // Unknown at best
                return DiSEqCLocations.None;
            }
        }

        /// <summary>
        /// Die Auswahl des zu konfigurierenden Ursprungs wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selDish_SelectionChangeCommitted( object sender, EventArgs e )
        {
            // Hide
            pnlSpecial.Visible = false;

            // Copy back current settings - if any
            selGroups.GetSelection();

            // What to select
            DiSEqCLocations selection = CurrentLocation;

            // Template to use
            ScanTemplate<SatelliteLocation> scan = m_SatTemplates.Find( t => t.Location.LNB == selection );
            if (null == scan)
            {
                // Create new
                scan = new ScanTemplate<SatelliteLocation>();

                // Find it
                foreach (ScanTemplate<SatelliteLocation> scanLocation in m_PlugIn.Profile.ScanLocations)
                    if (null != scanLocation.Location)
                        if (scanLocation.Location.LNB == selection)
                        {
                            // Copy over
                            scan.Location = SatelliteLocation.Parse( scanLocation.Location.ToString() );
                            scan.ScanLocations.AddRange( scanLocation.ScanLocations );

                            // Did it - use only the first one
                            break;
                        }

                // Create new from default
                if (null == scan.Location)
                    scan.Location =
                        new SatelliteLocation
                            {
                                Frequency1 = SatelliteLocation.Defaults.Frequency1,
                                Frequency2 = SatelliteLocation.Defaults.Frequency2,
                                SwitchFrequency = SatelliteLocation.Defaults.SwitchFrequency,
                                UsePower = SatelliteLocation.Defaults.UsePower,
                                LNB = selection
                            };

                // Remember
                m_SatTemplates.Add( scan );
            }

            // Load locations
            selGroups.SetSelection( scan );

            // Load LNB settings
            selLOF1.Value = scan.Location.Frequency1;
            selLOF2.Value = scan.Location.Frequency2;
            selSwitch.Value = scan.Location.SwitchFrequency;
            ckPower.Checked = scan.Location.UsePower;
        }

        /// <summary>
        /// Ermittelt die aktuelle ausgewählte Antenne.
        /// </summary>
        private SatelliteLocation CurrentLocationConfiguration
        {
            get
            {
                // Forward
                return (SatelliteLocation) selGroups.CurrentTemplate.GroupLocation;
            }
        }

        /// <summary>
        /// Aktualisiert die untere Oszillatorfrequenz.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selLOF1_ValueChanged( object sender, EventArgs e )
        {
            // Just copy
            if (pnlSpecial.Visible)
                CurrentLocationConfiguration.Frequency1 = (uint) selLOF1.Value;
        }

        /// <summary>
        /// Aktualisiert die obere Oszillatorfrequenz.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selLOF2_ValueChanged( object sender, EventArgs e )
        {
            // Just copy
            if (pnlSpecial.Visible)
                CurrentLocationConfiguration.Frequency2 = (uint) selLOF2.Value;
        }

        /// <summary>
        /// Aktualisiert die Wechselfrequenz.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selSwitch_ValueChanged( object sender, EventArgs e )
        {
            // Just copy
            if (pnlSpecial.Visible)
                CurrentLocationConfiguration.SwitchFrequency = (uint) selSwitch.Value;
        }

        /// <summary>
        /// Aktualisiert die Spannungsversorgungsoption.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ckPower_CheckedChanged( object sender, EventArgs e )
        {
            // Just copy
            if (pnlSpecial.Visible)
                CurrentLocationConfiguration.UsePower = ckPower.Checked;
        }

        /// <summary>
        /// Blendet die Konfiguration der LNB Konfiguration ein oder aus.
        /// </summary>
        /// <param name="sender">Wird ignioriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdDish_Click( object sender, EventArgs e )
        {
            // Swap
            pnlSpecial.Visible = !pnlSpecial.Visible;
        }

        #region IPlugInControl Members

        /// <summary>
        /// Beginnt mit der Ausführung der Aufgabe.
        /// </summary>
        /// <returns>Gesetzt, wenn die Aufgabe synchron abgeschlossen wurde.</returns>
        bool IPlugInControl.Start()
        {
            // Finish
            selGroups.GetSelection();

            // Reset locations
            m_PlugIn.Profile.ScanLocations.Clear();

            // Check mode
            if (m_SatTemplates.Count < 1)
            {
                // The one any only DVB-C or DVB-T
                m_PlugIn.Profile.ScanLocations.Add( selGroups.CurrentTemplate );
            }
            else
            {
                // Choose all
                foreach (ScanTemplate<SatelliteLocation> location in m_SatTemplates)
                {
                    // Load the type
                    DiSEqCLocations selection = location.Location.LNB;

                    // Must match
                    switch (selMode.SelectedIndex)
                    {
                        case 0: if (DiSEqCLocations.None != selection) continue; break;
                        case 1: if ((DiSEqCLocations.BurstOn != selection) && (DiSEqCLocations.BurstOff != selection)) continue; break;
                        case 2: if ((DiSEqCLocations.DiSEqC1 != selection) && (DiSEqCLocations.DiSEqC2 != selection) && (DiSEqCLocations.DiSEqC3 != selection) && (DiSEqCLocations.DiSEqC4 != selection)) continue; break;
                        default: continue;
                    }

                    // Remember
                    m_PlugIn.Profile.ScanLocations.Add( location );
                }
            }

            // Just put to disk
            m_PlugIn.Profile.Save();

            // Done
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
                return true;
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

        #endregion

    }
}
