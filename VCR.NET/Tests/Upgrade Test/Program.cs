using System;
using JMS.DVB;
using System.IO;
using JMS.DVBVCR.RecordingService;
using JMS.DVBVCR.RecordingService.Status;
using JMS.DVBVCR.RecordingService.Win32Tools;

namespace Upgrade_Test
{
    partial class Program
    {
        private static RunTimeLoader m_Loader = RunTimeLoader.Instance;

        static void Main( string[] args )
        {
            // Start server
            using (VCRServer server = new VCRServer( new DirectoryInfo( @"C:\temp\VCR.NET 3.9 Alpha\Jobs" ) ))
            {
                // Load the settings
                Settings settings = server.Settings;

                // Fire our first schedule
                PowerManager.OnResume();

                // Report
                Console.WriteLine( "Running {4} {0} [Hibernate={1} S3={2} NVOD={3}]", string.Join( ",", settings.Profiles.ToArray() ), settings.MayHibernateSystem, settings.UseStandByForHibernation, settings.CanRecordNVOD, VCRServer.CurrentVersion );
                Console.ReadLine();
            }

            // Done
            Console.WriteLine( "Done" );
            Console.ReadLine();
        }

#if OLD
        static void Main7( string[] args )
        {
            // Attach to job manager
            VCRServer server = new VCRServer( new DirectoryInfo( @"C:\temp\VCR.NET 3.9 Alpha\Jobs" ) );
            JobManager jobs = server.JobManager;

            ProgramGuideEntries entries = new ProgramGuideEntries();
            entries.Add( new ProgramGuideEntry { Name = "Te\x0005st" } );
            SerializationTools.Save( entries, @"c:\temp\test.xml" );
            ProgramGuideEntries reload = SerializationTools.Load<ProgramGuideEntries>( new FileInfo( @"c:\temp\test.xml" ) );
        }

        static void Main6( string[] args )
        {
            // Attach to job manager
            VCRServer server = new VCRServer( new DirectoryInfo( @"C:\temp\VCR.NET 3.9 Alpha\Jobs" ) );
            JobManager jobs = server.JobManager;

            // Create EPG collection
            using (ZappingRequest request = ZappingRequest.CreateDefault( "Nexus Scan", jobs ))
                if (null != request)
                {
                    // Get the information
                    FullInfo info0 = request.CreateFullInformation();

                    // Report
                    Console.WriteLine( info0.Recording.JobType );

                    // Start it
                    request.BeginExecute( false );

                    // Go to ZDF
                    LiveModeStatus stat1 = request.SetSource( VCRProfiles.FindSource( "Nexus Scan", "ZDF [ZDFvision]" ), "localhost:5555" );

                    // Report
                    Console.WriteLine( "ZDF on" );
                    Console.ReadLine();

                    // Get status
                    LiveModeStatus stat2 = request.CreateStatus();

                    // Go to PREMIERE
                    LiveModeStatus stat3 = request.SetSource( VCRProfiles.FindSource( "Nexus Scan", "PREMIERE DIREKT [PREMIERE]" ), "localhost:5555" );

                    // Report
                    Console.WriteLine( "PREMIERE on" );
                    Console.ReadLine();

                    // Get status
                    LiveModeStatus stat4 = request.CreateStatus();

                    // Stop it
                    request.Stop();

                    // Get the information
                    FullInfo info1 = request.CreateFullInformation();
                }

            // Done
            Console.WriteLine( "Done" );
            Console.ReadLine();
        }

        static void Main5( string[] args )
        {
            // Attach to job manager
            VCRServer server = new VCRServer( new DirectoryInfo( @"C:\temp\VCR.NET 3.9 Alpha\Jobs" ) );
            JobManager jobs = server.JobManager;

            // Create EPG collection
            using (SourceScanRequest request = SourceScanRequest.CreateDefault( "Nexus Scan", jobs ))
                if (null != request)
                {
                    // Get the information
                    FullInfo info0 = request.CreateFullInformation();

                    // Report
                    Console.WriteLine( info0.Recording.JobType );

                    // Start it
                    request.BeginExecute( false );

                    // Wait for end
                    Console.WriteLine( "Updateing..." );

                    // Process
                    for (string line; !string.IsNullOrEmpty( line = Console.ReadLine() ); )
                    {
                        // Get the information
                        FullInfo info2 = request.CreateFullInformation();

                        // Report
                        Console.WriteLine( info2.Recording.TotalSize );
                    }

                    // Stop it
                    request.Stop();

                    // Get the information
                    FullInfo info1 = request.CreateFullInformation();
                }

            // Done
            Console.WriteLine( "Done" );
            Console.ReadLine();
        }


