using System;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Aktiviert eine einzelne Quelle im <i>Zapping Modus</i>.
    /// </summary>
    [Serializable]
    public class SetZappingSourceRequest : Request<Response<ServerInformation>>
    {
        /// <summary>
        /// Die eindeutige Auswahl der gewünschten Quelle.
        /// </summary>
        public string SelectionKey { get; set; }

        /// <summary>
        /// Das Ziel des Netzwerkversands.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Erzeugt eine neue Anfrage.
        /// </summary>
        public SetZappingSourceRequest()
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
            response.ResponseData = server.BeginSetZappingSource( SelectionKey, Target ).Result;
        }
    }
}
