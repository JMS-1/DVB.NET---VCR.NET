using System;
using System.Text;
using System.Collections.Generic;


namespace JMS.DVB.TS.VideoText
{
    /// <summary>
    /// Klasse zur Analyse eines Videotextdatenstroms.
    /// </summary>
    public class TTXParser
    {
        /// <summary>
        /// Die Hamming 8/4 Tabelle. Jedem Byte ist der entsprechende Datenwert von
        /// 0 bis 15 zugeordnet. Fehler werden mit 255 markiert.
        /// </summary>
        private static byte[] Hamming8To4 = new byte[256];

        /// <summary>
        /// Die 7-Bit Parity Prüftabelle. Jedem Byte ist der entsprechende Datenwert von
        /// 0 bis 127 zugeordnet. Fehler werden mit 255 markiert.
        /// </summary>
        private static byte[] OddParity = new byte[256];

        /// <summary>
        /// Rückrufmethode für erkannte Seiten.
        /// </summary>
        /// <param name="page">Daten zur erkannten Seite.</param>
        public delegate void PageHandler( TTXPage page );

        /// <summary>
        /// Rückrufmethode zur Bearbeitung eines Paketes mit Videotextdaten.
        /// </summary>
        /// <param name="offset">Erstes zu analysierendes Byte.</param>
        /// <param name="length">Anzahl der zu analysierenden Bytes.</param>
        private delegate void DataTypeHandler( int offset, int length );

        /// <summary>
        /// Verarbeitet eine einzelne Videotextzeile im EBU Format.
        /// </summary>
        /// <param name="offset">Erstes Byte der Rohdaten.</param>
        /// <param name="length">Rohdaten für diese eine Zeile.</param>
        /// <returns>Gesetzt, wenn die Rohdaten zulässig sind.</returns>
        private delegate bool EBULineHandler( int offset, int length );

        /// <summary>
        /// Ereignis, das bei vollständige Analyse einer Videotext Seite aufgerufen wird.
        /// </summary>
        public event PageHandler OnPage;

        /// <summary>
        /// Gesetzt, wenn auf den ersten PES Kopf gewartet wird.
        /// </summary>
        private bool m_Synchronize = true;

        /// <summary>
        /// Aktueller Zwischenspeicher für eine Size.
        /// </summary>
        private byte[] m_PacketBuffer = new byte[0];

        /// <summary>
        /// Aktueller Füllgrad des Zwischenspeichers.
        /// </summary>
        private int m_CurrentSize = 0;

        /// <summary>
        /// Zähle, wie oft das Datenpaket kleiner als ein PES Kopf war.
        /// </summary>
        private long m_TooSmall = 0;

        /// <summary>
        /// Zählt, wie oft die Längenangabe im PES Kopf fehlerhaft war.
        /// </summary>
        private long m_BadPayloadLength = 0;

        /// <summary>
        /// Zählt, wie oft die Längenangabe für den PES Kopf fehlerhaft war.
        /// </summary>
        private long m_BadHeaderLength = 0;

        /// <summary>
        /// Zählt, wie oft der Typ der Videotext Daten nicht erkannt wurde.
        /// </summary>
        private long m_UnknownDataType = 0;

        /// <summary>
        /// Zähle, wie oft die Daten einer Videotextzeile die falsche Länge haben.
        /// </summary>
        private long m_BadLineLength = 0;

        /// <summary>
        /// Zähle, wie oft eine EBU Zeile mit einem unbekannten Code nicht ausgewertet werden konnte.
        /// </summary>
        private long m_BadEBULineType = 0;

        /// <summary>
        /// Zählt ungültige Prüfbytes.
        /// </summary>
        private long m_EBULineInvalid = 0;

        /// <summary>
        /// Zählt ungültige Zeilennummern.
        /// </summary>
        private long m_BadLineNumber = 0;

        /// <summary>
        /// Zähle die Übertragungsfehler.
        /// </summary>
        private long m_ParityError = 0;

