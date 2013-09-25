using System;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Beschreibt eine geplante Aufzeichnung.
    /// </summary>
    public interface IRecordingDefinition : IScheduleDefinition
    {
        /// <summary>
        /// Die Quelle, die aufgezeichnet werden soll.
        /// </summary>
        IScheduleSource Source { get; }
    }

    /// <summary>
    /// Beschreibt eine geplante Aufzeichnung.
    /// </summary>
    /// <typeparam name="UserDataType">Die Art der vom Anwender der Schnittstelle zusätzlich bereitgestellten Daten.</typeparam>
    public interface IRecordingDefinition<UserDataType> : IScheduleDefinition<UserDataType>, IRecordingDefinition
    {
    }
}
