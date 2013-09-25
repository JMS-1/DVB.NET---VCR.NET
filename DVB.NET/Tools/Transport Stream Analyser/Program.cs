using System;
using System.Windows.Forms;

using JMS.DVB;


namespace Transport_Stream_Analyser
{
    /// <summary>
    /// Rahmen zum Starten der Anwendung.
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
        /// Beginnt mit der Ausführung des Programms.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            // Activate DVB.NET language settings
            UserProfile.ApplyLanguage();

            // Prepare UI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            // Start the form
            Application.Run( new AnalyserMain() );
        }
    }
}
