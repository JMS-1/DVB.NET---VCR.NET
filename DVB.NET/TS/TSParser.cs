using System;
using JMS.DVB.EPG;
using System.Threading;
using JMS.DVB.EPG.Tables;
using JMS.DVB.TS.TSBuilders;
using System.Collections.Generic;

namespace JMS.DVB.TS
{
    /// <summary>
    /// Diese Klasse analyisiert einen DVB <i>Transport Stream</i> und zerlegt ihn in
    /// die individuellen Datenströme.
    /// </summary>
    public class TSParser : IDisposable
    {
        /// <summary>
        /// Informationen über den Interessenten an einem der Nutzdatenströme. Instanzen dieser
        /// Klasse werden für die automatische Extraktion von Bild- und Tonsignal verwendet.
        /// </summary>
        private class PESInfo : IDisposable
        {
            /// <summary>
            /// Die eindeutige Datenstromkennung.
            /// </summary>
            public readonly ushort PID;

            /// <summary>
            /// Der zugehörige Rekonstruktionskomponente für den Nutzdatenstrom.
            /// </summary>
            private PESBuilder m_Builder;

            /// <summary>
            /// Erzeugt eine neue Analyseinstanz für einen Nutzdatenstrom.
            /// </summary>
            /// <param name="parser">Die zugehörige Instanz zum Gesamtdatenstrom.</param>
            /// <param name="pid">Die betroffenen Datenstromkennung.</param>
            /// <param name="callback">Der Empfänger der eigentlichen Nutzdaten.</param>
            public PESInfo( TSParser parser, ushort pid, FilterHandler callback )
            {
                // Remember
                PID = pid;

                // Create
                m_Builder = new PESBuilder( parser, callback );
            }

            /// <summary>
            /// Beginnt die Rekonstruktion der Nutzdaten von neuem.
            /// </summary>
            public void Reset()
            {
                // Forward
                m_Builder.Reset();
            }

            /// <summary>
            /// Nimmt ein Rohdatenpaket entgegen.
            /// </summary>
            /// <param name="packet">Zwischenspeicher für Daten.</param>
            /// <param name="offset">Index des ersten Bytes für das aktuelle Rohdatenpaket.</param>
            /// <param name="length">Anzahl der Bytes im Rohdatenpaket.</param>
            /// <param name="noincrement">Gesetzt, wenn der Rohdatenpaketzähler nicht erhöht werden darf.</param>
            /// <param name="first">Gesetzt, wenn dieses Rohdatenpaket einen PES Kopf enthält.</param>
            /// <param name="counter">Der Rohdatenpaktzähler zu diesem Paket.</param>
            public void AddPacket( byte[] packet, int offset, int length, bool noincrement, bool first, byte counter )
            {
                // Forward
                m_Builder.AddPacket( packet, offset, length, noincrement, first, counter );
            }

            /// <summary>
            /// Beendet die Nutzung dieser Analyseinstanz.
            /// </summary>
            public void Dispose()
            {
                // Cleanup
                using (m_Builder)
                    m_Builder = null;
            }
        }

        /// <summary>
        /// Die Anzahl der potentiellen Rohdatenpakete, die zur Synchronisation am Anfang des Gesamtdatenstroms
        /// verwendet werden.
        /// </summary>
        private const int SyncCount = 100;

        /// <summary>
        /// Verwaltet alle Verbraucher von einzelnen Datenströmen innerhalb des Gesamtdatenstroms.
        /// </summary>
        private Dictionary<ushort, TSBuilder> m_Consumers = new Dictionary<ushort, TSBuilder>();

        /// <summary>
        /// Enthält eine Statistik über die Anteile der individuellen Datenströme am Gesamtdatenstrom.
        /// </summary>
        private Dictionary<ushort, long> m_PacketStatistics = new Dictionary<ushort, long>();

        /// <summary>
        /// Gesetzt, wenn die Statistik über die Anzeile der einzelnen Datenströme geführt werden soll.
        /// </summary>
        private bool m_FillStatisics = false;

        /// <summary>
        /// Zwischenspeicher zur Synchronisation am Beginn einer Rohdatenübertragung.
        /// </summary>
        private byte[] m_SyncBuffer = new byte[SyncCount * Manager.FullSize];

        /// <summary>
        /// Aktuelle Synchronisationsposition.
        /// </summary>
        private int m_SyncIndex = 0;

