using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;

namespace JMS.DVB.Administration.SourceScanner
{
    /// <summary>
    /// Mit dieser Erweiterung wird die Liste der Quellgruppen in einem Geräteprofil verwaltet, 
    /// die bei einer Aktualisierung nicht berücksichtigt werden sollen.
    /// </summary>
    public class ExcludeGroups : PlugInWithProfile
    {
        /// <summary>
        /// Erzeugt eine neue Erweiterungsinstanz.
        /// </summary>
        public ExcludeGroups()
        {
        }

        /// <summary>
        /// Meldet den (lokalisierten) Name der Erweiterung, die vom Anwender zur Auswahl der Aufgabe
        /// verwendet wird.
        /// </summary>
        public override string DisplayName
        {
            get
            {
                // Report
                return Properties.Resources.GroupExclude_Name;
            }
        }

        /// <summary>
        /// Die Anzeigereihenfolge für diese Erweiterung.
        /// </summary>
        public override string DisplayPriority
        {
            get
            {
                // Report
                return "12";
            }
        }

        /// <summary>
        /// Meldet einen (lokalisierten) Kurznamen für diese Erweiterung.
        /// </summary>
        public override string ShortName
        {
            get
            {
                // Report
                return Properties.Resources.GroupExclude_ShortName;
            }
        }

        /// <summary>
        /// Meldet die Art dieser Aufgabe.
        /// </summary>
        public override PlugInCategories Category
        {
            get
            {
                // Report
                return PlugInCategories.SourceScanning;
            }
        }

        /// <summary>
        /// Entfernt aus der Liste der aktuell ausgewählten neuen Geräteprofile die
        /// Profile, die im Rahmen der aktuellen Aufgabe nicht zulässig sind.
        /// </summary>
        /// <param name="profiles">Die Liste der ausgewählten Geräteprofile.</param>
        public override void FilterNewProfiles( List<Profile> profiles )
        {
            // We only accept leaf profiles holding source lists
            for (int i = profiles.Count; i-- > 0; )
                if (!string.IsNullOrEmpty( profiles[i].UseSourcesFrom ))
                    profiles.RemoveAt( i );
                else if (profiles[i].ScanLocations.Count < 1)
                    profiles.RemoveAt( i );
        }

        /// <summary>
        /// Erzeugt eine Benutzerschnittstelle zu dieser Aufgabe.
        /// </summary>
        /// <param name="site">Die Arbeitsumgebung der Erweiterung - üblicherweise das DVB.NET
        /// Administrationswerkzeug.</param>
        /// <returns>Die gewünschte Benutzerschnittstelle.</returns>
        public override Control CreateUserInferface( IPlugInUISite site )
        {
            // Just create
            return new GroupExclusion( this, site );
        }

        /// <summary>
        /// Meldet, ob die Konfiguration ausreicht, um die aktuelle Aufgabe zu starten.
        /// </summary>
        public override bool IsReady
        {
            get
            {
                // Forward
                return (null != Profile);
            }
        }
    }
}
