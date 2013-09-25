using System;
using System.Configuration;
using System.IO;
using JMS.DVBVCR.RecordingService;


namespace JMS.DVBVCR.UnitTests
{
    /// <summary>
    /// Aktiviert eine Konfigurationsüberlagerung.
    /// </summary>
    public class TestConfigurationProvider : IDisposable
    {
        /// <summary>
        /// Stellt sicher, dass die Laufzeitumgebung zum Zugriff auf die Konfiguration vorbereitet ist.
        /// </summary>
        static TestConfigurationProvider()
        {
            // Start configuration
            VCRConfiguration.Startup();
        }

        /// <summary>
        /// Die von uns erstellte Datei.
        /// </summary>
        private string m_ConfigPath;

        /// <summary>
        /// Eine zusätzliche Leerdatei.
        /// </summary>
        private string m_ExePath;

        /// <summary>
        /// Erzeugt eine neue Überlagerung.
        /// </summary>
        /// <param name="configuration">Die zu verwendende Konfiguration in Rohform.</param>
        private TestConfigurationProvider( byte[] configuration )
        {
            // Requires cleanup
            try
            {
                // Create path
                var path = Path.Combine( Path.GetTempPath(), string.Format( "{0:N}.exe", Guid.NewGuid() ) );
                var configPath = path + ".config";

                // Create dummy executable - configuration manager will check if it exists
                File.WriteAllBytes( path, new byte[0] );
                m_ExePath = path;

                // Copy the configuration from the resource into the scratch file
                File.WriteAllBytes( configPath, configuration );
                m_ConfigPath = configPath;

                // Activate
                Tools.RefreshConfiguration( ConfigurationManager.OpenExeConfiguration( m_ExePath ) );
            }
            catch
            {
                // Cleanup
                Dispose();

                // Forward error
                throw;
            }
        }

        /// <summary>
        /// Erzeugt eine neue Überlagerung.
        /// </summary>
        /// <param name="configuration">Die zu verwendende Konfiguration in Rohform.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Konfiguration angegeben.</exception>
        public static IDisposable Create( byte[] configuration )
        {
            // Forward
            if (configuration == null)
                throw new ArgumentNullException( "configuration" );
            else
                return new TestConfigurationProvider( configuration );
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        public void Dispose()
        {
            // Revert configuration
            Tools.RefreshConfiguration( null );

            // Configuration
            var path = m_ConfigPath;
            if (!string.IsNullOrEmpty( path ))
            {
                // Reset
                m_ConfigPath = null;

                // Forward
                File.Delete( path );
            }

            // Executable
            path = m_ExePath;
            if (!string.IsNullOrEmpty( path ))
            {
                // Reset
                m_ExePath = null;

                // Forward
                File.Delete( path );
            }
        }

        #endregion
    }
}
