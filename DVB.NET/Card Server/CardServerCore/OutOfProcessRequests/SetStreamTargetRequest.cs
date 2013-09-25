using System;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Eine Anfrage zur Aktivierung oder Deaktivierung des Netzwerkversands einer aktiven
    /// Quelle.
    /// </summary>
    [Serializable]
    public class SetStreamTargetRequest : Request<Response>
    {
        /// <summary>
        /// Die betroffene Quelle.
        /// </summary>
        public SourceIdentifier Source { get; set; }

        /// <summary>
        /// Der eindeutige Name der Quelle.
        /// </summary>
        public Guid UniqueIdentifier { get; set; }

        /// <summary>
        /// Meldet oder setzt die TCP/IP UDP Empfangsadresse.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Erzeugt eine neue Anfrage.
        /// </summary>
        public SetStreamTargetRequest()
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
            ServerImplementation.EndRequest( server.BeginSetStreamTarget( Source, UniqueIdentifier, Target ) );
        }
    }
}
