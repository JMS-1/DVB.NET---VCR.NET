using System;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Aktiviert den Empfang einer oder mehrerer Quellen.
    /// </summary>
    [Serializable]
    public class AddSourcesRequest : Request<Response<StreamInformation[]>>
    {
        /// <summary>
        /// Die Informationen zum gewünschten Empfang.
        /// </summary>
        public ReceiveInformation[] Sources { get; set; }

        /// <summary>
        /// Erzeugt eine neue Anfrage.
        /// </summary>
        public AddSourcesRequest()
        {
        }

        /// <summary>
        /// Führt eine Anfrage aus.
        /// </summary>
        /// <param name="response">Die zu befüllende Antwort für den Aufrufer.</param>
        /// <param name="server">Die aktuelle Implementierung des <i>Card Servers</i>.</param>
        protected override void OnExecute( Response<StreamInformation[]> response, ServerImplementation server )
        {
            // Execute
            response.ResponseData = server.BeginAddSources( Sources ).Result;
        }
    }
}
