using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Web.Hosting;


namespace JMS.DVBVCR.RecordingService.WebServer
{
    /// <summary>
    /// Diese Hilfsklasse dient zur Verwaltung einer ASP.NET Laufzeitumgebung <see cref="ServerRuntime"/>
    /// in einer anderen <see cref="AppDomain"/>.
    /// </summary>
    public class ServerHost : IDisposable
    {
        /// <summary>
        /// Die ASP.NET Laufzeitumgebung.
        /// </summary>
        private ServerRuntime m_Runtime = null;

        /// <summary>
        /// 
        /// </summary>
        private object m_Sync = new object();

        /// <summary>
        /// 
        /// </summary>
        private VCRServer m_Server = null;

        /// <summary>
        /// Die Verwaltung eingehender HTTP Anfragen.
        /// </summary>
        private HttpListener m_Listener = null;

        /// <summary>
        /// Die Anzahl der gestarteten aber noch nicht zu Ende durchgeführten Operationen.
        /// </summary>
        private int m_Threads = 0;

        /// <summary>
        /// Erzeugt eine neue Verwaltungsinstanz.
        /// </summary>
        public ServerHost( VCRServer server )
        {
            // Remember
            m_Server = server;
        }

        /// <summary>
        /// Bearbeitet einen einzelnen HTTP Aufruf.
        /// </summary>
        /// <param name="result">Der aktuelle Aufruf.</param>
        private void StartContext( IAsyncResult result )
        {
            // No longer active
            var listener = m_Listener;
            if (listener == null)
                return;
            if (!listener.IsListening)
                return;

            // Correct counter
            Interlocked.Increment( ref m_Threads );

            // Check current state
            bool endContext = true;

            // With cleanup
            try
            {
                // Context to process
                HttpListenerContext context;

                // Safe load
                try
                {
                    // Read the context
                    context = listener.EndGetContext( result );

                    // Got a full request
                    endContext = false;

                    // Special
                    if (context == null)
                        VCRServer.Log( LoggingLevel.Full, "Lost Context - Web Server may be in Error" );
                }
                catch (Exception e)
                {
                    // This can be quite normal on XP so better only do a light report
                    Tools.ExtendedLogging( "EndGetContext Exception: {0}", e.Message );

                    // Reset
                    context = null;
                }

                // Needs cleanup
                try
                {
                    // Start next
                    if (listener.IsListening)
                        listener.BeginGetContext( StartContext, null );

                    // Process
                    if (context != null)
                        RunTime.ProcessRequest( new ContextAccessor( context ) );
                }
                finally
                {
                    // Be safe
                    try
                    {
                        // Release respose
                        if (context != null)
                            context.Response.Close();
                    }
                    catch
                    {
                        // Ignore any error during cleanup
                    }
                }
            }
            catch (ThreadAbortException e)
            {
                // Report
                VCRServer.Log( e );
            }
            catch (HttpListenerException e)
            {
                // Report
                if (!endContext)
                    VCRServer.Log( e );
            }
            catch (Exception e)
            {
                // Report
                VCRServer.Log( e );
            }
            finally
            {
                // Correct counter
                Interlocked.Decrement( ref m_Threads );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private ServerRuntime RunTime
        {
            get
            {
                // Synchronize
                lock (m_Sync)
                {
                    // Test it
                    try
                    {
                        // See if the domain is still up and running
                        if (null != m_Runtime) m_Runtime.Test();
                    }
                    catch
                    {
                        // Report to event log
                        VCRServer.Log( LoggingLevel.Errors, EventLogEntryType.Warning, Properties.Resources.WebServerReload );

                        // Reset
                        m_Runtime = null;
                    }

                    // Load once
                    if (null == m_Runtime)
                    {
                        // Create
                        m_Runtime = (ServerRuntime) ApplicationHost.CreateApplicationHost( typeof( ServerRuntime ), "/VCR.NET", Tools.ApplicationDirectory.Parent.FullName );

                        // Run it with a reference back to this application domain
                        m_Runtime.SetServer( m_Server );
                    }

                    // Report
                    return m_Runtime;
                }
            }
        }

        /// <summary>
        /// Meldet das Laufzeitverzeichnis aller Binärdateien für die ASP.NET Umgebung.
        /// </summary>
        public static DirectoryInfo BinariesDirectory
        {
            get
            {
                // Report
                return new DirectoryInfo( Path.Combine( Tools.ApplicationDirectory.Parent.FullName, "bin" ) );
            }
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
            if (m_Listener != null)
                return;

            // Attach to the configuration
            var configuration = VCRConfiguration.Current;

            // Report
            Tools.ExtendedLogging( "Starting Listener on Port {0}", configuration.WebServerTcpPort );

            // Create listener
            m_Listener = new HttpListener { AuthenticationSchemes = AuthenticationSchemes.IntegratedWindowsAuthentication };

            // May allow in addition Basic authentication
            if (configuration.EnableBasicAuthentication)
                m_Listener.AuthenticationSchemes |= AuthenticationSchemes.Basic;

            // Regular HTTP sink
            m_Listener.Prefixes.Add( string.Format( "http://*:{0}/VCR.NET/", configuration.WebServerTcpPort ) );

            // Secure HTTP sink
            if (configuration.EncryptWebCommunication)
                m_Listener.Prefixes.Add( string.Format( "https://*:{0}/VCR.NET/", configuration.WebServerSecureTcpPort ) );

            // Finish configuration
            m_Listener.AuthenticationSchemeSelectorDelegate = SelectAuthentication;
            m_Listener.IgnoreWriteExceptions = false;

            // Startup
            m_Listener.Start();
            m_Listener.BeginGetContext( StartContext, null );

            // Report
            Tools.ExtendedLogging( "Listener is up and running" );

            // Report
            VCRServer.Log( LoggingLevel.Full, Properties.Resources.WebServerStarted );
        }

        private AuthenticationSchemes SelectAuthentication( HttpListenerRequest request )
        {
            // Must authenticate
            var configuration = VCRConfiguration.Current;
            if (configuration.EnableBasicAuthentication)
                return AuthenticationSchemes.Basic | AuthenticationSchemes.IntegratedWindowsAuthentication;
            else
                return AuthenticationSchemes.IntegratedWindowsAuthentication;
        }

        /// <summary>
        /// Beendet die aktuelle ASP.NET Laufzeitumgebung.
        /// <seealso cref="ServerRuntime.Stop"/>
        /// </summary>
        public void Stop()
        {
            // Forward
            var listener = m_Listener;
            if (listener != null)
                if (listener.IsListening)
                    try
                    {
                        // Process
                        listener.Stop();
                    }
                    catch
                    {
                        // Report to event log
                        VCRServer.Log( LoggingLevel.Errors, EventLogEntryType.Warning, Properties.Resources.WebServerStopFailedChannel );
                    }

            // Wait for all outstanding requests to terminate
            while (Thread.VolatileRead( ref m_Threads ) > 0)
                Thread.Sleep( 100 );

            // See if worker is up
            var runtime = m_Runtime;
            if (runtime != null)
                try
                {
                    // Save call - may be already dead
                    runtime.Stop();

                    // Terminate the domain
                    AppDomain.Unload( runtime.AppDomain );
                }
                catch
                {
                    // Report to event log
                    VCRServer.Log( LoggingLevel.Errors, EventLogEntryType.Warning, Properties.Resources.WebServerStopFailed );
                }

            // Done
            m_Listener = null;
            m_Runtime = null;

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
