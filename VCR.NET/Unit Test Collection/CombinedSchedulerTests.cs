using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using JMS.DVB;
using JMS.DVB.Algorithms.Scheduler;
using JMS.DVBVCR.RecordingService;
using JMS.DVBVCR.RecordingService.Persistence;
using JMS.DVBVCR.RecordingService.Planning;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace JMS.DVBVCR.UnitTests
{
    /// <summary>
    /// Prüft die mit 4.1 eingeführte gemeinsame Planung für Aufzeichnungen und Sonderaufgaben.
    /// </summary>
    [TestClass]
    public class CombinedSchedulerTests : IRecordingPlannerSite, IDisposable
    {
        /// <summary>
        /// Der Bezugspunkt für alle Tests.
        /// </summary>
        private static readonly DateTime m_now = new DateTime( 2013, 3, 17, 19, 44, 12, DateTimeKind.Local ).ToUniversalTime();

        /// <summary>
        /// Der Name des für diesen Test zu verwendenden Testverzeichnisses.
        /// </summary>
        private readonly DirectoryInfo m_jobDirectory = new DirectoryInfo( Path.Combine( Path.GetTempPath(), Guid.NewGuid().ToString( "N" ) ) );

        /// <summary>
        /// Alle Aufträge zum Nachschlagen nach der Kennung der Aufträge.
        /// </summary>
        private readonly Dictionary<Guid, VCRJob> m_jobs;

        /// <summary>
        /// Alle Aufzeichnungen zum Nachschlagen nach der Kennung der Aufzeichnung.
        /// </summary>
        private readonly Dictionary<Guid, VCRSchedule> m_schedules;

        /// <summary>
        /// Die Namen aller Geräteprofile.
        /// </summary>
        private readonly string[] m_profiles;

        /// <summary>
        /// Der Zeitpunkt, an dem zum letzten Mal die Programmzeitschriften aktualisiert wurden.
        /// </summary>
        private Dictionary<string, DateTime> m_lastRunGuide;

        /// <summary>
        /// Der Zeitpunkt, an dem zum letzten Mal die Senderlisten aktualisiert wurden.
        /// </summary>
        private Dictionary<string, DateTime> m_lastRunScan;

        /// <summary>
        /// Der aktuelle Planungszeitpunkt.
        /// </summary>
        private DateTime m_planTime;

        /// <summary>
        /// Wird einmalig vor dem ersten Test aufgerufen.
        /// </summary>
        public CombinedSchedulerTests()
        {
            // Create directory
            m_jobDirectory.Create();

            // Path of the scratch file
            var zipPath = Path.Combine( m_jobDirectory.FullName, "temp.zip" );

            // Copy our archive to the directory
            File.WriteAllBytes( zipPath, Properties.Resources.TestJobs );

            // Extract [.NET 4.5]
            //ZipFile.ExtractToDirectory( zipPath, m_jobDirectory.FullName );

            // Forget it
            File.Delete( zipPath );

            // Process all files
            m_jobs =
                m_jobDirectory
                    .GetFiles()
                    .Select( p => SerializationTools.Load<VCRJob>( p ) )
                    .ToDictionary( j => j.UniqueID.Value );

            // Process all jobs
            m_schedules =
                m_jobs
                    .Values
                    .SelectMany( j => j.Schedules )
                    .ToDictionary( s => s.UniqueID.Value );

            // Select all profiles
            m_profiles =
                m_jobs
                   .Values
                   .Select( job => job.Source )
                   .Concat( m_schedules.Values.Select( schedule => schedule.Source ) )
                   .Where( source => source != null )
                   .Select( source => source.ProfileName )
                   .Where( profileName => !string.IsNullOrEmpty( profileName ) )
                   .Distinct( ProfileManager.ProfileNameComparer )
                   .Select( profileName => ProfileManager.FindProfile( profileName ) )
                   .Where( profile => profile != null )
                   .Select( profile => profile.Name )
                   .ToArray();
        }

        /// <summary>
        /// Wird einmalig nach dem letzten Test aufgerufen.
        /// </summary>
        public void Dispose()
        {
            // Get rid of directory
            if (m_jobDirectory.Exists)
                m_jobDirectory.Delete( true );
        }

        /// <summary>
        /// Prüft die aktuellen Testdaten.
        /// </summary>
        [TestMethod]
        public void ValidateTestData()
        {
            // Test count
            Assert.AreEqual( 16, m_jobs.Count, "Jobs" );
            Assert.AreEqual( 28, m_schedules.Count, "Schedules" );
        }

        /// <summary>
        /// Wird einmalig vor jedem Test aufgerufen.
        /// </summary>
        [TestInitialize]
        public void BeforeEachTest()
        {
            // Reset counters
            m_lastRunScan = m_profiles.ToDictionary( profileName => profileName, profileName => m_now.AddDays( -7 ) );
            m_lastRunGuide = m_profiles.ToDictionary( profileName => profileName, profileName => m_now );
            m_planTime = m_now;
        }

        /// <summary>
        /// Zeigt den Aufzeichnungsplan an.
        /// </summary>
        [TestMethod]
        public void ShowPlan()
        {
            // Create component under Test
            using (var cfg = TestConfigurationProvider.Create( Properties.Resources.AllSchedulers ))
            using (var cut = RecordingPlanner.Create( this ))
                foreach (var plan in cut.GetPlan( m_now ).Take( 250 ))
                    Debug.WriteLine( string.Format
                        (
                            "{0:dd.MM.yyyy HH:mm}-{5:HH:mm}{1} on {2} [{3}] {4}",
                            plan.Time.LocalStart,
                            plan.StartsLate ? " [late]" : string.Empty,
                            (plan.Resource == null) ? string.Format( "[{0}]", string.Join( ", ", plan.Definition.Resources.Select( r => r.Name ) ) ) : (object) plan.Resource,
                            plan.Definition.UniqueIdentifier,
                            plan.Definition.Name,
                            plan.Time.LocalEnd
                        ) );
        }

        /// <summary>
        /// Führt eine vollständige Prüfung aus.
        /// </summary>
        [TestMethod]
        public void FullValidation()
        {
            // Create component under Test
            using (var cfg = TestConfigurationProvider.Create( Properties.Resources.AllSchedulers ))
            using (var cut = RecordingPlanner.Create( this ))
                for (int i = 250; i-- > 0; )
                    cut.DispatchNextActivity( m_planTime );
        }

        /// <summary>
        /// Prüft eine Quelle und bringt sie auf den aktuellen Stand.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Die überprüfte Quelle.</returns>
        private SourceSelection FindSource( SourceSelection source )
        {
            // Forward
            return ProfileManager.FindProfile( source.ProfileName ).FindSource( source.Source ).Single();
        }

        #region Schnittstelle IRecordingPlannerSite

        /// <summary>
        /// Der volle Pfad zu dem Regeldatei der Aufzeichnungsplanung.
        /// </summary>
        public string ScheduleRulesPath { get { return Path.GetTempFileName(); } }

        /// <summary>
        /// Die Namen aller Geräteprofile, die verwendet werden sollen.
        /// </summary>
        public IEnumerable<string> ProfileNames { get { return m_profiles; } }

        /// <summary>
        /// Erstellt eine periodische Aufgabe zum Aktualisieren der Programmzeitschrift.
        /// </summary>
        /// <param name="resource">Die zugehörige Ressource.</param>
        /// <param name="profile">Die vollen Informationen zum Geräteprofil.</param>
        /// <returns>Die Beschreibung der Aufgabe oder <i>null</i>.</returns>
        public PeriodicScheduler CreateProgramGuideTask( IScheduleResource resource, Profile profile )
        {
            // Forward
            return new ProgramGuideTask( resource, () => m_lastRunGuide[resource.Name] );
        }

        /// <summary>
        /// Erstellt eine periodische Aufgabe zum Aktualisieren der Quellen.
        /// </summary>
        /// <param name="resource">Die zugehörige Ressource.</param>
        /// <param name="profile">Die vollen Informationen zum Geräteprofil.</param>
        /// <returns>Die Beschreibung der Aufgabe oder <i>null</i>.</returns>
        public PeriodicScheduler CreateSourceScanTask( IScheduleResource resource, Profile profile )
        {
            // Forward
            return new SourceListTask( resource, () => m_lastRunScan[resource.Name] );
        }

        /// <summary>
        /// Überträgt alle Aufträge in einen Ablaufplanung.
        /// </summary>
        /// <param name="scheduler">Die zu befüllende Ablaufplanung.</param>
        /// <param name="disabled">Alle deaktivierten Aufträge.</param>
        /// <param name="planner">Die zugehörige Aufzeichnungsplanung.</param>
        /// <param name="context">Detailinformationen zur Planung.</param>
        public void AddRegularJobs( RecordingScheduler scheduler, Func<Guid, bool> disabled, RecordingPlanner planner, PlanContext context )
        {
            // Retrieve all jobs related to this profile
            foreach (var job in m_jobs.Values)
                foreach (var schedule in job.Schedules)
                {
                    // No longer in use
                    if (!schedule.IsActive)
                        continue;

                    // Resolve source
                    var source = schedule.Source ?? job.Source;
                    if (source == null)
                        continue;

                    // Resolve profile
                    var resource = planner.GetResourceForProfile( source.ProfileName );
                    if (resource == null)
                        continue;

                    // Register
                    schedule.AddToScheduler( scheduler, job, new[] { resource }, FindSource, disabled, context );

                    // Connect
                    context.RegisterSchedule( schedule, job );
                }
        }

        /// <summary>
        /// Meldet eine Warteperiode.
        /// </summary>
        /// <param name="until">Der Zeitpunkt, an dem eine erneute Abfrage notwendig ist.</param>
        public void Idle( DateTime until )
        {
            // Validate
            Assert.IsTrue( until > m_planTime, "backward in time" );

            // Report
            Debug.WriteLine( string.Format( "{0} wait until {1}", m_planTime.ToLocalTime(), until.ToLocalTime() ) );

            // Just adjust our virtual time
            m_planTime = until;
        }

        /// <summary>
        /// Fordert zum Beenden einer Aufzeichnung oder Aufgabe aus.
        /// </summary>
        /// <param name="item">Alle notwendigen Informationen zur Aufzeichnung.</param>
        /// <param name="planner">Die zugehörige Aufzeichnungsplanung.</param>
        public void Stop( IScheduleInformation item, RecordingPlanner planner )
        {
            // Report
            Debug.WriteLine( string.Format( "{0} stop {1}", m_planTime.ToLocalTime(), item.Definition.Name ) );

            // Readout
            var definition = item.Definition;
            var resource = item.Resource;

            // Forward
            planner.Stop( definition.UniqueIdentifier );

            // Check mode
            if (definition is ProgramGuideTask)
                m_lastRunGuide[resource.Name] = m_planTime;
            else if (definition is SourceListTask)
                m_lastRunScan[resource.Name] = m_planTime;
            else
                VCRConfiguration.Current.HasRecordedSomething = true;
        }

        /// <summary>
        /// Meldet, dass eine Aufzeichnung nicht ausgeführt wird.
        /// </summary>
        /// <param name="item">Die verborgene Aufzeichnung.</param>
        public void Discard( IScheduleDefinition item )
        {
            // Report
            Debug.WriteLine( string.Format( "{0} discardced {1}", m_planTime.ToLocalTime(), item.Name ) );
        }

        /// <summary>
        /// Fordert zum Starten einer Aufzeichnung oder Aufgabe auf.
        /// </summary>
        /// <param name="item">Die Beschreibung der Aufgabe.</param>
        /// <param name="planner">Die zugehörige Aufzeichnungsplanung.</param>
        /// <param name="context">Zusatzinformationen zur Aufzeichnungsplanung.</param>
        public void Start( IScheduleInformation item, RecordingPlanner planner, PlanContext context )
        {
            // Report
            Debug.WriteLine( string.Format( "{0} start{3} on {1}: {2}", m_planTime.ToLocalTime(), item.Resource, item.Definition.Name, item.StartsLate ? " late" : string.Empty ) );

            // Forward
            planner.Start( item );
        }

        #endregion
    }
}
