using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;


namespace JMS.DVB.DirectShow.RawDevices
{
    /// <summary>
    /// Beschreibt die Konfiguration der Fernbedienung.
    /// </summary>
    [Serializable]
    public class RCSettings
    {
        /// <summary>
        /// Der volle Pfad zur Standardkonfiguration im Verzeichnis der Anwendung.
        /// </summary>
        public static FileInfo ConfigurationFile { get; private set; }

        /// <summary>
        /// Die Serialisierungskomponente.
        /// </summary>
        private static readonly XmlSerializer s_Serializer;

        /// <summary>
        /// Initialisiert die statischen Daten des Dienstes.
        /// </summary>
        static RCSettings()
        {
            // Directory of executable
            var rootDirectory = new DirectoryInfo( Path.GetDirectoryName( Application.ExecutablePath ) );

            // Load environment
            ConfigurationFile = new FileInfo( Path.Combine( rootDirectory.FullName, "Infrared.xml" ) );

            // Create serializer
            s_Serializer = new XmlSerializer( typeof( RCSettings ), null, new[] { typeof( Keys ) }, null, "http://psimarron.net/DVBNET/RemoteControl" );
        }

        /// <summary>
        /// Alle bekannten Abbildungen.
        /// </summary>
        private Dictionary<InputMapping, InputMapping> m_Mappings = new Dictionary<InputMapping, InputMapping>();

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public RCSettings()
        {
        }

        /// <summary>
        /// Ergänzt eine Abbildungsvorschrift während des Ladevorgangs.
        /// </summary>
        /// <param name="mapping">Eine Vorschrift.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Vorschrift angegeben.</exception>
        /// <returns>Gesetzt, wenn die Vorschrift tatsächlich ergänzt wurde.</returns>
        public bool Add( InputMapping mapping )
        {
            // Validate
            if (mapping == null)
                throw new ArgumentNullException( "mapping" );

            // Got it
            if (m_Mappings.ContainsKey( mapping ))
                return false;

            // Just remember           
            m_Mappings.Add( mapping, mapping );

            // Report
            return true;
        }

        /// <summary>
        /// Startet eine Anwendung zur Pflege einer Konfiguration.
        /// </summary>
        /// <param name="path">Der volle Pfad zur Konfiguration.</param>
        /// <returns>Der neu gestartete Prozess.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Dateipfad angegeben.</exception>
        public static Process Edit( FileInfo path )
        {
            // Validate
            if (path == null)
                throw new ArgumentNullException( "path" );

            // Create process record
            var startup =
                new ProcessStartInfo
                    (
                        Path.Combine( RunTimeLoader.RootDirectory.FullName, @"Tools\RCLearner.exe" ),
                        string.Format( "\"{0}\"", path.FullName.Replace( "\"", "\"\"" ) )
                    )
                    {
                        UseShellExecute = false,
                    };

            // Start the process
            return Process.Start( startup );
        }

        /// <summary>
        /// Liest oder setzt alle bereits zugeordneten Befehle.
        /// </summary>
        [XmlElement( "Mapping" )]
        public InputMapping[] Mappings
        {
            get
            {
                // Report
                return m_Mappings.Keys.ToArray();
            }
            set
            {
                // Reset
                m_Mappings.Clear();

                // Fill in all
                if (value != null)
                    foreach (var item in value)
                        Add( item );
            }
        }

        /// <summary>
        /// Ermittelt ein Eingabeelement.
        /// </summary>
        /// <param name="index">Die Liste der Eingabeinformationen.</param>
        /// <returns>Das gewünschte Element.</returns>
        [XmlIgnore]
        public InputKey? this[params MappingItem[] index]
        {
            get
            {
                // Create the key
                var key = new InputMapping();

                // Add index
                if (index != null)
                    key.Items.AddRange( index );

                // Find it
                InputMapping existing;
                if (m_Mappings.TryGetValue( key, out existing ))
                    return existing.Meaning;
                else
                    return null;
            }
            set
            {
                // Create the key
                var key = new InputMapping { Meaning = value.GetValueOrDefault() };

                // Add index
                if (index != null)
                    key.Items.AddRange( index );

                // Check mode
                if (value.HasValue)
                    m_Mappings[key] = key;
                else
                    m_Mappings.Remove( key );
            }
        }

        /// <summary>
        /// Rekonstruiert eine Konfiguration aus der aktuellen Konfigurationsdatei.
        /// </summary>
        /// <returns>Die gewünschte Rekonstruktion.</returns>
        public static RCSettings Load()
        {
            // Forward
            return Load( ConfigurationFile.FullName );
        }

        /// <summary>
        /// Rekonstruiert eine Konfiguration aus einer Konfigurationsdatei.
        /// </summary>
        /// <param name="path">Der volle Pfad zur gewünschten Datei.</param>
        /// <returns>Die gewünschte Rekonstruktion.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Datei angegeben.</exception>
        public static RCSettings Load( string path )
        {
            // Validate
            if (string.IsNullOrEmpty( path ))
                throw new ArgumentNullException( "path" );

            // Open the file and do it
            using (var stream = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.Read ))
            {
                // Load
                return (RCSettings) s_Serializer.Deserialize( stream );
            }
        }

        /// <summary>
        /// Rekonstruiert eine Konfiguration aus der aktuellen Konfigurationsdatei
        /// oder meldet eine neue Instanz.
        /// </summary>
        /// <returns>Die gewünschte Rekonstruktion.</returns>
        public static RCSettings LoadOrDefault()
        {
            // Check mode
            if (ConfigurationFile.Exists)
                return Load();
            else
                return new RCSettings();
        }

        /// <summary>
        /// Erstellt eine Textrepräsentation.
        /// </summary>
        /// <returns>Die Textrepräsentation.</returns>
        public override string ToString()
        {
            // Create helper
            using (var stream = new MemoryStream())
            {
                // Fill in
                Save( stream );

                // Report
                return Encoding.Unicode.GetString( stream.ToArray() );
            }
        }

        /// <summary>
        /// Speichert die Konfiguration in der aktuellen Konfigurationsdatei.
        /// </summary>
        public void Save()
        {
            // Open the file and process
            using (var stream = ConfigurationFile.Create())
                Save( stream );
        }

        /// <summary>
        /// Speichert die Konfigurationsdatei.
        /// </summary>
        /// <param name="path">Der Pfad zur Zieldatei.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Pfad angegeben.</exception>
        public void Save( string path )
        {
            // Validate
            if (string.IsNullOrEmpty( path ))
                throw new ArgumentNullException( "path" );

            // Open the file and process
            using (var stream = new FileStream( path, FileMode.Create, FileAccess.Write, FileShare.None ))
                Save( stream );
        }

        /// <summary>
        /// Speichert die Konfiguration in einem Datenstrom.
        /// </summary>
        /// <param name="stream">Der gewünschte Datenstrom.</param>
        private void Save( Stream stream )
        {
            // Configure
            var settings = new XmlWriterSettings { Encoding = Encoding.Unicode, Indent = true };

            // Open the file and process
            using (var writer = XmlWriter.Create( stream, settings ))
                s_Serializer.Serialize( writer, this );
        }
    }
}
