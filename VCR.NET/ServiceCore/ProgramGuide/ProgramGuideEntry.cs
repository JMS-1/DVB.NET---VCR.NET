using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using JMS.DVB;


namespace JMS.DVBVCR.RecordingService.ProgramGuide
{
    /// <summary>
    /// This class hold the core EPG information used in the VCR.NET
    /// Recording Service.
    /// </summary>
    [Serializable]
    [XmlType( "EPGEvent" )]
    public class ProgramGuideEntry : IComparable
    {
        /// <summary>
        /// Wird gesetzt, um eine Umrechnung der Zeiten auf die lokale Zeitzone zu aktivieren.
        /// </summary>
        [XmlIgnore]
        public bool ShowLocalTimes { get; set; }

        /// <summary>
        /// Die Startzeit der Sendung in UTC/GMT Darstellung.
        /// </summary>
        private DateTime m_Start;

        /// <summary>
        /// Start time for the event in UTC/GMT notation.
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                // Report
                if (ShowLocalTimes)
                    return m_Start.ToLocalTime();
                else
                    return m_Start;
            }
            set
            {
                // Remember
                if (ShowLocalTimes)
                    m_Start = value.ToUniversalTime();
                else
                    m_Start = value;
            }
        }

        /// <summary>
        /// Transport identifier of the station.
        /// </summary>
        public ushort TransportIdentifier { get; set; }

        /// <summary>
        /// Original netowrk identifier of the station.
        /// </summary>
        public ushort NetworkIdentifier { get; set; }

        /// <summary>
        /// Service identifier of the station.
        /// </summary>
        public ushort ServiceIdentifier { get; set; }

        /// <summary>
        /// Short name of the station.
        /// </summary>
        [XmlElement( IsNullable = true )]
        public string StationName { get; set; }

        /// <summary>
        /// Der Name der Quell inklusive des Namens des Dienstanbieters.
        /// </summary>
        public string StationAndProviderName { get; set; }

        /// <summary>
        /// Description of the event.
        /// </summary>
        [XmlElement( IsNullable = true )]
        public string Description { get; set; }

        /// <summary>
        /// Eine Kurzbeschreibung der Sendung.
        /// </summary>
        [XmlElement( IsNullable = true )]
        public string ShortDescription { get; set; }

        /// <summary>
        /// Language for the event.
        /// </summary>
        [XmlElement( IsNullable = true )]
        public string Language { get; set; }

        /// <summary>
        /// All ratings related with this event.
        /// </summary>
        public readonly List<string> Ratings = new List<string>();

        /// <summary>
        /// Alle Kategorien der Sendung.
        /// </summary>
        public readonly List<string> Categories = new List<string>();

        /// <summary>
        /// The name of the event.
        /// </summary>
        [XmlElement( IsNullable = true )]
        public string Name { get; set; }

        /// <summary>
        /// The duration of the event in seconds.
        /// </summary>
        /// <remarks>
        /// This is used instead of a <see cref="TimeSpan"/> because the
        /// later one can not be properly serialzed and deserialized to
        /// XML using the .NET 1.1 core functionality.
        /// </remarks>
        public long Duration { get; set; }

        /// <summary>
        /// Alle Zeichen, die SOAP im XML Modus nicht ohne weiteres unterstützt.
        /// </summary>
        private static char[] m_Disallowed;

        /// <summary>
        /// Eine eindeutige Kennung für diesen Eintrag - dieser wird in einem
        /// Filter zum Blättern verwendet.
        /// </summary>
        [XmlIgnore]
        public Guid UniqueIdentifier { get; set; }

        /// <summary>
        /// Initialisiert statische Felder.
        /// </summary>
        static ProgramGuideEntry()
        {
            // Helper
            List<char> disallow = new List<char>();

            // Fill
            for (char ch = '\x0020'; ch-- > '\x0000'; )
                if (('\x0009' != ch) && ('\x000a' != ch) && ('\x000d' != ch))
                    disallow.Add( ch );

            // Use
            m_Disallowed = disallow.ToArray();
        }

        /// <summary>
        /// Create a new event instance.
        /// </summary>
        public ProgramGuideEntry()
        {
            // Setup fields
            UniqueIdentifier = Guid.NewGuid();
            StartTime = DateTime.MinValue;
        }

        /// <summary>
        /// Remove forbidden characters from string.
        /// </summary>
        /// <param name="input">Original string.</param>
        /// <returns>String ready to be sent using XML format.</returns>
        private static string CleanString( string input )
        {
            // Check mode
            if (input == null)
                return string.Empty;
            if (input.IndexOfAny( m_Disallowed ) < 0)
                return input;

            // Helper
            StringBuilder clean = new StringBuilder();

            // Process
            for (int cur = 0; cur < input.Length; )
            {
                // Find next
                int next = input.IndexOfAny( m_Disallowed, cur );

                // Done
                if (next < 0)
                {
                    // Take the test
                    clean.Append( input, cur, input.Length - cur );

                    // Done
                    break;
                }

                // Copy part
                clean.Append( input, cur, next - cur );

                // Advance
                cur = ++next;
            }

            // Use helper
            return clean.ToString();
        }

        /// <summary>
        /// Meldet das Ende der zugehörigen Sendung.
        /// </summary>
        [XmlIgnore]
        public DateTime EndTime
        {
            get
            {
                // Report
                if (ShowLocalTimes)
                    return m_Start.AddSeconds( Duration ).ToLocalTime();
                else
                    return m_Start.AddSeconds( Duration );
            }
        }

        /// <summary>
        /// Meldet die Quelle zu diesem Eintrag.
        /// </summary>
        [XmlIgnore]
        public SourceIdentifier Source
        {
            get
            {
                // Report
                return new SourceIdentifier( NetworkIdentifier, TransportIdentifier, ServiceIdentifier );
            }
        }

        #region IComparable Members

        /// <summary>
        /// Events are ordered by the <see cref="StartTime"/> field.
        /// </summary>
        /// <param name="obj">Some other instance.</param>
        /// <returns><see cref="DateTime.CompareTo(DateTime)"/> of the <see cref="StartTime"/>
        /// or -1 if the parameter is not an <see cref="ProgramGuideEntry"/>.</returns>
        public int CompareTo( object obj )
        {
            // Check other
            ProgramGuideEntry other = obj as ProgramGuideEntry;

            // Not comparable - we are left of these
            if (null == other)
                return -1;

            // Forward
            return StartTime.CompareTo( other.StartTime );
        }

        #endregion
    }
}