        /// <summary>
        /// Ein einzelnes Rohdatenpaket.
        /// </summary>
        private byte[] m_Packet = new byte[Manager.FullSize];

        /// <summary>
        /// Aktueller Füllstand des Rohdatenpaketes.
        /// </summary>
        private int m_PacketPos = 0;

        /// <summary>
        /// Anzahl der als fehlerhaft markierten Rohdatenpakete.
        /// </summary>
        private long m_TransmissionErrors = 0;

        /// <summary>
        /// Anzahl der empfangenen Rohdatenpakete.
        /// </summary>
        private long m_PacketsReceived = 0;

        /// <summary>
        /// Anzahl der Fehler in Nutzdatenströmen.
        /// </summary>
        private long m_CorruptedStream = 0;

        /// <summary>
        /// Anzahl der Fehler in Kontrolldatenströmen.
        /// </summary>
        private long m_CorruptedTable = 0;

        /// <summary>
        /// Anzahl der bisher ordnungsgemäß verarbeiteten PATs.
        /// </summary>
        private long m_ValidPATCount = 0;

        /// <summary>
        /// Anzahl der notwendigen Synchronisationen des Gesamtdatenstroms nach schweren
        /// Übertragungsfehlern.
        /// </summary>
        private long m_Resynchronized = 0;

        /// <summary>
        /// Anzahl der empfagenen Bytes im Gesamtdatenstrom.
        /// </summary>
        private long m_BytesReceived = 0;

        /// <summary>
        /// Anzahl der Bytes, die wegen erfolgter Synchronisationen des Gesamtdatenstroms
        /// verworfen wurden.
        /// </summary>
        private long m_BytesSkipped = 0;

        /// <summary>
        /// Anzahl der verschlüsselten Rohdatenpakete.
        /// </summary>
        private long m_Scrambled = 0;

        /// <summary>
        /// Anzahl der empfangenen Rohdatenpaketblöcke.
        /// </summary>
        private long m_Callbacks = 0;

        /// <summary>
        /// Meldet die Anzahl der als fehlerhaft markierten Rohdatenpakete.
        /// </summary>
        public long TransmissionErrors
        {
            get
            {
                // Report
                return m_TransmissionErrors;
            }
        }

        /// <summary>
        /// Meldet die Anzahl der Fehler in Nutzdatenströmen.
        /// </summary>
        public long CorruptedStream
        {
            get
            {
                // Report
                return m_CorruptedStream;
            }
        }

        /// <summary>
        /// Meldet die Anzahl der Fehler in Kontrolldatenströmen.
        /// </summary>
        public long CorruptedTable
        {
            get
            {
                // Report
                return m_CorruptedTable;
            }
        }

        /// <summary>
        /// Meldet die Anzahl der empfangenen Rohdatenpaketblöcke.
        /// </summary>
        public long Callbacks
        {
            get
            {
                // Report
                return m_Callbacks;
            }
        }

        /// <summary>
        /// Meldet die Anzahl der als fehlerhaft markierten Rohdatenpakete.
        /// </summary>
        public long PacketsReceived
        {
            get
            {
                // Report
                return m_PacketsReceived;
            }
        }

        /// <summary>
        /// Meldet die Anzahl der notwendigen Synchronisationen des Gesamtdatenstroms nach schweren
        /// Übertragungsfehlern.
        /// </summary>
        public long Resynchronized
        {
            get
            {
                // Report
                return m_Resynchronized;
            }
        }

        /// <summary>
        /// Meldet die Anzahl der empfagenen Bytes im Gesamtdatenstrom.
        /// </summary>
        public long BytesReceived
        {
            get
            {
                // Report
                return m_BytesReceived;
            }
        }

        /// <summary>
        /// Meldet die Anzahl der Bytes, die wegen erfolgter Synchronisationen des Gesamtdatenstroms
        /// verworfen wurden.
        /// </summary>
        public long BytesSkipped
        {
            get
            {
                // Report
                return m_BytesSkipped;
            }
        }

        /// <summary>
        /// Meldet die Anzahl der verschlüsselten Rohdatenpakete.
        /// </summary>
        public long Scrambled
        {
            get
            {
                // Report
                return m_Scrambled;
            }
        }

        /// <summary>
        /// Hilfskomponente zur Analyse der PAT.
        /// </summary>
        private TypedSIParser<PAT> m_PATParser = new TypedSIParser<PAT>();

