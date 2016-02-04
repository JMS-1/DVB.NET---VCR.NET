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
    public class ZappingSource : SourceInformation<ZappingSource>
    {
        /// <summary>
        /// Die eindeutige Kennung der Quelle.
        /// </summary>
        [DataMember( Name = "source" )]
        public string Source { get; set; }

        /// <summary>
        /// Führt individuelle Initialisierungen aus.
        /// </summary>
        /// <param name="station">Die Informationen zur Quelle.</param>
        protected override void OnCreate( Station station ) => Source = SourceIdentifier.ToString( station ).Replace( " ", "" );
    }
}
