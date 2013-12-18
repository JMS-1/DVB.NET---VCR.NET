using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using JMS.DVB;
using JMS.DVB.Algorithms.Scheduler;
using JMS.DVBVCR.RecordingService.Persistence;
using JMS.DVBVCR.RecordingService.Planning;
using JMS.DVBVCR.RecordingService.WebServer;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Beschreibt eine geplante Aktivität.
    /// </summary>
    [DataContract]
    [Serializable]
    public class PlanActivity
    {
        /// <summary>
        /// Vergleicht Planungen nach dem Startzeitpunkt.
        /// </summary>
        public static readonly IComparer<PlanActivity> ByStartComparer = new PlanActivityComparer();

        /// <summary>
        /// Vergleicht Planungen nach dem Startzeitpunkt.
        /// </summary>
        private class PlanActivityComparer : IComparer<PlanActivity>
        {
            /// <summary>
            /// Vergleicht zwei Planungseinträge.
            /// </summary>
            /// <param name="left">Die erste Planung.</param>
            /// <param name="right">Die zweite Planung.</param>
            /// <returns>Die Anordnung der beiden Planungen.</returns>
            public int Compare( PlanActivity left, PlanActivity right )
            {
                // One is not set
                if (left == null)
                    if (right == null)
                        return 0;
                    else
                        return -1;
                else if (right == null)
                    return +1;

                // Compare by start time
                return left.StartTime.CompareTo( right.StartTime );
            }
        }

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
        public string DurationInSeconds
        {
            get { return ((int) Math.Round( Duration.TotalSeconds )).ToString( CultureInfo.InvariantCulture ); }
            set { Duration = TimeSpan.FromSeconds( uint.Parse( value ) ); }
        }

        /// <summary>
        /// Die Laufzeit der Aufzeichnung.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Der Name der Aufzeichnung.
        /// </summary>
        [DataMember( Name = "name" )]
        public string FullName { get; set; }

        /// <summary>
        /// Das Gerät, auf dem die Aktion ausgeführt wird.
        /// </summary>
        [DataMember( Name = "device" )]
        public string Device { get; set; }

        /// <summary>
        /// Der Name des zugehörigen Senders.
        /// </summary>
        [DataMember( Name = "station" )]
        public string Station { get; set; }

        /// <summary>
        /// Gesetzt, wenn alle Sprachen aufgezeichnet werden sollen.
        /// </summary>
        [DataMember( Name = "allAudio" )]
        public bool AllLanguages { get; set; }

        /// <summary>
        /// Gesetzt, wenn auch die <i>AC3</i> Tonspur aufgezeichnet werden soll.
        /// </summary>
        [DataMember( Name = "ac3" )]
        public bool Dolby { get; set; }

        /// <summary>
        /// Gesetzt, wenn auch der Videotext aufgezeichnet werden soll.
        /// </summary>
        [DataMember( Name = "ttx" )]
        public bool VideoText { get; set; }

        /// <summary>
        /// Gesetzt, wenn auch DVB Untertitel aufgezeichnet werden sollen.
        /// </summary>
        [DataMember( Name = "dvbsub" )]
        public bool SubTitles { get; set; }

        /// <summary>
        /// Gesetzt, wenn auch die Programmzeitschrift extrahiert wird.
        /// </summary>
        [DataMember( Name = "epgCurrent" )]
        public bool CurrentProgramGuide { get; set; }

        /// <summary>
        /// Gesetzt, wenn die Aufzeichnung verspätet beginnt.
        /// </summary>
        [DataMember( Name = "late" )]
        public bool IsLate { get; set; }

        /// <summary>
        /// Gesetzt, wenn die Ausführung gar nicht durchgeführt wird.
        /// </summary>
        [DataMember( Name = "lost" )]
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gesetzt, wenn es Einträge in der Programmzeitschrift zu dieser Aufzeichnung gibt.
        /// </summary>
        [DataMember( Name = "epg" )]
        public bool HasGuideEntry { get; set; }

        /// <summary>
        /// Der Name des Gerätes, zu dem ein Eintrag in der Programmzeitschrift existiert.
        /// </summary>
        [DataMember( Name = "epgDevice" )]
        public string GuideEntryDevice { get; set; }

        /// <summary>
        /// Die zugehörige Quelle, sofern bekannt.
        /// </summary>
        [DataMember( Name = "source" )]
        public string Source { get; set; }

        /// <summary>
        /// Die Referenz einer Aufzeichnung, so wie sie
        /// </summary>
        [DataMember( Name = "id" )]
        public string LegacyReference { get; set; }

        /// <summary>
        /// Die zugehörige Ausnahmeregel.
        /// </summary>
        [DataMember( Name = "exception" )]
        public PlanException ExceptionRule { get; set; }

        /// <summary>
        /// Gesetzt, wenn die Endzeit möglicherweise durch die Sommerzeit nicht wie
        /// programmiert ist.
        /// </summary>
        [DataMember( Name = "suspectEndTime" )]
        public bool EndTimeCouldBeWrong { get; set; }

        /// <summary>
        /// Prüft, ob die Aufzeichnung über eine Zeitumstellung erfolgt.
        /// </summary>
        /// <param name="plannedStart">Die ursprünglich gewünschte Startzeit.</param>
        /// <returns>Gesetzt, wenn die Aufzeichnung eventuell durch eine Zeitumstellung verfälscht wird.</returns>
        private bool CheckEndTime( DateTime plannedStart )
        {
            // What we want
            var expectedEnd = (plannedStart + Duration).ToLocalTime().TimeOfDay;

            // What we get
            var realEnd = (StartTime + Duration).ToLocalTime().TimeOfDay;

            // Difference
            var delta = realEnd - expectedEnd;

            // Compare - give it some room for rounding
            return Math.Abs( delta.TotalSeconds ) >= 1;
        }

        /// <summary>
        /// Erstellt einen neuen Eintrag.
        /// </summary>
        /// <param name="schedule">Die zugehörige Beschreibung der geplanten Aktivität.</param>
        /// <param name="context">Die Abbildung auf die Aufträge.</param>
        /// <param name="profiles">Die Verwaltung der Geräteprofile.</param>
        /// <returns>Die angeforderte Repräsentation.</returns>
        public static PlanActivity Create( IScheduleInformation schedule, PlanContext context, ProfileStateCollection profiles )
        {
            // Request context information
            var definition = schedule.Definition;
            var runningInfo = context.GetRunState( definition.UniqueIdentifier );
            var isAllocation = definition is IResourceAllocationInformation;

            // Maybe it's an resource allocation
            if (isAllocation)
                if (runningInfo != null)
                    definition = runningInfo.Schedule.Definition;
                else
                    return null;

            // Create initial entry
            var time = schedule.Time;
            var start = time.Start;
            var end = time.End;
            var activity =
                new PlanActivity
                    {
                        IsHidden = (schedule.Resource == null),
                        IsLate = schedule.StartsLate,
                    };

            // May need some correction
            if (runningInfo != null)
                if (end == runningInfo.Schedule.Time.End)
                {
                    // Only report the allocation itself
                    if (!isAllocation)
                        return null;

                    // Reload the real start and times - just in case someone manipulated
                    start = runningInfo.Schedule.Time.Start;
                    end = runningInfo.RealTime.End;

                    // Never report as late - actually since we have some spin up time most of the time the recording is late
                    activity.IsLate = false;
                }

            // Get the beautified range
            start = PlanCurrent.RoundToSecond( start );
            end = PlanCurrent.RoundToSecond( end );

            // Set times
            activity.Duration = end - start;
            activity.StartTime = start;

            // Set name
            if (definition != null)
                activity.FullName = definition.Name;

            // Set resource
            var resource = schedule.Resource;
            if (resource != null)
                activity.Device = resource.Name;

            // Schedule to process
            VCRSchedule vcrSchedule = null;
            VCRJob vcrJob = null;

            // Analyse definition
            var scheduleDefinition = definition as IScheduleDefinition<VCRSchedule>;
            if (scheduleDefinition != null)
            {
                // Regular plan
                vcrSchedule = scheduleDefinition.Context;
                vcrJob = context.TryFindJob( vcrSchedule );
            }

            // Process if we found one
            if (vcrSchedule != null)
            {
                // See if we have a job
                if (vcrJob != null)
                    activity.LegacyReference = ServerRuntime.GetUniqueWebId( vcrJob, vcrSchedule );

                // Find the source to use - stream selection is always bound to the context of the source
                var streams = vcrSchedule.Streams;
                var source = vcrSchedule.Source;
                if (source == null)
                    if (vcrJob != null)
                    {
                        // Try job
                        source = vcrJob.Source;

                        // Adjust stream flags to use
                        if (source == null)
                            streams = null;
                        else
                            streams = vcrJob.Streams;
                    }

                // Copy station name 
                if (source != null)
                {
                    // Remember
                    activity.Source = SourceIdentifier.ToString( source.Source ).Replace( " ", "" );
                    activity.Station = source.DisplayName;

                    // Load the profile
                    var profile = profiles[activity.GuideEntryDevice = source.ProfileName];
                    if (profile != null)
                        activity.HasGuideEntry = profile.ProgramGuide.HasEntry( source.Source, activity.StartTime, activity.StartTime + activity.Duration );
                }

                // Apply special settings
                activity.EndTimeCouldBeWrong = activity.CheckEndTime( vcrSchedule.FirstStart );
                activity.CurrentProgramGuide = streams.GetUsesProgramGuide();
                activity.AllLanguages = streams.GetUsesAllAudio();
                activity.SubTitles = streams.GetUsesSubtitles();
                activity.VideoText = streams.GetUsesVideotext();
                activity.Dolby = streams.GetUsesDolbyAudio();

                // Check for exception rule on the day
                var exception = vcrSchedule.FindException( time.End );
                if (exception != null)
                    activity.ExceptionRule = PlanException.Create( exception, vcrSchedule );
            }
            else if (definition is ProgramGuideTask)
                activity.Station = VCRJob.ProgramGuideName;
            else if (definition is SourceListTask)
                activity.Station = VCRJob.SourceScanName;

            // Report
            return activity;
        }
    }
}