        /// <summary>
        /// Hilfskomponente zur Analyse der PMT.
        /// </summary>
        private TypedSIParser<PMT> m_PMTParser = new TypedSIParser<PMT>();

        /// <summary>
        /// Hilfskomponente zur Analyse eines Rohdatenstroms mit PATs.
        /// </summary>
        private SIBuilder m_PATBuilder;

        /// <summary>
        /// Hilfskomponente zur Analyse eines Rohdatenstroms mit PMTs.
        /// </summary>
        private SIBuilder m_PMTBuilder;

        /// <summary>
        /// Der Sender, dessen PMT ermittelt werden soll.
        /// </summary>
        private ushort m_WaitForService;

        /// <summary>
        /// Gesetzt, wenn nach dem Auffinden der gewünschten PMT diese weiter überwacht werden soll.
        /// </summary>
        private ushort m_ResetAfterServiceFound;

        /// <summary>
        /// Der aktuelle Datenstrom, dessen SI Tabellen überwacht werden.
        /// </summary>
        private ushort m_WaitForPID;

        /// <summary>
        /// Signatur einer Methode, die über eine bestimmte PMT informiert.
        /// </summary>
        /// <param name="pmt">Die zugehörige Informationstabelle.</param>
        public delegate void PMTFoundHandler( PMT pmt );

        /// <summary>
        /// Wird aktiviert, wenn eine bestimmte PMT zur Verfügung steht.
        /// </summary>
        public event PMTFoundHandler PMTFound;

        /// <summary>
        /// Erzeugt eine neue Analyseinstanz für einen <i>Transport Stream</i>.
        /// </summary>
        public TSParser()
        {
            // Register
            m_PATParser.TableFound += OnPATFound;
            m_PMTParser.TableFound += OnPMTFound;

            // Install the analyser
            m_PATBuilder = new SIBuilder( this, m_PATParser.OnData );
            m_PMTBuilder = new SIBuilder( this, m_PMTParser.OnData );
        }

        /// <summary>
        /// Wertet eine SI Tabelle aus.
        /// </summary>
        /// <param name="pat"></param>
        private void OnPATFound( PAT pat )
        {
            // At least count it
            Interlocked.Increment( ref m_ValidPATCount );

            // Disabled
            if (m_WaitForService == 0)
                return;

            // Nothing more to do in PMT mode
            if (m_WaitForPID != 0)
                return;

            // Try to find the PID of the service
            ushort pmtPID;
            if (pat.ProgramIdentifier.TryGetValue( m_WaitForService, out pmtPID ) && (pmtPID != 0))
            {
                // Can wait
                m_WaitForPID = pmtPID;
            }
            else
            {
                // Disable
                m_WaitForService = 0;
            }
        }

        /// <summary>
        /// Wertet eine SI Tabelle aus.
        /// </summary>
        /// <param name="pmt"></param>
        private void OnPMTFound( PMT pmt )
        {
            // Validate
            if (null != pmt)
                if (pmt.ProgramNumber != m_WaitForService)
                    pmt = null;

            // Disable or restart 
            m_WaitForService = m_ResetAfterServiceFound;
            m_WaitForPID = 0;

            // Nothing to do
            if (null == pmt)
                return;

            // Forward
            PMTFoundHandler callback = PMTFound;
            if (null != callback)
                callback( pmt );
        }

        /// <summary>
        /// Ermittelt die nächste PMT zu einem Sender.
        /// </summary>
        /// <param name="serviceIdentifier">Der gewünschte Sender.</param>
        public void RequestPMT( ushort serviceIdentifier )
        {
            // Forward
            RequestPMT( serviceIdentifier, false );
        }

        /// <summary>
        /// Ermittelt die nächste PMT zu einem Sender.
        /// </summary>
        /// <param name="serviceIdentifier">Der gewünschte Sender.</param>
        /// <param name="resetAfterEvent">Gesetzt, wenn nach dem Melden der Kennung
        /// weiter überwacht werden soll.</param>
        public void RequestPMT( ushort serviceIdentifier, bool resetAfterEvent )
        {
            // Activate PMT scanning
            lock (m_Consumers)
            {
                // Reset to find the indicated PMT
                m_ResetAfterServiceFound = resetAfterEvent ? serviceIdentifier : (ushort) 0;
                m_WaitForService = serviceIdentifier;
                m_WaitForPID = 0;
            }
        }

