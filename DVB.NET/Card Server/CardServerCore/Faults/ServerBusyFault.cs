using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Meldet, dass ein <i>Card Server</i> keine neuen Aufgaben annehmen kann, da er gerade beschäftigt 
    /// ist.
    /// </summary>
    [Serializable]
    public class ServerBusyFault : CardServerFault
    {
        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        public ServerBusyFault()
            : base( Properties.Resources.Exception_Busy )
        {
        }
    }
}
