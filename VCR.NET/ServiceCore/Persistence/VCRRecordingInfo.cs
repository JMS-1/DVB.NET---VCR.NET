using JMS.DVB;
using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.CardServer;
using JMS.DVBVCR.RecordingService.Planning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;


namespace JMS.DVBVCR.RecordingService.Persistence
{
    /// <summary>
    /// Enthält alle Informationen zur Durchführung einer Aufzeichnung.
    /// </summary>
    [Serializable]
    public class VCRRecordingInfo
    {
        /// <summary>
        /// Hilfsklasse zum Vergleich zweier Aufzeichnungen nach der
        /// Eigenschaft <see cref="VCRRecordingInfo.PhysicalStart"/>.
        /// </summary>
        private class _CompareByPhysicalStart : IComparer<VCRRecordingInfo>
        {
            /// <summary>
            /// Vergleicht zwei Aufzeichnungen nach dem Startdatum.
            /// </summary>
            /// <param name="leftRecording">Erste Aufzeichung.</param>
            /// <param name="rightRecording">Zweite Aufzeichnung.</param>
            /// <returns>Ergebnis von <see cref="DateTime.Compare"/> der
            /// <see cref="VCRRecordingInfo.PhysicalStart"/> Eigenschaften.</returns>
            public int Compare( VCRRecordingInfo leftRecording, VCRRecordingInfo rightRecording )
            {
                // Compare start times - will be called on log entries only
                if (ReferenceEquals( leftRecording, null ))
                    return (ReferenceEquals( rightRecording, null )) ? 0 : -1;
                if (ReferenceEquals( rightRecording, null ))
                    return +1;

                // Load times
                var leftStart = leftRecording.PhysicalStart;
                var rightStart = rightRecording.PhysicalStart;
                if (!leftStart.HasValue)
                    return rightStart.HasValue ? -1 : 0;
                else if (!rightStart.HasValue)
                    return +1;
                else
                    return DateTime.Compare( leftStart.Value, rightStart.Value );
            }
        }

        /// <summary>
        /// Instanz der Vergleichshilfsklasse auf den tatsächlichen Startzeitpunkt.
        /// </summary>
        public static readonly IComparer<VCRRecordingInfo> ComparerByStarted = new _CompareByPhysicalStart();

        /// <summary>
        /// Dateiendung für Protokolleinträge im XML Serialisierungsformat.
        /// </summary>
        public const string FileSuffix = ".l39";

        /// <summary>
        /// Eindeutige Kennung der zugehörigen Aufzeichnung.
        /// </summary>
        public Guid? ScheduleUniqueID { get; set; }

        /// <summary>
        /// Tatsächlicher Startzeitpunkt.
        /// </summary>
        private object m_physicalStart;

        /// <summary>
        /// Tatsächlicher Startzeitpunkt.
        /// </summary>
        public DateTime? PhysicalStart
        {
            get
            {
                // Report in an atomic operation - DateTime is not atomic on 32-Bit systems!
                return (DateTime?) m_physicalStart;
            }
            set
            {
                // Store in one atomic operation using boxing of the DateTime
                m_physicalStart = value;
            }
        }

        /// <summary>
        /// Eindeutige Kennung des zugehörigen Auftrags.
        /// </summary>
        public Guid? JobUniqueID { get; set; }

        /// <summary>
        /// Vom Anwender gewünschter Startzeitpunkt.
        /// </summary>
        public DateTime? StartsAt { get; set; }

        /// <summary>
        /// Vom Anwender gewünschtes Ende der Aufzeichnung und nach der Aufzeichnung das tatsächliche Ende.
        /// </summary>
        private long m_endsAt = DateTime.MinValue.Ticks;

        /// <summary>
        /// Vom Anwender gewünschtes Ende der Aufzeichnung und nach der Aufzeichnung das tatsächliche Ende.
        /// </summary>
        public DateTime EndsAt
        {
            get
            {
                // Report
                return new DateTime( Interlocked.Read( ref m_endsAt ), DateTimeKind.Utc );
            }
            set
            {
                // Save set
                Interlocked.Exchange( ref m_endsAt, value.Ticks );
            }
        }

        /// <summary>
        /// Bisher aufgezeichnete Datenmenge.
        /// </summary>
        public uint TotalSize { get; set; }

