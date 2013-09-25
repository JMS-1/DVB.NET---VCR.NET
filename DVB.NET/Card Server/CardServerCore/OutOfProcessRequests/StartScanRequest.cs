using System;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Mit dieser Anfrage wird die Aktualisierung der Quellen (Sendersuchlauf) angestossen.
    /// </summary>
    [Serializable]
    public class StartScanRequest : Request<Response>
    {
        /// <summary>
        /// Erzeugt eine neue Anfrage.
        /// </summary>
        public StartScanRequest()
        {
        }

        /// <summary>
        /// Führt eine Anfrage aus.
        /// </summary>
        /// <param name="response">Die zu befüllende Antwort für den Aufrufer.</param>
        /// <param name="server">Die aktuelle Implementierung des <i>Card Servers</i>.</param>
        protected override void OnExecute( Response response, ServerImplementation server )
        {
            // Execute
            ServerImplementation.EndRequest( server.BeginStartScan() );
        }
    }
}
