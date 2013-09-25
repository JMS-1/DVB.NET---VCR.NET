using System;
using NUnit.Framework;
using System.Configuration;

namespace JMS.DVB.SITables.Tests
{
    /// <summary>
    /// Basisklasse zur Implementierung von Tests.
    /// </summary>
    public abstract class TestBase
    {
        /// <summary>
        /// Erlaubt das dynamische Nachladen der DVB.NET Bibliotheken, die hier nicht direkt
        /// eingebunden sind.
        /// </summary>
        private static readonly RunTimeLoader s_Loader = RunTimeLoader.Instance;

        /// <summary>
        /// Stellt sicher, dass die Reservierung der Hardware nach einem Test aufgehoben wird.
        /// </summary>
        private IDisposable m_Hardware;

        /// <summary>
        /// Initialisiert die Basisklasse.
        /// </summary>
        protected TestBase()
        {
        }

        /// <summary>
        /// Ermittelt den Namen des zu verwendenden Geräteprofiles aus der Konfiguration.
        /// </summary>
        public static string ProfileName
        {
            get
            {
                // Just read
                return ConfigurationManager.AppSettings["ProfileName"];
            }
        }

        /// <summary>
        /// Ermittelt das aktuelle Geräteprofil.
        /// </summary>
        public static Profile Profile
        {
            get
            {
                // Forward
                return ProfileManager.FindProfile( ProfileName );
            }
        }

        /// <summary>
        /// Meldet das zu verwendende Gerät.
        /// </summary>
        /// <exception cref="NotSupportedException">Diese Methode darf nur während eines
        /// laufenden Tests aufgerufen werden.</exception>
        public Hardware Hardware
        {
            get
            {
                // Check mode
                if (m_Hardware == null)
                    throw new NotSupportedException();
                else
                    return HardwareManager.OpenHardware( Profile );
            }
        }

        /// <summary>
        /// Bereitet die Ausführung der Tests vor.
        /// </summary>
        [TestFixtureSetUp]
        public virtual void StartupTests()
        {
            // Allocate hardware
            using (m_Hardware)
                m_Hardware = HardwareManager.Open();
        }

        /// <summary>
        /// Schließt die Ausführung der Tests ab.        
        /// </summary>
        [TestFixtureTearDown]
        public virtual void FinishTests()
        {
            // Detach hardware
            using (m_Hardware)
                m_Hardware = null;
        }
    }
}
