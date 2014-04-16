using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace JMS.TV.Core.UnitTests
{
    /// <summary>
    /// Simuliert den Zugriff auf Quellen.
    /// </summary>
    public class FeedProviderMock : IFeedProvider<string>, IEnumerable<string>
    {
        /// <summary>
        /// Meldet die für die meisten Tests geeignete Standardverwaltung.
        /// </summary>
        public static IFeedProvider<string> Default =
            new FeedProviderMock 
            { 
                { "ARD", "WDR", "MDR" },
                { "ZDF", "KIKA" },
                { "RTL", "VOX" },
                { "Pro7", "SAT1" },
            };

        /// <summary>
        /// Alle Quellen.
        /// </summary>
        private readonly List<HashSet<string>> m_sources = new List<HashSet<string>>();

        /// <summary>
        /// Erstellt eine neue Zugriffssimulation.
        /// </summary>
        private FeedProviderMock()
        {
        }

        /// <summary>
        /// Ergänzt eine Quellgruppe - wird eine Quelle einer Quellgruppe empfangen,
        /// so stehen alle anderen Quellen dieser Gruppe auch zur Verfügung.
        /// </summary>
        /// <param name="sourcesInGroup">Die Liste der Quellen der Gruppe.</param>
        public void Add( params string[] sourcesInGroup )
        {
            m_sources.Add( new HashSet<string>( sourcesInGroup ?? Enumerable.Empty<string>(), StringComparer.InvariantCultureIgnoreCase ) );
        }

        /// <summary>
        /// Meldet alle verfügbaren Quellen.
        /// </summary>
        /// <returns>Die Liste aller Sender.</returns>
        public IEnumerator<string> GetEnumerator()
        {
            return m_sources.SelectMany( group => group ).OrderBy( name => name ).GetEnumerator();
        }

        /// <summary>
        /// Meldet alle verfügbaren Quellen.
        /// </summary>
        /// <returns>Die Liste aller Sender.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
