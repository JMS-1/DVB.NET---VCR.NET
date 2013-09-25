using System;
using System.Collections.Generic;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Beschreibt eine zugeteilte Aufzeichnung.
    /// </summary>
    public interface IResourceAllocationInformation : IRecordingDefinition
    {
        /// <summary>
        /// Meldet das zu verwendende Gerät.
        /// </summary>
        IScheduleResource Resource { get; }

        /// <summary>
        /// Die geplante Laufzeit.
        /// </summary>
        PlannedTime Time { get; }
    }
}
