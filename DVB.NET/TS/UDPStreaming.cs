using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace JMS.DVB.TS
{
    /// <summary>
    /// Verwaltet das Senden eines TS Datenstroms an eine TCP/IP UDP Addresse.
    /// </summary>
    public class UDPStreaming
    {
        /// <summary>
        /// Beschreibt die aktuelle Verbindung.
        /// </summary>
        private class ConnectionInfo
        {
            /// <summary>
            /// Beschreibt eine inaktive Verbindung.
            /// </summary>
            public static readonly ConnectionInfo Empty = new ConnectionInfo( "localhost", 0, null, null );

            /// <summary>
            /// Der Name des Empfängersystems.
            /// </summary>
            public string ClientName { get; private set; }

            /// <summary>
            /// Der verwendete UDP Port.
            /// </summary>
            public int TCPPort { get; private set; }

            /// <summary>
            /// Die zugehörige Netzwerkverbindung.
            /// </summary>
            public Socket Socket { get; private set; }

            /// <summary>
            /// Die zugehörige Empfangsadresse.
            /// </summary>
            private readonly EndPoint m_target;

            /// <summary>
            /// Erzeugt eine neue Beschreibung.
            /// </summary>
            /// <param name="client">Der aktuelle Empfänger.</param>
            /// <param name="port">Der empfangende UDP Port.</param>
            /// <param name="socket">Die TCP/IP Verbindung, über die Daten versendet werden sollen.</param>
            /// <param name="target">Die Adresse des Empfängers.</param>
            public ConnectionInfo( string client, int port, Socket socket, EndPoint target )
            {
                // Remember
                IsActive = (socket != null);
                ClientName = client;
                m_target = target;
                Socket = socket;
                TCPPort = port;
            }

            /// <summary>
            /// Meldet, ob eine Verbindung aktiv ist.
            /// </summary>
            public bool IsActive { get; private set; }

            /// <summary>
            /// Überträgt Daten.
            /// </summary>
            /// <param name="buffer">Die gewünschten Daten.</param>
            public void Send( byte[] buffer )
            {
                // Forward
                Socket.SendTo( buffer, m_target );
            }
        }

        /// <summary>
        /// Daten werden grundsätzlich in diesen Einheiten verschickt.
        /// </summary>
        public const int BufferSize = 40 * Manager.FullSize;

        /// <summary>
        /// Die aktuell gültige Netzwerkverbindung.
        /// </summary>
        private volatile ConnectionInfo m_currentConnection = ConnectionInfo.Empty;

        /// <summary>
        /// Zwischenspeicher zur maximalen Nutzung der UDP Bandbreite.
        /// </summary>
        private byte[] m_buffer = new byte[BufferSize];

        /// <summary>
        /// Aktuelle Position im Zwischenspeicher.
        /// </summary>
        private int m_nextFreeByteInBuffer = 0;

        /// <summary>
        /// Hops, die ein Multicast-Paket überpringen darf.
        /// </summary>
        private readonly int m_TTL;

        /// <summary>
        /// Maximale Anzahl von wartenden Paketen.
        /// </summary>
        private readonly int m_maxQueueLength;

        /// <summary>
        /// Zu versendende Pakete.
        /// </summary>
        private readonly List<byte[]> m_queue = new List<byte[]>();

        /// <summary>
        /// Synchronisation mit dem Versendethread.
        /// </summary>
        private readonly AutoResetEvent m_queueWakeup = new AutoResetEvent( true );

        /// <summary>
        /// Our current worker thread.
        /// </summary>
        private Thread m_currentThread = null;

        /// <summary>
        /// Erzeugt eine neue Verwaltungsinstanz.
        /// </summary>
        /// <param name="multicastTTL">Hops, die ein Multicast Paket überpringen darf.</param>
        /// <param name="maxQueue">Die maximal erlaubte Länge der Warteschlange.</param>
        public UDPStreaming( int multicastTTL, int maxQueue )
        {
            // Remember
            m_maxQueueLength = maxQueue;
            m_TTL = multicastTTL;
        }

        /// <summary>
        /// Meldet die Zwischenspeicher der Sendeliste.
        /// </summary>
        /// <returns>Alle aktiven Zwischenspeicher.</returns>
        private byte[][] GetBuffers()
        {
            // Synchronized
            lock (m_queue)
            {
                // Load all
                var buffers = m_queue.ToArray();

                // Reset queue
                if (buffers.Length > 0)
                    m_queue.Clear();

                // Report
                return buffers;
            }
        }

        /// <summary>
        /// Nimmt Daten aus der Warteschlange und versendet diese.
        /// </summary>
        private void Worker()
        {
            // Local counters
            var maxPackets = int.MinValue;
            var totalPackets = 0L;

            // Avoid thread crashes
            try
            {
                // As long as necessary
                for (; m_currentConnection.IsActive; m_queueWakeup.WaitOne())
                    for (; ; )
                    {
                        // Load all
                        var buffers = GetBuffers();
                        var bufferCount = buffers.Length;
                        if (bufferCount < 1)
                            break;

                        // Bounds
                        if (bufferCount > maxPackets)
                            maxPackets = bufferCount;

                        // Count
                        totalPackets += bufferCount;

                        // Be safe
                        try
                        {
                            // Send all
                            foreach (var buffer in buffers)
                            {
                                // See if we have to finish
                                var connection = m_currentConnection;
                                if (connection.IsActive)
                                    connection.Send( buffer );
                                else
                                    return;
                            }
                        }
                        catch
                        {
                            // Disable streaming
                            return;
                        }
                    }
            }
            catch
            {
                // Dies silently
            }
            finally
            {
                // Report
                Trace.WriteLine( string.Format( "UDP Thread: {0} Packet(s), Maximum Queue Length = {1} ({2} Bytes)", totalPackets, maxPackets, maxPackets * m_buffer.Length ) );
            }
        }

        /// <summary>
        /// Versendet einen Datenblock an die gewünschte Adresse.
        /// </summary>
        /// <param name="buffer">Vollständiger Datenblock.</param>
        public void Send( byte[] buffer )
        {
            // Process all
            for (int i = 0; m_currentConnection.IsActive && (i < buffer.Length); )
                lock (m_queue)
                {
                    // Retest inside synchronized area
                    if (!m_currentConnection.IsActive)
                        break;

                    // How much to send
                    var send = Math.Min( buffer.Length - i, m_buffer.Length - m_nextFreeByteInBuffer );

                    // Fill in
                    if (send > 0)
                    {
                        // Add to buffer
                        Array.Copy( buffer, i, m_buffer, m_nextFreeByteInBuffer, send );

                        // Adjust outer
                        i += send;

                        // Adjust inner
                        m_nextFreeByteInBuffer += send;
                    }

                    // Send to queue as soon as buffer is filled
                    if (m_nextFreeByteInBuffer >= m_buffer.Length)
                        if (!Enqueue())
                            break;
                }
        }

        /// <summary>
        /// Überträgt Daten in die Warteschlange.
        /// </summary>
        /// <remarks>Der Aufrufer hält alle notwendigen Sperren.</remarks>
        /// <returns>Gesetzt, wenn die Daten übertragen werden konnten.</returns>
        private bool Enqueue()
        {
            // We are not allowed to enqueue any more
            if (m_queue.Count >= m_maxQueueLength)
                return false;

            // Enter to queue
            m_queue.Add( (byte[]) m_buffer.Clone() );

            // Reset index - buffer has been sent
            m_nextFreeByteInBuffer = 0;

            // Wakeup thread
            m_queueWakeup.Set();

            // Did it 
            return true;
        }

        /// <summary>
        /// Aktiviert oder deaktiviert den Versand des TS Stroms.
        /// </summary>
        /// <param name="client">Empfängerrechner oder IP Adresse.</param>
        /// <param name="port">TCP/IP Port des Empfängers</param>
        public void SetStreamTarget( string client, int port )
        {
            // May need to split off multi-cast part
            var isMulti = client.StartsWith( "*" );

            // Find the first IP4 address
            var host = Dns.GetHostEntry( isMulti ? Dns.GetHostName() : client );
            var hostIP = host.AddressList.FirstOrDefault( addr => addr.AddressFamily == AddressFamily.InterNetwork );

            // If multicast is enabled
            var multi = isMulti ? new MulticastOption( IPAddress.Parse( client.Substring( 1 ) ), hostIP ) : null;

            // Read current connection
            var previousConnection = m_currentConnection;

            // Disable streaming
            m_currentConnection = ConnectionInfo.Empty;

            // Wait until thread finishes
            var currentThread = Interlocked.Exchange( ref m_currentThread, null );
            if (currentThread != null)
            {
                // Wakeup call
                m_queueWakeup.Set();

                // Wait
                currentThread.Join();
            }

            // Disconnect
            using (var previousSocket = previousConnection.Socket)
                if (previousSocket != null)
                    previousSocket.Close( 2 );

            // Reset counters
            lock (m_queue)
            {
                // Empty queue
                m_queue.Clear();

                // Empty buffer
                m_nextFreeByteInBuffer = 0;
            }

            // Do nothing more, streaming has been disabled
            if (port == 0)
                return;

            // Create socket
            var socket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
            try
            {
                // Default target
                var target = new IPEndPoint( hostIP, port );

                // Attach to multi-casting
                if (multi != null)
                {
                    // Share port
                    socket.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );

                    // Must bind
                    socket.Bind( target );

                    // Register
                    socket.SetSocketOption( SocketOptionLevel.IP, SocketOptionName.AddMembership, multi );
                    socket.SetSocketOption( SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, m_TTL );

                    // Change target
                    target = new IPEndPoint( multi.Group, port );
                }

                // Try synchronous mode
                socket.Blocking = true;

                // Update - from now on streaming is active
                var connectionInfo = new ConnectionInfo( client, port, socket, target );

                // Make sure that all data is valid on all cores - just to be safe
                Thread.MemoryBarrier();

                // Remember
                m_currentConnection = connectionInfo;
            }
            catch (Exception)
            {
                // Cleanup
                using (socket)
                    socket.Close();

                // Forward
                throw;
            }

            // Wakeup call
            m_queueWakeup.Set();

            // Create thread
            m_currentThread = new Thread( Worker ) { Name = "DVB.NET TS Streaming" };

            // Start it
            m_currentThread.Start();
        }

        /// <summary>
        /// Meldet den aktuellen Empfänger des Datenstroms.
        /// </summary>
        public string Client { get { return m_currentConnection.ClientName; } }

        /// <summary>
        /// Meldet den TCP/IP UDP Port des aktuellen Empfängers des Datenstroms.
        /// </summary>
        public int Port { get { return m_currentConnection.TCPPort; } }
    }
}
