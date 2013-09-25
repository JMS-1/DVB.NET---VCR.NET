using System;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.Windows.Forms;


namespace JMS.DVB
{
    /// <summary>
    /// Verwaltet die Voreinstellungen des aktuellen Anwenders.
    /// </summary>
    public static class UserProfile
    {
        /// <summary>
        /// Die Einstellungen des aktuellen Anwenders.
        /// </summary>
        private static JMS.ChannelManagement.UserProfile m_Settings = JMS.ChannelManagement.UserProfile.Load();

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
        public static string RawProfileName
        {
            get
            {
                // None
                if (null == m_Settings.Profile)
                    return null;

                // Not valid
                if (!m_Settings.Profile.IsSystemProfile)
                    return null;

                // Not set
                if (string.IsNullOrEmpty( m_Settings.Profile.ProfileName ))
                    return null;
                else
                    return m_Settings.Profile.ProfileName;
            }
            set
            {
                // Store
                if (null == value)
                    m_Settings.Profile = null;
                else
                    m_Settings.Profile = new JMS.ChannelManagement.DeviceProfileReference( value, true );
            }
        }

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
            using (UserProfileManager dialog = new UserProfileManager())
                return (DialogResult.OK == dialog.ShowDialog());
        }

        /// <summary>
        /// Meldet das Geräteprofil, dass über die Befehlszeile festlegt wurde.
        /// </summary>
        public static string CommandLineProfile
        {
            get
            {
                // Get parameter list
                string[] args = Environment.GetCommandLineArgs();

                // None
                if (null == args)
                    return null;

                // Find the parameter
                string overwrite = args.FirstOrDefault( a => (null != a) && a.StartsWith( "/profile=Common:" ) );

                // Not found
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
                string[] args = Environment.GetCommandLineArgs();

                // None
                if (null == args)
                    return RawLanguage;

                // Find the parameter
                string overwrite = Array.Find( args, a => (null != a) && a.StartsWith( "/language=" ) );

                // Not found
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
            string language = Language;

            // None set
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
        public static bool RawAlwaysShowDialog
        {
            get
            {
                // Report
                return m_Settings.AllwaysShowDialog;
            }
            set
            {
                // Store
                m_Settings.AllwaysShowDialog = value;
            }
        }
    }
}
