using System;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Beschreibt eine Ausnahme einer geplanten Aufzeichnung.
    /// </summary>
    public struct PlanException
    {
        /// <summary>
        /// Das Datum in der lokalen Zeitzone, für das diese Ausnahme gültig ist.
        /// </summary>
        public DateTime ExceptionDate;

        /// <summary>
        /// Die Veränderung, die am Startzeitpunkt vorzunehmen ist.
        /// </summary>
        public TimeSpan StartDelta;

        /// <summary>
        /// Die Veränderung, die an der Dauer vorzunehmen ist.
        /// </summary>
        public TimeSpan DurationDelta;
    }
}
