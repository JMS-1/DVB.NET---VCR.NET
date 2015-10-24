using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Web.Hosting;


namespace JMS.DVBVCR.RecordingService.WebServer
{
    /// <summary>
    /// Verwaltet eine Anwendung im Web Server.
    /// </summary>
    /// <typeparam name="TRuntimeType">Die Art der Laufzeitumgebung.</typeparam>
    public abstract class ApplicationEndPoint<TRuntimeType> : IDisposable where TRuntimeType : ApplicationRuntime
    {
        /// <summary>
        /// Die Empfangsstelle für die Aufrufe.
        /// </summary>
        private HttpListener m_listener;

        /// <summary>
        /// Der Name der Anwendung.
        /// </summary>
        private readonly string m_name;

        /// <summary>
        /// Die Anzahl der gestarteten aber noch nicht zu Ende durchgeführten Operationen.
        /// </summary>
        private int m_Threads = 0;

        /// <summary>
        /// Die ASP.NET Laufzeitumgebung.
        /// </summary>
        private TRuntimeType m_runtime = null;

        /// <summary>
        /// Synchronisiert den Zugriff auf die Laufzeitumgebung.
        /// </summary>
        private object m_Sync = new object();

        /// <summary>
        /// Der relative Pfad zum Anwendungsverzeichnis.
        /// </summary>
        private readonly string m_path;

        /// <summary>
        /// Erstellt eine neue Verwaltung.
        /// </summary>
        /// <param name="name">Der Name der Anwendung.</param>
        /// <param name="path">Der relative Pfad zum Anwendungsverzeichnis.</param>
        public ApplicationEndPoint( string name, string path )
        {
            // Remember
            m_name = name;
            m_path = path;

            // Create listener
            m_listener = new HttpListener { AuthenticationSchemes = AuthenticationSchemes.IntegratedWindowsAuthentication };

            // May allow in addition Basic authentication
            var configuration = VCRConfiguration.Current;
            if (configuration.EnableBasicAuthentication)
                m_listener.AuthenticationSchemes |= AuthenticationSchemes.Basic;

            // Regular HTTP sink
            m_listener.Prefixes.Add( $"http://*:{configuration.WebServerTcpPort}/{name}/" );

            // Secure HTTP sink
            if (configuration.EncryptWebCommunication)
                m_listener.Prefixes.Add( $"https://*:{configuration.WebServerSecureTcpPort}/{name}/" );

            // Finish configuration
            m_listener.AuthenticationSchemeSelectorDelegate = SelectAuthentication;
            m_listener.IgnoreWriteExceptions = false;
        }

        /// <summary>
        /// Aktiviert den Empfang von Anfragen.
        /// </summary>
        protected void Start()
        {
            // Startup
            m_listener.Start();
            m_listener.BeginGetContext( StartContext, null );
        }

        /// <summary>
        /// Meldet die unterstützten Verfahren zu Benutzererkennung.
        /// </summary>
        /// <param name="request">Ein laufender Zugriff.</param>
        /// <returns>Die erlaubten Verfahren.</returns>
        private static AuthenticationSchemes SelectAuthentication( HttpListenerRequest request )
        {
            // Must authenticate
            var configuration = VCRConfiguration.Current;
            if (configuration.EnableBasicAuthentication)
                return AuthenticationSchemes.Basic | AuthenticationSchemes.IntegratedWindowsAuthentication;
            else
                return AuthenticationSchemes.IntegratedWindowsAuthentication;
        }

        /// <summary>
        /// Meldet die aktuelle Laufzeitumgebung.
        /// </summary>
        private TRuntimeType RunTime
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
                        if (m_runtime != null)
                            m_runtime.Test();
                    }
                    catch
                    {
                        // Report to event log
                        VCRServer.Log( LoggingLevel.Errors, EventLogEntryType.Warning, Properties.Resources.WebServerReload );

                        // Reset
                        m_runtime = null;
                    }

                    // Load once
                    if (m_runtime == null)
                    {
                        // Find target
                        var rootDir = Path.Combine( Tools.ApplicationDirectory.Parent.FullName, m_path );
                        var binDir = Path.Combine( rootDir, "bin" );
                        var source = new Uri( GetType().Assembly.CodeBase ).LocalPath;
                        var target = Path.Combine( binDir, Path.GetFileName( source ) );

                        // Must install server runtime to process
                        if (!StringComparer.InvariantCultureIgnoreCase.Equals( source, target ))
                        {
                            // Make sure target exists
                            Directory.CreateDirectory( binDir );

                            // Install file - existing will be overwritten
                            File.Copy( source, target, true );
                        }

                        // Create
                        m_runtime = (TRuntimeType) ApplicationHost.CreateApplicationHost( typeof( ServerRuntime ), $"/{m_name}", rootDir );

                        // Report
                        RuntimeStarted( m_runtime );
                    }

                    // Report
                    return m_runtime;
                }
            }
        }

        /// <summary>
        /// Wird aufgerufen, sobald die Laufzeitumgebung gestartet wurde.
        /// </summary>
        /// <param name="runtime">Die neu angelegte Laufzeitumgebung.</param>
        protected abstract void RuntimeStarted( TRuntimeType runtime );

        /// <summary>
        /// Bearbeitet einen einzelnen HTTP Aufruf.
        /// </summary>
        /// <param name="result">Der aktuelle Aufruf.</param>
        private void StartContext( IAsyncResult result )
        {
            // No longer active
            var listener = m_listener;
            if (listener == null)
                return;
            if (!listener.IsListening)
                return;

            // Correct counter
            Interlocked.Increment( ref m_Threads );

            // Check current state
            var endContext = true;

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
                catch (Exception)
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
        /// Beendet die Anwendung endgültig.
        /// </summary>
        public void Dispose()
        {
            // Forward
            var listener = Interlocked.Exchange( ref m_listener, null );
            if (listener != null)
                if (listener.IsListening)
                    try
                    {
                        // Process
                        listener.Stop();
                    }
                    catch (Exception)
                    {
                        // Report to event log
                        VCRServer.Log( LoggingLevel.Errors, EventLogEntryType.Warning, Properties.Resources.WebServerStopFailedChannel );
                    }

            // Wait for all outstanding requests to terminate
            while (Thread.VolatileRead( ref m_Threads ) > 0)
                Thread.Sleep( 100 );

            // See if worker is up
            var runtime = Interlocked.Exchange( ref m_runtime, null );
            if (runtime != null)
                try
                {
                    // Save call - may be already dead
                    runtime.Stop();

                    // Terminate the domain
                    AppDomain.Unload( runtime.AppDomain );
                }
                catch (Exception)
                {
                    // Report to event log
                    VCRServer.Log( LoggingLevel.Errors, EventLogEntryType.Warning, Properties.Resources.WebServerStopFailed );
                }
        }
    }
}