        static void Main3( string[] args )
        {
            // Attach to job manager
            VCRServer server = new VCRServer( new DirectoryInfo( @"C:\temp\VCR.NET 3.9 Alpha\Jobs" ) );
            JobManager jobs = server.JobManager;

            // Get the profile
            Profile nexus = VCRProfiles.FindProfile( "Nexus" );

            // Find the next recording
            VCRRecordingInfo rec = jobs.FindNextJob( new DateTime( 2009, 2, 19, 20, 0, 0 ), null, nexus );
            if (null != rec)
                if (rec.StartsAt.HasValue)
                    using (RecordingRequest recording = jobs.CreateRecording( rec, rec.StartsAt.Value, nexus ))
                    {
                        // Get the information
                        FullInfo info0 = recording.CreateFullInformation();

                        // Start it
                        recording.BeginExecute( false );

                        // Report
                        Console.WriteLine( "Recording..." );
                        Console.ReadLine();

                        // Get the information
                        FullInfo info1 = recording.CreateFullInformation();
                    }

            // Done
            Console.WriteLine( "Done" );
            Console.ReadLine();
        }

        static void Main2( string[] args )
        {
            // Process all
            Console.WriteLine( VCRProfiles.GetSources( "Nexus" ).Count() );
            Console.WriteLine( VCRProfiles.GetSources( "Nexus2" ).Count() );

            // Create the filter
            ChannelFilter filter =
                new ChannelFilter
                    {
                        Profile = "Nexus",
                        ServiceType = FilterServiceTypes.Radio,
                        Encryption = FilterEncryptionTypes.Free,
                        Name = "^[AB]",
                        NameMatching = FilterNameMatching.RegularExpression
                    };

            // Process
            foreach (Channel channel in VCRProfiles.GetSources( filter ).ToChannels().OrderBy( c => c.UniqueName ))
                Console.WriteLine( "{0} {1} {2} {3}", channel.UniqueName, channel.Station, channel.Source.DisplayName, channel.Source.SelectionKey );

            // Done
            Console.ReadLine();
        }

        static void Main1( string[] args )
        {
            // Attach to job manager
            VCRServer server = new VCRServer( new DirectoryInfo( @"C:\temp\VCR.NET 3.9 Alpha\Jobs" ) );
            JobManager jobs = server.JobManager;

            // Process all profiles
            foreach (Profile profile in VCRProfiles.Profiles)
            {
                // Report
                Console.WriteLine( profile.Name );

                // Start time
                DateTime start = DateTime.UtcNow;

                // Process a bit
                for (int i = 10; i-- > 0; )
                {
                    // Helper data
                    VCRRecordingInfo[] included;
                    bool inComplete;

                    // Find the recording
                    FullInfo info = jobs.FindNextRecording( start, profile, out inComplete, out included );
                    VCRRecordingInfo rec = info.Recording;
                    VCRJob job = jobs[rec.JobUniqueID.Value];
                    VCRSchedule sched = job[rec.ScheduleUniqueID.Value];

                    // Report
                    Console.WriteLine( "\t{0} {1} {2} ({3}) {4} {5} {6} {7}", rec.StartsAt.Value.ToLocalTime(), rec.EndsAt.ToLocalTime().TimeOfDay, job.Name, sched.Name, inComplete, included.Length, rec.JobUniqueID, rec.ScheduleUniqueID );

                    // Report
                    foreach (VCRRecordingInfo include in included)
                        Console.WriteLine( "\t\t{0} {1} {2} {3} {4}", include.Source.GetUniqueName(), include.StartsAt.Value.ToLocalTime(), include.EndsAt.ToLocalTime().TimeOfDay, include.JobUniqueID, include.ScheduleUniqueID );

                    // Report
                    foreach (StreamInfo stream in info.Streams)
                        foreach (ScheduleInfo schedule in stream.Schedules)
                            Console.WriteLine( "\t\t{0} {1} {2} {3}", schedule.JobUniqueID, schedule.ScheduleUniqueID, schedule.StartsAt.ToLocalTime(), schedule.EndsAt.ToLocalTime().TimeOfDay );

                    // Advance
                    start = rec.EndsAt;
                }

                // Helper data
                VCRRecordingInfo[] incDummy;
                bool dummyFlag;

                // Start timer
                Stopwatch timer = Stopwatch.StartNew();

                // Measure
                for (int n = 0; ++n > 0; )
                {
                    // Find the recording
                    jobs.FindNextRecording( DateTime.UtcNow, profile, out dummyFlag, out incDummy );

                    // Get the time
                    TimeSpan delta = timer.Elapsed;
                    if (delta.TotalSeconds >= 5)
                    {
                        // Report
                        Console.WriteLine( "{0}ms", delta.TotalMilliseconds / n );

                        // Done
                        break;
                    }
                }
            }

            // Get the profile
            Profile nexus = VCRProfiles.FindProfile( "Nexus" );

            // Find the next recording
            VCRRecordingInfo rec2 = jobs.FindNextJob( new DateTime( 2009, 2, 19, 20, 0, 0 ), null, nexus );
            if (null != rec2)
                if (rec2.StartsAt.HasValue)
                    using (RecordingRequest recording = jobs.CreateRecording( rec2, rec2.StartsAt.Value, nexus ))
                    {
                        // Get the information
                        FullInfo info0 = recording.CreateFullInformation();

#if EXEC
                        // Start it
                        recording.BeginExecute( false );

                        // Report
                        Console.WriteLine( "Recording..." );
                        Console.ReadLine();

                        // Get the information
                        FullInfo info1 = recording.CreateFullInformation();
#endif
                    }

            // Done
            Console.WriteLine( "Done" );
            Console.ReadLine();
        }
#endif
    }
}
