using JMS.DVB.Algorithms.Scheduler;
using System;


namespace JMS.DVBVCR.RecordingService.Planning
{
    /// <summary>
    /// Beschreibt eine aktive Aufzeichnung.
    /// </summary>
    public class ScheduleInformation
    {
        /// <summary>
        /// Die originalen Informationen.
        /// </summary>
        public IScheduleInformation Schedule { get; private set; }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="original">Die originalen Informationen.</param>
        public ScheduleInformation( IScheduleInformation original )
        {
            // Validate
            if (original == null)
                throw new ArgumentNullException( nameof( original ) );

            // Remember
            RealTime = original.Time;
            Schedule = original;
        }

        /// <summary>
        /// Meldet oder ändert den Zeitraum der Aufzeichnung.
        /// </summary>
        public PlannedTime RealTime { get; set; }
    }
}
