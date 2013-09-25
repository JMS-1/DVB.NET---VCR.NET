using System;
using System.Text;
using System.Collections.Generic;

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
            return Station.FindISOLanguage( language );
        }
    }
}
