

namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// Beschreibt eine Quellgruppe für den DVB-S Empfang.
    /// </summary>
    public class SatelliteDelivery : Descriptor
    {
        /// <summary>
        /// Die Sendefrequenz.
        /// </summary>
        public readonly uint Frequency;

        /// <summary>
        /// Die Symbolrate.
        /// </summary>
        public readonly uint SymbolRate;

        /// <summary>
        /// Die Position des Satelliten.
        /// </summary>
        public readonly ushort OrbitalPosition;

        /// <summary>
        /// Die Position des Satelliten.
        /// </summary>
        public readonly bool West;

        /// <summary>
        /// Die Signalpolarisation.
        /// </summary>
        public readonly Polarizations Polarization;

        /// <summary>
        /// Die Modulation des Signals.
        /// </summary>
        public readonly byte Modulation;

        /// <summary>
        /// Die innere Fehlerkorrektur.
        /// </summary>
        public readonly InnerFECs InnerFEC;

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="container">Die Sammlung aller Beschreibungen.</param>
        /// <param name="offset">Das erste Byte in den Rohdaten zu dieser Beschreibung.</param>
        /// <param name="length">Die Größe der Beschreibung in den Rohdaten.</param>
        public SatelliteDelivery( IDescriptorContainer container, int offset, int length )
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
            ushort orb0 = (ushort) Tools.FromBCD( section[offset + 4] );
            ushort orb1 = (ushort) Tools.FromBCD( section[offset + 5] );
            uint rate0 = (uint) Tools.FromBCD( section[offset + 7] );
            uint rate1 = (uint) Tools.FromBCD( section[offset + 8] );
            uint rate2 = (uint) Tools.FromBCD( section[offset + 9] );
            uint rate3 = (uint) Tools.FromBCD( (byte) ((section[offset + 10] >> 4) & 0xf) );

            // Flags
            byte flags = section[offset + 6];

            // Load all
            Frequency = (freq3 + 100 * (freq2 + 100 * (freq1 + 100 * freq0))) * 10;
            SymbolRate = (rate3 + 10 * (rate2 + 100 * (rate1 + 100 * rate0))) / 10;
            Polarization = (Polarizations) ((flags >> 5) & 0x03);
            InnerFEC = (InnerFECs) (section[offset + 10] & 0x0f);
            OrbitalPosition = (ushort) (orb1 + 100 * orb0);
            West = (0x00 == (0x80 & flags));
            Modulation = (byte) (flags & 0x1f);

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
            return (DescriptorTags.SatelliteDeliverySystem == (DescriptorTags) tag);
        }

        /// <summary>
        /// Wandelt eine Frequenzangabe aus einer <see cref="FrequencyList"/> in eine echte Frequenz um.
        /// </summary>
        /// <param name="frequency">Die Rohdaten der Frequenz.</param>
        /// <returns>Die Frequenz in Hz.</returns>
        internal static ulong ConvertFrequency( uint frequency )
        {
            // Coding same as cable - only interpretation is different
            return 100 * CableDelivery.ConvertFrequency( frequency );
        }
    }
}
