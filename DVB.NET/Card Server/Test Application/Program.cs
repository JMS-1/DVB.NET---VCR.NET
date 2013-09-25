extern alias oldVersion;

using System;
using System.Threading;
using System.Globalization;
using System.Windows.Forms;

using JMS.DVB;

using legacy = oldVersion.JMS.ChannelManagement;


namespace CardServerTester
{
    /// <summary>
    /// Mit dieser Klasse wird die Anwendung gestartet.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Installiert die Laufzeitumgebung.
        /// </summary>
        static Program()
        {
            // Activate dynamic loading
            RunTimeLoader.Startup();
        }

        /// <summary>
        /// Die bevorzugten Werte des aktuellen Anwenders.
        /// </summary>
        public static legacy.UserProfile UserProfile { get; private set; }

        /// <summary>
        /// Einsprungpunkt der Anwendung.
        /// </summary>
        /// <param name="args">Befehlszeilenparameter zur Anwendung.</param>
        [STAThread]
        public static void Main( string[] args )
        {
            // Load user profile
            try
            {
                // Load
                UserProfile = legacy.UserProfile.Load();
            }
            catch
            {
                // Use empty
                UserProfile = new legacy.UserProfile();
            }

            // Set language
            if (!string.IsNullOrEmpty( UserProfile.PreferredLanguage ))
                try
                {
                    // Set the language
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture( UserProfile.PreferredLanguage );
                }
                catch
                {
                    // Just ignore
                }

            // Show the main form
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new TesterMain() );
        }
    }
}
