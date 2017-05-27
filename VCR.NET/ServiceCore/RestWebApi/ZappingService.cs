using JMS.DVB;
using JMS.DVB.CardServer;
using System;
using System.Runtime.Serialization;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Beschreibt einen Dienst.
    /// </summary>
    [Serializable]
    [DataContract]
    public class ZappingService
    {
        /// <summary>
        /// Der Name des Dienstes.
        /// </summary>
        [DataMember( Name = "nameWithIndex" )]
        public string Name { get; set; }

        /// <summary>
        /// Die eindeutige Kennung.
        /// </summary>
        [DataMember( Name = "source" )]
        public string Source { get; set; }

        /// <summary>
        /// Die laufende Nummer dieses Dienstes.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Ermittelt die laufende Nummer eines Dienstes.
        /// </summary>
        /// <param name="uniqueName">Ein Dienst.</param>
        /// <returns>Die laufende Nummer des Dienstes.</returns>
        private static int GetServiceIndex( string uniqueName )
        {
            // Check name
            if (string.IsNullOrEmpty( uniqueName ))
                return -1;

            // Split
            int i = uniqueName.IndexOf( ',' );
            if (i < 0)
                return -1;

            // Get the part
            if (uint.TryParse(uniqueName.Substring(0, i), out uint result))
                if (result < int.MaxValue)
                    return (int)result;
                else
                    return -1;
            else
                return -1;
        }

        /// <summary>
        /// Erstellt eine neue Dienstbeschreibung.
        /// </summary>
        /// <param name="service">Die Beschreibung des Dienstes.</param>
        /// <returns>Der gewünschte Dienst.</returns>
        public static ZappingService Create( ServiceInformation service )
        {
            // Validate
            if (service == null)
                throw new ArgumentNullException( nameof( service ) );

            // Create new
            return
                new ZappingService
                {
                    Source = SourceIdentifier.ToString( service.Service ).Replace( " ", "" ),
                    Index = GetServiceIndex( service.UniqueName ),
                    Name = service.UniqueName
                };
        }
    }
}
