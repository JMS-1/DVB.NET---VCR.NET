

namespace JMS.DVB.EPG
{
    /// <summary>
    /// Die Referenz auf eine Frequenz.
    /// </summary>
    public class FrequencyLink
    {
        /// <summary>
        /// Die Größe dieser Information in Bytes.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Die eindeutige Kennung der Zelle.
        /// </summary>
        public ushort Identifier { get; set; }

        /// <summary>
        /// Die Bezugsfrequenz.
        /// </summary>
        public ulong Frequency { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        private FrequencyLink()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="section">Der SI Bereich, in dem die Information gefunden wurde.</param>
        /// <param name="offset">Der Index des ersten Bytes dieser Information in den Rohdaten des SI Bereichs.</param>
        /// <param name="length">Die Anzahl der Bytes für diese Information.</param>
        /// <returns>Die zugehörige Information oder <i>null</i>, wenn eine Rekonstruktion nicht möglich war.</returns>
        public static FrequencyLink Create( Section section, int offset, int length )
        {
            // Check minimum length
            if (length < 7)
                return null;

            // Correct length
            length -= 7;

            // Read length of extension data
            int extlen = section[offset + 6];

            // Validate
            if (extlen > length)
                return null;

            // Read direct data
            ushort cellId = Tools.MergeBytesToWord( section[offset + 1], section[offset + 0] );
            uint frequency = Tools.MergeBytesToDoubleWord( section[offset + 5], section[offset + 4], section[offset + 3], section[offset + 2] );

            // Create new
            FrequencyLink info = new FrequencyLink
            {
                Length = 7 + extlen,
                Identifier = cellId,
                Frequency = Descriptors.TerrestrialDelivery.ConvertFrequency( frequency )
            };

            // Report
            return info;
        }
    }
}
