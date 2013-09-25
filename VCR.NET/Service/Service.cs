using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using JMS.DVBVCR.RecordingService.Win32Tools;


namespace JMS.DVBVCR.RecordingService
{
    /// <summary>
    /// Die Windows Dienstschnittstelle des VCR.NET Recording Service.
    /// </summary>
    public class Service : ServiceBase
    {
        #region Fields

        /// <summary>
        /// Verwaltung aller abhängigen Komponenten.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// ASP.NET Web Server für die Kommunikation mit VCR.NET Clients.
        /// </summary>
        private WebServer.ServerHost m_WebServer;

        /// <summary>
        /// Ermöglicht das periodische Abfragen des Schlafzustands.
        /// </summary>
        private volatile Thread m_HibernateHelper = null;

        /// <summary>
        /// Gesetzt, wenn die periodische Überwachung des Schlafzustands aktiv ist.
        /// </summary>
        private volatile bool m_HibernateHelperIsActive = true;

        /// <summary>
        /// Die tatsächliche VCR.NET Instanz.
        /// </summary>
        internal VCRServer VCRServer { get; private set; }

        /// <summary>
        /// Steuert den Start des Web Servers.
        /// </summary>
        private Thread m_webStarter;

        #endregion

        #region Win32 API

        /// <summary>
        /// Rückrufmethode für <see cref="SetConsoleCtrlHandler"/>.
        /// </summary>
        private delegate bool ConsoleHandler( UInt32 commandType );

        /// <summary>
        /// Die Win32 API Methode <see cref="SetConsoleCtrlHandler"/>.
        /// </summary>
        [DllImport( "kernel32.dll", EntryPoint = "SetConsoleCtrlHandler" )]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool SetConsoleCtrlHandler( ConsoleHandler callback, bool add );

        /// <summary>
        /// Die Win32 API Methode <see cref="AdjustTokenPrivileges"/>.
        /// </summary>
        [DllImport( "Advapi32.dll", EntryPoint = "AdjustTokenPrivileges" )]
        [SuppressUnmanagedCodeSecurity]
        static private extern bool AdjustTokenPrivileges( IntPtr handle, bool disableAllPrivileges, ref TokenPrivileges newState, UInt32 bufferLength, IntPtr previousState, IntPtr returnLength );

        /// <summary>
        /// Die Win32 API Methode <see cref="OpenProcessToken"/>.
        /// </summary>
        [DllImport( "Advapi32.dll", EntryPoint = "OpenProcessToken" )]
        [SuppressUnmanagedCodeSecurity]
        static private extern bool OpenProcessToken( IntPtr process, UInt32 desiredAccess, out IntPtr token );

        /// <summary>
        /// Die Win32 API Methode <see cref="LookupPrivilegeValue"/>.
        /// </summary>
        [DllImport( "Advapi32.dll", EntryPoint = "LookupPrivilegeValue" )]
        [SuppressUnmanagedCodeSecurity]
        static private extern bool LookupPrivilegeValue( string systemName, string name, out UInt64 LUID );

        /// <summary>
        /// Die Win32 API Methode <see cref="CloseHandle"/>.
        /// </summary>
        [DllImport( "kernel32.dll", EntryPoint = "CloseHandle" )]
        [SuppressUnmanagedCodeSecurity]
        static private extern bool CloseHandle( IntPtr handle );

        /// <summary>
        /// Parameter für <see cref="AdjustTokenPrivileges"/>
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct TokenPrivileges
        {
            /// <summary>
            /// Anzahl der zu bearbeitenden Rechte.
            /// </summary>
            public UInt32 PrivilegeCount;

            /// <summary>
            /// LUID.
            /// </summary>
            public UInt64 Luid;

            /// <summary>
            /// Besondere Optionen.
            /// </summary>
            public UInt32 Attributes;
        }

        #endregion

        /// <summary>
        /// Erzeuge eine neue Dienstinstanz.
        /// </summary>
        /// <remarks>
        /// Die Implementierung sorgt dafür, dass das OnPowerEvent Verhalten von .NET 1.1
        /// reaktiviert wird und der Dienst so in den Übergang in den und aus dem 
        /// Schlafzustand eingreifen kann.
        /// </remarks>
        public Service()
        {
            // Report
            Tools.ExtendedLogging( "Creating new Service Instance" );

            // This call is required by the Windows.Forms Component Designer.
            InitializeComponent();

            // Report
            Tools.ExtendedLogging( "Service Instance properly initialized" );
        }

