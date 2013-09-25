using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;

namespace JMS.DVB.Administration.ProfileManager
{
    /// <summary>
    /// Die Erweiterung erlaubt es, ein Geräteprofil zu löschen.
    /// </summary>
    public class ProfileDeleter : PlugInWithProfile
    {
        /// <summary>
        /// Erzeugt eine neue Erweiterung.
        /// </summary>
        public ProfileDeleter()
        {
        }

        /// <summary>
        /// Meldet, ob die Bearbeitung der Aufgabe endgültig abgeschlossen ist.
        /// </summary>
        public override bool IsReady
        {
            get
            {
                // No other profile may reference this one
                foreach (Profile profile in JMS.DVB.ProfileManager.AllProfiles)
                    if (0 == string.Compare( profile.UseSourcesFrom, Profile.Name, true ))
                        return false;

                // We are a root
                return true;
            }
        }

        /// <summary>
        /// Meldet den Namen der Erweiterung, die der Anwender zur Auswahl verwendet.
        /// </summary>
        public override string DisplayName
        {
            get
            {
                // Report
                return Properties.Resources.DeleteProfile_Long;
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
                return "02";
            }
        }

        /// <summary>
        /// Meldet einen Kurznamen der Erweiterung, die während der Arbeitsschritte in
        /// der Anzeige verwendet wird.
        /// </summary>
        public override string ShortName
        {
            get
            {
                // Report
                return Properties.Resources.DeleteProfile_Short;
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
                return PlugInCategories.ProfileManagement;
            }
        }

        /// <summary>
        /// Erzeugt die Benutzerschnittstelle zur Durchführung dieser Aufgabe.
        /// </summary>
        /// <param name="site">Die aktuelle Laufzeitumgebung der Erweiterung, üblicherweise
        /// das Administrationswerkzeug.</param>
        /// <returns>Das visuelle Element zur Pflege des Profils.</returns>
        public override Control CreateUserInferface( IPlugInUISite site )
        {
            // Create UI
            return new DeleteConfirmation( this, site );
        }
    }
}
