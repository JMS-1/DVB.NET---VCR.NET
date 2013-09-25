using System;
using System.Linq;
using System.Collections.Generic;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Diese Schnittstelle wird von einer Verwaltungsinstanz für Geräte angeboten.
    /// </summary>
    public interface IResourceManager : IDisposable
    {
        /// <summary>
        /// Der Algorithmus zum Vergleich der Namen von Geräten.
        /// </summary>
        IEqualityComparer<string> ResourceNameComparer { get; }

        /// <summary>
        /// Meldet alle bekannten Geräte.
        /// </summary>
        IScheduleResource[] Resources { get; }

        /// <summary>
        /// Meldet die aktuelle Belegung der Geräte.
        /// </summary>
        IResourceAllocationInformation[] CurrentAllocations { get; }

        /// <summary>
        /// Meldet die Vergleichsmethode für Geräte.
        /// </summary>
        IEqualityComparer<IScheduleResource> ResourceComparer { get; }

        /// <summary>
        /// Ergänzt ein Gerät.
        /// </summary>
        /// <param name="resource">Das gewünschte Gerät.</param>
        void Add( IScheduleResource resource );

        /// <summary>
        /// Meldet komplete Entschlüsselungsregeln an.
        /// </summary>
        /// <param name="group">Eine neue Regel.</param>
        /// <exception cref="ArgumentNullException">Die Regel ist ungültig.</exception>
        void Add( DecryptionGroup group );

        /// <summary>
        /// Aktiviert eine Aufzeichnung auf einem Gerät.
        /// </summary>
        /// <param name="resource">Eines der verwalteten Geräte.</param>
        /// <param name="source">Eine Quelle, die auf dem Gerät abgespielt werden kann.</param>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
        /// <param name="scheduleName">Der Anzeigename der Aufzeichnung.</param>
        /// <param name="plannedStart">Der ursprüngliche Start der Aufzeichnung in UTC / GMT Notation.</param>
        /// <param name="currentEnd">Das aktuelle Ende der Aufzeichnung in UTC / GMT Notation.</param>
        /// <returns>Gesetzt, wenn die Aufzeichnung auf dem gewünschten Gerät aktiviert werden kann.</returns>
        bool Start( IScheduleResource resource, IScheduleSource source, Guid scheduleIdentifier, string scheduleName, DateTime plannedStart, DateTime currentEnd );

        /// <summary>
        /// Beendet eine Aufzeichnung auf einem Gerät.
        /// </summary>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
        void Stop( Guid scheduleIdentifier );

        /// <summary>
        /// Verändert eine Aufzeichnung.
        /// </summary>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
        /// <param name="newEnd">Der neue Endzeitpunkt der Aufzeichnung.</param>
        /// <returns>Gesetzt, wenn die Veränderung möglich ist.</returns>
        bool Modify( Guid scheduleIdentifier, DateTime newEnd );

        /// <summary>
        /// Erzeugt eine passende Planungskomponente.
        /// </summary>
        /// <param name="excludeActiveRecordings">Gesetzt um alle bereits aktiven Aufzeichnungen auszublenden.</param>
        /// <returns>Die zur aktuellen Reservierung passende Planungskomponente.</returns>
        RecordingScheduler CreateScheduler( bool excludeActiveRecordings = true );
    }

    /// <summary>
    /// Methoden zur einfacheren Nutzung der <see cref="IResourceManager"/> Schnittstelle.
    /// </summary>
    public static class IResourceManagerExtensions
    {
        /// <summary>
        /// Überträgt eine geplante Ausführung.
        /// </summary>
        /// <param name="manager">Die zu erweiternde Schnittstelle.</param>
        /// <param name="schedule">Die Planungsinformationen zur Aufzeichnung.</param>
        /// <returns>Gesetzt, wenn ein Start möglich war.</returns>
        /// <exception cref="NullReferenceException">Es wurde keine Schnittstelle angegeben.</exception>
        /// <exception cref="ArgumentNullException">Es wurde keine Planung angegeben.</exception>
        public static bool Start( this IResourceManager manager, IScheduleInformation schedule )
        {
            // Validate
            if (manager == null)
                throw new NullReferenceException();
            if (schedule == null)
                throw new ArgumentNullException( "schedule" );

            // Load
            var definition = schedule.Definition;
            var recording = definition as IRecordingDefinition;

            // Forward
            return manager.Start( schedule.Resource, (recording == null) ? null : recording.Source, definition.UniqueIdentifier, definition.Name, schedule.Time.Start, schedule.Time.End );
        }

        /// <summary>
        /// Meldet den Zeitpunkt der letzten Nutzung eines Gerätes.
        /// </summary>
        /// <param name="manager">Die zu erweiternde Schnittstelle.</param>
        /// <returns>Der letzte Zeitpunkt aller aktiven Aufzeichnungen und Aufgaben.</returns>
        public static DateTime? GetEndOfAllocation( this IResourceManager manager )
        {
            // Validate
            if (manager == null)
                throw new NullReferenceException();

            // Load and forward
            var recordings = manager.CurrentAllocations;
            if (recordings.Length < 1)
                return null;
            else
                return recordings.Max( r => r.Time.End );
        }

        /// <summary>
        /// Ermittelt die laufende Nummer einer Aufzeichnung.
        /// </summary>
        /// <param name="manager">Die zu erweiternde Schnittstelle.</param>
        /// <param name="uniqueIdentifier"></param>
        /// <returns>Die laufende Nummer der Aufzeichnung.</returns>
        /// <exception cref="NullReferenceException">Es wurde keine Schnittstelle angegeben.</exception>
        /// <exception cref="ArgumentException">Die bezeichnete Aufzeichnung ist nicht bekannt.</exception>
        public static int FindIndex( this IResourceManager manager, Guid uniqueIdentifier )
        {
            // Validate
            if (manager == null)
                throw new NullReferenceException();

            // Locate
            var allocations = manager.CurrentAllocations;
            for (var index = 0; index < allocations.Length; index++)
                if (allocations[index].UniqueIdentifier == uniqueIdentifier)
                    return index;

            // Failed
            throw new ArgumentException( uniqueIdentifier.ToString(), "uniqueIdentifier" );
        }

        /// <summary>
        /// Ermittelt alle ausstehenden Aufzeichnungen unter Berücksichtigung der aktuellen Zuordnung zu den Geräten.
        /// </summary>
        /// <param name="manager">Die zu erweiternde Schnittstelle.</param>
        /// <param name="now">Der Referenzzeitpunkt.</param>
        /// <param name="getSchedules">Methode zur Übertragung der Aufzeichnungen und Aufgaben in den Planungsalgorithmus.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Initialisierungsmethode angegeben.</exception>
        /// <returns>Die Liste der Aufzeichnungen.</returns>
        public static IEnumerable<IScheduleInformation> GetSchedules( this IResourceManager manager, DateTime now, Func<RecordingScheduler, DateTime, IEnumerable<IScheduleInformation>> getSchedules )
        {
            // Validate
            if (manager == null)
                throw new NullReferenceException();
            if (getSchedules == null)
                throw new ArgumentNullException( "getSchedules" );

            // Create schedule manager
            var scheduler = manager.CreateScheduler( false );

            // Attach to anything current 
            var active = manager.CurrentAllocations.ToDictionary( a => a.UniqueIdentifier );

            // Process all
            foreach (var schedule in getSchedules( scheduler, now ))
            {
                // We guess this is the same if the recording starts before the active one ends
                IResourceAllocationInformation allocation;
                if (active.TryGetValue( schedule.Definition.UniqueIdentifier, out allocation ))
                    if (ReferenceEquals( allocation, schedule.Definition ))
                        continue;
                    else if (schedule.Time.Start < allocation.Time.End)
                        continue;

                // Report
                yield return schedule;
            }
        }

        /// <summary>
        /// Ermittelt die nächste auszuführende Aktion.
        /// </summary>
        /// <param name="manager">Die zu erweiternde Schnittstelle.</param>
        /// <param name="now">Der zu verwendene Referenzzeitpunkt.</param>
        /// <param name="getSchedules">Methode zum Ermitteln der als nächstes auszuführenden Aufzeichnungen und Aufträge.</param>
        /// <returns>Die nächste Aktion, sofern bekannt.</returns>
        /// <exception cref="NullReferenceException">Es wurde keine Schnittstelle angegeben.</exception>
        /// <exception cref="ArgumentNullException">Es wurde keine Initialisierungsmethode angegeben.</exception>
        public static ResourceActivity GetNextActivity( this IResourceManager manager, DateTime now, Func<RecordingScheduler, DateTime, IEnumerable<IScheduleInformation>> getSchedules )
        {
            // Validate
            if (manager == null)
                throw new NullReferenceException();
            if (getSchedules == null)
                throw new ArgumentNullException( "getSchedules" );

            // Create schedule manager
            var scheduler = manager.CreateScheduler( true );

            // Get the next to stop
            var allocations = manager.CurrentAllocations;
            var stopTime = (allocations.Length > 0) ? allocations.Min( a => a.Time.End ) : default( DateTime? );

            // Process anything scheduled for stop to make devices free
            if (stopTime.HasValue)
                if (stopTime.Value <= now)
                    return new StopActivity( allocations.First( a => a.Time.End == stopTime.Value ).UniqueIdentifier );

            // See if there is anything in the plan
            var nextToStart = getSchedules( scheduler, now ).FirstOrDefault();
            if (nextToStart != null)
            {
                // Find the start time
                var startTime = nextToStart.Time.Start;
                if (startTime <= now)
                    return new StartActivity( nextToStart );

                // Get the minimum
                if (!stopTime.HasValue)
                    stopTime = startTime;
                else if (startTime < stopTime.Value)
                    stopTime = startTime;
            }

            // Just wait
            if (stopTime.HasValue)
                return new WaitActivity( stopTime.Value );
            else
                return null;
        }
    }
}
