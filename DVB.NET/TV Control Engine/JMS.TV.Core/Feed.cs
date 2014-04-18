using System;
using System.Collections.Generic;


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
        private bool m_primaryView;

        /// <summary>
        /// Gesetzt, wenn dieser Sender gerade vollständig angezeigt wird - Bild, Ton und Videotext.
        /// </summary>
        public bool IsPrimaryView
        {
            get { return m_primaryView; }
            internal set
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
        private bool m_secondaryView;

        /// <summary>
        /// Gesetzt, wenn dieser Sender gerade als Bild-In-Bild (PiP) angezeigt wird.
        /// </summary>
        public bool IsSecondaryView
        {
            get { return m_secondaryView; }
            internal set
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
        internal virtual bool IsActive { get { return m_primaryView || m_secondaryView; } }

        /// <summary>
        /// Gesetzt, wenn dieser Sender bei Bedarf abgeschaltet werden darf.
        /// </summary>
        internal virtual bool ReusePossible { get { return !m_primaryView; } }

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
    /// <typeparam name="TRecordingType">Die Art der Identifikation für Aufzeichnungen.</typeparam>
    public abstract class Feed<TRecordingType> : Feed
    {
        /// <summary>
        /// Alle Aufzeichnungen auf diesem Sender.
        /// </summary>
        private readonly HashSet<TRecordingType> m_activeRecordings = new HashSet<TRecordingType>();

        /// <summary>
        /// Gesetzt, wenn dieser Sender benutzt wird.
        /// </summary>
        internal override bool IsActive { get { return base.IsActive || (m_activeRecordings.Count > 0); } }

        /// <summary>
        /// Gesetzt, wenn dieser Sender bei Bedarf abgeschaltet werden darf.
        /// </summary>
        internal override bool ReusePossible { get { return base.ReusePossible && (m_activeRecordings.Count < 1); } }

        /// <summary>
        /// Meldet alle aktiven Aufzeichnungen für diesen Sender.
        /// </summary>
        public IEnumerable<TRecordingType> Recordings { get { return m_activeRecordings; } }

        /// <summary>
        /// Prüft, ob eine bestimmte Aufzeichnung bereits aktiv ist.
        /// </summary>
        /// <param name="key">Die Identifikation der Aufzeichnung.</param>
        /// <returns>Gesetzt, wenn die angegebene Aufzeichnung aktiv ist.</returns>
        public bool IsRecording( TRecordingType key ) { return m_activeRecordings.Contains( key ); }

        /// <summary>
        /// Beendet eine Aufzeichnung.
        /// </summary>
        /// <param name="key">Die Identifikation der Aufzeichnung.</param>
        /// <returns>Gesetzt, wenn die Aufzeichnung beendet wurde.</returns>
        internal bool StopRecording( TRecordingType key ) { return m_activeRecordings.Remove( key ); }

        /// <summary>
        /// Beginnt eine Aufzeichnung.
        /// </summary>
        /// <param name="key">Die Identifikation der Aufzeichnung.</param>
        internal void StartRecording( TRecordingType key ) { m_activeRecordings.Add( key ); }

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
    /// <typeparam name="TRecordingType">Die Art der Identifikation für Aufzeichnungen.</typeparam>
    internal class Feed<TSourceType, TRecordingType> : Feed<TRecordingType>
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
