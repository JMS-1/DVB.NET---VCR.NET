using System;
using System.Collections.Generic;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Beschreibt eine geplante Aktion auf einem Gerät.
    /// </summary>
    public interface IScheduleDefinition
    {
        /// <summary>
        /// Eine eindeutige Identifikation der Aktion.
        /// </summary>
        Guid UniqueIdentifier { get; }

        /// <summary>
        /// Ein Anzeigename zur Identifikation der Aktion.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Optional eine Liste von Geräten, die für eine Aufzeichnung erlaubt sind.
        /// </summary>
        IScheduleResource[] Resources { get; }

        /// <summary>
        /// Meldet alle geplanten Aufzeichnungszeiten.
        /// </summary>
        /// <param name="minTime">Aufzeichnunge, die vor diesem Zeitpunkt enden, brauchen nicht in
        /// der Auflistung zu erscheinen. Es handelt sich um eine optionale Optimierung.</param>
        /// <returns>Eine Liste aller geplanten Zeiten.</returns>
        IEnumerable<SuggestedPlannedTime> GetTimes( DateTime minTime );
    }

    /// <summary>
    /// Beschreibt eine geplante Aktion auf einem Gerät.
    /// </summary>
    /// <typeparam name="UserDataType">Die Art der vom Anwender der Schnittstelle zusätzlich bereitgestellten Informationen.</typeparam>
    public interface IScheduleDefinition<UserDataType> : IScheduleDefinition
    {
        /// <summary>
        /// Vom Anwender zusätzlich bereitgestellte Daten.
        /// </summary>
        UserDataType Context { get; }
    }
}
