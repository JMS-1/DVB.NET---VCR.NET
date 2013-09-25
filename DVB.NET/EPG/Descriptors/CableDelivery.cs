using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
    public class CableDelivery : Descriptor
    {
        public uint Frequency { get; private set; }

        public uint SymbolRate { get; private set; }

        public OuterFECs OuterFEC { get; private set; }

        public CableModulations Modulation { get; private set; }

        public InnerFECs InnerFEC { get; private set; }

        public CableDelivery( IDescriptorContainer container, int offset, int length )
            : base( container, offset, length )
        {
            // Not possible
            if (11 != length) return;

            // Attach to section
            Section section = container.Section;

            // Load parts
            uint freq0 = (uint) Tools.FromBCD( section[offset + 0] );
            uint freq1 = (uint) Tools.FromBCD( section[offset + 1] );
            uint freq2 = (uint) Tools.FromBCD( section[offset + 2] );
            uint freq3 = (uint) Tools.FromBCD( section[offset + 3] );
            uint rate0 = (uint) Tools.FromBCD( section[offset + 7] );
            uint rate1 = (uint) Tools.FromBCD( section[offset + 8] );
            uint rate2 = (uint) Tools.FromBCD( section[offset + 9] );
            uint rate3 = (uint) Tools.FromBCD( (byte) ((section[offset + 10] >> 4) & 0xf) );

            // Load all
            SymbolRate = (rate3 + 10 * (rate2 + 100 * (rate1 + 100 * rate0))) / 10;
            Frequency = (freq3 + 100 * (freq2 + 100 * (freq1 + 100 * freq0))) / 10;
            InnerFEC = (InnerFECs) (section[offset + 10] & 0x0f);
            Modulation = (CableModulations) section[offset + 6];
            OuterFEC = (OuterFECs) (section[offset + 5] & 0x0f);

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
            return (DescriptorTags.CableDeliverySystem == (DescriptorTags) tag);
        }

        /// <summary>
        /// Wandelt eine Frequenzangabe aus einer <see cref="FrequencyList"/> in eine echte Frequenz um.
        /// </summary>
        /// <param name="frequency">Die Rohdaten der Frequenz.</param>
        /// <returns>Die Frequenz in Hz.</returns>
        internal static ulong ConvertFrequency( uint frequency )
        {
            // Get parts
            ulong freq0 = (ulong) Tools.FromBCD( (byte) ((frequency >> 24) & 0xff) );
            ulong freq1 = (ulong) Tools.FromBCD( (byte) ((frequency >> 16) & 0xff) );
            ulong freq2 = (ulong) Tools.FromBCD( (byte) ((frequency >> 8) & 0xff) );
            ulong freq3 = (ulong) Tools.FromBCD( (byte) ((frequency >> 0) & 0xff) );

            // Report
            return 100 * (freq3 + 100 * (freq2 + 100 * (freq1 + 100 * freq0)));
        }
    }
}
