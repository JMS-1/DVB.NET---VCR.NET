using System;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Diese Anfrage fordert den <i>Card Server</i> auf, eine bestimmte Quellgruppe (Transponder)
    /// anzuwählen.
    /// </summary>
    [Serializable]
    public class SelectRequest : Request<Response>
    {
        /// <summary>
        /// Die gewünschte Quellgruppe.
        /// </summary>
        public string SelectionKey { get; set; }

        /// <summary>
        /// Erzeugt eine neue Anfrage.
        /// </summary>
        public SelectRequest()
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
            ServerImplementation.EndRequest( server.BeginSelect( SelectionKey ) );
        }
    }
}
