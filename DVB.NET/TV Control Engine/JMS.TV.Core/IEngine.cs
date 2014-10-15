using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JMS.TV.Core
{
    /// <summary>
    /// Wird von einer Anzeigeimplementierung angeboten.
    /// </summary>
    public interface IEngine : IDisposable
    {
        /// <summary>
        /// Alle Funktionen zur Steuerung der angezeigten Quelle.
        /// </summary>
        IFrontend Frontend { get; }
    }
}
