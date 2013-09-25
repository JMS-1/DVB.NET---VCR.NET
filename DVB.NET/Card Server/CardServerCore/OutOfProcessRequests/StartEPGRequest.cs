using System;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Die Anfrage fordetr den <i>Card Server</i> auf, eine Aktualisierung der Programmzeitschrift zu starten.
    /// </summary>
    [Serializable]
    public class StartEPGRequest : Request<Response>
    {
        /// <summary>
        /// Die Quellen, deren Daten benötigt werden.
        /// </summary>
        public SourceIdentifier[] Sources { get; set; }

        /// <summary>
        /// Die Erweiterungen des Suchalgorithmus, die zu aktivieren sind.
        /// </summary>
        public EPGExtensions Extensions { get; set; }

        /// <summary>
        /// Erzeugt eine neue Anfrage.
        /// </summary>
        public StartEPGRequest()
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
            ServerImplementation.EndRequest( server.BeginStartEPGCollection( Sources, Extensions ) );
        }
    }
}
