using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;

namespace JMS.DVB.Administration.ProfileManager
{
    /// <summary>
    /// Diese Erweiterung konvertiert ältere (vor 3.5.1) Geräteprofile in das aktuelle
    /// Format.
    /// </summary>
    public class ProfileUpgrade : PlugIn
    {
        /// <summary>
        /// Gesetzt, wenn eine Ausführung möglich ist.
        /// </summary>
        internal bool CanProcess { get; set; }

        /// <summary>
        /// Erzeugt eine neue Erweiterung.
        /// </summary>
        public ProfileUpgrade()
        {
        }

        /// <summary>
        /// Meldet den Anzeigenamen der Erweiterung, so wie er dem Anwender zur Auswahl
        /// der Aufgabe angeboten wird.
        /// </summary>
        public override string DisplayName
        {
            get
            {
                // Just load
                return Properties.Resources.Convert_Long;
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
                return "03";
            }
        }

        /// <summary>
        /// Meldet einen Kurznamen für diese Erweiterung, die auf den einzelnen
        /// Schritten im Administrationswerkzeug zur Anzeige verwendet wird.
        /// </summary>
        public override string ShortName
        {
            get
            {
                // Just load
                return Properties.Resources.Convert_Short;
            }
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
        /// Meldet, ob eine Ausführung möglich ist.
        /// </summary>
        public override bool IsReady
        {
            get 
            { 
                // Report - will be controlled by visual element
                return CanProcess;
            }
        }

        /// <summary>
        /// Erzeugt die Benutzerschnittstelle für diese Erweiterung.
        /// </summary>
        /// <param name="site">Die Arbeitsumgebung der Erweiterung, im Allgemeinen
        /// das Administrationswerkzeug.</param>
        /// <returns>Ein visuelles Element zur Ausführung dieser Aufgabe.</returns>
        public override Control CreateUserInferface( IPlugInUISite site )
        {
            // Forward
            return new ProfileConversion( this, site );
        }
    }
}
