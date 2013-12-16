using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using JMS.DVB.TS;
using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DeviceAccess.BDAElements
{
    /// <summary>
    /// Endpunkt zur Entgegennahme eines Rohdatenstroms.
    /// </summary>
    public class InputPin : TypedComIdentity<IPin>, IPin, IMemInputPin
    {
        /// <summary>
        /// Alle aktiven Verbraucher.
        /// </summary>
        private Dictionary<ushort, Action<byte[]>> m_Handlers = new Dictionary<ushort, Action<byte[]>>();

        /// <summary>
        /// Alle Verbraucher, die nicht mit Nutz- sondern mit Kontrolldaten arbeiten.
        /// </summary>
        private Dictionary<ushort, bool> m_IsTableHandler = new Dictionary<ushort, bool>();

        /// <summary>
        /// Die zu verwendende Speicherverwaltung.
        /// </summary>
        private IMemAllocator m_Allocator = null;

        /// <summary>
        /// Der eingesetzten Datentyp.
        /// </summary>
        private MediaType m_ConnectType = new MediaType();

        /// <summary>
        /// Die Analyseinstanz für die Rohdaten.
        /// </summary>
        private TSParser m_Parser = new TSParser();

        /// <summary>
        /// Optional eine Datei für die Protokollierung des Rohdatenstroms.
        /// </summary>
        private DoubleBufferedFile m_Dump = null;

        /// <summary>
        /// Sperre zum Schreiben der Rohdaten in eine Datei.
        /// </summary>
        private object m_DumpLock = new object();

        /// <summary>
        /// Gesetzt, wenn die Speicherverwaltung nur lesend verwendet werden darf.
        /// </summary>
        private bool m_AllocatorReadOnly = false;

        /// <summary>
        /// Der zugehörige Verbraucher.
        /// </summary>
        private IntPtr m_Connected = IntPtr.Zero;

        /// <summary>
        /// Gesetzt, wenn Daten verarbeitet werden sollen.
        /// </summary>
        private volatile bool m_Running = false;

        /// <summary>
        /// Der Datentyp dieses Endpunktes.
        /// </summary>
        private MediaType[] m_Types;

        /// <summary>
        /// Der zugehörige Filter - es handelt sich immer um eine .NET Instanz, daher ist eine Freigabe unnötig.
        /// </summary>
        private TypedComIdentity<IBaseFilter> m_Filter;

        /// <summary>
        /// Die einzig relevante BDA Komponente als zusätzlicher Verbraucher.
        /// </summary>
        private OutputPin m_TIF = null;

        /// <summary>
        /// Erzeugt einen neuen Endpunkt.
        /// </summary>
        /// <param name="filter">Der zugehörige Filter.</param>
        /// <param name="createTIF">Gesetzt, wenn eine BDA Komponente eingebunden werden soll.</param>
        public InputPin( TypedComIdentity<IBaseFilter> filter, bool createTIF )
        {
            // Remember
            m_Filter = filter;

            // Create types
            m_Types = new[] { BDAEnvironment.TransportStreamMediaType1, BDAEnvironment.TransportStreamMediaType2 };

            // Create optional output pins
            if (createTIF)
                m_TIF = new OutputPin( m_Filter, "TIF", m_Types[0] );
        }

        /// <summary>
        /// Meldet die verwendete Analysekomponente.
        /// </summary>
        public TSParser TSParser
        {
            get
            {
                // Report
                return m_Parser;
            }
        }

        /// <summary>
        /// Meldet den Anschluss für die BDA Komponente.
        /// </summary>
        internal OutputPin TIFConnector
        {
            get
            {
                // Report
                return m_TIF;
            }
        }

        /// <summary>
        /// Meldet oder legt fest, ob eine Durchflussstatistik erstellt werden soll.
        /// </summary>
        public bool EnableStatistics
        {
            get
            {
                // Forward
                return (m_Parser != null) && m_Parser.FillStatistics;
            }
            set
            {
                // Forward
                m_Parser.FillStatistics = value;
            }
        }

        /// <summary>
        /// Ermittelt die Durchflussdaten zu einem Teildatenstrom.
        /// </summary>
        /// <param name="pid">Die Datenstromkennung.</param>
        /// <returns>Die Informationen zu dem Teildatenstrom.</returns>
        public PinStatistics GetStatistics( ushort pid )
        {
            // Create
            var result = new PinStatistics();

            // Load
            var filter = m_Parser[pid];
            if (filter != null)
            {
                // Fill
                result.SampleMinSize = filter.MinimumPacketSize;
                result.SampleMaxSize = filter.MaximumPacketSize;
                result.SampleCount = filter.PacketCount;
                result.SampleTotal = filter.TotalBytes;
            }

            // Done
            return result;
        }

        /// <summary>
        /// Meldet einen Verbraucher für einen Teildatenstrom an.
        /// </summary>
        /// <param name="pid">Die Datenstromkennung.</param>
        /// <param name="isSITable">Gesetzt, wenn nicht Nutz- sondern Steuerdaten verarbeitet werden sollen.</param>
        /// <param name="handler">Der Verbraucher.</param>
        public void AddFilter( ushort pid, bool isSITable, Action<byte[]> handler )
        {
            // Synchronized add
            lock (m_Handlers)
            {
                // All information
                m_IsTableHandler[pid] = isSITable;
                m_Handlers[pid] = handler;
            }
        }

        /// <summary>
        /// Erstellt ein Protokoll zum Datenfluss.
        /// </summary>
        /// <param name="format">Format für die Protokollierung.</param>
        /// <param name="args">Parameter zur Erstellung eines Protokolleintrags aus dem Format.</param>
        private static void Dump( string format, params object[] args )
        {
            // File name
            string path = Path.Combine( Path.GetTempPath(), "DVBNETTSStatistics.log" );

            // Be safe
            try
            {
                // Open file
                using (StreamWriter writer = new StreamWriter( path, true, Encoding.GetEncoding( 1252 ) ))
                {
                    // Write a line
                    writer.WriteLine( "{0:yyyy/MM/dd HH:mm:ss} {1}", DateTime.Now, string.Format( format, args ) );
                }
            }
            catch
            {
                // Ignore any error
            }
        }

        /// <summary>
        /// Beendet einen Verbraucher.
        /// </summary>
        /// <param name="pid">Die gewünschte Datenstromlennung.</param>
        public void StopFilter( ushort pid )
        {
            // Check statistics
            if (EnableStatistics)
            {
                // Load
                PinStatistics statistics = GetStatistics( pid );
                if (null != statistics)
                    if (statistics.SampleCount > 0)
                        Dump( "[{0:00000}] #{1} {2} ([{3}..{4}] {5})", pid, statistics.SampleCount, statistics.SampleTotal, statistics.SampleMinSize, statistics.SampleMaxSize, statistics.BytesPerSample );
            }

            // Deregister
            m_Parser.RemoveFilter( pid );
        }

        /// <summary>
        /// Aktiviert einen Verbraucher.
        /// </summary>
        /// <param name="pid">Die gewünschte Datenstromkennung.</param>       
        public void StartFilter( ushort pid )
        {
            // Synchronized load
            Action<byte[]> handler;
            bool isSITable;
            lock (m_Handlers)
            {
                // None
                if (!m_Handlers.TryGetValue( pid, out handler ))
                    return;
                if (!m_IsTableHandler.TryGetValue( pid, out isSITable ))
                    return;
            }

            // Restart
            m_Parser.SetFilter( pid, isSITable, handler );
        }

        /// <summary>
        /// Meldet alle Verbraucher ab.
        /// </summary>
        public void RemoveAllFilters()
        {
            // Helper
            List<ushort> pids = new List<ushort>();

            // Fill
            lock (m_Handlers) pids.AddRange( m_Handlers.Keys );

            // Remove all
            foreach (ushort pid in pids) RemoveFilter( pid );
        }

        /// <summary>
        /// Meldet einen Verbraucher ab.
        /// </summary>
        /// <param name="pid">Die zugehörige Datenstromkennung.</param>
        public void RemoveFilter( ushort pid )
        {
            // From parser
            StopFilter( pid );

            // Synchronized remove
            lock (m_Handlers)
            {
                // All
                m_IsTableHandler.Remove( pid );
                m_Handlers.Remove( pid );
            }
        }

        /// <summary>
        /// Aktiviert den Datenfluss.
        /// </summary>
        public void Start()
        {
            // Enable
            m_Running = true;
        }

        /// <summary>
        /// Beendet den Datenfluss.
        /// </summary>
        public void Stop()
        {
            // Already did it
            if (!m_Running)
                return;

            // Disable
            m_Running = false;

            // Test
            if (!EnableStatistics)
                return;

            // Report
            Dump( "cb={0} b={1} s={2} p={3} enc={4}", m_Parser.Callbacks, m_Parser.BytesReceived, m_Parser.BytesSkipped, m_Parser.PacketsReceived, m_Parser.Scrambled );
            Dump( "strm={0} tbl={1} sync={2} err={3}", m_Parser.CorruptedStream, m_Parser.CorruptedTable, m_Parser.Resynchronized, m_Parser.TransmissionErrors );

            // Special filters
            if (m_TIF != null)
                Dump( "TIF: {0}", m_TIF.SamplesReceived );

            // Special
            foreach (KeyValuePair<ushort, long> stat in m_Parser.PacketStatistics)
            {
                // Show
                Dump( "{0:00000} #{1}", stat.Key, stat.Value );
            }
        }

        /// <summary>
        /// Beendet das Kopieren des Rohdatenstroms in eine Datei.
        /// </summary>
        public void StopDump()
        {
            // Load it
            lock (m_DumpLock)
                using (var dump = Interlocked.Exchange( ref m_Dump, null ))
                    if (dump != null)
                        dump.Flush();
        }

        /// <summary>
        /// Beginnt damit, den Rohdatenstrom in eine Datei zu schreiben.
        /// </summary>
        /// <param name="path">Der volle Pfad zur Datei.</param>
        public void StartDump( string path )
        {
            // Synchronize
            lock (m_DumpLock)
            {
                // Always stop
                StopDump();

                // Create new
                m_Dump = new DoubleBufferedFile( path, 10000000 );
            }
        }

        #region IPin Members

        /// <summary>
        /// Verbindet diesen Endpunkt mit einem anderen.
        /// </summary>
        /// <param name="receivePin">Ein Verbraucher.</param>
        /// <param name="mediaType">Der gewünschte Datentyp.</param>
        public void Connect( IntPtr receivePin, IntPtr mediaType )
        {
            // Not possible
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Verbindet diesen Endpunkt mit einem anderen.
        /// </summary>
        /// <param name="connector">Ein Erzeuger.</param>
        /// <param name="mediaType">Der gewünschte Datentyp.</param>
        public void ReceiveConnection( IntPtr connector, IntPtr mediaType )
        {
            // Free
            BDAEnvironment.Release( ref m_Connected );

            // Remember
            if (connector != null)
                Marshal.AddRef( connector );
            m_Connected = connector;

            // Clone the media type
            m_ConnectType.Dispose();
            m_ConnectType = new MediaType( mediaType );
        }

        /// <summary>
        /// Trennt diesen Endpunkt von allen anderen.
        /// </summary>
        public void Disconnect()
        {
            // Release
            BDAEnvironment.Release( ref m_Allocator );
            BDAEnvironment.Release( ref m_Connected );

            // Release
            m_ConnectType.Dispose();
            m_ConnectType = new MediaType();

            // Reset
            m_AllocatorReadOnly = false;
            m_Connected = IntPtr.Zero;
            m_Allocator = null;
        }

        /// <summary>
        /// Prüft, ob eine Verbindung vorliegt.
        /// </summary>
        /// <param name="other">Der andere Endpunkt.</param>
        /// <returns>Ergebnis der Prüfung, negativ im Falle eines Fehlers.</returns>
        public Int32 ConnectedTo( ref IntPtr other )
        {
            // Not connected
            if (m_Connected == IntPtr.Zero)
                return BDAEnvironment.NotConnected;

            // Add reference
            Marshal.AddRef( m_Connected );

            // Remember
            other = m_Connected;

            // Done
            return 0;
        }

        /// <summary>
        /// Ermittelt den Datentyp dieses Endpunktes.
        /// </summary>
        /// <param name="mediaType">Der aktuelle Datentyp.</param>
        public void ConnectionMediaType( ref RawMediaType mediaType )
        {
            // Colone
            m_ConnectType.CopyTo( ref mediaType );
        }

        /// <summary>
        /// Beschreibt diesen Endpunkt.
        /// </summary>
        /// <param name="info">Die gewünschte Beschreibung.</param>
        public void QueryPinInfo( ref PinInfo info )
        {
            // Fill the rest
            info.Direction = PinDirection.Input;
            info.Filter = m_Filter.AddRef();
            info.Name = QueryId();
        }

        /// <summary>
        /// Ermittelt die Orientierung dieses Endpunktes.
        /// </summary>
        /// <returns>Die Orientierung des Endpunktes.</returns>
        public PinDirection QueryDirection()
        {
            // Always input
            return PinDirection.Input;
        }

        /// <summary>
        /// Ermittelt den eindeutigen Namen dieses Endpunktes.
        /// </summary>
        /// <returns>Der gewünschte Name.</returns>
        public string QueryId()
        {
            // Static
            return "Input";
        }

        /// <summary>
        /// Prüft, ob der Endpunkt bestimmte Daten annehmen kann.
        /// </summary>
        /// <param name="mediaType">Eine Art von Daten.</param>
        /// <returns>Ergebnis der Prüfung, negativ im Falle eines Fehlers.</returns>
        public Int32 QueryAccept( IntPtr mediaType )
        {
            // Always
            return 0;
        }

        /// <summary>
        /// Meldet alle unterstützten Datentypen.
        /// </summary>
        /// <returns>Eine Auflistung über alle Datentypen.</returns>
        public IEnumMediaTypes EnumMediaTypes()
        {
            // Create
            return new TypeEnum( m_Types );
        }

        /// <summary>
        /// Prüft auf interne Verbindungen.
        /// </summary>
        /// <param name="pin">Ein anderer Endpunkt.</param>
        /// <param name="pinIndex">Die laufende Nummer des Endpunktes.</param>
        public void QueryInternalConnections( out IPin pin, ref uint pinIndex )
        {
            // None
            pinIndex = 0;
            pin = null;
        }

        /// <summary>
        /// Meldet das Ende des Datenstroms.
        /// </summary>
        public void EndOfStream()
        {
        }

        /// <summary>
        /// Beginnt mit dem Entleeren von Zwischenspeichern.
        /// </summary>
        public void BeginFlush()
        {
        }

        /// <summary>
        /// Beendet das Entleeren von Zwischenspeichern.
        /// </summary>
        public void EndFlush()
        {
        }

        /// <summary>
        /// Meldet einen neuen Datenbereich.
        /// </summary>
        /// <param name="tStart">Startzeitpunkt.</param>
        /// <param name="tStop">Endzeitpunkt.</param>
        /// <param name="dRate">Datenrate.</param>
        public void NewSegment( long tStart, long tStop, double dRate )
        {
        }

        #endregion

        #region IMemInputPin Members

        /// <summary>
        /// Meldet die aktuelle Speicherverwaltung.
        /// </summary>
        /// <returns>Die gerade verwendete Speicherverwaltung.</returns>
        public IMemAllocator GetAllocator()
        {
            // Create on first call
            if (m_Allocator == null)
                m_Allocator = (IMemAllocator) Activator.CreateInstance( Type.GetTypeFromCLSID( new Guid( "1e651cc0-b199-11d0-8212-00c04fc32c45" ) ) );

            // Report
            return m_Allocator;
        }

        /// <summary>
        /// Ändert die zu verwendende Speicherverwaltung.
        /// </summary>
        /// <param name="allocator">Die neue Speicherverwaltung.</param>
        /// <param name="bReadOnly">Gesetzt, wenn auf die Speicherverwaltung nur lesend zugegriffen werden darf.</param>
        public void NotifyAllocator( IMemAllocator allocator, bool bReadOnly )
        {
            // Cleanup
            BDAEnvironment.Release( ref m_Allocator );

            // Remember
            m_AllocatorReadOnly = bReadOnly;
            m_Allocator = allocator;
        }

        /// <summary>
        /// Meldet die Anforderungen dieses Endpunktes an die Speicherverwaltung.
        /// </summary>
        /// <param name="info">Die gewünschten Anforderungen.</param>
        public void GetAllocatorRequirements( ref AllocatorProperties info )
        {
            // Default all
            info.cbAlign = 0;
            info.cbBuffer = 0;
            info.cbPrefix = 0;
            info.cBuffers = 0;
        }

        /// <summary>
        /// Nimmt ein einzelnes Datenpaket entgegen.
        /// </summary>
        /// <param name="rawSample">Die COM Schnittstelle des Paketes.</param>
        public void Receive( IntPtr rawSample )
        {
            // Increment reference counter - will be released by wrapper
            Marshal.AddRef( rawSample );

            // Wrap it
            using (var sample = new NoMarshalComObjects.MediaSample( rawSample ))
                try
                {
                    // Skip
                    if (!m_Running)
                        return;

                    // No parser
                    if (m_Parser == null)
                        return;

                    // Attach to the information
                    int size = sample.ActualDataLength;

                    // Skip
                    if (size < 1)
                        return;

                    // Attach to data
                    var data = sample.BaseAddress;
                    var buffer = new byte[size];

                    // Fill
                    Marshal.Copy( data, buffer, 0, buffer.Length );

                    // Process
                    m_Parser.AddPayload( buffer );

                    // Synchronize
                    if (m_Dump != null)
                        lock (m_DumpLock)
                            if (m_Dump != null)
                                m_Dump.Write( buffer, 0, buffer.Length );
                }
                finally
                {
                    // Forward
                    if (m_TIF != null)
                        m_TIF.Receive( rawSample );
                }
        }

        /// <summary>
        /// Nimmet eine Reihe von Datenpaketen entgegen.
        /// </summary>
        /// <param name="sampleArray">Die zu verarbeitenden Pakete.</param>
        /// <param name="sampleCount">Die Anzahl der Pakete.</param>
        /// <param name="processed">Die Anzahl der verarbeiteten Pakete.</param>
        public void ReceiveMultiple( IntPtr[] sampleArray, Int32 sampleCount, out Int32 processed )
        {
            // Reset
            processed = 0;

            // Loop
            for (int i = 0; i < sampleCount; ++processed)
                Receive( sampleArray[i] );
        }

        /// <summary>
        /// Meldet, ob Datenpakete entgegen genommen werden.
        /// </summary>
        /// <returns>Ergebnis der Prüfung, negative Werte weisen auf eine Fehlersituation hin.</returns>
        public Int32 ReceiveCanBlock()
        {
            // 1 == can not block (S_FALSE, we do all), 0 == can block (S_OK, graph must do)
            return 0;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        public override void Dispose()
        {
            // Stop any raw TS dump
            StopDump();

            // Stop always
            Stop();

            // Do proper cleanup
            using (m_Parser)
                m_Parser = null;
            using (m_TIF)
                m_TIF = null;

            // Forward
            base.Dispose();
        }

        #endregion
    }
}
