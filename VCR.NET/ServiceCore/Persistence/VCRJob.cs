using JMS.DVB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;


namespace JMS.DVBVCR.RecordingService.Persistence
{
    /// <summary>
    /// Beschreibt einen Auftrag.
    /// </summary>
    /// <remarks>
    /// Ein Auftrag enthält zumindest eine Aufzeichnung.
    /// </remarks>
    [Serializable]
    public class VCRJob
    {
        /// <summary>
        /// Der spezielle Name für die Aktualisierung der Quellen eines Geräteprofils.
        /// </summary>
        public const string SourceScanName = "PSI";

        /// <summary>
        /// Der spezielle Name für die Aktualisierung der Programmzeitschrift.
        /// </summary>
        public const string ProgramGuideName = "EPG";

        /// <summary>
        /// Der spezielle Name für den LIVE Modus, der von <i>Zapping Clients</i> wie
        /// dem DVB.NET / VCR.NET Viewer verwendet werden.
        /// </summary>
        public const string ZappingName = "LIVE";

        /// <summary>
        /// Dateiendung für Aufträge im XML Serialisierungsformat.
        /// </summary>
        public const string FileSuffix = ".j39";

        /// <summary>
        /// Aufzeichnungen zu diesem Auftrag.
        /// </summary>        
        [XmlElement( "Schedule" )]
        public readonly List<VCRSchedule> Schedules = new List<VCRSchedule>();

        /// <summary>
        /// Verzeichnis, in dem Aufzeichnungsdateien abgelegt werden sollen.
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Eindeutige Kennung des Auftrags.
        /// </summary>
        public Guid? UniqueID { get; set; }

        /// <summary>
        /// Die gewünschte Quelle.
        /// </summary>
        public SourceSelection Source { get; set; }

        /// <summary>
        /// Die Datenströme, die aufgezeichnet werden sollen.
        /// </summary>
        public StreamSelection Streams { get; set; }

        /// <summary>
        /// Name des Auftrags.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gesetzt, wenn es das Gerät zur Aufzeichnung automatisch ausgewählt werden darf.
        /// </summary>
        public bool AutomaticResourceSelection { get; set; }

        /// <summary>
        /// Speichert diesen Auftrag ab.
        /// </summary>
        /// <param name="target">Der Pfad zu einem Zielverzeichnis.</param>
        /// <returns>Gesetzt, wenn der Speichervorgang erfolgreich war. <i>null</i> wird
        /// gemeldet, wenn diesem Auftrag keine Datei zugeordnet ist.</returns>
        public bool? Save( DirectoryInfo target )
        {
            // Get the file
            var file = GetFileName( target );
            if (file == null)
                return null;

            // Be safe
            try
            {
                // Process
                SerializationTools.Save( this, file );
            }
            catch (Exception e)
            {
                // Report
                VCRServer.Log( e );

                // Done
                return false;
            }

            // Done
            return true;
        }

        /// <summary>
        /// Löschte diesen Auftrag.
        /// </summary>
        /// <param name="target">Der Pfad zu einem Zielverzeichnis.</param>
        /// <returns>Gesetzt, wenn der Löschvorgang erfolgreich war. <i>null</i> wird gemeldet,
        /// wenn die Datei nicht existierte.</returns>
        public bool? Delete( DirectoryInfo target )
        {
            // Get the file
            var file = GetFileName( target );
            if (file == null)
                return null;
            if (!file.Exists)
                return null;

            // Be safe
            try
            {
                // Process
                file.Delete();
            }
            catch (Exception e)
            {
                // Report error
                VCRServer.Log( e );

                // Failed
                return false;
            }

            // Did it
            return true;
        }

        /// <summary>
        /// Ermittelt den Namen dieses Auftrags in einem Zielverzeichnis.
        /// </summary>
        /// <param name="target">Der Pfad zu einem Zielverzeichnis.</param>
        /// <returns>Die zugehörige Datei.</returns>
        private FileInfo GetFileName( DirectoryInfo target ) => UniqueID.HasValue ? new FileInfo( Path.Combine( target.FullName, UniqueID.Value.ToString( "N" ).ToUpper() + FileSuffix ) ) : null;

        /// <summary>
        /// Ermittelt alle Aufträge in einem Verzeichnis.
        /// </summary>
        /// <param name="directory">Das zu bearbeitende Verzeichnis.</param>
        /// <returns>Alle Aufträge.</returns>
        public static IEnumerable<VCRJob> Load( DirectoryInfo directory )
        {
            // Process
            return
                directory
                    .GetFiles( "*" + FileSuffix )
                    .Select( SerializationTools.Load<VCRJob> )
                    .Where( job => job != null );
        }

        /// <summary>
        /// Prüft, ob dieser Auftrag noch einmal verwendet wird. Das ist der Fall, wenn mindestens
        /// eine Aufzeichnung noch vorhanden ist.
        /// </summary>
        [XmlIgnore]
        public bool IsActive => Schedules.Any( schedule => schedule.IsActive );