        #region Shutdown

        /// <summary>
        /// Bereitet den VCR.NET Recording Service auf die endgültige Terminierung
        /// des Dienstes vor.
        /// </summary>
        private void Terminate()
        {
            // Get the hibernation control thread
            var hibTest = m_HibernateHelper;

            // Reset it
            m_HibernateHelper = null;

            // Unregister from power manager
            PowerManager.OnChanged -= HibernationChanged;

            // Wait for the web server starter thread to finish
            var webThread = Interlocked.Exchange( ref m_webStarter, null );
            if (webThread != null)
                webThread.Join();

            // Stop web server - client requests are no longer accepted
            if (m_WebServer != null)
                m_WebServer.Stop();

            // Stop all recordings
            using (VCRServer)
                VCRServer = null;

            // Finally wait for the hibernation helper to terminate (at most five seconds)
            if (hibTest != null)
                hibTest.Join();
        }

        /// <summary>
        /// Nimmt ein Windows <i>Console</i> Ereignis entgegen.
        /// </summary>
        /// <param name="eventType">Vom Anwender ausgelöstes Ereignis.</param>
        /// <returns>Beeiflußt die weitere Bearbeitung des Ereignisses.</returns>
        private bool ConsoleEvent( UInt32 eventType )
        {
            // Only for shut down
            if (eventType == 6)
                Terminate();

            // Continue
            return false;
        }

        /// <summary>
        /// Der SCM möchte den VCR.NET Recording Service beenden.
        /// </summary>
        protected override void OnStop()
        {
            // Shut down self
            Terminate();

            // Show event
            VCRServer.Log( LoggingLevel.Full, Properties.Resources.ServiceStopped );
        }

        /// <summary>
        /// Der Rechner wird heruntergefahren.
        /// </summary>
        protected override void OnShutdown()
        {
            // Show event
            VCRServer.Log( LoggingLevel.Full, Properties.Resources.GotShutdown );

            // Forward
            OnStop();
        }

        /// <summary>
        /// Freigabe aller Ressourcen.
        /// </summary>
        /// <param name="disposing">Die .NET Komponente zum Dienst wird beendet.</param>
        protected override void Dispose( bool disposing )
        {
            // See if this is a regular dispose operation
            if (disposing)
                using (components)
                    components = null;

            // Forward to base
            base.Dispose( disposing );
        }

        #endregion

        #region Startup

        /// <summary>
        /// Die .NET Komponente zum Dienst wird gestartet.
        /// </summary>
        private void InitializeComponent()
        {
            // Service
            ServiceName = "VCR.NET Service";
            CanHandlePowerEvent = true;
            CanShutdown = true;
            AutoLog = false;
        }

        /// <summary>
        /// Diese Methode wird beim Starten der VCR.NET Recording Service
        /// Anwendung aufgerufen.
        /// </summary>
        /// <param name="args">Befehlszeilenargumente für den Aufruf der Anwendung.</param>
        public static void Startup( string[] args )
        {
            // Report
            Tools.ExtendedLogging( "Parsing Command Line '{0}'", string.Join( " ", args ) );

            // Error handling
            try
            {
                // First index to check
                int index = 0;

                // Check for debug mode
                if (args.Length > index)
                    if (string.Equals( args[index], "Console" ))
                    {
                        // Report
                        Tools.ExtendedLogging( "Stand-Alone Mode activated" );

                        // Enable debug mode
                        Tools.DebugMode = true;

                        // Skip argument
                        ++index;
                    }

                // Check special mode
                if ((args.Length > index) && string.Equals( args[index], "Restart" ))
                {
                    // Report
                    Tools.ExtendedLogging( "Restart Request detected" );

                    // Call debugger or delay to allow web response send to client
                    if (Tools.DebugMode)
                        Debugger.Break();
                    else
                        Thread.Sleep( 1000 );

                    // Attach to controller
                    using (var service = new ServiceController( "VCR.NET Service" ))
                        if (service.Status == ServiceControllerStatus.Running)
                        {
                            // Stop
                            service.Stop();

                            // Wait for finish
                            service.WaitForStatus( ServiceControllerStatus.Stopped );

                            // Restart
                            service.Start();
                        }
                }
                else if (Tools.DebugMode)
                {
                    // Create
                    using (var vcrnet = new Service())
                    {
                        // Start
                        vcrnet.OnStart( new string[0] );

                        // Wait
                        Application.Run( new DebuggerForm( vcrnet ) );

                        // Stop
                        vcrnet.OnStop();
                    }
                }
                else
                {
                    // Start SCM processings
                    Run( new Service() );
                }
            }
            catch (Exception e)
            {
                // Report
                using (var log = new EventLog( "Application", ".", "VCR.NET Recording Service" ))
                    log.WriteEntry( e.ToString(), EventLogEntryType.Error );
            }
        }

