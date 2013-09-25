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
        static void Main_EPGLookup( string[] args )
        {
            // Create the server
            using (VCRServer server = new VCRServer( new DirectoryInfo( @"C:\temp\VCR.NET 3.9 Alpha\Jobs" ) ))
            {
                // Create filter
                GuideEntryFilter filter =
                    new GuideEntryFilter
                        {
                            MaximumEntriesPerStation = 30,
                            MinimumDateTime = DateTime.UtcNow,
                            MinimumIsCurrent = true,
                            Profile = null,
                            Station = "ZDF [ZDFvision]",
                            ContentFilter = null
                        };

                // Load a bit of events
                ProgramGuideEntry[] entries = server.GetProgramGuideEntries( filter );
            }
        }

#if DISABLED
        static void Main_EPGScan( string[] args )
        {
            // Create the server
            using (VCRServer server = new VCRServer( new DirectoryInfo( @"C:\temp\VCR.NET 3.9 Alpha\Jobs" ) ))
            {
                // Attach to the profile state of interest
                ProfileState state = server.FindProfile( "DVB-S2" );

                // Report all
                Console.WriteLine( ProgramGuideManager.UpdateEnabled );
                Console.WriteLine( state.ProgramGuide.NextUpdateTime );

                // Create request
                using (ProgramGuideRequest request = state.CreateProgramGuideRequest())
                    if (null != request)
                    {
                        // Manipulate
                        request.Recording.EndsAt = DateTime.UtcNow.AddSeconds( 30 );

                        // Get the information
                        FullInfo info0 = request.CreateFullInformation();

                        // Start it
                        request.Start();

                        // Wait for end
                        for (; request.IsRunning; Thread.Sleep( 2500 ))
                            Console.WriteLine( "Collecting... {0}", DateTime.Now );

                        // Get the information
                        FullInfo info1 = request.CreateFullInformation();

                        // Report
                        Console.WriteLine( info1.Recording.TotalSize );
                    }
            }
        }
#endif
    }
}
