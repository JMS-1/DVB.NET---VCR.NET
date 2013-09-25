using System;
using System.Runtime.Serialization;
using JMS.DVBVCR.RecordingService.Persistence;
using JMS.DVBVCR.RecordingService.ProgramGuide;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Die Daten zum Anlegen eines neuen Auftrags mit der ersten Aufzeichnung.
    /// </summary>
    [DataContract]
    [Serializable]
    public class JobScheduleData
    {
        /// <summary>
        /// Der zugehörige Auftrag.
        /// </summary>
        [DataMember( Name = "job" )]
        public EditJob Job { get; set; }

        /// <summary>
        /// Die zugehörige Aufzeichnung.
        /// </summary>
        [DataMember( Name = "schedule" )]
        public EditSchedule Schedule { get; set; }
    }

    /// <summary>
    /// Informationen zu einer Aufzeichnung.
    /// </summary>
    [DataContract]
    [Serializable]
    public class JobScheduleInfo : JobScheduleData
    {
        /// <summary>
        /// Die eindeutige Kennung der Auftrags.
        /// </summary>
        [DataMember( Name = "jobId" )]
        public string JobIdentifier { get; set; }

        /// <summary>
        /// Die eindeutige Kennung der Aufzeichnung.
        /// </summary>
        [DataMember( Name = "scheduleId" )]
        public string ScheduleIdentifier { get; set; }

        /// <summary>
        /// Legt eine neue Information an.
        /// </summary>
        /// <param name="job">Der Auftrag.</param>
        /// <param name="schedule">Die Aufzeichnung.</param>
        /// <param name="guide">Ein Eintrag der Programmzeitschrift.</param>
        /// <param name="profile">Vorgabe für das Geräteprofil.</param>
        /// <returns>Die Information.</returns>
        public static JobScheduleInfo Create( VCRJob job, VCRSchedule schedule, ProgramGuideEntry guide, string profile )
        {
            // Process
            return
                new JobScheduleInfo
                {
                    ScheduleIdentifier = (schedule == null) ? null : schedule.UniqueID.Value.ToString( "N" ),
                    JobIdentifier = (job == null) ? null : job.UniqueID.Value.ToString( "N" ),
                    Schedule = EditSchedule.Create( schedule, job, guide ),
                    Job = EditJob.Create( job, guide, profile ),
                };
        }
    }
}