        /// <summary>
        /// Dateiname für die aufgezeichneten Daten.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Der Name der Aufzeichnung.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Alle während der Aufzeichnung entstandenen Dateien.
        /// </summary>
        [XmlElement( "File" )]
        public readonly List<FileInformation> RecordingFiles = new List<FileInformation>();

        /// <summary>
        /// Die zugehörige Quelle.
        /// </summary>
        public SourceSelection Source { get; set; }

        /// <summary>
        /// Die aktuelle Auswahl der aufgezeichneten Datenströme.
        /// </summary>
        public StreamSelection Streams { get; set; }

        /// <summary>
        /// Die zugehörige Aufzeichnung.
        /// </summary>
        [XmlIgnore]
        public VCRSchedule RelatedSchedule { get { return m_RelatedSchedule; } set { m_RelatedSchedule = value; } }

        /// <summary>
        /// Die zugehörige Aufzeichnung.
        /// </summary>
        [NonSerialized]
        private VCRSchedule m_RelatedSchedule;

        /// <summary>
        /// Der zugehörige Auftrag.
        /// </summary>
        [XmlIgnore]
        public VCRJob RelatedJob { get { return m_RelatedJob; } set { m_RelatedJob = value; } }

        /// <summary>
        /// Der zugehörige Auftrag.
        /// </summary>
        [NonSerialized]
        private VCRJob m_RelatedJob;

        /// <summary>
        /// Verhindert, dass nach der Ausführung dieses Auftrags der Schlafzustand eingeleitet wird.
        /// </summary>
        [XmlIgnore]
        public bool DisableHibernation { get { return m_DisableHibernation; } set { m_DisableHibernation = value; } }

        /// <summary>
        /// Verhindert, dass nach der Ausführung dieses Auftrags der Schlafzustand eingeleitet wird.
        /// </summary>
        [NonSerialized]
        private bool m_DisableHibernation;

        /// <summary>
        /// Ein eindeutiger Name für den Fall, dass es sich um einen Protokolleintrag handelt.
        /// </summary>
        [XmlIgnore]
        public string LogIdentifier { get; set; }

        /// <summary>
        /// Gesetzt, ob diese Aufzeichnung ignoriert wird soll.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gesetzt, wenn diese Aufzeichnung verspätet beginnt.
        /// </summary>
        public bool StartsLate { get; set; }

        /// <summary>
        /// Meldet die zugehörige Quelle, so wie sie im aktuellen Geräteprofil bekannt ist.
        /// </summary>
        [XmlIgnore]
        public SourceSelection CurrentSource => VCRProfiles.FindSource( Source );

        /// <summary>
        /// Erstellt eine exakte Kopie dieser Aufzeichnungsinformation.
        /// </summary>
        /// <returns>Die exakte Kopie.</returns>
        public VCRRecordingInfo Clone()
        {
            // Create clone
            var clone =
                new VCRRecordingInfo
                {
                    Source = (Source == null) ? null : new SourceSelection { DisplayName = Source.DisplayName, SelectionKey = Source.SelectionKey },
                    Streams = (Streams == null) ? null : Streams.Clone(),
                    DisableHibernation = DisableHibernation,
                    ScheduleUniqueID = ScheduleUniqueID,
                    m_physicalStart = m_physicalStart,
                    JobUniqueID = JobUniqueID,
                    StartsLate = StartsLate,
                    TotalSize = TotalSize,
                    StartsAt = StartsAt,
                    FileName = FileName,
                    IsHidden = IsHidden,
                    m_endsAt = m_endsAt,
                    Name = Name,
                };

            // File data
            clone.RecordingFiles.AddRange( RecordingFiles.Select( info => new FileInformation { Path = info.Path, VideoType = info.VideoType, ScheduleIdentifier = info.ScheduleIdentifier } ) );

            // Report
            return clone;
        }

