using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JMS.TV.Core
{
    /// <summary>
    /// Beschreibt alle Sender, die zurzeit empfangen werden. 
    /// </summary>
    public abstract class FeedSet : IEnumerable<Feed>
    {
        /// <summary>
        /// Initialisiert eine Senderverwaltung.
        /// </summary>
        internal FeedSet()
        {
        }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="provider">Die Verwaltung aller Sender.</param>
        /// <typeparam name="TSourceType">Die Art der Quellen.</typeparam>
        public static FeedSet Create<TSourceType>( IFeedProvider<TSourceType> provider )
        {
            // Validate
            if (ReferenceEquals( provider, null ))
                throw new ArgumentException( "keine Senderverwaltung angegeben", "provider" );

            // Forward
            return new FeedSet<TSourceType>( provider );
        }

        /// <summary>
        /// Meldet alle über diese Verwaltung verfügbaren Sender.
        /// </summary>
        /// <returns>Die Liste der Sender.</returns>
        public abstract IEnumerator<Feed> GetEnumerator();

        /// <summary>
        /// Meldet alle über diese Verwaltung verfügbaren Sender.
        /// </summary>
        /// <returns>Die Liste der Sender.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Beschreibt alle Sender, die zurzeit empfangen werden. 
    /// </summary>
    /// <typeparam name="TSourceType">Die Art der Quellen.</typeparam>
    internal class FeedSet<TSourceType> : FeedSet
    {
        /// <summary>
        /// Verwaltete ein einzelnes Gerät.
        /// </summary>
        private class Device
        {
            /// <summary>
            /// Verwaltet alle verfügbaren Sender.
            /// </summary>
            private readonly IFeedProvider<TSourceType> m_provider;

            /// <summary>
            /// Die laufende Nummer des Geräte.
            /// </summary>
            private readonly int m_index;

            /// <summary>
            /// Alle gerade verfügbaren Sender.
            /// </summary>
            private volatile Feed<TSourceType>[] m_feeds = null;

            /// <summary>
            /// Erstellt ein neues Gerät.
            /// </summary>
            /// <param name="index">Die laufende Nummer des Gerätes.</param>
            /// <param name="provider">Die Verwaltung aller Quellen.</param>
            public Device( int index, IFeedProvider<TSourceType> provider )
            {
                m_provider = provider;
                m_index = index;
            }

            /// <summary>
            /// Meldet alle gerade verfügbaren Sender.
            /// </summary>
            public IEnumerable<Feed<TSourceType>> Feeds { get { return m_feeds ?? Enumerable.Empty<Feed<TSourceType>>(); } }

            /// <summary>
            /// Gesetzt, wenn das Gerät nicht in Benutzung ist.
            /// </summary>
            public bool IsIdle { get { return Feeds.All( feed => feed.UsageCounter < 1 ); } }

            /// <summary>
            /// Ermittelt eine einzelne Quelle.
            /// </summary>
            /// <param name="source">Die gewünschte Quelle.</param>
            /// <returns>Der zugehörige Sender.</returns>
            private Feed<TSourceType> GetFeed( TSourceType source )
            {
                return Feeds.SingleOrDefault( feed => ReferenceEquals( feed.Source, source ) );
            }

            /// <summary>
            /// Beginnt die Nutzung einer Quelle.
            /// </summary>
            /// <param name="source">Die gewünschte Quelle.</param>
            public void Start( TSourceType source )
            {
                // Must start
                if (m_feeds == null)
                {
                    // Attach hardware
                    var sources = m_provider.Start( m_index, source );

                    // Remember all
                    m_feeds = sources.Select( s => new Feed<TSourceType>( s ) ).ToArray();
                }

                // Remember
                GetFeed( source ).UsageCounter += 1;
            }

            /// <summary>
            /// Beendet die Nutzung einer Quelle.
            /// </summary>
            /// <param name="source">Die betroffene Quelle.</param>
            public void Stop( TSourceType source )
            {
                // Count self
                GetFeed( source ).UsageCounter -= 1;
            }

            /// <summary>
            /// Prüft, ob das zugehörige Gerät beendet werden kann.
            /// </summary>
            public void TestIdle()
            {
                // Check all
                if (!IsIdle)
                    return;

                // Forget all
                m_feeds = null;

                // Release hardware
                m_provider.Stop( m_index );
            }
        }

        /// <summary>
        /// Verwaltet alle verfügbaren Sender.
        /// </summary>
        private readonly IFeedProvider<TSourceType> m_provider;

        /// <summary>
        /// Alle zur Verfügung stehenden Geräte.
        /// </summary>
        private readonly Device[] m_devices;

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="provider">Die Verwaltung aller Sender.</param>
        public FeedSet( IFeedProvider<TSourceType> provider )
        {
            m_devices = Enumerable.Range( 0, provider.NumberOfDevices ).Select( i => new Device( i, provider ) ).ToArray();
            m_provider = provider;
        }

        /// <summary>
        /// Meldet alle über diese Verwaltung verfügbaren Sender.
        /// </summary>
        /// <returns>Die Liste der Sender.</returns>
        public override IEnumerator<Feed> GetEnumerator()
        {
            return m_devices.SelectMany( device => device.Feeds ).GetEnumerator();
        }
    }
}
