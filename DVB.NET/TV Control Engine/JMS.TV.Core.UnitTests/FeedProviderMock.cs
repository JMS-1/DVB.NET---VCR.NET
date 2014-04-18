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
    public class FeedProviderMock : IFeedProvider<FeedProviderMock.Source>, IEnumerable<FeedProviderMock.Source>
    {
        /// <summary>
        /// Repräsentiert eine einzelne Quelle.
        /// </summary>
        public sealed class Source : IEquatable<Source>
        {
            /// <summary>
            /// Der Algorithmus zum Vergleich von Namen.
            /// </summary>
            private static readonly StringComparer _Comparer = StringComparer.InvariantCultureIgnoreCase;

            /// <summary>
            /// Der Name der Quelle.
            /// </summary>
            private readonly string m_name;

            /// <summary>
            /// Erstellt eine neue Quelle.
            /// </summary>
            /// <param name="name">Der Name der Quelle.</param>
            private Source( string name )
            {
                m_name = name ?? string.Empty;
            }

            /// <summary>
            /// Erstellt eine neue Quelle.
            /// </summary>
            /// <param name="name">Der Name der Quelle.</param>
            /// <returns>Die gewünschte Quelle.</returns>
            public static implicit operator Source( string name ) { return new Source( name ); }

            /// <summary>
            /// Vergleicht eine Quelle mit einer anderen.
            /// </summary>
            /// <param name="other">Irgendeine Quelle.</param>
            /// <returns>Gesetzt, wenn es sich eine Quelle mit dem selben Namen handelt.</returns>
            public bool Equals( Source other )
            {
                if (ReferenceEquals( other, null ))
                    return false;
                else
                    return _Comparer.Equals( m_name, other.m_name );
            }

            /// <summary>
            /// Vergleicht eine Quelle mit einem beliebigen Objekt.
            /// </summary>
            /// <param name="obj">Irgendein Objekt.</param>
            /// <returns>Gesetzt, wenn das Objekt eine Quelle mit dem selben Namen ist.</returns>
            public override bool Equals( object obj )
            {
                return Equals( obj as Source );
            }

            /// <summary>
            /// Meldet ein Kürzel für die Quelle.
            /// </summary>
            /// <returns>Das gewünschte Kürzel.</returns>
            public override int GetHashCode()
            {
                return _Comparer.GetHashCode( m_name );
            }

            /// <summary>
            /// Meldet den Namen der Quelle.
            /// </summary>
            /// <returns>Der Name der Quelle.</returns>
            public override string ToString()
            {
                return m_name;
            }
        }

        /// <summary>
        /// Meldet die für die meisten Tests geeignete Standardverwaltung.
        /// </summary>
        public static FeedProviderMock Default
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
        private readonly List<HashSet<Source>> m_sources = new List<HashSet<Source>>();

        /// <summary>
        /// Alle Geräte samt deren Belegung.
        /// </summary>
        private HashSet<Source>[] m_devices = { };

        /// <summary>
        /// Meldet alle Geräte.
        /// </summary>
        public ICollection<IEnumerable<Source>> Devices { get { return m_devices; } }

        /// <summary>
        /// Meldet die Anzahl der Geräte.
        /// </summary>
        int IFeedProvider<Source>.NumberOfDevices { get { return m_devices.Length; } }

        /// <summary>
        /// Reserviert ein Gerät.
        /// </summary>
        /// <param name="index">Die 0-basierte laufende Nummer des gewünschten Gerätes.</param>
        void IFeedProvider<Source>.AllocateDevice( int index )
        {
            // Validate
            Assert.IsTrue( (index >= 0) && (index < m_devices.Length), "out of range" );
            Assert.IsNull( m_devices[index], "already allocated" );

            // Allocate
            m_devices[index] = new HashSet<Source>();
        }

        /// <summary>
        /// Gibt ein Gerät wieder frei.
        /// </summary>
        /// <param name="index">Die 0-basierte laufende Nummer des gewünschten Gerätes.</param>
        void IFeedProvider<Source>.ReleaseDevice( int index )
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
        Source[] IFeedProvider<Source>.Activate( int index, Source source )
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
            m_devices = new HashSet<Source>[numberOfDevices];
        }

        /// <summary>
        /// Ergänzt eine Quellgruppe - wird eine Quelle einer Quellgruppe empfangen,
        /// so stehen alle anderen Quellen dieser Gruppe auch zur Verfügung.
        /// </summary>
        /// <param name="sourcesInGroup">Die Liste der Quellen der Gruppe.</param>
        public void Add( params Source[] sourcesInGroup )
        {
            m_sources.Add( new HashSet<Source>( sourcesInGroup ?? Enumerable.Empty<Source>() ) );
        }

        /// <summary>
        /// Meldet alle verfügbaren Quellen.
        /// </summary>
        /// <returns>Die Liste aller Sender.</returns>
        public IEnumerator<Source> GetEnumerator()
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
        Source IFeedProvider<Source>.Translate( string sourceName )
        {
            Source asKey = sourceName;

            return this.SingleOrDefault( asKey.Equals );
        }

        /// <summary>
        /// Prüft, ob ein bestimmtes Gerät einen bestimmten Sender empfängt.
        /// </summary>
        /// <param name="index">Die 0-basierte laufende Nummer des Gerätes.</param>
        /// <param name="source">Die gewünschte Quelle.</param>
        public void AssertDevice( int index, string source )
        {
            // Load map 
            var group = m_devices[index];

            // Test
            Assert.IsNotNull( group, "device {0} not in use", index );
            Assert.IsTrue( group.Contains( source ), "device {0} not receiving source {1}", index, source );
        }
    }
}
