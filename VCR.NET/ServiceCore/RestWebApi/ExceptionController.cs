using System;
using System.Web.Http;
using JMS.DVBVCR.RecordingService.WebServer;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Ändert Ausnahmeregelungen.
    /// </summary>
    public class ExceptionController : ApiController
    {
        /// <summary>
        /// Verändert eine Ausnahme.
        /// </summary>
        /// <param name="detail">Die betroffene Aufzeichnung.</param>
        /// <param name="when">Der Referenztag.</param>
        /// <param name="startDelta">Die Verschiebung des Aufzeichnungsbeginns in Minuten.</param>
        /// <param name="durationDelta">Die Verschiebung der Aufzeichnungsdauer in Minuten.</param>
        [HttpPut]
        public void ChangeException( string detail, string when, int startDelta, int durationDelta )
        {
            // Parse the date
            var date = new DateTime( long.Parse( when ), DateTimeKind.Utc );
            ServerRuntime.ParseUniqueWebId( detail, out Guid jobIdentifier, out Guid scheduleIdentifier );

            // Forward
            ServerRuntime.VCRServer.ChangeException( jobIdentifier, scheduleIdentifier, date, startDelta, durationDelta );
        }
    }
}
