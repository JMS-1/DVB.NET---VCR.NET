using System;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Lädt eine Bibliothek mit Erweiterungen in diese <i>Card Server</i> Instanz.
    /// </summary>
    [Serializable]
    public class LoadExtensionsRequest : Request<Response>
    {
        /// <summary>
        /// Die eigentliche Erweiterung.
        /// </summary>
        public byte[] AssemblyData { get; set; }

        /// <summary>
        /// Option Informationen zum Debuggen der Erweiterung.
        /// </summary>
        public byte[] DebugData { get; set; }

        /// <summary>
        /// Erzeugt eine neue Anfrage.
        /// </summary>
        public LoadExtensionsRequest()
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
            ServerImplementation.EndRequest( server.BeginLoadExtensions( AssemblyData, DebugData ) );
        }
    }
}
