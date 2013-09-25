using System;

namespace JMS.DVB.EPG.OpenTV
{
    /// <summary>
    /// Diese Klasse wird zur Dekomprimierung von Moduldaten verwendet.
    /// </summary>
    public class Decompressor
    {
        /// <summary>
        /// Größe der komprimierte Daten in Bytes.
        /// </summary>
        private uint Compressed;

        /// <summary>
        /// Größe der unkomprimierten Daten in Bytes.
        /// </summary>
        private uint Uncompressed;

        /// <summary>
        /// Position der unkomprimierten Rohdaten.
        /// </summary>
        private uint FirstUncompressed;

        /// <summary>
        /// Speicher zur Aufnahme der unkomprimierten Daten.
        /// </summary>
        private byte[] Buffer;

        /// <summary>
        /// Ursprüngliche Daten.
        /// </summary>
        private byte[] RawData;

        /// <summary>
        /// Zielindex der Daten.
        /// </summary>
        private uint OutputPosition;

        /// <summary>
        /// Leseindex der Rohdaten.
        /// </summary>
        private uint InputPosition;

        /// <summary>
        /// Erstes Byte, dass nicht mehr komprimiert ist.
        /// </summary>
        private uint InputPositionLimit;

        /// <summary>
        /// Gesetzt, wenn die aktuelle Position in der Mitte eines Bytes beginnt.
        /// </summary>
        private bool HalfByteMode;

        /// <summary>
        /// Erzeugt eine neue Dekomprimierungsinstanz.
        /// </summary>
        public Decompressor()
        {
        }

        /// <summary>
        /// Beendet eine Dekomprimierung.
        /// </summary>
        /// <returns>Der Datenblock mit den dekomprimierten Inhalt.</returns>
        public byte[] FinishDecompression()
        {
            // Copy over
            Array.Copy( RawData, InputPosition - 1, Buffer, OutputPosition, Buffer.Length - OutputPosition );

            // Did it
            return Buffer;
        }

        /// <summary>
        /// Beginnt die Dekomprimierung eines Datenblocks.
        /// </summary>
        /// <param name="compressed">Der komprimierte Block.</param>
        /// <returns>Gesetzt, wenn eine gültige Komprimierung vorliegt, <i>null</i>, 
        /// wenn eine Komprimierung zwar vorliegt, diese aber fehlerhaft ist..</returns>
        public bool? StartDecompression( byte[] compressed )
        {
            // Forward
            return StartDecompression( compressed, 0, compressed.Length );
        }

        /// <summary>
        /// Beginnt die Dekomprimierung eines Datenblocks.
        /// </summary>
        /// <param name="compressed">Der komprimierte Block.</param>
        /// <param name="offset">Das erste zu verwendende Bytes.</param>
        /// <param name="length">Die Anzahl der zu verwendenden Bytes.</param>
        /// <returns>Gesetzt, wenn eine gültige Komprimierung vorliegt, <i>null</i>, 
        /// wenn eine Komprimierung zwar vorliegt, diese aber fehlerhaft ist..</returns>
        public bool? StartDecompression( byte[] compressed, int offset, int length )
        {
            // Find compression marker
            for (; length >= 17; ++offset, --length)
                if ('C' == compressed[offset + 0])
                    if ('O' == compressed[offset + 1])
                        if ('M' == compressed[offset + 2])
                            if ('P' == compressed[offset + 3])
                                break;

            // Not possible
            if (length < 17) return false;

            // Read parameters
            Compressed = Tools.MergeBytesToDoubleWord( compressed[offset + 7], compressed[offset + 6], compressed[offset + 5], compressed[offset + 4] );
            Uncompressed = Tools.MergeBytesToDoubleWord( compressed[offset + 11], compressed[offset + 10], compressed[offset + 9], compressed[offset + 8] );
            FirstUncompressed = Tools.MergeBytesToDoubleWord( compressed[offset + 15], compressed[offset + 14], compressed[offset + 13], compressed[offset + 12] );

            // See if the input buffer is large enough
            if ((Compressed > length) || (Compressed < 17)) return null;

            // Get positions
            InputPosition = (uint) (offset + 17);
            InputPositionLimit = (uint) (offset + Compressed);

            // Create new buffer
            try
            {
                // Allocate
                Buffer = new byte[Uncompressed];
            }
            catch
            {
                // Report
                return null;
            }

            // Reset
            RawData = compressed;
            HalfByteMode = false;
            OutputPosition = 0;

            // Ready to take off
            return true;
        }

