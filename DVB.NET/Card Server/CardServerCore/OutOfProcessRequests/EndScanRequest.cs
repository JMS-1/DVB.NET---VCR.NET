using System;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Diese Anfrage schliesst den Sendersuchlauf ab.
    /// </summary>
    [Serializable]
    public class EndScanRequest : Request<Response>
    {
        /// <summary>
        /// Meldet oder legt fest, ob das Geräteprofil überhaupt aktualisiert werden soll und
        /// wenn ja, ob die vorhandenen Quellen vorher entfernt werden sollen (nicht gesetzt)
        /// oder mit den neuen Informationen vermischt werden sollen (gesetzt).
        /// </summary>
        public bool? UpdateProfile { get; set; }

        /// <summary>
        /// Erzeugt eine neue Anfrage.
        /// </summary>
        public EndScanRequest()
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
            ServerImplementation.EndRequest( server.BeginEndScan( UpdateProfile ) );
        }
    }
}
