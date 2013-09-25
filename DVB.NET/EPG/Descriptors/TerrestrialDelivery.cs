using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// Beschreibt einen DVB-T Transponder.
    /// </summary>
    public class TerrestrialDelivery : Descriptor
    {
        /// <summary>
        /// Die Frequenz des Transponders in kHz.
        /// </summary>
        public uint Frequency { get; private set; }

        /// <summary>
        /// Die Bandbreite der Ausstrahlung.
        /// </summary>
        public TerrestrialBandwidths Bandwidth { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="container">Der SI Bereich, in dem diese Beschreibung gefunden wurde.</param>
        /// <param name="offset">Das erste Byte zu den Rohdaten dieser Beschreibung in dem zugehörigen Bereich.</param>
        /// <param name="length">Die Anzahl der Bytes für die Rohdaten dieser Beschreibung.</param>
        public TerrestrialDelivery( IDescriptorContainer container, int offset, int length )
            : base( container, offset, length )
        {
            // Not possible
            if (11 != length)
                return;

            // Attach to section
            Section section = container.Section;

            // Load direct data
            Frequency = Tools.MergeBytesToDoubleWord( section[offset + 3], section[offset + 2], section[offset + 1], section[offset + 0] ) / 100;

            // Load bandwith
            Bandwidth = (TerrestrialBandwidths) (section[offset + 4] >> 5);

            // We are valid
            m_Valid = true;
        }

        /// <summary>
        /// Prüft, ob diese Klasse für eine bestimmte Art von SI Beschreibungen zuständig ist.
        /// </summary>
        /// <param name="tag">Die eindeutige Kennung einer SI Beschreibung.</param>
        /// <returns>Gesetzt, wenn diese Klasse für die angegebene Art von Beschreibung zurständig ist.</returns>
        public static bool IsHandlerFor( byte tag )
        {
            // Check it
            return (DescriptorTags.TerrestrialDeliverySystem == (DescriptorTags) tag);
        }

        /// <summary>
        /// Wandelt eine Frequenzangabe aus einer <see cref="FrequencyList"/> in eine echte Frequenz um.
        /// </summary>
        /// <param name="frequency">Die Rohdaten der Frequenz.</param>
        /// <returns>Die gewünschte Frequenz in Hz.</returns>
        internal static ulong ConvertFrequency( uint frequency )
        {
            // Easy
            return 10 * (ulong) frequency;
        }
    }
}
