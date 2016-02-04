using JMS.DVBVCR.RecordingService.WebServer;
using System;
using System.Globalization;
using System.Web.Http;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Dieser Webdienst erlaubt den Zugriff auf Protokolldaten.
    /// </summary>
    public class ProtocolController : ApiController
    {
        /// <summary>
        /// Ermittelt einen Auszug aus dem Protokoll eines Gerätes.
        /// </summary>
        /// <param name="detail">Das gewünschte Gerät.</param>
        /// <param name="start">Der Startzeitpunkt in ISO Notation.</param>
        /// <param name="end">Der Endzeitpunkt in ISO Notation.</param>
        /// <returns>Die gewünschte Liste.</returns>
        [HttpGet]
        public ProtocolEntry[] Query( string detail, string start, string end )
        {
            // Decode
            var startTime= DateTime.Parse( start, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind );
            var endTime= DateTime.Parse( end, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind );

            // Forward
            return ServerRuntime.VCRServer.QueryLog( detail, startTime.Date, endTime.Date, ProtocolEntry.Create );
        }
    }
}
