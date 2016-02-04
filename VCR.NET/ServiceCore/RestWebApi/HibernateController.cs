using JMS.DVBVCR.RecordingService.WebServer;
using System.Web.Http;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Verwaltet den Schlafzustand.
    /// </summary>
    public class HibernateController : ApiController
    {
        /// <summary>
        /// Vergißt, dass ein Übergang in den Schlafzustand nicht ausgeführt wurde.
        /// </summary>
        /// <param name="reset">Dient zur Unterscheidung der Methoden.</param>
        [HttpPost]
        public void ResetPendingHibernation( string reset ) => ServerRuntime.VCRServer.ResetPendingHibernation();

        /// <summary>
        /// Versucht, den Schlafzustand auszulösen.
        /// </summary>
        /// <param name="hibernate">Dient zur Unterscheidung der Methoden</param>
        [HttpPost]
        public void TryHibernate( string hibernate ) => ServerRuntime.VCRServer.TryHibernateIgnoringInteractiveUsers();
    }
}