        /// <summary>
        /// Zeitstempel bei Beginn des letzten Paketes.
        /// </summary>
        private long m_LastPTS = -1;

        /// <summary>
        /// Analysemethoden für alle bekannten Videotext Formate.
        /// </summary>
        private Dictionary<byte, DataTypeHandler> m_TypeHandlers = new Dictionary<byte, DataTypeHandler>();

        /// <summary>
        /// Analysemethoden für alle bekannten EBU Zeilenformate.
        /// </summary>
        private Dictionary<byte, EBULineHandler> m_EBULineHandler = new Dictionary<byte, EBULineHandler>();

        /// <summary>
        /// Muss gesetzt sein, damit Pakete ausgewertet werden.
        /// </summary>
        private bool m_Enabled = false;

        /// <summary>
        /// Die aktuell zusammengestellte Seite.
        /// </summary>
        private TTXPage[] m_Page = new TTXPage[8];

        /// <summary>
        /// Initialisiert die diversen Tabellen.
        /// </summary>
        static TTXParser()
        {
            // Invalidate Hamming 8/4 table
            for (int i = Hamming8To4.Length; i-- > 0; )
                Hamming8To4[i] = 255;

            // Fill Hamming 8/4 table
            for (int D1 = 2; D1-- > 0; )
                for (int D2 = 2; D2-- > 0; )
                    for (int D3 = 2; D3-- > 0; )
                        for (int D4 = 2; D4-- > 0; )
                        {
                            // Calculate parity bits
                            int P1 = 1 ^ D1 ^ D3 ^ D4;
                            int P2 = 1 ^ D1 ^ D2 ^ D4;
                            int P3 = 1 ^ D1 ^ D2 ^ D3;
                            int P4 = 1 ^ P1 ^ D1 ^ P2 ^ D2 ^ P3 ^ D3 ^ D4;

                            // Create the hamming code
                            int hamming = (P1 << 7) | (D1 << 6) | (P2 << 5) | (D2 << 4) | (P3 << 3) | (D3 << 2) | (P4 << 1) | D4;

                            // Create the related data - least significant bit comes first
                            int data = (D4 << 3) | (D3 << 2) | (D2 << 1) | D1;

                            // Store
                            Hamming8To4[hamming] = (byte) data;
                        }

            // Invalidate parity table
            for (int i = OddParity.Length; i-- > 0; )
                OddParity[i] = 255;

            // Fill odd parity table
            for (int D1 = 2; D1-- > 0; )
                for (int D2 = 2; D2-- > 0; )
                    for (int D3 = 2; D3-- > 0; )
                        for (int D4 = 2; D4-- > 0; )
                            for (int D5 = 2; D5-- > 0; )
                                for (int D6 = 2; D6-- > 0; )
                                    for (int D7 = 2; D7-- > 0; )
                                    {
                                        // Calculate the parity but
                                        int P = 1 ^ D1 ^ D2 ^ D3 ^ D4 ^ D5 ^ D6 ^ D7;

                                        // Create the protected code
                                        int odd = (D1 << 7) | (D2 << 6) | (D3 << 5) | (D4 << 4) | (D5 << 3) | (D6 << 2) | (D7 << 1) | P;

                                        // Create the related data - least significat bit comes first
                                        int data = (D7 << 6) | (D6 << 5) | (D5 << 4) | (D4 << 3) | (D3 << 2) | (D2 << 1) | D1;

                                        // Store
                                        OddParity[odd] = (byte) data;
                                    }
        }

        /// <summary>
        /// Erzeugt eine neue Analyseinstanz.
        /// </summary>
        public TTXParser()
        {
            // Install data type handlers
            for (byte i = 0x10; i <= 0x1f; ++i)
                m_TypeHandlers[i] = ProcessEBUOnly;

            // Install EBU line handlers
            m_EBULineHandler[0x02] = ( offset, length ) => ProcessEBUText( offset, length, false );
            m_EBULineHandler[0x03] = ( offset, length ) => ProcessEBUText( offset, length, true );
            m_EBULineHandler[0xff] = SkipEBULine;
        }

