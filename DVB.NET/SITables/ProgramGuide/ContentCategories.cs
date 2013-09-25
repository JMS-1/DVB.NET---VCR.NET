using System;
using System.Xml.Serialization;

namespace JMS.DVB.SI.ProgramGuide
{
    /// <summary>
    /// Beschreibt die Daten zu einer einzelnen Kategorie einer Ausstrahlung.
    /// </summary>
    [Serializable]
    public abstract class ContentCategory : IEquatable<ContentCategory>
    {
        /// <summary>
        /// Die elementare Beschreibung der Kategorie.
        /// </summary>
        protected int ContentNibbles { get; set; }

        /// <summary>
        /// Initialisiert die Kategoriebeschreibung.
        /// </summary>
        /// <param name="contentNibbles">Die elementare Beschreibung der Kategorie.</param>
        internal ContentCategory( int contentNibbles )
        {
            // Remember
            ContentNibbles = contentNibbles;
        }

        /// <summary>
        /// Ermittelt einen Suchschlüssel für diese Beschreibung.
        /// </summary>
        /// <returns>Der gewünschte Suchschlüssel.</returns>
        public override int GetHashCode()
        {
            // Forward
            return ContentNibbles.GetHashCode();
        }

        /// <summary>
        /// Vergleicht diese Beschreibung mit einem beliebigen Objekt.
        /// </summary>
        /// <param name="obj">Ein beliebiges Objekt.</param>
        /// <returns>Gesetzt, wenn das andere Objekt eine semantisch äquivalente 
        /// Beschreibung ist.</returns>
        public override bool Equals( object obj )
        {
            // Forward
            return Equals( obj as ContentCategory );
        }

        /// <summary>
        /// Meldet einen Anzeigetext.
        /// </summary>
        /// <returns>Ein Anzeigetext für diese Beschreibung.</returns>
        public override string ToString()
        {
            // Fallback
            return string.Format( "{0}.{1}", ContentNibbles >> 4, SubCategory );
        }

        /// <summary>
        /// Meldet die Detailbeschreibung.
        /// </summary>
        protected int SubCategory
        {
            get
            {
                // Report
                return (ContentNibbles & 0xf);
            }
            set
            {
                // Validate
                if (value < 0)
                    throw new ArgumentOutOfRangeException( "value" );
                if (value > 0xf)
                    throw new ArgumentOutOfRangeException( "value" );

                // Merge
                ContentNibbles = (ContentNibbles & 0xf0) | value;
            }
        }

        /// <summary>
        /// Erzeugt eine neue Beschreibung für die angegebene Kategorie.
        /// </summary>
        /// <param name="contentNibbles">Die ursprüngliche Darstellung der Kategorie.</param>
        /// <returns>Eine Repräsentation der Kategorie.</returns>
        internal static ContentCategory Create( int contentNibbles )
        {
            // Validate
            if (contentNibbles < 0)
                throw new ArgumentOutOfRangeException( "contentNibbles" );
            if (contentNibbles > 0xff)
                throw new ArgumentOutOfRangeException( "contentNibbles" );

            // Dispatch
            switch (contentNibbles >> 4)
            {
                default: return new UndefinedContentCategory( contentNibbles );
                case 1: return new MovieContentCategory( contentNibbles );
                case 2: return new NewsContentCategory( contentNibbles );
                case 3: return new ShowContentCategory( contentNibbles );
                case 4: return new SportContentCategory( contentNibbles );
                case 5: return new ChildrenContentCategory( contentNibbles );
                case 6: return new MusicContentCategory( contentNibbles );
                case 7: return new ArtContentCategory( contentNibbles );
                case 8: return new SocialContentCategory( contentNibbles );
                case 9: return new EducationContentCategory( contentNibbles );
                case 10: return new LeisureContentCategory( contentNibbles );
                case 11: return new ContentCharacteristicsCategory( contentNibbles );
            }
        }

        #region IEquatable<ContentCategory> Members

        /// <summary>
        /// Vergleicht zwei Beschreibungen.
        /// </summary>
        /// <param name="other">Eine andere Beschreibung.</param>
        /// <returns>Gesetzt, wenn beide Beschreibungen die gleiche Art von Inhalt bezeichnen.</returns>
        public bool Equals( ContentCategory other )
        {
            // Easy
            if (ReferenceEquals( other, null ))
                return false;
            else
                return (ContentNibbles == other.ContentNibbles);
        }

