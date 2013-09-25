using System;
using System.IO;
using JMS.DVBVCR.RecordingService;
using JMS.DVBVCR.RecordingService.Requests;
using JMS.DVBVCR.RecordingService.Status;
using JMS.DVBVCR.RecordingService.ProgramGuide;
using System.Threading;
using JMS.DVBVCR.RecordingService.Persistence;

namespace Upgrade_Test
{
    partial class Program
    {
        static void Main_FindRecording( string[] args )
        {
            // Create the server
            using (VCRServer server = new VCRServer( new DirectoryInfo( @"C:\temp\VCR.NET 3.9 Alpha\Jobs" ) ))
            {
                // Attach to the state of a profile
                ProfileState nexus = server.FindProfile( "Nexus" );

                // Get the next job to execute
                VCRRecordingInfo next = nexus.NextJob;

                // Get recording information for the next jon
                bool incomplete;
                VCRRecordingInfo[] hidden;
                FullInfo full = nexus.FindNextRecording( DateTime.UtcNow, out incomplete, out hidden );

                // Prepare it
                using (HardwareRequest request = nexus.CreateRecordingRequest( next, DateTime.UtcNow ))
                {
                    // Set end time
                    request.Recording.EndsAt = DateTime.UtcNow.AddSeconds( 30 );

                    // Report
                    Console.WriteLine( "Recording {0}", request.Recording.FileName );

                    // Process it
                    request.Start();

                    // Wait
                    while (request.IsRunning)
                        Thread.Sleep( 100 );
                }
            }
        }
    }
}
