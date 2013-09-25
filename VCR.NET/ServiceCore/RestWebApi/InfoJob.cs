using System;
using System.Linq;
using System.Runtime.Serialization;
using JMS.DVBVCR.RecordingService.Persistence;
using JMS.DVBVCR.RecordingService.WebServer;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Beschreibt einen Auftrag.
    /// </summary>
    [Serializable]
    [DataContract]
    public class InfoJob
    {
        /// <summary>
        /// Der Name des Auftrags.
        /// </summary>
        [DataMember( Name = "name" )]
        public string Name { get; set; }

        /// <summary>
        /// Die eindeutige Kennung des Auftrags.
        /// </summary>
        [DataMember( Name = "id" )]
        public string WebId { get; set; }

        /// <summary>
        /// Die einzelnen Aufzeichnungen des Auftrags.
        /// </summary>
        [DataMember( Name = "schedules" )]
        public InfoSchedule[] Schedules { get; set; }

        /// <summary>
        /// Gesetzt, wenn der Auftrag aktiv ist.
        /// </summary>
        [DataMember( Name = "active" )]
        public bool IsActive { get; set; }

        /// <summary>
        /// Der Name des Geräteprofils.
        /// </summary>
        [DataMember( Name = "device" )]
        public string ProfileName { get; set; }

        /// <summary>
        /// Der Name des Quelle.
        /// </summary>
        [DataMember( Name = "source" )]
        public string SourceName { get; set; }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="job">Ein Auftrag.</param>
        /// <param name="active">Gesetzt, wenn es sich um einen aktiven Auftrag handelt.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public static InfoJob Create( VCRJob job, bool active )
        {
            // Report
            return
                new InfoJob
                {
                    Schedules = job.Schedules.Select( schedule => InfoSchedule.Create( schedule, job ) ).OrderBy( schedule => schedule.Name ?? string.Empty, StringComparer.InvariantCultureIgnoreCase ).ToArray(),
                    WebId = ServerRuntime.GetUniqueWebId( job, null ),
                    ProfileName = job.Source.ProfileName,
                    SourceName = job.Source.DisplayName,
                    IsActive = active,
                    Name = job.Name,
                };
        }
    }
}
