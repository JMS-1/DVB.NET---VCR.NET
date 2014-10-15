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

        /// <summary>
        /// Alle unmittelbar nutzbaren Quellen - hierbei handelt es sich im Allgemeinen um eine Favoritenliste 
        /// und damit einen Ausschnitt aller bekannten Quellen.
        /// </summary>
        IReadOnlyList<ISource> Sources { get; }
    }
}
