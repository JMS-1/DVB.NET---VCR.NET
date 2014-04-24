using System.Collections.Generic;
using System.Linq;
using JMS.DVB;


namespace JMS.TV.Core
{
    /// <summary>
    /// Verwaltete ein einzelnes Gerät.
    /// </summary>
    internal class Device
    {
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
        public bool IsIdle { get { return IsAllocated && m_feeds.All( feed => !feed.IsActive ); } }

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
        /// Stellt den Empfang einer Quelle sicher.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        public void EnsureFeed( SourceSelection source )
        {
            ResetFeeds();

            m_feeds =
                Provider
                    .Activate( Index, source )
                    .Select( sourceOnGroup => new Feed( sourceOnGroup, this ) )
                    .ToArray();
        }

        /// <summary>
        /// Gibt alle Senderinformationen frei.
        /// </summary>
        private void ResetFeeds()
        {
            if (m_feeds != null)
                foreach (var feed in m_feeds)
                    feed.RefreshSourceInformation();

            m_feeds = null;
        }

        /// <summary>
        /// Aktiviert das Gerät.
        /// </summary>
        public void EnsureDevice()
        {
            Provider.AllocateDevice( Index );
        }

        /// <summary>
        /// Deaktiviert das Gerät, wenn es nicht mehr benötigt wird.
        /// </summary>
        public void TestIdle()
        {
            if (!IsIdle)
                return;

            ResetFeeds();

            Provider.ReleaseDevice( Index );           
        }
    }
}
