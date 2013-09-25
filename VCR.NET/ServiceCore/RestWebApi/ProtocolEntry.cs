using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using JMS.DVBVCR.RecordingService.Persistence;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Beschreibt einen einzelnen Protokolleintrag.
    /// </summary>
    [DataContract]
    [Serializable]
    public class ProtocolEntry
    {
        /// <summary>
        /// Der Startzeitpunkt der Gerätenutzung.
        /// </summary>
        [DataMember( Name = "start" )]
        public string StartTimeISO
        {
            get { return StartTime.HasValue ? StartTime.Value.ToString( "o" ) : null; }
            set { StartTime = string.IsNullOrEmpty( value ) ? default( DateTime? ) : DateTime.Parse( value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind ); }
        }

        /// <summary>
        /// Der Startzeitpunkt der Gerätenutzung.
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Der Endzeitpunkt der Gerätenutzung.
        /// </summary>
        [DataMember( Name = "end" )]
        public string EndTimeISO
        {
            get { return EndTime.ToString( "o" ); }
            set { EndTime = DateTime.Parse( value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind ); }
        }

        /// <summary>
        /// Der Endzeitpunkt der Gerätenutzung.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Der Name der ersten Quelle.
        /// </summary>
        [DataMember( Name = "firstSourceName" )]
        public string Source { get; set; }

        /// <summary>
        /// Die Liste aller Aufzeichnungsdateien.
        /// </summary>
        [DataMember( Name = "files" )]
        public string[] Files { get; set; }

        /// <summary>
        /// Ein Hinweis auf die Größe der Aufzeichnungen.
        /// </summary>
        [DataMember( Name = "size" )]
        public string SizeHint { get; set; }

        /// <summary>
        /// Der Name der primären Aufzeichnungsdatei.
        /// </summary>
        [DataMember( Name = "primaryFile" )]
        public string PrimaryFile { get; set; }

        /// <summary>
        /// Kovertiert einen Protokolleintrag in ein für den Client nützliches Format.
        /// </summary>
        /// <param name="entry">Der originale Eintrag.</param>
        /// <returns>Der zugehörige Protokolleintrag.</returns>
        public static ProtocolEntry Create( VCRRecordingInfo entry )
        {
            // Single recording - typically a task
            var source = entry.Source;
            var sourceName = source.DisplayName;

            // Create
            var protocol =
                new ProtocolEntry
                {
                    PrimaryFile = string.IsNullOrEmpty( entry.FileName ) ? null : Path.GetFileName( entry.FileName ),
                    Files = entry.RecordingFiles.Select( file => file.Path ).Where( File.Exists ).ToArray(),
                    Source = entry.Source.DisplayName,
                    StartTime = entry.PhysicalStart,
                    EndTime = entry.EndsAt,
                };

            // Finish            
            if (VCRJob.ProgramGuideName.Equals( sourceName ))
                protocol.SizeHint = string.Format( "{0:N0} Einträge", entry.TotalSize );
            else if (VCRJob.SourceScanName.Equals( sourceName ))
                protocol.SizeHint = string.Format( "{0:N0} Quellen", entry.TotalSize );
            else
                protocol.SizeHint = PlanCurrent.GetSizeHint( entry.TotalSize );

            // Report
            return protocol;
        }
    }
}
