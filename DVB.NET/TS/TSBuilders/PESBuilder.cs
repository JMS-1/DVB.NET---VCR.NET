using System;

namespace JMS.DVB.TS.TSBuilders
{
    /// <summary>
    /// Hilfsklasse zur Rekonstruktion eines elementaren Nutzdatenstroms aus
    /// den Rohdaten eines <i>Transport Streams</i>.
    /// </summary>
    public class PESBuilder : TSBuilder
    {
        /// <summary>
        /// Zwischenspeicher zur Konstruktion von Paketen.
        /// </summary>
        private byte[] m_Buffer = null;

        /// <summary>
        /// Aktueller Füllstand des Zwischenspeichers.
        /// </summary>
        private int m_BufferPos = 0;

        /// <summary>
        /// Aktueller Paketzähler.
        /// </summary>
        private byte m_Counter = 0;

        /// <summary>
        /// Erzeugt eine neue Rekonstruktionsinstanz.
        /// </summary>
        /// <param name="parser">Die zugehörige Analyseeinheit.</param>
        /// <param name="callback">Eine Methode zum Empfang rekonstruierter Pakete.</param>
        public PESBuilder( TSParser parser, Action<byte[]> callback )
            : base( parser, callback )
        {
        }

        /// <summary>
        /// Setzt alle Zwischenspeicher auf den Anfangszustand zurück.
        /// </summary>
        public override void Reset()
        {
            // See if packet can be processed
            if (null != m_Buffer)
                if (m_BufferPos > 0)
                    Process( m_Buffer, 0, m_BufferPos );

            // Reset
            m_Buffer = null;
        }

        /// <summary>
        /// Wertet ein elementares Paket aus.
        /// </summary>
        /// <param name="packet">Zwischenspeicher mit den Paketdaten.</param>
        /// <param name="offset">Position des ersten zu rekonstruierenden Bytes im Zwischenspeicher.</param>
        /// <param name="length">Anzahl der zu rekonstruierenden Bytes.</param>
        /// <param name="noincrement">Gesetzt, wenn der Paketzähler nicht verändert werden darf.</param>
        /// <param name="first">Gesetzt, wenn es sich um das erste Paket einer Sequenz handelt.</param>
        /// <param name="counter">Der aktuelle Paketzähler.</param>
        public override void AddPacket( byte[] packet, int offset, int length, bool noincrement, bool first, byte counter )
        {
            // Start it all
            if (null == m_Buffer)
            {
                // Allocate and reset
                m_Buffer = new byte[8 * 1024];
                m_Counter = counter;
                m_BufferPos = 0;
            }

            // Correct PCR only
            if (noincrement)
                counter = (byte) ((counter + 1) & 0xf);

            // Counter
            if (m_Counter != counter)
            {
                // Report
                Parser.StreamCorrupted();

                // See if packet can be processed
                if (m_BufferPos > 0)
                    Process( m_Buffer, 0, m_BufferPos );

                // Restart
                m_Counter = counter;
                m_BufferPos = 0;
            }

            // Expect next to be one higher
            if (!noincrement)
                m_Counter = (byte) ((m_Counter + 1) & 0xf);

            // Short cut evaluation
            if (length < 1)
                return;

            // Get size
            int copy = Math.Min( m_Buffer.Length - m_BufferPos, length );

            // Fill
            Array.Copy( packet, offset, m_Buffer, m_BufferPos, copy );

            // Advance
            m_BufferPos += copy;

            // Not filled
            if (m_BufferPos < m_Buffer.Length)
                return;

            // Reset
            m_BufferPos = 0;

            // Increment
            offset += copy;
            length -= copy;

            // Process - force copy of buffer
            Process( m_Buffer, 0, m_Buffer.Length );

            // Nothing left
            if (length < 1)
                return;

            // Remember for next step
            Array.Copy( packet, offset, m_Buffer, 0, m_BufferPos = length );
        }
    }
}
