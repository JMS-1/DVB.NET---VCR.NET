using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Meldet, dass ein Geräteprofil keine eigene Liste von Quellen verwaltet.
    /// </summary>
    [Serializable]
    public class NoSourceListFault : CardServerFault
    {
        /// <summary>
        /// Der Name des bereits nich gefundenen Profils.
        /// </summary>
        public string ProfileName { get; set; }

        /// <summary>
        /// Wird für die XML Serialisierung benötigt.
        /// </summary>
        public NoSourceListFault()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        /// <param name="profileName">Der Name des gewünschten Geräteprofils.</param>
        public NoSourceListFault( string profileName )
            : base( string.Format( Properties.Resources.Exception_NoScan, profileName ) )
        {
            // Remember
            ProfileName = profileName;
        }
    }
}
