using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace JMS.DVB.Administration.SourceScanner
{
    /// <summary>
    /// Mit diesem Dialog wird die Liste der Quellgruppen gepflegt, die bei einer Aktualisierung
    /// nicht berücksichtigt werden sollen.
    /// </summary>
    public partial class GroupExclusion : UserControl, IPlugInControl
    {
        /// <summary>
        /// Die zugehörige Erweiterung.
        /// </summary>
        private ExcludeGroups m_PlugIn;

        /// <summary>
        /// Eine Methode, mit der das Administrationswerkzeug zur
        /// Aktualisierung der Schaltflächen aufgefordert werden kann.
        /// </summary>
        private IPlugInUISite m_Site;

        /// <summary>
        /// Die Formatierungsvorschrift für die Anzahl der ausgewählten Gruppen.
        /// </summary>
        private string m_CountFormat;

        /// <summary>
        /// Erzeugt einen neuen Dialog.
        /// </summary>
        /// <param name="configuration">Die Konfiguration für diese Auswahl.</param>
        /// <param name="site">Die Arbeitsumgebung der Erweiterung - üblicherweise das DVB.NET
        /// Administrationswerkzeug.</param>
        public GroupExclusion( ExcludeGroups configuration, IPlugInUISite site )
        {
            // Remember
            m_PlugIn = configuration;
            m_Site = site;

            // Fill data from designer
            InitializeComponent();

            // Remember
            m_CountFormat = lbGroups.Text;
        }

        /// <summary>
        /// Initalisiert den Dialog.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void GroupExclusion_Load( object sender, EventArgs e )
        {
            // Set the profile name
            lbProfile.Text = string.Format( lbProfile.Text, m_PlugIn.Profile.Name );

            // Map of all deactivated groups
            Dictionary<SourceGroup, bool> deactivated = new Dictionary<SourceGroup, bool>();

            // Fill
            foreach (SourceGroup group in m_PlugIn.Profile.ScanConfiguration.ExcludedSourceGroups)
                deactivated[group] = true;

            // All groups not processed
            Dictionary<SourceGroup, bool> notProcessed = new Dictionary<SourceGroup, bool>( deactivated );

            // Get all the groups
            foreach (GroupLocation location in m_PlugIn.Profile.Locations)
                foreach (SourceGroup group in location.Groups)
                {
                    // Create the item
                    ListViewItem item = lstGroups.Items.Add( group.ToString() );

                    // Attach the group and finish
                    item.Checked = deactivated.ContainsKey( group );
                    item.Tag = group;

                    // Did it
                    notProcessed.Remove( group );
                }

            // Get all groups not yet processed
            foreach (SourceGroup group in notProcessed.Keys)
            {
                // Create the item
                ListViewItem item = lstGroups.Items.Add( group.ToString() );

                // Attach the group and finish
                item.Checked = true;
                item.Tag = group;
            }

            // Set width
            lstGroups.Columns[0].AutoResize( ColumnHeaderAutoResizeStyle.ColumnContent );

            // Update selection
            ShowCount();

            // Update GUI
            if (lstGroups.CheckedItems.Count > 0)
                lstGroups.CheckedItems[0].EnsureVisible();
        }

        /// <summary>
        /// Eine Auswahl wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void lstGroups_ItemChecked( object sender, ItemCheckedEventArgs e )
        {
            // Update
            ShowCount();
        }

        /// <summary>
        /// Zeigt die Anzahl der markierten Quellgruppen an.
        /// </summary>
        private void ShowCount()
        {
            // Show
            lbGroups.Text = string.Format( m_CountFormat, lstGroups.CheckedIndices.Count );
        }

        #region IPlugInControl Members

        /// <summary>
        /// Beginnt mit der Ausführung der Aufgabe.
        /// </summary>
        /// <returns>Gesetzt, wenn die Aufgabe synchron abgeschlossen wurde.</returns>
        bool IPlugInControl.Start()
        {
            // Attach to the scanning filter
            ScanningFilter filter = m_PlugIn.Profile.ScanConfiguration;

            // Wipe out current settings
            filter.ExcludedSourceGroups.Clear();

            // Process all
            foreach (ListViewItem item in lstGroups.CheckedItems)
            {
                // Attach to the group
                SourceGroup group = (SourceGroup) item.Tag;

                // Clone it 
                group = SourceGroup.FromString<SourceGroup>( group.ToString() );

                // Add to list
                filter.ExcludedSourceGroups.Add( group );
            }

            // Store to disk
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
                // We can't 
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

        #endregion
    }
}