        /// <summary>
        /// Überträgte Rohdatenpakete in den Gesamtdatenstrom.
        /// </summary>
        /// <param name="buffer">Ein Speicherblock mit Rohdatenpaketen.</param>
        public void AddPayload( byte[] buffer )
        {
            // Forward
            AddPayload( buffer, 0, buffer.Length );
        }

        /// <summary>
        /// Überträgte Rohdatenpakete in den Gesamtdatenstrom.
        /// </summary>
        /// <param name="buffer">Ein Speicherblock mit Rohdatenpaketen.</param>
        /// <param name="index">Erstes Byte im Speicherblock, das analysiert werden soll.</param>
        /// <param name="length">Anzahl der zu analyiserenden Bytes.</param>
        public void AddPayload( byte[] buffer, int index, int length )
        {
            // Validate
            if (null == buffer)
                throw new ArgumentNullException( "buffer" );
            if ((index < 0) || (index > buffer.Length))
                throw new ArgumentException( index.ToString(), "index" );
            if ((length < 0) || (length > buffer.Length))
                throw new ArgumentException( length.ToString(), "length" );
            if ((index + length) > buffer.Length)
                throw new ArgumentException( length.ToString(), "length" );

            // Count
            m_BytesReceived += length;
            m_Callbacks += 1;

            // As long as necessary
            lock (m_Consumers)
                lock (m_PacketStatistics)
                    while (length > 0)
                    {
                        // We are in synchronisation mode
                        while (m_SyncIndex < m_SyncBuffer.Length)
                        {
                            // How many to copy
                            int copy = Math.Min( m_SyncBuffer.Length - m_SyncIndex, length );

                            // Copy
                            if (copy > 0)
                                Array.Copy( buffer, index, m_SyncBuffer, m_SyncIndex, copy );

                            // Advance
                            m_SyncIndex += copy;

                            // Count
                            m_BytesSkipped += copy;

                            // Not filled
                            if (m_SyncIndex < m_SyncBuffer.Length)
                                return;

                            // Adjust
                            length -= copy;
                            index += copy;

                            // Reset
                            m_SyncIndex = 0;

                            // Find a sequence of start codes
                            for (int i = 0, j, t; i < Manager.FullSize; ++i)
                            {
                                // Not a candidate
                                if (0x47 != m_SyncBuffer[i])
                                    continue;

                                // Test all
                                for (j = SyncCount, t = i; j-- > 1; )
                                    if (0x47 != m_SyncBuffer[t += Manager.FullSize])
                                        break;

                                // Did it
                                if (0 == j)
                                {
                                    // See how many bytes we could use
                                    int copyBack = m_SyncBuffer.Length - i;

                                    // Adjust
                                    while (copyBack > copy) copyBack -= Manager.FullSize;

                                    // Check mode
                                    if (copyBack > 0)
                                    {
                                        // Correct counter
                                        m_BytesSkipped -= copyBack;

                                        // Push data back to current buffer
                                        length += copyBack;
                                        index -= copyBack;

                                        // Use all
                                        m_PacketPos = 0;
                                    }
                                    else
                                    {
                                        // Calculate the buffer size
                                        m_PacketPos = Manager.FullSize - i;

                                        // Correct counter
                                        m_BytesSkipped -= m_PacketPos;

                                        // Copy in
                                        Array.Copy( m_SyncBuffer, m_SyncBuffer.Length - m_PacketPos, m_Packet, 0, m_PacketPos );

                                    }

                                    // Mark as found
                                    m_SyncIndex = m_SyncBuffer.Length;

                                    // Done
                                    break;
                                }
                            }
                        }

                        // Finish the packet
                        int packet = Math.Min( Manager.FullSize - m_PacketPos, length );

                        // Copy over
                        if (packet > 0)
                            Array.Copy( buffer, index, m_Packet, m_PacketPos, packet );

                        // Adjust
                        m_PacketPos += packet;

                        // End of buffer
                        if (m_PacketPos < Manager.FullSize)
                            return;

                        // Count and restart
                        m_PacketsReceived += 1;
                        m_PacketPos = 0;

                        // Adjust
                        length -= packet;
                        index += packet;

                        // Needs re-synchronize
                        if (0x47 != m_Packet[0])
                        {
                            // Internal reset
                            Resynchronize();

                            // Next
                            continue;
                        }

                        // Get information from header
                        int pidh = m_Packet[1];
                        int pidl = m_Packet[2];
                        byte flags = m_Packet[3];

                        // Numbers
                        ushort pid = (ushort) (pidl + 256 * (0x1f & pidh));
                        byte counter = (byte) (0xf & flags);

                        // See if statistics should be updated
                        if (m_FillStatisics)
                        {
                            // Overall counter
                            long countPackets;
                            if (!m_PacketStatistics.TryGetValue( pid, out countPackets ))
                                countPackets = 0;

                            // Update
                            m_PacketStatistics[pid] = countPackets + 1;
                        }

                        // Check for error
                        if (0x80 == (0x80 & pidh))
                        {
                            // Skip packet
                            m_TransmissionErrors += 1;

                            // Next
                            continue;
                        }

                        // Check for scrambled data
                        if (0x00 != (0xc0 & flags))
                        {
                            // Skip packet
                            m_Scrambled += 1;

                            // Next
                            continue;
                        }

                        // Load flags
                        bool adaption = (0x20 == (0x20 & flags));
                        bool payload = (0x10 == (0x10 & flags));
                        bool first = (0x40 == (0x40 & pidh));

                        // Get the payload
                        int start = 4, size = Manager.PacketSize;

                        // Cut off adaption
                        if (adaption)
                        {
                            // Read size
                            int skip = m_Packet[start];

                            // Reduce
                            start += ++skip;
                            size -= skip;

                            // In error
                            if (size < 0)
                            {
                                // Internal reset
                                Resynchronize();

                                // Next
                                continue;
                            }
                        }

                        // There are special situations where the counter is not incremented
                        bool noInc = (adaption && !payload && (0 == size));

                        // Get the real size which is 0 if the payload indicator is not set
                        int realSize = (payload ? size : 0);

                        // Check for custom handler
                        TSBuilder consumer;
                        if (m_Consumers.TryGetValue( pid, out consumer ))
                            consumer.AddPacket( m_Packet, start, realSize, noInc, first, counter );

                        // Forward to PAT
                        if (m_PATBuilder != null)
                            if (pid == 0)
                                m_PATBuilder.AddPacket( m_Packet, start, realSize, noInc, first, counter );

                        // Forward to PMT
                        if (m_PMTBuilder != null)
                            if (m_WaitForService != 0)
                                if (pid != 0)
                                    if (m_WaitForPID == pid)
                                        m_PMTBuilder.AddPacket( m_Packet, start, realSize, noInc, first, counter );
                    }
        }

