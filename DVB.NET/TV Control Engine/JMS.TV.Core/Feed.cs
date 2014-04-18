

using System;
namespace JMS.TV.Core
{
    /// <summary>
    /// Beschreibt einen einzelnen Sender - in der ersten Version wird es nur Fernsehsender
    /// geben.
    /// </summary>
    public abstract class Feed
    {
        /// <summary>
        /// Gesetzt, wenn dieser Sender gerade vollständig angezeigt wird - Bild, Ton und Videotext.
        /// </summary>
        public bool m_primaryView;

        /// <summary>
        /// Gesetzt, wenn dieser Sender gerade vollständig angezeigt wird - Bild, Ton und Videotext.
        /// </summary>
        public bool IsPrimaryView
        {
            get { return m_primaryView; }
            set
            {
                // Must validate
                if (value && m_secondaryView)
                    throw new InvalidOperationException( "Sender kann nicht gleichzeitig primär und sekundär verwendet werden" );
                else
                    m_primaryView = value;
            }
        }

        /// <summary>
        /// Gesetzt, wenn dieser Sender gerade als Bild-In-Bild (PiP) angezeigt wird.
        /// </summary>
        public bool m_secondaryView;

        /// <summary>
        /// Gesetzt, wenn dieser Sender gerade als Bild-In-Bild (PiP) angezeigt wird.
        /// </summary>
        public bool IsSecondaryView
        {
            get { return m_secondaryView; }
            set
            {
                // Must validate
                if (value && m_primaryView)
                    throw new InvalidOperationException( "Sender kann nicht gleichzeitig primär und sekundär verwendet werden" );
                else
                    m_secondaryView = value;
            }
        }

        /// <summary>
        /// Gesetzt, wenn dieser Sender benutzt wird.
        /// </summary>
        public bool IsActive { get { return m_primaryView || m_secondaryView; } }

        /// <summary>
        /// Gesetzt, wenn dieser Sender bei Bedarf abgeschaltet werden darf.
        /// </summary>
        public bool ReusePossible { get { return !m_primaryView; } }

        /// <summary>
        /// Erstellt die Beschreibung eines Senders.
        /// </summary>
        internal Feed()
        {
        }
    }

    /// <summary>
    /// Beschreibt einen einzelnen Sender - in der ersten Version wird es nur Fernsehsender
    /// geben.
    /// </summary>
    /// <typeparam name="TSourceType">Die Art der Quellen.</typeparam>
    internal class Feed<TSourceType> : Feed
    {
        /// <summary>
        /// Die zugehörige Quelle.
        /// </summary>
        public TSourceType Source { get; private set; }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="source">Die zugehörige Quelle.</param>
        public Feed( TSourceType source )
        {
            Source = source;
        }
    }
}
