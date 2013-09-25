using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// Enthält die Beschreibung der DVB Untertitel zu einem Sender.
    /// </summary>
	public class Subtitle: Descriptor
	{
        /// <summary>
        /// Enthält die Beschreibung aller DVB Untertitel.
        /// </summary>
        public readonly List<SubtitleInfo> Subtitles = new List<SubtitleInfo>();

		/// <summary>
        /// Erzeugt eine neue Beschreibung.
		/// </summary>
        public Subtitle()
			: base(DescriptorTags.Subtitling)
		{
		}

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="container">Die zugehörige Liste zusammengehöriger Beschreibungen.</param>
        /// <param name="offset">Erstes Byte für diese einzelne Beschreibung.</param>
        /// <param name="length">Die Länge dieser Beschreibung in Bytes.</param>
        public Subtitle(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
		{
            // Process all
            while (length > 0)
            {
                // Create sub title data
                SubtitleInfo info = SubtitleInfo.Create(container.Section, offset, length);
                if (null == info) break;

                // Adjust
                offset += info.Length;
                length -= info.Length;

                // Remember
                Subtitles.Add(info);
            }

            // Check
            m_Valid = (0 == length);
        }

        /// <summary>
        /// Meldet, ob diese Beschreibung in der Lage ist, bestimmte Rohdaten zu analysieren.
        /// </summary>
        /// <param name="tag">Die eindeutige Nummer der Beschreibung.</param>
        /// <returns>Gesetzt, wenn es sich um die Beschreibung von DVB Untertiteln handelt.</returns>
        public static bool IsHandlerFor(byte tag)
        {
            // Check it
            return (DescriptorTags.Subtitling == (DescriptorTags)tag);
        }

        /// <summary>
        /// Rekonstruiert diese Beschreibung zur Erstellung einer SI Tabelle.
        /// </summary>
        /// <param name="buffer">Ein Speicherbereich, in dem die Tabelle rekonstruiert wird.</param>
        protected override void CreatePayload(TableConstructor buffer)
        {
            // Forward
            foreach (SubtitleInfo info in Subtitles) info.CreatePayload(buffer);
        }
    }
}
