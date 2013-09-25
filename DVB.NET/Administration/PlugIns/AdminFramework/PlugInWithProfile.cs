using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;

namespace JMS.DVB.Administration
{
    /// <summary>
    /// Hilfsklasse zur Implementierung von Erweiterungen, deren Aufgabe die
    /// Auswahl genau eines Geräteprofils voraus setzt.
    /// </summary>
    public abstract class PlugInWithProfile : PlugIn
    {
        /// <summary>
        /// Initialisiert eine neue Erweiterungsinstanz.
        /// </summary>
        protected PlugInWithProfile()
        {
        }

        /// <summary>
        /// Meldet, wieviele neue Geräteprofile mindestens ausgewählt werden müssen.
        /// </summary>
        public sealed override uint MinimumNewProfiles
        {
            get
            {
                // Report
                return 1;
            }
        }

        /// <summary>
        /// Meldet, wieviele neue Geräteprofile höchstens ausgewählt werden dürfen.
        /// </summary>
        public sealed override uint MaximumNewProfiles
        {
            get
            {
                // Report
                return 1;
            }
        }

        /// <summary>
        /// Meldet das zu verwendene Geräteprofil.
        /// </summary>
        public Profile Profile
        {
            get
            {
                // Must have new profiles
                if (null == SelectedNewProfiles)
                    return null;

                // Must have exactly one
                if (1 != SelectedNewProfiles.Length)
                    return null;

                // Report the one
                return SelectedNewProfiles[0];
            }
        }
    }
}
