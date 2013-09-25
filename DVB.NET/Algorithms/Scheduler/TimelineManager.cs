using System;
using System.Collections;
using System.Collections.Generic;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Verwaltet eine Zeitlinie.
    /// </summary>
    /// <typeparam name="TItemType">Ein einzelner Eintrag.</typeparam>
    public abstract class TimelineManager<TItemType> : IEnumerable<TimelineManager<TItemType>.Range> where TItemType : struct
    {
        /// <summary>
        /// Ein Zeitbereich.
        /// </summary>
        public struct Range
        {
            /// <summary>
            /// Der Beginn des Bereichs.
            /// </summary>
            private DateTime m_start;

            /// <summary>
            /// Der Beginn des Bereichs.
            /// </summary>
            public DateTime Start { get { return m_start; } internal set { m_start = value; } }

            /// <summary>
            /// Das Ende des Bereichs.
            /// </summary>
            private DateTime m_end;

            /// <summary>
            /// Das Ende des Bereichs.
            /// </summary>
            public DateTime End { get { return m_end; } internal set { m_end = value; } }

            /// <summary>
            /// Die zugehörigen Daten.
            /// </summary>
            private TItemType m_data;

            /// <summary>
            /// Die zugehörigen Daten.
            /// </summary>
            public TItemType Data { get { return m_data; } internal set { m_data = value; } }

            /// <summary>
            /// Erstellt einen neuen Bereich.
            /// </summary>
            /// <param name="start">Der Anfang des Bereichs (einschließlich).</param>
            /// <param name="end">Das Ende des Bereichs (ausschließlich).</param>
            /// <param name="data">Die zugehörigen Daten.</param>
            private Range( DateTime start, DateTime end, TItemType data )
            {
                // Remember all
                m_start = start;
                m_data = data;
                m_end = end;
            }

            /// <summary>
            /// Erstellt einen neuen Bereich.
            /// </summary>
            /// <param name="start">Der Anfang des Bereichs (einschließlich).</param>
            /// <param name="end">Das Ende des Bereichs (ausschließlich).</param>
            /// <param name="data">Die zugehörigen Daten.</param>
            /// <returns>Der gewünschte Bereich.</returns>
            public static Range Create( DateTime start, DateTime end, TItemType data )
            {
                // Must not be empty
                if (end <= start)
                    throw new ArgumentOutOfRangeException( "end" );

                // Create
                return new Range( start, end, data );
            }

            /// <summary>
            /// Erstellt einen neuen Bereich.
            /// </summary>
            /// <param name="start">Der Anfang des Bereichs (einschließlich).</param>
            /// <param name="duration">Die Größe des Bereichs.</param>
            /// <param name="data">Die zugehörigen Daten.</param>
            /// <returns>Der gewünschte Bereich.</returns>
            public static Range Create( DateTime start, TimeSpan duration, TItemType data )
            {
                // Forward
                return Create( start, checked( start + duration ), data );
            }

            /// <summary>
            /// Erzeugt einen Anzeigetext zu Testzwecken.
            /// </summary>
            /// <returns>Der gewünschte Anzeigetext.</returns>
            public override string ToString()
            {
                // Take it easy
                return string.Format( "[{0}..{1}={3}] '{2}'", Start, End, Data, End - Start );
            }
        }

        /// <summary>
        /// Alle bereits vermerkten Bereiche.
        /// </summary>
        private readonly List<Range> m_ranges = new List<Range>();

        /// <summary>
        /// Ergänzt einen neuen Bereich.
        /// </summary>
        /// <param name="range">Der gewünschte Bereich.</param>
        public void Add( Range range )
        {
            // Validate
            if (range.End <= range.Start)
                throw new ArgumentOutOfRangeException( "range" );

            // Find a place to add it
            for (var index = 0; index < m_ranges.Count; index++)
            {
                // Attach to the range in the list
                var existingRange = m_ranges[index];

                // Left to existing with no overlap: just insert and leave
                if (range.End <= existingRange.Start)
                {
                    // Insert
                    m_ranges.Insert( index, range );

                    // Done
                    return;
                }

                // Right to existing with no overlap: check next
                if (range.Start >= existingRange.End)
                    continue;

                // Overlapping but starting early: create a range for the new item data
                if (range.Start < existingRange.Start)
                {
                    // Just add a new bit
                    m_ranges.Insert( index++, Range.Create( range.Start, existingRange.Start, range.Data ) );

                    // Correct us
                    range.Start = existingRange.Start;
                }

                // Overlapping but starting late: separate a range with the existing item data
                else if (range.Start > existingRange.Start)
                {
                    // Actually this is a split
                    m_ranges.Insert( index++, Range.Create( existingRange.Start, range.Start, existingRange.Data ) );

                    // Update the previous data
                    existingRange.Start = range.Start;

                    // Write it back
                    m_ranges[index] = existingRange;
                }

                // Overlapping and ending in time or late: merge the item data and eventually check for next
                if (range.End >= existingRange.End)
                {
                    // Update
                    existingRange.Data = Merge( existingRange.Data, range.Data );

                    // Write it back
                    m_ranges[index] = existingRange;

                    // Done
                    if (range.End == existingRange.End)
                        return;

                    // Correct for further processing
                    range.Start = existingRange.End;

                    // Next iteration
                    continue;
                }

                // Overlapping and ending early: merge item data and append a range for the left existing one
                m_ranges.Insert( index + 1, Range.Create( range.End, existingRange.End, existingRange.Data ) );

                // Update exisiting 
                existingRange.Data = Merge( existingRange.Data, range.Data );
                existingRange.End = range.End;

                // Write it back
                m_ranges[index] = existingRange;

                // Did it
                return;
            }

            // Start time beyond the last range: just append
            m_ranges.Add( range );
        }

        /// <summary>
        /// Führt die Daten zweier Bereiche zusammen.
        /// </summary>
        /// <param name="existing">Die Daten des existierenden Bereiches.</param>
        /// <param name="added">Die Daten des neuen Bereichs.</param>
        /// <returns>Die Zusammenfassung der Daten.</returns>
        protected abstract TItemType Merge( TItemType existing, TItemType added );

        /// <summary>
        /// Meldet eine Auflistung über alle Bereich.
        /// </summary>
        /// <returns>Die gewünschte Auflistung.</returns>
        public IEnumerator<TimelineManager<TItemType>.Range> GetEnumerator()
        {
            // Forward
            return m_ranges.GetEnumerator();
        }

        /// <summary>
        /// Meldet eine Auflistung über alle Bereich.
        /// </summary>
        /// <returns>Die gewünschte Auflistung.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            // Forward
            return GetEnumerator();
        }
    }
}
