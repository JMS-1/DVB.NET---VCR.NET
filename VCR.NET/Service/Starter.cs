using System;
using System.Diagnostics;
using System.IO;
using JMS.DVB;
using JMS.DVBVCR.RecordingService.WebServer;


namespace JMS.DVBVCR.RecordingService
{
    /// <summary>
    /// Allgemeine Klasse mit der Startmethode für den VCR.NET Recording Service.
    /// </summary>
    public class Starter
    {
        /// <summary>
        /// Ermöglicht das dynamische Laden der DVB.NET Bibliotheken.
        /// </summary>
        static Starter()
        {
            // Start dynamic loading
            RunTimeLoader.Startup();
        }

        /// <summary>
        /// Startet den VCR.NET Recording Service über <see cref="Service.Startup"/>.
        /// Alle <see cref="Exception"/> Meldungen werden über <see cref="Tools.LogException"/>
        /// in eine Datei protokolliert.
        /// </summary>
        /// <param name="args">
        /// Parameter beim Aufruf des VCR.NET Recording Service.
        /// <seealso cref="Service.Startup"/>
        /// </param>
        [STAThread]
        public static void Main( string[] args )
        {
            // Must be fully safe
            try
            {
                // See if debugger attached
                Tools.EnableTracing = Debugger.IsAttached;
                Tools.DomainName = "VCR.NET";

                // Dump environment
                //foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables())
                //    Tools.ExtendedLogging( "Environment {0}={1}", env.Key, env.Value );

                // Load configuration into main application domain
                VCRConfiguration.Startup();

                // Cleanup mess from earlier version
                try
                {
                    // Clear
                    var target = Path.Combine( ServerHost.BinariesDirectory.FullName, "JMS.DVB.SourceManagement.dll" );
                    if (File.Exists( target ))
                        File.Delete( target );
                }
                catch (Exception e)
                {
                    // Report
                    VCRServer.LogError( "DVB.NET Cleanup failed: {0}", e.Message );
                }

                // Show startup
                Tools.ExtendedLogging( "Starting Service" );

                // Run
                Service.Startup( args );

                // Show termination
                Tools.ExtendedLogging( "Terminating Service" );
            }
            catch (Exception e)
            {
                // Report
                Tools.LogException( "Main", e );

                // Re-Throw
                throw e;
            }
        }
    }
}
