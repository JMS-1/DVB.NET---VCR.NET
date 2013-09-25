using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Meldet, dass die Sammlung der Programmzeitschrift aktiv ist.
    /// </summary>
    [Serializable]
    public class EPGActiveFault : CardServerFault
    {
        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        public EPGActiveFault()
            : base( Properties.Resources.Exception_CollectionActive )
        {
        }
    }
}
