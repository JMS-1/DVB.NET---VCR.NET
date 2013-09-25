using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using JMS.DVB;
using JMS.DVBVCR.RecordingService.Persistence;
using JMS.DVBVCR.RecordingService.ProgramGuide;
using JMS.DVBVCR.RecordingService.WebServer;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Beschreibt eine einzelne Aufzeichnung eines Auftrags.
    /// </summary>
    [DataContract]
    [Serializable]
    public class EditSchedule
    {
        /// <summary>
        /// Der optionale Name der Aufzeichnung.
        /// </summary>
        [DataMember( Name = "name" )]
        public string Name { get; set; }

        /// <summary>
        /// Der optionale Name der Quelle, von der aufgezeichnet werden soll.
        /// </summary>
        [DataMember( Name = "sourceName" )]
        public string Source { get; set; }

        /// <summary>
        /// Falls <see cref="Source"/> definiert ist wird diese Eigenschaft gesetzt
        /// um alle Tonspuren aufzuzeichnen.
        /// </summary>
        [DataMember( Name = "allLanguages" )]
        public bool AllLanguages { get; set; }

        /// <summary>
        /// Falls <see cref="Source"/> definiert ist wird diese Eigenschaft gesetzt
        /// um auch die <i>Dolby Digital</i> Tonspur aufzuzeichnen.
        /// </summary>
        [DataMember( Name = "includeDolby" )]
        public bool DolbyDigital { get; set; }

        /// <summary>
        /// Falls <see cref="Source"/> definiert ist wird diese Eigenschaft gesetzt
        /// um auch die <i>Dolby Digital</i> Tonspur aufzuzeichnen.
        /// </summary>
        [DataMember( Name = "withVideotext" )]
        public bool Videotext { get; set; }

        /// <summary>
        /// Falls <see cref="Source"/> definiert ist wird diese Eigenschaft gesetzt
        /// um auch alle DVB Untertitelspuren aufzuzeichnen.
        /// </summary>
        [DataMember( Name = "withSubtitles" )]
        public bool DVBSubtitles { get; set; }

        /// <summary>
        /// Der Zeitpunkt, an dem die erste Aufzeichnung stattfinden soll.
        /// </summary>
        [DataMember( Name = "firstStart" )]
        public string FirstStartISO
        {
            get { return FirstStart.ToString( "o" ); }
            set { FirstStart = DateTime.Parse( value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind ); }
        }

        /// <summary>
        /// Der Zeitpunkt, an dem die erste Aufzeichnung stattfinden soll.
        /// </summary>
        public DateTime FirstStart { get; set; }

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
        /// Falls <see cref="RepeatPattern"/> gesetzt ist der Tag der letzten Aufzeichnung.
        /// </summary>
        [DataMember( Name = "lastDay" )]
        public string LastDayISO
        {
            get { return RepeatPattern.HasValue ? LastDay.Date.ToString( "o" ) : null; }
            set { LastDay = string.IsNullOrEmpty( value ) ? DateTime.MinValue : DateTime.Parse( value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind ).Date; }
        }

        /// <summary>
        /// Falls <see cref="RepeatPattern"/> gesetzt ist der Tag der letzten Aufzeichnung.
        /// </summary>
        public DateTime LastDay { get; set; }

        /// <summary>
        /// Die Dauer der Aufzeichnung.
        /// </summary>
        [DataMember( Name = "duration" )]
        public int Duration { get; set; }

        /// <summary>
        /// Alle Ausnahmeregelungen, die aktiv sind.
        /// </summary>
        [DataMember( Name = "exceptions" )]
        public PlanException[] Exceptions { get; set; }

        /// <summary>
        /// Erstellt einen neue Aufzeichnung.
        /// </summary>
        public EditSchedule()
        {
            // Finish
            Exceptions = new PlanException[0];
        }

        /// <summary>
        /// Erstellt eine Beschreibung zu dieser Aufzeichnung.
        /// </summary>
        /// <param name="schedule">Eine Aufzeichnung.</param>
        /// <param name="job">Der bereits vorhandene Auftrag.</param>
        /// <param name="guide">Ein Eintrag aus der Programmzeitschrift.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public static EditSchedule Create( VCRSchedule schedule, VCRJob job, ProgramGuideEntry guide )
        {
            // None
            if (schedule == null)
            {
                // No hope
                if (guide == null)
                    return null;

                // Calculate
                var start = guide.StartTime - TimeSpan.FromMinutes( UserProfileSettings.EPGPreTime );
                var duration = checked( (int) (UserProfileSettings.EPGPreTime + (guide.Duration / 60) + UserProfileSettings.EPGPostTime) );

                // Partial - we have a brand new job which is pre-initialized with the source
                if (job == null)
                    return new EditSchedule { FirstStart = start, Duration = duration };

                // Full monty - we have to overwrite the jobs settings since we are not the first schedule
                return
                    new EditSchedule
                    {
                        Source = ServerRuntime.VCRServer.GetUniqueName( new SourceSelection { ProfileName = job.Source.ProfileName, Source = guide.Source } ),
                        DVBSubtitles = UserProfileSettings.UseSubTitles,
                        DolbyDigital = UserProfileSettings.UseAC3,
                        AllLanguages = UserProfileSettings.UseMP2,
                        Videotext = UserProfileSettings.UseTTX,
                        Name = guide.Name.MakeValid(),
                        Duration = duration,
                        FirstStart = start,
                    };
            }

            // Consolidate exceptions
            schedule.CleanupExceptions();

            // Optionen ermitteln
            var streams = schedule.Streams;
            var sourceName = ServerRuntime.VCRServer.GetUniqueName( schedule.Source );

            // Create
            return
                new EditSchedule
                {
                    Exceptions = schedule.Exceptions.Select( exception => PlanException.Create( exception, schedule ) ).ToArray(),
                    LastDay = schedule.LastDay.GetValueOrDefault( VCRSchedule.MaxMovableDay ),
                    DolbyDigital = streams.GetUsesDolbyAudio(),
                    DVBSubtitles = streams.GetUsesSubtitles(),
                    AllLanguages = streams.GetUsesAllAudio(),
                    Videotext = streams.GetUsesVideotext(),
                    FirstStart = schedule.FirstStart,
                    RepeatPattern = schedule.Days,
                    Duration = schedule.Duration,
                    Name = schedule.Name,
                    Source = sourceName,
                };
        }

        /// <summary>
        /// Erstellt die Beschreibung der Aufzeichnung für die persistente Ablage.
        /// </summary>
        /// <param name="job">Der zugehörige Auftrag.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public VCRSchedule CreateSchedule( VCRJob job )
        {
            // Forward
            return CreateSchedule( Guid.NewGuid(), job );
        }

        /// <summary>
        /// Erstellt die Beschreibung der Aufzeichnung für die persistente Ablage.
        /// </summary>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
        /// <param name="job">Der zugehörige Auftrag.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public VCRSchedule CreateSchedule( Guid scheduleIdentifier, VCRJob job )
        {
            // Create
            var schedule =
                new VCRSchedule
                {
                    UniqueID = scheduleIdentifier,
                    FirstStart = FirstStart,
                    Days = RepeatPattern,
                    Duration = Duration,
                    LastDay = LastDay,
                    Name = Name,
                };

            // See if we have a source
            var sourceName = Source;
            if (string.IsNullOrEmpty( sourceName ))
                return schedule;

            // See if there is a profile
            var jobSource = job.Source;
            if (jobSource == null)
                return schedule;

            // Locate the source
            schedule.Source = ServerRuntime.VCRServer.FindSource( jobSource.ProfileName, sourceName );
            if (schedule.Source == null)
                return schedule;

            // Configure streams
            schedule.Streams = new StreamSelection();

            // Set all - oder of audio settings is relevant, dolby MUST come last
            schedule.Streams.SetUsesAllAudio( AllLanguages );
            schedule.Streams.SetUsesDolbyAudio( DolbyDigital );
            schedule.Streams.SetUsesSubtitles( DVBSubtitles );
            schedule.Streams.SetUsesVideotext( Videotext );
            schedule.Streams.ProgramGuide = true;

            // Report
            return schedule;
        }
    }
}
