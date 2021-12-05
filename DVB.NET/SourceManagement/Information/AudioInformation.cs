using System;

namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt eine einzelne Tonspur innerhalb der Daten eines Senders.
    /// </summary>
    [Serializable]
    public class AudioInformation : ICloneable
    {
        /// <summary>
        /// Liest oder setzt die Art der Nutzdaten der Tonspur.
        /// </summary>
        public AudioTypes AudioType { get; set; }

        /// <summary>
        /// Liest oder setzt die Datenstromkennung (PID) der Tonspur.
        /// </summary>
        public ushort AudioStream { get; set; }

        /// <summary>
        /// Liest oder setzt den Namen der Sprache der Tonspur.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Liest´oder setzt die AAC Informationen zur Tonspur.
        /// </summary>
        public ushort? AAC { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public AudioInformation()
        {
        }

        /// <summary>
        /// Erzeugt einen Anzeigetext für diese Tonspur.
        /// </summary>
        /// <returns>Ein Anzeigetext gemäß der aktuellen Konfiguration.</returns>
        public override string ToString()
        {
            // Report
            if (AudioType == AudioTypes.AC3)
                return string.Format("{0} (AC3) [{1}]", Language, AudioStream);
            else if (AAC.HasValue)
                return string.Format("{0} (AAC {2:X4}) [{1}]", Language, AudioStream, AAC.Value);
            else
                return string.Format("{0} [{1}]", Language, AudioStream);
        }

        #region ICloneable Members

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Information.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        public AudioInformation Clone() => new AudioInformation
        {
            AAC = AAC,
            AudioStream = AudioStream,
            AudioType = AudioType,
            Language = Language,
        };

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Information.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        object ICloneable.Clone() => Clone();

        #endregion
    }
}
