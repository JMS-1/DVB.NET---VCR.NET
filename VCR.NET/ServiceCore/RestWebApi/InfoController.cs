using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Http;
using JMS.DVBVCR.RecordingService.WebServer;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Web Service für allgemeine Informationen.
    /// </summary>
    public class InfoController : ApiController
    {
        /// <summary>
        /// Ermittelt zu einer Komponente das Produkt.
        /// </summary>
        /// <param name="componentCode">Die eindeutige Kennung der Komponente.</param>
        /// <param name="productCode">Die eindeutige Kennung des Produktes.</param>
        /// <returns>Der zugehörige Fehlercode.</returns>
        [DllImport( "msi.dll", CharSet = CharSet.Unicode )]
        private static extern uint MsiGetProductCode( string componentCode, StringBuilder productCode );

        /// <summary>
        /// Ermittelt eine Eigenschaft eines Produktes.
        /// </summary>
        /// <param name="productCode">Die eindeutige Kennung des Produktes.</param>
        /// <param name="propertyIdentifier">Die gewünschte Eigenschaft.</param>
        /// <param name="propertyValue">Der Wert der Eigenschaft.</param>
        /// <param name="valueSize">Die Größe des Speichers.</param>
        /// <returns>Der zugehörige Fehlercode.</returns>
        [DllImport( "msi.dll", CharSet = CharSet.Unicode )]
        private static extern uint MsiGetProductInfo( string productCode, string propertyIdentifier, char[] propertyValue, ref UInt32 valueSize );

        /// <summary>
        /// Die exakte Version der Installation.
        /// </summary>
        private static volatile string _InstalledVersion = null;

        /// <summary>
        /// Sorgt dafür, dass die Version nur einmalig ermittelt wird.
        /// </summary>
        private static object _VersionLock = new object();

        /// <summary>
        /// Meldet die exakte Version der Installation.
        /// </summary>
        private static string InstalledVersion
        {
            get
            {
                // Load once
                if (_InstalledVersion == null)
                    lock (_VersionLock)
                        if (_InstalledVersion == null)
                        {
                            // Process
                            try
                            {
                                // Buffer for product
                                var productCode = new StringBuilder( 39 );

                                // Lookup product
                                if (MsiGetProductCode( "{2cbfae32-689e-43d8-9665-ab5b6c06d4d6}", productCode ) == 0)
                                {
                                    // Buffer for version
                                    UInt32 bufferSize = 100;
                                    var buffer = new char[bufferSize + 1];

                                    // Retrieve
                                    if (MsiGetProductInfo( productCode.ToString(), "VersionString", buffer, ref bufferSize ) == 0)
                                        _InstalledVersion = new string( buffer, 0, checked( (int) bufferSize ) );
                                }
                            }
                            catch (Exception e)
                            {
                                // Report
                                Tools.ExtendedLogging( "Unable to retrieve MSI Product Version: {0}", e.Message );
                            }

                            // Default
                            if (_InstalledVersion == null)
                                _InstalledVersion = "-";
                        }

                // Report
                return _InstalledVersion;
            }
        }

        /// <summary>
        /// Meldet Informationen zur Version des VCR.NET Recording Service.
        /// </summary>
        [HttpGet]
        public InfoService VersionInformation()
        {
            // Load
            var settings = ServerRuntime.VCRServer.Settings;

            // Report
            return
                new InfoService
                {
                    HasPendingExtensions = ServerRuntime.VCRServer.ExtensionProcessManager.HasActiveProcesses,
                    SourceScanEnabled = (VCRConfiguration.Current.SourceListUpdateInterval != 0),
                    GuideUpdateEnabled = VCRConfiguration.Current.ProgramGuideUpdateEnabled,
                    MinimumHibernationDelay = settings.MinimumHibernationDelay,
                    HasPendingHibernation = settings.HasPendingHibernation,
                    IsRunning = ServerRuntime.VCRServer.IsActive,
                    ProfilesNames = settings.Profiles.ToArray(),
                    InstalledVersion = InstalledVersion,
                    Version = VCRServer.CurrentVersion,
                    IsAdmin = ServerRuntime.IsAdmin,
                };
        }

        /// <summary>
        /// Meldet alle möglichen Aufzeichnungsverzeichnisse.
        /// </summary>
        /// <param name="directories">Wird zur Unterscheidung der Methoden verwendet.</param>
        /// <returns>Die gewünschte Liste.</returns>
        [HttpGet]
        public string[] GetRecordingDirectories( string directories )
        {
            // First is default
            return VCRConfiguration.Current.TargetDirectoriesNames.SelectMany( ScanDirectory ).ToArray();
        }

        /// <summary>
        /// Meldet alle Aufträge.
        /// </summary>
        /// <param name="jobs">Wird zur Unterscheidung der Methoden verwendet.</param>
        /// <returns>Die Liste aller Aufträge.</returns>
        [HttpGet]
        public InfoJob[] GetJobs( string jobs )
        {
            // Report
            return
                ServerRuntime
                    .VCRServer
                    .GetJobs( InfoJob.Create )
                    .OrderBy( job => job.Name ?? string.Empty, StringComparer.InvariantCulture )
                    .ToArray();
        }

        /// <summary>
        /// Meldet alle Benutzergruppen des Rechners, auf dem der <i>VCR.NET Recording Service läuft.</i>
        /// </summary>
        /// <param name="groups">Wird zur Unterscheidung der Methoden verwendet.</param>
        /// <returns>Die gewünschte Liste.</returns>
        [HttpGet]
        public string[] GetUserGroups( string groups )
        {
            // Resulting groups
            var result = new List<string>();

            // Load list box
            using (var query = new ManagementObjectSearcher( "SELECT Name FROM Win32_Group WHERE LocalAccount = TRUE" ))
                foreach (var group in query.Get())
                    using (group)
                        result.Add( (string) group["Name"] );

            // Sort
            result.Sort( StringComparer.InvariantCultureIgnoreCase );

            // Report
            return result.ToArray();
        }

        /// <summary>
        /// Meldet den Namen eines Verzeichnisses und rekursiv alle Unterverzeichnisse.
        /// </summary>
        /// <param name="directory">Das zu untersuchende Verzeichnis.</param>
        /// <returns>Die Liste der Verzeichnisse.</returns>
        private static IEnumerable<string> ScanDirectory( string directory )
        {
            // See if the directory is valid
            DirectoryInfo info;
            try
            {
                // Create
                info = new DirectoryInfo( directory );
            }
            catch (Exception)
            {
                // Not valid
                info = null;
            }

            // Done
            if (info == null)
                yield break;

            // Report self - always, even if it does not exist
            yield return info.FullName;

            // Get the sub directories
            string[] children;
            try
            {
                // Try if directory exists
                if (info.Exists)
                    children = info.GetDirectories().Select( child => child.FullName ).ToArray();
                else
                    children = null;
            }
            catch (Exception)
            {
                // None
                children = null;
            }

            // Forward
            if (children != null)
                foreach (var child in children.SelectMany( ScanDirectory ))
                    yield return child;
        }
    }
}
