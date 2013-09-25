using System;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Über diese Anfrage wird der Empfang aller Quellen eingestellt.
    /// </summary>
    [Serializable]
    public class RemoveAllSourcesRequest : Request<Response>
    {
        /// <summary>
        /// Erzeugt eine neue Anfrage.
        /// </summary>
        public RemoveAllSourcesRequest()
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
            ServerImplementation.EndRequest( server.BeginRemoveAllSources() );
        }
    }
}
