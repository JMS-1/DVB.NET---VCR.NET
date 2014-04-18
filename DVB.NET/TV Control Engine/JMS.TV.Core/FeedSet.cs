using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace JMS.TV.Core
{
    /// <summary>
    /// Beschreibt alle Sender, die zurzeit empfangen werden. 
    /// </summary>
    public abstract class FeedSet
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
        /// <typeparam name="TRecordingType">Die Art der Identifikation von Aufzeichnungen.</typeparam>
        public static FeedSet<TRecordingType> Create<TSourceType, TRecordingType>( IFeedProvider<TSourceType> provider ) where TSourceType : class
        {
            // Validate
            if (ReferenceEquals( provider, null ))
                throw new ArgumentException( "keine Senderverwaltung angegeben", "provider" );
            else
                return new FeedSet<TSourceType, TRecordingType>( provider );
        }

        /// <summary>
        /// Meldet alle über diese Verwaltung verfügbaren Sender.
        /// </summary>
        /// <returns>Die Liste der Sender.</returns>
        public IEnumerable<Feed> Feeds { get { return EnumerateFeeds(); } }

        /// <summary>
        /// Meldet alle über diese Verwaltung verfügbaren Sender.
        /// </summary>
        /// <returns>Die Liste der Sender.</returns>
        protected abstract IEnumerable<Feed> EnumerateFeeds();

        /// <summary>
        /// Ermittelt einen Sender.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <returns>Der gesuchte Sender.</returns>
        public Feed FindFeed( string sourceName )
        {
            return OnFindFeed( sourceName );
        }

        /// <summary>
        /// Ermittelt einen Sender.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <returns>Der gesuchte Sender.</returns>
        protected abstract Feed OnFindFeed( string sourceName );

        /// <summary>
        /// Verändert die primäre Anzeige.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <returns>Gesetzt, wenn die Änderung erfolgreich war.</returns>
        public abstract bool TryStartPrimaryFeed( string sourceName );

        /// <summary>
        /// Aktiviert eine sekundäre Anzeige.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <returns>Gesetzt, wenn die Änderung erfolgreich war.</returns>
        public abstract bool TryStartSecondaryFeed( string sourceName );

        /// <summary>
        /// Beendet eine sekundäre Anzeige.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        public abstract void StopSecondaryFeed( string sourceName );

        /// <summary>
        /// Meldet den primären Sender.
        /// </summary>
        public Feed PrimaryView { get { return Feeds.SingleOrDefault( feed => feed.IsPrimaryView ); } }

        /// <summary>
        /// Meldet alle sekundären Sender.
        /// </summary>
        public IEnumerable<Feed> SecondaryViews { get { return Feeds.Where( feed => feed.IsSecondaryView ); } }
    }

    /// <summary>
    /// Beschreibt alle Sender, die zurzeit empfangen werden. 
    /// </summary>
    /// <typeparam name="TRecordingType">Die Art der Identifikation von Aufzeichnungen.</typeparam>
    public abstract class FeedSet<TRecordingType> : FeedSet
    {
        /// <summary>
        /// Initialisiert eine Senderverwaltung.
        /// </summary>
        internal FeedSet()
        {
        }

        /// <summary>
        /// Ermittelt einen Sender.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <returns>Der gesuchte Sender.</returns>
        protected sealed override Feed OnFindFeed( string sourceName )
        {
            return FindFeed( sourceName );
        }

        /// <summary>
        /// Ermittelt einen Sender.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <returns>Der gesuchte Sender.</returns>
        public abstract new Feed<TRecordingType> FindFeed( string sourceName );

        /// <summary>
        /// Meldet alle über diese Verwaltung verfügbaren Sender.
        /// </summary>
        /// <returns>Die Liste der Sender.</returns>
        public new IEnumerable<Feed<TRecordingType>> Feeds { get { return RecordingFeeds; } }

        /// <summary>
        /// Meldet alle über diese Verwaltung verfügbaren Sender.
        /// </summary>
        /// <returns>Die Liste der Sender.</returns>
        protected override IEnumerable<Feed> EnumerateFeeds() { return RecordingFeeds; }

        /// <summary>
        /// Meldet alle über diese Verwaltung verfügbaren Sender.
        /// </summary>
        /// <returns>Die Liste der Sender.</returns>
        protected abstract IEnumerable<Feed<TRecordingType>> RecordingFeeds { get; }

        /// <summary>
        /// Meldet den primären Sender.
        /// </summary>
        public new Feed<TRecordingType> PrimaryView { get { return RecordingFeeds.SingleOrDefault( feed => feed.IsPrimaryView ); } }

        /// <summary>
        /// Meldet alle sekundären Sender.
        /// </summary>
        public new IEnumerable<Feed<TRecordingType>> SecondaryViews { get { return RecordingFeeds.Where( feed => feed.IsSecondaryView ); } }

        /// <summary>
        /// Beginnt eine Aufzeichnung.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <param name="key">Die Identifikation der Aufzeichnung.</param>
        /// <returns>Gesetzt, wenn der Start erfolgreich war.</returns>
        public abstract bool TryStartRecordingFeed( string sourceName, TRecordingType key );

        /// <summary>
        /// Beendet eine Aufzeichnung.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <param name="key">Die Identifikation der Aufzeichnung.</param>
        public abstract void StopRecordingFeed( string sourceName, TRecordingType key );

        /// <summary>
        /// Meldet alle Sender, auf denen Aufzeichnungen aktiv sind.
        /// </summary>
        public IEnumerable<Feed<TRecordingType>> Recordings { get { return RecordingFeeds.Where( feed => feed.Recordings.Any() ); } }
    }

    /// <summary>
    /// Beschreibt alle Sender, die zurzeit empfangen werden. 
    /// </summary>
    /// <typeparam name="TSourceType">Die Art der Quellen.</typeparam>
    /// <typeparam name="TRecordingType">Die Art der Identifikation von Aufzeichnungen.</typeparam>
    internal class FeedSet<TSourceType, TRecordingType> : FeedSet<TRecordingType> where TSourceType : class
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
            private volatile Feed<TSourceType, TRecordingType>[] m_feeds = null;

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
            public IEnumerable<Feed<TSourceType, TRecordingType>> Feeds { get { return IsAllocated ? m_feeds : Enumerable.Empty<Feed<TSourceType, TRecordingType>>(); } }

            /// <summary>
            /// Gesetzt, wenn das zugehörige Gerät zugewiesen wurde.
            /// </summary>
            public bool IsAllocated { get { return (m_feeds != null); } }

            /// <summary>
            /// Gesetzt, wenn das Gerät zugewiesen wurde aber gerade nicht in Benutzung ist.
            /// </summary>
            public bool IsIdle { get { return IsAllocated && m_feeds.All( feed => !feed.IsActive ); } }

            /// <summary>
            /// Gesetzt, wenn dieses Gerät nur für sekundäre Sender verwendet wird und daher eine Wiederbenutzung für
            /// wichtigere Aufgaben möglich ist.
            /// </summary>
            public bool ReusePossible { get { return IsAllocated && m_feeds.All( feed => feed.ReusePossible ); } }

            /// <summary>
            /// Ermittelt alle sekundären Sender, die gerade in Benutzung sind.
            /// </summary>
            public IEnumerable<Feed<TSourceType, TRecordingType>> SecondaryFeeds { get { return Feeds.Where( feed => feed.IsSecondaryView ); } }

            /// <summary>
            /// Stellt den Empfang einer Quelle sicher.
            /// </summary>
            /// <param name="source">Die gewünschte Quelle.</param>
            public void EnsureFeed( TSourceType source )
            {
                m_feeds = m_provider.Activate( m_index, source ).Select( s => new Feed<TSourceType, TRecordingType>( s ) ).ToArray();
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
                if (!IsIdle)
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
        protected override IEnumerable<Feed<TRecordingType>> RecordingFeeds { get { return m_devices.SelectMany( device => device.Feeds ); } }

        /// <summary>
        /// Ermittelt einen Sender.
        /// </summary>
        /// <param name="source">Die zugehörige Quelle.</param>
        /// <returns>Der Sender, sofern dieser verfügbar ist.</returns>
        private Feed<TSourceType, TRecordingType> FindFeed( TSourceType source )
        {
            return m_devices.SelectMany( device => device.Feeds ).SingleOrDefault( feed => ReferenceEquals( feed.Source, source ) );
        }

        /// <summary>
        /// Stellt sicher, dass eine Aufzeichnung gestartet werden kann.
        /// </summary>
        /// <param name="source">Der gewünschte Sender.</param>
        /// <param name="tx">Die aktuelle Änderungsumgebung.</param>
        /// <returns>Gesetzt, wenn die Aufzeichnung möglich war.</returns>
        private bool EnsureRecordingFeed( TSourceType source, FeedTransaction tx )
        {
            // Make sure we can receive it
            if (EnsurePrimaryFeed( source, tx ))
                return true;

            // See if there is a primary
            var primary = PrimaryView;
            if (primary == null)
                return false;

            // Switch off primary
            tx.ChangePrimaryView( primary, false );

            // Try again
            return EnsurePrimaryFeed( source, tx );
        }

        /// <summary>
        /// Versucht einen Sender für die primäre Anzeige zu aktivieren.
        /// </summary>
        /// <param name="source">Der gewünschte Sender.</param>
        /// <param name="tx">Die aktuelle Änderungsumgebung.</param>
        /// <returns>Gesetzt, wenn die Aktivierung erfolgreich war.</returns>
        private bool EnsurePrimaryFeed( TSourceType source, FeedTransaction tx )
        {
            // Make sure we can receive it
            if (EnsureFeed( source ))
                return true;

            // See if there is any device we can free
            var availableDevice =
                m_devices
                    .Where( device => device.ReusePossible )
                    .Aggregate( default( Device ), ( best, test ) => ((best != null) && (best.SecondaryFeeds.Count() <= test.SecondaryFeeds.Count())) ? best : test );

            // None
            if (availableDevice == null)
                return false;

            // Stop all secondaries
            foreach (var secondaryFeed in availableDevice.SecondaryFeeds)
                tx.ChangeSecondaryView( secondaryFeed, false );

            // Run test again
            return EnsureFeed( source );
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
                if (device.IsIdle)
                {
                    // Tune it
                    device.EnsureFeed( source );

                    // Report success
                    return true;
                }

            // See if the is any active not in use
            foreach (var device in m_devices)
                if (!device.IsAllocated)
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
        /// Ermittelt einen Sender.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <returns>Der gesuchte Sender.</returns>
        public override Feed<TRecordingType> FindFeed( string sourceName )
        {
            // Look it up
            var source = m_provider.Translate( sourceName );
            if (source == null)
                throw new ArgumentException( "unbekannter sender", "sourceName" );
            else
                return FindFeed( source );
        }

        /// <summary>
        /// Verändert die primäre Anzeige.
        /// </summary>
        /// <param name="sourceName">Die neue primäre Anzeige.</param>
        /// <returns>Gesetzt, wenn die Änderung erfolgreich war.</returns>
        public override bool TryStartPrimaryFeed( string sourceName )
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
                var primary = PrimaryView;
                if (primary != null)
                {
                    // Primary operation                
                    tx.ChangePrimaryView( primary, false );

                    // May want to swap views
                    if (wasSecondary)
                        tx.ChangeSecondaryView( primary, true );
                }

                // Make sure we can receive it
                if (!EnsurePrimaryFeed( source, tx ))
                    return false;

                // Mark as active
                tx.ChangePrimaryView( FindFeed( source ), true );

                // Avoid cleanup
                tx.Commit();

                // Report success
                return true;
            }
        }

        /// <summary>
        /// Verändert eine sekundäre Anzeige.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <returns>Gesetzt, wenn die Änderung erfolgreich war.</returns>
        public override bool TryStartSecondaryFeed( string sourceName )
        {
            // Look it up
            var source = m_provider.Translate( sourceName );
            if (source == null)
                throw new ArgumentException( "unbekannter sender", "sourceName" );

            // Find the feed
            var feed = FindFeed( source );
            if (feed != null)
                if (feed.IsSecondaryView)
                    return true;
                else if (feed.IsPrimaryView)
                    return false;

            // Prepare the change
            using (var tx = new FeedTransaction( TestIdle ))
            {
                // Make sure we can receive it
                if (!EnsureFeed( source ))
                    return false;

                // Mark as active
                tx.ChangeSecondaryView( FindFeed( source ), true );

                // Avoid cleanup
                tx.Commit();

                // Report success
                return true;
            }
        }

        /// <summary>
        /// Verändert eine sekundäre Anzeige.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        public override void StopSecondaryFeed( string sourceName )
        {
            // Look it up
            var source = m_provider.Translate( sourceName );
            if (source == null)
                throw new ArgumentException( "unbekannter sender", "sourceName" );

            // Find the feed
            var feed = FindFeed( source );
            if (feed == null)
                return;
            else if (!feed.IsSecondaryView)
                return;

            // Prepare the change
            using (var tx = new FeedTransaction( TestIdle ))
            {
                // Mark as active
                tx.ChangeSecondaryView( feed, false );

                // Avoid cleanup
                tx.Commit();
            }
        }

        /// <summary>
        /// Beginnt eine Aufzeichnung.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <param name="key">Die Identifikation der Aufzeichnung.</param>
        /// <returns>Gesetzt, wenn der Start erfolgreich war.</returns>
        public override bool TryStartRecordingFeed( string sourceName, TRecordingType key )
        {
            // Look it up
            var source = m_provider.Translate( sourceName );
            if (source == null)
                throw new ArgumentException( "unbekannter sender", "sourceName" );

            // Find the feed
            var feed = FindFeed( source );
            if (feed != null)
                if (feed.IsRecording( key ))
                    return false;

            // Prepare the change
            using (var tx = new FeedTransaction( TestIdle ))
            {
                // Make sure we can receive it
                if (!EnsureRecordingFeed( source, tx ))
                    return false;

                // Attach to the feed
                var recording = FindFeed( source );

                // Mark as active
                tx.ChangeRecording( recording, key, true );

                // May want to make it primary
                if (PrimaryView == null)
                    tx.ChangePrimaryView( recording, true );

                // Avoid cleanup
                tx.Commit();

                // Report success
                return true;
            }
        }

        /// <summary>
        /// Beendet eine Aufzeichnung.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <param name="key">Die Identifikation der Aufzeichnung.</param>
        public override void StopRecordingFeed( string sourceName, TRecordingType key )
        {
            // Look it up
            var source = m_provider.Translate( sourceName );
            if (source == null)
                throw new ArgumentException( "unbekannter sender", "sourceName" );

            // Find the feed
            var feed = FindFeed( source );
            if (feed == null)
                return;
            else if (!feed.IsRecording( key ))
                return;

            // Prepare the change
            using (var tx = new FeedTransaction( TestIdle ))
            {
                // Mark as active
                tx.ChangeRecording( feed, key, false );

                // Avoid cleanup
                tx.Commit();
            }
        }
    }
}
