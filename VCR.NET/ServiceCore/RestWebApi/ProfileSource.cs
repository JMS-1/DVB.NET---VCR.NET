using JMS.DVB;
using System;
using System.Runtime.Serialization;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Beschreibt eine mögliche Datenquelle.
    /// </summary>
    [Serializable]
    [DataContract]
    public class ProfileSource : SourceInformation<ProfileSource>
    {
        /// <summary>
        /// Gesetzt, wenn es sich um einen Fernsehsender handelt.
        /// </summary>
        [DataMember( Name = "tvNotRadio" )]
        public bool IsTVStation { get; set; }

        /// <summary>
        /// Führt individuelle Initialisierungen aus.
        /// </summary>
        /// <param name="station">Die Informationen zur Quelle.</param>
        protected override void OnCreate( Station station ) => IsTVStation = (station.SourceType != SourceTypes.Radio);
    }
}
