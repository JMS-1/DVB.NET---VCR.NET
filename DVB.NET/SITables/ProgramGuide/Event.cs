extern alias oldVersion;

using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;

using legacy = oldVersion::JMS.DVB.EPG;

namespace JMS.DVB.SI.ProgramGuide
{
    /// <summary>
    /// Beschreibt einen einzelnen Eintrag in der elektronischen Programmzeitschrift.
    /// </summary>
    [Serializable]
    [XmlType( "ProgramGuideEntry" )]
    public class Event
    {
        /// <summary>
        /// Der zugehörige Eintrag aus dem Datenstrom.
        /// </summary>
        private IEnumerable<legacy.Descriptor> m_Descriptors;

        /// <summary>
        /// Die den Inhalt dieser Sendung beschreibenden Kategorien.
        /// </summary>
        private HashSet<ContentCategory> m_Categories;

        /// <summary>
        /// Informationen zur Altersfreigabe.
        /// </summary>
        private HashSet<string> m_Ratings;

        /// <summary>
        /// Der Kurzname dieses Eintrags.
        /// </summary>
        private string m_Name;

        /// <summary>
        /// Die Beschreibung der zugehörigen Sendung.
        /// </summary>
        private string m_Description;

        /// <summary>
        /// Die optionale Kurzbeschreibung der zugehörigen Sendung.
        /// </summary>
        private string m_ShortDescription;

        /// <summary>
        /// Die Landessprache, in der diese Ausstrahlung erfolgt.
        /// </summary>
        private string m_Language;

        /// <summary>
        /// Der Zeitpunkt, an dem diese Sendung anfängt (in GMT / UTC).
        /// </summary>
        [XmlAttribute( "start" )]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Die Laufzeit in 0.1µs Einheiten.
        /// </summary>
        [XmlAttribute( "duration" )]
        public long DurationTicks { get; set; }

        /// <summary>
        /// Die Dauer dieser Sendung.
        /// </summary>
        [XmlIgnore]
        public TimeSpan Duration
        {
            get
            {
                // Report
                return new TimeSpan( DurationTicks );
            }
            set
            {
                // Update
                DurationTicks = value.Ticks;
            }
        }

        /// <summary>
        /// Die zugehörige Quelle.
        /// </summary>
        public SourceIdentifier Source { get; set; }

        /// <summary>
        /// Die eindeutige Kennung der Sendung.
        /// </summary>
        [XmlAttribute( "id" )]
        public uint Identifier { get; set; }

        /// <summary>
        /// Wird zur Deserialisierung benötigt.
        /// </summary>
        public Event()
        {
        }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="source">Die zugehörige Quelle.</param>
        /// <param name="entry">Die Rohdaten aus dem DVB Datenstrom.</param>
        /// <exception cref="ArgumentNullException">Ein Parameter wurde nicht angegeben.</exception>
        public Event( SourceIdentifier source, legacy.EventEntry entry )
            : this( source, entry.EventIdentifier, entry.StartTime, entry.Duration, entry.Descriptors )
        {
        }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="source">Die zugehörige Quelle.</param>
        /// <param name="identifier">Die eindeutige Nummer des Ereignisses.</param>
        /// <param name="startTime">Der Beginn der Sendung in UTC / GMT.</param>
        /// <param name="duration">Die Dauer der Ausstrahlung.</param>
        /// <param name="descriptors">Die Beschreibung der Sendung.</param>
        /// <exception cref="ArgumentNullException">Ein Parameter wurde nicht angegeben.</exception>
        public Event( SourceIdentifier source, uint identifier, DateTime startTime, TimeSpan duration, IEnumerable<legacy.Descriptor> descriptors )
        {
            // Validate
            if (source == null)
                throw new ArgumentNullException( "source" );

            // Copy over
            Identifier = identifier;
            StartTime = startTime;
            Duration = duration;
            Source = source;

            // Remember
            m_Descriptors = descriptors;
        }

        /// <summary>
        /// Der Zeitpunkt, an dem die Sendung endet (in GMT / UTC).
        /// </summary>
        [XmlIgnore]
        public DateTime EndTime
        {
            get
            {
                // Caluclate
                return StartTime + Duration;
            }
        }

        /// <summary>
        /// Meldet den Namen dieser Sendung.
        /// </summary>
        public string Name
        {
            get
            {
                // Synchronize
                Load();

                // Report 
                return m_Name;
            }
            set
            {
                // Change
                m_Name = value;
            }
        }

        /// <summary>
        /// Meldet eine Beschreibung zu dieser Sendung.
        /// </summary>
        public string Description
        {
            get
            {
                // Synchronize
                Load();

                // Report 
                return m_Description;
            }
            set
            {
                // Update
                m_Description = value;
            }
        }

        /// <summary>
        /// Meldet die optionale Kurzbeschreibung zu dieser Sendung.
        /// </summary>
        public string ShortDescription
        {
            get
            {
                // Synchronize
                Load();

                // Report 
                return m_ShortDescription;
            }
            set
            {
                // Update
                m_ShortDescription = value;
            }
        }

        /// <summary>
        /// Meldet die Landessprache zu dieser Sendung.
        /// </summary>
        [XmlAttribute( "lang" )]
        public string Language
        {
            get
            {
                // Synchronize
                Load();

                // Report 
                return m_Language;
            }
            set
            {
                // Update
                m_Language = value;
            }
        }

