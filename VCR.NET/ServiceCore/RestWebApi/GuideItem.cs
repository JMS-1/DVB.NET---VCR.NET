using System;
using System.Globalization;
using System.Runtime.Serialization;
using JMS.DVB;
using JMS.DVBVCR.RecordingService.ProgramGuide;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Beschreibt einen einzelnen Eintrag aus der Programmzeitschrift.
    /// </summary>
    [DataContract]
    [Serializable]
    public class GuideItem
    {
        /// <summary>
        /// Der Startzeitpunkt der Sendung.
        /// </summary>
        [DataMember( Name = "start" )]
        public string StartTimeISO
        {
            get { return StartTime.ToString( "o" ); }
            set { StartTime = DateTime.Parse( value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind ); }
        }

        /// <summary>
        /// Die Dauer der Sendung in Sekunden.
        /// </summary>
        [DataMember( Name = "duration" )]
        public int DurationInSeconds
        {
            get { return (int)Math.Round( Duration.TotalSeconds ); }
            set { Duration = TimeSpan.FromSeconds( value ); }
        }

        /// <summary>
        /// Der Name der Sendung.
        /// </summary>
        [DataMember( Name = "name" )]
        public string Name { get; set; }

        /// <summary>
        /// Die Sprache der Sendung.
        /// </summary>
        [DataMember( Name = "language" )]
        public string Language { get; set; }

        /// <summary>
        /// Der Sender, auf dem die Sendung empfangen wird.
        /// </summary>
        [DataMember( Name = "station" )]
        public string Station { get; set; }

        /// <summary>
        /// Der Startzeitpunkt der Sendung.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Die Dauer der Sendung.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Die Liste der Freigaben.
        /// </summary>
        [DataMember( Name = "ratings" )]
        public string[] Ratings { get; set; }

        /// <summary>
        /// Die Liste der Kategorien.
        /// </summary>
        [DataMember( Name = "categories" )]
        public string[] Categories { get; set; }

        /// <summary>
        /// Die Langbeschreibung der Sendung.
        /// </summary>
        [DataMember( Name = "description" )]
        public string Description { get; set; }

        /// <summary>
        /// Die Kurzbeschreibung der Sendung.
        /// </summary>
        [DataMember( Name = "shortDescription" )]
        public string Summary { get; set; }

        /// <summary>
        /// Gesetzt, wenn das Ende in der Zukunft liegt.
        /// </summary>
        [DataMember( Name = "active" )]
        public bool IsActive { get { return (StartTime + Duration) > DateTime.UtcNow; } set { } }

        /// <summary>
        /// Die eindeutige Kennung.
        /// </summary>
        [DataMember( Name = "id" )]
        public string Identifier { get; set; }

        /// <summary>
        /// Erstellt einen neuen Eintrag für die Programmzeitschrift.
        /// </summary>
        /// <param name="entry">Der originale Eintrag aus der Verwaltung.</param>
        /// <param name="profileName">Der Name des zugehörigen Geräteprofils.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public static GuideItem Create( ProgramGuideEntry entry, string profileName )
        {
            // Validate
            if (entry == null)
                throw new ArgumentNullException( "entry" );

            // Default name of the station
            var source = VCRProfiles.FindSource( profileName, entry.Source );

            // Create
            return
                new GuideItem
                {
                    Identifier = $"{entry.StartTime.Ticks}:{profileName}:{SourceIdentifier.ToString( entry.Source ).Replace( " ", "" )}",
                    Station = (source == null) ? entry.StationName : source.GetUniqueName(),
                    Duration = TimeSpan.FromSeconds( entry.Duration ),
                    Categories = entry.Categories.ToArray(),
                    Ratings = entry.Ratings.ToArray(),
                    Summary = entry.ShortDescription,
                    Description = entry.Description,
                    StartTime = entry.StartTime,
                    Language = entry.Language,
                    Name = entry.Name,
                };
        }
    }
}