        /// <summary>
        /// Der SCM möchte den VCR.NET Recording Service starten.
        /// </summary>
        /// <param name="args">Befehlszeilenparameter für den Start.</param>
        protected override void OnStart( string[] args )
        {
            // Fully save
            try
            {
                // Report
                Tools.ExtendedLogging( "SCM has requested a Service Start" );

                // Report delay
                if (!Tools.DebugMode)
                    RequestAdditionalTime( 300000 );

                // Create server instance
                VCRServer = new VCRServer( Tools.ApplicationDirectory.Parent );

                // Create web server instance
                m_WebServer = new WebServer.ServerHost( VCRServer );

                // Report
                Tools.ExtendedLogging( "Attaching Console Control Handler" );

                // Register shutdown handler
                if (!SetConsoleCtrlHandler( ConsoleEvent, true ))
                    VCRServer.Log( LoggingLevel.Errors, Properties.Resources.ShutdownHandlerFailed );

                // Report
                Tools.ExtendedLogging( "Attaching Process" );

                // Get a token for this process
                IntPtr processToken;
                if (!OpenProcessToken( Process.GetCurrentProcess().Handle, 0x28, out processToken ))
                    return;

                // Report
                Tools.ExtendedLogging( "Aquiring Shutdown Privileges" );

                // Helper
                TokenPrivileges privileges = new TokenPrivileges();

                // Lookup the privilege value
                if (LookupPrivilegeValue( null, "SeShutdownPrivilege", out privileges.Luid ))
                {
                    // Finish
                    privileges.PrivilegeCount = 1;
                    privileges.Attributes = 2;

                    // Update
                    if (!AdjustTokenPrivileges( processToken, false, ref privileges, 0, IntPtr.Zero, IntPtr.Zero ))
                        VCRServer.Log( LoggingLevel.Security, EventLogEntryType.Warning, Properties.Resources.AquirePrivilegeFailed );
                }
                else
                {
                    // Report
                    VCRServer.Log( LoggingLevel.Security, EventLogEntryType.Error, Properties.Resources.LookupPrivilegeFailed );
                }

                // Release
                CloseHandle( processToken );

                // Report
                Tools.ExtendedLogging( "Web Server Start initiated from Thread {0}", Thread.CurrentThread.ManagedThreadId );

                // Create the web server on a separate thread
                m_webStarter = new Thread( () =>
                    {
                        // Be safe
                        try
                        {
                            // Report
                            Tools.ExtendedLogging( "Starting Web Server on Thread {0}", Thread.CurrentThread.ManagedThreadId );

                            // Start Web Server
                            m_WebServer.Start();
                        }
                        catch (Exception e)
                        {
                            // Report
                            EventLog.WriteEntry( e.ToString(), EventLogEntryType.Error );
                        }
                    } );

                // Start it
                m_webStarter.Start();

                // Report
                Tools.ExtendedLogging( "Final Initialisation started" );

                // Start our child process and report success
                VCRServer.Log( LoggingLevel.Full, Properties.Resources.ServiceStarted );

                // Register with power manager
                PowerManager.OnChanged += HibernationChanged;

                // Run initial scan
                PowerManager.OnResume();

                // Create hibernate thread
                m_HibernateHelper = new Thread( ReTestShutdown ) { Name = "Periodic Check for Hibernation" };
                m_HibernateHelper.Start();

                // Report
                Tools.ExtendedLogging( "Returning Control to SCM" );
            }
            catch (Exception e)
            {
                // Report
                Tools.LogException( "OnStart", e );

                // Process
                throw e;
            }
        }

