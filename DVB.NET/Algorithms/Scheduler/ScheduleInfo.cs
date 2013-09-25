using System;


namespace JMS.DVB.Algorithms.Scheduler
{

    /// <summary>
    /// Beschreibt eine auszuführende Aufzeichnung.
    /// </summary>
    internal class ScheduleInfo : IScheduleInformation
    {
        /// <summary>
        /// Die ursprüngliche Planung.
        /// </summary>
        public IScheduleDefinition Definition { get; private set; }

        /// <summary>
        /// Das zu verwendende Gerät.
        /// </summary>
        public IScheduleResource Resource { get; private set; }

        /// <summary>
        /// Die tatsächliche Ausführungszeit.
        /// </summary>
        public PlannedTime Time { get; private set; }

        /// <summary>
        /// Gesetzt, wenn die Ausführung verspätet beginnt.
        /// </summary>
        public bool StartsLate { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="definition">Die ursprüngliche Beschreibung der Aufzeichnung.</param>
        /// <param name="resource">Das zu verwendende Gerät.</param>
        /// <param name="time">Die tatsächliche Ausführungszeit.</param>
        /// <param name="late">Gesetzt, wenn die Ausführung verspätet beginnt.</param>
        internal ScheduleInfo( IScheduleDefinition definition, IScheduleResource resource, PlannedTime time, bool late )
        {
            // Remember
            Definition = definition;
            Resource = resource;
            StartsLate = late;
            Time = time;
        }

        /// <summary>
        /// Erzeugt einen Anzeigetext zu Testzwecken.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // Test
            var recording = Definition as IRecordingDefinition;

            // Format
            return
                string.Format
                    (
                        Properties.SchedulerResources.Debug_ScheduleInformation,
                        Time.Start,
                        Time.End,
                        StartsLate ? "*" : string.Empty,
                        Resource,
                        (recording == null) ? null : recording.Source
                    );
        }
    }
}
