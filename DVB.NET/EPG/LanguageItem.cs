using System.Collections.Generic;
using System.Globalization;


namespace JMS.DVB.EPG
{
    /// <summary>
    /// Beschreibt eine einzelne Sprachkomponente.
    /// </summary>
    public class LanguageItem
    {
        /// <summary>
        /// Der <i>ISO</i> Name der Sprache.
        /// </summary>
        public readonly string ISOLanguage;

        /// <summary>
        /// Details zur Sprachkomponente.
        /// </summary>
        public readonly AudioTypes Effect;

        /// <summary>
        /// Erstellt eine neue Sprache.
        /// </summary>
        /// <param name="language">Die Sprach in <i>ISO</i> Notation.</param>
        /// <param name="effect">Details zum Imhalt der Sprachkomponente.</param>
        public LanguageItem( string language, AudioTypes effect )
        {
            // Remember
            ISOLanguage = language;
            Effect = effect;
        }

        /// <summary>
        /// Erstellt eine neue Sprache.
        /// </summary>
        /// <param name="section">Die Rohdaten.</param>
        /// <param name="offset">Das erste Byte der Beschreibung in den Rohdaten.</param>
        private LanguageItem( Section section, int offset )
        {
            // Load the string
            ISOLanguage = section.ReadString( offset, 3 );

            // Load the effect
            Effect = (AudioTypes) section[offset + 3];
        }

        /// <summary>
        /// Meldet die Größe der Rohbeschreibung in Bytes.
        /// </summary>
        public int Length { get { return 4; } }

        /// <summary>
        /// Erstellt eine neue Sprache.
        /// </summary>
        /// <param name="section">Die Rohdaten.</param>
        /// <param name="offset">Das erste Byte der Beschreibung in den Rohdaten.</param>
        /// <param name="length">Die Größe der Rohdaten zu dieser Beschreibung in Bytes.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        internal static LanguageItem Create( Section section, int offset, int length )
        {
            // Test for length
            if (length < 4) return null;

            // Create
            return new LanguageItem( section, offset );
        }

        /// <summary>
        /// Rekonstruiert eine Beschreibung.
        /// </summary>
        /// <param name="buffer">Sammelt die Rekonstruktion mehrere Beschreibungen.</param>
        internal void CreatePayload( TableConstructor buffer )
        {
            // Append language
            buffer.AddLanguage( ISOLanguage );

            // Append effect
            buffer.Add( (byte) Effect );
        }
    }

    /// <summary>
    /// Erweiterungsmethoden für das Arbeiten mit Sprachen.
    /// </summary>
    public static class LanguageItemExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        private static Dictionary<string, string> m_LanguageMap = new Dictionary<string, string>();

        /// <summary>
        /// 
        /// </summary>
        private static Dictionary<string, string> m_LanguageMapEx = new Dictionary<string, string>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="audioName"></param>
        /// <returns></returns>
        private static string FindISOLanguage( string audioName )
        {
            // Not possible
            if (string.IsNullOrEmpty( audioName )) return null;

            // Split off
            string[] parts = audioName.Split( ' ', '[' );

            // Find it
            string shortName;
            if (LanguageMap.TryGetValue( parts[0], out shortName ))
                return shortName;

            // Find it
            if (EnglishLanguageMap.TryGetValue( parts[0], out shortName ))
                return shortName;

            // Not found
            return parts[0];
        }

        /// <summary>
        /// Fill the language mappings once.
        /// </summary>
        private static void LoadLanguageMap()
        {
            // Lock the map and load
            lock (m_LanguageMap)
                if (m_LanguageMap.Count < 1)
                    foreach (CultureInfo info in CultureInfo.GetCultures( CultureTypes.NeutralCultures ))
                    {
                        // Primary
                        m_LanguageMap[info.NativeName] = info.ThreeLetterISOLanguageName;

                        // Extended
                        m_LanguageMapEx[info.EnglishName] = info.ThreeLetterISOLanguageName;
                    }
        }

        /// <summary>
        /// Report the current mapping of native names to ISO names.
        /// </summary>
        private static Dictionary<string, string> LanguageMap
        {
            get
            {
                // Lock the map
                LoadLanguageMap();

                // Report
                return m_LanguageMap;
            }
        }

        /// <summary>
        /// Report the current mapping of native names to ISO names.
        /// </summary>
        private static Dictionary<string, string> EnglishLanguageMap
        {
            get
            {
                // Lock the map
                LoadLanguageMap();

                // Report
                return m_LanguageMapEx;
            }
        }

        /// <summary>
        /// Ermittelt zu einer Sprachangabe die ISO Kurznotation, sofern möglich.
        /// </summary>
        /// <param name="language">Die gewünschte Sprache.</param>
        /// <returns>Die ISO Kurznotation.</returns>
        public static string ToISOLanguage( this string language )
        {
            // Not possible
            if (null == language)
                return null;

            // Use helper
            return FindISOLanguage( language );
        }
    }
}
