using JMS.DVBVCR.RecordingService.WebServer;
using System.Web.Http;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Pflegt die Benutzerkonfiguration.
    /// </summary>
    public class UserProfileController : ApiController
    {
        /// <summary>
        /// Meldet die aktuelle Benutzerkonfiguration.
        /// </summary>
        /// <returns>Die Einstellungen des Anwenders.</returns>
        [HttpGet]
        public UserProfile GetCurrentProfile() => UserProfile.Create();

        /// <summary>
        /// Aktualisiert die Daten des Geräteprofils.
        /// </summary>
        /// <param name="newProfile">Das neue Geräteprofil.</param>
        /// <returns>Die aktuellen Daten.</returns>
        [HttpPut]
        public UserProfile UpdateProfile( [FromBody] UserProfile newProfile )
        {
            // Forward
            newProfile.Update();

            // Report
            return UserProfile.Create();
        }

        /// <summary>
        /// Aktualisiert die Suchen der Programmzeitschrift.
        /// </summary>
        /// <param name="favorites">Dient zur Unterscheidung der Methoden.</param>
        [HttpPut]
        public void UpdateGuideFavorites( string favorites )
        {
            // Just store body as data
            UserProfileSettings.GuideFavorites = ControllerContext.Request.Content.ReadAsStringAsync().Result ?? string.Empty;

            // And update
            UserProfileSettings.Update();
        }
    }
}
