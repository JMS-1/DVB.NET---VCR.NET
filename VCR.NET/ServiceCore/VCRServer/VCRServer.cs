using System;
using System.IO;
using System.Linq;
using System.Text;
using JMS.DVB;
using JMS.DVB.Algorithms.Scheduler;
using JMS.DVBVCR.RecordingService.Status;
using JMS.DVBVCR.RecordingService.Win32Tools;
using Microsoft.Win32;


namespace JMS.DVBVCR.RecordingService
{
    /// <summary>
    /// Von dieser Klasse existiert im Allgemeinen nur eine einzige Instanz. Sie
    /// realisiert die fachliche Logik des VCR.NET Recording Service.
    /// </summary>
    public partial class VCRServer : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Wird beim Bauen automatisch eingemischt.
        /// </summary>
        private const string CURRENTDATE = "2016/12/11";

        /// <summary>
        /// Aktuelle Version des VCR.NET Recording Service.
        /// </summary>
        public const string CurrentVersion = "4.5 [" + CURRENTDATE + "]";

        /// <summary>
        /// Konfigurationseintrag in der Registrierung von Windows.
        /// </summary>
        public static readonly RegistryKey ServiceRegistry = Registry.LocalMachine.CreateSubKey( @"SYSTEM\CurrentControlSet\Services\VCR.NET Service\Parameters" );

        /// <summary>
        /// Die zugeh?rige Verwaltung der aktiven Ger?teprofile.
        /// </summary>
        internal ProfileStateCollection Profiles { get; private set; }

        /// <summary>
        /// Alle Prozesse, die gestartet wurden.
        /// </summary>
        public readonly ExtensionManager ExtensionProcessManager = new ExtensionManager();

        /// <summary>
        /// L?dt Verwaltungsinstanzen f?r alle freigeschalteten DVB.NET Ger?teprofile.
        /// </summary>
        static VCRServer()
        {
            // Report
            Tools.ExtendedLogging( "VCRServer static Initialisation started" );

            // Set the default directory
            Environment.CurrentDirectory = Tools.ApplicationDirectory.FullName;

            // Report
            Tools.ExtendedLogging( "VCRServer static Initialisation completed" );
        }

        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        /// <param name="rootDirectory">Das Arbeitsverzeichnis f?r die Auftragsverwaltung.</param>
        public VCRServer( DirectoryInfo rootDirectory )
        {
            // Report
            Tools.ExtendedLogging( "Using Root Directory {0}", rootDirectory.FullName );

            // Prepare profiles
            VCRProfiles.Reset();

            // Create job manager and start it up
            JobManager = new JobManager( new DirectoryInfo( Path.Combine( rootDirectory.FullName, "Jobs" ) ), this );

            // Create profile state manager and start it up
            Profiles = new ProfileStateCollection( this );

            // Register with power manager
            PowerManager.OnPowerUp += BeginNewPlan;
        }

        /// <summary>
        /// Meldet die aktuell zu verwendende Konfiguration.
        /// </summary>
        public VCRConfiguration Configuration { get { return VCRConfiguration.Current; } }

        /// <summary>
        /// Instanzen dieser Klasse sind nicht zeitgebunden.
        /// </summary>
        /// <returns>Die Antwort muss immer <i>null</i> sein.</returns>
        public override object InitializeLifetimeService()
        {
            // No lease at all
            return null;
        }

        /// <summary>
        /// Meldet die aktuellen Einstellungen des VCR.NET Recording Service.
        /// </summary>
        public Settings Settings
        {
            get
            {
                // Create a new instance
                var result =
                    new Settings
                        {
                            MinimumHibernationDelay = VCRConfiguration.Current.RawDelayAfterForcedHibernation,
                            UseStandByForHibernation = VCRConfiguration.Current.UseS3ForHibernate,
                            MayHibernateSystem = VCRConfiguration.Current.MayHibernateSystem,
                            HasPendingHibernation = m_PendingHibernation,
                        };

                // Load profile names
                result.Profiles.AddRange( VCRProfiles.ProfileNames );

                // Report
                return result;
            }
        }

        /// <summary>
        /// Ermittelt den Startmodus in der aktuellen <see cref="AppDomain"/>.
        /// </summary>
        public bool InDebugMode { get { return Tools.DebugMode; } }

        /// <summary>
        /// Ermittelt ein Ger?teprofil und meldet einen Fehler, wenn keins gefunden wurde.
        /// </summary>
        /// <param name="profileName">Der Name des gew?nschten Ger?teprofils.</param>
        /// <returns>Der Zustand des Profils.</returns>
        public ProfileState FindProfile( string profileName )
        {
            // Forward
            var state = Profiles[profileName];
            if (state == null)
                LogError( Properties.Resources.NoProfile, profileName );

            // Report
            return state;
        }

