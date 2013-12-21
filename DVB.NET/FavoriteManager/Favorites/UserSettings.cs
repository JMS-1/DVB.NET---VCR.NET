using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace JMS.DVB.Favorites
{
    /// <summary>
    /// Die Serialisierung der bevorzugten Quellen.
    /// </summary>
    [Serializable]
    public class UserSettings
    {
        /// <summary>
        /// Gesetzt, wenn die Einschränkung auf bevorzugte Quellen aktiviert werden soll.
        /// </summary>
        public bool EnableShortCuts = false;

        /// <summary>
        /// Die bevorzugte Sprache der Tonspur.
        /// </summary>
        public string PreferredLanguage = "Deutsch";

        /// <summary>
        /// Gesetzt, wenn bevorzugt <i>Dolby Digital</i> Tonspuren verwendet werden sollen.
        /// </summary>
        public bool PreferAC3 = false;

        /// <summary>
        /// Die Liste der Namen der bevorzugten Quellen.
        /// </summary>
        public string[] FavoriteChannels = { };

        private static string DefaultPath
        {
            get
            {
                // Attach to user settings directory
                var userDir = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );

                // Append
                var file = new FileInfo( Path.Combine( userDir, @"JMS\DVB.NET Favorites.xml" ) );

                // Create directory
                if (!file.Directory.Exists) file.Directory.Create();

                // Report
                return file.FullName;
            }
        }

        /// <summary>
        /// Speichert die Konfiguration.
        /// </summary>
        public void Save()
        {
            // Open file
            using (var file = new FileStream( DefaultPath, FileMode.Create, FileAccess.Write, FileShare.None ))
            {
                // Create serializer
                var serializer = new XmlSerializer( GetType() );

                // Create settings
                var settings = new XmlWriterSettings
                {
                    Encoding = Encoding.Unicode,
                    Indent = true,
                };

                // Process
                using (var writer = XmlWriter.Create( file, settings ))
                    serializer.Serialize( writer, this );
            }
        }

        /// <summary>
        /// Lädt die Konfiguration.
        /// </summary>
        /// <returns>Die aktuell gültige Konfiguration.</returns>
        public static UserSettings Load()
        {
            try
            {
                // Open file
                using (var file = new FileStream( DefaultPath, FileMode.Open, FileAccess.Read, FileShare.Read ))
                {
                    // Create serializer
                    var serializer = new XmlSerializer( typeof( UserSettings ) );

                    // Load
                    return (UserSettings) serializer.Deserialize( file );
                }
            }
            catch (Exception)
            {
                // Create a new one on every error
                return new UserSettings();
            }
        }
    }
}
