using System;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Enthält alle Daten zum Empfang einer Quelle.
    /// </summary>
    [Serializable]
    public class StreamInformation
    {
        /// <summary>
        /// Die eindeutige Kennung der Quelle.
        /// </summary>
        public SourceIdentifier Source { get; set; }

        /// <summary>
        /// Die eindeutige Kennung der Quelle, so wie sie beim Start angegeben wurde.
        /// </summary>
        public Guid UniqueIdentifier { get; set; }

        /// <summary>
        /// In für den Empfang berücksichtigten Aspekte.
        /// </summary>
        public StreamSelection Streams { get; set; }

        /// <summary>
        /// Meldet oder legt fest, ob für diese Quelle eine Entschlüsselung aktiv ist.
        /// </summary>
        public bool IsDecrypting { get; set; }

        /// <summary>
        /// Der vollständige Pfad zur Aufzeichnungdatei oder <i>null</i>, wenn keine Datei
        /// angelegt wird.
        /// </summary>
        public string TargetPath { get; set; }

        /// <summary>
        /// Die Netzwerkadresse, an den die Aufzeichnungsdaten verschickt werden.
        /// </summary>
        public string StreamTarget { get; set; }

        /// <summary>
        /// Die Anzahl der Teildatentströme (PID), die für den Empfang dieser Quelle benötigt werden.
        /// </summary>
        public int ConsumerCount { get; set; }

        /// <summary>
        /// Die gesamte Anzahl von Bytes, die bisher empfangen wurden.
        /// </summary>
        public long BytesReceived { get; set; }

        /// <summary>
        /// Die im aktuellen Datenstrom empfangenen Bild- und Tondaten.
        /// </summary>
        public long? CurrentAudioVideoBytes { get; set; }

        /// <summary>
        /// Alle Dateien, die für diese Quelle angelegt wurden.
        /// </summary>
        public FileStreamInformation[] AllFiles { get; set; }

        /// <summary>
        /// Erzeugt eine neue Informationsinstanz.
        /// </summary>
        public StreamInformation()
        {
        }
    }


}