        /// <summary>
        /// Ermittelt die Ersetzungsmuster für Dateinamen gemäß den Daten dieser Aufzeichnung.
        /// </summary>
        /// <returns>Die konkreten Ersetzungswertes für diesen Auftrag.</returns>
        public Dictionary<string, string> GetReplacementPatterns()
        {
            // Pattern map static parts
            var localNow = DateTime.Now;
            var patterns =
                new Dictionary<string, string>
                {
                    { "%ScheduleIdentifier%", ScheduleUniqueID.GetValueOrDefault().ToString( "N" ).ToUpper() },
                    { "%JobIdentifier%", JobUniqueID.GetValueOrDefault().ToString( "N" ).ToUpper() },
                    { "%End%", EndsAt.ToLocalTime().ToString( "dd-MM-yyyy HH-mm-ss" ) },
                    { "%UniqueIdentifier%", Guid.NewGuid().ToString( "N" ).ToUpper() },
                    { "%SortableStart%", localNow.ToString( "yyyy-MM-dd HH-mm-ss" ) },
                    { "%Start%", localNow.ToString( "dd-MM-yyyy HH-mm-ss" ) },
                    { "%SortableDate%", localNow.ToString( "yyyy-MM-dd" ) },
                    { "%Date%", localNow.ToString( "dd-MM-yyyy" ) },
                    { "%Time%", localNow.ToString( "HH-mm-ss" ) },
                    { "%Year%", localNow.ToString( "yyyy" ) },
                    { "%Minute%", localNow.ToString( "mm" ) },
                    { "%Second%", localNow.ToString( "ss" ) },
                    { "%Month%", localNow.ToString( "MM" ) },
                    { "%Hour%", localNow.ToString( "HH" ) },
                    { "%Day%", localNow.ToString( "dd" ) },
                    { "%Transponder%", string.Empty },
                    { "%StationFull%", string.Empty },
                    { "%AllAudio%", string.Empty },
                    { "%Profile%", string.Empty },
                    { "%Station%", string.Empty },
                    { "%Service%", string.Empty },
                    { "%DVBSub%", string.Empty },
                    { "%Audio%", string.Empty },
                    { "%AC3%", string.Empty },
                    { "%TTX%", string.Empty },
                };

            // Fill it
            if (RelatedSchedule != null)
                patterns["%Schedule%"] = RelatedSchedule.Name;
            if (RelatedJob != null)
                patterns["%Job%"] = RelatedJob.Name;

            // Duration
            var start = PhysicalStart;
            var end = EndsAt;
            if (start.HasValue)
                patterns["%Duration%"] = ((int) (end - start.Value).TotalMinutes).ToString();
            else if (StartsAt.HasValue)
                patterns["%Duration%"] = ((int) (end - StartsAt.Value).TotalMinutes).ToString();

            // Check station
            var source = CurrentSource;
            if (source != null)
            {
                // Attach to the station
                var station = (Station) source.Source;

                // Copy over
                patterns["%Transponder%"] = station.Provider;
                patterns["%StationFull%"] = station.FullName;
                patterns["%Profile%"] = source.ProfileName;
                patterns["%Station%"] = station.Name;
            }
            else if (Source != null)
            {
                // Copy over
                if (!string.IsNullOrEmpty( Source.DisplayName ))
                {
                    // Pseudo-Source
                    patterns["%StationFull%"] = Source.DisplayName;
                    patterns["%Station%"] = Source.DisplayName;
                }
                if (!string.IsNullOrEmpty( Source.ProfileName ))
                    patterns["%Profile%"] = Source.ProfileName;
            }

            // Check streams
            if (Streams != null)
            {
                // Videotext
                if (Streams.Videotext)
                    patterns["%TTX%"] = "TTX";

                // Check MP2
                switch (Streams.MP2Tracks.LanguageMode)
                {
                    case LanguageModes.Selection: if (Streams.MP2Tracks.Languages.Count == 1) patterns["%Audio%"] = Streams.MP2Tracks.Languages[0]; break;
                    case LanguageModes.All: patterns["%AllAudio%"] = "MultiLang"; break;
                    case LanguageModes.Primary: break;
                }

                // Check AC3
                bool usesAC3 = false;
                switch (Streams.AC3Tracks.LanguageMode)
                {
                    case LanguageModes.Selection: usesAC3 = (Streams.AC3Tracks.Languages.Count > 0); break;
                    case LanguageModes.Primary: usesAC3 = true; break;
                    case LanguageModes.All: usesAC3 = true; break;
                }

                // Store
                if (usesAC3)
                    patterns["%AC3%"] = "AC3";

                // Check DVB Subtitles
                bool usesSUB = false;
                switch (Streams.SubTitles.LanguageMode)
                {
                    case LanguageModes.Selection: usesSUB = (Streams.SubTitles.Languages.Count > 0); break;
                    case LanguageModes.Primary: usesSUB = true; break;
                    case LanguageModes.All: usesSUB = true; break;
                }

                // Store
                if (usesSUB)
                    patterns["%DVBSub%"] = "DVBSUB";
            }

            // Report
            return patterns;
        }

