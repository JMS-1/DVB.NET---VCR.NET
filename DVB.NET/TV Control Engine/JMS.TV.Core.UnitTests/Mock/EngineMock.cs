using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JMS.TV.Core.UnitTests.Mock
{
    /// <summary>
    /// Erstellt eine neue Anzeigesimulation.
    /// </summary>
    public class EngineMock : IEngine
    {
        /// <summary>
        /// Meldet die Steuerung der aktuellen Anzeige.
        /// </summary>
        public IFrontend Frontend { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Simulation.
        /// </summary>
        private EngineMock()
        {
            Frontend = new FrontendMock( this );
        }

        /// <summary>
        /// Erstellt eine neue Simulation.
        /// </summary>
        /// <returns>Die gewünschte Simulation.</returns>
        public static IEngine Create()
        {
            return new EngineMock();
        }

        /// <summary>
        /// Beendet die Nutzung dieser Simulation endgültig.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
