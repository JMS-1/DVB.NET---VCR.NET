using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http;
using JMS.DVB;
using JMS.DVBVCR.RecordingService.WebServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Erlaubt den administrativen Zugriff auf den <i>VCR.NET Recording Service</i>.
    /// </summary>
    public class ConfigurationController : ApiController
    {
        /// <summary>
        /// Die Einstellungen der Sicherheit.
        /// </summary>
        [DataContract]
        public class SecuritySettings
        {
            /// <summary>
            /// Die Gruppe der normalen Benutzer.
            /// </summary>
            [DataMember( Name = "users" )]
            public string UserRole { get; set; }

            /// <summary>
            /// Die Gruppe der Administratoren.
            /// </summary>
            [DataMember( Name = "admins" )]
            public string AdminRole { get; set; }
        }

        /// <summary>
        /// Die Einstellung der Aufzeichnungsverzeichnisse.
        /// </summary>
        [DataContract]
        public class DirectorySettings
        {
            /// <summary>
            /// Die aktuelle Liste der erlaubten Verzeichnisse.
            /// </summary>
            [DataMember( Name = "directories" )]
            public string[] TargetDirectories { get; set; }

            /// <summary>
            /// Das Muster für die Erstellung der Dateinamen.
            /// </summary>
            [DataMember( Name = "pattern" )]
            public string RecordingPattern { get; set; }
        }

        /// <summary>
        /// Die Einstellungen zur Programmzeitschrift.
        /// </summary>
        [DataContract]
        public class GuideSettings
        {
            /// <summary>
            /// Der Schwellwert für vorgezogene Aktualisierungen (in Stunden).
            /// </summary>
            [DataMember( Name = "joinHours" )]
            public int? Threshold { get; set; }

            /// <summary>
            /// Der minimale Abstand zwischen Aktualisierungen (in Stunden).
            /// </summary>
            [DataMember( Name = "minDelay" )]
            public int? Interval { get; set; }

            /// <summary>
            /// Die maximale Dauer einer Aktualisierung (in Minuten).
            /// </summary>
            [DataMember( Name = "duration" )]
            public uint Duration { get; set; }

            /// <summary>
            /// Die Stunden, zu denen eine Aktualisierung stattfinden soll.
            /// </summary>
            [DataMember( Name = "hours" )]
            public uint[] Hours { get; set; }

            /// <summary>
            /// Die Quellen, die bei der Aktualisierung zu berücksichtigen sind.
            /// </summary>
            [DataMember( Name = "sources" )]
            public string[] Sources { get; set; }

            /// <summary>
            /// Gesetzt, wenn auch die britischen Sendungen zu berücksichtigen sind.
            /// </summary>
            [DataMember( Name = "includeUK" )]
            public bool WithUKGuide { get; set; }
        }

        /// <summary>
        /// Die Einstellungen zur Aktualisierung der Quellen.
        /// </summary>
        [DataContract]
        public class SourceScanSettings
        {
            /// <summary>
            /// Der Schwellwert für vorgezogene Aktualisierungen (in Tagen).
            /// </summary>
            [DataMember( Name = "joinDays" )]
            public int? Threshold { get; set; }

            /// <summary>
            /// Der minimale Abstand zwischen Aktualisierungen (in Tagen).
            /// </summary>
            [DataMember( Name = "interval" )]
            public int? Interval { get; set; }

            /// <summary>
            /// Die maximale Dauer einer Aktualisierung (in Minuten).
            /// </summary>
            [DataMember( Name = "duration" )]
            public uint Duration { get; set; }

            /// <summary>
            /// Die Stunden, zu denen eine Aktualisierung stattfinden soll.
            /// </summary>
            [DataMember( Name = "hours" )]
            public uint[] Hours { get; set; }

            /// <summary>
            /// Gesetzt, wenn die neue Liste mit der alten zusammengeführt werden soll.
            /// </summary>
            [DataMember( Name = "merge" )]
            public bool MergeLists { get; set; }
        }

        /// <summary>
        /// Die Konfiguration der Geräteprofile.
        /// </summary>
        [DataContract]
        public class ProfileSettings
        {
            /// <summary>
            /// Die Liste aller bekannten Geräteprofile.
            /// </summary>
            [DataMember( Name = "profiles" )]
            public ConfigurationProfile[] SystemProfiles { get; set; }

            /// <summary>
            /// Der Name des bevorzugten Geräteprofils.
            /// </summary>
            [DataMember( Name = "defaultProfile" )]
            public string DefaultProfile { get; set; }
        }

        /// <summary>
        /// Alle sonstigen Einstellungen.
        /// </summary>
        [DataContract]
        public class OtherSettings
        {
            /// <summary>
            /// Gesetzt, wenn der <i>VCR.NET Recording Service</i> den Rechner in den Schlafzustand versetzen darf-
            /// </summary>
            [DataMember( Name = "mayHibernate" )]
            public bool AllowHibernate { get; set; }

            /// <summary>
            /// Gesetzt, wenn beim Übergang in den Schlafzustand <i>StandBy</i> verwendet werden soll.
            /// </summary>
            [DataMember( Name = "useStandBy" )]
            public bool UseStandBy { get; set; }

            /// <summary>
            /// Verweildauer (in Wochen) von Aufträgen im Archiv.
            /// </summary>
            [DataMember( Name = "archive" )]
            public uint ArchiveTime { get; set; }

            /// <summary>
            /// Verweildauer (in Wochen) von Protokolleinträgen.
            /// </summary>
            [DataMember( Name = "protocol" )]
            public uint ProtocolTime { get; set; }

            /// <summary>
            /// Vorlaufzeit (in Sekunden) beim Aufwecken aus dem Schlafzustand.
            /// </summary>
            [DataMember( Name = "hibernationDelay" )]
            public uint HibernationDelay { get; set; }

            /// <summary>
            /// Gesetzt, wenn für H.264 Aufzeichnungen kein PCR generiert werden soll.
            /// </summary>
            [DataMember( Name = "noH264PCR" )]
            public bool DisablePCRFromH264 { get; set; }

            /// <summary>
            /// Gesetzt, wenn für MPEG-2 Aufzeichnungen kein PCR generiert werden soll.
            /// </summary>
            [DataMember( Name = "noMPEG2PCR" )]
            public bool DisablePCRFromMPEG2 { get; set; }

            /// <summary>
            /// Minimale Verweildauer (in Minuten) im Schlafzustand bei einem erzwungenen Übergang.
            /// </summary>
            [DataMember( Name = "forcedHibernationDelay" )]
            public uint DelayAfterForcedHibernation { get; set; }

            /// <summary>
            /// Gesetzt um die minimale Verweildauer im Schlafzustand zu ignorieren.
            /// </summary>
            [DataMember( Name = "suppressHibernationDelay" )]
            public bool SuppressDelayAfterForcedHibernation { get; set; }

            /// <summary>
            /// Gesetzt, wenn auch das <i>Basic</i> Protokoll zur Autorisierung verwendet werden darf.
            /// </summary>
            [DataMember( Name = "basicAuth" )]
            public bool AllowBasic { get; set; }

            /// <summary>
            /// Gesetzt, wenn auch eine verschlüsselter SSL Verbindung unterstützt werden soll.
            /// </summary>
            [DataMember( Name = "ssl" )]
            public bool UseSSL { get; set; }

            /// <summary>
            /// Der TCP/IP Port für verschlüsselte Verbindungen.
            /// </summary>
            [DataMember( Name = "sslPort" )]
            public ushort SSLPort { get; set; }

            /// <summary>
            /// Der TCP/IP Port des Web Servers.
            /// </summary>
            [DataMember( Name = "webPort" )]
            public ushort WebPort { get; set; }

            /// <summary>
            /// Die Art der Protokollierung.
            /// </summary>
            [DataMember( Name = "logging" ), JsonConverter( typeof( StringEnumConverter ) )]
            public LoggingLevel Logging { get; set; }
        }

        /// <summary>
        /// Informationen zum aktuell verwendeten Regelwerk der Aufzeichnungsplanung.
        /// </summary>
        [DataContract]
        public class SchedulerRules
        {
            /// <summary>
            /// Der Inhalt der Regeldatei.
            /// </summary>
            [DataMember( Name = "rules" )]
            public string RuleFileContents { get; set; }
        }

        /// <summary>
        /// Ermittelt eine Verzeichnisstruktur.
        /// </summary>
        /// <param name="browse">Wird zur Unterscheidung der Methoden verwendet.</param>
        /// <param name="toParent">Gesetzt, wenn zum übergeordneten Verzeichnis gewechselt werden soll.</param>
        /// <param name="root">Das Bezugsverzeichnis.</param>
        /// <returns>Die Verzeichnisse innerhalb ober oberhalb des Bezugsverzeichnisses.</returns>
        [HttpGet]
        public string[] Browse( string browse, bool toParent = false, string root = null )
        {
            // Validate
            ServerRuntime.TestAdminAccess();

            // See if we can move up
            if (!string.IsNullOrEmpty( root ))
                if (toParent)
                    if (StringComparer.InvariantCultureIgnoreCase.Equals( root, Path.GetPathRoot( root ) ))
                        root = null;
                    else
                        root = Path.GetDirectoryName( root );

            // Devices
            var names = string.IsNullOrEmpty( root )
                ? DriveInfo
                        .GetDrives()
                        .Where( drive => drive.DriveType == DriveType.Fixed )
                        .Where( drive => drive.IsReady )
                        .Select( drive => drive.RootDirectory.FullName )
                : Directory
                    .GetDirectories( root );

            // Report
            return new[] { root }.Concat( names.OrderBy( name => name, StringComparer.InvariantCultureIgnoreCase ) ).ToArray();
        }

        /// <summary>
        /// Prüft, ob ein Verzeichnis verfügbar ist.
        /// </summary>
        /// <param name="validate">Wird zur Unterscheidung der Methoden verwendet.</param>
        /// <param name="directory">Das zu prüfende Verzeichnis.</param>
        /// <returns>Gesetzt, wenn das Verzeichnis verwendet werden kann.</returns>
        [HttpGet]
        public bool Validate( string validate, string directory )
        {
            // Validate
            ServerRuntime.TestAdminAccess();

            // Be safe
            try
            {
                // Test
                return Directory.Exists( directory );
            }
            catch (Exception)
            {
                // Nope
                return false;
            }
        }

        /// <summary>
        /// Ermittelt die aktuellen Regeln für die Aufzeichnunsplanung.
        /// </summary>
        /// <param name="rules">Wird zur Unterscheidung der Methoden verwendet.</param>
        /// <returns>Die aktuellen Regeln.</returns>
        [HttpGet]
        public SchedulerRules ReadSchedulerRules( string rules )
        {
            // Blind forward - we allow regular users to read the current rule set as well
            return new SchedulerRules { RuleFileContents = ServerRuntime.VCRServer.SchedulerRules };
        }

        /// <summary>
        /// Aktualisiert das Regelwerk für die Aufzeichnungsplanung.
        /// </summary>
        /// <param name="rules">Unterscheidet die einzelnen Aufrufe.</param>
        /// <param name="settings">Die ab sofort zu verwendenden Regeln.</param>
        /// <returns>Meldet, ob ein Neustart erforderlich ist.</returns>
        [HttpPut]
        public bool? WriteSchedulerRules( string rules, [FromBody] SchedulerRules settings )
        {
            // Process
            return ServerRuntime.UpdateSchedulerRules( settings.RuleFileContents );
        }

        /// <summary>
        /// Meldet die Konfigurationsdaten der Geräte.
        /// </summary>
        /// <param name="devices">Wird zur Unterscheidung der Methoden verwendet.</param>
        /// <returns>Die aktuelle Konfiguration.</returns>
        [HttpGet]
        public ProfileSettings ReadProfiles( string devices )
        {
            // Validate
            ServerRuntime.TestAdminAccess();

            // Helper
            string defaultName;

            // Create response
            var settings =
                new ProfileSettings
                {
                    SystemProfiles =
                        ServerRuntime
                            .VCRServer
                            .GetProfiles( ConfigurationProfile.Create, out defaultName )
                            .OrderBy( profile => profile.Name, ProfileManager.ProfileNameComparer )
                            .ToArray()
                };

            // Merge default
            settings.DefaultProfile = defaultName;

            // Report
            return settings;
        }

        /// <summary>
        /// Aktualisiert die Einstellungen zu den Geräteprofilen.
        /// </summary>
        /// <param name="devices">Dient zur Unterscheidung der Methoden.</param>
        /// <param name="settings">Die gewünschten neuen Einstellungen.</param>
        /// <returns>Das Ergebnis der Änderung.</returns>
        [HttpPut]
        public bool? WriteProfiles( string devices, [FromBody] ProfileSettings settings )
        {
            // Validate
            ServerRuntime.TestAdminAccess();

            // List of profiles to use
            var profiles = settings.SystemProfiles.Where( profile => profile.UsedForRecording ).Select( profile => profile.Name ).ToList();

            // Move default to the front
            var defaultIndex = profiles.IndexOf( settings.DefaultProfile );
            if (defaultIndex >= 0)
            {
                // Insert at the very beginning
                profiles.Insert( 0, profiles[defaultIndex] );
                profiles.RemoveAt( defaultIndex + 1 );
            }

            // Prepare
            var update = VCRConfiguration.Current.BeginUpdate( SettingNames.Profiles );

            // Fill
            update[SettingNames.Profiles].NewValue = string.Join( "|", profiles );

            // Process
            return ServerRuntime.Update( update.Values, ServerRuntime.VCRServer.UpdateProfiles( settings.SystemProfiles, profile => profile.Name, ( profile, device ) => profile.WriteBack( device ) ) );
        }

        /// <summary>
        /// Meldet die aktuellen Einstellungen zu den Verzeichnissen.
        /// </summary>
        /// <param name="directory">Wird zur Unterscheidung der Methoden verwendet.</param>
        /// <returns>Die gewünschten Einstellungen.</returns>
        [HttpGet]
        public DirectorySettings ReadDirectory( string directory )
        {
            // Validate
            ServerRuntime.TestAdminAccess();

            // Report
            return
                new DirectorySettings
                {
                    TargetDirectories = VCRConfiguration.Current.TargetDirectoriesNames.ToArray(),
                    RecordingPattern = VCRConfiguration.Current.FileNamePattern,
                };
        }

        /// <summary>
        /// Aktualisiert die Konfiguration der Aufzeichnungsdateien.
        /// </summary>
        /// <param name="directory">Wird zur Unterscheidung der Methoden verwendet.</param>
        /// <param name="settings">Die neuen Daten.</param>
        /// <returns>Das Ergebnis der Operation.</returns>
        [HttpPut]
        public bool? WriteDirectory( string directory, [FromBody] DirectorySettings settings )
        {
            // Prepare to update
            var update = VCRConfiguration.Current.BeginUpdate( SettingNames.VideoRecorderDirectory, SettingNames.AdditionalRecorderPaths, SettingNames.FileNamePattern );

            // Change settings
            update[SettingNames.AdditionalRecorderPaths].NewValue = string.Join( ", ", settings.TargetDirectories.Skip( 1 ) );
            update[SettingNames.VideoRecorderDirectory].NewValue = settings.TargetDirectories.FirstOrDefault();
            update[SettingNames.FileNamePattern].NewValue = settings.RecordingPattern;

            // Process
            return ServerRuntime.Update( update.Values );
        }

        /// <summary>
        /// Liest die Konfiguration für die Aktualisierung der Quellen.
        /// </summary>
        /// <param name="scan">Unterscheidet zwischen den einzelnen Methoden.</param>
        /// <returns>Die aktuellen Einstellungen.</returns>
        [HttpGet]
        public SourceScanSettings ReadSoureScan( string scan )
        {
            // Validate
            ServerRuntime.TestAdminAccess();

            // Load
            var interval = VCRConfiguration.Current.SourceListUpdateInterval;
            var join = VCRConfiguration.Current.SourceListJoinThreshold;

            // Report
            return
                new SourceScanSettings
                {
                    Hours = VCRConfiguration.Current.SourceListUpdateHoursAsArray.OrderBy( hour => hour ).ToArray(),
                    Threshold = join.HasValue ? (int) join.Value.TotalDays : default( int? ),
                    MergeLists = VCRConfiguration.Current.MergeSourceListUpdateResult,
                    Duration = VCRConfiguration.Current.SourceListUpdateDuration,
                    Interval = (interval != 0) ? interval : default( int? ),
                };
        }

        /// <summary>
        /// Aktualisiert die Einstellungen für die Aktualisierung der Quellen.
        /// </summary>
        /// <param name="scan">Unterscheidet zwischen den einzelnen Methoden.</param>
        /// <param name="settings">Die neuen Einstellungen.</param>
        /// <returns>Das Ergebnis der Änderung.</returns>
        [HttpPut]
        public bool? WriteSourceScan( string scan, [FromBody] SourceScanSettings settings )
        {
            // Check mode
            if (settings.Interval == 0)
            {
                // Create settings
                var disable = VCRConfiguration.Current.BeginUpdate( SettingNames.ScanInterval );

                // Store
                disable[SettingNames.ScanInterval].NewValue = "0";

                // Process
                return ServerRuntime.Update( disable.Values );
            }

            // Check mode
            if (settings.Interval < 0)
            {
                // Create settings
                var manual = VCRConfiguration.Current.BeginUpdate( SettingNames.ScanDuration, SettingNames.MergeScanResult, SettingNames.ScanInterval );

                // Store
                manual[SettingNames.MergeScanResult].NewValue = settings.MergeLists.ToString();
                manual[SettingNames.ScanDuration].NewValue = settings.Duration.ToString();
                manual[SettingNames.ScanInterval].NewValue = "-1";

                // Process
                return ServerRuntime.Update( manual.Values );
            }

            // Prepare to update
            var update = VCRConfiguration.Current.BeginUpdate( SettingNames.ScanDuration, SettingNames.MergeScanResult, SettingNames.ScanInterval, SettingNames.ScanHours, SettingNames.ScanJoinThreshold );

            // Fill it
            update[SettingNames.ScanHours].NewValue = string.Join( ", ", settings.Hours.Select( hour => hour.ToString() ) );
            update[SettingNames.ScanJoinThreshold].NewValue = settings.Threshold.ToString();
            update[SettingNames.MergeScanResult].NewValue = settings.MergeLists.ToString();
            update[SettingNames.ScanInterval].NewValue = settings.Interval.ToString();
            update[SettingNames.ScanDuration].NewValue = settings.Duration.ToString();

            // Process
            return ServerRuntime.Update( update.Values );
        }

        /// <summary>
        /// Liest die Konfiguration für die Programmzeitschrift.
        /// </summary>
        /// <param name="guide">Unterscheidet zwischen den einzelnen Methoden.</param>
        /// <returns>Die aktuellen Einstellungen.</returns>
        [HttpGet]
        public GuideSettings ReadGuide( string guide )
        {
            // Validate
            ServerRuntime.TestAdminAccess();

            // Load
            var interval = VCRConfiguration.Current.ProgramGuideUpdateInterval;
            var join = VCRConfiguration.Current.ProgramGuideJoinThreshold;

            // Report
            return
                new GuideSettings
                {
                    Sources = VCRConfiguration.Current.ProgramGuideSourcesAsArray.OrderBy( name => name, StringComparer.InvariantCultureIgnoreCase ).ToArray(),
                    Hours = VCRConfiguration.Current.ProgramGuideUpdateHoursAsArray.OrderBy( hour => hour ).ToArray(),
                    Interval = interval.HasValue ? (int) interval.Value.TotalHours : default( int? ),
                    Threshold = join.HasValue ? (int) join.Value.TotalHours : default( int? ),
                    Duration = VCRConfiguration.Current.ProgramGuideUpdateDuration,
                    WithUKGuide = VCRConfiguration.Current.EnableFreeSat,
                };
        }

        /// <summary>
        /// Aktualisiert die Einstellungen der Programmzeitschrift.
        /// </summary>
        /// <param name="guide">Dient zur Auswahl der Methoden.</param>
        /// <param name="settings">Die neuen Einstellungen.</param>
        /// <returns>Das Ergebnis der Änderung.</returns>
        [HttpPut]
        public bool? WriteGuide( string guide, [FromBody] GuideSettings settings )
        {
            // Check mode
            if (settings.Duration < 1)
            {
                // Create settings
                var disable = VCRConfiguration.Current.BeginUpdate( SettingNames.EPGDuration );

                // Store
                disable[SettingNames.EPGDuration].NewValue = "0";

                // Process
                return ServerRuntime.Update( disable.Values );
            }

            // Prepare to update
            var update = VCRConfiguration.Current.BeginUpdate( SettingNames.EPGDuration, SettingNames.EPGStations, SettingNames.EPGHours, SettingNames.EPGIncludeFreeSat, SettingNames.EPGInterval, SettingNames.EPGJoinThreshold );

            // Fill it
            update[SettingNames.EPGHours].NewValue = string.Join( ", ", settings.Hours.Select( hour => hour.ToString() ) );
            update[SettingNames.EPGIncludeFreeSat].NewValue = settings.WithUKGuide.ToString();
            update[SettingNames.EPGStations].NewValue = string.Join( ", ", settings.Sources );
            update[SettingNames.EPGJoinThreshold].NewValue = settings.Threshold.ToString();
            update[SettingNames.EPGInterval].NewValue = settings.Interval.ToString();
            update[SettingNames.EPGDuration].NewValue = settings.Duration.ToString();

            // Process
            return ServerRuntime.Update( update.Values );
        }

        /// <summary>
        /// Liest die Sicherheitseinstellungen.
        /// </summary>
        /// <param name="security">Unterscheidet zwischen den einzelnen Methoden.</param>
        /// <returns>Die aktuellen Einstellungen.</returns>
        [HttpGet]
        public SecuritySettings ReadSecurity( string security )
        {
            // Validate
            ServerRuntime.TestAdminAccess();

            // Report
            return
                new SecuritySettings
                {
                    AdminRole = VCRConfiguration.Current.AdminRole,
                    UserRole = VCRConfiguration.Current.UserRole,
                };
        }

        /// <summary>
        /// Aktualisiert die Sicherheitseinstellungen.
        /// </summary>
        /// <param name="security">Unterscheidet zwischen den einzelnen Methoden.</param>
        /// <param name="settings">Die neuen Einstellungen.</param>
        /// <returns><i>null</i> bei Fehlern und ansonsten gesetzt, wenn ein Neustart des Dienstes ausgeführt wird.</returns>
        [HttpPut]
        public bool? WriteSecurity( string security, [FromBody] SecuritySettings settings )
        {
            // Prepare to update
            var update = VCRConfiguration.Current.BeginUpdate( SettingNames.RequiredUserRole, SettingNames.RequiredAdminRole );

            // Change settings
            update[SettingNames.RequiredAdminRole].NewValue = settings.AdminRole;
            update[SettingNames.RequiredUserRole].NewValue = settings.UserRole;

            // Process
            return ServerRuntime.Update( update.Values );
        }

        /// <summary>
        /// Meldet sonstige Konfigurationsparameter.
        /// </summary>
        /// <param name="other">Wird zur Unterscheidung der Methoden verwendet.</param>
        /// <returns>Die aktuellen Einstellungen.</returns>
        [HttpGet]
        public OtherSettings ReadOtherSettings( string other )
        {
            // Validate
            ServerRuntime.TestAdminAccess();

            // Create response
            return
                new OtherSettings
                {
                    DelayAfterForcedHibernation = (uint) VCRConfiguration.Current.DelayAfterForcedHibernation.TotalMinutes,
                    SuppressDelayAfterForcedHibernation = VCRConfiguration.Current.SuppressDelayAfterForcedHibernation,
                    DisablePCRFromMPEG2 = VCRConfiguration.Current.DisablePCRFromMPEG2Generation,
                    DisablePCRFromH264 = VCRConfiguration.Current.DisablePCRFromH264Generation,
                    AllowBasic = VCRConfiguration.Current.EnableBasicAuthentication,
                    AllowHibernate = VCRConfiguration.Current.MayHibernateSystem,
                    HibernationDelay = VCRConfiguration.Current.HibernationDelay,
                    SSLPort = VCRConfiguration.Current.WebServerSecureTcpPort,
                    UseSSL = VCRConfiguration.Current.EncryptWebCommunication,
                    UseStandBy = VCRConfiguration.Current.UseS3ForHibernate,
                    ArchiveTime = VCRConfiguration.Current.ArchiveLifeTime,
                    ProtocolTime = VCRConfiguration.Current.LogLifeTime,
                    WebPort = VCRConfiguration.Current.WebServerTcpPort,
                    Logging = VCRConfiguration.Current.LoggingLevel,
                };
        }

        /// <summary>
        /// Aktualisiert die sonstigen Einstellungen.
        /// </summary>
        /// <param name="other">Wird zur Unterscheidung der Methoden verwendet.</param>
        /// <param name="settings">Die neuen Einstellungen.</param>
        /// <returns>Das Ergebnis der Änderung.</returns>
        [HttpPut]
        public bool? WriteOther( string other, [FromBody] OtherSettings settings )
        {
            // Prepare to update
            var update =
                VCRConfiguration.Current.BeginUpdate
                   (
                        SettingNames.SuppressDelayAfterForcedHibernation,
                        SettingNames.DisablePCRFromMPEG2Generation,
                        SettingNames.DisablePCRFromH264Generation,
                        SettingNames.DelayAfterForcedHibernation,
                        SettingNames.UseStandByForHibernation,
                        SettingNames.MayHibernateSystem,
                        SettingNames.HibernationDelay,
                        SettingNames.ArchiveLifeTime,
                        SettingNames.LoggingLevel,
                        SettingNames.LogLifeTime,
                        SettingNames.AllowBasic,
                        SettingNames.TCPPort,
                        SettingNames.SSLPort,
                        SettingNames.UseSSL
                    );

            // Change
            update[SettingNames.SuppressDelayAfterForcedHibernation].NewValue = settings.SuppressDelayAfterForcedHibernation.ToString();
            update[SettingNames.DelayAfterForcedHibernation].NewValue = settings.DelayAfterForcedHibernation.ToString();
            update[SettingNames.DisablePCRFromMPEG2Generation].NewValue = settings.DisablePCRFromMPEG2.ToString();
            update[SettingNames.DisablePCRFromH264Generation].NewValue = settings.DisablePCRFromH264.ToString();
            update[SettingNames.UseStandByForHibernation].NewValue = settings.UseStandBy.ToString();
            update[SettingNames.HibernationDelay].NewValue = settings.HibernationDelay.ToString();
            update[SettingNames.MayHibernateSystem].NewValue = settings.AllowHibernate.ToString();
            update[SettingNames.ArchiveLifeTime].NewValue = settings.ArchiveTime.ToString();
            update[SettingNames.LogLifeTime].NewValue = settings.ProtocolTime.ToString();
            update[SettingNames.AllowBasic].NewValue = settings.AllowBasic.ToString();
            update[SettingNames.LoggingLevel].NewValue = settings.Logging.ToString();
            update[SettingNames.SSLPort].NewValue = settings.SSLPort.ToString();
            update[SettingNames.TCPPort].NewValue = settings.WebPort.ToString();
            update[SettingNames.UseSSL].NewValue = settings.UseSSL.ToString();

            // Process
            return ServerRuntime.Update( update.Values );
        }
    }
}
