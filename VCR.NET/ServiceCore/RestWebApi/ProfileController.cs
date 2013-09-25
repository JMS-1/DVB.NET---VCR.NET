using System;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using JMS.DVB;
using JMS.DVBVCR.RecordingService.WebServer;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Erlaubt den Zurgiff auf die Geräteprofile, die der <i>VCR.NET Recording Service</i>
    /// verwaltet.
    /// </summary>
    public class ProfileController : ApiController
    {
        /// <summary>
        /// Meldet alle Geräteprofile, die der <i>VCR.NET Recording Service</i> verwenden darf.
        /// </summary>
        /// <returns>Die Liste der Geräteprofile.</returns>
        [HttpGet]
        public ProfileInfo[] ListProfiles()
        {
            // Forward
            return
                ServerRuntime
                    .VCRServer
                    .GetProfiles( ProfileInfo.Create )
                    .OrderBy( profile => profile.Name, ProfileManager.ProfileNameComparer )
                    .ToArray();
        }

        /// <summary>
        /// Ermittelt alle verfügbaren Sender.
        /// </summary>
        /// <param name="detail">Der Name des zu verwendenden Geräteprofils.</param>
        /// <returns>Die gewünschte Liste von Sendern.</returns>
        [HttpGet]
        public ProfileSource[] FindSources( string detail )
        {
            // Forward to other application domain
            return ServerRuntime.VCRServer.GetSources( detail, true, true, ProfileSource.Create );
        }

        /// <summary>
        /// Verändert den Endzeitpunkt.
        /// </summary>
        /// <param name="detail">Der Name eines Geräteprofils.</param>
        /// <param name="disableHibernate">Gesetzt, wenn der Übergang in den Schlafzustand vermieden werden soll.</param>
        /// <param name="schedule">Die Identifikation einer laufenden Aufzeichnung.</param>
        /// <param name="endTime">Der neue Endzeitpunkt.</param>
        [HttpPut]
        public void SetNewEndTime( string detail, bool disableHibernate, string schedule, string endTime )
        {
            // Map parameters to native types
            var scheduleIdentifier = new Guid( schedule );
            var end = DateTime.Parse( endTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind );

            // Forward
            ServerRuntime.VCRServer.ChangeRecordingStreamEndTime( detail, scheduleIdentifier, end, disableHibernate );
        }

        /// <summary>
        /// Ermittelt alle aktiven Aufträge eines Geräteprofils.
        /// </summary>
        /// <param name="detail">Der Name des Geräteprofils.</param>
        /// <param name="activeJobs">Markierung zur Unterscheidung der Aufträge.</param>
        /// <returns>Die Liste der Aufträge.</returns>
        [HttpGet]
        public ProfileJobInfo[] GetActiveJobs( string detail, string activeJobs )
        {
            // Request jobs
            return
                ServerRuntime
                    .VCRServer
                    .GetJobs( ProfileJobInfo.Create )
                    .Where( job => job != null )
                    .Where( job => ProfileManager.ProfileNameComparer.Equals( job.Profile, detail ) )
                    .OrderBy( job => job.Name, StringComparer.InvariantCultureIgnoreCase )
                    .ToArray();
        }
    }
}
