

namespace JMS.TV.Core.UnitTests.Mock
{
    /// <summary>
    /// Simuliert eine Quelle.
    /// </summary>
    public class SourceMock : ISource
    {
        /// <summary>
        /// Der eindeutige Name der Quelle.
        /// </summary>
        public string UniqueName { get; private set; }

        /// <summary>
        /// Erstellt eine neue Quelle.
        /// </summary>
        private SourceMock()
        {
        }

        /// <summary>
        /// Erstellt eine neue Simulation einer Quelle.
        /// </summary>
        /// <param name="uniqueName">Der eindeutige Name der Quelle.</param>
        /// <returns>Die gewünschte Simulation.</returns>
        public static ISource Create( string uniqueName )
        {
            return new SourceMock { UniqueName = uniqueName };
        }
    }
}
