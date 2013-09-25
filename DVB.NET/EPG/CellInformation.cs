using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// Beschreibt eine DVB-T Zelle.
    /// </summary>
    public class CellInformation
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
        /// Der Längengrad.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Die Ausbreitung auf der Längenachse.
        /// </summary>
        public double LongitudeExtension { get; set; }

        /// <summary>
        /// Der Breitengrad.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Die Ausbreitung auf der Breitenachse.
        /// </summary>
        public double LatitudeExtension { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        private CellInformation()
        {
        }

        /// <summary>
        /// Skaliert einen Rohwert in eine Längen- oder Breitenangabe.
        /// </summary>
        /// <param name="number">Der Rohwert.</param>
        /// <param name="bits">Die Anzahl der Bits im Rohwert.</param>
        /// <param name="angle">Der volle Winkelbereich in Grad.</param>
        /// <returns>Das zugehörige Winkelmaß.</returns>
        private static double Rescale( int number, int bits, int angle )
        {
            // Get the center value
            int center = 1 << bits;

            // See if the value is negative
            if (number >= center)
                number = number - 2 * center;

            // Create result
            return number * angle / 32768.0;
        }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="section">Der SI Bereich, in dem die Information gefunden wurde.</param>
        /// <param name="offset">Der Index des ersten Bytes dieser Information in den Rohdaten des SI Bereichs.</param>
        /// <param name="length">Die Anzahl der Bytes für diese Information.</param>
        /// <returns>Die zugehörige Information oder <i>null</i>, wenn eine Rekonstruktion nicht möglich war.</returns>
        public static CellInformation Create( Section section, int offset, int length )
        {
            // Check minimum length
            if (length < 10)
                return null;

            // Correct length
            length -= 10;

            // Read length of extension data
            int extlen = section[offset + 9];

            // Validate
            if (extlen > length)
                return null;

            // Read direct data
            ushort cellId = Tools.MergeBytesToWord( section[offset + 1], section[offset + 0] );
            int latitude = Tools.MergeBytesToWord( section[offset + 3], section[offset + 2] );
            int longitude = Tools.MergeBytesToWord( section[offset + 5], section[offset + 4] );

            // Read to be merged data
            int ext0 = section[offset + 6];
            int ext1 = section[offset + 7];
            int ext2 = section[offset + 8];

            // Merge extends
            int latitude_ext = (ext1 >> 4) + 16 * ext0;
            int longitude_ext = ext2 + 256 * (ext1 & 0x0f);

            // Create new
            CellInformation info = new CellInformation 
            { 
                Length = 10 + extlen,
                Identifier = cellId,
                Latitude = Rescale(latitude, 16, 90),
                Longitude = Rescale(longitude, 16, 180),
                LatitudeExtension = Rescale(latitude_ext, 13, 90),
                LongitudeExtension = Rescale(longitude_ext, 13, 180)
            };

            // Report
            return info;
        }
    }
}
