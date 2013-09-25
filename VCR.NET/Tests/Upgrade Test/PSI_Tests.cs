using System;
using System.IO;
using JMS.DVBVCR.RecordingService;
using JMS.DVBVCR.RecordingService.Requests;
using JMS.DVBVCR.RecordingService.Status;
using JMS.DVBVCR.RecordingService.ProgramGuide;
using System.Threading;

namespace Upgrade_Test
{
    partial class Program
    {
        static void Main_PSIRun( string[] args )
        {
            // Create the server
            using (VCRServer server = new VCRServer( new DirectoryInfo( @"C:\temp\VCR.NET 3.9 Alpha\Jobs" ) ))
            {
                // Attach to the profile state of interest
                ProfileState nova = server.FindProfile( "DVB-S2" );
                ProfileState nexus = server.FindProfile( "Nexus" );

                // Report
                Console.WriteLine( "{0} {1}", nova.ProfileName, nova.NextSourceUpdateTime );
                Console.WriteLine( "{0} {1}", nexus.ProfileName, nexus.NextSourceUpdateTime );
            }
        }
    }
}
