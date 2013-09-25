using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Erlaubt das Erzeugen einer Geräteverwaltung.
    /// </summary>
    public static partial class ResourceManager
    {
        /// <summary>
        /// Verwaltet Geräte zur digitalen Aufzeichnung.
        /// </summary>
        private class Implementation : IResourceManager
        {
            /// <summary>
            /// Die Liste aller verwalteten Geräte.
            /// </summary>
            private ResourceCollection m_Resources = new ResourceCollection();

            /// <summary>
            /// Die aktuelle Liste der Aufzeichnungen.
            /// </summary>
            private List<ResourceAllocationInformation> m_Recordings = new List<ResourceAllocationInformation>();

            /// <summary>
            /// Die aktuelle Gesamtplanung.
            /// </summary>
            private SchedulePlan m_CurrentPlan;

            /// <summary>
            /// Der Vergleichsalgorithmus zur Bewertung von Aufzeichnungsplänen.
            /// </summary>
            private IComparer<SchedulePlan> m_PlanComparer;

            /// <summary>
            /// Erzeugt eine neue Verwaltung.
            /// </summary>
            /// <param name="rulePath">Der volle Pfad zur Regeldatei.</param>
            /// <param name="resourceNameComparer">Die Vergleichsmethode </param>
            public Implementation( string rulePath, IEqualityComparer<string> resourceNameComparer )
            {
                // Remember
                ResourceNameComparer = resourceNameComparer;

                // Load rules
                if (string.IsNullOrEmpty( rulePath ))
                    m_PlanComparer = CustomComparer.Default( ResourceNameComparer );
                else
                    m_PlanComparer = CustomComparer.Create( File.ReadAllBytes( rulePath ), resourceNameComparer );
            }

            /// <summary>
            /// Erzeugt eine Planungsinstanz basierend auf den aktuell bekannten Aufzeichnungen.
            /// </summary>
            /// <returns>Der gewünschte Plan.</returns>
            private SchedulePlan CreateSchedulePlan()
            {
                // Create
                var plan = new SchedulePlan( m_Resources );

                // Report
                var resources = plan.Resources.ToDictionary( r => r.Resource, ReferenceComparer<IScheduleResource>.Default );

                // Merge in each schedule
                foreach (var recording in m_Recordings)
                {
                    // Attach to the resource
                    var resource = recording.Resource;
                    var resourcePlan = resources[resource];

                    // Check mode
                    if (recording.Source == null)
                    {
                        // Try to reserve
                        if (!resourcePlan.Reserve( recording.Time.End ))
                            return null;

                        // Next 
                        continue;
                    }

                    // Attach to the timing
                    SuggestedPlannedTime planned = recording.Time;

                    // Try add
                    if (!resourcePlan.Add( recording, planned, DateTime.MinValue ))
                        return null;

                    // Must not start late
                    if (planned.Planned.Start != recording.Time.Start)
                        return null;
                }

                // Report
                return plan;
            }

            #region IDisposable Members

            /// <summary>
            /// Beendet die Nutzung dieser Instanz endgültig.
            /// </summary>
            public void Dispose()
            {
            }

            #endregion

            #region IResourceManager Members

            /// <summary>
            /// Der Algorithmus zum Vergleich der Namen von Geräten.
            /// </summary>
            public IEqualityComparer<string> ResourceNameComparer { get; private set; }

            /// <summary>
            /// Meldet die aktuelle Belegung der Geräte.
            /// </summary>
            public IResourceAllocationInformation[] CurrentAllocations { get { return m_Recordings.ToArray(); } }

            /// <summary>
            /// Meldet alle bekannten Geräte.
            /// </summary>
            public IScheduleResource[] Resources { get { return m_Resources.ToArray(); } }

            /// <summary>
            /// Meldet die Vergleichsmethode für Geräte.
            /// </summary>
            public IEqualityComparer<IScheduleResource> ResourceComparer { get { return ReferenceComparer<IScheduleResource>.Default; } }

            /// <summary>
            /// Ergänzt ein Gerät.
            /// </summary>
            /// <param name="resource">Das gewünschte Gerät.</param>
            public void Add( IScheduleResource resource )
            {
                // Not allowed
                if (m_CurrentPlan != null)
                    throw new NotSupportedException( "Add" );

                // Forward
                m_Resources.Add( resource );
            }

            /// <summary>
            /// Meldet komplete Entschlüsselungsregeln an.
            /// </summary>
            /// <param name="group">Eine neue Regel.</param>
            /// <exception cref="ArgumentNullException">Die Regel ist ungültig.</exception>
            public void Add( DecryptionGroup group )
            {
                // Not allowed
                if (m_CurrentPlan != null)
                    throw new NotSupportedException( "Add" );

                // Forward
                m_Resources.Add( group );
            }

            /// <summary>
            /// Beendet eine Aufzeichnung auf einem Gerät.
            /// </summary>
            /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
            public void Stop( Guid scheduleIdentifier )
            {
                // Locate and validate
                var index = this.FindIndex( scheduleIdentifier );

                // Keep current recording settings
                var recording = m_Recordings[index];

                // Remove it
                m_Recordings.RemoveAt( index );

                // Create a new plan
                try
                {
                    // Get the plan - should NEVER fail or we are in BIG trouble
                    var plan = CreateSchedulePlan();
                    if (plan == null)
                        throw new InvalidOperationException( "Stop" );

                    // Remember the new situation
                    m_CurrentPlan = plan;
                }
                catch
                {
                    // Recover
                    m_Recordings.Insert( index, recording );

                    // Forward
                    throw;
                }
            }

            /// <summary>
            /// Verändert eine Aufzeichnung.
            /// </summary>
            /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
            /// <param name="newEnd">Der neue Endzeitpunkt der Aufzeichnung.</param>
            /// <returns>Gesetzt, wenn die Veränderung möglich ist.</returns>
            public bool Modify( Guid scheduleIdentifier, DateTime newEnd )
            {
                // Locate and validate
                var index = this.FindIndex( scheduleIdentifier );

                // Keep current recording settings
                var recording = m_Recordings[index];

                // Validate
                if (newEnd <= recording.Time.Start)
                    throw new ArgumentOutOfRangeException( "newEnd" );

                // Remove it
                m_Recordings[index] = new ResourceAllocationInformation( recording.Resource, recording.Source, recording.UniqueIdentifier, recording.Name, recording.Time.Start, newEnd - recording.Time.Start );

                // Create a new plan
                try
                {
                    // Try to apply the change
                    var plan = CreateSchedulePlan();
                    if (plan != null)
                    {
                        // Remember the new situation
                        m_CurrentPlan = plan;

                        // Confirm
                        return true;
                    }
                }
                catch
                {
                    // Recover
                    m_Recordings[index] = recording;

                    // Forward
                    throw;
                }

                // Recover
                m_Recordings[index] = recording;

                // Back to normal
                return false;
            }

            /// <summary>
            /// Aktiviert eine Aufzeichnung auf einem Gerät.
            /// </summary>
            /// <param name="resource">Eines der verwalteten Geräte.</param>
            /// <param name="source">Optional eine Quelle, die auf dem Gerät angesteuert werden kann.</param>
            /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
            /// <param name="scheduleName">Der Anzeigename der Aufzeichnung.</param>
            /// <param name="plannedStart">Der ursprüngliche Start der Aufzeichnung in UTC / GMT Notation.</param>
            /// <param name="currentEnd">Das aktuelle Ende der Aufzeichnung in UTC / GMT Notation.</param>
            /// <returns>Gesetzt, wenn die Aufzeichnung auf dem gewünschten Gerät aktiviert werden kann.</returns>
            /// <exception cref="ArgumentNullException">Es wurde kein Gerät angegeben.</exception>
            /// <exception cref="ArgumentException">Das Gerät ist nicht bekannt oder kann die Quelle nicht empfangen.</exception>
            /// <exception cref="ArgumentOutOfRangeException">Die Laufzeit der Aufzeichnung ist nicht positiv.</exception>
            public bool Start( IScheduleResource resource, IScheduleSource source, Guid scheduleIdentifier, string scheduleName, DateTime plannedStart, DateTime currentEnd )
            {
                // Validate
                if (resource == null)
                    throw new ArgumentNullException( "resource" );
                if (!m_Resources.Contains( resource ))
                    throw new ArgumentException( resource.Name, "resource" );
                if (plannedStart >= currentEnd)
                    throw new ArgumentOutOfRangeException( "currentEnd" );
                if (m_Recordings.Any( r => r.UniqueIdentifier == scheduleIdentifier ))
                    throw new ArgumentException( "resource" );

                // Create helper entry
                m_Recordings.Add( new ResourceAllocationInformation( resource, source, scheduleIdentifier, scheduleName, plannedStart, currentEnd - plannedStart ) );

                // May require cleanup
                try
                {
                    // Create the new plan
                    var plan = CreateSchedulePlan();

                    // Did it
                    if (plan != null)
                    {
                        // Remember as current
                        m_CurrentPlan = plan;

                        // Report
                        return true;
                    }
                }
                catch
                {
                    // Cleanup
                    m_Recordings.RemoveAt( m_Recordings.Count - 1 );

                    // Report
                    throw;
                }

                // Cleanup
                m_Recordings.RemoveAt( m_Recordings.Count - 1 );

                // Add to list
                return false;
            }

            /// <summary>
            /// Erzeugt eine passende Planungskomponente.
            /// </summary>
            /// <param name="excludeActiveRecordings">Gesetzt um alle bereits aktiven Aufzeichnungen auszublenden.</param>
            /// <returns>Die zur aktuellen Reservierung passende Planungskomponente.</returns>
            public RecordingScheduler CreateScheduler( bool excludeActiveRecordings = true )
            {
                // Time to create plan
                if (m_CurrentPlan == null)
                    if ((m_CurrentPlan = CreateSchedulePlan()) == null)
                        throw new NotSupportedException( "CreateScheduler" );

                // Create exclusion map
                var alreadyActive = new HashSet<Guid>( excludeActiveRecordings ? m_Recordings.Select( r => r.UniqueIdentifier ) : Enumerable.Empty<Guid>() );

                // Process
                return new RecordingScheduler( m_Resources, alreadyActive, () => m_CurrentPlan, m_PlanComparer );
            }

            #endregion
        }

        /// <summary>
        /// Erzeugt eine neue Geräteverwaltung.
        /// </summary>
        /// <param name="resourceNameComparer">Die Vergleichsmethode </param>
        /// <returns>Die gewünschte neue Verwaltungsinstanz.</returns>
        public static IResourceManager Create( IEqualityComparer<string> resourceNameComparer )
        {
            // Validate
            if (resourceNameComparer == null)
                throw new ArgumentNullException( "resourceNameComparer" );

            // Forward
            return new Implementation( null, resourceNameComparer );
        }

        /// <summary>
        /// Erzeugt eine neue Geräteverwaltung.
        /// </summary>
        /// <param name="rulePath">Der volle Pfad zur Regeldatei.</param>
        /// <param name="resourceNameComparer">Die Vergleichsmethode </param>
        /// <returns>Die gewünschte neue Verwaltungsinstanz.</returns>
        public static IResourceManager Create( string rulePath, IEqualityComparer<string> resourceNameComparer )
        {
            // Validate
            if (resourceNameComparer == null)
                throw new ArgumentNullException( "resourceNameComparer" );
            if (string.IsNullOrEmpty( rulePath ))
                throw new ArgumentNullException( "rulePath" );

            // Forward
            if (File.Exists( rulePath ))
                return new Implementation( rulePath, resourceNameComparer );
            else
                return new Implementation( null, resourceNameComparer );
        }
    }
}
