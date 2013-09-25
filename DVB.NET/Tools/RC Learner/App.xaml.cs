using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Configuration;
using System.Collections.Generic;

using JMS.DVB;
using JMS.DVB.DirectShow.RawDevices;
using System.Reflection;


namespace RC_Learner
{
    /// <summary>
    /// Repräsentiert den Windows Prozess.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Die aktuelle Umgebung zum Anlernen der Fernbedienung.
        /// </summary>
        public static LearnContext LearnContext { get; private set; }

        /// <summary>
        /// Aktiviert die Laufzeitumgebung von DVB.NET.
        /// </summary>
        static App()
        {
            // Activate dynamic loader
            RunTimeLoader.Startup();
        }

        /// <summary>
        /// Wird beim Starten der Anwendung aufgerufen.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Die Befehlszeilenparameter.</param>
        private void Application_Startup( object sender, StartupEventArgs e )
        {
            // Check settings
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (!version.Equals( RC_Learner.Properties.Settings.Default.Version ))
            {
                // Upgrade
                RC_Learner.Properties.Settings.Default.Upgrade();
                RC_Learner.Properties.Settings.Default.Version = version;
                RC_Learner.Properties.Settings.Default.Save();
            }

            // Apply language
            UserProfile.ApplyLanguage();

            // Check parameters
            if (e.Args.Length != 1)
                Shutdown();
            else
                LearnContext = LearnContext.Create( e.Args[0], null );
        }
    }
}
