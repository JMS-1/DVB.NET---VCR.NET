using System;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Beschreibt einen Vorschlag für eine geplante Aufzeichung.
    /// </summary>
    public class SuggestedPlannedTime
    {
        /// <summary>
        /// Der Vorschlag, der optional durch den tatsächliche Wert ersetzt werden kann.
        /// </summary>
        public PlannedTime Planned;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        private SuggestedPlannedTime()
        {
        }

        /// <summary>
        /// Erstellt einen Vorschlag.
        /// </summary>
        /// <param name="planned">Der ursprüngliche Vorschlag.</param>
        /// <returns>Der modifizierbare Vorschlag.</returns>
        public static implicit operator SuggestedPlannedTime( PlannedTime planned )
        {
            // Create
            return new SuggestedPlannedTime { Planned = planned };
        }
    }
}
