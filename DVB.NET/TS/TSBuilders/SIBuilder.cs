using System;

namespace JMS.DVB.TS.TSBuilders
{
    /// <summary>
    /// Hilfsklasse zur Rekonstruktion von SI Tabellen aus dem Rohdatenstrom.
    /// </summary>
    public class SIBuilder : TSBuilder
    {
        /// <summary>
        /// Zwischenspeicher zum Zusammenbau der Tabellen.
        /// </summary>
        private byte[] m_Collector = null;

        /// <summary>
        /// Aktueller Füllstand des Zwischenspeichers.
        /// </summary>
        private int m_CollectorPos = 0;

        /// <summary>
        /// Aktueller Paketzähler.
        /// </summary>
        private byte m_Counter = 0;

        /// <summary>
        /// Erzeugt eine neue Rekonstruktionsinstanz.
        /// </summary>
        /// <param name="parser">Die zugehörige Analyseeinheit.</param>
        /// <param name="callback">Eine Methode, an die alle vollständig rekonstruierten Pakete gemeldet werden.</param>
        public SIBuilder( TSParser parser, Action<byte[]> callback )
            : base( parser, callback )
        {
        }

        /// <summary>
        /// Setzt alle Zwischenspeicher auf den Anfangsstand zurück.
        /// </summary>
        public override void Reset()
        {
            // Force restart
            m_Collector = null;
        }

        /// <summary>
        /// Wertet ein elementares Paket aus.
        /// </summary>
        /// <param name="packet">Ein Zwischenspeicher mit den Paketdaten.</param>
        /// <param name="offset">Die Position des ersten zu analysierenden Bytes im Zwischenspeicher.</param>
        /// <param name="length">Die Anzahl der zu analysierenden Bytes.</param>
        /// <param name="noincrement">Gesetzt, wenn der Paketzähler nicht verändert werden darf.</param>
        /// <param name="first">Gesetzt, wenn die zu analyiserenden Daten den Tabellekopf enthalten.</param>
        /// <param name="counter">Der aktuelle Paketzähler.</param>
        public override void AddPacket( byte[] packet, int offset, int length, bool noincrement, bool first, byte counter )
        {
            // See if we are awaiting the first packet
            if ((null == m_Collector) || first)
            {
                // Not the start
                if (!first)
                    return;

                // Validate
                if (length < 1)
                {
                    // Report
                    Parser.TableCorrupted();

                    // Reset
                    m_Collector = null;

                    // Next
                    return;
                }

                // Read the pointer field
                int pointer = packet[offset];

                // Adjust
                offset += ++pointer;
                length -= pointer;

                // See if we have to finish a previous package
                if (--pointer > 0)
                    if (length >= 0)
                        if (m_Collector != null)
                            if ((m_Collector.Length - m_CollectorPos) == pointer)
                            {
                                // Fill
                                Array.Copy( packet, offset - pointer, m_Collector, m_CollectorPos, pointer );

                                // Send
                                Process( m_Collector );
                            }

                // Validate
                if (length < 3)
                {
                    // Report
                    Parser.TableCorrupted();

                    // Wait for next
                    m_Collector = null;

                    // Next
                    return;
                }

                // Get the section length
                int lenh = 0xf & packet[offset + 1];
                int lenl = packet[offset + 2];

                // Overall length
                int total = lenl + 256 * lenh;

                // Crap
                if (total < 3)
                {
                    // Report
                    Parser.TableCorrupted();

                    // Reset
                    m_Collector = null;

                    // Done
                    return;
                }

                // Allocate
                m_Collector = new byte[3 + total];
                m_CollectorPos = 0;

                // Set counter
                m_Counter = counter;
            }

            // Match counter
            if (m_Counter == counter)
            {
                // Expect next to be one higher
                m_Counter = (byte) ((m_Counter + 1) & 0xf);

                // How may do we need
                int copy = Math.Min( m_Collector.Length - m_CollectorPos, length );

                // Fill in
                Array.Copy( packet, offset, m_Collector, m_CollectorPos, copy );

                // Adjust
                m_CollectorPos += length;

                // Not enough
                if (m_CollectorPos < m_Collector.Length)
                    return;

                // Process
                Process( m_Collector );
            }
            else
            {
                // Report
                Parser.TableCorrupted();
            }

            // Wait for next start
            m_Collector = null;
        }
    }
}
