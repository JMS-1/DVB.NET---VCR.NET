using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace JMS.DVB.Administration.SourceScanner
{
    /// <summary>
    /// Hilfselement zur Auswahl einer Menge von Quellgruppen für einen Ursprung.
    /// </summary>
    public partial class SourceGroupsSelector : UserControl
    {
        /// <summary>
        /// Format zur Anzeige der bereits ausgewählten Elemente.
        /// </summary>
        private string m_LabelFormat;

        /// <summary>
        /// Die aktuelle gepflegte Konfiguration.
        /// </summary>
        public ScanTemplate CurrentTemplate { get; private set; }

        /// <summary>
        /// Erzeugt ein neues Auswahlelement.
        /// </summary>
        public SourceGroupsSelector()
        {
            // Load designer stuff
            InitializeComponent();

            // Remember
            m_LabelFormat = lbFeedback.Text;
        }

        /// <summary>
        /// Füllt die Auswahlliste passend zum Geräteprofil
        /// </summary>
        /// <param name="profile">Das zu verwendende Geräteprofil.</param>
        public void LoadLocations( Profile profile )
        {
            // Get the type of the locations
            Type locationType = profile.GetScanLocationType();

            // Something is wrong
            if (null == locationType)
                return;

            // Load all
            List<ScanLocation> locations = ScanLocations.Default.Locations.FindAll( l => locationType.IsAssignableFrom( l.GetType() ) );

            // Add to list
            lstLocations.Items.AddRange( locations.ConvertAll( l => new LocationItem( l ) ).ToArray() );
        }

        /// <summary>
        /// Bereitet die Auswahl vor.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void SourceGroupsSelector_Load( object sender, EventArgs e )
        {
            // Set size
            lstLocations.Columns[0].AutoResize( ColumnHeaderAutoResizeStyle.ColumnContent );

            // Refresh GUI
            UpdateSelection();
        }

        /// <summary>
        /// Aktualisiert die Anzeige mit der Anzahl bereits ausgewählter Einträge.
        /// </summary>
        private void UpdateSelection()
        {
            // Just set the selection count
            lbFeedback.Text = string.Format( m_LabelFormat, lstLocations.CheckedItems.Count );
        }

        /// <summary>
        /// Eine Auswahl wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void lstLocations_ItemChecked( object sender, ItemCheckedEventArgs e )
        {
            // Refresh GUI
            UpdateSelection();
        }

        /// <summary>
        /// Überträgt die aktuelle Auswahl.
        /// </summary>
        public void GetSelection()
        {
            // None available
            if (null == CurrentTemplate)
                return;

            // Reset
            CurrentTemplate.ScanLocations.Clear();

            // Add all checked
            foreach (LocationItem item in lstLocations.CheckedItems)
                CurrentTemplate.ScanLocations.Add( item.Location.UniqueName );
        }

        /// <summary>
        /// Markiert die bereits ausgewählten Ursprünge.
        /// </summary>
        /// <param name="template">Das verwendete Muster.</param>
        public void SetSelection( ScanTemplate template )
        {
            // Remember
            CurrentTemplate = template;

            // Reset all
            foreach (LocationItem item in lstLocations.Items)
                item.Checked = false;

            // Reset view
            lstLocations.Items[0].EnsureVisible();

            // Nothing more to to
            if (null == template)
                return;

            // Select
            foreach (string location in template.ScanLocations)
                foreach (LocationItem item in lstLocations.Items)
                    if (Equals( location, item.Location.UniqueName ))
                        item.Checked = true;

            // Make first visible
            foreach (LocationItem item in lstLocations.Items)
                if (item.Checked)
                {
                    // Move in
                    item.EnsureVisible();

                    // Done
                    break;
                }
        }
    }

    /// <summary>
    /// Hilfsmethoden zum Arbeiten mit typisierten Geräteprofilen.
    /// </summary>
    public static class SourceGroupSelectionExtensions
    {
        /// <summary>
        /// Ermittelt zu einem Geräteprofil die Art der Ursprünge für die Akutalisierung der
        /// Quellen (Sendersuchlauf).
        /// </summary>
        /// <param name="profile">Das zu prüfende Geräteprofil.</param>
        /// <returns>Die Art der Ursprünge für den Sendersuchlauf.</returns>
        public static Type GetScanLocationType( this Profile profile )
        {
            // Process
            for (Type profileType = profile.GetType(); null != profileType; profileType = profileType.BaseType)
            {
                // Not possible
                if (!profileType.IsGenericType)
                    continue;
                if (profileType.IsGenericTypeDefinition)
                    continue;
                if (profileType.ContainsGenericParameters)
                    continue;

                // See if this is the expected type
                if (profileType.GetGenericTypeDefinition() != typeof( Profile<,,> ))
                    continue;

                // Not move down to the generic version
                for (Type locationType = profileType.GetGenericArguments()[2]; null != locationType; locationType = locationType.BaseType)
                {
                    // Not possible
                    if (!locationType.IsGenericType)
                        continue;
                    if (locationType.IsGenericTypeDefinition)
                        continue;
                    if (locationType.ContainsGenericParameters)
                        continue;

                    // See if this is the expected type
                    if (locationType.GetGenericTypeDefinition() != typeof( ScanLocation<> ))
                        continue;

                    // Report this one
                    return locationType;
                }
            }

            // Not found
            return null;
        }

        /// <summary>
        /// Ermittelt zu einem Geräteprofil die Art der Quellgruppen.
        /// </summary>
        /// <param name="profile">Das betroffenen Geräteprofil.</param>
        /// <returns>Die Art der Quellgruppe.</returns>
        public static Type GetGroupType( this Profile profile )
        {
            // Process
            for (Type profileType = profile.GetType(); null != profileType; profileType = profileType.BaseType)
            {
                // Not possible
                if (!profileType.IsGenericType)
                    continue;
                if (profileType.IsGenericTypeDefinition)
                    continue;
                if (profileType.ContainsGenericParameters)
                    continue;

                // See if this is the expected type
                if (profileType.GetGenericTypeDefinition() != typeof( Profile<,,> ))
                    continue;

                // Report
                return profileType.GetGenericArguments()[1];
            }

            // Not found
            return null;
        }

        /// <summary>
        /// Ermittelt zu einem Geräteprofil die Konfigurationelemente für den Sendersuchlauf, die
        /// noch nicht optimiert wurden.
        /// </summary>
        /// <param name="profile">Das gewünschte Geräteprofil.</param>
        /// <returns>Die Liste der noch nicht optimierten Elemente.</returns>
        public static ScanTemplate[] GetNonOptimizedScanLocations( this Profile profile )
        {
            // Load
            List<ScanTemplate> locations = new List<ScanTemplate>();

            // Get the type of the scan locations
            Type scanLocationType = profile.GetScanLocationType();

            // All scan locations of the correct type which are not yet optimized
            Dictionary<string, bool> validLocations = new Dictionary<string, bool>();

            // Fill
            foreach (ScanLocation template in ScanLocations.Default.Locations)
                if (template.AutoConvert)
                    if (scanLocationType.IsAssignableFrom( template.GetType() ))
                        if (!string.IsNullOrEmpty( template.UniqueName ))
                            validLocations[template.UniqueName] = true;

            // Process
            foreach (ScanTemplate location in profile.ScanLocations)
                foreach (string name in location.ScanLocations)
                    if (!string.IsNullOrEmpty( name ))
                        if (validLocations.ContainsKey( name ))
                        {
                            // Can use it
                            locations.Add( location );

                            // Next template
                            break;
                        }

            // Report
            return locations.ToArray();
        }
    }
}
