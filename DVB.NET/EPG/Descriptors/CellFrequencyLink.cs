using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// Beschreibt DVB-T Referenzen auf andere Zellen.
    /// </summary>
    public class CellFrequencyLink : Descriptor
    {
        /// <summary>
        /// Die Kennungen und Frequenzen der anderen Zellen.
        /// </summary>
        public readonly List<FrequencyLink> Links = new List<FrequencyLink>();

        /// <summary>
        /// Erzeugt eine neue Referenzliste.
        /// </summary>
        /// <param name="container">Der SI Bereich, in dem diese Liste gefunden wurde.</param>
        /// <param name="offset">Der Index des ersten Bytes dieser Liste in den Rohdaten des SI Bereichs.</param>
        /// <param name="length">Die Anzahl der Bytes für diese Liste.</param>
        public CellFrequencyLink( IDescriptorContainer container, int offset, int length )
            : base( container, offset, length )
        {
            // Check minimum length
            if (length < 0)
                return;

            // Attach to data
            Section section = container.Section;

            // Helper
            List<FrequencyLink> links = new List<FrequencyLink>();

            // Load
            while (length > 0)
            {
                // Create
                FrequencyLink link = FrequencyLink.Create( section, offset, length );

                // Done
                if (null == link)
                    break;

                // Remember
                links.Add( link );

                // Correct 
                offset += link.Length;
                length -= link.Length;
            }

            // Test
            m_Valid = (0 == length);

            // Load
            if (m_Valid)
                Links = links;
        }

        /// <summary>
        /// Prüft, ob diese Klasse für eine bestimmte Art von SI Beschreibungen zuständig ist.
        /// </summary>
        /// <param name="tag">Die eindeutige Kennung einer SI Beschreibung.</param>
        /// <returns>Gesetzt, wenn diese Klasse für die angegebene Art von Beschreibung zurständig ist.</returns>
        public static bool IsHandlerFor( byte tag )
        {
            // Check it
            return (DescriptorTags.CellFrequencyLink == (DescriptorTags) tag);
        }
    }
}
