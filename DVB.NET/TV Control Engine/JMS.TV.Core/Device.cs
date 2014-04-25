using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JMS.DVB;


namespace JMS.TV.Core
{
    /// <summary>
    /// Verwaltete ein einzelnes Gerät.
    /// </summary>
    internal class Device
    {
        /// <summary>
        /// Ein leeres Feld von Sendern.
        /// </summary>
        private static readonly Feed[] _NoFeeds = { };

        /// <summary>
        /// Verwaltet alle verfügbaren Sender.
        /// </summary>
        public readonly IFeedProvider Provider;

        /// <summary>
        /// Die laufende Nummer des Geräte.
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// Alle gerade verfügbaren Sender.
        /// </summary>
        private volatile Feed[] m_feeds = null;

        /// <summary>
        /// Erstellt ein neues Gerät.
        /// </summary>
        /// <param name="index">Die laufende Nummer des Gerätes.</param>
        /// <param name="provider">Die Verwaltung aller Quellen.</param>
        public Device( int index, IFeedProvider provider )
        {
            Provider = provider;
            Index = index;
        }

        /// <summary>
        /// Meldet alle gerade verfügbaren Sender.
        /// </summary>
        public IEnumerable<Feed> Feeds { get { return IsAllocated ? m_feeds : Enumerable.Empty<Feed>(); } }

        /// <summary>
        /// Gesetzt, wenn das zugehörige Gerät zugewiesen wurde.
        /// </summary>
        public bool IsAllocated { get { return (m_feeds != null); } }

        /// <summary>
        /// Gesetzt, wenn das Gerät zugewiesen wurde aber gerade nicht in Benutzung ist.
        /// </summary>
        public bool IsIdle { get { return IsAllocated && (m_feeds.Length > 0) && m_feeds.All( feed => !feed.IsActive ); } }

        /// <summary>
        /// Gesetzt, wenn dieses Gerät nur für sekundäre Sender verwendet wird und daher eine Wiederbenutzung für
        /// wichtigere Aufgaben möglich ist.
        /// </summary>
        public bool ReusePossible { get { return IsAllocated && m_feeds.All( feed => feed.ReusePossible ); } }

        /// <summary>
        /// Ermittelt alle sekundären Sender, die gerade in Benutzung sind.
        /// </summary>
        public IEnumerable<Feed> SecondaryFeeds { get { return Feeds.Where( feed => feed.IsSecondaryView ); } }

        /// <summary>
        /// Liest asynchron die Quellen einer Gruppe ein.
        /// </summary>
        private CancellableTask<SourceSelection[]> m_groupReader;

        /// <summary>
        /// Meldet asynchron neue Quellen.
        /// </summary>
        private Task<Feed[]> m_feedAwaiter;

        /// <summary>
        /// Stellt den Empfang einer Quelle sicher.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        public void EnsureFeed( SourceSelection source )
        {
            // Unregister all outstanding requests
            ResetFeeds();

            // Stop current reader
            if (m_groupReader != null)
                m_groupReader.Cancel();

            // Create new
            m_groupReader = Provider.Activate( Index, source );
            m_feeds = _NoFeeds;

            // Start processing
            m_feedAwaiter = m_groupReader.ContinueWith( task =>
                {
                    // Load feed data
                    var sources = task.Result;
                    if (sources != null)
                        if (IsAllocated)
                            return m_feeds = sources.Select( sourceOnGroup => new Feed( sourceOnGroup, this ) ).ToArray();

                    // None
                    return null;
                } );
        }

        /// <summary>
        /// Meldet eine Benachrichtigung an.
        /// </summary>
        /// <param name="source">Eine zu überwachende Quelle.</param>
        /// <param name="sink">Die auszulösende Benachrichtigung.</param>
        public void FireWhenAvailable( SourceIdentifier source, Action<Feed, bool> sink )
        {
            // No one is interested
            if (sink == null)
                return;

            // Can not provide data
            var reader = m_feedAwaiter;
            if (reader == null)
                return;

            // Register
            reader.ContinueWith( task =>
            {
                // See if there are any
                var feeds = task.Result;
                if (feeds == null)
                    return;

                // Find the one
                var feed = feeds.FirstOrDefault( f => f.Source.Source.Equals( source ) );
                if (feed != null)
                    sink( feed, true );
            } );
        }

        /// <summary>
        /// Gibt alle Senderinformationen frei.
        /// </summary>
        public void RefreshSourceInformations()
        {
            // Not in use
            if (!IsAllocated)
                return;

            // Unregister all outstanding requests
            ResetFeeds();

            Provider.RefreshSourceInformations( Index );
        }

        /// <summary>
        /// Gibt alle Senderinformationen frei.
        /// </summary>
        private void ResetFeeds()
        {
            // Unregister all outstanding requests
            if (m_feeds != null)
                foreach (var feed in m_feeds)
                    feed.RefreshSourceInformation();
        }

        /// <summary>
        /// Aktiviert das Gerät.
        /// </summary>
        public void EnsureDevice()
        {
            Provider.AllocateDevice( Index );
        }

        /// <summary>
        /// Meldet eine Änderung.
        /// </summary>
        /// <param name="sink">Der Empfänger der Meldung.</param>
        /// <param name="feed">Der betroffene Sender.</param>
        /// <param name="show">Gesetzt, wenn eine Anzeige erfolgen soll.</param>
        public void OnViewChanged( Action<IFeed, bool> sink, Feed feed, bool show )
        {
            // Not interested
            if (sink == null)
                return;

            // Report
            sink( feed, show );
        }
    }
}
