using System;
using JMS.DVBVCR.RecordingService.Persistence;


namespace JMS.DVBVCR.RecordingService
{
    partial class VCRServer
    {
        /// <summary>
        /// Die Verwaltung der Aufträge.
        /// </summary>
        internal JobManager JobManager { get; private set; }

        /// <summary>
        /// Ermittelt einen Auftrag.
        /// </summary>
        /// <param name="uniqueIdentifier">Die eindeutige Kennung des Auftrags.</param>
        /// <returns>Der gewünschte Auftrag oder <i>null</i>.</returns>
        public VCRJob FindJob( Guid uniqueIdentifier )
        {
            // Ask job manager
            return JobManager.FindJob( uniqueIdentifier );
        }

        /// <summary>
        /// Aktualisiert einen Auftrag oder legt einen neue an.
        /// </summary>
        /// <param name="job">Der betroffene Auftrag.</param>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der veränderten Aufzeichnung.</param>
        public void UpdateJob( VCRJob job, Guid? scheduleIdentifier )
        {
            // Client is talking to us - do not hibernate after update
            m_PendingHibernation = false;

            // Cleanup
            if (scheduleIdentifier.HasValue)
                foreach (var schedule in job.Schedules)
                    if (schedule.UniqueID.HasValue)
                        if (schedule.UniqueID.Value.Equals( scheduleIdentifier.Value ))
                            schedule.NoStartBefore = null;

            // Add to job manager
            JobManager.Update( job, scheduleIdentifier );

            // Recalculate
            BeginNewPlan();
        }

        /// <summary>
        /// Entfernt einen Auftrag.
        /// </summary>
        /// <param name="job">Der betroffene Auftrag.</param>
        public void DeleteJob( VCRJob job )
        {
            // Client is talking to us - do not hibernate after change
            m_PendingHibernation = false;

            // Remove from job manager
            JobManager.Delete( job );

            // Recalculate
            BeginNewPlan();
        }
    }
}
