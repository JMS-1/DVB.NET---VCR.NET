using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.Administration
{
    /// <summary>
    /// Diese Schnittstelle wird vom visuellen Element einer Erweiterung angeboten.
    /// </summary>
    public interface IPlugInControl
    {
        /// <summary>
        /// Beginnt mit der Ausführung der Aufgabe.
        /// </summary>
        /// <returns>Gesetzt, wenn die Aufgabe synchron abgeschlossen wurde.</returns>
        bool Start();

        /// <summary>
        /// Meldet, ob eine Aufgabe unterbrochen werden kann.
        /// </summary>
        bool CanCancel { get; }

        /// <summary>
        /// Prüft, ob eine Ausführung möglich ist.
        /// </summary>
        /// <returns>Gesetzt, wenn <see cref="Start"/> aufgerufen werden darf.</returns>
        bool TestStart();
    }
}