        /// <summary>
        /// Meldet einen Verbraucher zu einem Teildatenstrom.
        /// </summary>
        /// <param name="pid">Die gewünschte Datenstromkennung.</param>
        /// <returns>Der zugehörige Verbraucher oder <i>null</i>.</returns>
        public TSBuilder this[ushort pid]
        {
            get
            {
                // Result
                TSBuilder consumer;

                // Synchronize
                lock (m_Consumers)
                    if (m_Consumers.TryGetValue( pid, out consumer ))
                        return consumer;

                // Not dound
                return null;
            }
        }

        /// <summary>
        /// Registriert einen einfacher Verbraucher für einen Datenstrom.
        /// </summary>
        /// <param name="pid">Die gewünschte Datenstromkennung.</param>
        /// <param name="isSITable">Gesetzt, wenn es sich um einen Kontroll- und keinen
        /// Nutzdatenstrom handelt.</param>
        /// <param name="callback"></param>
        public void SetFilter( ushort pid, bool isSITable, FilterHandler callback )
        {
            // Validate
            if (null == callback) throw new ArgumentNullException( "callback" );

            // Create
            TSBuilder consumer;
            if (isSITable)
                consumer = new SIBuilder( this, callback );
            else
                consumer = new PESBuilder( this, callback );

            // Remember
            RegisterCustomFilter( pid, consumer );
        }

        /// <summary>
        /// Registriert einen Verbraucher für einen Datenstrom.
        /// </summary>
        /// <param name="pid">Die gewünschte Datenstromkennung.</param>
        /// <param name="consumer">Der Verbrqaucher für die Daten.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Verbraucher angegeben.</exception>
        public void RegisterCustomFilter( ushort pid, TSBuilder consumer )
        {
            // Validate
            if (null == consumer)
                throw new ArgumentNullException( "consumer" );

            // Remove previous
            RemoveFilter( pid );

            // Add synchronized
            lock (m_Consumers)
                m_Consumers[pid] = consumer;
        }

