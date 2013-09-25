using System;
using System.IO;
using System.Xml.Serialization;
using JMS.DVB;


namespace JMS.DVBVCR.RecordingService.Persistence
{
    /// <summary>
    /// Beschreibt eine Aufzeichnungsdatei.
    /// </summary>
    [Serializable]
    public class FileInformation
    {
        /// <summary>
        /// Der volle Name der Datei.
        /// </summary>
        [XmlText]
        public string Path { get; set; }

        /// <summary>
        /// Das Format des Bildsignals.
        /// </summary>
        [XmlAttribute( "video" )]
        public VideoTypes VideoType { get; set; }

        /// <summary>
        /// Die eindeutige Kennung der zugehörigen Aufzeichnung.
        /// </summary>
        [XmlAttribute( "schedule" )]
        public string ScheduleIdentifier { get; set; }

        /// <summary>
        /// Erzeugt eine Beschreibung aus der DVB.NET Repräsentation.
        /// </summary>
        /// <param name="file">Die DVB.NET Repräsentation.</param>
        /// <param name="scheduleIdentifier">Die zugehörige Aufzeichnung.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Information angegeben.</exception>
        public static FileInformation Create( FileStreamInformation file, Guid scheduleIdentifier )
        {
            // Validate
            if (file == null)
                throw new ArgumentNullException( "file" );
            else
                return new FileInformation { VideoType = file.VideoType, Path = file.FilePath, ScheduleIdentifier = scheduleIdentifier.ToString( "N" ).ToLower() };
        }

        /// <summary>
        /// Ermittelt einen Schlüssel zu dieser Beschreibung.
        /// </summary>
        /// <returns>Der gewünschte Schlüssel.</returns>
        public override int GetHashCode()
        {
            // Core
            int hash = string.IsNullOrEmpty( Path ) ? 0 : Path.ToLowerInvariant().GetHashCode();

            // Merge
            return (hash * 311) ^ VideoType.GetHashCode();
        }

        /// <summary>
        /// Vergleicht zwei Beschreibungen.
        /// </summary>
        /// <param name="obj">Eine andere Beschreibung.</param>
        /// <returns>Gesetzt, wenn die Beschreibungen völlig identisch sind.</returns>
        public override bool Equals( object obj )
        {
            // Check type
            var other = obj as FileInformation;
            if (ReferenceEquals( other, null ))
                return false;
            if (ReferenceEquals( other, this ))
                return true;

            // Check video type
            if (VideoType != other.VideoType)
                return false;

            // Check strings for null
            if (string.IsNullOrEmpty( Path ) != string.IsNullOrEmpty( other.Path ))
                return false;

            // Check strings
            if (string.IsNullOrEmpty( Path ))
                return true;
            else
                return string.Equals( Path.ToLowerInvariant(), other.Path.ToLowerInvariant() );
        }

        /// <summary>
        /// Meldet die Größe der zugehörigen Datei.
        /// </summary>
        [XmlIgnore]
        public long? FileSize
        {
            get
            {
                // Be safe
                try
                {
                    // Ask library
                    return new FileInfo( Path ).Length;
                }
                catch
                {
                    // Ignore any error
                    return null;
                }
            }
        }
    }
}
