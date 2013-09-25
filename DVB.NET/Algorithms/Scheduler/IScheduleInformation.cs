using System;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Beschreibt eine Aufzeichnung.
    /// </summary>
    public interface IScheduleInformation
    {
        /// <summary>
        /// Das Gerät, auf dem die Ausführung stattfinden soll.
        /// </summary>
        IScheduleResource Resource { get; }

        /// <summary>
        /// Die zugehörige Aufzeichnung.
        /// </summary>
        IScheduleDefinition Definition { get; }

        /// <summary>
        /// Der Zeitbereich der Aufzeichnung.
        /// </summary>
        PlannedTime Time { get; }

        /// <summary>
        /// Gesetzt, wenn die Aufzeichnung verglichen mit der Planung verspätet beginnt.
        /// </summary>
        bool StartsLate { get; }
    }
}
