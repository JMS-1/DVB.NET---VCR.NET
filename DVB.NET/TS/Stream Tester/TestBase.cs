using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Configuration;

namespace JMS.DVB.TS.Tests
{
    /// <summary>
    /// Basisklasse zur Implementierung von Tests.
    /// </summary>
    public abstract class TestBase
    {
        /// <summary>
        /// Ein Zufallsgenerator für die Tests.
        /// </summary>
        protected static readonly Random Generator = new Random( Environment.TickCount );

        /// <summary>
        /// Erlaubt das dynamische Nachladen der DVB.NET Laufzeitumgebung.
        /// </summary>
        private static readonly RunTimeLoader s_Loader = RunTimeLoader.Instance;

        /// <summary>
        /// Das aktuelle Arbeitsverzeichnis.
        /// </summary>
        protected static DirectoryInfo TestDirectory { get; private set; }

        /// <summary>
        /// Das aktuelle Geräteprofil.
        /// </summary>
        protected static Profile Profile { get; private set; }

        /// <summary>
        /// Die Steuerung des Zugriffs auf die DVB Hardware.
        /// </summary>
        private IDisposable m_Hardware;

        /// <summary>
        /// Initialisiert statische Felder.
        /// </summary>
        static TestBase()
        {
            // Load from configuration
            TestDirectory = new DirectoryInfo( ConfigurationManager.AppSettings["TestDirectory"] );
            Profile = ProfileManager.FindProfile( ConfigurationManager.AppSettings["ProfileName"] );

            // Create
            if (!TestDirectory.Exists)
            {
                // Do it
                TestDirectory.Create();

                // Must manually refresh
                TestDirectory.Refresh();
            }
        }

        /// <summary>
        /// Initialisiert die Basisklasse.
        /// </summary>
        protected TestBase()
        {
        }

        /// <summary>
        /// Entfernt alle Dateien aus dem Testverzeichnis.
        /// </summary>
        public void Cleanup()
        {
            // Wipe out
            foreach (var file in TestDirectory.GetFiles())
                file.Delete();
        }

        /// <summary>
        /// Ermittelt das aktuell gewählte Gerät.
        /// </summary>
        protected Hardware Device
        {
            get
            {
                // Forward
                return HardwareManager.OpenHardware( Profile );
            }
        }

        /// <summary>
        /// Wird einmal vor allen Tests aufgerufen.
        /// </summary>
        [TestFixtureSetUp]
        public virtual void OnBeforeAllTests()
        {
            // Cleanup directory and re-created
            Cleanup();

            // Attach to hardware
            using (m_Hardware)
                m_Hardware = HardwareManager.Open();
        }

        /// <summary>
        /// Wird einmal nach allen Tests aufgerufen.
        /// </summary>
        [TestFixtureTearDown]
        public virtual void OnAfterAllTests()
        {
            // Detach from hardward
            using (m_Hardware)
                m_Hardware = null;
        }

        /// <summary>
        /// Ermittelt den bevorzugten Sender.
        /// </summary>
        /// <returns>Der gewünschte Sender.</returns>
        protected SourceInformation GetDefaultStation()
        {
            // Load
            var source = Profile.FindSource( ConfigurationManager.AppSettings["Station"] ).Single();

            // Select
            source.SelectGroup();

            // Attach to source information
            var info = source.GetSourceInformation();

            // Must decyrpt
            if (info != null)
                if (info.IsEncrypted)
                    try
                    {
                        // Try
                        Device.Decrypt( info.Source );
                    }
                    catch (Exception e)
                    {
                        // Report
                        Console.Error.WriteLine( "Decrypt: {0}", e.Message );
                    }

            // Report
            return info;
        }

        /// <summary>
        /// Ermittelt den Namen einer Datei.
        /// </summary>
        /// <returns>Der Name einer neuen Datei.</returns>
        public static FileInfo GetUniqueFile()
        {
            // Create
            return new FileInfo( Path.Combine( TestDirectory.FullName, Guid.NewGuid().ToString( "N" ) + ".ts" ) );
        }
    }
}
