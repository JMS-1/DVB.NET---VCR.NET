using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// Verwaltet eine Liste von Frequenzen.
    /// </summary>
    public class FrequencyList : Descriptor
    {
        /// <summary>
        /// Beschreibt, wie die Rohwerte der Frequenzen zu interpretieren sind.
        /// </summary>
        public enum CodingTypes
        {
            /// <summary>
            /// Nicht festgelegt.
            /// </summary>
            NotDefined,

            /// <summary>
            /// DVB-S(2).
            /// </summary>
            Satellite,

            /// <summary>
            /// DVB-C.
            /// </summary>
            Cable,

            /// <summary>
            /// DVB-T.
            /// </summary>
            Terrestrial
        }

        /// <summary>
        /// Meldet oder legt fest, wie die Frequenzen zu interpretieren sind.
        /// </summary>
        public CodingTypes CodingType { get; set; }

        /// <summary>
        /// Alle Frequenzen diese Liste.
        /// </summary>
        public readonly List<uint> Frequencies = new List<uint>();

        /// <summary>
        /// Erzeugt eine neue Liste.
        /// </summary>
        /// <param name="container">Der SI Bereich, in dem diese Liste gefunden wurde.</param>
        /// <param name="offset">Der Index des ersten Bytes dieser Liste in den Rohdaten des SI Bereichs.</param>
        /// <param name="length">Die Anzahl der Bytes für diese Liste.</param>
        public FrequencyList( IDescriptorContainer container, int offset, int length )
            : base( container, offset, length )
        {
            // Check for minimum length
            if (length < 1)
                return;

            // Attach to section
            Section section = container.Section;

            // Read the coding type
            CodingType = (CodingTypes) (section[offset + 0] & 0x3);

            // Load all
            for (length -= 1, offset += 1; length > 0; length -= 4, offset += 4)
            {
                // Not possible
                if (length < 4)
                    return;

                // Load the item
                Frequencies.Add( Tools.MergeBytesToDoubleWord( section[offset + 3], section[offset + 2], section[offset + 1], section[offset + 0] ) );
            }

            // Test
            m_Valid = true;
        }

        /// <summary>
        /// Meldet eine der Frequenzen in der natürlichen Form.
        /// </summary>
        /// <param name="index">Die 0-basierte laufende Nummer der gewünschten Frequenz.</param>
        /// <returns>Die Frequenz in Hz.</returns>
        public ulong ConvertFrequency( int index )
        {
            // Check mode
            switch (CodingType)
            {
                case CodingTypes.Satellite: return SatelliteDelivery.ConvertFrequency( Frequencies[index] );
                case CodingTypes.Cable: return CableDelivery.ConvertFrequency( Frequencies[index] );
                case CodingTypes.Terrestrial: return TerrestrialDelivery.ConvertFrequency( Frequencies[index] );
                default: return Frequencies[index];
            }
        }

        /// <summary>
        /// Prüft, ob diese Klasse für eine bestimmte Art von SI Beschreibungen zuständig ist.
        /// </summary>
        /// <param name="tag">Die eindeutige Kennung einer SI Beschreibung.</param>
        /// <returns>Gesetzt, wenn diese Klasse für die angegebene Art von Beschreibung zurständig ist.</returns>
        public static bool IsHandlerFor( byte tag )
        {
            // Check it
            return (DescriptorTags.FrequencyList == (DescriptorTags) tag);
        }
    }
}
