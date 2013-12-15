using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;

namespace JMS.DVB.EPG
{
	public class LanguageItem
	{
        public readonly string ISOLanguage;

        public readonly AudioTypes Effect;

		public LanguageItem(string language, AudioTypes effect)
		{
			// Remember
			ISOLanguage = language;
			Effect = effect;
		}

        private LanguageItem(Section section, int offset)
        {
            // Load the string
            ISOLanguage = section.ReadString(offset, 3);

            // Load the effect
            Effect = (AudioTypes)section[offset + 3];
        }

        public int Length
        {
            get
            {
                // Report static size
                return 4;
            }
        }

        internal static LanguageItem Create(Section section, int offset, int length)
        {
            // Test for length
            if (length < 4) return null;

            // Create
            return new LanguageItem(section, offset);
        }

		internal void CreatePayload(TableConstructor buffer)
		{
			// Append language
			buffer.AddLanguage(ISOLanguage);

			// Append effect
			buffer.Add((byte)Effect);
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
