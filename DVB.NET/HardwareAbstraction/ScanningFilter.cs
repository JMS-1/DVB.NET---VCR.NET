using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections;
using System.IO;

namespace JMS.DVB
{
    /// <summary>
    /// Instanzen dieser Klasse beschreiben Sonderwünsche bezüglich des Sendersuchlaufs 
    /// über eine Quellgruppe (Transponder).
    /// </summary>
    public class SourceGroupFilter
    {
        /// <summary>
        /// Meldet oder legt fest, ob diese Quellgruppe vom Suchlauf ausgeschlossen werden soll.
        /// </summary>
        public bool ExcludeFromScan { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public SourceGroupFilter()
        {
        }
    }

    /// <summary>
    /// Beschreibt, in welcher Form eine Quelle beim Sendersuchlauf modifiziert werden soll.
    /// </summary>
    [Serializable]
    [XmlType( "Modifier" )]
    public class SourceModifier : SourceIdentifier, ICloneable
    {
        /// <summary>
        /// Gesetzt, wenn diese Quelle nicht berücksichtigt werden soll.
        /// </summary>
        public bool ExcludeFromScan { get; set; }

        /// <summary>
        /// Optional ein fester Name für die Quelle.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional der Name eines festen Anbieters für die Quelle.
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Kann verwendet werden um festzulegen, ob die Quelle verschlüsselt gesendet wird.
        /// </summary>
        public bool? IsEncrypted { get; set; }

        /// <summary>
        /// Kann verwendet werden um festzulegen, ob es sich bei einer Quelle um
        /// einen NVOD Dienst handelt.
        /// </summary>
        public bool? IsService { get; set; }

        /// <summary>
        /// Erlaubt es optional, die Art der Quelle zu verändern - nicht empfohlen!
        /// </summary>
        public SourceTypes? SourceType { get; set; }

        /// <summary>
        /// Die fixierte Datenstromkennung (PID) des Bildsignals.
        /// </summary>
        public ushort? VideoStream { get; set; }

        /// <summary>
        /// Liest oder setzt die fixierte Datenstromkennung (PID) des Videotextsignals.
        /// </summary>
        public ushort? TextStream { get; set; }

        /// <summary>
        /// Liest oder setzt die fixierte Art des Bildsignals.
        /// </summary>
        public VideoTypes? VideoType { get; set; }

        /// <summary>
        /// Meldet oder legt fest, ob die Programmzeitschrift für diese Quelle deaktiviert
        /// werden soll.
        /// </summary>
        public bool DisableProgramGuide { get; set; }

        /// <summary>
        /// Liest oder setzt fixierte Tonspuren - es können immer nur alle Tonspuren auf einmal
        /// fixiert werden.
        /// </summary>
        [XmlArrayItem( "Stream" )]
        public AudioInformation[] AudioStreams { get; set; }

        /// <summary>
        /// Liest oder setzt fixierte DVB Untertitel - es können immer nur alle Untertitel auf einmal
        /// fixiert werden.
        /// </summary>
        [XmlArrayItem( "Stream" )]
        public SubtitleInformation[] SubtitlesStreams { get; set; }

        /// <summary>
        /// Erzeugt eine neue Modifikationsbeschreibung.
        /// </summary>
        public SourceModifier()
        {
        }

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Einstellungen.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        public SourceModifier Clone()
        {
            // Create serializer
            XmlSerializer serializer = new XmlSerializer( GetType() );

            // Create stream
            using (MemoryStream stream = new MemoryStream())
            {
                // Create
                serializer.Serialize( stream, this );

                // Reset
                stream.Seek( 0, SeekOrigin.Begin );

                // Reload
                return (SourceModifier) serializer.Deserialize( stream );
            }
        }

        /// <summary>
        /// Verändert die Daten einer Quelle gemäß der Konfiguration dieser Instanz.
        /// </summary>
        /// <param name="station">Die zu verändernde Quelle.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
        public void ApplyTo( Station station )
        {
            // Validate
            if (null == station)
                throw new ArgumentNullException( "station" );

            // Blind apply - even if identifier would not match
            if (!string.IsNullOrEmpty( Name ))
                station.Name = Name;
            if (!string.IsNullOrEmpty( Provider ))
                station.Provider = Provider;
            if (IsEncrypted.HasValue)
                station.IsEncrypted = IsEncrypted.Value;
            if (IsService.HasValue)
                station.IsService = IsService.Value;
            if (SourceType.HasValue)
                station.SourceType = SourceType.Value;
        }

        /// <summary>
        /// Meldet, ob keine Veränderungen aktiv sind.
        /// </summary>
        public bool IsDefault
        {
            get
            {
                // Test all
                if (!string.IsNullOrEmpty( Name ))
                    return false;
                if (!string.IsNullOrEmpty( Provider ))
                    return false;
                if (IsEncrypted.HasValue)
                    return false;
                if (IsService.HasValue)
                    return false;
                if (SourceType.HasValue)
                    return false;
                if (VideoStream.HasValue)
                    return false;
                if (VideoType.HasValue)
                    return false;
                if (TextStream.HasValue)
                    return false;
                if (null != AudioStreams)
                    return false;
                if (null != SubtitlesStreams)
                    return false;
                if (DisableProgramGuide)
                    return false;

                // All on default
                return true;
            }
        }

