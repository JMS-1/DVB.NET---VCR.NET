using System;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Beschreibt die Anfrage zum Festlegen des DVB.NET Geräteprofils, dass der <i>Card Server</i>
    /// verwenden soll.
    /// </summary>
    [Serializable]
    public class SetProfileRequest : Request<Response>
    {
        /// <summary>
        /// Der Name des zu verwendenden Geräteprofils.
        /// </summary>
        public string ProfileName { get; set; }

        /// <summary>
        /// Meldet oder legt fest, ob das verbundene Windows Gerät neu initialisiert werden soll.
        /// </summary>
        public bool ResetWakeUpDevice { get; set; }

        /// <summary>
        /// Meldet oder legt fest, ob es gestattet ist, aus einem H.264 Bildsignal die Systemzeit
        /// (PCR) eines <i>Transport Streams</i> abzuleiten.
        /// </summary>
        public bool DisablePCRFromH264Reconstruction { get; set; }

        /// <summary>
        /// Meldet oder legt fest, ob es gestattet ist, aus einem MPEG2 Bildsignal die Systemzeit
        /// (PCR) eines <i>Transport Streams</i> abzuleiten.
        /// </summary>
        public bool DisablePCRFromMPEG2Reconstruction { get; set; }

        /// <summary>
        /// Erzeugt eine neue Anfrage.
        /// </summary>
        public SetProfileRequest()
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
            ServerImplementation.EndRequest( server.BeginSetProfile( ProfileName, ResetWakeUpDevice, DisablePCRFromH264Reconstruction, DisablePCRFromMPEG2Reconstruction ) );
        }
    }
}