        /// <summary>
        /// Prüft, ob ein Auftrag zulässig ist.
        /// </summary>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der veränderten Aufzeichnung.</param>
        /// <exception cref="InvalidJobDataException">Die Konfiguration dieses Auftrags is ungültig.</exception>
        public void Validate( Guid? scheduleIdentifier )
        {
            // Identifier
            if (!UniqueID.HasValue)
                throw new InvalidJobDataException( Properties.Resources.BadUniqueID );

            // Name
            if (!Name.IsValidName())
                throw new InvalidJobDataException( Properties.Resources.BadName );

            // Test the station
            if (HasSource)
            {
                // Source
                if (!Source.Validate())
                    throw new InvalidJobDataException( Properties.Resources.BadTVStation );

                // Streams
                if (!Streams.Validate())
                    throw new InvalidJobDataException( Properties.Resources.BadStreams );
            }
            else if (Streams != null)
                throw new InvalidJobDataException( Properties.Resources.BadStreams );

            // List of schedules
            if (Schedules.Count < 1)
                throw new InvalidJobDataException( Properties.Resources.NoSchedules );

            // Only validate the schedule we updated
            if (scheduleIdentifier.HasValue)
                foreach (var schedule in Schedules)
                    if (!schedule.UniqueID.HasValue || schedule.UniqueID.Value.Equals( scheduleIdentifier ))
                        schedule.Validate( this );
        }

        /// <summary>
        /// Meldet, ob diesem Auftrag eine Quelle zugeordnet ist.
        /// </summary>
        [XmlIgnore]
        public bool HasSource => (Source != null) && (Source.Source != null);

        /// <summary>
        /// Ermittelt eine Aufzeichnung dieses Auftrags.
        /// </summary>
        /// <param name="uniqueIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
        /// <returns>Die Aufzeichnung oder <i>null</i>.</returns>
        public VCRSchedule this[Guid uniqueIdentifier] => Schedules.Find( s => s.UniqueID.HasValue && (s.UniqueID.Value == uniqueIdentifier) );

        /// <summary>
        /// Entfernt alle Ausnahmeregelungen, die bereits verstrichen sind.
        /// </summary>
        public void CleanupExceptions() => Schedules.ForEach( schedule => schedule.CleanupExceptions() );

        /// <summary>
        /// Stellt sicher, dass für diesen Auftrag ein Geräteprprofil ausgewählt ist.
        /// </summary>
        internal void SetProfile()
        {
            // No need
            if (!string.IsNullOrEmpty( Source?.ProfileName ))
                return;

            // Attach to the default profile
            var defaultProfile = VCRProfiles.DefaultProfile;
            if (defaultProfile == null)
                return;

            // Process
            if (Source == null)
                Source = new SourceSelection { ProfileName = defaultProfile.Name };
            else
                Source.ProfileName = defaultProfile.Name;
        }

        /// <summary>
        /// Stellt sicher, dass für diesen Auftrag ein Geräteprprofil ausgewählt ist.
        /// </summary>
        /// <param name="defaultProfileName">Der Name des bevorzugten Geräteprofils.</param>
        internal void SetProfile( string defaultProfileName )
        {
            // No source at all
            if (Source == null)
                Source = new SourceSelection { ProfileName = defaultProfileName };
            else if (string.IsNullOrEmpty( Source.ProfileName ))
                Source.ProfileName = defaultProfileName;
        }
    }

    /// <summary>
    /// Hilfsmethoden zur Validierung von Aufträgen und Aufzeichnungen.
    /// </summary>
    public static class ValidationExtension
    {
        /// <summary>
        /// Alle in Dateinamen nicht erlaubten Zeichen.
        /// </summary>
        private static readonly char[] m_BadCharacters = Path.GetInvalidPathChars().Union( Path.GetInvalidFileNameChars() ).Distinct().ToArray();

        /// <summary>
        /// Prüft, ob eine Quelle gültig ist.
        /// </summary>
        /// <param name="source">Die Auswahl der Quelle oder <i>null</i>.</param>
        /// <returns>Gesetzt, wenn die Auswahl gültig ist.</returns>
        public static bool Validate( this SourceSelection source ) => (VCRProfiles.FindSource( source ) != null);

        /// <summary>
        /// Prüft, ob eine Datenstromauswahl zulässig ist.
        /// </summary>
        /// <param name="streams">Die Auswahl der Datenströme.</param>
        /// <returns>Gesetzt, wenn die Auswahl gültig ist - und mindestens eine Tonspur enthält.</returns>
        public static bool Validate( this StreamSelection streams )
        {
            // Not possible
            if (streams == null)
                return false;

            // Test for wildcards - may fail at runtime!
            if (streams.MP2Tracks.LanguageMode != LanguageModes.Selection)
                return true;
            if (streams.AC3Tracks.LanguageMode != LanguageModes.Selection)
                return true;

            // Test for language selection - may fail at runtime but at least we tried
            if (streams.MP2Tracks.Languages.Count > 0)
                return true;
            if (streams.AC3Tracks.Languages.Count > 0)
                return true;

            // Will definitly fail
            return false;
        }

        /// <summary>
        /// Prüft, ob eine Zeichenkette als Name für einen Auftrag oder eine
        /// Aufzeichnung verwendet werden darf.
        /// </summary>
        /// <param name="name">Der zu prüfenden Name.</param>
        /// <returns>Gesetzt, wenn der Name verwendet werden darf.</returns>
        public static bool IsValidName( this string name ) => string.IsNullOrEmpty( name ) || (name.IndexOfAny( m_BadCharacters ) < 0);

        /// <summary>
        /// Ersetzt alle Zeichen, die nicht in Dateinamen erlaubt sind, durch einen
        /// Unterstrich.
        /// </summary>
        /// <param name="s">Eine Zeichenkette.</param>
        /// <returns>Die korrigierte Zeichenkette.</returns>
        public static string MakeValid( this string s )
        {
            // No at all
            if (string.IsNullOrEmpty( s ))
                return string.Empty;

            // Correct all
            if (s.IndexOfAny( m_BadCharacters ) >= 0)
                foreach (var ch in m_BadCharacters)
                    s = s.Replace( ch, '_' );

            // Report
            return s;
        }
    }
}
