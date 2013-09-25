using System.Windows.Forms;

namespace JMS.DVB.Administration.Tools
{
    /// <summary>
    /// Diese Erweiterung zeigt nach der Auswahl einer Quelle Informationen zum
    /// Empfang der Teildatenströme an.
    /// </summary>
    public class SourceStreamsAndStrength : PlugInWithProfile
    {
        /// <summary>
        /// Die zuletzt ausgewählte Quelle.
        /// </summary>
        public SourceSelection LastSource { get; set; }

        /// <summary>
        /// Erzeugt eine neue Erweiterung.
        /// </summary>
        public SourceStreamsAndStrength()
        {
        }

        /// <summary>
        /// Meldet, ob eine Ausführung möglich ist.
        /// </summary>
        public override bool IsReady
        {
            get
            {
                // Never
                return false;
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
                return "20";
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
                return Properties.Resources.SourceStreams_Name;
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
                return Properties.Resources.SourceStreams_ShortName;
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
            return new StreamDisplay( this, site );
        }
    }
}