        /// <summary>
        /// Fügt einen Auszug aus einem ES Videotext Datenstrom zur Analyse hinzu.
        /// </summary>
        /// <param name="startOfPacket">Gesetzt, wenn der PES Kopf am Anfang der Daten steht.</param>
        /// <param name="buf">Ein Puffer.</param>
        /// <param name="offset">Erstes zu nutzendes Byte im Puffer.</param>
        /// <param name="length">Anzahl der Bytes im Puffer.</param>
        /// <param name="pts">Aktueller Zeitstempel.</param>
        public void AddPayload( bool startOfPacket, byte[] buf, int offset, int length, long pts )
        {
            // Do nothing
            if (!m_Enabled)
                return;

            // Filling
            if (!m_Synchronize)
                if (startOfPacket)
                {
                    // Process last page
                    AnalysePage();

                    // Remember the current time stamp as the start of the next page
                    m_LastPTS = pts;
                }
                else
                {
                    // Just add data
                    AddToBuffer( buf, offset, length );

                    // Next
                    return;
                }

            // Wait for next packet header
            if (!startOfPacket)
                return;

            // Got it
            m_Synchronize = false;
            m_CurrentSize = 0;

            // Add to buffer
            AddToBuffer( buf, offset, length );
        }

        /// <summary>
        /// Fügt Daten zum Analysebuffer hinzu.
        /// </summary>
        /// <param name="buf">Videotext Rohdaten.</param>
        /// <param name="offset">Erstes nutzbares Byte.</param>
        /// <param name="length">Anzahl der nutzbaren Bytes.</param>
        private void AddToBuffer( byte[] buf, int offset, int length )
        {
            // Get the new buffer size
            int bufferSize = m_CurrentSize + length;

            // Must reallocate
            if (bufferSize > m_PacketBuffer.Length)
                Array.Resize( ref m_PacketBuffer, bufferSize );

            // Fill in
            Array.Copy( buf, offset, m_PacketBuffer, m_CurrentSize, length );

            // Advance
            m_CurrentSize += length;
        }

        /// <summary>
        /// Analysiert die aktuelle Videotext Seite.
        /// </summary>
        private void AnalysePage()
        {
            // Check overall size
            if (m_CurrentSize < 9)
            {
                // No header at all
                ++m_TooSmall;

                // Re-synchronize
                return;
            }

            // Read the length of the packet
            int len = JMS.DVB.EPG.Tools.MergeBytesToWord( m_PacketBuffer[5], m_PacketBuffer[4] );

            // Must fit
            if (m_CurrentSize != (6 + len))
            {
                // Wrong PES length
                ++m_BadPayloadLength;

                // Resynchronize
                return;
            }

            // Read the length of the header extra data
            byte extLen = m_PacketBuffer[8];

            // Get the full header length
            int header = 9 + extLen;

            // Must fit
            if (header > m_CurrentSize)
            {
                // Wrong header extension length
                ++m_BadHeaderLength;

                // Resynchronize
                return;
            }

            // Get the payload length
            int payload = m_CurrentSize - header;

            // None
            if (payload < 1)
                return;

            // Read the data identifier
            byte type = m_PacketBuffer[header];

            // Load the handler
            DataTypeHandler handler;
            if (!m_TypeHandlers.TryGetValue( type, out handler ))
            {
                // Unknown type
                ++m_UnknownDataType;

                // Resynchronize
                return;
            }

            // Dispatch
            handler( header + 1, payload - 1 );
        }

