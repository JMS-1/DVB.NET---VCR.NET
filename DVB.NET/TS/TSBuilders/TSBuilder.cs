using System;

namespace JMS.DVB.TS.TSBuilders
{
    /// <summary>
    /// Basisklasse für Rekonstuktionsalgorithmen auf einem <i>Transport Stream</i>.
    /// </summary>
    public abstract class TSBuilder : IDisposable
    {
        /// <summary>
        /// Vermerkt die Größe des kleinsten an den Verbraucher weitergereichten Paketes.
        /// </summary>
        private int m_MinPacket = int.MaxValue;

        /// <summary>
        /// Vermerkt die Größe des größten an den Verbraucher weitergereichten Paketes.
        /// </summary>
        private int m_MaxPacket = int.MinValue;

        /// <summary>
        /// Ein optionaler Verbraucher für rekonstruierte Pakete.
        /// </summary>
        private FilterHandler m_Callback;

        /// <summary>
        /// Die Anzahl der weitergereichten Pakete.
        /// </summary>
        private long m_Packets = 0;

        /// <summary>
        /// Die zugehörige Analyseeinheit.
        /// </summary>
        private TSParser m_Parser;

        /// <summary>
        /// Die insgesamt weitergereichten Bytes.
        /// </summary>
        private long m_Bytes = 0;

        /// <summary>
        /// Initialisiert die Rekonstruktionsinstanz.
        /// </summary>
        /// <param name="parser">Die zugehörige Analyseeinheit.</param>
        /// <param name="callback">Optional ein Verbraucher für rekonstruierte Pakete.</param>
        protected TSBuilder( TSParser parser, FilterHandler callback )
        {
            // Remember
            m_Callback = callback;
            m_Parser = parser;
        }

        /// <summary>
        /// Meldet die Größe des kleinsten an den Verbraucher gemeldeten Paketes.
        /// </summary>
        public int MinimumPacketSize
        {
            get
            {
                // Report
                return (int.MaxValue == m_MinPacket) ? 0 : m_MinPacket;
            }
        }

        /// <summary>
        /// Meldet die Größe des größten an den Verbraucher gemeldeten Paketes.
        /// </summary>
        public int MaximumPacketSize
        {
            get
            {
                // Report
                return (int.MinValue == m_MaxPacket) ? 0 : m_MaxPacket;
            }
        }

        /// <summary>
        /// Meldet die Anzahl der an den Verbraucher durchgereichten Pakete.
        /// </summary>
        public long PacketCount
        {
            get
            {
                // Report
                return m_Packets;
            }
        }

        /// <summary>
        /// Meldet die gesamte Anzahl der an den Verbraucher durchgereichten Bytes.
        /// </summary>
        public long TotalBytes
        {
            get
            {
                // Report
                return m_Bytes;
            }
        }

        /// <summary>
        /// Meldet die zugehörige Analyseeinheit.
        /// </summary>
        protected TSParser Parser
        {
            get
            {
                // Report
                return m_Parser;
            }
        }

        /// <summary>
        /// Überträgt ein elementares Paket von der Analyseeinheit zur Rekonstruktion.
        /// </summary>
        /// <param name="packet">Ein Zwischenspeicher mit den Paketdaten.</param>
        /// <param name="offset">Die Position des ersten relevanten Bytes im Zwischenspeicher.</param>
        /// <param name="length">Die Anzahl der relevanten Bytes im Zwischenspeicher.</param>
        /// <param name="noincrement">Gesetzt, wenn der Paketzähler nicht erhöht werden darf.</param>
        /// <param name="first">Gesetzt, wenn das elementare Paket als Start einer Sequenz von Paketen gekennzeichnet ist.</param>
        /// <param name="counter">Der aktuelle Paketzähler.</param>
        public abstract void AddPacket( byte[] packet, int offset, int length, bool noincrement, bool first, byte counter );

        /// <summary>
        /// Fordert zum Zurücksetzen aller Zwischenergebnisse auf.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Sendet ein rekonstruiertes Paket an den zugeordneten Verbraucher.
        /// </summary>
        /// <param name="buffer">Das rekonstruierte Paket.</param>
        protected void Process( byte[] buffer )
        {
            // Counter
            m_Bytes += buffer.Length;
            m_Packets += 1;

            // Bounds
            if (buffer.Length < m_MinPacket)
                m_MinPacket = buffer.Length;
            if (buffer.Length > m_MaxPacket)
                m_MaxPacket = buffer.Length;

            // Forward
            m_Callback( buffer );
        }

        /// <summary>
        /// Sendet ein rekonstuiertes Paket an den zugeordneten Verbraucher.
        /// </summary>
        /// <param name="buffer">Ein Gesamtspeicherbereich mit den Paketdaten.</param>
        /// <param name="start">Position des ersten Bytes des Paketes im Gesamtspeicher.</param>
        /// <param name="length">Anzahl der Bytes im Paket.</param>
        protected void Process( byte[] buffer, int start, int length )
        {
            // Allocate new
            byte[] data = new byte[length];

            // Copy
            Array.Copy( buffer, start, data, 0, data.Length );

            // Use
            Process( data );
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Rekonstruktionsinstanz endgültig.
        /// </summary>
        public virtual void Dispose()
        {
        }

        #endregion
    }
}
