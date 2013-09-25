using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;


namespace TVBrowserPlugIn
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static int Main( string[] args )
        {
            // Check settings
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (!version.Equals( Properties.Settings.Default.Version ))
            {
                // Upgrade
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.Version = version;
                Properties.Settings.Default.Save();
            }

            // For screen shots
            //Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("de");

            // Prepare
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            // Check for configuration call
            if (args.Length < 1)
            {
                // Process
                Application.Run( new Configuration() );

                // Done
                return 0;
            }

            // In error
            try
            {
                // Get command
                string cmd = args[0];

                // Check mode
                bool delete = Equals( cmd, "/del" );
                bool clickfinder = false;

                // Check parameter
                if (!delete && !Equals( cmd, "/add" ))
                {
                    // Check for click finder call
                    clickfinder = cmd.StartsWith( "Pos=" );

                    // In error
                    if (!clickfinder) throw new ArgumentException( "/del /add" );
                }

                // Relevant information
                string channelName, dura, title;
                DateTime start;

                // Check mode
                if (clickfinder)
                {
                    // Parts
                    string starttime;

                    // Get parts
                    channelName = args[2].Substring( 7 ).Replace( "\"", string.Empty );
                    starttime = args[3].Substring( 7 ).Replace( "\"", string.Empty );
                    dura = args[4].Substring( 6 );
                    title = args[5].Substring( 8 ).Replace( "\"", string.Empty );

                    // Create date
                    starttime = starttime.Insert( 10, ":" );
                    starttime = starttime.Insert( 8, " " );
                    starttime = starttime.Insert( 6, "-" );
                    starttime = starttime.Insert( 4, "-" );

                    // Parse
                    start = DateTime.Parse( starttime );
                }
                else
                {
                    // Get parts
                    channelName = args[1];
                    title = args[5];
                    dura = args[4];

                    // Parse
                    start = DateTime.Parse( args[2] ) + TimeSpan.Parse( args[3] );
                }

                // Parse
                var minutes = int.Parse( dura );

                // Load the name of the profile - can only use the default
                var profileName = VCRNETRestProxy.GetProfiles( EndPoint ).Select( profile => profile.name ).FirstOrDefault();

                // Load collections
                var externals = Properties.Settings.Default.ExternalNames ?? new StringCollection();
                var internals = Properties.Settings.Default.VCRNETNames ?? new StringCollection();

                // See if channel is mapped
                for (var i = Math.Min( externals.Count, internals.Count ); i-- > 0; )
                    if (StringComparer.CurrentCultureIgnoreCase.Equals( externals[i], channelName ))
                        return UpdateRecording( profileName, delete, internals[i], title, start, minutes );

                // Ask user
                using (var dialog = new ChooseMapping( channelName, VCRNETRestProxy.GetSources( EndPoint, profileName ).Select( source => source.nameWithProvider ) ))
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // Cut off
                        while (externals.Count > internals.Count)
                            externals.RemoveAt( internals.Count );
                        while (internals.Count > externals.Count)
                            internals.RemoveAt( externals.Count );

                        // Expand
                        externals.Add( channelName );
                        internals.Add( dialog.MappedName );

                        // Back to settings
                        Properties.Settings.Default.ExternalNames = externals;
                        Properties.Settings.Default.VCRNETNames = internals;
                        Properties.Settings.Default.Save();

                        // Process
                        return UpdateRecording( profileName, delete, dialog.MappedName, title, start, minutes );
                    }

                // Report
                MessageBox.Show( string.Format( Properties.Resources.NoSuchChannel, channelName ) );

                // Done
                return 1;
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( ex.Message );

                // Done
                return 2;
            }
        }

        /// <summary>
        /// Manipuliert eine Aufzeichnung.
        /// </summary>
        /// <param name="profile">Das zu verwendende Geräteprofil.</param>
        /// <param name="delete">Gesetzt, wenn eine Aufzeichnung entfernt werden soll.</param>
        /// <param name="stationName">Die gewünschte Quelle.</param>
        /// <param name="title">Der Name der Aufzeichnung.</param>
        /// <param name="start">Der Zeitpunkt der ersten Ausführung.</param>
        /// <param name="minutes">Die Laufzeit in Minuten.</param>
        /// <returns>Der Fehlercode zur Ausführung.</returns>
        private static int UpdateRecording( string profile, bool delete, string stationName, string title, DateTime start, int minutes )
        {
            // Real data
            DateTime utcStart = start.ToUniversalTime();

            // Correct
            utcStart = utcStart.AddMinutes( -Properties.Settings.Default.PreTime );
            minutes += Properties.Settings.Default.PreTime + Properties.Settings.Default.PostTime;

            // Not yet deleting
            if (delete)
            {
                // Process all jobs
                foreach (var job in VCRNETRestProxy.GetJobs( EndPoint ))
                {
                    // Test job data
                    if (!StringComparer.InvariantCultureIgnoreCase.Equals( profile, job.device ))
                        continue;
                    if (!Equals( job.source, stationName ))
                        continue;

                    // Test schedules
                    if (job.schedules.Length != 1)
                        continue;

                    // Attach to the one and only schedule
                    var schedule = job.schedules[0];
                    if (schedule.start != utcStart)
                        continue;
                    if (schedule.duration != minutes)
                        continue;
                    if (schedule.repeatPattern != 0)
                        continue;

                    // Try to delete
                    VCRNETRestProxy.Delete( EndPoint, schedule.id );

                    // Done
                    return 0;
                }

                // Report
                MessageBox.Show( Properties.Resources.NoSuchJob );

                // Done
                return 3;
            }

            // Create the recording
            var newJob =
                new VCRNETRestProxy.Job
                {
                    sourceName = stationName,
                    withVideotext = true,
                    withSubtitles = true,
                    allLanguages = true,
                    includeDolby = true,
                    device = profile,
                    name = title,
                };

            // Correct the name
            foreach (char bad in System.IO.Path.GetInvalidFileNameChars())
                newJob.name = newJob.name.Replace( bad, '_' );

            // Special
            newJob.name = newJob.name.Replace( '&', '_' );

            // Create the schedule
            var newSchedule =
                new VCRNETRestProxy.Schedule
                {
                    lastDay = new DateTime( 2999, 12, 31 ),
                    firstStart = utcStart,
                    name = string.Empty,
                    duration = minutes,
                };

            // Process
            var uniqueId = VCRNETRestProxy.CreateNew( EndPoint, newJob, newSchedule );

            // See if we are allowed to open the browser
            if (Properties.Settings.Default.ShowConfirmation)
            {
                // Create the URL
                string url = string.Format( "{0}/default.html#edit;id={1}", EndPoint, uniqueId );

                // Report error
                try
                {
                    // Show in default browser
                    using (var explorer = Process.Start( url ))
                        if (explorer != null)
                            explorer.Close();
                }
                catch (Exception e)
                {
                    // Report
                    MessageBox.Show( e.Message );
                }
            }

            // Succeeded
            return 0;
        }

        /// <summary>
        /// Ermittelt die Verbindung zum <i>VCR.NET Recording Service</i>.
        /// </summary>
        private static string EndPoint
        {
            get
            {
                // Full path in legacy mode
                var uri = Properties.Settings.Default.TVBrowserPlugIn_VCRNETService_VCRServer30;

                // Report
                return uri.Substring( 0, uri.LastIndexOf( '/' ) );
            }
        }
    }
}