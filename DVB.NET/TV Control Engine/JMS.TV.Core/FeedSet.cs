using System;
using System.Collections.Generic;
using System.Linq;
using JMS.DVB;


namespace JMS.TV.Core
{
    /// <summary>
    /// Beschreibt alle Sender, die zurzeit empfangen werden. 
    /// </summary>
    internal class FeedSet : IFeedSet
    {
        /// <summary>
        /// Verwaltet alle verfügbaren Sender.
        /// </summary>
        private readonly IFeedProvider m_provider;

        /// <summary>
        /// Alle zur Verfügung stehenden Geräte.
        /// </summary>
        private readonly Device[] m_devices;

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="provider">Die Verwaltung aller Sender.</param>
        public FeedSet( IFeedProvider provider )
        {
            m_devices = Enumerable.Range( 0, provider.NumberOfDevices ).Select( i => new Device( i, provider ) ).ToArray();
            m_provider = provider;
        }

        /// <summary>
        /// Ermittelt einen Sender.
        /// </summary>
        /// <param name="source">Die zugehörige Quelle.</param>
        /// <returns>Der Sender, sofern dieser verfügbar ist.</returns>
        public Feed FindFeed( SourceSelection source )
        {
            return Feeds.SingleOrDefault( feed => ReferenceEquals( feed.Source, source ) );
        }

        /// <summary>
        /// Stellt sicher, dass eine Aufzeichnung gestartet werden kann.
        /// </summary>
        /// <param name="source">Der gewünschte Sender.</param>
        /// <param name="tx">Die aktuelle Änderungsumgebung.</param>
        /// <returns>Gesetzt, wenn die Aufzeichnung möglich war.</returns>
        private bool EnsureRecordingFeed( SourceSelection source, FeedTransaction tx )
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

            // Try again - may fail
            return EnsurePrimaryFeed( source, tx );
        }

        /// <summary>
        /// Versucht einen Sender für die primäre Anzeige zu aktivieren.
        /// </summary>
        /// <param name="source">Der gewünschte Sender.</param>
        /// <param name="tx">Die aktuelle Änderungsumgebung.</param>
        /// <returns>Gesetzt, wenn die Aktivierung erfolgreich war.</returns>
        private bool EnsurePrimaryFeed( SourceSelection source, FeedTransaction tx )
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

