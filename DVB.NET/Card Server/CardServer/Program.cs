using System;
using System.IO.Pipes;
using System.Diagnostics;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Mit dieser Klasse werden <i>Card Server</i> Instanzen gestartet.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Installiert die Laufzeitumgebung.
        /// </summary>
        static Program()
        {
            // Activate dynamic loading
            RunTimeLoader.Startup();
        }

        /// <summary>
        /// Der Einsprungpunkt für einen <i>Card Server</i>. Die Befehlszeilenparameter beschreiben
        /// die Kommunikationskanäle zum steuernden Client.
        /// </summary>
        /// <param name="args">Befehlsparameter für die Kommunikation.</param>
        public static void Main( string[] args )
        {
            // Be safe
            try
            {
                // Always use the configured language
                UserProfile.ApplyLanguage();

                // Set priority
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

                // Open the communication channels and attach to the pipe server
                using (AnonymousPipeClientStream reader = new AnonymousPipeClientStream( PipeDirection.In, args[0] ))
                using (AnonymousPipeClientStream writer = new AnonymousPipeClientStream( PipeDirection.Out, args[1] ))
                using (ServerImplementation server = ServerImplementation.CreateInMemory())
                    for (Request request; null != (request = Request.ReceiveRequest( reader )); )
                    {
                        // Process it
                        Response response = request.Execute( server );

                        // Send the response
                        response.SendResponse( writer );
                    }
            }
            catch (Exception e)
            {
                // Report error
                Logging.Log( EventLogEntryType.Error, e.ToString() );
            }
        }
    }
}