        /// <summary>
        /// Ermittelt alle Quellen eines Ger?teprofils f?r die Nutzung durch den <i>LIVE</i> Zugang.
        /// </summary>
        /// <typeparam name="TTarget">Die Art der Zielklasse.</typeparam>
        /// <param name="profileName">Der Name des Ger?teprofils.</param>
        /// <param name="withTV">Gesetzt, wenn Fernsehsender zu ber?cksichtigen sind.</param>
        /// <param name="withRadio">Gesetzt, wenn Radiosender zu ber?cksichtigen sind.</param>
        /// <param name="factory">Eine Methode zum Erzeugen der Zielelemente aus den Daten einer einzelnen Quelle.</param>
        /// <returns></returns>
        public TTarget[] GetSources<TTarget>( string profileName, bool withTV, bool withRadio, Func<SourceSelection, TTarget> factory )
        {
            // Find the profile
            var profile = FindProfile( profileName );
            if (profile == null)
                return new TTarget[0];

            // Create matcher
            Func<Station, bool> matchStation;
            if (withTV)
                if (withRadio)
                    matchStation = station => true;
                else
                    matchStation = station => station.SourceType == SourceTypes.TV;
            else
                if (withRadio)
                    matchStation = station => station.SourceType == SourceTypes.Radio;
                else
                    return new TTarget[0];

            // Filter all we want
            return
                VCRProfiles
                    .GetSources( profile.ProfileName, matchStation )
                    .Select( factory )
                    .ToArray();
        }

        /// <summary>
        /// Ermittelt eine Quelle.
        /// </summary>
        /// <param name="profile">Das zu verwendende Ger?teprofil.</param>
        /// <param name="name">Der (hoffentlicH) eindeutige Name der Quelle.</param>
        /// <returns>Die Beschreibung der Quelle.</returns>
        public SourceSelection FindSource( string profile, string name )
        {
            // Process
            return VCRProfiles.FindSource( profile, name );
        }

        /// <summary>
        /// Ermittelt den eindeutigen Namen einer Quelle.
        /// </summary>
        /// <param name="source">Die gew?nschte Quelle.</param>
        /// <returns>Der eindeutige Name oder <i>null</i>, wenn die Quelle nicht
        /// bekannt ist.</returns>
        public string GetUniqueName( SourceSelection source )
        {
            // Forward
            return source.GetUniqueName();
        }

        /// <summary>
        /// Meldet die aktuellen Regeln f?r die Aufzeichnungsplanung.
        /// </summary>
        public string SchedulerRules
        {
            get
            {
                // Attach to the path
                var rulePath = Profiles.ScheduleRulesPath;
                if (File.Exists( rulePath ))
                    using (var reader = new StreamReader( rulePath, true ))
                        return reader.ReadToEnd().Replace( "\r\n", "\n" );

                // Not set
                return null;
            }
            set
            {
                // Check mode
                var rulePath = Profiles.ScheduleRulesPath;
                if (string.IsNullOrWhiteSpace( value ))
                {
                    // Back to default
                    if (File.Exists( rulePath ))
                        File.Delete( rulePath );
                }
                else
                {
                    // Update line feeds
                    var content = value.Replace( "\r\n", "\n" ).Replace( "\n", "\r\n" );
                    var scratchFile = Path.Combine( Path.GetTempPath(), Guid.NewGuid().ToString( "N" ) );

                    // Write to scratch file
                    File.WriteAllText( scratchFile, content, Encoding.UTF8 );

                    // With cleanup
                    try
                    {
                        // See if resource manager could be created
                        ResourceManager.Create( scratchFile, ProfileManager.ProfileNameComparer ).Dispose();

                        // Try to overwrite
                        File.Copy( scratchFile, rulePath, true );
                    }
                    finally
                    {
                        // Get rid of scratch file
                        File.Delete( scratchFile );
                    }
                }
            }
        }

        /// <summary>
        /// F?hrt periodische Aufr?umarbeiten aus.
        /// </summary>
        public void PeriodicCleanup()
        {
            // Forward
            JobManager.CleanupArchivedJobs();
            JobManager.CleanupLogEntries();
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endg?ltig.
        /// </summary>
        public void Dispose()
        {
            // Register with power manager
            PowerManager.OnPowerUp -= BeginNewPlan;

            // Shutdown profiles
            using (Profiles)
                Profiles = null;

            // Detach from jobs
            JobManager = null;
        }

        #endregion
    }
}
