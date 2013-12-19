using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;


namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt die benutzerspezifischen Vorgaben.
    /// </summary>
    [
        Serializable,
        XmlRoot( "UserProfile" )
    ]
    public class PersistedUserProfile
    {
        /// <summary>
        /// Die alte Art der Darstellung.
        /// </summary>
        [Serializable]
        public class Legacy
        {
            /// <summary>
            /// Der Name des Geräteprofils.
            /// </summary>
            public string ProfileName { get; set; }
        }

        /// <summary>
        /// Der XML Namensraum, der für die serialisierte Form verwendet wird.
        /// </summary>
        private const string ProfileNamespace = "http://jochen-manns.de/DVB.NET/Profiles/User";

        /// <summary>
        /// Die Datei, aus der diese Vorgaben entnommen wurden.
        /// </summary>
        private FileInfo m_File;

        /// <summary>
        /// Die bevorzugte Benutzersprache.
        /// </summary>
        public string PreferredLanguage { get; set; }

        /// <summary>
        /// Das zugehörige Geräteprofil.
        /// </summary>
        public Legacy Profile { get; set; }

        /// <summary>
        /// Der Name des zugehörigen Geräteprofils.
        /// </summary>
        [XmlIgnore]
        public string ProfileName
        {
            get
            {
                // Check profile
                var profile = Profile;
                if (profile == null)
                    return null;
                else
                    return profile.ProfileName;
            }
            set
            {
                // Update
                if (string.IsNullOrEmpty( value ))
                    Profile = null;
                else
                    Profile = new Legacy { ProfileName = value };
            }
        }

        /// <summary>
        /// Meldet oder legt fest, ob die Auswahl des Geräteprofils immer angezeigt werden soll.
        /// </summary>
        public bool AllwaysShowDialog { get; set; }

        /// <summary>
        /// Spichert die Vorgaben ab.
        /// </summary>
        public void Save()
        {
            // Open and forward
            using (var stream = new FileStream( m_File.FullName, FileMode.Create, FileAccess.Write, FileShare.None ))
            {
                // Create configuration
                var settings = new XmlWriterSettings { Encoding = Encoding.Unicode, Indent = true };

                // Create serializer
                var serializer = new XmlSerializer( GetType(), ProfileNamespace );

                // Process
                using (var writer = XmlWriter.Create( stream, settings ))
                    serializer.Serialize( writer, this );
            }
        }

        /// <summary>
        /// Lädt die Vorgaben des aktuellen Anwenders.
        /// </summary>
        /// <returns>Die Vorgaben für diesen Anwender.</returns>
        public static PersistedUserProfile Load()
        {
            // Get the user profile directory
            var profileDir = new DirectoryInfo( Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), "DVBNETProfiles" ) );

            // Create
            profileDir.Create();

            // Get the file name
            var profile = new FileInfo( Path.Combine( profileDir.FullName, "UserProfile.dup" ) );

            // The new profile
            PersistedUserProfile settings;

            // Load or create
            if (!profile.Exists)
                settings = new PersistedUserProfile();
            else
                using (var stream = new FileStream( profile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read ))
                {
                    // Create serializer
                    var serializer = new XmlSerializer( typeof( PersistedUserProfile ), ProfileNamespace );

                    // Process
                    settings = (PersistedUserProfile) serializer.Deserialize( stream );
                }

            // Remember root
            settings.m_File = profile;

            // Report
            return settings;
        }
    }

    /// <summary>
    /// Verwaltet die Voreinstellungen des aktuellen Anwenders.
    /// </summary>
    public static class UserProfile
    {
        /// <summary>
        /// Die aktuelle Konfiguration.
        /// </summary>
        private static PersistedUserProfile m_Settings = PersistedUserProfile.Load();

        /// <summary>
        /// Speichert die Einstellungen des aktuellen Anwenders.
        /// </summary>
        public static void Save()
        {
            // Forward
            m_Settings.Save();
        }

        /// <summary>
        /// Der Name des zu verwendenden Geräteprofils.
        /// </summary>
        public static string RawProfileName { get { return m_Settings.ProfileName; } set { m_Settings.ProfileName = value; } }

        /// <summary>
        /// Meldet das zu verwendende Geräteprofil. Sollte dieses nicht gesetzt sein, so wird
        /// der Auswahldialog angezeigt.
        /// </summary>
        public static Profile Profile
        {
            get
            {
                // Attach the profile from the command line
                string commandProfile = CommandLineProfile;

                // Show always?
                if (!string.IsNullOrEmpty( commandProfile ) || !RawAlwaysShowDialog)
                {
                    // See if profile is set
                    string name = string.IsNullOrEmpty( commandProfile ) ? RawProfileName : commandProfile;
                    if (!string.IsNullOrEmpty( name ))
                    {
                        // Find it
                        Profile profile = ProfileManager.FindProfile( name );
                        if (null != profile)
                            return profile;
                    }
                }

                // Show dialog
                if (!ShowDialog())
                    return null;

                // Try again
                string profileName = ProfileName;
                if (string.IsNullOrEmpty( profileName ))
                    return null;
                else
                    return ProfileManager.FindProfile( profileName );
            }
        }

        /// <summary>
        /// Zeigt den Dialog zur Pflege der Benutzerdaten an.
        /// </summary>
        /// <returns>Gesetzt, wenn der Dialog bestätigt wurde.</returns>
        public static bool ShowDialog()
        {
            // Just show it
            using (var dialog = new UserProfileManager())
                return (dialog.ShowDialog() == DialogResult.OK);
        }

        /// <summary>
        /// Meldet das Geräteprofil, dass über die Befehlszeile festlegt wurde.
        /// </summary>
        public static string CommandLineProfile
        {
            get
            {
                // Get parameter list
                var args = Environment.GetCommandLineArgs();
                if (null == args)
                    return null;

                // Find the parameter
                var overwrite = args.FirstOrDefault( a => (null != a) && a.StartsWith( "/profile=Common:" ) );
                if (string.IsNullOrEmpty( overwrite ))
                    return null;

                // Cut off
                overwrite = overwrite.Substring( 16 ).Trim();

                // Report
                if (string.IsNullOrEmpty( overwrite ))
                    return null;
                else
                    return overwrite;
            }
        }

        /// <summary>
        /// Ermittelt das bevorzugte Geräteprofil des Anwenders, i.a. <see cref="RawProfileName"/>, aber
        /// überladbar duch den Befehlszeileneparameter <i>/profile=Common:</i>.
        /// </summary>
        public static string ProfileName
        {
            get
            {
                // Ask command line
                string overwrite = CommandLineProfile;

                // Dispatch
                if (string.IsNullOrEmpty( overwrite ))
                    return RawProfileName;
                else
                    return overwrite;
            }
        }

        /// <summary>
        /// Ermittelt die zu verwendende Sprache, im Prinzip <see cref="RawLanguage"/> aber optional
        /// überschrieben durch den Befehlszeilenparameter <i>/language=</i>.
        /// </summary>
        public static string Language
        {
            get
            {
                // Get parameter list
                var args = Environment.GetCommandLineArgs();
                if (null == args)
                    return RawLanguage;

                // Find the parameter
                var overwrite = Array.Find( args, a => (null != a) && a.StartsWith( "/language=" ) );
                if (string.IsNullOrEmpty( overwrite ))
                    return RawLanguage;

                // Cut off
                overwrite = overwrite.Substring( 10 ).Trim();

                // Report
                if (string.IsNullOrEmpty( overwrite ))
                    return RawLanguage;
                else
                    return overwrite;
            }
        }

        /// <summary>
        /// Setzt die Sprache auf dem aktuellen <see cref="Thread"/>.
        /// </summary>
        public static void ApplyLanguage()
        {
            // Load the language
            var language = Language;
            if (string.IsNullOrEmpty( language ))
                return;

            // Try to use it
            try
            {
                // Set the language
                Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture( language );
            }
            catch
            {
                // Just ignore
            }
        }

        /// <summary>
        /// Meldet die bevorzugte Sprache des Anwenders, so wie sie in den Vorgaben festgelegt 
        /// ist.
        /// </summary>
        public static string RawLanguage
        {
            get
            {
                // Just forward
                if (string.IsNullOrEmpty( m_Settings.PreferredLanguage ))
                    return null;
                else
                    return m_Settings.PreferredLanguage;
            }
            set
            {
                // Store
                if (string.IsNullOrEmpty( value ))
                    m_Settings.PreferredLanguage = null;
                else
                    m_Settings.PreferredLanguage = value;
            }
        }

        /// <summary>
        /// Meldet oder legt fest, ob der Auswahldialog immer angezeigt werden soll.
        /// </summary>
        public static bool RawAlwaysShowDialog { get { return m_Settings.AllwaysShowDialog; } set { m_Settings.AllwaysShowDialog = value; } }
    }
}
