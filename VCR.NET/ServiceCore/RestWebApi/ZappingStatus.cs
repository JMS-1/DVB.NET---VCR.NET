using JMS.DVB;
using JMS.DVB.CardServer;
using System;
using System.Linq;
using System.Runtime.Serialization;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Beschreibt den aktuellen Zustand.
    /// </summary>
    [Serializable]
    [DataContract]
    public class ZappingStatus
    {
        /// <summary>
        /// Eine leere Liste von Diensten.
        /// </summary>
        private static readonly ZappingService[] s_NoServices = { };

        /// <summary>
        /// Das aktuelle Ziel der Nutzdaten.
        /// </summary>
        [DataMember( Name = "target" )]
        public string Target { get; set; }

        /// <summary>
        /// Die aktuelle Quelle.
        /// </summary>
        [DataMember( Name = "source" )]
        public string Source { get; set; }

        /// <summary>
        /// Alle auf der aktuellen Quellgruppe verfügbaren Dienste.
        /// </summary>
        [DataMember( Name = "services" )]
        public ZappingService[] Services { get; set; }

        /// <summary>
        /// Erstellt einen neuen Zustand.
        /// </summary>
        /// <param name="target">Das aktuelle Ziel des Datenversands.</param>
        /// <param name="server">Die zugehörigen Informationen des Aufzeichnungsprozesses.</param>
        /// <returns>Der gwünschte Zustand.</returns>
        public static ZappingStatus Create( string target, ServerInformation server )
        {
            // Create new
            var status = new ZappingStatus { Target = target, Services = s_NoServices };

            // No state 
            if (server == null)
                return status;

            // Attach to the first stream
            var streams = server.Streams;
            if (streams != null)
                if (streams.Count > 0)
                    status.Source = SourceIdentifier.ToString( streams[0].Source ).Replace( " ", "" );

            // Fill in NVOD services in the standard index order
            var services = server.Services;
            if (services != null)
                status.Services =
                    services
                        .Where( service => service != null )
                        .Select( ZappingService.Create )
                        .OrderBy( service => service.Index )
                        .ToArray();

            // Report
            return status;
        }
    }
}