        /// <summary>
        /// Schreibt ein Byte in das Zielfeld.
        /// </summary>
        /// <param name="data">Das gewünschte Byte.</param>
        /// <returns>Gesetzt, wenn die Operation möglich war.</returns>
        private bool WriteByte( uint data )
        {
            // Validate
            if (OutputPosition >= FirstUncompressed) return false;

            // Store
            Buffer[OutputPosition++] = (byte) data;

            // Dit it
            return true;
        }

        /// <summary>
        /// Ermittelt das nächste Byte aus den Eingangsdaten.
        /// </summary>
        /// <param name="data">Wird mit dem nächsten Bytewert gefüllt.</param>
        /// <returns>Gesetzt, wenn ein Wert ermittelt werden konnte.</returns>
        private bool ReadByte( ref uint data )
        {
            // Beyond the edge
            if (InputPosition >= InputPositionLimit) return false;

            // Get the byte and advance
            data = RawData[InputPosition++];

            // Full byte access mode
            if (!HalfByteMode) return true;

            // Read the other half
            uint prev = RawData[InputPosition - 2];

            // Merge together
            data = (data & 0xf0) | (prev & 0x0f);

            // Done
            return true;
        }

        /// <summary>
        /// Liest einen Offset.
        /// </summary>
        /// <param name="offset">Der gewünschte Offset.</param>
        /// <returns>Gesetzt, wenn der Lesevorgang möglich war.</returns>
        private bool ReadNibble( ref uint offset )
        {
            // See if we can access the full byte
            if (HalfByteMode)
            {
                // Get the byte
                offset = RawData[InputPosition - 1];

                // Adjust
                offset = offset & 0x0f;
            }
            else
            {
                // Beyond the edge
                if (InputPosition >= InputPositionLimit) return false;

                // Get the byte and advance
                offset = RawData[InputPosition++];

                // Adjust
                offset = offset >> 4;
            }

            // Swap mode
            HalfByteMode = !HalfByteMode;

            // Did it
            return true;
        }

