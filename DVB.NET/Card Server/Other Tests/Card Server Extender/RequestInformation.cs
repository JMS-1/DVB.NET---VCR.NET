using System;
using JMS.DVB;
using System.Linq;
using JMS.DVB.CardServer;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Card_Server_Extender
{
    /// <summary>
    /// Die Art des Aufrufs.
    /// </summary>
    public enum InformationRequestType
    {
        /// <summary>
        /// Alle Quellgruppe des aktuell gewählten Ursprungs.
        /// </summary>
        NetworkInformation,

        /// <summary>
        /// Alle Quellen der aktuellen Quellgruppe.
        /// </summary>
        GroupInformation,

        /// <summary>
        /// Informationen zu einer oder mehreren Quellen.
        /// </summary>
        SourceInformation
    }

    /// <summary>
    /// Beschreibt eine Anfrage.
    /// </summary>
    [Serializable]
    public class InformationRequest
    {
        /// <summary>
        /// Die Art der Anfrage.
        /// </summary>
        public InformationRequestType Type { get; set; }

        /// <summary>
        /// Die Liste der Quellen.
        /// </summary>
        public SourceIdentifier[] SourceList { get; set; }

        /// <summary>
        /// Erzeugt eine neue Anfrage.
        /// </summary>
        public InformationRequest()
        {
        }
    }

    /// <summary>
    /// Die Antwort auf eine Anfrage.
    /// </summary>
    [Serializable]
    [XmlInclude( typeof( GenericInformationResponse ) )]
    [XmlInclude( typeof( NetworkInformationResponse ) )]
    public abstract class InformationResponse
    {
        /// <summary>
        /// Initialisiert eine Antwort.
        /// </summary>
        protected InformationResponse()
        {
        }
    }

    /// <summary>
    /// Enthält alle Quellgruppen des aktuellen Ursprungs.
    /// </summary>
    [Serializable]
    public class NetworkInformationResponse : InformationResponse
    {
        /// <summary>
        /// Alle Quellgruppen des Ursprungs.
        /// </summary>
        [XmlArrayItem( typeof( SatelliteGroup ) )]
        [XmlArrayItem( typeof( CableGroup ) )]
        [XmlArrayItem( typeof( TerrestrialGroup ) )]
        public readonly List<SourceGroup> Groups = new List<SourceGroup>();

        /// <summary>
        /// Erzeugt eine neue Antwort.
        /// </summary>
        public NetworkInformationResponse()
        {
        }
    }

    /// <summary>
    /// Während der Umstellungsphase...
    /// </summary>
    [Serializable]
    public class GenericInformationResponse : InformationResponse
    {
        /// <summary>
        /// Die Antwortdaten in Rohform.
        /// </summary>
        public string[] Strings { get; set; }

        /// <summary>
        /// Erzeugt eine generische Information.
        /// </summary>
        public GenericInformationResponse()
        {
        }
    }

    /// <summary>
    /// Ruft Informationen zur aktuellen Umgebung ab.
    /// </summary>
    public class RequestInformation : CustomAction<InformationRequest, InformationResponse>
    {
        /// <summary>
        /// Erstellt eine neue Erweiterung.
        /// </summary>
        /// <param name="cardServer">Die zugehörige Instanz des <i>Card Servers</i> - auf
        /// der Client Seite.</param>
        public RequestInformation( ServerImplementation cardServer )
            : base( cardServer )
        {
        }

        /// <summary>
        /// Führt eine generische Anfrage aus.
        /// </summary>
        /// <param name="request">Die Anfrage.</param>
        /// <returns>Das Ergebnis der Anfrage.</returns>
        private new IAsyncResult<InformationResponse> BeginExecute( InformationRequest request )
        {
            // Dismiss
            throw new NotSupportedException();
        }

        /// <summary>
        /// Ermittelt alle Quellgruppen des aktuellen Ursprungs.
        /// </summary>
        /// <returns>Die Steuereinheit für diesen Aufruf.</returns>
        public IAsyncResult<InformationResponse> BeginGetNetworkInformation()
        {
            // Forward
            return base.BeginExecute( new InformationRequest { Type = InformationRequestType.NetworkInformation } );
        }

        /// <summary>
        /// Ermittelt alle Quellen der aktuellen Quellgruppe.
        /// </summary>
        /// <returns>Die Steuereinheit für diesen Aufruf.</returns>
        public IAsyncResult<InformationResponse> BeginGetGroupInformation()
        {
            // Forward
            return base.BeginExecute( new InformationRequest { Type = InformationRequestType.GroupInformation } );
        }

        /// <summary>
        /// Ermittelt Informationen zu den Quellen einer Quellgruppe.
        /// </summary>
        /// <returns>Die Steuereinheit für diesen Aufruf.</returns>
        public IAsyncResult<InformationResponse> BeginGetSourceInformation( IEnumerable<SourceIdentifier> sources )
        {
            // Forward
            return base.BeginExecute( new InformationRequest { Type = InformationRequestType.SourceInformation, SourceList = sources.ToArray() } );
        }

        /// <summary>
        /// Führt die zugehörige Operation aus.
        /// </summary>
        /// <param name="request">Optionale Parameter für den Aufruf.</param>
        /// <returns>Das Ergebnis der Operation.</returns>
        protected override InformationResponse OnExecute( InformationRequest request )
        {
            // Check mode
            if (request.Type == InformationRequestType.NetworkInformation)
            {
                // Load it
                var locationInformation = Device.GetLocationInformation();

                // Create result
                var response = new NetworkInformationResponse();

                // Fill it
                if (locationInformation != null)
                    if (locationInformation.Groups != null)
                        response.Groups.AddRange( locationInformation.Groups.Cast<SourceGroup>() );

                // Done
                return response;
            }

            // Result
            var result = new List<string>();

            // Check mode
            if (request.Type == InformationRequestType.GroupInformation)
            {
                // Load it
                var groupInformation = Device.GetGroupInformation();

                // Check mode
                if (groupInformation != null)
                    result.AddRange( groupInformation.Sources.Select( s => SourceIdentifier.ToString( s ) ) );
            }
            else if (request.Type == InformationRequestType.SourceInformation)
                foreach (var source in request.SourceList)
                {
                    // Get the information
                    var sourceInformation = source.GetSourceInformation( Device, CardServer.Profile );

                    // Report
                    if (sourceInformation == null)
                        result.Add( source.ToString() );
                    else
                        result.Add( sourceInformation.Name );
                }

            // Not supported
            return new GenericInformationResponse { Strings = result.ToArray() };
        }
    }
}
