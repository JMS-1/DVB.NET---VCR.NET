using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Meldet, dass bereits ein Geräteprofil zugeordnet ist.
    /// </summary>
    [Serializable]
    public class ProfileAlreadyAttachedFault : CardServerFault
    {
        /// <summary>
        /// Der Name des bereits zugeordneten Geräteprofils.
        /// </summary>
        public string ProfileName { get; set; }

        /// <summary>
        /// Wird für die XML Serialisierung benötigt.
        /// </summary>
        public ProfileAlreadyAttachedFault()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        /// <param name="profileName">Der Name des bereits zugeordneten Geräteprofils.</param>
        public ProfileAlreadyAttachedFault( string profileName )
            : base( string.Format( Properties.Resources.Exception_ProfileAttached, profileName ) )
        {
            // Remember
            ProfileName = profileName;
        }
    }
}
