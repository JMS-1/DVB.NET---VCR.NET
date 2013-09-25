using System;
using JMS.DVB;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace DVBNETAdmin.WizardSteps
{
    /// <summary>
    /// Repräsentiert ein Geräteprofil in einer Auswahlliste.
    /// </summary>
    internal class ProfileItem
    {
        /// <summary>
        /// Das zugehörige Geräteprofil oder <i>null</i>.
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// Ist <see cref="Profile"/> nicht gesetzt, so wird hiermit festgelegt, ob eine
        /// Auswahl zwingend erforderlich ist.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Erzeugt eine neue Repräsentation.
        /// </summary>
        public ProfileItem()
        {
        }

        /// <summary>
        /// Ermittelt einen Anzeigenamen.
        /// </summary>
        /// <returns>Üblicherweise der Name des Profils.</returns>
        public override string ToString()
        {
            // Check mode
            if (null != Profile)
                return Profile.Name;
            else if (IsRequired)
                return Properties.Resources.Profile_Required;
            else
                return Properties.Resources.Profile_Optional;
        }
    }
}
