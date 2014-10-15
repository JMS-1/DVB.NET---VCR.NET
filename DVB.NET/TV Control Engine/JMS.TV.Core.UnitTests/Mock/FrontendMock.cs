using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JMS.TV.Core.UnitTests.Mock
{
    /// <summary>
    /// Simuliert die Anzeige einer Quelle.
    /// </summary>
    public class FrontendMock : IFrontend
    {
        /// <summary>
        /// Die zugehörige Gesamtsimulation.
        /// </summary>
        private readonly EngineMock m_engine;

        /// <summary>
        /// Erstellt eine neue Simulation.
        /// </summary>
        /// <param name="engine">Die zugehörige Gesamtsimulation.</param>
        public FrontendMock( EngineMock engine )
        {
            m_engine = engine;
        }
    }
}
