using System;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Ermittelt den aktuellen Arbeitszustand eines <i>Card Servers</i>.
    /// </summary>
    [Serializable]
    public class GetStateRequest : Request<Response<ServerInformation>>
    {
        /// <summary>
        /// Erzeugt eine neue Anfrage.
        /// </summary>
        public GetStateRequest()
        {
        }

        /// <summary>
        /// Führt eine Anfrage aus.
        /// </summary>
        /// <param name="response">Die zu befüllende Antwort für den Aufrufer.</param>
        /// <param name="server">Die aktuelle Implementierung des <i>Card Servers</i>.</param>
        protected override void OnExecute( Response<ServerInformation> response, ServerImplementation server )
        {
            // Execute
            response.ResponseData = server.BeginGetState().Result;
        }
    }
}
