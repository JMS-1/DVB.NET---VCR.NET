using System;
using System.IO;
using NUnit.Framework;
using System.Reflection;
using System.Configuration;
using Card_Server_Extender;

namespace JMS.DVB.CardServer.Tests
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
        /// Vermittelt den Zugriff auf die Hardware in einem separaten Prozeß.
        /// </summary>
        protected ServerImplementation CardServer { get; private set; }

        /// <summary>
        /// Initialisiert globale Felder.
        /// </summary>
        static TestBase()
        {
        }

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
        /// Bereitet die Ausführung der Tests vor.
        /// </summary>
        [TestFixtureSetUp]
        public virtual void StartupTests()
        {
            // Create card server
            using (CardServer)
                CardServer = ServerImplementation.CreateOutOfProcess();

            // Attach to profile
            ServerImplementation.EndRequest( CardServer.BeginSetProfile( ProfileName, false ) );
        }

        /// <summary>
        /// Schließt die Ausführung der Tests ab.        
        /// </summary>
        [TestFixtureTearDown]
        public virtual void FinishTests()
        {
            // Detach server
            using (CardServer)
                CardServer = null;
        }
    }
}
