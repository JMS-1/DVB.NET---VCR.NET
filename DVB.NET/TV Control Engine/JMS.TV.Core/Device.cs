using System.Collections.Generic;
using System.Linq;


namespace JMS.TV.Core
{
    /// <summary>
    /// Verwaltete ein einzelnes Gerät.
    /// </summary>
    /// <typeparam name="TSourceType">Die Art der Quellen.</typeparam>
    /// <typeparam name="TRecordingType">Die Art der Identifikation von Aufzeichnungen.</typeparam>
    internal class Device<TSourceType, TRecordingType> where TSourceType : class
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
            m_feeds =
                m_provider
                    .Activate( m_index, source )
                    .Select( sourceOnGroup => new Feed<TSourceType, TRecordingType>( sourceOnGroup, this ) )
                    .ToArray();
        }

        /// <summary>
        /// Aktiviert das Gerät.
        /// </summary>
        public void EnsureDevice()
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
}
