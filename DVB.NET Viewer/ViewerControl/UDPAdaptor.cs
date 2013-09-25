using System;
using System.Net;
using JMS.DVB.DirectShow.AccessModules;


namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Verbindet die Anwendung mit einem TCP Datenstrom.
    /// </summary>
    public abstract class UDPAdaptor : TSAdaptor
    {
        /// <summary>
        /// Die lokale TCP/IP UDP Empfangsadresse.
        /// </summary>
        /// <remarks>
        /// Dieser Wert wird nicht verwendet, wenn eine lokale Datei abgespielt wird.
        /// </remarks>
        private string m_Target = null;

        /// <summary>
        /// Informationen über die Einstellungen zur Verbindung an einen VCR.NET Recording
        /// Service.
        /// </summary>
        public readonly IRemoteInfo RemoteInfo;

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz.
        /// </summary>
        /// <param name="main">Die zugehörige Anwendung.</param>
        protected UDPAdaptor( IViewerSite main )
            : base( main )
        {
            // Alternate interfaces
            RemoteInfo = main as IRemoteInfo;
        }

        /// <summary>
        /// Verbindet den eingehenden Datenstrom.
        /// </summary>
        /// <param name="host">Optionale Multicast Adresse für den Empfang.</param>
        /// <param name="port">TCP/IP Port, an dem die Daten entgegen genommen werden sollen.</param>
        /// <param name="server">Optionale Angabe des VCR.NET Servers.</param>
        protected void Connect( string host, ushort port, string server )
        {
            // The endpoint
            TransportStreamReceiver endPoint;

            // Host name to use
            if (string.IsNullOrEmpty( host ))
            {
                // Find us
                var self = Dns.GetHostEntry( Dns.GetHostName() );
                if (string.IsNullOrEmpty( server ))
                {
                    // Use us
                    host = self.HostName;
                }
                else
                {
                    // Check for local access
                    var remote = Dns.GetHostEntry( server );

                    // See if these are the same
                    host = StringComparer.InvariantCultureIgnoreCase.Equals( self.HostName, remote.HostName ) ? "localhost" : self.HostName;
                }

                // Use Unicast mode
                endPoint = new TransportStreamReceiver( port );

                // Where to connect to
                m_Target = string.Format( "{0}:{1}", host, endPoint.Port );
            }
            else
            {
                // Use Multicast mode
                endPoint = new TransportStreamReceiver( IPAddress.Parse( host ), port );

                // Where to connect to
                m_Target = string.Format( "*{0}:{1}", host, endPoint.Port );
            }

            // Register accessor
            SetAccessor( endPoint );

            // Register with accessor
            ConnectWaiter();

            // Finsihed
            OnConnected();
        }

        /// <summary>
        /// Aktiviert die Überwachung eines gesteuerten Eingangsdatenstroms.
        /// </summary>
        public override void ConnectWaiter()
        {
            // Register with accessor
            Accessor.OnWaitData += OnWaitData;
        }

        /// <summary>
        /// Deaktiviert die Überwachung eines gesteuerten Eingangsdatenstroms.
        /// </summary>
        public override void DisconnectWaiter()
        {
            // Unregister from accessor
            Accessor.OnWaitData -= OnWaitData;
        }

        /// <summary>
        /// Das TCP/IP UDP Modul zur Übertragung des Transport Streams an den DirectShow
        /// Graphen.
        /// </summary>
        public new TransportStreamReceiver Accessor { get { return (TransportStreamReceiver) base.Accessor; } }

        /// <summary>
        /// Meldet die lokale Empfangsstelle für den VCR.NET Transport Stream.
        /// </summary>
        public string Target { get { return m_Target; } }
    }
}
