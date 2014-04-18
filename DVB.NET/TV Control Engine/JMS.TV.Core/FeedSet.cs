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
        public static FeedSet Create<TSourceType>( IFeedProvider<TSourceType> provider ) where TSourceType : class
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

        /// <summary>
        /// Verändert die primäre Anzeige.
        /// </summary>
        /// <param name="source">Der Name des Senders.</param>
        /// <returns>Gesetzt, wenn die Änderung erfolgreich war.</returns>
        public abstract bool TryChangePrimaryView( string source );
    }

    /// <summary>
    /// Beschreibt alle Sender, die zurzeit empfangen werden. 
    /// </summary>
    /// <typeparam name="TSourceType">Die Art der Quellen.</typeparam>
    internal class FeedSet<TSourceType> : FeedSet where TSourceType : class
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
            /// Stellt den Empfang einer Quelle sicher.
            /// </summary>
            /// <param name="source">Die gewünschte Quelle.</param>
            public void EnsureFeed( TSourceType source )
            {
                m_feeds = m_provider.Activate( m_index, source ).Select( s => new Feed<TSourceType>( s ) ).ToArray();
            }

            /// <summary>
            /// Aktiviert das Gerät.
            /// </summary>
            public void EnsurceDevice()
            {
                m_provider.AllocateDevice( m_index );
            }

            /// <summary>
            /// Deaktiviert das Gerät, wenn es nicht mehr benötigt wird.
            /// </summary>
            public void TestIdle()
            {
                if (m_feeds == null)
                    return;
                if (m_feeds.Any( feed => feed.IsPrimaryView || feed.IsSecondaryView ))
                    return;

                m_provider.ReleaseDevice( m_index );
                m_feeds = null;
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

        /// <summary>
        /// Ermittelt einen Sender.
        /// </summary>
        /// <param name="source">Die zugehörige Quelle.</param>
        /// <returns>Der Sender, sofern dieser verfügbar ist.</returns>
        private Feed<TSourceType> FindFeed( TSourceType source )
        {
            return FindFeed( feed => ReferenceEquals( feed.Source, source ) );
        }

        /// <summary>
        /// Ermittelt einen Sender.
        /// </summary>
        /// <param name="filter">Die Suchbedingung.</param>
        /// <returns>Der Sender, sofern dieser verfügbar ist.</returns>
        private Feed<TSourceType> FindFeed( Func<Feed<TSourceType>, bool> filter )
        {
            return m_devices.SelectMany( device => device.Feeds ).SingleOrDefault( filter );
        }

        /// <summary>
        /// Stellt sicher, dass ein Sender empfangen wird.
        /// </summary>
        /// <param name="source">Der gewünschte Sender.</param>
        /// <returns>Gesetzt, wenn ein Empfang möglich ist.</returns>
        private bool EnsureFeed( TSourceType source )
        {
            // First see if there is a device handling the source
            if (FindFeed( source ) != null)
                return true;

            // See if the is any active device idle
            foreach (var device in m_devices)
                if (device.Feeds.Any())
                {
                    // Tune it
                    device.EnsureFeed( source );

                    // Report success
                    return true;
                }

            // See if the is any active not in use
            foreach (var device in m_devices)
                if (!device.Feeds.Any())
                {
                    // Tune it
                    device.EnsurceDevice();
                    device.EnsureFeed( source );

                    // Report success
                    return true;
                }

            // Not found
            return false;
        }

        /// <summary>
        /// Beendet den Zugriff auf nicht mehr benötigte Geräte.
        /// </summary>
        private void TestIdle()
        {
            foreach (var device in m_devices)
                device.TestIdle();
        }

        /// <summary>
        /// Verändert die primäre Anzeige.
        /// </summary>
        /// <param name="sourceName">Die neue primäre Anzeige.</param>
        /// <returns>Gesetzt, wenn die Änderung erfolgreich war.</returns>
        public override bool TryChangePrimaryView( string sourceName )
        {
            // Look it up
            var source = m_provider.Translate( sourceName );
            if (source == null)
                throw new ArgumentException( "unbekannter sender", "sourceName" );

            // Find the feed
            var feed = FindFeed( source );
            if (feed != null)
                if (feed.IsPrimaryView)
                    return true;

            // Prepare the change
            using (var tx = new FeedTransaction( TestIdle ))
            {
                // See if we are secondary
                var wasSecondary = (feed != null) && feed.IsSecondaryView;
                if (wasSecondary)
                    tx.ChangeSecondaryView( feed, false );

                // Locate the current primary view
                var primary = FindFeed( f => f.IsPrimaryView );
                if (primary != null)
                {
                    // Primary operation                
                    tx.ChangePrimaryView( primary, false );

                    // May want to swap views
                    if (wasSecondary)
                        tx.ChangeSecondaryView( primary, true );
                }

                // Make sure we can receive it
                if (!EnsureFeed( source ))
                    return false;

                // Mark as active
                tx.ChangePrimaryView( FindFeed( source ), true );

                // Avoid cleanup
                tx.Commit();

                // Report success
                return true;
            }
        }
    }
}
