using System;
using System.Collections.Generic;

namespace JMS.DVB
{
    /// <summary>
    /// Enthält alle Laufzeitinformationen zu einer aktiven Quelle.
    /// </summary>
    [Serializable]
    public class SourceInformation
    {
        /// <summary>
        /// Die zugehörige Quelle, im Allgemeinen eine <see cref="Station"/> Instanz.
        /// </summary>
        public SourceIdentifier Source { get; set; }

        /// <summary>
        /// Meldet oder legt fest, ob die Quelle verschlüsselt sendet.
        /// </summary>
        public bool IsEncrypted { get; set; }

        /// <summary>
        /// Meldet oder legt fest, ob es sich um einen dynamischen Dienst handelt.
        /// </summary>
        public bool IsService { get; set; }

        /// <summary>
        /// Die Datenstromkennung (PID) des Bildsignals.
        /// </summary>
        public ushort VideoStream { get; set; }

        /// <summary>
        /// Liest oder setzt die Datenstromkennung (PID) des Videotextsignals.
        /// </summary>
        public ushort TextStream { get; set; }

        /// <summary>
        /// Liest oder setzt die Art des Bildsignals.
        /// </summary>
        public VideoTypes VideoType { get; set; }

        /// <summary>
        /// Die Liste aller Tonspuren, die von dieser Quelle angeboten werden.
        /// </summary>
        public readonly List<AudioInformation> AudioTracks = new List<AudioInformation>();

        /// <summary>
        /// Die Liste aller Untertitelspuren zu dieser Quelle.
        /// </summary>
        public readonly List<SubtitleInformation> Subtitles = new List<SubtitleInformation>();

        /// <summary>
        /// Der Name des Dienstanbieters.
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Der Name dieser Quelle.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Erzeugt eine neue Laufzeitinformation.
        /// </summary>
        public SourceInformation()
        {
        }

        /// <summary>
        /// Ermittelt die primäre MP2 Tonspur.
        /// </summary>
        public AudioInformation PrimaryMP2Audio
        {
            get
            {
                // Lookup
                return AudioTracks.Find( a => AudioTypes.MP2 == a.AudioType );
            }
        }

        /// <summary>
        /// Ermittelt die primäre AC3 Tonspur.
        /// </summary>
        public AudioInformation PrimaryAC3Audio
        {
            get
            {
                // Lookup
                return AudioTracks.Find( a => AudioTypes.AC3 == a.AudioType );
            }
        }
    }
}