        /// <summary>
        /// Verarbeitet Videotextdaten im EBU Format.
        /// </summary>
        /// <param name="offset">Erstes zu bearbeitendes Byte.</param>
        /// <param name="length">Anzahl der zu analysierenden Bytes.</param>
        private void ProcessEBUOnly( int offset, int length )
        {
            // Process
            while (length > 0)
            {
                // Must have header bytes
                if (length < 2)
                {
                    // Bad header
                    ++m_BadLineLength;

                    // Resynchronize
                    return;
                }

                // Get type and length
                byte lineType = m_PacketBuffer[offset + 0];
                int lineSize = m_PacketBuffer[offset + 1];

                // Correct header
                offset += 2;
                length -= 2;

                // Correct length
                length -= lineSize;

                // In error
                if (length < 0)
                {
                    // Bad header
                    ++m_BadLineLength;

                    // Resynchronize
                    return;
                }

                // Get the handler
                EBULineHandler handler;
                if (m_EBULineHandler.TryGetValue( lineType, out handler ))
                {
                    // Proces
                    if (!handler( offset, lineSize ))
                        return;
                }
                else
                {
                    // Count but continue
                    ++m_BadEBULineType;
                }

                // Finally correct the offset
                offset += lineSize;
            }
        }

        /// <summary>
        /// Verarbeitet eine Zeile im EBU Teletext Format.
        /// </summary>
        /// <param name="offset">Erstes zu analysierendes Bytes.</param>
        /// <param name="length">Anzahl der zu analysierenden Bytes.</param>
        /// <param name="subTitle">Gesetzt für Seiten, die Untertitel enthalten.</param>
        /// <returns>Gesetzt, wenn die Rohdaten zulässig sind.</returns>
        private bool ProcessEBUText( int offset, int length, bool subTitle )
        {
            // Test for total length - fixed to 44 Bytes
            if (44 != length)
            {
                // Bad header
                ++m_BadLineLength;

                // Resynchronize
                return false;
            }

            // Read first bytes
            byte lineByte = m_PacketBuffer[offset++], syncByte = m_PacketBuffer[offset++];

            // Validate
            if ((0xc0 != (lineByte & 0xc0)) || (0xe4 != syncByte))
            {
                // Count
                ++m_EBULineInvalid;

                // Stop this page
                return false;
            }

            // Extract
            bool parity = (0 != (0x20 & lineByte));
            int line = (lineByte & 0x1f);

            // Validate line
            if (0 != line)
                if ((line < 7) || (line > 22))
                {
                    // Count
                    ++m_BadLineNumber;

                    // Stop this page
                    return false;
                }

            // Read the packet address
            int magRaw = Hamming8To4[m_PacketBuffer[offset++]], packRaw = Hamming8To4[m_PacketBuffer[offset++]];

            // Check failed
            if ((255 == magRaw) || (255 == packRaw))
            {
                // Count
                ++m_ParityError;

                // Skip line
                return false;
            }

            // Calculate magazine and packet
            int magazine = magRaw & 0x7, packet = (magRaw >> 3) | (packRaw << 1);

            // Time to correct data length
            length -= 4;

            // Check packet 
            if (packet == 0)
            {
                // Header
                return ProcessEBUPageHeader( offset, length, magazine, subTitle );
            }
            else if (packet < 26)
            {
                // Display
                return ProcessEBUDisplayData( packet, offset, length, magazine );
            }
            else
            {
                // Control
                return ProcessEBUNonDisplayData( packet, offset, length, magazine );
            }
        }

        /// <summary>
        /// Wertet darstellbare Daten aus (Y/1 bis Y/25).
        /// </summary>
        /// <param name="packet">Paketkennung (26 bis 31).</param>
        /// <param name="offset">Erstes auszuwertendes Byte.</param>
        /// <param name="length">Anzahl der auszuwertenden Bytes.</param>
        /// <param name="magazine">Magazinkennung (X oder M).</param>
        /// <returns>Gesetzt, wenn die Daten zulässig waren.</returns>
        private bool ProcessEBUDisplayData( int packet, int offset, int length, int magazine )
        {
            // Get the display data
            var data = ReadOddParityBytes( offset, length );
            if (data == null)
                return false;

            // Add to page if page is active
            if (m_Page[magazine] != null)
                m_Page[magazine][packet] = data;

            // Done
            return true;
        }