            // Run test again - can not fail
            return EnsureFeed( source );
        }

        /// <summary>
        /// Stellt sicher, dass ein Sender empfangen wird.
        /// </summary>
        /// <param name="source">Der gewünschte Sender.</param>
        /// <returns>Gesetzt, wenn ein Empfang möglich ist.</returns>
        private bool EnsureFeed( SourceSelection source )
        {
            // First see if there is a device handling the source
            if (FindFeed( source ) != null)
                return true;

            // See if there is any device activated but idle and then reuse it - better than starting a brand new device
            foreach (var device in m_devices)
                if (device.IsIdle)
                {
                    // Tune it
                    device.EnsureFeed( source );

                    // Report success
                    return true;
                }

            // See if there is any device not yet activated
            foreach (var device in m_devices)
                if (!device.IsAllocated)
                {
                    // Tune it
                    device.EnsureDevice();
                    device.EnsureFeed( source );

                    // Report success
                    return true;
                }

            // All devices are in use
            return false;
        }

        /// <summary>
        /// Stellt sicher, dass von nun an alle Quelldaten neu ermittelt werden.
        /// </summary>
        public void RefreshSourceInformations()
        {
            foreach (var device in m_devices)
                device.RefreshSourceInformations();
        }

        /// <summary>
        /// Beendet den Zugriff auf nicht mehr benötigte Geräte.
        /// </summary>
        public void TestIdle()
        {
            foreach (var device in m_devices)
                device.TestIdle();
        }

        /// <summary>
        /// Meldet alle über diese Verwaltung verfügbaren Sender.
        /// </summary>
        /// <returns>Die Liste der Sender.</returns>
        private IEnumerable<Feed> Feeds { get { return m_devices.SelectMany( device => device.Feeds ); } }

        /// <summary>
        /// Meldet den primären Sender.
        /// </summary>
        private Feed PrimaryView { get { return Feeds.SingleOrDefault( feed => feed.IsPrimaryView ); } }

        /// <summary>
        /// Meldet alle sekundären Sender.
        /// </summary>
        private IEnumerable<Feed> SecondaryViews { get { return Feeds.Where( feed => feed.IsSecondaryView ); } }

        /// <summary>
        /// Ermittelt einen Sender.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <returns>Der gesuchte Sender.</returns>
        private Feed FindFeed( string sourceName )
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
        public bool TryStartPrimaryFeed( string sourceName )
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
            using (var tx = new FeedTransaction( this ))
            {
                // See if we are secondary
                if (feed != null)
                    if (feed.IsSecondaryView)
                        tx.ChangeSecondaryView( feed, false );

                // Locate the current primary view
                var previous = PrimaryView;
                if (previous != null)
                    tx.ChangePrimaryView( previous, false );

                // Make sure we can receive it
                if (!EnsurePrimaryFeed( source, tx ))
                    return false;

                // Mark as active
                tx.ChangePrimaryView( source, true );

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
        public bool TryStartSecondaryFeed( string sourceName )
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
            using (var tx = new FeedTransaction( this ))
            {
                // Make sure we can receive it
                if (!EnsureFeed( source ))
                    return false;

                // Mark as active
                tx.ChangeSecondaryView( source, true );

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
        public void StopSecondaryFeed( string sourceName )
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
            using (var tx = new FeedTransaction( this ))
            {
                // Mark as active
                tx.ChangeSecondaryView( feed, false );

                // Avoid cleanup
                tx.Commit();
            }
        }

        /// <summary>
        /// Ermittelt einen Sender.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <returns>Der gesuchte Sender.</returns>
        IFeed IFeedSet.FindFeed( string sourceName )
        {
            return FindFeed( sourceName );
        }

        /// <summary>
        /// Meldet alle über diese Verwaltung verfügbaren Sender.
        /// </summary>
        /// <returns>Die Liste der Sender.</returns>
        IEnumerable<IFeed> IFeedSet.Feeds { get { return Feeds; } }

        /// <summary>
        /// Meldet den primären Sender.
        /// </summary>
        IFeed IFeedSet.PrimaryView { get { return PrimaryView; } }

        /// <summary>
        /// Meldet alle sekundären Sender.
        /// </summary>
        IEnumerable<IFeed> IFeedSet.SecondaryViews { get { return SecondaryViews; } }

        /// <summary>
        /// Meldet alle Sender, auf denen Aufzeichnungen aktiv sind.
        /// </summary>
        IEnumerable<IFeed> IFeedSet.Recordings { get { return Recordings; } }

        /// <summary>
        /// Beendet alle Aufträge.
        /// </summary>
        public void Shutdown()
        {
            // Prepare the change
            using (var tx = new FeedTransaction( this ))
            {
                // All feeds
                foreach (var feed in Feeds)
                {
                    // Primary
                    if (feed.IsPrimaryView)
                        tx.ChangePrimaryView( feed, false );

                    // Secondary
                    if (feed.IsSecondaryView)
                        tx.ChangeSecondaryView( feed, false );

                    // Recording
                    foreach (var recording in feed.Recordings.ToArray())
                        tx.ChangeRecording( feed, recording, false );
                }

                // Process all
                tx.Commit();
            }
        }

        /// <summary>
        /// Wird ausgelöst, wenn sich die Sichtbarkeit des primären Senders tatsächlich verändert hat.
        /// </summary>
        public event Action<IFeed, bool> PrimaryViewVisibilityChanged;

        /// <summary>
        /// Wird ausgelöst, wenn sich die Sichtbarkeit eines sekundären Senders tatsächlich verändert hat.
        /// </summary>
        public event Action<IFeed, bool> SecondaryViewVisibilityChanged;

        /// <summary>
        /// Meldet, dass sich die Sichtbarkeit des primären Senders verändert hat.
        /// </summary>
        /// <param name="feed">Der betroffene Sender.</param>
        /// <param name="show">Gesetzt, wenn der Sender angezeigt werden soll.</param>
        public void OnPrimaryViewChanged( Feed feed, bool show )
        {
            feed.OnViewChanged( PrimaryViewVisibilityChanged, show );
        }

        /// <summary>
        /// Meldet, dass sich die Sichtbarkeit eines sekundären Senders verändert hat.
        /// </summary>
        /// <param name="feed">Der betroffene Sender.</param>
        /// <param name="show">Gesetzt, wenn der Sender angezeigt werden soll.</param>
        public void OnSecondaryViewChanged( Feed feed, bool show )
        {
            feed.OnViewChanged( SecondaryViewVisibilityChanged, show );
        }

        /// <summary>
        /// Meldet alle Sender, auf denen Aufzeichnungen aktiv sind.
        /// </summary>
        private IEnumerable<Feed> Recordings { get { return Feeds.Where( feed => feed.Recordings.Any() ); } }

        /// <summary>
        /// Beginnt eine Aufzeichnung.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <param name="key">Die Identifikation der Aufzeichnung.</param>
        /// <returns>Gesetzt, wenn der Start erfolgreich war.</returns>
        public bool TryStartRecordingFeed( string sourceName, string key )
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
            using (var tx = new FeedTransaction( this ))
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
        public void StopRecordingFeed( string sourceName, string key )
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
            using (var tx = new FeedTransaction( this ))
            {
                // Mark as active
                tx.ChangeRecording( feed, key, false );

                // Avoid cleanup
                tx.Commit();
            }
        }

        /// <summary>
        /// Wird ausgelöst, wenn sich eine Aufzeichnung verändert hat.
        /// </summary>
        public event Action<IFeed, string, bool> RecordingStateChanged;

        /// <summary>
        /// Wird ausgelöst, wenn sich der Zustand eine Aufzeichnung verändert hat.
        /// </summary>
        /// <param name="feed">Der betroffene Sender.</param>
        /// <param name="key">Die betroffene Aufzeichnung.</param>
        /// <param name="start">Gesetzt, wenn die Aufzeichnung begonnen werden soll.</param>
        public void OnRecordingChanged( Feed feed, string key, bool start )
        {
            var sink = RecordingStateChanged;
            if (sink != null)
                sink( feed, key, start );
        }
    }
}