        /// <summary>
        /// Meldet die inhaltliche Beschreibung zu dieser Sendung.
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
        public ContentCategory[] Content
        {
            get
            {
                // Synchronize
                Load();

                // Report 
                return m_Categories.ToArray();
            }
            set
            {
                // Reset
                if (m_Categories == null)
                    m_Categories = new HashSet<ContentCategory>();
                else
                    m_Categories.Clear();

                // Store back
                if (value != null)
                    foreach (var category in value)
                        if (category != null)
                            m_Categories.Add( category );
            }
        }

        /// <summary>
        /// Meldet die bekannten Altersfreigaben zu dieser Sendung.
        /// </summary>
        [XmlElement( "Rating" )]
        public string[] Ratings
        {
            get
            {
                // Synchronize
                Load();

                // Report 
                return m_Ratings.ToArray();
            }
            set
            {
                // Reset
                if (m_Ratings == null)
                    m_Ratings = new HashSet<string>( StringComparer.InvariantCultureIgnoreCase );
                else
                    m_Ratings.Clear();

                // Store
                if (value != null)
                    foreach (var rating in value)
                        if (!string.IsNullOrEmpty( rating ))
                            m_Ratings.Add( rating );
            }
        }

        /// <summary>
        /// Berechnet einmalig die Daten dieser Beschreibung.
        /// </summary>
        public void Load()
        {
            // Already did it
            if (m_Ratings != null)
                return;

            // Once only
            m_Ratings = new HashSet<string>( StringComparer.InvariantCultureIgnoreCase );
            m_Categories = new HashSet<ContentCategory>();

            // Descriptors we can have
            legacy.Descriptors.ShortEvent shortEvent = null;

            // Extended events
            var exEvents = new List<legacy.Descriptors.ExtendedEvent>();

            // Check all descriptors
            if (m_Descriptors != null)
                foreach (var descr in m_Descriptors)
                    if (descr.IsValid)
                    {
                        // Load short event description once
                        if (null == shortEvent)
                        {
                            // Read
                            shortEvent = descr as legacy.Descriptors.ShortEvent;

                            // Done for now
                            if (null != shortEvent)
                                continue;
                        }

                        // Check for rating
                        var rating = descr as legacy.Descriptors.ParentalRating;

                        // Done for now
                        if (null != rating)
                        {
                            // Process
                            if (rating.Ratings != null)
                                foreach (string singleRating in rating.Ratings)
                                    if (!string.IsNullOrEmpty( singleRating ))
                                        m_Ratings.Add( singleRating );

                            // Next
                            continue;
                        }

                        // Check for extended event
                        var exEvent = descr as legacy.Descriptors.ExtendedEvent;

                        // Register
                        if (null != exEvent)
                        {
                            // Remember
                            exEvents.Add( exEvent );

                            // Next
                            continue;
                        }

                        // Check for content information
                        var content = descr as legacy.Descriptors.Content;

                        // Remember
                        if (content != null)
                        {
                            // Process
                            if (content.Categories != null)
                                foreach (var singleCategory in content.Categories)
                                    m_Categories.Add( GetContentCategory( singleCategory ) );

                            // Next
                            continue;
                        }
                    }

            // Take the best we got
            if (exEvents.Count > 0)
            {
                // Text builder
                var text = new StringBuilder();

                // Process all
                foreach (var exEvent in exEvents)
                {
                    // Normal
                    if (null == m_Name)
                        m_Name = exEvent.Name;
                    if (null == m_Language)
                        m_Language = exEvent.Language;

                    // Merge
                    if (exEvent.Text != null)
                        text.Append( exEvent.Text );
                }

                // Use
                m_Description = text.ToString();
            }

            // Try short event
            if (shortEvent != null)
            {
                // Remember
                if (string.IsNullOrEmpty( shortEvent.Name ))
                    m_ShortDescription = shortEvent.Text;
                else if (string.IsNullOrEmpty( shortEvent.Text ))
                    m_ShortDescription = shortEvent.Name;
                else if (string.IsNullOrEmpty( m_Description ) || StringComparer.Ordinal.Equals( shortEvent.Text, m_Description ))
                    m_ShortDescription = shortEvent.Name;
                else
                    m_ShortDescription = string.Format( "{0} ({1})", shortEvent.Name, shortEvent.Text );

                // Read
                if (string.IsNullOrEmpty( m_Name ))
                    m_Name = shortEvent.Name;
                if (string.IsNullOrEmpty( m_Description ))
                    m_Description = shortEvent.Text;
                if (string.IsNullOrEmpty( m_Language ))
                    m_Language = shortEvent.Language;
            }

            // Not possible
            if (string.IsNullOrEmpty( m_Name ))
                return;

            // Defaults
            if (m_ShortDescription == null)
                m_ShortDescription = string.Empty;

            // Defaults
            if (string.IsNullOrEmpty( m_Description ))
                m_Description = "-";
            if (null == m_Language)
                m_Language = string.Empty;
        }

        #region Hilfsmethoden zur Analyse von Kategorien

        /// <summary>
        /// Ermittelt die Kategorie einer Ausstrahlung.
        /// </summary>
        /// <param name="contentNibbles">Die Rohdarstellung der Kategorie.</param>
        /// <returns></returns>
        public static ContentCategory GetContentCategory( int contentNibbles )
        {
            // Forward
            return ContentCategory.Create( contentNibbles );
        }

        #endregion
    }
}
