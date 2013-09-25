using System;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Beendet die Aktualisierung der Programmzeitschrift und übermittelt die gesammelten Informationen.
    /// </summary>
    [Serializable]
    public class EndEPGRequest : Request<Response<ProgramGuideItem[]>>
    {
        /// <summary>
        /// Erzeugt eine neue Anfrage.
        /// </summary>
        public EndEPGRequest()
        {
        }

        /// <summary>
        /// Führt eine Anfrage aus.
        /// </summary>
        /// <param name="response">Die zu befüllende Antwort für den Aufrufer.</param>
        /// <param name="server">Die aktuelle Implementierung des <i>Card Servers</i>.</param>
        protected override void OnExecute( Response<ProgramGuideItem[]> response, ServerImplementation server )
        {
            // Execute
            response.ResponseData = server.BeginEndEPGCollection().Result;
        }
    }
}
