using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using JMS.DVB;


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
        /// Einsprungpunkt der Anwendung.
        /// </summary>
        /// <param name="args">Befehlszeilenparameter zur Anwendung.</param>
        [STAThread]
        public static void Main( string[] args )
        {
            // Set language
            if (!string.IsNullOrEmpty( UserProfile.Language ))
                try
                {
                    // Set the language
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture( UserProfile.Language );
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
