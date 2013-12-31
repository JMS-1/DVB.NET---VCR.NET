using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace JMS.DVB.DirectShow.AccessModules
{
    /// <summary>
    /// Dieses Zugriffsmodul erhält im Eingang einen Transport Stream über einen
    /// TCP/IP UDP Port, so wie ihn VCR.NET erzeugt. 
    /// </summary>
    /// <remarks>
    /// Ein VCR.NET Transport Stream enthält im Allgemeinen
    /// ein Bild- und diverse Tonsignale sowie den VideoText. Das Modul erzeugt nach Auswahl
    /// einer Tonspur über <see cref="JMS.DVB.TS.Manager"/> einen neuen Transport Stream
    /// mit nur diesen Datenströmen, der dann an einen DVB.NET DirectShow Graphen
    /// übertragen wird.</remarks>
    public class TransportStreamReceiver : TransportStreamAccessor
    {
        /// <summary>
        /// Signatur einer Methode, die beim Abfragen von Daten aufgerufen wird.
        /// </summary>
        /// <param name="endPoint">Das Zugriffsmodul, das den Aufruf startet.</param>
        public delegate void WaitDataHandler( TransportStreamReceiver endPoint );

        /// <summary>
        /// Ereignis zur Benachrichtigung, wenn Daten angefordert werden.
        /// </summary>
        public event WaitDataHandler OnWaitData;

        /// <summary>
        /// Speichert eingehende TCP/IP UDP Pakete zwischen und sorgt damit für einen
        /// möglichst verlustfreien Empfang des Datenstroms.
        /// </summary>
        private byte[] m_UDPBuffer = new byte[20000000];

        /// <summary>
        /// TCP/IP UDP Adresse, an der die eingehenden Daten entgegen genommen werden.
        /// </summary>
        private IPEndPoint m_EndPoint;

        /// <summary>
        /// Anzahl der Bytes im Paketspeicher.
        /// </summary>
        private int m_UDPIndex = 0;

        /// <summary>
        /// TCP/IP UDP Empfangsstelle.
        /// </summary>
        private Socket m_Socket;

        /// <summary>
        /// Erzeugt eine neue neue Instanz des Zugriffsmoduls im <i>Unicast</i> Modus.
        /// </summary>
        /// <param name="port">TCP/IP UDP Port, an dem der Transport Stream eintrifft.</param>
        public TransportStreamReceiver( ushort port )
            : this( null, port )
        {
        }

        /// <summary>
        /// Erzeugt eine neue neue Instanz des Zugriffsmoduls im <i>Unicast</i> oder
        /// <i>Multicast</i> Modus.
        /// </summary>
        /// <param name="multicastIP">Eine optional Multicast Adresse.</param>
        /// <param name="port">TCP/IP UDP Port, an dem der Transport Stream eintrifft.</param>
        public TransportStreamReceiver( IPAddress multicastIP, ushort port )
        {
            // Verifiy
            if (multicastIP != null)
            {
                // Load
                byte net = multicastIP.GetAddressBytes()[0];

                // Test
                if ((net < 224) || (net > 239))
                    throw new ArgumentException( multicastIP.ToString(), "multicastIP" );
            }

            // Endpoint to use
            m_EndPoint = new IPEndPoint( IPAddress.Any, port );

            // Create the socket
            m_Socket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
            m_Socket.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 10000000 );
            m_Socket.Blocking = true;

            // Prepare for multicast
            if (multicastIP != null)
                m_Socket.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );

            // Bind for receive
            m_Socket.Bind( m_EndPoint );

            // Should use multicast
            if (multicastIP != null)
            {
                // Create the option
                MulticastOption option = new MulticastOption( multicastIP, m_EndPoint.Address );

                // Activate on socket
                m_Socket.SetSocketOption( SocketOptionLevel.IP, SocketOptionName.AddMembership, option );
            }

            // Create receiver
            Thread worker = new Thread( Receive ) { Name = "DVB.NET TS Receiver", IsBackground = true };

            // Prepare apartment
            worker.SetApartmentState( ApartmentState.MTA );

            // Run it
            worker.Start();
        }

        /// <summary>
        /// Empfängt TCP/IP UDP Pakete und überträgt sie in den Paketspeicher.
        /// </summary>
        /// <seealso cref="GetNextChunk"/>
        private void Receive()
        {
            // Fully save
            try
            {
                // Helper
                byte[] request = new byte[500000];

                // As long as possible
                for (int n; (n = m_Socket.Receive( request )) > 0; )
                    for (; ; )
                    {
                        // Dead
                        if (IsDisposing)
                            return;

                        // Load the index
                        int udpIndex = Thread.VolatileRead( ref m_UDPIndex );

                        // Lost packages - just wait for next
                        if ((udpIndex + n) > m_UDPBuffer.Length)
                            break;

                        // Merge in
                        Array.Copy( request, 0, m_UDPBuffer, udpIndex, n );

                        // Advance
                        int nextIndex = udpIndex + n;

                        // Store back
                        if (Interlocked.CompareExchange( ref m_UDPIndex, nextIndex, udpIndex ) == udpIndex)
                        {
                            // Do the wakeup call
                            ReportAudioAvailable();

                            // Wait for more
                            break;
                        }
                    }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Beendet alle Aktivitäten dieses Zugriffsmoduls.
        /// </summary>
        protected override void OnDispose()
        {
            // Load
            Socket socket = m_Socket;

            // Forget
            m_Socket = null;

            // Check socket
            if (null != socket)
                socket.Close();

            // Forward
            base.OnDispose();
        }

        /// <summary>
        /// Ermittelt den nächsten Datenblock von Transport Stream Paketen
        /// zur Übertragung in den DirectShow Graphen.
        /// </summary>
        /// <seealso cref="Receive"/>
        /// <param name="video">Abfrage der Videodaten.</param>
        /// <returns>Zwischenspeicher, dessen erste Bytes zu verwenden sind.</returns>
        protected override byte[] GetNextChunk( bool video )
        {
            // Audio only
            if (video)
                return base.GetNextChunk( video );

            // Load handler
            WaitDataHandler onWaitData = OnWaitData;

            // Inform clients
            if (onWaitData != null)
                onWaitData( this );

            // Set flag
            SetExternalFeed( onWaitData == null );

            // Data
            byte[] udp = null;

            // Process all UDP data available
            for (; ; )
            {
                // Read out
                int udpIndex = Thread.VolatileRead( ref m_UDPIndex );

                // Nothing in it
                if (udpIndex < 1)
                    break;

                // Create
                udp = new byte[udpIndex];

                // Fill
                Array.Copy( m_UDPBuffer, udp, udpIndex );

                // Reset
                if (Interlocked.CompareExchange( ref m_UDPIndex, 0, udpIndex ) == udpIndex)
                    break;
            }

            // Process
            if (udp != null)
                AddPayload( udp, 0, udp.Length );

            // Forward
            return base.GetNextChunk( video );
        }

        /// <summary>
        /// Leert die Zwischenspeicher.
        /// </summary>
        public override void ClearBuffers()
        {
            // Self
            Interlocked.Exchange( ref m_UDPIndex, 0 );

            // To base
            base.ClearBuffers();
        }

        /// <summary>
        /// Meldet den zugeordneten TCP/IP UDP Port für die Entgegennahme des eingehenden
        /// Transport Streams.
        /// </summary>
        public ushort Port { get { return (ushort) m_EndPoint.Port; } }
    }
}
