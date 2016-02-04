using JMS.DVB;
using System;
using System.Runtime.Serialization;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Beschreibt eine mögliche Datenquelle.
    /// </summary>
    /// <typeparam name="TReal">Die konkrete Art der Klasse.</typeparam>
    [Serializable]
    [DataContract]
    public abstract class SourceInformation<TReal> where TReal : SourceInformation<TReal>, new()
    {
        /// <summary>
        /// Der Anzeigename der Quelle.
        /// </summary>
        [DataMember( Name = "nameWithProvider" )]
        public string Name { get; set; }

        /// <summary>
        /// Gesetzt, wenn die Quelle verschlüsselt ist.
        /// </summary>
        [DataMember( Name = "encrypted" )]
        public bool IsEncrypted { get; set; }

        /// <summary>
        /// Führt individuelle Initialisierungen aus.
        /// </summary>
        /// <param name="station">Die Informationen zur Quelle.</param>
        protected abstract void OnCreate( Station station );

        /// <summary>
        /// Erstellt eine alternative Repräsentation einer Quelle.
        /// </summary>
        /// <param name="source">Die volle Beschreibung der Quelle.</param>
        /// <returns>Das Transferformat.</returns>
        public static TReal Create( SourceSelection source )
        {
            // Attach to the station
            var station = (Station) source.Source;

            // Construct
            var info = new TReal
                {
                    IsEncrypted = station.IsEncrypted || station.IsService,
                    Name = source.GetUniqueName(),
                };

            // Finish setup
            info.OnCreate( station );

            // Report
            return info;
        }
    }
}
