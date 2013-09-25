using System;
using JMS.DVB;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace CardServerTester
{
    /// <summary>
    /// Verwaltet ein einzelnes Geräteprofil.
    /// </summary>
    internal class ProfileItem
    {
        /// <summary>
        /// Das zugehörige Geräteprofil.
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// Erzeugt eine neue Verwaltungsinstanz.
        /// </summary>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Geräteprofil angegeben.</exception>
        public ProfileItem( Profile profile )
        {
            // Validate
            if (null == profile)
                throw new ArgumentNullException( "profile" );
            
            // Remember
            Profile = profile;
        }

        /// <summary>
        /// Meldet den Namen des zugehörigen Geräteprofil.
        /// </summary>
        /// <returns>Der Name des enthaltenen Profils.</returns>
        public override string ToString()
        {
            // Forward
            return Profile.Name;
        }
    }
}
