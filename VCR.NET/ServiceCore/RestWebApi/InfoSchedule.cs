using System;
using System.Globalization;
using System.Runtime.Serialization;
using JMS.DVBVCR.RecordingService.Persistence;
using JMS.DVBVCR.RecordingService.WebServer;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Meldet Kerninformationen zu einer Aufzeichnung.
    /// </summary>
    [Serializable]
    [DataContract]
    public class InfoSchedule
    {
        /// <summary>
        /// Der optionale Name der Aufzeichnung.
        /// </summary>
        [DataMember( Name = "name" )]
        public string Name { get; set; }

        /// <summary>
        /// Der Startzeitpunkt der Aufzeichnung.
        /// </summary>
        [DataMember( Name = "start" )]
        public string StartTimeISO
        {
            get { return StartTime.ToString( "o" ); }
            set { StartTime = DateTime.Parse( value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind ); }
        }

        /// <summary>
        /// Der Startzeitpunkt der Aufzeichnung.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Die Tage, an denen die Aufzeichnung wiederholt werden soll.
        /// </summary>
        [DataMember( Name = "repeatPattern" )]
        public int RepeatPatternJSON
        {
            get { return RepeatPattern.HasValue ? (int) RepeatPattern.Value : 0; }
            set { RepeatPattern = (value == 0) ? null : (VCRDay?) value; }
        }

        /// <summary>
        /// Die Tage, an denen die Aufzeichnung wiederholt werden soll.
        /// </summary>
        public VCRDay? RepeatPattern { get; set; }

        /// <summary>
        /// Der optionale Name der Quelle, von der aufgezeichnet werden soll.
        /// </summary>
        [DataMember( Name = "sourceName" )]
        public string Source { get; set; }

        /// <summary>
        /// Der eindeutige Name der Aufzeichnung.
        /// </summary>
        [DataMember( Name = "id" )]
        public string WebId { get; set; }

        /// <summary>
        /// Die Dauer der Aufzeichnung.
        /// </summary>
        [DataMember( Name = "duration" )]
        public int Duration { get; set; }

        /// <summary>
        /// Meldet die Daten zu einer Aufzeichnung.
        /// </summary>
        /// <param name="schedule">Die Aufzeichnung.</param>
        /// <param name="job">Der zugehörige Auftrag.</param>
        /// <returns></returns>
        public static InfoSchedule Create( VCRSchedule schedule, VCRJob job )
        {
            // Create
            return
                new InfoSchedule
                {
                    Source = (schedule.Source ?? job.Source).GetUniqueName(),
                    WebId = ServerRuntime.GetUniqueWebId( job, schedule ),
                    StartTime = schedule.FirstStart,
                    RepeatPattern = schedule.Days,
                    Duration = schedule.Duration,
                    Name = schedule.Name,
                };
        }
    }
}