        /// <summary>
        /// Wertet den Videotext Kopf aus (Y/0).
        /// </summary>
        /// <param name="offset">Erstes auszuwertendes Byte.</param>
        /// <param name="length">Anzahl der auszuwertenden Bytes.</param>
        /// <param name="magazine">Magazinkennung (X oder M).</param>
        /// <param name="subTitle">Gesetzt für Untertitelseiten.</param>
        /// <returns>Gesetzt, wenn die Daten zulässig waren.</returns>
        private bool ProcessEBUPageHeader( int offset, int length, int magazine, bool subTitle )
        {
            // Finish any outstanding page
            if (m_Page[magazine] != null)
            {
                // Send notification
                var handler = OnPage;
                if (handler != null)
                    handler( m_Page[magazine] );

                // Forget
                m_Page[magazine] = null;
            }

            // Extract the pager number
            int units = Hamming8To4[m_PacketBuffer[offset++]], tens = Hamming8To4[m_PacketBuffer[offset++]];

            // Check it
            if ((255 == units) || (255 == tens))
            {
                // Count
                ++m_ParityError;

                // Leave
                return false;
            }

            // Get the page number
            int page = (10 * tens) + units;

            // Process only if this is a regular page (100 - 899)
            if (page > 99)
            {
                // Silent skip
                return true;
            }

            // Read the rest
            int s1 = Hamming8To4[m_PacketBuffer[offset++]];
            int s2c4 = Hamming8To4[m_PacketBuffer[offset++]];
            int s3 = Hamming8To4[m_PacketBuffer[offset++]];
            int s4c5c6 = Hamming8To4[m_PacketBuffer[offset++]];
            int c7To10 = Hamming8To4[m_PacketBuffer[offset++]];
            int c11To14 = Hamming8To4[m_PacketBuffer[offset++]];

            // Verify all
            if ((255 == s1) || (255 == s2c4) || (255 == s3) || (255 == s4c5c6) || (255 == c7To10) || (255 == c11To14))
            {
                // Count
                ++m_ParityError;

                // Leave
                return false;
            }

            // Can adjust length now
            length -= 8;

            // Extract sub code
            int subcode = ((s4c5c6 & 0x3) * 1000) + (s3 * 100) + ((s2c4 & 0x7) * 10) + s1;

            // Read data
            byte[] data = ReadOddParityBytes( offset, length );
            if (data == null)
                return false;

            // Create the new page
            m_Page[magazine] =
                new TTXPage( magazine, page, subcode, data )
                {
                    IsSubtitle = subTitle || (0 != (s4c5c6 & 0x08)),
                    MagazineSerial = (0 != (c11To14 & 0x01)),
                    OutOfSequence = (0 != (c7To10 & 0x04)),
                    NoHeader = (0 != (c7To10 & 0x01)),
                    NationalOptions = (c11To14 >> 1),
                    Updated = (0 != (c7To10 & 0x02)),
                    Flash = (0 != (s4c5c6 & 0x04)),
                    Hide = (0 != (c7To10 & 0x08)),
                    Erase = (0 != (s2c4 & 0x08)),
                    TimeStamp = m_LastPTS,
                };

            // Done
            return true;
        }

