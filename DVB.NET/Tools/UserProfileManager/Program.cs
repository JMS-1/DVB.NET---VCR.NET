using System;
using System.Windows.Forms;

using JMS.DVB;


namespace UserProfileManager
{
    /// <summary>
    /// Erzwingt die Anzeige des Dialogs für die Einstellungen des DVB.NET Benutzerprofils.
    /// </summary>
    public class Program
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
        /// Startet das Hauptprogramm.
        /// </summary>
        /// <param name="args">Befehlszeilenparameter.</param>
        public static void Main( string[] args )
        {
            // Load the language
            UserProfile.ApplyLanguage();

            // Standard stuff 
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            // Force dialog to open
            UserProfile.ShowDialog();
        }
    }
}
