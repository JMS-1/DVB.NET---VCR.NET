using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JMS.DVB.Algorithms.Scheduler;
using JMS.DVBVCR.RecordingService.Persistence;


namespace JMS.DVBVCR.RecordingService.Planning
{
    /// <summary>
    /// Meldet Detailinformationen zu einer Liste von Aufzeichnungen.
    /// </summary>
    public class PlanContext : IEnumerable<IScheduleInformation>
    {
        /// <summary>
        /// Vergleicht Planungen nach dem Startzeitpunkt.
        /// </summary>
        public static readonly IComparer<IScheduleInformation> ByStartComparer = new ScheduleInformationComparer();

        /// <summary>
        /// Vergleicht Planungen nach dem Startzeitpunkt.
        /// </summary>
        private class ScheduleInformationComparer : IComparer<IScheduleInformation>
        {
            /// <summary>
            /// Vergleicht zwei Planungseinträge.
            /// </summary>
            /// <param name="left">Die erste Planung.</param>
            /// <param name="right">Die zweite Planung.</param>
            /// <returns>Die Anordnung der beiden Planungen.</returns>
            public int Compare( IScheduleInformation left, IScheduleInformation right )
            {
                // Check mode
                if (left == null)
                    if (right == null)
                        return 0;
                    else
                        return -1;
                else if (right == null)
                    return +1;
                else
                    return left.Time.Start.CompareTo( right.Time.Start );
            }
        }

        /// <summary>
        /// Erlaubt es, zu jeder Aufzeichnung den zugehörigen Auftrag nachzuschlagen.
        /// </summary>
        private readonly Dictionary<Guid, VCRJob> m_jobsBySchedule = new Dictionary<Guid, VCRJob>();

        /// <summary>
        /// Alle laufenden Aufzeichnungen.
        /// </summary>
        private readonly Dictionary<Guid, ScheduleInformation> m_running;

        /// <summary>
        /// Der gesamte Aufzeichnungsplan.
        /// </summary>
        private List<IScheduleInformation> m_schedules = new List<IScheduleInformation>();

        /// <summary>
        /// Erstellt eine neue Detailinformation.
        /// </summary>
        /// <param name="running">Alle laufenden Aufzeichnungen.</param>
        internal PlanContext( IEnumerable<ScheduleInformation> running )
        {
            // Remember
            m_running = (running ?? Enumerable.Empty<ScheduleInformation>()).ToDictionary( info => info.Schedule.Definition.UniqueIdentifier );
        }

        /// <summary>
        /// Ermittelt Daten zu einer laufenden Aufzeichnung.
        /// </summary>
        /// <param name="uniqueIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
        /// <returns>Die Informationen, mit denen die Aufzeichnung gestartet wurde.</returns>
        public ScheduleInformation GetRunState( Guid uniqueIdentifier )
        {
            // Report
            ScheduleInformation info;
            return m_running.TryGetValue( uniqueIdentifier, out info ) ? info : null;
        }

        /// <summary>
        /// Ergänz die Abbildung von einer Aufzeichnung auf einen Auftrag.
        /// </summary>
        /// <param name="schedule">Eine Aufzeichnung.</param>
        /// <param name="job">Der zugehörige Auftrag.</param>
        public void RegisterSchedule( VCRSchedule schedule, VCRJob job )
        {
            // Validate
            if (schedule == null)
                throw new ArgumentNullException( "schedule" );
            if (job == null)
                throw new ArgumentNullException( "job" );

            // Skip on missing identifier
            var scheduleIdentifier = schedule.UniqueID;
            if (scheduleIdentifier.HasValue)
                m_jobsBySchedule.Add( scheduleIdentifier.Value, job );
        }

        /// <summary>
        /// Versucht zu einer Aufzeichnung den zugehörigen Auftrag zu finden.
        /// </summary>
        /// <param name="scheduleIdentifier">Die Kennung einer Aufzeichnung.</param>
        /// <returns>Der zugehörige Auftrag oder <i>null</i>.</returns>
        public VCRJob TryFindJob( Guid scheduleIdentifier )
        {
            // Test
            VCRJob job;
            return m_jobsBySchedule.TryGetValue( scheduleIdentifier, out job ) ? job : null;
        }

        /// <summary>
        /// Versucht zu einer Aufzeichnung den zugehörigen Auftrag zu finden.
        /// </summary>
        /// <param name="schedule">Eine Aufzeichnung.</param>
        /// <returns>Der zugehörige Auftrag oder <i>null</i>.</returns>
        public VCRJob TryFindJob( VCRSchedule schedule )
        {
            // Validate
            if (schedule == null)
                throw new ArgumentNullException( "schedule" );

            // Skip on missing identifier
            var scheduleIdentifier = schedule.UniqueID;
            if (!scheduleIdentifier.HasValue)
                return null;

            // Forward
            return TryFindJob( scheduleIdentifier.Value );
        }

        /// <summary>
        /// Vermerkt den Aufzeichnungsplan.
        /// </summary>
        /// <param name="planItems">Alle einzelnen Aufzeichnungen.</param>
        public void LoadPlan( IEnumerable<IScheduleInformation> planItems )
        {
            // Load
            m_schedules = (planItems ?? Enumerable.Empty<IScheduleInformation>()).ToList();

            // Finish
            m_schedules.Sort( ByStartComparer );
        }

        /// <summary>
        /// Meldet alle Aufzeichnungen.
        /// </summary>
        /// <returns>Die Liste aller Aufzeichnungen.</returns>
        public IEnumerator<IScheduleInformation> GetEnumerator()
        {
            // Forward
            return m_schedules.GetEnumerator();
        }

        /// <summary>
        /// Meldet alle Aufzeichnungen.
        /// </summary>
        /// <returns>Die Liste aller Aufzeichnungen.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            // Forward
            return GetEnumerator();
        }
    }
}
