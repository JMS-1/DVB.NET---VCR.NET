using System.Web.Http;
using JMS.DVB;
using JMS.DVBVCR.RecordingService.WebServer;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Vermittelt den Zugriff auf die im <i>VCR.NET Recording Service</i> verwalteten
    /// Geräte zur Fernnutzung.
    /// </summary>
    public class ZappingController : ApiController
    {
        /// <summary>
        /// Ermittelt den aktuellen Zustand.
        /// </summary>
        /// <param name="detail">Der Name des zu verwendenden Geräteprofils.</param>
        /// <returns>Der Zustand auf dem gewählten Geräteprofil.</returns>
        [HttpGet]
        public ZappingStatus GetCurrentStatus( string detail )
        {
            // Process
            return ServerRuntime.VCRServer.LiveModeOperation( detail, true, null, null, ZappingStatus.Create );
        }

        /// <summary>
        /// Ermittelt alle verfügbaren Sender.
        /// </summary>
        /// <param name="detail">Der Name des zu verwendenden Geräteprofils.</param>
        /// <param name="tv">Gesetzt, wenn Fernsehender berücksichtigt werden sollen.</param>
        /// <param name="radio">Gesetzt, wenn Radiosender berücksichtigt werden sollen.</param>
        /// <returns>Die gewünschte Liste von Sendern.</returns>
        [HttpGet]
        public ZappingSource[] FindSources( string detail, bool tv, bool radio )
        {
            // Forward to other application domain
            return ServerRuntime.VCRServer.GetSources( detail, tv, radio, ZappingSource.Create );
        }

        /// <summary>
        /// Aktiviert eine neue Sitzung.
        /// </summary>
        /// <param name="detail">Der Name des zu verwendenden Geräteprofils.</param>
        /// <param name="target">Legt fest, wohin die Nutzdaten zu senden sind.</param>
        /// <returns>Der Zustand auf dem gewählten Geräteprofil.</returns>
        [HttpPost]
        public ZappingStatus Connect( string detail, string target )
        {
            // Process
            return ServerRuntime.VCRServer.LiveModeOperation( detail, true, target, null, ZappingStatus.Create );
        }

        /// <summary>
        /// Deaktiviert eine Sitzung.
        /// </summary>
        /// <param name="detail">Der Name des zu verwendenden Geräteprofils.</param>
        /// <returns>Der Zustand auf dem gewählten Geräteprofil.</returns>
        [HttpDelete]
        public ZappingStatus Disconnect( string detail )
        {
            // Process
            return ServerRuntime.VCRServer.LiveModeOperation( detail, false, null, null, ZappingStatus.Create );
        }

        /// <summary>
        /// Wählt einen Quelle aus.
        /// </summary>
        /// <param name="detail">Der Name des zu verwendenden Geräteprofils.</param>        
        /// <param name="source">Die gewünschte Quelle als Tripel analog zur Textdarstellung von <see cref="SourceIdentifier"/>.</param>
        /// <returns>Der Zustand auf dem gewählten Geräteprofil.</returns>
        [HttpPut]
        public ZappingStatus Tune( string detail, string source )
        {
            // Process
            return ServerRuntime.VCRServer.LiveModeOperation( detail, true, null, SourceIdentifier.Parse( source ), ZappingStatus.Create );
        }
    }
}
