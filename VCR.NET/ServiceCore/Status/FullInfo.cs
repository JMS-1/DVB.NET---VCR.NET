using JMS.DVBVCR.RecordingService.Persistence;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace JMS.DVBVCR.RecordingService.Status
{
    /// <summary>
    /// Beschreibt einen Gesamtauftrag.
    /// </summary>
    [Serializable]
    public class FullInfo
    {
        /// <summary>
        /// Alle Quellen zu dieser Aufzeichnung
        /// </summary>
        [XmlElement( "Stream" )]
        public readonly List<StreamInfo> Streams = new List<StreamInfo>();

        /// <summary>
        /// Die Daten der primären Aufzeichnung.
        /// </summary>
        public VCRRecordingInfo Recording { get; set; }

        /// <summary>
        /// Meldet oder legt fest, on ein Netzwerkversand unterstützt wird.
        /// </summary>
        public bool CanStream { get; set; }

        /// <summary>
        /// Gesetzt, wenn es sich um einen Auftrag handelt, bei dem Aufzeichnungen dynamisch
        /// ergänzt und entfernt werden können.
        /// </summary>
        public bool IsDynamic { get; set; }

        /// <summary>
        /// Der Zeitpunkt, zu dem der Gesamtauftrag enden soll.
        /// </summary>
        public DateTime EndsAt { get; set; }
    }
}
