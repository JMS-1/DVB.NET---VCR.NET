using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JMS.DVB;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace JMS.TV.Core.UnitTests
{
    /// <summary>
    /// Simuliert den Zugriff auf Quellen.
    /// </summary>
    public class FeedProviderMock : IFeedProvider, IEnumerable<SourceSelection>
    {
        /// <summary>
        /// Meldet die für die meisten Tests geeignete Standardverwaltung.
        /// </summary>
        /// <param name="numberOfDevices">Die Anzahl der zu verwendenden Geräte.</param>
        public static FeedProviderMock CreateDefault( int numberOfDevices = 4 )
        {
            return
                new FeedProviderMock( numberOfDevices )
                    { 
                        { "ARD", "WDR", "MDR" },
                        { "ZDF", "KIKA" },
                        { "RTL", "VOX" },
                        { "Pro7", "SAT1" },
                    };
        }

        /// <summary>
        /// Hilft bei dem Erstellen von Quellbeschreibungen.
        /// </summary>
        private readonly Dictionary<string, SourceSelection> m_mockedSources = new Dictionary<string, SourceSelection>( StringComparer.InvariantCultureIgnoreCase );

        /// <summary>
        /// Alle Quellen.
        /// </summary>
        private readonly List<HashSet<SourceSelection>> m_sources = new List<HashSet<SourceSelection>>();

        /// <summary>
        /// Alle Geräte samt deren Belegung.
        /// </summary>
        private HashSet<SourceSelection>[] m_devices = { };

        /// <summary>
        /// Meldet alle Geräte.
        /// </summary>
        public ICollection<IEnumerable<SourceSelection>> Devices { get { return m_devices; } }

        /// <summary>
        /// Meldet die Anzahl der Geräte.
        /// </summary>
        int IFeedProvider.NumberOfDevices { get { return m_devices.Length; } }

        /// <summary>
        /// Reserviert ein Gerät.
        /// </summary>
        /// <param name="index">Die 0-basierte laufende Nummer des gewünschten Gerätes.</param>
        void IFeedProvider.AllocateDevice( int index )
        {
            // Validate
            Assert.IsTrue( (index >= 0) && (index < m_devices.Length), "out of range" );
            Assert.IsNull( m_devices[index], "already allocated" );

            // Allocate
            m_devices[index] = new HashSet<SourceSelection>();
        }

        /// <summary>
        /// Gibt ein Gerät wieder frei.
        /// </summary>
        /// <param name="index">Die 0-basierte laufende Nummer des gewünschten Gerätes.</param>
        void IFeedProvider.ReleaseDevice( int index )
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
        SourceSelection[] IFeedProvider.Activate( int index, SourceSelection source )
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
            m_devices = new HashSet<SourceSelection>[numberOfDevices];
        }

        /// <summary>
        /// Ergänzt eine Quellgruppe - wird eine Quelle einer Quellgruppe empfangen,
        /// so stehen alle anderen Quellen dieser Gruppe auch zur Verfügung.
        /// </summary>
        /// <param name="sourcesInGroup">Die Liste der Quellen der Gruppe.</param>
        public void Add( params string[] sourcesInGroup )
        {
            m_sources.Add( new HashSet<SourceSelection>( sourcesInGroup.Select( CreateSourceSelection ) ?? Enumerable.Empty<SourceSelection>() ) );
        }

        /// <summary>
        /// Meldet alle verfügbaren Quellen.
        /// </summary>
        /// <returns>Die Liste aller Sender.</returns>
        public IEnumerator<SourceSelection> GetEnumerator()
        {
            return m_sources.SelectMany( group => group ).OrderBy( source => source.ToString() ).GetEnumerator();
        }

        /// <summary>
        /// Meldet alle verfügbaren Quellen.
        /// </summary>
        /// <returns>Die Liste aller Sender.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Ermittelt zu einem Namen die zugehörige Quelle.
        /// </summary>
        /// <param name="sourceName">Der Name der Quelle.</param>
        /// <returns>Die Quelle.</returns>
        SourceSelection IFeedProvider.Translate( string sourceName )
        {
            return this.SingleOrDefault( CreateSourceSelection( sourceName ).Equals );
        }

        /// <summary>
        /// Prüft, ob ein bestimmtes Gerät einen bestimmten Sender empfängt.
        /// </summary>
        /// <param name="index">Die 0-basierte laufende Nummer des Gerätes.</param>
        /// <param name="sources">Die gewünschte Quelle.</param>
        public void AssertDevice( int index, params string[] sources )
        {
            // Load map 
            var group = m_devices[index];

            // Test
            Assert.IsNotNull( group, "device {0} not in use", index );
            foreach (var source in sources)
                Assert.IsTrue( group.Contains( CreateSourceSelection( source ) ), "device {0} not receiving source {1}", index, source );
        }

        /// <summary>
        /// Prüft, ob ein Geräte in Benutzung sind.
        /// </summary>
        /// <param name="indexes">Die Nummern der Geräte</param>
        public void AssertIdle( params int[] indexes )
        {
            foreach (var index in indexes)
                Assert.IsNull( m_devices[index], "device {0} is in use", index );
        }

        /// <summary>
        /// Erstellt eine passende Senderverwaltung.
        /// </summary>
        /// <returns>Die angeforderte Verwaltung.</returns>
        public IFeedSet CreateFeedSet()
        {
            // Create
            var feedSet = TvController.CreateFeedSet( this );

            // Configure
            feedSet.PrimaryViewVisibilityChanged += ( f, v ) => Trace.TraceInformation( "Primary {0}: {1}", v ? "on" : "off", f );
            feedSet.SecondaryViewVisibilityChanged += ( f, v ) => Trace.TraceInformation( "Secondary {0}: {1}", v ? "on" : "off", f );
            feedSet.RecordingStateChanged += ( f, r, v ) => Trace.TraceInformation( "Recording {0} now {1}: {2}", r, v ? "on" : "off", f );

            // Report
            return feedSet;
        }

        /// <summary>
        /// Erstellt eine neue Quellinformation.
        /// </summary>
        /// <param name="sourceName">Der Name der Quelle.</param>
        /// <returns>Die Beschreibung der Quelle.</returns>
        private SourceSelection CreateSourceSelection( string sourceName )
        {
            // Already knowing it
            SourceSelection source;
            if (!m_mockedSources.TryGetValue( sourceName, out source ))
            {
                // Create new
                source =
                    new SourceSelection
                    {
                        Location = new TerrestrialLocation(),
                        Group = new TerrestrialGroup(),
                        DisplayName = sourceName,
                        ProfileName = "mocked",
                        Source =
                            new Station
                            {
                                Service = (ushort) (m_mockedSources.Count + 1),
                                SourceType = SourceTypes.TV,
                                TransportStream = 1,
                                Name = sourceName,
                                Provider = "mock",
                                Network = 1,
                            },
                    };

                // Remember it
                m_mockedSources.Add( sourceName, source );
            }

            // Report
            return source;
        }
    }
}