        #endregion
    }

    /// <summary>
    /// Eine nicht näher bezeichnete Kategorie.
    /// </summary>
    [XmlType( "UndefinedContent" ), Serializable]
    public class UndefinedContentCategory : ContentCategory
    {
        /// <summary>
        /// Erzeugt eine neue Kategoriebeschreibung.
        /// </summary>
        /// <param name="contentNibbles">Die elementare Beschreibung der Kategorie.</param>
        internal UndefinedContentCategory( int contentNibbles )
            : base( contentNibbles )
        {
        }

        /// <summary>
        /// Wird für die Serialisierung benötigt.
        /// </summary>
        public UndefinedContentCategory()
            : base( 0x00 )
        {
        }

        /// <summary>
        /// Meldet oder ändert die nicht festgelegte Hauptkategorie.
        /// </summary>
        [XmlAttribute( "category" )]
        public int Category
        {
            get
            {
                // Report
                return base.ContentNibbles >> 4;
            }
            set
            {
                // Validate
                if (value < 0)
                    throw new ArgumentOutOfRangeException( "value" );
                if (value > 0xf)
                    throw new ArgumentOutOfRangeException( "value" );

                // Cross check
                if (value > 0)
                    if (value < 12)
                        throw new ArgumentOutOfRangeException( "value" );

                // Update
                ContentNibbles = (ContentNibbles & 0xf) | (value << 4);
            }
        }

        /// <summary>
        /// Meldet die Detailbeschreibung.
        /// </summary>
        [XmlAttribute( "subCategory" )]
        public new int SubCategory
        {
            get
            {
                // Report
                return base.SubCategory;
            }
            set
            {
                // Update
                base.SubCategory = value;
            }
        }