        /// <summary>
        /// Ermittelt aus den Daten einer Videotextzeile eine Darstellung zu Debugzwecken.
        /// </summary>
        /// <param name="data">Rohdaten der Zeile.</param>
        /// <returns>Anzeige zum Testen.</returns>
        public static string GetDebugString( byte[] data )
        {
            // Convert and update
            return Encoding.GetEncoding( 1252 ).GetString( data )
                .Replace( "\x00", "<black>" )
                .Replace( "\x01", "<red>" )
                .Replace( "\x02", "<green>" )
                .Replace( "\x03", "<yellow>" )
                .Replace( "\x04", "<blue>" )
                .Replace( "\x05", "<magenta>" )
                .Replace( "\x06", "<cyan>" )
                .Replace( "\x07", "<white>" )
                .Replace( "\x08", "<FLASH>" )
                .Replace( "\x09", "<STEADY>" )
                .Replace( "\x0a", "<ENDBOX>" )
                .Replace( "\x0b", "<STARTBOX>" )
                .Replace( "\x0c", "<NORMALSIZE>" )
                .Replace( "\x0d", "<DOUBLEHEIGHT>" )
                .Replace( "\x0e", "<DOUBLEWIDTH>" )
                .Replace( "\x0f", "<DOUBLESIZE>" )
                .Replace( "\x10", "*<black>" )
                .Replace( "\x11", "*<red>" )
                .Replace( "\x12", "*<green>" )
                .Replace( "\x13", "*<yellow>" )
                .Replace( "\x14", "*<blue>" )
                .Replace( "\x15", "*<magenta>" )
                .Replace( "\x16", "*<cyan>" )
                .Replace( "\x17", "*<white>" )
                .Replace( "\x18", "*<CONCEAL>" )
                .Replace( "\x19", "*<CONTINGOUS>" )
                .Replace( "\x1a", "*<SEPARATED>" )
                .Replace( "\x1b", "*<ESC>" )
                .Replace( "\x1c", "*<BLACKBACK>" )
                .Replace( "\x1d", "*<NEWBACK>" )
                .Replace( "\x1e", "*<HOLD>" )
                .Replace( "\x1f", "*<RELEASE>" );
        }

        /// <summary>
        /// Ermittelt eine Sequenz von Zeichen.
        /// </summary>
        /// <param name="offset">Erstes Byte.</param>
        /// <param name="length">Anzahl der Bytes.</param>
        /// <returns>Die gewünschten Zeichen oder <i>null</i>, wenn ein Fehler erkannt wurde.</returns>
        private byte[] ReadOddParityBytes( int offset, int length )
        {
            // Create
            var result = new byte[length];

            // Fill
            for (int i = 0; i < length; ++i)
            {
                // Load
                var data = OddParity[m_PacketBuffer[offset++]];

                // Test
                if (255 == data)
                {
                    // Count
                    ++m_ParityError;

                    // Done
                    return null;
                }

                // Remember
                result[i] = data;
            }

            // Done
            return result;
        }

        /// <summary>
        /// Wertet nicht darstellbare Daten aus (Y/26 bis Y/31).
        /// </summary>
        /// <param name="packet">Paketkennung (26 bis 31).</param>
        /// <param name="offset">Erstes auszuwertendes Byte.</param>
        /// <param name="length">Anzahl der auszuwertenden Bytes.</param>
        /// <param name="magazine">Magazinkennung (X oder M).</param>
        /// <returns>Gesetzt, wenn die Daten zulässig waren.</returns>
        private bool ProcessEBUNonDisplayData( int packet, int offset, int length, int magazine )
        {
            // Done
            return true;
        }

        /// <summary>
        /// Überspringt Fülldaten.
        /// </summary>
        /// <param name="offset">Erstes zu analysierendes Bytes.</param>
        /// <param name="length">Anzahl der zu analysierenden Bytes.</param>
        /// <returns>Immer gesetzt.</returns>
        private bool SkipEBULine( int offset, int length )
        {
            // Allow
            return true;
        }

        /// <summary>
        /// Liest oder setzt ob Pakete analyisiert werden sollen.
        /// </summary>
        public bool EnableParser
        {
            get
            {
                // Report
                return m_Enabled;
            }
            set
            {
                // When processing will be enabled resynchronize
                if (value)
                    if (!m_Enabled)
                        m_Synchronize = true;

                // Change
                m_Enabled = value;
            }
        }
    }
}
