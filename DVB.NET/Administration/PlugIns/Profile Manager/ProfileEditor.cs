using System;
using System.Windows.Forms;

using JMS.DVB.Editors;


namespace JMS.DVB.Administration.ProfileManager
{
    /// <summary>
    /// Die Erweiterung erlaubt es, ein Geräteprofil zu verändern.
    /// </summary>
    public class ProfileEditor : PlugInWithProfile
    {
        /// <summary>
        /// Wird verwendet, um die Korrektheit der Konfiguration anzuzeigen.
        /// </summary>
        internal bool IsValid { get; set; }

        /// <summary>
        /// Der aktuell verwendete Pflegedialog.
        /// </summary>
        internal IHardwareEditor CurrentEditor { get; set; }

        /// <summary>
        /// Erzeugt eine neue Erweiterung.
        /// </summary>
        public ProfileEditor()
        {
        }

        /// <summary>
        /// Meldet, ob die Bearbeitung der Aufgabe endgültig abgeschlossen ist.
        /// </summary>
        public override bool IsReady
        {
            get
            {
                // Check it
                return IsValid;
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
                return "01";
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
                return Properties.Resources.EditProfile_Long;
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
                return Properties.Resources.EditProfile_Short;
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
            return new ProfileDialog( this, site );
        }
    }
}
