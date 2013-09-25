using System;
using System.Reflection;
using System.Windows.Forms;
using JMS.DVB;


namespace DVBNETAdmin
{
    /// <summary>
    /// Hilfsklasse zum Starten der Administrationswerkzeugs.
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
        /// Zeigt das Hauptformular der Administration an und beendet sich, wenn dieses
        /// geschlossen wird.
        /// </summary>
        /// <param name="args">Alle Befehlszeilenparameter, die beim Aufruf der Anwendung angegeben
        /// wurden.</param>
        [STAThread]
        public static void Main( string[] args )
        {
            // Check settings
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (!version.Equals( Properties.Settings.Default.Version ))
            {
                // Upgrade
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.Version = version;
                Properties.Settings.Default.Save();
            }

            // Use the user language
            UserProfile.ApplyLanguage();

            // Standard stuff 
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new AdminMain() );
        }
    }
}
