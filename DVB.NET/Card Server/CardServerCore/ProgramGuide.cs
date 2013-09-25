using System;
using JMS.DVB.SI.ProgramGuide;
using System.Xml.Serialization;

namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Beschreibt, welche Erweiterungen beim Einlesen der elektronischen Programmzeitschrift
    /// <i>Electronic Program Guide (EPG)</i> berücksichtig werden sollen.
    /// </summary>
    [Flags]
    public enum EPGExtensions
    {
        /// <summary>
        /// Es sollen keine Erweiterungen aktiviert werden.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Die PREMIERE Sport Dienstkanäle sollen verwendet werden.
        /// </summary>
        PREMIERESport = 0x01,

        /// <summary>
        /// Die PREMIERE Direkt (Kino et al) Dienstkanäle sollen verwendet werden.
        /// </summary>
        PREMIEREDirect = 0x02,

        /// <summary>
        /// Die programmzeitschrift der englischen Sender sollen berücksichtigt werden.
        /// </summary>
        FreeSatUK = 0x04
    }

    /// <summary>
    /// Diese Klasse bildet einen Schlüssel aus einem Ursprung und einer Quellgruppe (Transponder)
    /// darauf.
    /// </summary>
    internal class GroupKey
    {
        /// <summary>
        /// Der zugehörige Ursprung.
        /// </summary>
        public GroupLocation Location { get; private set; }

        /// <summary>
        /// Die Quellgruppe (Transponder).
        /// </summary>
        public SourceGroup Group { get; private set; }

        /// <summary>
        /// Erzeugt einen Schlüssel zu einer Quelle.
        /// </summary>
        /// <param name="source">Die Informationen zur Quelle.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
        public GroupKey( SourceSelection source )
        {
            // Validate
            if (null == source)
                throw new ArgumentNullException( "source" );

            // Just remember
            Location = source.Location;
            Group = source.Group;
        }

        /// <summary>
        /// Ermittelt einen Schlüsselwert zu diesem Schlüssel.
        /// </summary>
        /// <returns>Ein Schlüsselwert.</returns>
        public override int GetHashCode()
        {
            // Base
            int hash = (null == Location) ? 0 : Location.GetHashCode();

            // Merge
            if (null == Group)
                return hash;
            else
                return (hash ^ Group.GetHashCode());
        }

        /// <summary>
        /// Vergleicht zwei Schlüssel.
        /// </summary>
        /// <param name="obj">Der andere Schlüssel.</param>
        /// <returns>Gesetzt, wenn die Schlüssel semantisch identisch sind.</returns>
        public override bool Equals( object obj )
        {
            // Change type
            GroupKey other = obj as GroupKey;

            // Not possible
            if (null == other)
                return false;

            // Compare all
            return Equals( Group, other.Group ) && Equals( Location, other.Location );
        }
    }

    /// <summary>
    /// Die Beschreibung für einen einzelnen Eintrag in der elektronischen Programmzeitschrift.
    /// </summary>
    [Serializable]
    public class ProgramGuideItem
    {
        /// <summary>
        /// Die Quelle, für die diese Beschreibung gültig ist.
        /// </summary>
        public SourceIdentifier Source { get; set; }

        /// <summary>
        /// Die Uhrzeit in UTC / GMT, an der die zugehörige Sendung beginnt.
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Die Dauer der Sendung in Sekunden.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Die Beschreibung zur Sendung.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Der Name der Sendung.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Die optionale Kurzbeschreibung der Sendung.
        /// </summary>
        public string ShortDescription { get; set; }

        /// <summary>
        /// Die Sprache, in der die Sendung ausgestrahlt wird.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Optionale Informationen zur Jugendfreigabe.
        /// </summary>
        public string[] Ratings { get; set; }

        /// <summary>
        /// Die eindeutige Kennung dieser Sendung.
        /// </summary>
        public uint Identifier { get; set; }

        /// <summary>
        /// Optional Informationen zum Inhalt der Ausstrahlung.
        /// </summary>
        [XmlArrayItem( typeof( UndefinedContentCategory ) )]
        [XmlArrayItem( typeof( MovieContentCategory ) )]
        [XmlArrayItem( typeof( NewsContentCategory ) )]
        [XmlArrayItem( typeof( ShowContentCategory ) )]
        [XmlArrayItem( typeof( SportContentCategory ) )]
        [XmlArrayItem( typeof( ChildrenContentCategory ) )]
        [XmlArrayItem( typeof( MusicContentCategory ) )]
        [XmlArrayItem( typeof( ArtContentCategory ) )]
        [XmlArrayItem( typeof( SocialContentCategory ) )]
        [XmlArrayItem( typeof( EducationContentCategory ) )]
        [XmlArrayItem( typeof( LeisureContentCategory ) )]
        [XmlArrayItem( typeof( ContentCharacteristicsCategory ) )]
        public ContentCategory[] Content { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public ProgramGuideItem()
        {
        }

        /// <summary>
        /// Erstellt einen Eintrag aus der Beschreibung einer Ausstrahlung.
        /// </summary>
        /// <param name="event">Die vorgefertigte Beschreibung.</param>
        public ProgramGuideItem( Event @event )
        {
            // Copy all
            Duration = (int) @event.Duration.TotalSeconds;
            ShortDescription = @event.ShortDescription;
            Description = @event.Description;
            Language = @event.Language;
            Content = @event.Content;
            Start = @event.StartTime;
            Ratings = @event.Ratings;
            Source = @event.Source;
            Name = @event.Name;
        }

        /// <summary>
        /// Meldet das Ende der Aufzeichnung in UTC / GMT.
        /// </summary>
        public DateTime End
        {
            get
            {
                // Calculate
                return Start.AddSeconds( Duration );
            }
        }
    }
}
