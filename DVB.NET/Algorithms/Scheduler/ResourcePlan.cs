using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Verwaltet die Planung für ein einzelnes Gerät.
    /// </summary>
    internal class ResourcePlan
    {
        /// <summary>
        /// Vergleicht zwei Planungen nach der Anzahl der Aufzeichnungen.
        /// </summary>
        public static readonly IComparer<ResourcePlan> CompareByRecordingCount = new RecordingCountComparer();

        /// <summary>
        /// Vergleicht zwei Pläne nach der Anzahl der verwendeten Quellen.
        /// </summary>
        public static readonly IComparer<ResourcePlan> CompareBySourceCount = new SourceCountComparer();

        /// <summary>
        /// Vergleicht zwei Planungen nach der Anzahl der Aufzeichnungen.
        /// </summary>
        private class RecordingCountComparer : IComparer<ResourcePlan>
        {
            /// <summary>
            /// Vergleicht zwei Planungen nach der Anzahl der Aufzeichnungen.
            /// </summary>
            /// <param name="firstPlan">Die erste Planung.</param>
            /// <param name="secondPlan">Die zweite Planung.</param>
            /// <returns>Positiv, wenn der erste Plan mehr Aufzeichnungen enthält als der zweite.</returns>
            public int Compare( ResourcePlan firstPlan, ResourcePlan secondPlan )
            {
                // Forward
                return firstPlan.RecordingCount.CompareTo( secondPlan.RecordingCount );
            }
        }

        /// <summary>
        /// Vergleicht zwei Planungen nach der Anzahl der Quellen.
        /// </summary>
        private class SourceCountComparer : IComparer<ResourcePlan>
        {
            /// <summary>
            /// Vergleicht zwei Planungen nach der Anzahl der Quellen.
            /// </summary>
            /// <param name="firstPlan">Die erste Planung.</param>
            /// <param name="secondPlan">Die zweite Planung.</param>
            /// <returns>Positiv, wenn der erste Plan mehr Quellen enthält als der zweite.</returns>
            public int Compare( ResourcePlan firstPlan, ResourcePlan secondPlan )
            {
                // Forward
                return firstPlan.NumberOfDifferentSources.CompareTo( secondPlan.NumberOfDifferentSources );
            }
        }

        /// <summary>
        /// Beschreibt eine geplante Aufzeichnung.
        /// </summary>
        private class _RecordingItem
        {
            /// <summary>
            /// Die Beschreibung der Aufzeichnung.
            /// </summary>
            public IRecordingDefinition Recording { get; private set; }

            /// <summary>
            /// Die vorgesehene Ausführungszeit.
            /// </summary>
            public PlannedTime Time { get; private set; }

            /// <summary>
            /// Gesetzt, wenn die Aufzeichnung verspätet beginnt.
            /// </summary>
            public bool StartsLate { get; private set; }

            /// <summary>
            /// Erzeugt eine neue Beschreibung.
            /// </summary>
            /// <param name="recording">Die zugehörige Aufzeichnung.</param>
            /// <param name="time">Die vorgesehene Ausführungszeit.</param>
            /// <param name="startsLate">Gesetzt, wenn die Aufzeichnung verspätet beginnt.</param>
            public _RecordingItem( IRecordingDefinition recording, PlannedTime time, bool startsLate )
            {
                // Remember
                StartsLate = startsLate;
                Recording = recording;
                Time = time;
            }
        }

        /// <summary>
        /// Meldet das zugehörige Gerät.
        /// </summary>
        public IScheduleResource Resource { get; private set; }

        /// <summary>
        /// Alle Aufzeichnungen, die auf diesem Gerät ausgeführt werden sollen.
        /// </summary>
        private List<_RecordingItem> m_Recordings = new List<_RecordingItem>();

        /// <summary>
        /// Die Zähler für die Entschlüsselung.
        /// </summary>
        public HashSet<Guid> DecryptionCounters { get; private set; }

        /// <summary>
        /// Meldet die im Rahmen der Planung insgesamt verworfenen Anteil der Aufzeichnungen.
        /// </summary>
        public TimeSpan TotalCut { get; private set; }

        /// <summary>
        /// Meldet die Anzahl der im Rahmen der Planung nicht vollständig ausgeführten Aufzeichnungen.
        /// </summary>
        public int CutRecordings { get; private set; }

        /// <summary>
        /// Die zugehörige Gesamtplanung.
        /// </summary>
        public SchedulePlan SchedulePlan { get; private set; }

        /// <summary>
        /// Die auf diesem Gerät in Benutzung befindlichen Quellen.
        /// </summary>
        public AllocationMap Allocations { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Planung.
        /// </summary>
        /// <param name="resource">Das zugehörige Gerät.</param>
        /// <param name="schedulePlan">Die zugehörige Gesamtplanung.</param>
        /// <param name="decryptionCounter">Die Zähler für die Entschlüsselung.</param>
        /// <param name="allocations">Optional alle bereits vorgenommenen Zuordnungen.</param>
        /// <param name="planTime">Der aktuelle Planungsbeginn, sofern bekannt.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Gerät angegeben.</exception>
        public ResourcePlan( IScheduleResource resource, SchedulePlan schedulePlan, HashSet<Guid> decryptionCounter = null, AllocationMap allocations = null, DateTime? planTime = null )
        {
            // Remember
            SchedulePlan = schedulePlan;
            Resource = resource;

            // Register a single decryption counter
            DecryptionCounters = decryptionCounter ?? new HashSet<Guid> { schedulePlan.RegisterDecryption( Resource.Decryption.MaximumParallelSources ) };

            // Check for allocation
            if (allocations != null)
            {
                // Just clone
                Allocations = allocations.Clone( planTime );
            }
            else
            {
                // Number of sources we may use
                var sourceLimit = resource.SourceLimit;
                if (sourceLimit < 1)
                    sourceLimit = int.MaxValue;

                // Create brand new
                Allocations = new AllocationMap( sourceLimit, CheckForSourceGroupMatch );
            }
        }

        /// <summary>
        /// Erzeugt eine Kopie dieser Planung.
        /// </summary>
        /// <param name="original">Die originalen Planungsdaten.</param>
        /// <param name="schedulePlan">Die zugehörige Gesamtplanung.</param>
        private ResourcePlan( ResourcePlan original, SchedulePlan schedulePlan )
            : this( original.Resource, schedulePlan, original.DecryptionCounters, original.Allocations )
        {
            // Finish clone process
            m_Recordings.AddRange( original.m_Recordings );
            CutRecordings = original.CutRecordings;
            TotalCut = original.TotalCut;
        }

        /// <summary>
        /// Meldet alle auf diesem Gerät geplanten Aufzeichnungen.
        /// </summary>
        /// <returns>Alle Aufzeichnungen.</returns>
        public IEnumerable<ScheduleInfo> GetRecordings()
        {
            // Cache resource to avoid multiple lookups during the projection
            var resource = Resource;

            // Forward
            return m_Recordings.Select( r => new ScheduleInfo( r.Recording, resource, r.Time, r.StartsLate ) );
        }

        /// <summary>
        /// Meldet die Anzahl der Aufzeichnungen auf diesem Gerät.
        /// </summary>
        public int RecordingCount { get { return m_Recordings.Count; } }

        /// <summary>
        /// Meldet die Anzahl der unterschiedlichen Quellen auf diesem Gerät.
        /// </summary>
        public int NumberOfDifferentSources { get { return Allocations.NumberOfDifferentSources; } }

        /// <summary>
        /// Erzeugt eine neue Instanz unter Berücksichtigung der vorgenommenen Gerätezuordnung.
        /// </summary>
        /// <param name="plan">Ein neuer Plan.</param>
        /// <param name="planTime">Der aktuelle Planungsbeginn, sofern bekannt.</param>
        /// <returns>Eine Beschreibung des Gerätes.</returns>
        public ResourcePlan Restart( SchedulePlan plan, DateTime? planTime )
        {
            // Create new - this will reset the allocation map to improve performance
            return new ResourcePlan( Resource, plan, DecryptionCounters, Allocations, planTime );
        }

        /// <summary>
        /// Reserviert 
        /// </summary>
        /// <param name="until">Die Zeit, bis zu der eine Reservierung stattfinden soll.</param>
        /// <returns>Gesetzt, wenn eine Reservierung möglich war.</returns>
        public bool Reserve( DateTime until )
        {
            // Report
            return Allocations.SetStart( until );
        }

        /// <summary>
        /// Prüft, ob eine Aufzeichnung ergänzt werden kann.
        /// </summary>
        /// <param name="recording">Die neue Aufzeichnung.</param>
        /// <param name="time">Der Zeitraum der neuen Aufzeichnung.</param>
        /// <param name="minTime">Die Aufzeichnung darf auf keinen Fall vor diesem Zeitpunkt beginnen.</param>
        /// <returns>Gesetzt, wenn eine Aufzeichnung möglich war.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Aufzeichnung übergeben.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Die Startzeit liegt vor der Aktivierung des Gerätes.</exception>
        public bool Add( IRecordingDefinition recording, SuggestedPlannedTime time, DateTime minTime )
        {
            // Validate
            if (recording == null)
                throw new ArgumentNullException( "recording" );

            // If the current resource can see the source we can do nothing at all
            var resource = Resource;
            if (!resource.CanAccess( recording.Source ))
                return false;

            // If the recording is bound to dedicated sources but not use we should not process
            var allowedResources = recording.Resources;
            if (allowedResources != null)
                if (allowedResources.Length > 0)
                    if (!allowedResources.Any( r => ReferenceEquals( r, resource ) ))
                        return false;

            // See if we have to cut it off and load the corresponding end of the recording
            var initialPlan = time.Planned;

            // Clip to minimum allowed
            if (time.Planned.Start < minTime)
            {
                // Not possible at all - should never ever be requested but better be safe
                if (time.Planned.End <= minTime)
                    return false;

                // Correct the parameters - end is calculated from start and duration
                time.Planned.Duration = time.Planned.End - minTime;
                time.Planned.Start = minTime;
            }

            // See if device could at least receive
            var sourceAllocation = Allocations.PrepareAllocation( recording.Source, time );
            if (sourceAllocation == null)
                return false;

            // Add it - will clip the time accordingly
            sourceAllocation.Allocate();

            // Time to check for encryption - technically it should not be possible to have a recording on a source which is for some time encypted and for some other time not but better be safe
            if (recording.Source.IsEncrypted)
            {
                // What to check for
                var counters = DecryptionCounters.Select( i => SchedulePlan.DecryptionCounters[i] ).ToArray();

                // Check all
                foreach (var counter in counters)
                    if (counter.PrepareAllocation( recording.Source, time ) == null)
                        return false;

                // Now reserve all - this may manipulate the time slice for the recording as well
                foreach (var counter in counters)
                {
                    // Allocate again - we may only call the allocate immediatly after preparing
                    var allocation = counter.PrepareAllocation( recording.Source, time );
                    if (allocation == null)
                        throw new InvalidOperationException( "PrepareAllocation" );

                    // Process
                    allocation.Allocate();
                }
            }

            // Remember recording
            m_Recordings.Add( new _RecordingItem( recording, time.Planned, time.Planned.Start > initialPlan.Start ) );

            // Get cut time
            var delta = initialPlan.Duration - time.Planned.Duration;

            // Adjust cut time - this will be taken in account when checking weights to get the best plan           
            TotalCut += delta;

            // Count failures
            if (delta.Ticks > 0)
                CutRecordings += 1;

            // We did it
            return true;
        }

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Planung.
        /// </summary>
        /// <param name="schedulePlan">Die zugehörige Gesamtplanung.</param>
        /// <returns>Die gewünschte Kopie.</returns>
        public ResourcePlan Clone( SchedulePlan schedulePlan )
        {
            // Forward
            return new ResourcePlan( this, schedulePlan );
        }

        /// <summary>
        /// Prüft, ob eine Quelle zu einer Liste von Quellen hinzugefügt werden darf.
        /// </summary>
        /// <param name="source">Eine neue Quelle.</param>
        /// <param name="sources">Eine Liste von Quellen, durch genau diese Methode aller auf der
        /// selben Quellgruppe.</param>
        /// <returns>Gesetzt, wenn die Quelle die korrekte Quellgruppe verwendet.</returns>
        private static bool CheckForSourceGroupMatch( IScheduleSource source, IEnumerable<IScheduleSource> sources )
        {
            // Process
            var any = sources.FirstOrDefault();
            if (any == null)
                return true;
            else
                return any.BelongsToSameSourceGroupAs( source );
        }

        /// <summary>
        /// Meldet, wann die aktuelle Planung endet.
        /// </summary>
        public DateTime PlanEnd { get { return Allocations.PlanEnd; } }

        /// <summary>
        /// Meldet, wann die aktuelle Planung beginnt.
        /// </summary>
        public DateTime PlanStart { get { return Allocations.PlanStart; } }
    }
}