        /// <summary>
        /// Entfernt einen Verbraucher für einen Datenstrom.
        /// </summary>
        /// <param name="pid">Die gewünschte Datenstromkennung.</param>
        public void RemoveFilter( ushort pid )
        {
            // Synchronize
            TSBuilder consumer;
            lock (m_Consumers)
                if (m_Consumers.TryGetValue( pid, out consumer ))
                    m_Consumers.Remove( pid );
                else
                    return;

            // Cleanup
            consumer.Dispose();
        }

        /// <summary>
        /// Startet die erneute Synchronisation mit dem Gesamtdatenstrom.
        /// </summary>
        private void Resynchronize()
        {
            // Count
            m_Resynchronized += 1;

            // Reset buffer
            m_SyncIndex = 0;

            // Forward to all receivers
            foreach (TSBuilder consumer in m_Consumers.Values)
                consumer.Reset();

            // Forward to PAT / PMT
            if (null != m_PATBuilder)
                m_PATBuilder.Reset();
            if (null != m_PMTBuilder)
                m_PMTBuilder.Reset();
        }

        /// <summary>
        /// Meldet, dass ein Kontrolldatenstrom einen Fehler enthält.
        /// </summary>
        internal void TableCorrupted()
        {
            // Count
            m_CorruptedTable += 1;
        }

        /// <summary>
        /// Meldet, dass ein Nutzdatenstrom einen Fehler enthält.
        /// </summary>
        internal void StreamCorrupted()
        {
            // Count
            m_CorruptedStream += 1;
        }

        /// <summary>
        /// Meldet die aktuelle Statistik über die Anteile der Teildatenströme
        /// im Gesamtdatenstrom.
        /// </summary>
        /// <exception cref="InvalidOperationException">Die Statistik muss explizit aktiviert werden,
        /// bevor sie abgerufen werden kann.</exception>
        public Dictionary<ushort, long> PacketStatistics
        {
            get
            {
                // Synchronize
                lock (m_PacketStatistics)
                {
                    // Validate
                    if (!m_FillStatisics) throw new InvalidOperationException( "Statisics are disabled" );

                    // Clone
                    return new Dictionary<ushort, long>( m_PacketStatistics );
                }
            }
        }

        /// <summary>
        /// Setzt den PAT Zähler zurück.
        /// <seealso cref="ValidPATCount"/>
        /// </summary>
        public void RestartPATCounter()
        {
            // Do it
            Interlocked.Exchange( ref m_ValidPATCount, 0 );
        }

        /// <summary>
        /// Meldet die Anzahl der erkannten PATs seit dem letzten <see cref="RestartPATCounter"/>.
        /// </summary>
        public long ValidPATCount
        {
            get
            {
                // Report
                return Thread.VolatileRead( ref m_ValidPATCount );
            }
        }

        /// <summary>
        /// Meldet oder legt fest, ob eine Statistik über die Anteile der Teildateströme
        /// am Gesamtdatenstrom geführt werden soll. Beim Setzen dieser Eigenschaft werden
        /// die Zähler des Statistik immer zurückgesetzt.
        /// </summary>
        public bool FillStatistics
        {
            get
            {
                // Report
                lock (m_PacketStatistics) return m_FillStatisics;
            }
            set
            {
                // Synchronize
                lock (m_PacketStatistics)
                {
                    // Change
                    m_FillStatisics = value;

                    // Time to reset
                    m_PacketStatistics.Clear();
                }
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Arbeit dieser Analyseinstanz endgültig.
        /// </summary>
        public void Dispose()
        {
            // PAT / PMT Builder
            lock (m_Consumers)
            {
                // PAT
                if (null != m_PATBuilder)
                    try
                    {
                        // Discard
                        m_PATBuilder.Dispose();
                    }
                    finally
                    {
                        // Forget
                        m_PATBuilder = null;
                    }

                // PMT
                if (null != m_PMTBuilder)
                    try
                    {
                        // Discard
                        m_PMTBuilder.Dispose();
                    }
                    finally
                    {
                        // Forget
                        m_PMTBuilder = null;
                    }
            }

            // To clear
            List<TSBuilder> cleanup = new List<TSBuilder>();

            // Forward
            lock (m_Consumers)
                try
                {
                    // Copy over
                    cleanup.AddRange( m_Consumers.Values );
                }
                finally
                {
                    // Clear
                    m_Consumers.Clear();
                }

            // Shutdown all
            foreach (TSBuilder consumer in cleanup) consumer.Dispose();
        }

        #endregion
    }
}
