using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;


namespace JMS.DVB
{
    /// <summary>
    /// Mit dieser Klasse werden DVB.NET Geräteprofile verwaltet und der Zugriff
    /// auf DVB Hardware über Hardwareabstraktionen vermittelt.
    /// </summary>
    public static class ProfileManager
    {
        /// <summary>
        /// Vergleichsmethode für die Namen von Geräteprofilen.
        /// </summary>
        public static readonly StringComparer ProfileNameComparer = StringComparer.InvariantCultureIgnoreCase;

        /// <summary>
        /// Dateiendung für DVB.NET Geräteprofile ab Version 4.0.
        /// </summary>
        public const string ProfileExtension = "dnp";

        /// <summary>
        /// Der aktuelle Änderungszähler.
        /// </summary>
        private static volatile int _RefreshCounter;

        /// <summary>
        /// Meldet den aktuellen Änderungszähler.
        /// </summary>
        public static int RefreshCounter { get { return _RefreshCounter; } }

        /// <summary>
        /// Alle möglichen Arten von Geräteprofilen.
        /// </summary>
        private static readonly Dictionary<string, XmlSerializer> ProfileTypes =
            new Dictionary<string, XmlSerializer>
            {
                {"TerrestrialProfile", new XmlSerializer( typeof( TerrestrialProfile ), Profile.Namespace )},
                {"SatelliteProfile", new XmlSerializer( typeof( SatelliteProfile ), Profile.Namespace )},
                {"CableProfile", new XmlSerializer( typeof( CableProfile ), Profile.Namespace )},
            };

        /// <summary>
        /// Alle bekannten Geräteprofile.
        /// </summary>
        private static List<Profile> m_profiles = null;