        /// <summary>
        /// Befüllt vor allem den Dateinamen mit Vorgabewerten.
        /// </summary>
        internal void LoadDefaults()
        {
            // Construct file name
            var pattern = VCRConfiguration.Current.FileNamePattern;
            var file = FileName;

            // Check for test recording
            if ((RelatedJob != null) && (RelatedSchedule != null))
            {
                // Enter placeholders
                pattern = UpdatePlaceHolders( pattern );
            }
            else if (!string.IsNullOrEmpty( file ))
            {
                // Reconstruct name
                pattern = Path.GetFileNameWithoutExtension( file );

                // Cut off directory
                file = Path.GetDirectoryName( file );
            }
            else
            {
                // Set dummy name
                pattern = $"Test {DateTime.Now:dd-MM-yyyy HH-mm-ss}";
            }

            // Default directory
            if (string.IsNullOrEmpty( file ))
                file = VCRConfiguration.Current.PrimaryTargetDirectory.FullName;

            // Append pattern
            FileName = Path.Combine( file, pattern + ".ts" );

            // Check for valid path - user can try to jump out of the allowed area but we will move back
            if (!VCRConfiguration.Current.IsValidTarget( FileName ))
                FileName = Path.Combine( VCRConfiguration.Current.PrimaryTargetDirectory.FullName, pattern + ".ts" );
        }

        /// <summary>
        /// Setzt die Platzhalter in die Aufzeichnungsdatei ein.
        /// </summary>
        /// <param name="withPatterns">Der ursprüngliche Dateiname mit Platzhaltern.</param>
        /// <returns>Der Dateiname mit den ausgetauschten Platzhaltern.</returns>
        private string UpdatePlaceHolders( string withPatterns ) => GetReplacementPatterns().Aggregate( withPatterns, ( current, rule ) => current.Replace( rule.Key, rule.Value.MakeValid() ) );

        /// <summary>
        /// Prüft, ob diese Aufzeichnung zu einem gerade aufgezeichneten Datenstrom gehört.
        /// </summary>
        /// <param name="stream">Ein beliebiger Datenstrom.</param>
        /// <returns>Gesetzt, wenn diese die Aufzeichnung zum Datenstrom ist.</returns>
        public bool Match( StreamInformation stream )
        {
            // Test all
            if (stream == null)
                return false;
            else if (!Equals( stream.UniqueIdentifier, ScheduleUniqueID.GetValueOrDefault( Guid.Empty ) ))
                return false;
            else if (Source == null)
                return false;
            else
                return Equals( Source.Source, stream.Source );
        }

        /// <summary>
        /// Prüft, ob diese Instanz zu einer bestimmten Aufzeichnung gehört.
        /// </summary>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
        /// <returns>Gesetzt, wenn entweder keine Kennung angeben wurde oder die bezeichnete Aufzeichnung
        /// referenziert wird.</returns>
        public bool MatchesScheduleFilter( Guid? scheduleIdentifier )
        {
            // No filter active
            if (!scheduleIdentifier.HasValue)
                return true;

            // We have no identity
            var schedule = ScheduleUniqueID;
            if (!schedule.HasValue)
                return false;

            // Identity match
            return (scheduleIdentifier.Value == schedule.Value);
        }

