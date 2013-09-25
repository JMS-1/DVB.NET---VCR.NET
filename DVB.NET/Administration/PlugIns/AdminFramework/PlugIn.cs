using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;

namespace JMS.DVB.Administration
{
    /// <summary>
    /// Die Basisklasse zur Implementierung von Erweiterungen für das DVB.NET Administrationswerkzeug.
    /// </summary>
    public abstract class PlugIn
    {
        /// <summary>
        /// Alle ausgewählten Geräteprofile.
        /// </summary>
        public Profile[] SelectedNewProfiles { get; set; }

        /// <summary>
        /// Initialisiert eine Erweiterung.
        /// </summary>
        protected PlugIn()
        {
        }

        /// <summary>
        /// Meldet den (lokalisierten) Name der Erweiterung, die vom Anwender zur Auswahl der Aufgabe
        /// verwendet wird.
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Meldet einen (lokalisierten) Kurznamen für diese Erweiterung.
        /// </summary>
        public abstract string ShortName { get; }

        /// <summary>
        /// Meldet die Art dieser Aufgabe.
        /// </summary>
        public abstract PlugInCategories Category { get; }

        /// <summary>
        /// Die Anzeigereihenfolge für diese Erweiterung.
        /// </summary>
        public abstract string DisplayPriority { get; }

        /// <summary>
        /// Erzeugt eine Benutzerschnittstelle zu dieser Aufgabe.
        /// </summary>
        /// <param name="site">Die Arbeitsumgebung der Erweiterung - üblicherweise das DVB.NET
        /// Administrationswerkzeug.</param>
        /// <returns>Die gewünschte Benutzerschnittstelle.</returns>
        public abstract Control CreateUserInferface( IPlugInUISite site );

        /// <summary>
        /// Meldet, ob die Konfiguration ausreicht, um die aktuelle Aufgabe zu starten.
        /// </summary>
        public abstract bool IsReady { get; }

        /// <summary>
        /// Meldet alle Erweiterungen, die sich im <i>Administration PlugIns</i> Unterverzeichnis
        /// der aktuellen Anwendung befinden.
        /// </summary>
        /// <remarks>Die Liste der Erweiterungen.</remarks>
        public static PlugIn[] CreateFactories()
        {
            // Forward
            return CurrentApplication.CreatePlugInFactories<PlugIn>( "Administration PlugIns" );
        }

        /// <summary>
        /// Meldet, wieviele neue Geräteprofile mindestens ausgewählt werden müssen.
        /// </summary>
        public virtual uint MinimumNewProfiles
        {
            get
            {
                // Report
                return 0;
            }
        }

        /// <summary>
        /// Meldet, wieviele neue Geräteprofile höchstens ausgewählt werden dürfen.
        /// </summary>
        public virtual uint MaximumNewProfiles
        {
            get
            {
                // Report
                return 0;
            }
        }

        /// <summary>
        /// Entfernt aus der Liste der aktuell ausgewählten neuen Geräteprofile die
        /// Profile, die im Rahmen der aktuellen Aufgabe nicht zulässig sind.
        /// </summary>
        /// <param name="profiles">Die Liste der ausgewählten Geräteprofile.</param>
        public virtual void FilterNewProfiles( List<Profile> profiles )
        {
        }
    }
}
