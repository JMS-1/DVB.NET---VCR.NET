using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using JMS.DVB.Algorithms.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Führt einige Prüfung in einer Produktionsumgebung aus.
    /// </summary>
    //[TestClass]
    public class ProductionTests
    {
        /// <summary>
        /// Prüft die Laufzeit bei der Planung in einer realen Umgebung.
        /// </summary>
        [TestMethod]
        public void Performance_Of_A_Real_Life_Scenario()
        {
            // Get the path to the test directories
            var jobDirectory = @"C:\Backup\Software\Current\GIT\DVB.NET---VCR.NET\VCR.NET\WebClient41\Jobs\Active";
            var profileDirectory = Environment.ExpandEnvironmentVariables( @"%AllUsersProfile%\DVBNETProfiles" );

            // Helper - will be reused
            var job = new XmlDocument();

            // Sources
            var sourceMap = new Dictionary<string, SourceMock>( StringComparer.InvariantCultureIgnoreCase );
            var profileMap = new Dictionary<string, XmlDocument>( StringComparer.InvariantCultureIgnoreCase );
            var groupMap = new Dictionary<object, Guid>();
            var plan = new List<IRecordingDefinition>();

            // Process all job files
            foreach (var jobPath in Directory.EnumerateFiles( jobDirectory, "*.j39" ))
            {
                // Load to document
                job.Load( jobPath );

                // Common data
                var autoSelect = bool.Parse( job.SelectSingleNode( "VCRJob/AutomaticResourceSelection" ).InnerText );
                var defaultSource = job.SelectSingleNode( "VCRJob/Source" );
                var profileName = defaultSource.InnerText.Substring( defaultSource.InnerText.LastIndexOf( '@' ) + 1 );
                var jobName = job.SelectSingleNode( "VCRJob/Name" ).InnerText;

                // Load the profile once
                XmlDocument profile;
                if (!profileMap.TryGetValue( profileName, out profile ))
                {
                    // Create
                    profile = new XmlDocument();

                    // Load
                    profile.Load( Path.Combine( profileDirectory, profileName + ".dnp" ) );

                    // Remapping
                    for (; ; )
                    {
                        // Create a new namespace table
                        var namespaces = new XmlNamespaceManager( profile.NameTable );

                        // Add us
                        namespaces.AddNamespace( "dvbnet", "http://psimarron.net/DVBNET/Profiles" );

                        // Check for remap
                        var loadFrom = profile.SelectSingleNode( "//dvbnet:UseSourcesFrom", namespaces );
                        if (loadFrom == null)
                            break;

                        // Load the name
                        var refProfileName = loadFrom.InnerText;
                        if (string.IsNullOrEmpty( refProfileName ))
                            break;

                        // Reload
                        if (profileMap.TryGetValue( refProfileName, out profile ))
                            continue;

                        // Create
                        profile = new XmlDocument();

                        // Load
                        profile.Load( Path.Combine( profileDirectory, refProfileName + ".dnp" ) );

                        // Remember
                        profileMap.Add( refProfileName, profile );
                    }

                    // Remember
                    profileMap.Add( profileName, profile );
                }

                // Create a new namespace table
                var profileNamespaces = new XmlNamespaceManager( profile.NameTable );

                // Add us
                profileNamespaces.AddNamespace( "dvbnet", "http://psimarron.net/DVBNET/Profiles" );

                // Validate
                Assert.IsTrue( autoSelect, "Auto Select" );

                // Process all schedules
                foreach (XmlElement schedule in job.SelectNodes( "VCRJob/Schedule" ))
                {
                    // Extract data
                    var repeat = schedule.SelectSingleNode( "Days" ).InnerText.Split( ' ' ).Where( d => !string.IsNullOrEmpty( d ) ).Select( d => (DayOfWeek) Enum.Parse( typeof( DayOfWeek ), d ) ).ToArray();
                    var firstStart = DateTime.Parse( schedule.SelectSingleNode( "FirstStart" ).InnerText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind );
                    var lastDay = DateTime.Parse( schedule.SelectSingleNode( "LastDay" ).InnerText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind );
                    var duration = TimeSpan.FromMinutes( uint.Parse( schedule.SelectSingleNode( "Duration" ).InnerText ) );
                    var source = (XmlElement) (schedule.SelectSingleNode( "Source" ) ?? defaultSource);
                    var id = Guid.Parse( schedule.SelectSingleNode( "UniqueID" ).InnerText );
                    var scheduleName = schedule.SelectSingleNode( "Name" ).InnerText;
                    var sourceName = source.GetAttribute( "name" );

                    // Find the real source
                    SourceMock realSource;
                    if (!sourceMap.TryGetValue( sourceName, out realSource ))
                    {
                        // Split name
                        var split = sourceName.LastIndexOf( '[' );
                        var stationName = sourceName.Substring( 0, split ).TrimEnd();
                        var providerName = sourceName.Substring( split + 1 ).TrimEnd( ']' );

                        // Locate the station
                        var station = (XmlElement) profile.SelectSingleNode( "//dvbnet:Station[@name='" + stationName + "' and @provider='" + providerName + "']", profileNamespaces );
                        var encrypted = bool.Parse( station.GetAttribute( "scrambled" ) );
                        var group = station.ParentNode;

                        // Unique group identifier
                        Guid groupIdentifier;
                        if (!groupMap.TryGetValue( group, out groupIdentifier ))
                            groupMap.Add( group, groupIdentifier = Guid.NewGuid() );

                        // Create the source entry
                        sourceMap.Add( sourceName, realSource = SourceMock.Create( sourceName, groupIdentifier, encrypted ) );
                    }

                    // Get the full name
                    if (string.IsNullOrEmpty( scheduleName ))
                        scheduleName = jobName;
                    else if (!string.IsNullOrEmpty( jobName ))
                        scheduleName = jobName + " (" + scheduleName + ")";

                    // Create the request
                    if (repeat.Length > 0)
                        plan.Add( RecordingDefinition.Create( schedule, scheduleName, id, null, realSource, firstStart, duration, lastDay, repeat ) );
                    else
                        plan.Add( RecordingDefinition.Create( schedule, scheduleName, id, null, realSource, firstStart, duration ) );
                }
            }

            // Create component under test
            var componentUnderTest = new RecordingScheduler( StringComparer.InvariantCultureIgnoreCase );
            var sources = sourceMap.Values.ToArray();

            // Add resources - hard coded, should be taken from configuration in some future version
            for (var i = 0; i++ < 4; )
                componentUnderTest.Add( ResourceMock.Create( "duo" + i.ToString( "0" ), sources ).SetEncryptionLimit( 1 ) );

            // Add the plan
            foreach (var recording in plan)
                componentUnderTest.Add( recording );

            // Process
            var refTime = new DateTime( 2013, 10, 26, 12, 0, 0, DateTimeKind.Utc );
            var timer = Stopwatch.StartNew();
            var all = componentUnderTest.GetSchedules( refTime ).Take( 1000 ).ToArray();
            var elapsed = timer.Elapsed;

            // Report
            Console.WriteLine( "{0:N3}ms", elapsed.TotalMilliseconds );

            // Validate
            Assert.AreEqual( 1000, all.Length, "#jobs" );
        }
    }
}