        /// <summary>
        /// Erstellt einen neuen Eintrag.
        /// </summary>
        /// <param name="planItem">Die zugehörige Beschreibung der geplanten Aktivität.</param>
        /// <param name="context">Die Abbildung auf die Aufträge.</param>
        /// <returns>Die angeforderte Repräsentation.</returns>
        public static VCRRecordingInfo Create( IScheduleInformation planItem, PlanContext context )
        {
            // Validate
            if (planItem == null)
                throw new ArgumentNullException( nameof( planItem ) );
            if (context == null)
                throw new ArgumentNullException( nameof( context ) );

            // Check type
            var definition = planItem.Definition as IScheduleDefinition<VCRSchedule>;
            if (definition == null)
            {
                // Check for program guide collector
                var guideCollection = planItem.Definition as ProgramGuideTask;
                if (guideCollection != null)
                    return
                        new VCRRecordingInfo
                        {
                            Source = new SourceSelection { ProfileName = planItem.Resource.Name, DisplayName = VCRJob.ProgramGuideName },
                            FileName = Path.Combine( guideCollection.CollectorDirectory.FullName, Guid.NewGuid().ToString( "N" ) + ".epg" ),
                            ScheduleUniqueID = guideCollection.UniqueIdentifier,
                            IsHidden = planItem.Resource == null,
                            StartsLate = planItem.StartsLate,
                            StartsAt = planItem.Time.Start,
                            Name = guideCollection.Name,
                            EndsAt = planItem.Time.End,
                        };

                // Check for source list update
                var sourceUpdater = planItem.Definition as SourceListTask;
                if (sourceUpdater != null)
                    return
                        new VCRRecordingInfo
                        {
                            Source = new SourceSelection { ProfileName = planItem.Resource.Name, DisplayName = VCRJob.SourceScanName },
                            FileName = Path.Combine( sourceUpdater.CollectorDirectory.FullName, Guid.NewGuid().ToString( "N" ) + ".psi" ),
                            ScheduleUniqueID = sourceUpdater.UniqueIdentifier,
                            IsHidden = planItem.Resource == null,
                            StartsLate = planItem.StartsLate,
                            StartsAt = planItem.Time.Start,
                            EndsAt = planItem.Time.End,
                            Name = sourceUpdater.Name,
                        };

                // None
                return null;
            }

            // Attach to the schedule and its job - using the context and the map is the easiest way although there may be better alternatives
            var job = context.TryFindJob( definition.UniqueIdentifier );
            var schedule = definition.Context;

            // Find the source
            var source = schedule.Source ?? job.Source;
            if (source != null)
            {
                // Create a clone
                source = new SourceSelection { DisplayName = source.DisplayName, SelectionKey = source.SelectionKey };

                // Update the name of the profile
                var resource = planItem.Resource;
                if (resource != null)
                    source.ProfileName = resource.Name;
            }

            // Create the description of this recording
            var recording =
                new VCRRecordingInfo
                {
                    Streams = (schedule.Source == null) ? job.Streams : schedule.Streams,
                    ScheduleUniqueID = schedule.UniqueID,
                    IsHidden = planItem.Resource == null,
                    StartsLate = planItem.StartsLate,
                    StartsAt = planItem.Time.Start,
                    EndsAt = planItem.Time.End,
                    JobUniqueID = job.UniqueID,
                    RelatedSchedule = schedule,
                    FileName = job.Directory,
                    Name = definition.Name,
                    RelatedJob = job,
                    Source = source,
                };

            // May want to adjust start time if job is active
            var runningInfo = context.GetRunState( definition.UniqueIdentifier );
            if (runningInfo != null)
                if (runningInfo.Schedule.Time.End == recording.EndsAt)
                {
                    // Assume we never start late - we are running
                    recording.StartsLate = false;

                    // If we started prior to this plan report the time we really started
                    if (planItem.Time.Start > runningInfo.Schedule.Time.Start)
                        recording.StartsAt = runningInfo.Schedule.Time.Start;
                }

            // Finish
            recording.LoadDefaults();

            // Report
            return recording;
        }

        /// <summary>
        /// Wandelt eine Aufzeichnung in die Beschreibung eines Empfangsdatenstroms.
        /// </summary>
        /// <returns>Die passende Beschreibung.</returns>
        public ReceiveInformation ToReceiveInformation()
        {
            // Attach to the station and the profile
            var source = Source;
            var profile = VCRProfiles.FindProfile( source.ProfileName );

            // May want to disable the program guide
            var streams = Streams.Clone();
            var disableProgramGuide = profile.ScanConfiguration.GetFilter( source.Source ).DisableProgramGuide;
            if (disableProgramGuide)
                streams.ProgramGuide = false;

            // Create protocol structure
            return
                new ReceiveInformation
                {
                    HDTVFileBufferSize = VCRConfiguration.Current.HighDefinitionVideoBufferSize,
                    SDTVFileBufferSize = VCRConfiguration.Current.StandardVideoBufferSize,
                    AudioFileBufferSize = VCRConfiguration.Current.AudioBufferSize,
                    UniqueIdentifier = ScheduleUniqueID.Value,
                    SelectionKey = source.SelectionKey,
                    RecordingPath = FileName,
                    Streams = streams,
                };
        }

    }
}
