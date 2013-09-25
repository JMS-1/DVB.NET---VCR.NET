using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Meldet, dass kein Sendersuchlauf ausgeführt wird.
    /// </summary>
    [Serializable]
    public class SourceUpdateNotActiveFault : CardServerFault
    {
        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        public SourceUpdateNotActiveFault()
            : base( Properties.Resources.Exception_ScanNotActive )
        {
        }
    }
}
