using System;
using System.Collections;
using System.Collections.Generic;


namespace JMS.DVBVCR.RecordingService.ProgramGuide
{
    /// <summary>
    /// Management instance for EPG events on a single station.
    /// </summary>
    internal class OrderedEntries : IEnumerable<ProgramGuideEntry>, ICloneable
    {
        /// <summary>
        /// All events collected so far.
        /// </summary>
        private readonly List<ProgramGuideEntry> m_Events = new List<ProgramGuideEntry>();

        /// <summary>
        /// Create a new collection instance.
        /// </summary>
        public OrderedEntries()
        {
        }

        /// <summary>
        /// Prüft, ob für den gewählten Zeitraum ein Eintrag existiert.
        /// </summary>
        /// <param name="start">Der Beginn des Zeitraums (einschließlich).</param>
        /// <param name="end">Das Ende des Zeitraums (ausschließlich).</param>
        /// <returns>Gesetzt, wenn ein Eintrag existiert.</returns>
        public bool HasEntry( DateTime start, DateTime end )
        {
            // Create search key
            var key = new ProgramGuideEntry { StartTime = start };

            // Find the index - in case of an exact match we assume that we can use the entry
            int ix = m_Events.BinarySearch( key );
            if (ix >= 0)
                return true;

            // Get the entry with the lower start time
            if ((ix = ~ix) <= 0)
                return false;

            // If our end time is beyond the start we can use it
            return (m_Events[ix - 1].EndTime > start);
        }

        /// <summary>
        /// Ermittelt einen bestimmten Eintrag.
        /// </summary>
        /// <param name="start">Der exakte Startzeitpunkt.</param>
        /// <returns>Der gewünschte Eintrag.</returns>
        public ProgramGuideEntry FindEntry( DateTime start )
        {
            // Find the index
            var ix = m_Events.BinarySearch( new ProgramGuideEntry { StartTime = start } );
            if (ix >= 0)
                return m_Events[ix];
            else
                return null;
        }

        /// <summary>
        /// Ermittelt den am besten passenden Eintrag aus einem Zeitraum.
        /// </summary>
        /// <typeparam name="TTarget">Die Art der Rückgabewerte.</typeparam>
        /// <param name="start">Der Beginn des Zeitraums.</param>
        /// <param name="end">Das Ende des Zeitraums.</param>
        /// <param name="factory">Methode zum Erstellen eines Rückgabewertes.</param>
        /// <returns>Der am besten passende Eintrag.</returns>
        public TTarget FindBestEntry<TTarget>( DateTime start, DateTime end, Func<ProgramGuideEntry, TTarget> factory )
        {
            // Create search key
            var key = new ProgramGuideEntry { StartTime = start };

            // Find the index
            int ix = m_Events.BinarySearch( key );
            if (ix < 0)
                if ((ix = ~ix) > 0)
                    ix--;

            // Best
            var bestEntry = default( ProgramGuideEntry );
            var bestOverlapTime = TimeSpan.Zero;
            var bestFull = false;

            // Find the best overlap
            while (ix < m_Events.Count)
            {
                // Attach to the entry
                var entry = m_Events[ix++];

                // Did it
                var entryStart = entry.StartTime;
                if (entryStart >= end)
                    break;

                // Get the real start
                var overlapStart = (entryStart >= start);
                if (!overlapStart)
                    entryStart = start;

                // Get the real end
                var entryEnd = entry.EndTime;
                var overlapEnd = (entryEnd <= end);
                if (!overlapEnd)
                    entryEnd = end;

                // Get the delta
                var overlap = entryEnd - entryStart;
                if (overlap.Ticks < 0)
                    break;

                // Skip partials if we already have a full match
                var fullOverlap = overlapStart && overlapEnd;
                if (!fullOverlap)
                    if (bestFull)
                        continue;

                // There are better - test only if we are not going from partial to full
                if (fullOverlap == bestFull)
                    if (overlap <= bestOverlapTime)
                        continue;

                // Remember
                bestOverlapTime = overlap;
                bestFull = fullOverlap;
                bestEntry = entry;
            }

            // Create response
            if (bestEntry == null)
                return default( TTarget );
            else
                return factory( bestEntry );
        }

        /// <summary>
        /// Ergänzt einen einzelnen Eintrag und ersetzt dabei eventuell bereits vorhandene
        /// Einträge.
        /// </summary>
        /// <param name="entry">Der zu ergänzende Eintrag.</param>
        public void Add( ProgramGuideEntry entry )
        {
            // Find the index
            int ix = m_Events.BinarySearch( entry );
            if (ix >= 0)
            {
                // Silent replace
                m_Events[ix] = entry;
            }
            else
            {
                // Get the index
                ix = ~ix;

                // Insert
                m_Events.Insert( ix, entry );

                // May need to cleanup
                for (int iy = ix; --iy >= 0; ix--)
                {
                    // Get the entry
                    var cur = m_Events[iy];

                    // Get next start time
                    var nextStart = cur.EndTime;
                    if (nextStart <= entry.StartTime)
                        break;

                    // Remove it
                    m_Events.RemoveAt( iy );
                }
            }

            // Calculate the next start time
            var tNext = entry.EndTime;

            // May need to cleanup
            for (++ix; ix < m_Events.Count;)
            {
                // Load the entry
                var cur = m_Events[ix];
                if (cur.StartTime >= tNext)
                    break;

                // Remove it
                m_Events.RemoveAt( ix );
            }
        }

        /// <summary>
        /// Remove any event collected older than two days.
        /// </summary>
        public void DiscardOld()
        {
            // Check time
            var threshold = DateTime.UtcNow.Date.AddDays( -1 );

            // Find the first event not older than one days
            int firstGoodIndex = m_Events.FindIndex( e => e.StartTime >= threshold );
            if (firstGoodIndex < 0)
                m_Events.Clear();
            else
                m_Events.RemoveRange( 0, firstGoodIndex );
        }

        /// <summary>
        /// Meldet die Anzahl der verwalteten Einträge.
        /// </summary>
        public int Count => m_Events.Count;

        /// <summary>
        /// Ermittelt einen bestimmten Eintrag.
        /// </summary>
        /// <param name="index">Die 0-basierte laufende Nummer des Eintrags.</param>
        /// <returns>Der gewünschte Eintrag.</returns>
        public ProgramGuideEntry this[int index] => m_Events[index];

        #region IEnumerable<ProgramGuideEntry> Members

        /// <summary>
        /// Erzeugt eine Auflistung über alle enthaltenen Einträge.
        /// </summary>
        /// <returns>Die gewünschte Auflistung.</returns>
        public IEnumerator<ProgramGuideEntry> GetEnumerator() => m_Events.GetEnumerator();

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Erzeugt eine Auflistung über alle enthaltenen Einträge.
        /// </summary>
        /// <returns>Die gewünschte Auflistung.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Liste.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        public OrderedEntries Clone()
        {
            // Create
            var clone = new OrderedEntries();

            // Finish
            clone.m_Events.AddRange( m_Events );

            // Report
            return clone;
        }

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Liste.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        object ICloneable.Clone() => Clone();

        #endregion
    }
}
