using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// Beschreibt eine Ausstrahlung des deutschen PayTV Senders PREMIERE.
    /// </summary>
    public class ContentTransmissionPremiere : Descriptor
    {
        /// <summary>
        /// Die Kennung des zugehörigen <i>Transport Streams</i>.
        /// </summary>
        public ushort TransportStreamIdentifier;

        /// <summary>
        /// Die ursprüngliche Netzwerkkennung.
        /// </summary>
        public ushort OriginalNetworkIdentifier;

        /// <summary>
        /// Der zugehörige Sender.
        /// </summary>
        public ushort ServiceIdentifier;

        /// <summary>
        /// Alle Startzeiten zu der Sendung.
        /// </summary>
        public DateTime[] StartTimes;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="container">Die zugehörige Liste von Beschreibungen.</param>
        /// <param name="offset">Erstes Byte dieser Beschreibung.</param>
        /// <param name="length">Anzahl der Bytes für diese Beschreibung.</param>
        public ContentTransmissionPremiere(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
        {
            // Validate size
            if (length < 6) return;

            // Attach to data
            Section section = container.Section;

            // Load station data
            TransportStreamIdentifier = Tools.MergeBytesToWord(section[offset + 1], section[offset + 0]);
            OriginalNetworkIdentifier = Tools.MergeBytesToWord(section[offset + 3], section[offset + 2]);
            ServiceIdentifier = Tools.MergeBytesToWord(section[offset + 5], section[offset + 4]);

            // Adjust all
            offset += 6;
            length -= 6;

            // Times to use
            List<DateTime> dates = new List<DateTime>();

            // Process all
            while (length >= 2)
            {
                // Read the data
                DateTime day = Tools.DecodeDate(section, offset + 0);

                // Read the number of times
                int bytes = section[offset + 2];

                // Correct length and offset
                length -= 3 + bytes;
                offset += 3;

                // In error
                if (length < 0) return;

                // Read all
                for (; bytes > 0; bytes -= 3, offset += 3)
                {
                    // Add the schedule
                    dates.Add(day + Tools.DecodeDuration(section, offset));
                }

                // Validate
                if (0 != bytes) return;
            }

            // In error
            if (0 != length) return;

            // Nothing found
            if (dates.Count < 1) return;

            // Remember
            StartTimes = dates.ToArray();

            // Check if there is at least one time
            m_Valid = true;
        }

        /// <summary>
        /// Prüft, ob es sich um einen Sendungsbeschreibung von PREMIERE handelt.
        /// </summary>
        /// <param name="tag">Die zu prüfende Beschreibungsart.</param>
        /// <returns>Gesetzt, wenn es sich um die unterstützte Art (<i>0xF0</i>) handelt.</returns>
        public static bool IsHandlerFor(byte tag)
        {
            // Check it
            return (DescriptorTags.ContentTransmissionPremiere == (DescriptorTags)tag);
        }
    }
}
