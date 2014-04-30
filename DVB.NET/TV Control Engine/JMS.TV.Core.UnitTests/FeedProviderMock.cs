using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
        /// Beschreibt ein einzelnes Gerät.
        /// </summary>
        private class DeviceMock : IDevice
        {
            /// <summary>
            /// Die aktuell verwendete Quellgruppe.
            /// </summary>
            private volatile HashSet<SourceSelection> m_currentSourceGroup;

            /// <summary>
            /// Die zugehörige Gesamtsimulation.
            /// </summary>
            private readonly FeedProviderMock m_provider;

            /// <summary>
            /// Erstellt die Simulation eines Gerätes.
            /// </summary>
            /// <param name="provider">Die Simulation der Gesamtumgebung.</param>
            public DeviceMock( FeedProviderMock provider )
            {
                m_provider = provider;
            }

            /// <summary>
            /// Aktiviert den Empfang einer Quellgruppe.
            /// </summary>
            /// <param name="source">Eine Quelle der Gruppe.</param>
            /// <returns>Die Hintergrundaufgabe zur Aktivierung.</returns>
            CancellableTask<SourceSelection[]> IDevice.Activate( SourceSelection source )
            {
                // Find the source map
                var group = m_provider.AllAvailableSources.SingleOrDefault( g => g.Contains( source ) );

                Assert.IsNotNull( group, "no such source" );

                m_currentSourceGroup = new HashSet<SourceSelection>();

                // Report
                return CancellableTask<SourceSelection[]>.Run( cancel => (m_currentSourceGroup = group).ToArray() );
            }

            /// <summary>
            /// Beginnt mit dem Laden von Quellinformationen.
            /// </summary>
            /// <param name="source">Die gewünschte Quelle.</param>
            /// <returns>Die Hintergrundaufgabe zum Laden der Quellinformationen.</returns>
            CancellableTask<SourceInformation> IDevice.GetSourceInformationAsync( SourceSelection source )
            {
                Assert.IsNotNull( m_currentSourceGroup, "not allocated" );

                // Make it async
                return CancellableTask<SourceInformation>.Run( cancel => m_currentSourceGroup.Contains( source ) ? new SourceInformation { Source = source.Source } : null );
            }

            /// <summary>
            /// Fordert aktuelle Quellinformationen an.
            /// </summary>
            void IDevice.RefreshSourceInformations()
            {
                Assert.IsNotNull( m_currentSourceGroup, "not allocated" );
            }

            /// <summary>
            /// Prüft, ob ein bestimmte Quellen empfangen werden.
            /// </summary>
            /// <param name="index">Die externe Nummer des Gerätes.</param>
            /// <param name="sources">Die mindestens aktiven Quellen.</param>
            public void AssertDevice( int index, params string[] sources )
            {
                // Load map 
                var group = m_currentSourceGroup;

                // Test
                Assert.IsNotNull( group, "device {0} not in use", index );
                foreach (var source in sources)
                    Assert.IsTrue( group.Contains( m_provider.CreateSourceSelection( source ) ), "device {0} not receiving source {1}", index, source );
            }


            /// <summary>
            /// Prüft, ob ein Gerät in Benutzung ist.
            /// </summary>
            /// <param name="index">Die externe Nummer des Gerätes.</param>
            public void AssertIdle( int index )
            {
                Assert.IsNull( m_currentSourceGroup, "device {0} is in use", index );
            }
        }

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
        private readonly List<HashSet<SourceSelection>> AllAvailableSources = new List<HashSet<SourceSelection>>();

        /// <summary>
        /// Alle Geräte samt deren Belegung.
        /// </summary>
        private DeviceMock[] m_devices = { };

        /// <summary>
        /// Meldet die Anzahl der Geräte.
        /// </summary>
        int IFeedProvider.NumberOfDevices { get { return m_devices.Length; } }

        /// <summary>
        /// Ermittelt ein Gerät.
        /// </summary>
        /// <param name="index">Die 0-basierte laufende Nummer des Gerätes.</param>
        /// <returns>Das gewünschte Gerät.</returns>
        IDevice IFeedProvider.GetDevice( int index )
        {
            // Validate
            Assert.IsTrue( (index >= 0) && (index < m_devices.Length), "out of range" );

            // Allocate
            return m_devices[index];
        }

        /// <summary>
        /// Erstellt eine neue Zugriffssimulation.
        /// </summary>
        /// <param name="numberOfDevices">Die Anzahl der unterstützten Geräte.</param>
        private FeedProviderMock( int numberOfDevices )
        {
            m_devices = Enumerable.Range( 0, numberOfDevices ).Select( i => new DeviceMock( this ) ).ToArray();
        }

        /// <summary>
        /// Ergänzt eine Quellgruppe - wird eine Quelle einer Quellgruppe empfangen,
        /// so stehen alle anderen Quellen dieser Gruppe auch zur Verfügung.
        /// </summary>
        /// <param name="sourcesInGroup">Die Liste der Quellen der Gruppe.</param>
        public void Add( params string[] sourcesInGroup )
        {
            AllAvailableSources.Add( new HashSet<SourceSelection>( sourcesInGroup.Select( CreateSourceSelection ) ?? Enumerable.Empty<SourceSelection>() ) );
        }

        /// <summary>
        /// Meldet alle verfügbaren Quellen.
        /// </summary>
        /// <returns>Die Liste aller Sender.</returns>
        public IEnumerator<SourceSelection> GetEnumerator()
        {
            return AllAvailableSources.SelectMany( group => group ).OrderBy( source => source.ToString() ).GetEnumerator();
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
        public SourceSelection Translate( string sourceName )
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
            m_devices[index].AssertDevice( index, sources );
        }

        /// <summary>
        /// Prüft, ob ein Geräte in Benutzung sind.
        /// </summary>
        /// <param name="indexes">Die Nummern der Geräte</param>
        public void AssertIdle( params int[] indexes )
        {
            foreach (var index in indexes)
                m_devices[index].AssertIdle( index );
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

    /// <summary>
    /// Einige Hilfsmethoden zum einfacheren Testen der <see cref="IFeedSet"/> Schnittstelle.
    /// </summary>
    public static class FeedSetExtensions
    {
        /// <summary>
        /// Aktiviert den primären Sender.
        /// </summary>
        /// <param name="feedSet">Die Verwaltung der Sender.</param>
        /// <param name="sourceName">Der Name einer Quelle.</param>
        /// <returns>Gesetzt, wenn der Sender aktiviert wurde.</returns>
        public static bool TryPrimary( this IFeedSet feedSet, string sourceName )
        {
            // Synchronize
            using (var waiter = new ManualResetEvent( false ))
            {
                // Waiter method
                Action<IFeed, bool> signal = ( feed, visible ) => { if (visible) waiter.Set(); };

                // Attach wait method
                feedSet.PrimaryViewVisibilityChanged += signal;
                try
                {
                    // Process
                    var started = feedSet.TryStartPrimaryFeed( sourceName );

                    // Synchronize
                    if (started)
                        Assert.IsTrue( waiter.WaitOne( 1000 ), "timeout" );

                    // Report
                    return started;
                }
                finally
                {
                    // Detach waiter
                    feedSet.PrimaryViewVisibilityChanged -= signal;
                }
            }
        }

        /// <summary>
        /// Aktiviert eine Aufzeichnung.
        /// </summary>
        /// <param name="feedSet">Die Verwaltung der Sender.</param>
        /// <param name="sourceName">Der Name einer Quelle.</param>
        /// <param name="key">Die Identifikation der Aufzeichnung.</param>
        /// <returns>Gesetzt, wenn der Sender aktiviert wurde.</returns>
        public static bool TryRecord( this IFeedSet feedSet, string sourceName, string key )
        {
            // Synchronize
            using (var waiter = new ManualResetEvent( false ))
            {
                // Waiter method
                Action<IFeed, string, bool> signal = ( feed, rec, start ) => { if (start && (rec == key)) waiter.Set(); };

                // Attach wait method
                feedSet.RecordingStateChanged += signal;
                try
                {
                    // Process
                    var started = feedSet.TryStartRecordingFeed( sourceName, key );

                    // Synchronize
                    if (started)
                        Assert.IsTrue( waiter.WaitOne( 1000 ), "timeout" );

                    // Report
                    return started;
                }
                finally
                {
                    // Detach waiter
                    feedSet.RecordingStateChanged -= signal;
                }
            }
        }

        /// <summary>
        /// Aktiviert einen sekundären Sender.
        /// </summary>
        /// <param name="feedSet">Die Verwaltung der Sender.</param>
        /// <param name="sourceName">Der Name einer Quelle.</param>
        /// <returns>Gesetzt, wenn der Sender aktiviert wurde.</returns>
        public static bool TrySecondary( this IFeedSet feedSet, string sourceName )
        {
            // Synchronize
            using (var waiter = new ManualResetEvent( false ))
            {
                // Waiter method
                Action<IFeed, bool> signal = ( feed, visible ) => { if (visible) waiter.Set(); };

                // Attach wait method
                feedSet.SecondaryViewVisibilityChanged += signal;
                try
                {
                    // Process
                    var started = feedSet.TryStartSecondaryFeed( sourceName );

                    // Synchronize
                    if (started)
                        Assert.IsTrue( waiter.WaitOne( 1000 ), "timeout" );

                    // Report
                    return started;
                }
                finally
                {
                    // Detach waiter
                    feedSet.SecondaryViewVisibilityChanged -= signal;
                }
            }
        }
    }
}
