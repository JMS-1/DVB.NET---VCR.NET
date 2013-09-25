using System.Windows.Forms;
using System.Collections.Generic;

namespace JMS.DVB.Administration.Tools
{
    /// <summary>
    /// Zeigt eine Übersicht über alle Quellgruppen auf allen Ursprüngen an.
    /// </summary>
    public class SignalOverview : PlugInWithProfile
    {
        /// <summary>
        /// Erzeugt eine neue Erweiterung.
        /// </summary>
        public SignalOverview()
        {
        }

        /// <summary>
        /// Meldet, ob eine Ausführung möglich ist.
        /// </summary>
        public override bool IsReady
        {
            get
            {
                // Always
                return true;
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
                return "21";
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
                return Properties.Resources.SignalReport_Name;
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
                return Properties.Resources.SignalReport_ShortName;
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
        /// Entfernt alle Geräteprofile, die keine Signalinformationen liefern.
        /// </summary>
        /// <param name="profiles">Die ursprüngliche Liste von Geräteprofilen, die
        /// geeignet aktualisiert wird.</param>
        public override void FilterNewProfiles( List<Profile> profiles )
        {
            // Just do it
            profiles.RemoveAll( p => !p.GetSafeRestrictions().ProvidesSignalInformation );
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
            return new SignalReport( this, site );
        }
    }

    /// <summary>
    /// Einige Hilfsmethoden zum Arbeiten mit Geräteprofilen.
    /// </summary>
    internal static class SignalExtensions
    {
        /// <summary>
        /// Ermittelt zu einem Geräteprofil die Hardwarebeschränkungen.
        /// </summary>
        /// <param name="profile">Das gewünschte Profil.</param>
        /// <returns>Die gewünschten Beschränkungen, niemals aber <i>null</i>.</returns>
        public static HardwareRestriction GetSafeRestrictions( this Profile profile )
        {
            // Process
            return profile.Restrictions ?? new HardwareRestriction();
        }
    }
}
