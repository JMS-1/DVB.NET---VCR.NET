using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;

namespace JMS.DVB.Administration.Tools
{
    /// <summary>
    /// Mit Hilfe dieser Erweiterung können DVB SI Tabelle von Datenquellen in
    /// Dateien exportiert werden.
    /// </summary>
    public class TableDumper : PlugInWithProfile
    {
        /// <summary>
        /// Die zuletzt ausgewählte Datenquellen zur Bestimmung der Quellgruppe.
        /// </summary>
        public SourceSelection LastSource { get; set; }

        /// <summary>
        /// Erzeugt eine neue Erweiterung.
        /// </summary>
        public TableDumper()
        {
        }

        /// <summary>
        /// Meldet, ob eine Ausführung möglich ist.
        /// </summary>
        public override bool IsReady
        {
            get
            {
                // As soon as source is selected
                return (null != LastSource);
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
                return "22";
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
                return Properties.Resources.Dumper_Name;
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
                return Properties.Resources.Dumper_ShortName;
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
                return PlugInCategories.Tools;
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
            // Just create
            return new DumperDialog( this, site );
        }
    }
}