        /// <summary>
        /// Verändert die Daten einer Quelle gemäß der Konfiguration dieser Instanz.
        /// </summary>
        /// <param name="source">Die zu verändernde Quelle.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
        public void ApplyTo( SourceInformation source )
        {
            // Validate
            if (null == source)
                throw new ArgumentNullException( "source" );

            // Blind apply - even if identifier would not match
            if (!string.IsNullOrEmpty( Name ))
                source.Name = Name;
            if (!string.IsNullOrEmpty( Provider ))
                source.Provider = Provider;
            if (IsEncrypted.HasValue)
                source.IsEncrypted = IsEncrypted.Value;
            if (IsService.HasValue)
                source.IsService = IsService.Value;
            if (VideoStream.HasValue)
                source.VideoStream = VideoStream.Value;
            if (VideoType.HasValue)
                source.VideoType = VideoType.Value;
            if (TextStream.HasValue)
                source.TextStream = TextStream.Value;

            // Audio
            if (null != AudioStreams)
            {
                // Wipe out
                source.AudioTracks.Clear();

                // Add all
                foreach (AudioInformation audio in AudioStreams)
                    source.AudioTracks.Add( audio.Clone() );
            }

            // Subtitles
            if (null != SubtitlesStreams)
            {
                // Wipe out
                source.Subtitles.Clear();

                // Add all
                foreach (SubtitleInformation sub in SubtitlesStreams)
                    source.Subtitles.Add( sub.Clone() );
            }
        }

        #region ICloneable Members

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Einstellungen.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        object ICloneable.Clone()
        {
            // Forward
            return Clone();
        }

        #endregion
    }

    /// <summary>
    /// Hilfsklasse für Feineinstellungen beim Sendersuchlauf.
    /// </summary>
    [Serializable]
    public abstract class ScanningFilter
    {
        /// <summary>
        /// Die Liste der Quellen, die beim Sendersuchlauf verändert werden.
        /// </summary>
        public readonly List<SourceModifier> SourceDetails = new List<SourceModifier>();

        /// <summary>
        /// Erzeugt eine neue Hilfsklasse.
        /// </summary>
        internal ScanningFilter()
        {
        }

        /// <summary>
        /// Ermittelt zu einer Quelle die Sonderwünsche für einen Sendersuchlauf.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Die vorzunehmenden Modifikationen.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
        public SourceModifier GetFilter( SourceIdentifier source )
        {
            // Validate
            if (null == source)
                throw new ArgumentNullException( "source" );

            // Find it
            SourceModifier filter = (null == SourceDetails) ? null : SourceDetails.Find( f => f.Equals( source ) );

            // Process
            if (null == filter)
                return new SourceModifier { Network = source.Network, TransportStream = source.TransportStream, Service = source.Service };
            else
                return filter;
        }

        /// <summary>
        /// Meldet alle Quellgruppen (Transponder), die von dem Suchlauf ausgeschlossen werden sollen
        /// </summary>
        [XmlIgnore]
        public abstract IList ExcludedSourceGroups { get; }

        /// <summary>
        /// Ermittelt die Einschränkungen des Sendersuchlaufs bezüglich einer Quellgruppe (Transponder).
        /// </summary>
        /// <param name="group">Die gewünschte Quellgruppe.</param>
        /// <returns>Informationen zur Quellgruppe.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Quellgruppe angegeben.</exception>
        public abstract SourceGroupFilter GetFilter( SourceGroup group );
    }

    /// <summary>
    /// Hilfsklasse für Feineinstellungen beim Sendersuchlauf.
    /// </summary>
    /// <typeparam name="T">Die Art der Quellgruppen (Transponder) zu diesen Feineinstellungen.</typeparam>
    [Serializable]
    public class ScanningFilter<T> : ScanningFilter where T : SourceGroup
    {
        /// <summary>
        /// Die Liste der Quellgruppen (Transponder), die vom Sendersuchlauf auszuschliessen sind.
        /// </summary>
        public readonly List<T> ExcludedGroups = new List<T>();

        /// <summary>
        /// Erzeugt eine neue Hilfsklasse.
        /// </summary>
        public ScanningFilter()
        {
        }

        /// <summary>
        /// Ermittelt die Einschränkungen des Sendersuchlaufs bezüglich einer Quellgruppe (Transponder).
        /// </summary>
        /// <param name="group">Die gewünschte Quellgruppe.</param>
        /// <returns>Informationen zur Quellgruppe.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Quellgruppe angegeben.</exception>
        public override SourceGroupFilter GetFilter( SourceGroup group )
        {
            // Validate
            if (null == group)
                throw new ArgumentNullException( "group" );

            // Check type
            T typedGroup = group as T;

            // Not us - never scan
            if (null == typedGroup)
                return new SourceGroupFilter { ExcludeFromScan = true };

            // Create result
            return
                new SourceGroupFilter
                    {
                        ExcludeFromScan = (null != ExcludedGroups) && ExcludedGroups.Contains( typedGroup )
                    };
        }

        /// <summary>
        /// Meldet alle Quellgruppen (Transponder), die von dem Suchlauf ausgeschlossen werden sollen
        /// </summary>
        [XmlIgnore]
        public override IList ExcludedSourceGroups
        {
            get
            {
                // Report
                return ExcludedGroups;
            }
        }
    }
}
