using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Zählt Quellen für die Entschlüsselung.
    /// </summary>
    internal class AllocationMap : IEnumerable<AllocationMap.IAllocationInformation>
    {
        /// <summary>
        /// Beschreibt das Ergebnis einer Prüfung.
        /// </summary>
        public class AllocationPlan
        {
            /// <summary>
            /// Der gewünschte Zeitbereich.
            /// </summary>
            private readonly SuggestedPlannedTime m_plan;

            /// <summary>
            /// Die zugehörige Quelle.
            /// </summary>
            private readonly IScheduleSource m_source;

            /// <summary>
            /// Die zugehörige Verwaltung einer Ressource.
            /// </summary>
            private readonly AllocationMap m_map;

            /// <summary>
            /// Der erste Eintrag, der von der Belegung verwendet werden soll.
            /// </summary>
            private readonly int m_allocationIndex;

            /// <summary>
            /// Erzeugt eine neue Beschreibung.
            /// </summary>
            /// <param name="map">Die zugehörige Verwaltung einer Ressource.</param>
            /// <param name="source">Die Quelle, die verwendet werden soll.</param>
            /// <param name="plan">Die ursprüngliche Planung.</param>
            /// <param name="index">Der erste Eintrag in der Verwaltung, der belegt werden soll.</param>
            public AllocationPlan( AllocationMap map, IScheduleSource source, SuggestedPlannedTime plan, int index )
            {
                // Remember
                m_allocationIndex = index;
                m_source = source;
                m_plan = plan;
                m_map = map;
            }

            /// <summary>
            /// Führt die eigentliche Belegung aus.
            /// </summary>
            public void Allocate()
            {
                // Blind forward
                m_map.Allocate( m_allocationIndex, m_source, ref m_plan.Planned );
            }
        }

        /// <summary>
        /// Beschreibt einen einzelnen Eintrag.
        /// </summary>
        public interface IAllocationInformation
        {
            /// <summary>
            /// Der früheste Zeitpunkt, an dem diese Zuordnung relevant ist.
            /// </summary>
            DateTime Start { get; }

            /// <summary>
            /// Der späteste Zeitpunkt, an dem diese Zuordnung relevant ist.
            /// </summary>
            DateTime End { get; }

            /// <summary>
            /// Gesetzt, wenn keine Zuordnung erfolgt ist.
            /// </summary>
            bool IsIdle { get; }
        }

        /// <summary>
        /// Beschreibt eine einzelne Zuordnung.
        /// </summary>
        private class AllocatedRange : IAllocationInformation
        {
            /// <summary>
            /// Der früheste Zeitpunkt, an dem diese Zuordnung relevant ist.
            /// </summary>
            public DateTime Start { get; set; }

            /// <summary>
            /// Der späteste Zeitpunkt, an dem diese Zuordnung relevant ist.
            /// </summary>
            public DateTime End { get; set; }

            /// <summary>
            /// Gesetzt, wenn keine Zuordnung erfolgt ist.
            /// </summary>
            public bool IsIdle { get { return Sources.Count < 1; } }

            /// <summary>
            /// Die Anzahl der in Benutzung befindlichen Quellen.
            /// </summary>
            public List<IScheduleSource> Sources { get; private set; }

            /// <summary>
            /// Die zugehörige Gesamtverwaltung.
            /// </summary>
            private AllocationMap m_Map;

            /// <summary>
            /// Erzeugt eine neue Beschreibung.
            /// </summary>
            /// <param name="from">Der Beginn des Zeitraums.</param>
            /// <param name="to">Das Ende des Zeitraums.</param>
            /// <param name="map">Die zugehörige Gesamtverwaltung.</param>
            public AllocatedRange( DateTime from, DateTime to, AllocationMap map )
            {
                // Validate
                if (to <= from)
                    throw new ArgumentOutOfRangeException( "to" );

                // Finish
                Sources = new List<IScheduleSource>();

                // Remember
                Start = from;
                m_Map = map;
                End = to;
            }

            /// <summary>
            /// Erstellt eine Kopie für einen neuen Zeitbereich.
            /// </summary>
            /// <param name="from">Der Beginn des Zeitraums.</param>
            /// <param name="to">Das Ende des Zeitraums.</param>
            /// <returns>Die gewünschte Kopie.</returns>
            public AllocatedRange Clone( DateTime from, DateTime to )
            {
                // Forward
                return new AllocatedRange( from, to, m_Map ) { Sources = new List<IScheduleSource>( Sources ) };
            }

            /// <summary>
            /// Reserviert eine Quelle.
            /// </summary>
            /// <param name="source">Die Quelle, die nun verwendet werden soll.</param>
            /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
            public void Allocate( IScheduleSource source )
            {
                // Validate
                if (source == null)
                    throw new ArgumentNullException( "source" );

                // Remember
                if (!Sources.Any( s => s.IsSameAs( source ) ))
                    Sources.Add( source );
            }

            /// <summary>
            /// Prüft, ob eine Quelle reserviert werden kann.
            /// </summary>
            /// <param name="source">Die zu prüfende Quelle.</param>
            /// <returns>Gesetzt, wenn eine Reservierung möglich ist.</returns>
            public bool CanAllocate( IScheduleSource source )
            {
                // Test
                if (Sources.Any( s => s.IsSameAs( source ) ))
                    return true;
                else if (!m_Map.CanMerge( source, Sources ))
                    return false;
                else
                    return (Sources.Count < m_Map.TotalNumberOfSources);
            }

            /// <summary>
            /// Prüft, ob dieser Zeitbereich mit einer geplanten Aufzeichnung überlappt.
            /// </summary>
            /// <param name="time">Der Zeitraum der Aufzeichnung.</param>
            /// <returns>Gesetzt, wenn eine Überlappung vorliegt.</returns>
            public bool Overlaps( PlannedTime time )
            {
                // Just rest
                return (time.Start < End) && (time.End > Start);
            }

            /// <summary>
            /// Erstellt einen Anzeigetext zu Testzwecken.
            /// </summary>
            /// <returns>Der gewünschte Anzeigetext.</returns>
            public override string ToString()
            {
                // Create
                return string.Format( Properties.SchedulerResources.Debug_DecryptionCounter, m_Map.TotalNumberOfSources - Sources.Count, Start, End );
            }
        }

        /// <summary>
        /// Beschreibt die Verwendung einer einzelnen Quelle.
        /// </summary>
        public class AllocationTimeline : TimelineManager<int>
        {
            /// <summary>
            /// Kombiniert die Nutzung zweier Quellen.
            /// </summary>
            /// <param name="existing">Der bisherige Nutzugnszähler.</param>
            /// <param name="added">Die zusätzliche Nutzung.</param>
            /// <returns>Die kombinierte Nutzung.</returns>
            protected override int Merge( int existing, int added )
            {
                // Just add up
                return existing + added;
            }
        }

        /// <summary>
        /// Alle Reservierungen für diese Entschlüsselung.
        /// </summary>
        private readonly List<AllocatedRange> m_Allocations;

        /// <summary>
        /// Die maximale Anzahl von Reservierungen.
        /// </summary>
        private int TotalNumberOfSources { get; set; }

        /// <summary>
        /// Prüft, ob eine Quelle zu einer Liste von Quellen passt.
        /// </summary>
        private readonly Func<IScheduleSource, IEnumerable<IScheduleSource>, bool> m_MergeTest;

        /// <summary>
        /// Alle Quellen, die auf dem zugehlrigen Gerät verwendet werden.
        /// </summary>
        private readonly List<IScheduleSource> m_Sources;

        /// <summary>
        /// Erzeugt einen neuen Zähler.
        /// </summary>
        /// <param name="initialValue">Der Anfangswert.</param>
        /// <param name="mergeTest">Optional eine Methode zur Beschränkung der Zuordnung.</param>
        public AllocationMap( int initialValue, Func<IScheduleSource, IEnumerable<IScheduleSource>, bool> mergeTest = null )
        {
            // Remember
            m_Allocations = new List<AllocatedRange> { new AllocatedRange( DateTime.MinValue, DateTime.MaxValue, this ) };
            m_Sources = new List<IScheduleSource>();
            TotalNumberOfSources = initialValue;
            m_MergeTest = mergeTest;
        }

        /// <summary>
        /// Erzeugt eine Arbeitskopie.
        /// </summary>
        /// <param name="original">Die ursprüngliche Zuordnung.</param>
        private AllocationMap( AllocationMap original )
        {
            // Just copy references - will be cloned on demand
            TotalNumberOfSources = original.TotalNumberOfSources;
            m_Allocations = original.m_Allocations.ToList();
            m_Sources = original.m_Sources.ToList();
            m_MergeTest = original.m_MergeTest;
        }

        /// <summary>
        /// Meldet, ob eine Quelle zu einer Liste von Quellen passt.
        /// </summary>
        /// <param name="source">Die Quelle.</param>
        /// <param name="sources">Die Liste der Quellen.</param>
        /// <returns>Gesetzt, wenn die Quelle passt.</returns>
        private bool CanMerge( IScheduleSource source, IEnumerable<IScheduleSource> sources )
        {
            // Check it
            var mergeTest = m_MergeTest;
            if (mergeTest == null)
                return true;
            else
                return mergeTest( source, sources );
        }

        /// <summary>
        /// Meldet, ob überhaupt eine Entschlüsselung möglich ist.
        /// </summary>
        public bool IsEnabled { get { return (TotalNumberOfSources > 0); } }

        /// <summary>
        /// Gesetzt, wenn keine Planung vorgenommen wurde.
        /// </summary>
        public bool IsEmpty { get { return m_Allocations.Count < 2; } }

        /// <summary>
        /// Legt eine Anfangszeit für die Planung fest.
        /// </summary>
        /// <param name="start">Die neue Anfangszeit.</param>
        /// <returns>Gesetzt, wenn bisher noch keine Verwendung stattgefunden hat.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><see cref="DateTime.MinValue"/> ist als aktueller Parameter
        /// nicht erlaubt.</exception>
        public bool SetStart( DateTime start )
        {
            // Reset not allowed
            if (start == DateTime.MinValue)
                throw new ArgumentOutOfRangeException( "start" );

            // Already in use
            if (m_Allocations.Count > 1)
                return false;

            // Load
            var allocation = m_Allocations[0];
            if (allocation.Start != DateTime.MinValue)
                return false;

            // Update
            allocation.Start = start;

            // Done
            return true;
        }

        /// <summary>
        /// Meldet die Anzahl unterschiedlicher Quellen, die das Gerät bedienen muss.
        /// </summary>
        public int NumberOfDifferentSources { get { return m_Sources.Count; } }

        /// <summary>
        /// Korrigiert einen Abruf einer Entschlüsselung.
        /// </summary>
        /// <param name="allocationIndex">Die laufende Nummer des zu belegenden Eintrags.</param>
        /// <param name="source">Die zu verwendende Quelle.</param>
        /// <param name="time">Der Zeitraum, in dem eine Entschlüsselung aktiv ist.</param>
        private void Allocate( int allocationIndex, IScheduleSource source, ref PlannedTime time )
        {
            // Reset all caches - just in case...
            m_cachedResourceStartTimes = null;
            m_sourceUsage = null;

            // We are now using the source
            if (!m_Sources.Any( s => s.IsSameAs( source ) ))
                m_Sources.Add( source );

            // Localize time - can not use ref parameters inside a delegate
            var timeCopy = time;

            // Find the one the prepare analysis gave us - no further tests will be made!
            var allocation = m_Allocations[allocationIndex];

            // See if we have to clip
            if (time.Start < allocation.Start)
            {
                // Do the clip
                time.Duration = time.End - allocation.Start;
                time.Start = allocation.Start;
            }

            // Correct it all
            for (; ; )
            {
                // On change we must create a clone
                var allocationIsPrivate = false;

                // Create a new starter
                if (time.Start > allocation.Start)
                {
                    // Create a new allocation entry
                    var split = allocation.Clone( allocation.Start, time.Start );

                    // Add it just in front of the current one
                    m_Allocations.Insert( allocationIndex++, split );

                    // Must create a brand new one
                    allocation = allocation.Clone( time.Start, allocation.End );

                    // Update in our private list
                    m_Allocations[allocationIndex] = allocation;

                    // We can now safely overwrite it
                    allocationIsPrivate = true;
                }

                // Create a new trailer
                if (time.End < allocation.End)
                {
                    // Create a new allocation entry
                    var split = allocation.Clone( time.End, allocation.End );

                    // Add it just behind the current one
                    m_Allocations.Insert( allocationIndex + 1, split );

                    // Update the allocation
                    if (allocationIsPrivate)
                    {
                        // We are allowed to change it
                        allocation.End = time.End;
                    }
                    else
                    {
                        // Must create a brand new one
                        allocation = allocation.Clone( allocation.Start, time.End );

                        // Update in our private list
                        m_Allocations[allocationIndex] = allocation;

                        // We are now allowed to write to it
                        allocationIsPrivate = true;
                    }
                }

                // Update the allocation
                if (allocationIsPrivate)
                {
                    // Just count down
                    allocation.Allocate( source );
                }
                else
                {
                    // Must create a brand new one
                    allocation = allocation.Clone( allocation.Start, allocation.End );

                    // Correct
                    allocation.Allocate( source );

                    // Update in our private list
                    m_Allocations[allocationIndex] = allocation;
                }

                // See if we are done
                if (time.End <= allocation.End)
                    break;

                // Load the next allocation area
                allocation = m_Allocations[++allocationIndex];
            }
        }

        /// <summary>
        /// Fordert eine Entschlüsselung an.
        /// </summary>
        /// <param name="source">Die zu verwendende Quelle.</param>
        /// <param name="timeHolder">Der Zeitraum, für den die Planung stattfinden soll.</param>
        /// <returns>Gesetzt, wenn eine Zuordnung überhaupt möglich ist. Gemeldet wird dann der Zeitversatz der Zuordnung.</returns>
        public AllocationPlan PrepareAllocation( IScheduleSource source, SuggestedPlannedTime timeHolder )
        {
            // Extract the time to use for plannung
            var time = timeHolder.Planned;

            // As long as needed
            for (var scanStart = 0; ; )
            {
                // See if there is at least one allocation area available
                var allocationIndex = m_Allocations.FindIndex( scanStart, a => a.Overlaps( time ) );
                if (allocationIndex < 0)
                    return null;

                // On the start skip all having no recording left
                while (!m_Allocations[allocationIndex].CanAllocate( source ))
                    if (++allocationIndex == m_Allocations.Count)
                        return null;
                    else if (!m_Allocations[allocationIndex].Overlaps( time ))
                        return null;

                // Initial index - where we start the allocation
                var startIndex = allocationIndex;

                // Now make sure that we can record to the end - allocation maps from the beginning to the end of all times
                while (++allocationIndex < m_Allocations.Count)
                    if (!m_Allocations[allocationIndex].Overlaps( time ))
                        break;
                    else if (!m_Allocations[allocationIndex].CanAllocate( source ))
                    {
                        // Prepare to rescan
                        scanStart = allocationIndex + 1;

                        // Do not end
                        startIndex = -1;

                        // Done with loop
                        break;
                    }

                // Response
                if (startIndex >= 0)
                    return new AllocationPlan( this, source, timeHolder, startIndex );
            }
        }

        /// <summary>
        /// Erzeugt eine Kopie.
        /// </summary>
        /// <param name="planTime">Der aktuelle Planungsbeginn, sofern bekannt.</param>
        /// <returns>Die gewünschte Kopie.</returns>
        public AllocationMap Clone( DateTime? planTime )
        {
            // Create
            var clone = new AllocationMap( this );

            // No check needed
            if (!planTime.HasValue)
                return clone;

            // See who is below the limit
            var allocations = clone.m_Allocations;
            var index = allocations.FindIndex( allocation => allocation.End > planTime.Value );

            // Cut off if not the very first
            if (--index > 0)
            {
                // Load the very start of it all
                var allocation = allocations[0];

                // Update it's end time
                allocations[0] = allocation.Clone( allocation.Start, allocations[index].End );

                // Cleanup the rest
                allocations.RemoveRange( 1, index );
            }

            // Report
            return clone;
        }

        /// <summary>
        /// Meldet einen Anzeigenamen zu Testzwecken.
        /// </summary>
        /// <returns>Die Anzahl der verbleibenden Quellen.</returns>
        public override string ToString()
        {
            // Report
            return string.Format( "({0})", string.Join( "*", m_Allocations.Select( a => a.ToString() ).ToArray() ) );
        }

        /// <summary>
        /// Meldet, wann die aktuelle Planung beginnt.
        /// </summary>
        public DateTime PlanStart { get { return m_Allocations[0].End; } }

        /// <summary>
        /// Meldet, wann die aktuelle Planung endet.
        /// </summary>
        public DateTime PlanEnd { get { return m_Allocations[m_Allocations.Count - 1].Start; } }

        /// <summary>
        /// Meldet alle aktiven Einträge.
        /// </summary>
        /// <returns>Die gewünschte Auflistung.</returns>
        public IEnumerator<AllocationMap.IAllocationInformation> GetEnumerator()
        {
            // Forward
            return m_Allocations.GetEnumerator();
        }

        /// <summary>
        /// Meldet alle aktiven Einträge.
        /// </summary>
        /// <returns>Die gewünschte Auflistung.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            // Forward
            return GetEnumerator();
        }

        /// <summary>
        /// Die Zeiten, zu denen das zugehörige Gerät aktiviert wird.
        /// </summary>
        private DateTime[] m_cachedResourceStartTimes;

        /// <summary>
        /// Ermittelt die Zeitpunkte, an denen das zugehörige Geräte tatsächlich aktiviert wird.
        /// </summary>
        public DateTime[] ResourceStartTimes
        {
            get
            {
                // Load once
                if (m_cachedResourceStartTimes == null)
                    m_cachedResourceStartTimes = GetResourceStartTimes().ToArray();

                // Report
                return m_cachedResourceStartTimes;
            }
        }

        /// <summary>
        /// Ermittelt die Zeitpunkte, an denen das zugehörige Geräte tatsächlich aktiviert wird.
        /// </summary>
        /// <returns>Die Liste der Zeitpunkte.</returns>
        private IEnumerable<DateTime> GetResourceStartTimes()
        {
            // All times where we really started the resource
            var wasIdle = true;

            // Fill it
            foreach (var allocation in m_Allocations)
            {
                // Always skip if we are a follow up
                if (wasIdle)
                    if (!allocation.IsIdle)
                        yield return allocation.Start;

                // Set mode
                wasIdle = allocation.IsIdle;
            }
        }

        /// <summary>
        /// Vermerkt das Aufzeichnungsmuster einzelner Quellen.
        /// </summary>
        private Dictionary<IScheduleSource, AllocationTimeline> m_sourceUsage;

        /// <summary>
        /// Ermittelt, wann einzelne Quellen aufgezeichnet werden.
        /// </summary>
        public Dictionary<IScheduleSource, AllocationTimeline> SourceUsage
        {
            get
            {
                // Create once
                if (m_sourceUsage == null)
                    m_sourceUsage = GetSourceUsage();

                // Report
                return m_sourceUsage;
            }
        }

        /// <summary>
        /// Ermittelt, wann einzelne Quellen aufgezeichnet werden.
        /// </summary>
        /// <returns>Für jede Quelle die Liste der Zeitpunkte, an denen sie verwendet wird.</returns>
        private Dictionary<IScheduleSource, AllocationTimeline> GetSourceUsage()
        {
            // Since we have no identity compare for sources we must do some (slow) remappings - will be cached!
            var sources = new List<IScheduleSource>();

            // Create map
            var map = new Dictionary<IScheduleSource, AllocationTimeline>( ReferenceComparer<IScheduleSource>.Default );

            // Process all allocations
            foreach (var allocation in m_Allocations)
                if (!allocation.IsIdle)
                {
                    // Create range for timeline entry
                    var range = AllocationTimeline.Range.Create( allocation.Start, allocation.End, 1 );

                    // Forward to all sources
                    foreach (var source in allocation.Sources)
                    {
                        // See if we know the source
                        var representative = sources.FirstOrDefault( s => s.IsSameAs( source ) );

                        // Must add a new one
                        if (ReferenceEquals( representative, null ))
                            sources.Add( representative = source );

                        // Now we can check the time line
                        AllocationTimeline timeline;
                        if (!map.TryGetValue( representative, out timeline ))
                            map.Add( representative, timeline = new AllocationTimeline() );

                        // Register
                        timeline.Add( range );
                    }
                }

            // Report
            return map;
        }
    }
}
