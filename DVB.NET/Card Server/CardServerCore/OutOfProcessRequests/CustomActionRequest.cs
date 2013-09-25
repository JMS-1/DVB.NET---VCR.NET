using System;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Führt eine Erweiterung aus.
    /// </summary>
    /// <typeparam name="TInput">Die Art der Eingangsdaten.</typeparam>
    /// <typeparam name="TOutput">Die Art der Ergebnisdaten.</typeparam>
    [Serializable]
    public class CustomActionRequest<TInput, TOutput> : Request<Response<TOutput>>
    {
        /// <summary>
        /// Die Klasse, von der aus die Erweierung angeboten wird.
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// Freie Parameter für die Ausführung der Erweiterung.
        /// </summary>
        public TInput Parameters { get; set; }

        /// <summary>
        /// Erzeugt eine neue Anfrage.
        /// </summary>
        public CustomActionRequest()
        {
        }

        /// <summary>
        /// Führt eine Anfrage aus.
        /// </summary>
        /// <param name="response">Die zu befüllende Antwort für den Aufrufer.</param>
        /// <param name="server">Die aktuelle Implementierung des <i>Card Servers</i>.</param>
        protected override void OnExecute( Response<TOutput> response, ServerImplementation server )
        {
            // Execute
            response.ResponseData = server.BeginCustomAction<TInput, TOutput>( ActionType, Parameters ).Result;
        }
    }
}
