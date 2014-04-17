using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


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
        public static IFeedProvider<string> Default
        {
            get
            {
                return
                    new FeedProviderMock( 1 )
                    { 
                        { "ARD", "WDR", "MDR" },
                        { "ZDF", "KIKA" },
                        { "RTL", "VOX" },
                        { "Pro7", "SAT1" },
                    };
            }
        }

        /// <summary>
        /// Alle Quellen.
        /// </summary>
        private readonly List<HashSet<string>> m_sources = new List<HashSet<string>>();

        /// <summary>
        /// Alle Geräte samt deren Belegung.
        /// </summary>
        private HashSet<string>[] m_devices = { };

        /// <summary>
        /// Meldet alle Geräte.
        /// </summary>
        public ICollection<IEnumerable<string>> Devices { get { return m_devices; } }

        /// <summary>
        /// Meldet die Anzahl der Geräte.
        /// </summary>
        int IFeedProvider<string>.NumberOfDevices { get { return m_devices.Length; } }

        /// <summary>
        /// Reserviert ein Gerät.
        /// </summary>
        /// <param name="index">Die 0-basierte laufende Nummer des gewünschten Gerätes.</param>
        void IFeedProvider<string>.AllocateDevice( int index )
        {
            // Validate
            Assert.IsTrue( (index >= 0) && (index < m_devices.Length), "out of range" );
            Assert.IsNull( m_devices[index], "already allocated" );

            // Allocate
            m_devices[index] = new HashSet<string>( StringComparer.InvariantCultureIgnoreCase );
        }

        /// <summary>
        /// Gibt ein Gerät wieder frei.
        /// </summary>
        /// <param name="index">Die 0-basierte laufende Nummer des gewünschten Gerätes.</param>
        void IFeedProvider<string>.ReleaseDevice( int index )
        {
            // Validate
            Assert.IsTrue( (index >= 0) && (index < m_devices.Length), "out of range" );
            Assert.IsNotNull( m_devices[index], "not allocated" );

            // Deallocate
            m_devices[index] = null;
        }

        /// <summary>
        /// Stellt sicher, dass ein Geräte eine bestimmte Quelle empfängt.
        /// </summary>
        /// <param name="index">Die laufende Nummer des Gerätes.</param>
        /// <param name="source">Die geforderte Quelle.</param>
        /// <returns>Alle Quellen, die nun ohne Umschaltung von diesem gerät empfangen werden können.</returns>
        string[] IFeedProvider<string>.Activate( int index, string source )
        {
            // Validate
            Assert.IsTrue( (index >= 0) && (index < m_devices.Length), "out of range" );
            Assert.IsNotNull( m_devices[index], "not allocated" );

            // Find the source map
            var group = m_sources.SingleOrDefault( g => g.Contains( source ) );

            // Validate
            Assert.IsNotNull( group, "no such source" );

            // Remember
            m_devices[index] = group;

            // Report
            return group.ToArray();
        }

        /// <summary>
        /// Erstellt eine neue Zugriffssimulation.
        /// </summary>
        /// <param name="numberOfDevices">Die Anzahl der unterstützten Geräte.</param>
        private FeedProviderMock( int numberOfDevices )
        {
            m_devices = new HashSet<string>[numberOfDevices];
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