        /// <summary>
        /// Meldet den vollen Pfad zum Verzeichnis aller Geräteprofile.
        /// </summary>
        public static DirectoryInfo ProfilePath
        {
            get
            {
                // Report
                return new DirectoryInfo( Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData ), "DVBNETProfiles" ) );
            }
        }

        /// <summary>
        /// Ermittelt ein bestimmtes Geräteprofil.
        /// </summary>
        /// <param name="name">Der eindeutige Name des Profils.</param>
        /// <returns>Das gewünschte profil oder <i>null</i>, wenn keines existiert.</returns>
        public static Profile FindProfile( string name )
        {
            // Just try it
            return Array.Find( AllProfiles, p => ProfileNameComparer.Equals( p.Name, name ) );
        }

        /// <summary>
        /// Lädt ein Geräteprofil aus einem Datenstrom.
        /// </summary>
        /// <param name="stream">Der Datenstrom mit einem serialisierten Geräteprofil.</param>
        /// <returns>Das gewünschte Profil oder <i>null</i>, wenn ein Laden nicht möglich war.</returns>
        public static Profile LoadProfile( Stream stream )
        {
            // Be safe
            try
            {
                // Remember position
                var pos = stream.Seek( 0, SeekOrigin.Current );

                // Load the profile
                using (var reader = XmlReader.Create( stream ))
                    while (reader.Read())
                        if (reader.IsStartElement())
                        {
                            // Attach to the serializer
                            var serializer = ProfileTypes[reader.Name];

                            // Reset the stream
                            stream.Seek( pos, SeekOrigin.Begin );

                            // Load the profile
                            return (Profile) serializer.Deserialize( stream );
                        }
            }
            catch
            {
                // Ignore any error
            }

            // Not available
            return null;
        }

        /// <summary>
        /// Lädt ein Geräteprofil aus einer Datei.
        /// </summary>
        /// <param name="path">Der volle Pfad zur Profildatei.</param>
        /// <returns>Das gewünschte Profil oder <i>null</i>, wenn ein Laden nicht möglich war.</returns>
        public static Profile LoadProfile( FileInfo path )
        {
            // Be safe
            try
            {
                // Not there
                if (path.Exists)
                    using (var stream = new FileStream( path.FullName, FileMode.Open, FileAccess.Read, FileShare.Read ))
                    {
                        // Load the profile
                        var profile = LoadProfile( stream );

                        // Attach to file
                        if (profile != null)
                            profile.ProfilePath = path;

                        // Report
                        return profile;
                    }
            }
            catch
            {
                // Ignore any error
            }

            // Not available
            return null;
        }

        /// <summary>
        /// Fügt ein neues Geräteprofil im Speicher hinzu.
        /// </summary>
        /// <param name="profile">Das hinzuzufügende temporäre Profil.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Geräteprofil übergeben.</exception>
        /// <exception cref="ArgumentException">Es existiert bereits ein Geräteprofil mit dem angegebenen
        /// Namen oder das übergebene Profil ist kein temporäres Profil.</exception>
        public static void AddVolatileProfile( Profile profile )
        {
            // Validate
            if (profile == null)
                throw new ArgumentNullException( "profile" );

            // Must be volatile
            if (string.IsNullOrEmpty( profile.VolatileName ))
                throw new ArgumentException( profile.Name, "profile" );

            // Synchronize
            lock (ProfileTypes)
                if (FindProfile( profile.Name ) != null)
                    throw new ArgumentException( profile.Name, "profile" );
                else
                    m_profiles.Add( profile );
        }

        /// <summary>
        /// Meldet alle verfügbaren Geräteprofile - beginnend mit DVB.NET 4.0
        /// werden ausschließlich gemeinsame Profile angeboten.
        /// </summary>
        public static Profile[] AllProfiles
        {
            get
            {
                // Synchronize
                lock (ProfileTypes)
                {
                    // Load once
                    if (m_profiles == null)
                    {
                        // Create
                        m_profiles = new List<Profile>();

                        // Attach to the path
                        var profileDirectory = ProfilePath;

                        // Find all
                        if (profileDirectory.Exists)
                            foreach (var profilePath in profileDirectory.GetFiles( "*." + ProfileExtension ))
                            {
                                // Load it
                                var profile = LoadProfile( profilePath );

                                // Remember it
                                if (profile != null)
                                    m_profiles.Add( profile );
                            }
                    }

                    // Report
                    return m_profiles.ToArray();
                }
            }
        }

        /// <summary>
        /// Lädt alle Geräteprofile beim nächsten Zugriff neu.
        /// </summary>
        public static void Refresh()
        {
            // Synchronize
            lock (ProfileTypes)
                m_profiles = null;

            // Count up
#pragma warning disable 0420
            Interlocked.Increment( ref _RefreshCounter );
#pragma warning restore 0420
        }

        /// <summary>
        /// Erzeugt ein neues Geräteprofil.
        /// </summary>
        /// <typeparam name="T">Die gewünschte Art des Geräteprofils.</typeparam>
        /// <param name="name">Der Name des Geräteprofils, wobei nur Zeichen erlaubt
        /// sind, die auch in Dateinamen vorkommen dürfen.</param>
        /// <returns>Ein neu angelegtes Geräteprofil. Es wurde noch nicht in
        /// der zugeordneten Datei gespeichert.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Name angegeben.</exception>
        /// <exception cref="ArgumentException">Der Name ist ungültig oder es existiert bereits
        /// ein gleichnamiges Geräteprofil.</exception>
        public static T CreateProfile<T>( string name ) where T : Profile, new()
        {
            // Validate
            if (string.IsNullOrEmpty( name ))
                throw new ArgumentNullException( "name" );
            if (name.IndexOfAny( Path.GetInvalidFileNameChars() ) >= 0)
                throw new ArgumentException( name, "name" );

            // Create the new instance
            T profile = new T();

            // Attach the file information to it
            profile.ProfilePath = new FileInfo( Path.Combine( ProfilePath.FullName, name + "." + ProfileExtension ) );

            // Validate
            if (profile.ProfilePath.Exists)
                throw new ArgumentException( profile.ProfilePath.FullName, "name" );
            else
                return profile;
        }

        /// <summary>
        /// Ermittelt die Zugriffsdaten für eine bestimmte Quelle.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Die Informationen zur Quelle.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
        public static SourceSelection[] FindSource( SourceIdentifier source )
        {
            // Validate
            if (source == null)
                throw new ArgumentNullException( "source" );
            else
                return AllProfiles.SelectMany( profile => profile.FindSource( source ) ).ToArray();
        }
    }
}
