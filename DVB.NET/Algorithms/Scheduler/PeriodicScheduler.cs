using System;
using System.Linq;
using System.Collections.Generic;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Diese Klasse übernimmt die periodische Ausführung von Sonderaktionen, wie
    /// etwa der Aktualisierung von Programmzeitschrift oder Liste der Quellen.
    /// </summary>
    /// <remarks>
    /// Alle Zeitangaben sind in GMT / UTC Notation.
    /// </remarks>
    public abstract class PeriodicScheduler : IScheduleDefinition
    {
        /// <summary>
        /// Ein Name zur Identifikation der Aufgabe.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Meldet die aktuelle Uhrzeit und kann zu Testzwecken überschrieben werden.
        /// </summary>
        public Func<DateTime> GetNow = () => DateTime.UtcNow;

        /// <summary>
        /// Eine eindeutige Identifikation der Aktion.
        /// </summary>
        public Guid UniqueIdentifier { get; private set; }

        /// <summary>
        /// Initialisiert eine neue Instanz.
        /// </summary>
        /// <param name="name">Eine Name zur Identifikation der Aufgabe.</param>
        /// <param name="uniqueIdentifier">Die eindeutige Kennung dieser Aufgabe.</param>
        protected PeriodicScheduler( string name, Guid uniqueIdentifier )
        {
            // Remember
            UniqueIdentifier = uniqueIdentifier;
            Name = name;
        }

        /// <summary>
        /// Meldet, ob die Ausführung grundsätzlich aktiviert ist.
        /// </summary>
        public abstract bool IsEnabled { get; }

        /// <summary>
        /// Meldet wenn möglich den Zeitpunkt, an dem letztmalig ein Durchlauf
        /// stattgefunden hat.
        /// </summary>
        public abstract DateTime? LastRun { get; }

        /// <summary>
        /// Meldet die Zeitspanne nach der ein neuer Durchlauf gestartet werden darf,
        /// wenn der Computer sowieso gerade aktiv ist.
        /// </summary>
        public abstract TimeSpan? JoinThreshold { get; }

        /// <summary>
        /// Meldet die Zeitspanne, die mindestens zwischen zwei Durchläufen liegen soll.
        /// </summary>
        public abstract TimeSpan DefaultInterval { get; }

        /// <summary>
        /// Meldet die bevorzugten Uhrzeiten für eine Ausführung. Die verwendeten Zeiten
        /// bezeichnen dabei Stunden in der lokalen Zeitzone.
        /// </summary>
        public abstract uint[] PreferredHours { get; }

        /// <summary>
        /// Die maximal erlaubte Laufzeit.
        /// </summary>
        public abstract TimeSpan Duration { get; }

        /// <summary>
        /// Ermittelt den nächsten Zeitpunkt für die Ausführung.
        /// </summary>
        public DateTime? NextSchedule
        {
            get
            {
                // Forward
                return GetNextSchedule( LastRun, GetNow() );
            }
        }

        /// <summary>
        /// Ermittelt den nächsten Zeitpunkt für die Ausführung.
        /// </summary>
        /// <param name="lastRun">Der Zeitpunkt der letzten Ausführung.</param>
        /// <param name="now">Die aktuelle Urzeit.</param>
        private DateTime? GetNextSchedule( DateTime? lastRun, DateTime now )
        {
            // Generally disabled
            if (!IsEnabled)
                return null;

            // Interval must be set positive - actually this should be included in IsEnabled but to be safe just test it again
            var interval = DefaultInterval;
            if (interval.TotalSeconds <= 0)
                return null;

            // Never run before - run now
            if (!lastRun.HasValue)
                return now;

            // See if we are in manual mode meaning that we need an explicit trigger to run which resets the last run time
            if (interval == TimeSpan.MaxValue)
                return null;

            // In hibernation mode we are allowed to schedule immediatly if join span has been expired
            var join = JoinThreshold;
            if (join.HasValue)
                if (join.Value.TotalSeconds > 0)
                    if (lastRun.Value < now - join.Value)
                        return now;

            // Move ahead to the next allowed point in time
            var nextRun = lastRun.Value + interval;

            // Map to next full hour in local time zone - using ticks allows us to detect all fractions down to the supported granularity of 0.1µs
            var nextLocal = nextRun.ToLocalTime();
            if ((nextLocal.Ticks % TimeSpan.TicksPerHour) != 0)
                nextLocal = nextLocal.Date.AddHours( nextLocal.Hour + 1 );

            // Find the nearest hour after the indicated start - if hour from list is below the indicated start hour we have to add just another day in local time
            var hours = (PreferredHours ?? Enumerable.Empty<uint>()).Where( hour => hour < 24 );
            var bestHour = hours.Select<uint, uint?>( h => (h < nextLocal.Hour) ? (h + 24) : h ).Min();
            if (bestHour.HasValue)
                nextRun = nextLocal.Date.AddHours( bestHour.Value ).ToUniversalTime();

            // Report preferred offset time
            if (nextRun < now)
                return now;
            else
                return nextRun;
        }

        #region IScheduleDefinition Members

        /// <summary>
        /// Meldet alle Ausführungszeiten.
        /// </summary>
        /// <param name="minTime">Der früheste Ausführungszeitpunkt.</param>
        /// <returns>Alle möglichen Zeiten.</returns>
        public IEnumerable<SuggestedPlannedTime> GetTimes( DateTime minTime )
        {
            // Process
            if (Duration.TotalSeconds > 0)
                for (var lastRun = LastRun; ; minTime = lastRun.Value)
                {
                    // Load next
                    var next = GetNextSchedule( lastRun, minTime );
                    if (!next.HasValue)
                        yield break;

                    // Create plan
                    SuggestedPlannedTime plan = new PlannedTime { Start = next.Value, Duration = Duration };

                    // Can always report - will never be smaller than the minimum time
                    yield return plan;

                    // Advance - please not that we use the eventually modified real schedule time not the one we begged for
                    lastRun = plan.Planned.End;
                }
        }

        /// <summary>
        /// Meldet alle Geräte, die eine Ausführung vornehmen können.
        /// </summary>
        public abstract IScheduleResource[] Resources { get; }

        #endregion
    }
}
