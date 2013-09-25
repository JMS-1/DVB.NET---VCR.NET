using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;

namespace JMS.DVB.Administration.ProfileManager
{
    /// <summary>
    /// Diese Erweiterung wird zum Anlegen eines neuen Geräteprofils verwendet.
    /// </summary>
    public class NewProfile : PlugIn
    {
        /// <summary>
        /// Meldet oder legt fest, ob die Aufgabe ausgeführt werden kann.
        /// </summary>
        internal bool CanProcess { get; set; }

        /// <summary>
        /// Erzeugt eine neue Erweiterung.
        /// </summary>
        public NewProfile()
        {
        }

        /// <summary>
        /// Meldet die Art dieser Erweiterung.
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
        /// Die Anzeigereihenfolge für diese Erweiterung.
        /// </summary>
        public override string DisplayPriority
        {
            get
            {
                // Report
                return "00";
            }
        }

        /// <summary>
        /// Meldet den Anzeigenamen der Erweiterung, der vom Anwender zur Auswahl verwendet wird.
        /// </summary>
        public override string DisplayName
        {
            get
            {
                // Report
                return Properties.Resources.NewProfile_Long;
            }
        }

        /// <summary>
        /// Meldet eine Kurzbezeichnung für diese Erweiterung, die während der 
        /// Ausführung der Aufgabe angezeigt wird.
        /// </summary>
        public override string ShortName
        {
            get
            {
                // Report
                return Properties.Resources.NewProfile_Short;
            }
        }

        /// <summary>
        /// Erzeugt eine Benutzerschnittstelle zur Ausführung dieser Aufgabe.
        /// </summary>
        /// <param name="site">Die Arbeitsumgebung für die Ausführung der Aufgabe,
        /// üblicherweise das DVB.NET Administrationswerkzeug.</param>
        /// <returns>Die gewünschte Benutzerschnittstelle.</returns>
        public override Control CreateUserInferface( IPlugInUISite site )
        {
            // Create new UI
            return new ProfileCreator( this, site );
        }

        /// <summary>
        /// Meldet, ob die Aufgabe ausgeführt werden kann.
        /// </summary>
        public override bool IsReady
        {
            get
            {
                // Use flag filled by GUI
                return CanProcess;
            }
        }
    }
}
