using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Meldet, dass bereits ein Sendersuchlauf ausgeführt wird.
    /// </summary>
    [Serializable]
    public class SourceUpdateActiveFault : CardServerFault
    {
        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        public SourceUpdateActiveFault()
            : base( Properties.Resources.Exception_ScanActive )
        {
        }
    }
}
