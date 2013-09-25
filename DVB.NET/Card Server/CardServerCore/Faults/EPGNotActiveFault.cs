using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Meldet, dass die Sammlung der Programmzeitschrift nicht aktiv ist.
    /// </summary>
    [Serializable]
    public class EPGNotActiveFault : CardServerFault
    {
        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        public EPGNotActiveFault()
            : base( Properties.Resources.Exception_CollectionNotActive )
        {
        }
    }
}