        /// <summary>
        /// Meldet einen Anzeigetext für die Kategorie.
        /// </summary>
        /// <returns>Der angeforderte Anzeigetext.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "Undefined({0}, {1})", Category, SubCategory );
        }
    }

    /// <summary>
    /// Beschreibt Spilmfilme.
    /// </summary>
    [XmlType( "MovieContent" ), Serializable]
    public class MovieContentCategory : ContentCategory
    {
        /// <summary>
        /// Die möglichen Genres.
        /// </summary>
        [Serializable]
        public enum Genres
        {
            /// <summary>
            /// Keine weiteren Angaben.
            /// </summary>
            General,

            /// <summary>
            /// Räuber &amp; Gendarm.
            /// </summary>
            Detective,

            /// <summary>
            /// Abenteuer.
            /// </summary>
            Adventure,

            /// <summary>
            /// Science Fiction.
            /// </summary>
            ScienceFiction,

            /// <summary>
            /// Komödie.
            /// </summary>
            Comedy,

            /// <summary>
            /// Sitcom.
            /// </summary>
            Soap,

            /// <summary>
            /// Romatisch.
            /// </summary>
            Romance,

            /// <summary>
            /// Ernsthaft.
            /// </summary>
            Serious,

            /// <summary>
            /// Für Erwachsene.
            /// </summary>
            Adult,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1001,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1010,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1011,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1100,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1101,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1110,

            /// <summary>
            /// Durch den Sender definiert.
            /// </summary>
            User
        }

        /// <summary>
        /// Erzeugt eine neue Kategoriebeschreibung.
        /// </summary>
        /// <param name="contentNibbles">Die elementare Beschreibung der Kategorie.</param>
        internal MovieContentCategory( int contentNibbles )
            : base( contentNibbles )
        {
        }

        /// <summary>
        /// Wird für die Serialisierung benötigt.
        /// </summary>
        public MovieContentCategory()
            : base( 0x10 )
        {
        }

        /// <summary>
        /// Meldet die Detailbeschreibung.
        /// </summary>
        [XmlAttribute( "category" )]
        public Genres Genre
        {
            get
            {
                // Report
                return (Genres) base.SubCategory;
            }
            set
            {
                // Change
                base.SubCategory = (int) value;
            }
        }

        /// <summary>
        /// Meldet einen Anzeigetext für die Kategorie.
        /// </summary>
        /// <returns>Der angeforderte Anzeigetext.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "Movie({0})", Genre );
        }
    }

    /// <summary>
    /// Beschreibt Nachrichten.
    /// </summary>
    [XmlType( "NewsContent" ), Serializable]
    public class NewsContentCategory : ContentCategory
    {
        /// <summary>
        /// Die möglichen Arten von Nachrichten.
        /// </summary>
        [Serializable]
        public enum NewsCategories
        {
            /// <summary>
            /// Allgemein.
            /// </summary>
            General,

            /// <summary>
            /// Wetter.
            /// </summary>
            Weather,

            /// <summary>
            /// Nachrichtenmagazin.
            /// </summary>
            Magazine,

            /// <summary>
            /// Dokumentation.
            /// </summary>
            Documentation,

            /// <summary>
            /// Diskussionsrunde.
            /// </summary>
            Discussion,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0101,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0110,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0111,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1000,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1001,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1010,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1011,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1100,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1101,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1110,

            /// <summary>
            /// Vom Sender definiert.
            /// </summary>
            User
        }

        /// <summary>
        /// Erzeugt eine neue Kategoriebeschreibung.
        /// </summary>
        /// <param name="contentNibbles">Die elementare Beschreibung der Kategorie.</param>
        internal NewsContentCategory( int contentNibbles )
            : base( contentNibbles )
        {
        }

        /// <summary>
        /// Wird für die Serialisierung benötigt.
        /// </summary>
        public NewsContentCategory()
            : base( 0x20 )
        {
        }

        /// <summary>
        /// Meldet die Detailbeschreibung.
        /// </summary>
        [XmlAttribute( "category" )]
        public NewsCategories NewsCategory
        {
            get
            {
                // Report
                return (NewsCategories) base.SubCategory;
            }
            set
            {
                // Change
                base.SubCategory = (int) value;
            }
        }

        /// <summary>
        /// Meldet einen Anzeigetext für die Kategorie.
        /// </summary>
        /// <returns>Der angeforderte Anzeigetext.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "News({0})", NewsCategory );
        }
    }

    /// <summary>
    /// Beschreibt Unterhaltungssendung.
    /// </summary>
    [XmlType( "ShowContent" ), Serializable]
    public class ShowContentCategory : ContentCategory
    {
        /// <summary>
        /// Die möglichen Arten einer Unterhaltungssendung.
        /// </summary>
        [Serializable]
        public enum ShowCategories
        {
            /// <summary>
            /// Nicht weiter spezifiziert.
            /// </summary>
            General,

            /// <summary>
            /// Rateshow.
            /// </summary>
            Quiz,

            /// <summary>
            /// Akrobatik.
            /// </summary>
            Variety,

            /// <summary>
            /// Gesprächsrunde.
            /// </summary>
            Talk,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0100,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0101,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0110,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0111,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1000,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1001,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1010,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1011,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1100,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1101,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1110,

            /// <summary>
            /// Vom Sender definiert.
            /// </summary>
            User
        }

        /// <summary>
        /// Erzeugt eine neue Kategoriebeschreibung.
        /// </summary>
        /// <param name="contentNibbles">Die elementare Beschreibung der Kategorie.</param>
        internal ShowContentCategory( int contentNibbles )
            : base( contentNibbles )
        {
        }

        /// <summary>
        /// Wird für die Serialisierung benötigt.
        /// </summary>
        public ShowContentCategory()
            : base( 0x30 )
        {
        }

        /// <summary>
        /// Meldet die Detailbeschreibung.
        /// </summary>
        [XmlAttribute( "category" )]
        public ShowCategories ShowCategory
        {
            get
            {
                // Report
                return (ShowCategories) base.SubCategory;
            }
            set
            {
                // Change
                base.SubCategory = (int) value;
            }
        }

        /// <summary>
        /// Meldet einen Anzeigetext für die Kategorie.
        /// </summary>
        /// <returns>Der angeforderte Anzeigetext.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "Show({0})", ShowCategory );
        }
    }

    /// <summary>
    /// Beschreibt Sportereignisse.
    /// </summary>
    [XmlType( "SportContent" ), Serializable]
    public class SportContentCategory : ContentCategory
    {
        /// <summary>
        /// Die möglichen Sportarten.
        /// </summary>
        [Serializable]
        public enum Activities
        {
            /// <summary>
            /// Nicht weiter spezifiziert.
            /// </summary>
            General,

            /// <summary>
            /// Meisterschaften und andere Ereignisse.
            /// </summary>
            Event,

            /// <summary>
            /// Magazine.
            /// </summary>
            Magazine,

            /// <summary>
            /// Fußball.
            /// </summary>
            Soccer,

            /// <summary>
            /// Tennis.
            /// </summary>
            Tennis,

            /// <summary>
            /// Mannschaftssport (außer Fußball).
            /// </summary>
            Team,

            /// <summary>
            /// Turnen.
            /// </summary>
            Athletics,

            /// <summary>
            /// Motorsport.
            /// </summary>
            Motor,

            /// <summary>
            /// Wassersport.
            /// </summary>
            Water,

            /// <summary>
            /// Wintersport.
            /// </summary>
            Winter,

            /// <summary>
            /// Reiten.
            /// </summary>
            Equestrian,

            /// <summary>
            /// Kampfsport.
            /// </summary>
            Martial,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1100,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1101,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1110,

            /// <summary>
            /// Vom Sender definiert.
            /// </summary>
            User
        }

        /// <summary>
        /// Erzeugt eine neue Kategoriebeschreibung.
        /// </summary>
        /// <param name="contentNibbles">Die elementare Beschreibung der Kategorie.</param>
        internal SportContentCategory( int contentNibbles )
            : base( contentNibbles )
        {
        }

        /// <summary>
        /// Wird für die Serialisierung benötigt.
        /// </summary>
        public SportContentCategory()
            : base( 0x40 )
        {
        }

        /// <summary>
        /// Meldet die Detailbeschreibung.
        /// </summary>
        [XmlAttribute( "category" )]
        public Activities Activity
        {
            get
            {
                // Report
                return (Activities) base.SubCategory;
            }
            set
            {
                // Change
                base.SubCategory = (int) value;
            }
        }

        /// <summary>
        /// Meldet einen Anzeigetext für die Kategorie.
        /// </summary>
        /// <returns>Der angeforderte Anzeigetext.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "Sport({0})", Activity );
        }
    }

    /// <summary>
    /// Beschreibt Ausstrahlungen, die für Kinder geeignet sind.
    /// </summary>
    [XmlType( "ChildrenContent" ), Serializable]
    public class ChildrenContentCategory : ContentCategory
    {
        /// <summary>
        /// Die Art der Sendung.
        /// </summary>
        [Serializable]
        public enum Targets
        {
            /// <summary>
            /// Nicht weiter spezifiziert.
            /// </summary>
            General,

            /// <summary>
            /// Vorschule.
            /// </summary>
            PreShool,

            /// <summary>
            /// 6 bis 14 Jahre.
            /// </summary>
            To14,

            /// <summary>
            /// 10 bis 16 Jahre.
            /// </summary>
            To16,

            /// <summary>
            /// Lehrprogramme.
            /// </summary>
            Shool,

            /// <summary>
            /// Zeichentrick.
            /// </summary>
            Cartoon,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0110,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0111,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1000,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1001,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1010,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1011,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1100,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1101,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1110,

            /// <summary>
            /// Vom Sender definiert.
            /// </summary>
            User
        }

        /// <summary>
        /// Erzeugt eine neue Kategoriebeschreibung.
        /// </summary>
        /// <param name="contentNibbles">Die elementare Beschreibung der Kategorie.</param>
        internal ChildrenContentCategory( int contentNibbles )
            : base( contentNibbles )
        {
        }

        /// <summary>
        /// Wird für die Serialisierung benötigt.
        /// </summary>
        public ChildrenContentCategory()
            : base( 0x50 )
        {
        }

        /// <summary>
        /// Meldet die Detailbeschreibung.
        /// </summary>
        [XmlAttribute( "category" )]
        public Targets Target
        {
            get
            {
                // Report
                return (Targets) base.SubCategory;
            }
            set
            {
                // Change
                base.SubCategory = (int) value;
            }
        }

        /// <summary>
        /// Meldet einen Anzeigetext für die Kategorie.
        /// </summary>
        /// <returns>Der angeforderte Anzeigetext.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "Children({0})", Target );
        }
    }

    /// <summary>
    /// Beschreibt Musiksendungen.
    /// </summary>
    [XmlType( "MusicContent" ), Serializable]
    public class MusicContentCategory : ContentCategory
    {
        /// <summary>
        /// Die möglichen Musikrichtungen.
        /// </summary>
        [Serializable]
        public enum MusicStyles
        {
            /// <summary>
            /// Nicht weiter spezifiziert.
            /// </summary>
            General,

            /// <summary>
            /// Rock &amp; Pop.
            /// </summary>
            Rock,

            /// <summary>
            /// Klassische Musik.
            /// </summary>
            Classical,

            /// <summary>
            /// Traditionalle Musik.
            /// </summary>
            Folk,

            /// <summary>
            /// Jazz.
            /// </summary>
            Jazz,

            /// <summary>
            /// Oper &amp; Musical.
            /// </summary>
            Opera,

            /// <summary>
            /// Ballet.
            /// </summary>
            Ballet,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0111,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1000,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1001,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1010,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1011,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1100,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1101,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1110,

            /// <summary>
            /// Vom Sender definiert.
            /// </summary>
            User
        }

        /// <summary>
        /// Erzeugt eine neue Kategoriebeschreibung.
        /// </summary>
        /// <param name="contentNibbles">Die elementare Beschreibung der Kategorie.</param>
        internal MusicContentCategory( int contentNibbles )
            : base( contentNibbles )
        {
        }

        /// <summary>
        /// Wird für die Serialisierung benötigt.
        /// </summary>
        public MusicContentCategory()
            : base( 0x60 )
        {
        }

        /// <summary>
        /// Meldet die Detailbeschreibung.
        /// </summary>
        [XmlAttribute( "category" )]
        public MusicStyles MusicStyle
        {
            get
            {
                // Report
                return (MusicStyles) base.SubCategory;
            }
            set
            {
                // Change
                base.SubCategory = (int) value;
            }
        }

        /// <summary>
        /// Meldet einen Anzeigetext für die Kategorie.
        /// </summary>
        /// <returns>Der angeforderte Anzeigetext.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "Music({0})", MusicStyle );
        }
    }

    /// <summary>
    /// Beschreibt Kultursendungen.
    /// </summary>
    [XmlType( "ArtContent" ), Serializable]
    public class ArtContentCategory : ContentCategory
    {
        /// <summary>
        /// Die verschiedenen Kunstrichtungen.
        /// </summary>
        [Serializable]
        public enum ArtStyles
        {
            /// <summary>
            /// Nicht weiter spezifiziert.
            /// </summary>
            General,

            /// <summary>
            /// Performance Kunst.
            /// </summary>
            Performance,

            /// <summary>
            /// Feine Künste.
            /// </summary>
            Fine,

            /// <summary>
            /// Religion.
            /// </summary>
            Religion,

            /// <summary>
            /// Traditionelle Kunst.
            /// </summary>
            Traditional,

            /// <summary>
            /// Literatur.
            /// </summary>
            Literature,

            /// <summary>
            /// Kino.
            /// </summary>
            Cinema,

            /// <summary>
            /// Experimentalfilm.
            /// </summary>
            Experimental,

            /// <summary>
            /// Presse.
            /// </summary>
            Broadcasting,

            /// <summary>
            /// Neue Medien.
            /// </summary>
            NewMedia,

            /// <summary>
            /// Kunstmagazin.
            /// </summary>
            Magazine,

            /// <summary>
            /// Mode.
            /// </summary>
            Fashion,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1100,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1101,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1110,

            /// <summary>
            /// Vom Sender definiert.
            /// </summary>
            User
        }

        /// <summary>
        /// Erzeugt eine neue Kategoriebeschreibung.
        /// </summary>
        /// <param name="contentNibbles">Die elementare Beschreibung der Kategorie.</param>
        internal ArtContentCategory( int contentNibbles )
            : base( contentNibbles )
        {
        }

        /// <summary>
        /// Wird für die Serialisierung benötigt.
        /// </summary>
        public ArtContentCategory()
            : base( 0x70 )
        {
        }

        /// <summary>
        /// Meldet die Detailbeschreibung.
        /// </summary>
        [XmlAttribute( "category" )]
        public ArtStyles ArtStyle
        {
            get
            {
                // Report
                return (ArtStyles) base.SubCategory;
            }
            set
            {
                // Change
                base.SubCategory = (int) value;
            }
        }

        /// <summary>
        /// Meldet einen Anzeigetext für die Kategorie.
        /// </summary>
        /// <returns>Der angeforderte Anzeigetext.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "Art({0})", ArtStyle );
        }
    }

    /// <summary>
    /// Beschreibt politische Ausstrahlungen.
    /// </summary>
    [XmlType( "SocialContent" ), Serializable]
    public class SocialContentCategory : ContentCategory
    {
        /// <summary>
        /// Die Arten des Inhalts.
        /// </summary>
        [Serializable]
        public enum SubCategories
        {
            /// <summary>
            /// Nicht weiter spezifiziert.
            /// </summary>
            General,

            /// <summary>
            /// Magazin oder Dokumentation.
            /// </summary>
            Magazine,

            /// <summary>
            /// Ratgeber.
            /// </summary>
            Advisory,

            /// <summary>
            /// Besondere Persönlichkeiten.
            /// </summary>
            People,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0100,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0101,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0110,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0111,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1000,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1001,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1010,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1011,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1100,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1101,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1110,

            /// <summary>
            /// Vom Sender definiert.
            /// </summary>
            User
        }

        /// <summary>
        /// Erzeugt eine neue Kategoriebeschreibung.
        /// </summary>
        /// <param name="contentNibbles">Die elementare Beschreibung der Kategorie.</param>
        internal SocialContentCategory( int contentNibbles )
            : base( contentNibbles )
        {
        }

        /// <summary>
        /// Wird für die Serialisierung benötigt.
        /// </summary>
        public SocialContentCategory()
            : base( 0x80 )
        {
        }

        /// <summary>
        /// Meldet die Detailbeschreibung.
        /// </summary>
        [XmlAttribute( "category" )]
        public new SubCategories SubCategory
        {
            get
            {
                // Report
                return (SubCategories) base.SubCategory;
            }
            set
            {
                // Change
                base.SubCategory = (int) value;
            }
        }

        /// <summary>
        /// Meldet einen Anzeigetext für die Kategorie.
        /// </summary>
        /// <returns>Der angeforderte Anzeigetext.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "Social({0})", SubCategory );
        }
    }

    /// <summary>
    /// Beschreibt Lernprogramme.
    /// </summary>
    [XmlType( "EducationContent" ), Serializable]
    public class EducationContentCategory : ContentCategory
    {
        /// <summary>
        /// Alle Richtungen.
        /// </summary>
        [Serializable]
        public enum Classes
        {
            /// <summary>
            /// Nicht weiter spezifiziert.
            /// </summary>
            General,

            /// <summary>
            /// Natur.
            /// </summary>
            Nature,

            /// <summary>
            /// Technik.
            /// </summary>
            Science,

            /// <summary>
            /// Medizin.
            /// </summary>
            Medicine,

            /// <summary>
            /// Erdkunde.
            /// </summary>
            Countries,

            /// <summary>
            /// Sozialkunde.
            /// </summary>
            Social,

            /// <summary>
            /// Weiterbildung.
            /// </summary>
            Futher,

            /// <summary>
            /// Sprachen.
            /// </summary>
            Languages,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1000,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1001,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1010,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1011,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1100,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1101,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1110,

            /// <summary>
            /// Vom Sender definiert.
            /// </summary>
            User
        }

        /// <summary>
        /// Erzeugt eine neue Kategoriebeschreibung.
        /// </summary>
        /// <param name="contentNibbles">Die elementare Beschreibung der Kategorie.</param>
        internal EducationContentCategory( int contentNibbles )
            : base( contentNibbles )
        {
        }

        /// <summary>
        /// Wird für die Serialisierung benötigt.
        /// </summary>
        public EducationContentCategory()
            : base( 0x90 )
        {
        }

        /// <summary>
        /// Meldet die Detailbeschreibung.
        /// </summary>
        [XmlAttribute( "category" )]
        public Classes Class
        {
            get
            {
                // Report               
                return (Classes) base.SubCategory;
            }
            set
            {
                // Change
                base.SubCategory = (int) value;
            }
        }

        /// <summary>
        /// Meldet einen Anzeigetext für die Kategorie.
        /// </summary>
        /// <returns>Der angeforderte Anzeigetext.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "Education({0})", Class );
        }
    }

    /// <summary>
    /// Beschreibt Freizeitprogramme.
    /// </summary>
    [XmlType( "LeisureContent" ), Serializable]
    public class LeisureContentCategory : ContentCategory
    {
        /// <summary>
        /// Die möglichen Arten der Freizeitgestaltung.
        /// </summary>
        [Serializable]
        public enum Directions
        {
            /// <summary>
            /// Nicht weiter spezifiziert.
            /// </summary>
            General,

            /// <summary>
            /// Reisen.
            /// </summary>
            Travel,

            /// <summary>
            /// Handarbeit.
            /// </summary>
            Handcraft,

            /// <summary>
            /// Motorsport.
            /// </summary>
            Motor,

            /// <summary>
            /// Gesundheit.
            /// </summary>
            Health,

            /// <summary>
            /// Kochen.
            /// </summary>
            Cooking,

            /// <summary>
            /// Einkaufen.
            /// </summary>
            Shopping,

            /// <summary>
            /// Garten.
            /// </summary>
            Gardening,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1000,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1001,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1010,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1011,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1100,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1101,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1110,

            /// <summary>
            /// Vom Sender definiert.
            /// </summary>
            User
        }

        /// <summary>
        /// Erzeugt eine neue Kategoriebeschreibung.
        /// </summary>
        /// <param name="contentNibbles">Die elementare Beschreibung der Kategorie.</param>
        internal LeisureContentCategory( int contentNibbles )
            : base( contentNibbles )
        {
        }

        /// <summary>
        /// Wird für die Serialisierung benötigt.
        /// </summary>
        public LeisureContentCategory()
            : base( 0xa0 )
        {
        }

        /// <summary>
        /// Meldet die Detailbeschreibung.
        /// </summary>
        [XmlAttribute( "category" )]
        public Directions Direction
        {
            get
            {
                // Report
                return (Directions) base.SubCategory;
            }
            set
            {
                // Change
                base.SubCategory = (int) value;
            }
        }

        /// <summary>
        /// Meldet einen Anzeigetext für die Kategorie.
        /// </summary>
        /// <returns>Der angeforderte Anzeigetext.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "Leisure({0})", Direction );
        }
    }

    /// <summary>
    /// Beschreibt spezielle Details zu einer Ausstrahlung.
    /// </summary>
    [XmlType( "CharacteristicsContent" ), Serializable]
    public class ContentCharacteristicsCategory : ContentCategory
    {
        /// <summary>
        /// Die Besonderheit dieser Sendung.
        /// </summary>
        [Serializable]
        public enum Characteristics
        {
            /// <summary>
            /// In Originalsprache.
            /// </summary>
            OriginalLanguage,

            /// <summary>
            /// Schwarz-Weiß Ausstrahlung.
            /// </summary>
            BlackWhite,

            /// <summary>
            /// Bisher unveröffentlicht.
            /// </summary>
            Unpublished,

            /// <summary>
            /// Live.
            /// </summary>
            Live,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0100,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0101,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0110,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved0111,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1000,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1001,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1010,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1011,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1100,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1101,

            /// <summary>
            /// Noch nicht verwendet.
            /// </summary>
            Reserved1110,

            /// <summary>
            /// Vom Sender definiert.
            /// </summary>
            User
        }

        /// <summary>
        /// Erzeugt eine neue Kategoriebeschreibung.
        /// </summary>
        /// <param name="contentNibbles">Die elementare Beschreibung der Kategorie.</param>
        internal ContentCharacteristicsCategory( int contentNibbles )
            : base( contentNibbles )
        {
        }

        /// <summary>
        /// Wird für die Serialisierung benötigt.
        /// </summary>
        public ContentCharacteristicsCategory()
            : base( 0xbf )
        {
        }

        /// <summary>
        /// Meldet die Detailbeschreibung.
        /// </summary>
        [XmlAttribute( "category" )]
        public Characteristics Characteristic
        {
            get
            {
                // Report
                return (Characteristics) base.SubCategory;
            }
            set
            {
                // Change
                base.SubCategory = (int) value;
            }
        }

        /// <summary>
        /// Meldet einen Anzeigetext für die Kategorie.
        /// </summary>
        /// <returns>Der angeforderte Anzeigetext.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "Characteristics({0})", Characteristic );
        }
    }
}
