using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Wird ausgelöst, wenn eine Quelle aktiviert werden soll, die nicht zum aktiven Geräteprofil gehört.
    /// </summary>
    [Serializable]
    public class ProfileMismatchFault : CardServerFault
    {
        /// <summary>
        /// Der Name des bereits zugeordneten Geräteprofils.
        /// </summary>
        public string ProfileInUse { get; set; }

        /// <summary>
        /// Der Name des angeforderten Geräteprofils.
        /// </summary>
        public string ProfileRequested { get; set; }

        /// <summary>
        /// Wird für die XML Serialisierung benötigt.
        /// </summary>
        public ProfileMismatchFault()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        /// <param name="inUseName">Der Name des bereits zugeordneten Geräteprofils.</param>
        /// <param name="requestName">Der Name des gewünschten Geräteprofils.</param>
        public ProfileMismatchFault( string inUseName, string requestName )
            : base( string.Format( Properties.Resources.Exception_WrongProfile, inUseName, requestName ) )
        {
            // Remember
            ProfileInUse = inUseName;
            ProfileRequested = requestName;
        }
    }
}
