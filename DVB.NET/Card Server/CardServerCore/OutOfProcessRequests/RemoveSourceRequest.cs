using System;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Eine Anfrage zum Deaktivieren des Empfangs einer einzelnen Quelle.
    /// </summary>
    [Serializable]
    public class RemoveSourceRequest : Request<Response>
    {
        /// <summary>
        /// Die eindeutige Kennung der betroffenen Quelle.
        /// </summary>
        public SourceIdentifier Source { get; set; }

        /// <summary>
        /// Der eindeutige Name der Quelle.
        /// </summary>
        public Guid UniqueIdentifier { get; set; }

        /// <summary>
        /// Erzeugt eine neue Anfrage.
        /// </summary>
        public RemoveSourceRequest()
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
            ServerImplementation.EndRequest( server.BeginRemoveSource( Source, UniqueIdentifier ) );
        }
    }
}
