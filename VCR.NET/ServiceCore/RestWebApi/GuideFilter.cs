using System;
using System.Globalization;
using System.Runtime.Serialization;
using JMS.DVBVCR.RecordingService.ProgramGuide;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Beschreibt einen Filter auf Einträge der Programmzeitschrift.
    /// </summary>
    [Serializable]
    [DataContract]
    public class GuideFilter
    {
        /// <summary>
        /// Der Name des zu verwendenden Geräteprofils.
        /// </summary>
        [DataMember( Name = "device" )]
        public string ProfileName { get; set; }

        /// <summary>
        /// Optional die Quelle.
        /// </summary>
        [DataMember( Name = "station" )]
        public string Source { get; set; }

        /// <summary>
        /// Optional der Startzeitpunkt.
        /// </summary>
        [DataMember( Name = "start" )]
        public string StartISO
        {
            get { return Start.HasValue ? Start.Value.ToString( "o" ) : null; }
            set { Start = string.IsNullOrEmpty( value ) ? default( DateTime? ) : DateTime.Parse( value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind ); }
        }

        /// <summary>
        /// Optional der Startzeitpunkt.
        /// </summary>
        public DateTime? Start { get; set; }

        /// <summary>
        /// Das Suchmuster für den Titel, das erste Zeichen bestimmt den Suchmodus.
        /// </summary>
        [DataMember( Name = "title" )]
        public string TitlePattern { get; set; }

        /// <summary>
        /// Das Suchmuster für den Inhalt, das erste Zeichen bestimmt den Suchmodus.
        /// </summary>
        [DataMember( Name = "content" )]
        public string ContentPattern { get; set; }

        /// <summary>
        /// Die gewünschte Seitengröße.
        /// </summary>
        [DataMember( Name = "size" )]
        public int PageSize { get; set; }

        /// <summary>
        /// Die aktuell gewünschte Seite.
        /// </summary>
        [DataMember( Name = "index" )]
        public int PageIndex;

        /// <summary>
        /// Die Art der Quelle.
        /// </summary>
        [DataMember( Name = "typeFilter" )]
        public GuideSourceFilter SourceType { get; set; }

        /// <summary>
        /// Die Art der Verschlüsselung.
        /// </summary>
        [DataMember( Name = "cryptFilter" )]
        public GuideEncryptionFilter SourceEncryption { get; set; }

        /// <summary>
        /// Erstellt die interne Repräsentation eines Filters.
        /// </summary>
        /// <param name="filter">Die externe Darstellung des Filters.</param>
        /// <returns>Die gewünschte Repräsentation.</returns>
        public static GuideEntryFilter Translate( GuideFilter filter )
        {
            // None
            if (filter == null)
                return null;

            // Lookup source by unique name
            var source = (filter.Source == null) ? null : VCRProfiles.FindSource( filter.ProfileName, filter.Source );

            // Process
            return
                new GuideEntryFilter
                {
                    Source = (source == null) ? null : source.Source,
                    SourceEncryption = filter.SourceEncryption,
                    ContentPattern = filter.ContentPattern,
                    TitlePattern = filter.TitlePattern,
                    ProfileName = filter.ProfileName,
                    SourceType = filter.SourceType,
                    PageIndex = filter.PageIndex,
                    PageSize = filter.PageSize,
                    Start = filter.Start,
                };
        }
    }
}
