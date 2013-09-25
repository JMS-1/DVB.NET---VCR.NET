using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Verwaltet eine Gesamtplanung für eine Liste von Geräten.
    /// </summary>
    internal class SchedulePlan
    {
        /// <summary>
        /// Vergleicht zwei Pläne nach der Anzahl des Gesamtverlustes.
        /// </summary>
        public static readonly IComparer<SchedulePlan> CompareByTotalCut = new TotalCutComparer();

        /// <summary>
        /// Vergleicht zwei Pläne nach der Gesamtzeit, in der Quellen auf mehreren Geräten gleichzeitig aufgezeichnet werden.
        /// </summary>
        public static readonly IComparer<SchedulePlan> CompareByParallelSourceTime = new ParallelSourceTimeComparer();

        /// <summary>
        /// Vergleicht zwei Pläne nach der Anzahl der verwendeten Geräte.
        /// </summary>
        public static readonly IComparer<SchedulePlan> CompareByResourceCount = new ResourceCountComparer();

        /// <summary>
        /// Vergleicht zwei Pläne nach Überlappungen von Startzeiten.
        /// </summary>
        /// <param name="rule">Die zu berücksichtigenden Regeln.</param>
        /// <param name="nameComparer">Der Algorithmus zum Vergleich von Gerätenamen.</param>
        /// <returns>Der gewünschte Algorithmus.</returns>
        public static IComparer<SchedulePlan> CompareByOverlappingStart( string rule, IEqualityComparer<string> nameComparer )
        {
            // Forward
            return new ByResourceStartComparer( rule, nameComparer );
        }

        /// <summary>
        /// Vergleicht zwei Pläne nach dem Gesamtverlust.
        /// </summary>
        private class TotalCutComparer : IComparer<SchedulePlan>
        {
            /// <summary>
            /// Vergleicht zwei Pläne nach dem Gesamtverlust.
            /// </summary>
            /// <param name="firstPlan">Der erste Plan.</param>
            /// <param name="secondPlan">Der zweite Plan.</param>
            /// <returns>Positiv, wenn der erste Plan mehr Aufzeichnungszeit abschneidet als der zweite.</returns>
            public int Compare( SchedulePlan firstPlan, SchedulePlan secondPlan )
            {
                // Forward
                return firstPlan.Resources.Sum( r => r.TotalCut.Ticks ).CompareTo( secondPlan.Resources.Sum( r => r.TotalCut.Ticks ) );
            }
        }

        /// <summary>
        /// Vergleicht zwei Pläne nach dem Zeitraum, in dem Quellen auf mehreren Geräten aufgezeichnet werden.
        /// </summary>
        private class ParallelSourceTimeComparer : IComparer<SchedulePlan>
        {
            /// <summary>
            /// Vergleicht zwei Pläne nach dem Zeitraum, in dem Quellen auf mehreren Geräten aufgezeichnet werden.
            /// </summary>
            /// <param name="firstPlan">Der erste Plan.</param>
            /// <param name="secondPlan">Der zweite Plan.</param>
            /// <returns>Die logische Differenz der parallelen Aufzeichnungszeit.</returns>
            public int Compare( SchedulePlan firstPlan, SchedulePlan secondPlan )
            {
                // Forward
                return 0;
            }
        }

        /// <summary>
        /// Vergleicht zwei Pläne auf Basis einzelner Geräte.
        /// </summary>
        private class ByResourceStartComparer : IComparer<SchedulePlan>
        {
            /// <summary>
            /// Beschreibt eine einzelne Regel.
            /// </summary>
            private class Rule
            {
                /// <summary>
                /// Das Gerät, das nicht nach anderen Geräten gestartet werden soll.
                /// </summary>
                private readonly string m_leadingResource;

                /// <summary>
                /// Alle Geräte, die nicht vor vor dem führenden Gerät gestartet werden sollen.
                /// </summary>
                private readonly HashSet<string> m_testResources;

                /// <summary>
                /// Der Algorithmus zum Vergleich von Namen.
                /// </summary>
                private readonly IEqualityComparer<string> m_nameComparer;

                /// <summary>
                /// Erstellt einen neuen Vergleich.
                /// </summary>
                /// <param name="rule">Die zu verwendenden Regeln.</param>
                /// <param name="nameComparer">Der Algorithmus zum Vergleich von Namen.</param>
                public Rule( string rule, IEqualityComparer<string> nameComparer )
                {
                    // Analyse
                    var parts = rule.Split( '<' );

                    // Remember
                    m_testResources = new HashSet<string>( parts.Skip( 1 ), nameComparer );
                    m_nameComparer = nameComparer;
                    m_leadingResource = parts[0];

                    // Reset
                    if (m_testResources.Contains( "*" ))
                        m_testResources = null;
                }

                /// <summary>
                /// Meldet, ob eine Planung unsere Regel verletzt.
                /// </summary>
                /// <param name="plan">Der zu prüfende Plan.</param>
                /// <returns>Zählt, wie oft ein Starte des problematischen Gerätes erfolgt.</returns>
                public int Count( SchedulePlan plan )
                {
                    // Find the primary resource
                    var primary = plan.Resources.FirstOrDefault( r => m_nameComparer.Equals( r.Resource.Name, m_leadingResource ) );
                    if (primary == null)
                        return 0;

                    // All times where we really started the resource
                    var startTimes = primary.Allocations.GetResourceStartTimes().ToArray();
                    if (startTimes.Length < 1)
                        return 0;

                    // Counter
                    var badCount = 0;

                    // Now inspect all resources of interest
                    foreach (var resource in plan.Resources)
                    {
                        // Our primary
                        var name = resource.Resource.Name;
                        if (m_nameComparer.Equals( name, m_leadingResource ))
                            continue;

                        // Not of interest
                        if (m_testResources != null)
                            if (!m_testResources.Contains( name ))
                                continue;

                        // Is idle
                        var allocations = resource.Allocations;
                        if (allocations.IsEmpty)
                            continue;

                        // Current index of inspection
                        var startTime = startTimes[0];
                        var startIndex = 0;

                        // Process all allocations
                        foreach (var allocation in allocations)
                            if (!allocation.IsIdle)
                                for (; ; )
                                {
                                    // Too early
                                    if (allocation.End <= startTime)
                                        break;

                                    // Rule violated
                                    if (allocation.Start <= startTime)
                                        badCount++;

                                    // Adjust to next
                                    if (++startIndex >= startTimes.Length)
                                        break;

                                    // Reload for next try
                                    startTime = startTimes[startIndex];
                                }
                    }

                    // Report the total violations - overall: the less the better
                    return badCount;
                }
            }

            /// <summary>
            /// Alle zu betrachtenden Regeln.
            /// </summary>
            private readonly Rule[] m_rules;

            /// <summary>
            /// Erstellt einen neuen Vergleich.
            /// </summary>
            /// <param name="rule">Die zu verwendenden Regeln.</param>
            /// <param name="nameComparer">Der Algorithmus zum Vergleich von Namen.</param>
            public ByResourceStartComparer( string rule, IEqualityComparer<string> nameComparer )
            {
                // Setup                
                m_rules =
                    rule
                        .Split( '|' )
                        .Select( part => new Rule( part, nameComparer ) )
                        .ToArray();
            }

            /// <summary>
            /// Meldet, ob eine Planung unsere Regel verletzt.
            /// </summary>
            /// <param name="plan">Der zu prüfende Plan.</param>
            /// <returns>Zählt, wie oft ein Starte des problematischen Gerätes erfolgt.</returns>
            private int CountRuleViolations( SchedulePlan plan )
            {
                // Process
                return m_rules.Sum( rule => rule.Count( plan ) );
            }

            /// <summary>
            /// Vergleicht zwei Pläne über die Startzeiten der enthaltenen Geräte.
            /// </summary>
            /// <param name="firstPlan">Der erste Plan.</param>
            /// <param name="secondPlan">Der zweite Plan.</param>
            /// <returns>Gesetzt, wenn der erste Plan nach allen Regeln eine bessere Lösung bietet.</returns>
            public int Compare( SchedulePlan firstPlan, SchedulePlan secondPlan )
            {
                // Compare violations
                return -CountRuleViolations( firstPlan ).CompareTo( CountRuleViolations( secondPlan ) );
            }
        }

        /// <summary>
        /// Vergleicht zwei Pläne nach der Anzahl der verwendeten Geräte.
        /// </summary>
        private class ResourceCountComparer : IComparer<SchedulePlan>
        {
            /// <summary>
            /// Vergleicht zwei Pläne nach der Anzahl der verwendeten Geräte.
            /// </summary>
            /// <param name="firstPlan">Der erste Plan.</param>
            /// <param name="secondPlan">Der zweite Plan.</param>
            /// <returns>Positiv, wenn der erste Plan mehr Geräte verwendet als der zweite.</returns>
            public int Compare( SchedulePlan firstPlan, SchedulePlan secondPlan )
            {
                // Forward
                return firstPlan.ResourcesInUseCount.CompareTo( secondPlan.ResourcesInUseCount );
            }
        }

        /// <summary>
        /// Alle Zähler für die verfügbaren Entschlüsselungen.
        /// </summary>
        public Dictionary<Guid, AllocationMap> DecryptionCounters = new Dictionary<Guid, AllocationMap>();

        /// <summary>
        /// Alle Geräte in dieser Planung. Diese sind aufsteigend nach der Prioriät geordnet, das unwichtigste
        /// Gerät kommt als erstes.
        /// </summary>
        public ResourcePlan[] Resources { get; private set; }

        /// <summary>
        /// Die zugehörige Planungsinstanz.
        /// </summary>
        public ResourceCollection ResourceCollection { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Gesamtplanung.
        /// </summary>
        /// <param name="resources">Die zugehörige Planungsinstanz.</param>
        /// <exception cref="ArgumentNullException">Es wurden keine Geräte angegeben.</exception>
        public SchedulePlan( ResourceCollection resources )
            : this( resources, default( Dictionary<Guid, AllocationMap> ) )
        {
            // Load
            Resources = resources.Select( r => new ResourcePlan( r, this ) ).ToArray();

            // Helper
            var inherited = new HashSet<Guid>();

            // Register all groups
            foreach (var group in resources.DecryptionGroups)
                RegisterDecryption( group, inherited );
        }

        /// <summary>
        /// Erzeugt eine neue Gesamtplanung.
        /// </summary>
        /// <param name="resources">Die zugehörige Planungsinstanz.</param>
        /// <param name="counters">Alle Informationen zu Entschlüsselungen.</param>
        private SchedulePlan( ResourceCollection resources, Dictionary<Guid, AllocationMap> counters )
        {
            // Validate
            if (resources == null)
                throw new ArgumentNullException( "scheduler" );

            // Attach
            ResourceCollection = resources;

            // Clone decryption information
            if (counters != null)
                foreach (var pair in counters)
                    DecryptionCounters.Add( pair.Key, pair.Value.Clone() );
        }

        /// <summary>
        /// Erstellt eine exakte unabhängige Kopie die Planung.
        /// </summary>
        /// <param name="original">Die ursprüngliche Planung.</param>
        private SchedulePlan( SchedulePlan original )
            : this( original.ResourceCollection, original.DecryptionCounters )
        {
            // Create array
            Resources = new ResourcePlan[original.Resources.Length];

            // Deep clone
            for (int i = Resources.Length; i-- > 0; )
                Resources[i] = original.Resources[i].Clone( this );
        }

        /// <summary>
        /// Meldet einen neuen Zähler für entschlüsselte Quellen an.
        /// </summary>
        /// <param name="limit">Die maximale Anzahl gleichzeitig entschlüsselbarer Quellen.</param>
        /// <returns>Die eindeutige Kennung des neuen Zählers.</returns>
        public Guid RegisterDecryption( int limit )
        {
            // Get new identifier
            var id = Guid.NewGuid();

            // Remember
            DecryptionCounters.Add( id, new AllocationMap( limit ) );

            // Report
            return id;
        }

        /// <summary>
        /// Registriert eine neue Entschlüsselungsregel.
        /// </summary>
        /// <param name="group">Die neue Regel.</param>
        /// <param name="inherited">Alle Zähler höherer Ebene.</param>
        private void RegisterDecryption( DecryptionGroup group, HashSet<Guid> inherited )
        {
            // Create the new counter
            var counter = RegisterDecryption( group.MaximumParallelSources );

            // Remember
            inherited.Add( counter );

            // Process all resources
            if (group.ScheduleResources != null)
                foreach (var resource in group.ScheduleResources.Select( r => Resources.FirstOrDefault( p => ReferenceEquals( p.Resource, r ) ) ))
                    if (resource != null)
                        resource.DecryptionCounters.UnionWith( inherited );

            // Forward
            if (group.DecryptionGroups != null)
                foreach (var subGroup in group.DecryptionGroups)
                    RegisterDecryption( subGroup, inherited );

            // Reset
            inherited.Remove( counter );
        }

        /// <summary>
        /// Erzeugt einen neuen Plan basierend auf der aktuellen Zuordnung.
        /// </summary>
        /// <returns>Ein neuer Plan.</returns>
        public SchedulePlan Restart()
        {
            // Create - make sure that we keep the decryption allocations
            var clone = new SchedulePlan( ResourceCollection, DecryptionCounters );

            // Fill
            clone.Resources = Resources.Select( r => r.Restart( clone ) ).ToArray();

            // Report
            return clone;
        }

        /// <summary>
        /// Ermittelt den besten Ausführungsplan.
        /// </summary>
        /// <param name="plans">Eine Liste von Plänen.</param>
        /// <param name="comparer">Der Algorithmus, der entscheidet, wann ein Plan besser als ein anderer ist.</param>
        /// <returns>Der beste Plan.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Liste angegeben.</exception>
        public static SchedulePlan FindBest( IEnumerable<SchedulePlan> plans, IComparer<SchedulePlan> comparer )
        {
            // Validate
            if (plans == null)
                throw new ArgumentNullException( "plans" );

#if !SILVERLIGHT
            // Report
            if (RecordingScheduler.SchedulerTrace.TraceInfo)
                Trace.TraceInformation( Properties.SchedulerResources.Trace_ChooseBestPlan, plans.Count() );
#endif

            // Reset
            SchedulePlan best = null;

            // Process all
            foreach (var plan in plans)
                if (plan.CompareTo( best, comparer ) > 0)
                    best = plan;

            // Report
            return best;
        }

        /// <summary>
        /// Meldet alle Aufzeichnungen in diesem Plan.
        /// </summary>
        /// <returns>Alle geplanten Aufzeichnungen.</returns>
        public IEnumerable<ScheduleInfo> GetRecordings()
        {
            // Report all
            return
                Resources
                    .SelectMany( r => r.GetRecordings() )
                    .OrderBy( r => r.Time.Start )
                    .ThenByDescending( r => r.Resource.AbsolutePriority );
        }

        /// <summary>
        /// Erstellt eine exakte Kopie dieses Plans.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        public SchedulePlan Clone()
        {
            // Forward
            return new SchedulePlan( this );
        }

        /// <summary>
        /// Meldet die Anzahl der Geräte, die in Benutzung sind.
        /// </summary>
        public int ResourcesInUseCount
        {
            get
            {
                // Report
                return Resources.Count( r => r.RecordingCount > 0 );
            }
        }

        /// <summary>
        /// Vergleicht zwei Pläne. Ein Plan ist umso besser, je mehr Aufzeichnung auf einem hoch priorisierten
        /// Gerät ausgeführt werden. Relevant ist die Reihenfolge der Geräte in der Liste.
        /// </summary>
        /// <param name="other">Ein anderer Plan.</param>
        /// <param name="comparer">Die Vergleichsregeln.</param>
        /// <returns>Der Unterschied zwischen den Plänen.</returns>
        public int CompareTo( SchedulePlan other, IComparer<SchedulePlan> comparer )
        {
            // Validate
            if (comparer == null)
                throw new ArgumentNullException( "comparer" );

            // Not possible
            if (other == null)
                return +1;

            // Internal cross check
            if (Resources.Length != other.Resources.Length)
                throw new InvalidOperationException( "Resources.Length" );

            // Process
            return comparer.Compare( this, other );
        }
    }
}