        /// <summary>
        /// Dekomprimiert das nächste Datenpaket.
        /// </summary>
        /// <returns>Gesetzt, wenn ein weiterer Aufruf notwendig ist.</returns>
        public bool DecompressPacket()
        {
            // The mask byte
            uint opMask = 0;

            // Read the control byte
            if (!ReadByte( ref opMask )) return false;

            // Process the control byte
            for (int n = 8; n-- > 0; opMask *= 2)
            {
                // The control byte
                uint control = 0;

                // Read the control byte
                if (!ReadByte( ref control )) return false;

                // Simple copy operation
                if (0x80 == (opMask & 0x80))
                {
                    // Copy
                    if (!WriteByte( control )) return false;

                    // Next
                    continue;
                }

                // Split off parts
                uint offsetLow = (control & 0x0f), offsetHigh = (control & 0x10);

                // Bytes to copy
                uint copyCount;

                // Decoding patterns: copy instructions
                // Bit Pattern                                                      Bytes to Copy       Maximum Offset      Nibbles
                // 0111 1111 1 ooo oooo oooo oooo 1111 1111 cccc cccc cccc cccc     289 .. 65824        32727               12
                // 0111 1111 1 ooo oooo oooo oooo cccc cccc                         34 .. 288           32767               8
                // 0111 cccc c ooo oooo oooo oooo                                   3 .. 33             32767               6
                // 0 ccc oooo oooo oooo                                             3 .. 9              4095                4
                // 100 o oooo                                                       2                   31                  2
                // 101 o oooo oooo                                                  2                   511                 3
                // 1100 oooo                                                        3                   15                  2
                // 1101 oooo oooo                                                   3                   255                 3
                // 111 c oooo oooo                                                  4 .. 5              511                 3

                // Dispatch decompression modes
                if (control < 0x70)
                {
                    // Calculate
                    copyCount = 3 + (control >> 4);

                    // Additional offset bits
                    uint moreOffset = 0;

                    // Load additional offset
                    if (!ReadByte( ref moreOffset )) return false;

                    // Merge
                    offsetLow = moreOffset + 256 * offsetLow;
                }
                else if (control < 0x80)
                {
                    // Get byte count
                    copyCount = 3 + 2 * offsetLow;

                    // Read next offset
                    if (!ReadByte( ref offsetLow )) return false;

                    // Merge in
                    if (0 != (offsetLow & 0x80))
                    {
                        // One more
                        ++copyCount;

                        // Clip off flag
                        offsetLow &= 0x7f;
                    }

                    // Additional offset bits
                    uint moreOffset = 0;

                    // Read next
                    if (!ReadByte( ref moreOffset )) return false;

                    // Merge
                    offsetLow = moreOffset + 256 * offsetLow;

                    // Check for extra long copy
                    if (copyCount == (3 + 0x1f))
                    {
                        // Read next
                        if (!ReadByte( ref copyCount )) return false;

                        // Even more
                        if (0xff == copyCount)
                        {
                            // Read more
                            if (!ReadByte( ref copyCount )) return false;

                            // Read next
                            if (!ReadByte( ref moreOffset )) return false;

                            // Merge in upper bytes
                            copyCount = moreOffset + 256 * copyCount;

                            // Correct
                            copyCount += 3 + 0x1f + 0xff;
                        }
                        else
                        {
                            // Just correct
                            copyCount += (3 + 0x1f);
                        }
                    }
                }
                else if (control < 0xa0)
                {
                    // Fixed count
                    copyCount = 2;

                    // Use full offset
                    offsetLow = offsetLow + offsetHigh;
                }
                else if (control < 0xc0)
                {
                    // Fixed count
                    copyCount = 2;

                    // Additional offset
                    uint moreOffset = 0;

                    // Read it
                    if (!ReadNibble( ref moreOffset )) return false;

                    // Merge together
                    offsetLow = offsetLow + offsetHigh + 32 * moreOffset;
                }
                else if (control < 0xe0)
                {
                    // Fixed count
                    copyCount = 3;

                    // Check for offset extension
                    if (0 != offsetHigh)
                    {
                        // Additional offset
                        uint moreOffset = 0;

                        // Read it
                        if (!ReadNibble( ref moreOffset )) return false;

                        // Merge together
                        offsetLow = offsetLow + 16 * moreOffset;
                    }
                }
                else
                {
                    // Fixed count
                    copyCount = 4;

                    // Adjust count
                    if (0 != offsetHigh) ++copyCount;

                    // Additional offset
                    uint moreOffset = 0;

                    // Read it
                    if (!ReadNibble( ref moreOffset )) return false;

                    // Merge together
                    offsetLow = offsetLow + 16 * moreOffset;
                }

                // See if there is enough room
                if ((OutputPosition + copyCount) > FirstUncompressed) return false;

                // Correct position
                ++offsetLow;

                // See if this points inside the data
                if (offsetLow > OutputPosition) return false;

                // Clone area
                for (uint iSrc = OutputPosition - offsetLow; copyCount-- > 0; )
                {
                    // Copy over
                    Buffer[OutputPosition++] = Buffer[iSrc++];
                }
            }

            // Done
            return true;
        }
    }
}
