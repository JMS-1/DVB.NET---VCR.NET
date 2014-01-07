using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using JMS.DVB;
using JMS.DVB.Algorithms.Scheduler;
using JMS.DVBVCR.RecordingService.Persistence;
using JMS.DVBVCR.RecordingService.Planning;
using JMS.DVBVCR.RecordingService.Status;
using JMS.DVBVCR.RecordingService.WebServer;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Beschreibt eine Aufzeichnung, die entweder aktiv ist oder als mächstes auf einem gerade unbenutzen
    /// Gerät ausgeführt wird.
    /// </summary>
    [Serializable]
    [DataContract]
    public class PlanCurrentMobile
    {
        /// <summary>
        /// Der Name des Gerätes.
        /// </summary>
        [DataMember( Name = "device" )]
        public string ProfileName { get; set; }

        /// <summary>
        /// Der Name der Aufzeichnung.
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
        /// Die Laufzeit der Aufzeichnung.
        /// </summary>
        [DataMember( Name = "duration" )]
        public int DurationInSeconds
        {
            get { return (int) Math.Round( Duration.TotalSeconds ); }
            set { Duration = TimeSpan.FromSeconds( value ); }
        }

        /// <summary>
        /// Die Laufzeit der Aufzeichnung.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gesetzt, wenn es Einträge in der Programmzeitschrift zu dieser Aufzeichnung gibt.
        /// </summary>
        [DataMember( Name = "epg" )]
        public bool HasGuideEntry { get; set; }

        /// <summary>
        /// Die zugehörige Quelle, sofern bekannt.
        /// </summary>
        [DataMember( Name = "sourceName" )]
        public string SourceName { get; set; }

        /// <summary>
        /// Die zugehörige Quelle, sofern bekannt.
        /// </summary>
        [DataMember( Name = "source" )]
        public string Source { get; set; }

        /// <summary>
        /// Erstellt eine reduzierte Version der Information zu einer Aktivität.
        /// </summary>
        /// <param name="full">Die volle Information.</param>
        /// <returns>Die reduzierte Information.</returns>
        public static PlanCurrentMobile Create( PlanCurrent full )
        {
            // Cut down
            return
                new PlanCurrentMobile
                {
                    HasGuideEntry = full.HasGuideEntry,
                    ProfileName = full.ProfileName,
                    SourceName = full.SourceName,
                    StartTime = full.StartTime,
                    Duration = full.Duration,
                    Source = full.Source,
                    Name = full.Name,
                };
        }
    }

    /// <summary>
    /// Beschreibt eine Aufzeichnung, die entweder aktiv ist oder als mächstes auf einem gerade unbenutzen
    /// Gerät ausgeführt wird.
    /// </summary>
    [Serializable]
    [DataContract]
    public class PlanCurrent : PlanCurrentMobile
    {
        /// <summary>
        /// Eine leere Liste von Dateien.
        /// </summary>
        private static readonly string[] _NoFiles = { };

        /// <summary>
        /// Die eindeutige Kennung der Aufzeichnung.
        /// </summary>
        [DataMember( Name = "id" )]
        public string Identifier { get; set; }

        /// <summary>
        /// Gesetzt, wenn die Aufzeichung gerade ausgeführt wird.
        /// </summary>
        [DataMember( Name = "referenceId" )]
        public string PlanIdentifier { get; set; }

        /// <summary>
        /// Gesetzt, wenn die Aufzeichnung verspätet beginnt.
        /// </summary>
        [DataMember( Name = "late" )]
        public bool IsLate { get; set; }

        /// <summary>
        /// Gesetzt, wenn es sich hier um einen Platzhalter für ein Gerät handelt, dass nicht in Benutzung ist.
        /// </summary>
        [DataMember( Name = "isIdle" )]
        public bool IsIdle { get; set; }

        /// <summary>
        /// Eine Beschreibung der Größe, Anzahl etc.
        /// </summary>
        [DataMember( Name = "size" )]
        public string SizeHint { get; set; }

        /// <summary>
        /// Die zugehörige Quelle.
        /// </summary>
        [NonSerialized]
        private SourceSelection m_source;

        /// <summary>
        /// Die laufende Nummer des Datenstroms, die zur Anzeige benötigt wird.
        /// </summary>
        [DataMember( Name = "streamIndex" )]
        public int Index { get; set; }

        /// <summary>
        /// Die Netzwerkadresse, an die gerade die Aufzeichnungsdaten versendet werden.
        /// </summary>
        [DataMember( Name = "streamTarget" )]
        public string StreamTarget { get; set; }

        /// <summary>
        /// Die verbleibende Restzeit der Aufzeichnung.
        /// </summary>
        [DataMember( Name = "remainingMinutes" )]
        public uint RemainingTimeInMinutes
        {
            get { return (PlanIdentifier == null) ? 0 : checked( (uint) Math.Max( 0, Math.Round( (StartTime + Duration - DateTime.UtcNow).TotalMinutes ) ) ); }
            set { }
        }

        /// <summary>
        /// Alle zu dieser Aktivität erstellten Dateien.
        /// </summary>
        [DataMember( Name = "files" )]
        public string[] Files { get; set; }

        /// <summary>
        /// Rundet einen Datumswert auf die volle Sekunde.
        /// </summary>
        /// <param name="original">Die originale Zeit.</param>
        /// <returns>Die gerundete Zeit.</returns>
        public static DateTime RoundToSecond( DateTime original )
        {
            // Helper
            const long HalfASecond = 5000000;
            const long FullSecond = 2 * HalfASecond;

            // Load as 100ns units
            var ticks = original.Ticks;

            // Get the difference to the previous second
            var mod = ticks % FullSecond;

            // Get the truncated time
            ticks -= mod;

            // Round up
            if (mod >= HalfASecond)
                ticks += FullSecond;

            // Done
            return new DateTime( ticks, DateTimeKind.Utc );
        }

        /// <summary>
        /// Erstellt eine neue Liste von Beschreibungen für eine aktive Aufzeichnung.
        /// </summary>
        /// <param name="active">Die Daten zur aktiven Aufzeichnung.</param>
        /// <param name="server">Der zugehörige Dienst.</param>
        /// <returns>Die gewünschten Beschreibungen.</returns>
        public static PlanCurrent[] Create( FullInfo active, VCRServer server )
        {
            // Validate
            if (active == null)
                throw new ArgumentNullException( "active" );

            // Validate
            var recording = active.Recording;
            if (recording == null)
                throw new ArgumentNullException( "recording" );

            // Multiple recordings
            var streams = active.Streams;
            if (streams != null)
                if (streams.Count > 0)
                    return streams.SelectMany( ( stream, index ) => Create( active, stream, index, server ) ).ToArray();

            // Single recording - typically a task
            var start = RoundToSecond( active.Recording.PhysicalStart.GetValueOrDefault( DateTime.UtcNow ) );
            var end = RoundToSecond( recording.EndsAt );
            var source = recording.Source;
            var sourceName = source.DisplayName;

            // Create
            var current =
                new PlanCurrent
                {
                    PlanIdentifier = recording.ScheduleUniqueID.Value.ToString( "N" ),
                    ProfileName = source.ProfileName,
                    Duration = end - start,
                    Name = recording.Name,
                    m_source = source,
                    StartTime = start,
                    Files = _NoFiles,
                    IsLate = false,
                    Index = -1,
                };

            // Finish            
            if (VCRJob.ProgramGuideName.Equals( sourceName ))
                current.SizeHint = string.Format( "{0:N0} Einträge", recording.TotalSize );
            else if (VCRJob.SourceScanName.Equals( sourceName ))
                current.SizeHint = string.Format( "{0:N0} Quellen", recording.TotalSize );
            else if (VCRJob.ZappingName.Equals( sourceName ))
                current.SizeHint = GetSizeHint( recording.TotalSize );
            else
                current.Complete( server );

            // Report
            return new[] { current };
        }

        /// <summary>
        /// Schließt die Konfiguration einer Beschreibung ab.
        /// </summary>
        /// <param name="server">Der zugehörige Dienst.</param>
        private void Complete( VCRServer server )
        {
            // No source
            if (m_source == null)
                return;

            // At least we have this
            Source = SourceIdentifier.ToString( m_source.Source ).Replace( " ", "" );

            // Check profile - should normally be available
            var profile = server.Profiles[ProfileName];
            if (profile == null)
                return;

            // Load the profile
            HasGuideEntry = profile.ProgramGuide.HasEntry( m_source.Source, StartTime, StartTime + Duration );
            SourceName = m_source.GetUniqueName();
        }

        /// <summary>
        /// Ermittelt die Größenangabe.
        /// </summary>
        /// <param name="totalSize">Die Anzahl von bisher übertragenen Kilobytes.</param>
        /// <returns>Die Größe in Textform.</returns>
        public static string GetSizeHint( decimal totalSize )
        {
            // Check mode
            if (totalSize < 10000m)
                return string.Format( "{0:N0} kBytes", totalSize );
            else
                return string.Format( "{0:N0} MBytes", Math.Round( totalSize / 1024m ) );
        }

        /// <summary>
        /// Erstellt eine Beschreibung zu einer einzelnen Aufzeichnung auf einem Gerät.
        /// </summary>
        /// <param name="active">Beschreibt die gesamte Aufzeichnung.</param>
        /// <param name="stream">Die zu verwendende Teilaufzeichnung.</param>
        /// <param name="streamIndex">Die laufende Nummer dieses Datenstroms.</param>
        /// <param name="server">Der zugehörige Dienst.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        private static IEnumerable<PlanCurrent> Create( FullInfo active, StreamInfo stream, int streamIndex, VCRServer server )
        {
            // Static data
            var recording = active.Recording;
            var profileName = recording.Source.ProfileName;
            var sizeHint = GetSizeHint( recording.TotalSize ) + " (Gerät)";

            // Process all - beginning with VCR.NET 4.1 there is only one schedule per stream
            foreach (var scheduleInfo in stream.Schedules)
            {
                // Try to locate the context
                var job = string.IsNullOrEmpty( scheduleInfo.JobUniqueID ) ? null : server.JobManager[new Guid( scheduleInfo.JobUniqueID )];
                var schedule = ((job == null) || string.IsNullOrEmpty( scheduleInfo.ScheduleUniqueID )) ? null : job[new Guid( scheduleInfo.ScheduleUniqueID )];

                // Create
                var start = RoundToSecond( scheduleInfo.StartsAt );
                var end = RoundToSecond( scheduleInfo.EndsAt );
                var current =
                    new PlanCurrent
                    {
                        Identifier = (schedule == null) ? null : ServerRuntime.GetUniqueWebId( job, schedule ),
                        PlanIdentifier = scheduleInfo.ScheduleUniqueID,
                        Files = scheduleInfo.Files ?? _NoFiles,
                        StreamTarget = stream.StreamsTo,
                        m_source = scheduleInfo.Source,
                        ProfileName = profileName,
                        Name = scheduleInfo.Name,
                        Duration = end - start,
                        Index = streamIndex,
                        SizeHint = sizeHint,
                        StartTime = start,
                    };

                // Finish
                current.Complete( server );

                // Report
                yield return current;
            }
        }

        /// <summary>
        /// Erstellt eine neue Beschreibung aus dem Aufzeichnungsplan.
        /// </summary>
        /// <param name="plan">Die Planung der Aufzeichnung.</param>
        /// <param name="context">Die aktuelle Analyseumgebung.</param>
        /// <param name="server">Der zugehörige Dienst.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public static PlanCurrent Create( IScheduleInformation plan, PlanContext context, VCRServer server )
        {
            // Attach to the definition
            var definition = (IScheduleDefinition<VCRSchedule>) plan.Definition;
            var job = context.TryFindJob( definition.UniqueIdentifier );
            var schedule = (job == null) ? null : job[definition.UniqueIdentifier];
            var source = (schedule == null) ? null : (schedule.Source ?? job.Source);

            // Create
            var planned =
                new PlanCurrent
                {
                    Identifier = (schedule == null) ? null : ServerRuntime.GetUniqueWebId( job, schedule ),
                    ProfileName = plan.Resource.Name,
                    Duration = plan.Time.Duration,
                    StartTime = plan.Time.Start,
                    IsLate = plan.StartsLate,
                    SizeHint = string.Empty,
                    Name = definition.Name,
                    m_source = source,
                    Files = _NoFiles,
                    Index = -1,
                };

            // Finish
            planned.Complete( server );

            // Report
            return planned;
        }

        /// <summary>
        /// Erstellt einen Eintrag für ein Geräteprofil, das nicht verwendet wird.
        /// </summary>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <returns>Die zugehörige Beschreibung.</returns>
        public static PlanCurrent Create( string profileName )
        {
            // Create
            return new PlanCurrent { ProfileName = profileName, IsIdle = true };
        }
    }
}
