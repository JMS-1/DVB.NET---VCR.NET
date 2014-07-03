using System;
using System.Collections.Generic;
using System.Threading;


namespace JMS.DVBVCR.RecordingService.WebServer
{
    /// <summary>
    /// Diese Hilfsklasse dient zur Verwaltung einer ASP.NET Laufzeitumgebung <see cref="ServerRuntime"/>
    /// in einer anderen <see cref="AppDomain"/>.
    /// </summary>
    public class ServerHost : IDisposable
    {
        /// <summary>
        /// Die Anwendung des <i>VCR.NET Recording Service</i> selbst.
        /// </summary>
        private class PrimaryEndPoint : ApplicationEndPoint<ServerRuntime>
        {
            /// <summary>
            /// Der Dienst selbst.
            /// </summary>
            private readonly VCRServer m_server;

            /// <summary>
            /// Erstellt eine Verwaltung der Anwendung.
            /// </summary>
            /// <param name="server">Der Dienst selbst.</param>
            public PrimaryEndPoint( VCRServer server )
                : base( "VCR.NET", string.Empty )
            {
                m_server = server;

                Start();
            }

            /// <summary>
            /// Wird aufgerufen, sobald die Laufzeitumgebung gestartet wurde.
            /// </summary>
            /// <param name="runtime">Die neu angelegte Laufzeitumgebung.</param>
            protected override void RuntimeStarted( ServerRuntime runtime )
            {
                runtime.SetServer( m_server );
            }
        }

        /// <summary>
        /// Der zugehörige Dienst.
        /// </summary>
        private readonly VCRServer m_server = null;

        /// <summary>
        /// Die Verwaltung eingehender HTTP Anfragen.
        /// </summary>
        private List<IDisposable> m_endPoints = new List<IDisposable>();

        /// <summary>
        /// Erzeugt eine neue Verwaltungsinstanz.
        /// </summary>
        public ServerHost( VCRServer server )
        {
            // Remember
            m_server = server;
        }

        /// <summary>
        /// Erzeugt eine neue Laufzeitumgebung und bindet diese an einen lokalen
        /// TCP/IP Port.
        /// </summary>
        /// <remarks>
        /// Als virtuelles Verzeichnis wird <i>/VCR.NET</i> verwendet. Dieses wird
        /// physikalisch an das Verzeichnis gebunden, dass der aktuellen Anwendung
        /// übergeordnet ist. Es empfiehlt sich, die Anwendung in einem Unterverzeichnis
        /// <i>bin</i> unterzubringen.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Es wurde bereits eine Laufzeitumgebung
        /// angelegt.</exception>
        public void Start()
        {
            // Report
            Tools.ExtendedLogging( "Starting Web Server" );

            // Already running
            if (m_endPoints.Count > 0)
                return;

            // Create
            m_endPoints.Add( new PrimaryEndPoint( m_server ) );

            // Report
            Tools.ExtendedLogging( "Listener is up and running" );

            // Report
            VCRServer.Log( LoggingLevel.Full, Properties.Resources.WebServerStarted );
        }

        /// <summary>
        /// Beendet die aktuelle ASP.NET Laufzeitumgebung.
        /// <seealso cref="ServerRuntime.Stop"/>
        /// </summary>
        public void Stop()
        {
            // Terminate all end-points we started
            foreach (var endPoint in Interlocked.Exchange( ref m_endPoints, new List<IDisposable>() ))
                try
                {
                    // Try shutdown
                    endPoint.Dispose();
                }
                catch (Exception)
                {
                    // Ignore any error
                }

            // Report
            VCRServer.Log( LoggingLevel.Full, Properties.Resources.WebServerStopped );
        }

        #region IDisposable Members

        /// <summary>
        /// Dient zur Freigabe aller verwendeten Ressourcen.
        /// <seealso cref="Stop"/>
        /// </summary>
        public void Dispose()
        {
            // Finish
            Stop();
        }

        #endregion
    }
}
