using JMS.DVB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;


namespace JMS.DVBVCR.RecordingService.ProgramGuide
{
    /// <summary>
    /// Represents an EPG event package.
    /// </summary>
    [Serializable]
    [XmlType( "EPGEvents" )]
    public class ProgramGuideEntries : ICloneable
    {
        /// <summary>
        /// Hold all events mapped by the related station.
        /// <seealso cref="SourceIdentifier"/>
        /// </summary>
        [XmlIgnore]
        private Dictionary<SourceIdentifier, OrderedEntries> m_Events = new Dictionary<SourceIdentifier, OrderedEntries>();

        /// <summary>
        /// Eine leere Liste von Sendungen.
        /// </summary>
        private static readonly ProgramGuideEntry[] s_NoEntries = { };

        /// <summary>
        /// Create a new event package.
        /// </summary>
        public ProgramGuideEntries()
        {
        }

        /// <summary>
        /// Fügt eine Liste von Einträgen zu dieser Verwaltung hinzu.
        /// </summary>
        /// <param name="entries">Die zu verwendende Liste.</param>
        public void AddRange( IEnumerable<ProgramGuideEntry> entries )
        {
            // Add in order
            if (entries != null)
                foreach (var entry in entries)
                    Add( entry );
        }

        /// <summary>
        /// Add a single EPG event.
        /// </summary>
        /// <param name="newEvent">A new event.</param>
        public void Add( ProgramGuideEntry newEvent )
        {
            // Create the key
            var key = newEvent.Source;

            // Attach to holder
            OrderedEntries events;
            if (!m_Events.TryGetValue( key, out events ))
                m_Events.Add( key, events = new OrderedEntries() );

            // Forward
            events.Add( newEvent );
        }

        /// <summary>
        /// Prüft, ob für den gewählten Zeitraum ein Eintrag existiert.
        /// </summary>
        /// <param name="source">Die Quelle, deren Einträge untersucht werden sollen.</param>
        /// <param name="start">Der Beginn des Zeitraums (einschließlich).</param>
        /// <param name="end">Das Ende des Zeitraums (ausschließlich).</param>
        /// <returns>Gesetzt, wenn ein Eintrag existiert.</returns>
        public bool HasEntry( SourceIdentifier source, DateTime start, DateTime end )
        {
            // Attach to holder
            OrderedEntries events;
            if (m_Events.TryGetValue( source, out events ))
                return events.HasEntry( start, end );
            else
                return false;
        }

        /// <summary>
        /// Ermittelt den am besten passenden Eintrag aus einem Zeitraum.
        /// </summary>
        /// <typeparam name="TTarget">Die Art der Rückgabewerte.</typeparam>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <param name="start">Der Beginn des Zeitraums.</param>
        /// <param name="end">Das Ende des Zeitraums.</param>
        /// <param name="factory">Methode zum Erzeugen eines Rückgabewertes.</param>
        /// <returns>Der am besten passende Eintrag.</returns>
        public TTarget FindBestEntry<TTarget>( SourceIdentifier source, DateTime start, DateTime end, Func<ProgramGuideEntry, TTarget> factory )
        {
            // Attach to holder
            OrderedEntries events;
            if (m_Events.TryGetValue( source, out events ))
                return events.FindBestEntry( start, end, factory );
            else
                return default( TTarget );
        }

        /// <summary>
        /// Ermittelt einen bestimmten Eintrag.
        /// </summary>
        /// <param name="source">Die Quelle, deren Eintrag ermittelt werden soll.</param>
        /// <param name="start">Der exakte Startzeitpunkt.</param>
        /// <returns>Der gewünschte Eintrag.</returns>
        public ProgramGuideEntry FindEntry( SourceIdentifier source, DateTime start )
        {
            // Attach to holder
            OrderedEntries events;
            if (m_Events.TryGetValue( source, out events ))
                return events.FindEntry( start );
            else
                return null;
        }

        /// <summary>
        /// Report all our events in a serializable form.
        /// </summary>
        public ProgramGuideEntry[] Events
        {
            get
            {
                // Helper
                return m_Events.Values.SelectMany( events => events ).ToArray();
            }
            set
            {
                // Reset
                m_Events.Clear();

                // Fill
                Merge( value );
            }
        }

        /// <summary>
        /// Merge a list of events into this package.
        /// </summary>
        /// <param name="events">The list of events to merge in.</param>
        public void Merge( ProgramGuideEntry[] events )
        {
            // Fill
            if (events != null)
                foreach (var newEvent in events)
                    Add( newEvent );
        }

        /// <summary>
        /// Merge another package into this one.
        /// </summary>
        /// <param name="events">The package to merge in.</param>
        public void Merge( ProgramGuideEntries events )
        {
            // Fill
            if (events != null)
                foreach (var ordered in events.m_Events.Values)
                    foreach (var entry in ordered)
                        Add( entry );
        }

        /// <summary>
        /// Remove any event collected older than two days.
        /// <seealso cref="OrderedEntries.DiscardOld"/>
        /// </summary>
        public void DiscardOld()
        {
            // Forward to all
            foreach (var events in m_Events.Values)
                events.DiscardOld();
        }

        /// <summary>
        /// Meldet alle Einträge der Programmzeitschrift zu einer Quelle.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Die gewünschte Liste.</returns>
        public IEnumerable<ProgramGuideEntry> GetEntries( SourceIdentifier source )
        {
            // Load list
            OrderedEntries entries;
            if (!m_Events.TryGetValue( source, out entries ))
                yield break;

            // Process all as long as caller needs it
            foreach (var entry in entries)
                yield return entry;
        }

        #region ICloneable Members

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Verwaltungsinstanz.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        public ProgramGuideEntries Clone()
        {
            // Create
            var clone = new ProgramGuideEntries();

            // Process
            foreach (var list in m_Events)
                clone.m_Events[list.Key] = list.Value.Clone();

            // Report
            return clone;
        }

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Verwaltungsinstanz.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        object ICloneable.Clone()
        {
            // Forward
            return Clone();
        }

        #endregion
    }
}