        /// <summary>
        /// Prüft periodisch, ob ein Test auf den Schlafzustand erfolgen soll.
        /// </summary>
        private void ReTestShutdown()
        {
            // Forever - each five seconds
            for (; m_HibernateHelper != null; Thread.Sleep( 5000 ))
                if (m_HibernateHelperIsActive)
                    VCRServer.ExtensionProcessManager.Cleanup();
        }

        #endregion

        #region Hibernation

        /// <summary>
        /// Aktuelle Schachtelungstiefe von <see cref="OnPowerEvent"/> Aufrufen.
        /// </summary>
        private int m_PowerEventDepth = 0;

        /// <summary>
        /// Löst einen Befehl zum Schlafzustand aus.
        /// </summary>
        /// <param name="powerStatus">Der gewünschte Befehl.</param>
        internal void SendPowerCommand( PowerBroadcastStatus powerStatus )
        {
            // Forward
            OnPowerEvent( powerStatus );
        }

        /// <summary>
        /// Verarbeitet den Übergang von dem und in den Schlafzustand.
        /// </summary>
        /// <param name="powerStatus">Zu bearbeitender Übergang.</param>
        /// <returns>Gesetzt, wenn der Übergang erlaubt ist.</returns>
        protected override bool OnPowerEvent( PowerBroadcastStatus powerStatus )
        {
            // Make sure that we are not interrupted by power management operations
            Interlocked.Increment( ref m_PowerEventDepth );
            try
            {
                // Show up
                Tools.ExtendedLogging( "Power Event {0} caught", powerStatus );

                // Report
                VCRServer.Log( LoggingLevel.Full, Properties.Resources.GotPowerEvent, powerStatus, DateTime.Now );

                // Forward calls
                switch (powerStatus)
                {
                    case PowerBroadcastStatus.QuerySuspendFailed:
                    case PowerBroadcastStatus.ResumeSuspend:
                    case PowerBroadcastStatus.ResumeAutomatic:
                    case PowerBroadcastStatus.ResumeCritical:
                        {
                            // Be safe
                            try
                            {
                                // Report to server to enable internal management operations
                                VCRServer.Resume();

                                // Check for next job
                                PowerManager.OnResume();
                            }
                            finally
                            {
                                // Reactive hibernation control in all situations
                                m_HibernateHelperIsActive = true;
                            }

                            // Final report
                            Tools.ExtendedLogging( "VCR.NET Recording Service has been resumed" );

                            // Done
                            break;
                        }
                    case PowerBroadcastStatus.Suspend:
                        {
                            // No longer try to suspend us
                            m_HibernateHelperIsActive = false;

                            // Prepare to suspend
                            VCRServer.PrepareSuspend();

                            // Inform runtime
                            VCRServer.Suspend();

                            // See if anything is down
                            if (!PowerManager.HibernationAllowed)
                                VCRServer.Log( LoggingLevel.Errors, "Suspend not allowed - may lead to trouble after resume!" );

                            // Final report
                            Tools.ExtendedLogging( "VCR.NET Recording Service is now suspended" );

                            // Done
                            break;
                        }
                }

                // Beginning with .NET 2.0 return value is ignored - starting with Windows Vista there is no support for cancelling suspension
                return true;
            }
            catch (Exception e)
            {
                // Report
                VCRServer.Log( e );

                // Forward
                throw;
            }
            finally
            {
                // Decrement counter back to the original value
                Interlocked.Decrement( ref m_PowerEventDepth );
            }
        }

        /// <summary>
        /// Bearbeitet einen Änderung an der Erlaubnis zum Übergang in den
        /// Schlafzustand.
        /// </summary>
        /// <param name="forbidHibernation">Gesetzt, wenn eine Verweigerung
        /// aktiviert wurde.</param>
        private void HibernationChanged( bool forbidHibernation )
        {
            // We can do nothing
            if (forbidHibernation)
                return;

            // We are currently inside the power manager
            if (Thread.VolatileRead( ref m_PowerEventDepth ) > 0)
                return;

            // Process
            new Thread( VCRServer.TestForHibernation ).Start();
        }

        #endregion
    }
}
