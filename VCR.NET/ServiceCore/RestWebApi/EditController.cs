using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using JMS.DVB;
using JMS.DVBVCR.RecordingService.Persistence;
using JMS.DVBVCR.RecordingService.ProgramGuide;
using JMS.DVBVCR.RecordingService.WebServer;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Der Web Service zur Pflege von Aufzeichnungen und Aufträgen.
    /// </summary>
    public class EditController : ApiController
    {
        /// <summary>
        /// Wird zum Anlegen einer neuen Aufzeichnung verwendet.
        /// </summary>
        /// <param name="data">Die Daten zur Aufzeichnung.</param>
        /// <returns>Die Identifikation des neuen Auftrags.</returns>
        [HttpPost]
        public string CreateNewJob( [FromBody] JobScheduleData data )
        {
            // Reconstruct
            var job = data.Job.CreateJob();
            var schedule = data.Schedule.CreateSchedule( job );

            // See if we can use it
            if (!schedule.IsActive)
                throw new ArgumentException( Properties.Resources.ScheduleInPast );

            // Connect
            job.Schedules.Add( schedule );

            // Process
            ServerRuntime.VCRServer.UpdateJob( job, schedule.UniqueID.Value );

            // Update recently used channels
            UserProfileSettings.AddRecentChannel( data.Job.Source );
            UserProfileSettings.AddRecentChannel( data.Schedule.Source );

            // Report
            return ServerRuntime.GetUniqueWebId( job, schedule );
        }

        /// <summary>
        /// Ermittelt die Daten zu einer einzelnen Aufzeichnung.
        /// </summary>
        /// <param name="detail">Die Referenz auf die Aufzeichnung.</param>
        /// <param name="epg">Informationen zu einem Eintrag aus der Programmzeitschrift.</param>
        /// <returns>Die Daten zur gewünschten Aufzeichnung.</returns>
        [HttpGet]
        public JobScheduleInfo FindJob( string detail, string epg = null )
        {
            // May need to recreate the identifier
            if (detail.StartsWith( "*" ))
                if (detail.Length == 1)
                    detail = Guid.NewGuid().ToString( "N" ) + Guid.NewGuid().ToString( "N" );
                else
                    detail = detail.Substring( 1, 32 ) + Guid.NewGuid().ToString( "N" );

            // Parameter analysieren
            VCRJob job;
            var schedule = ServerRuntime.ParseUniqueWebId( detail, out job );

            // See if we have to initialize from program guide
            ProgramGuideEntry epgEntry = null;
            string profile = null;
            if (!string.IsNullOrEmpty( epg ))
            {
                // Get parts
                var epgInfo = epg.Split( ':' );

                // Locate
                epgEntry = ServerRuntime.VCRServer.FindProgramGuideEntry( profile = epgInfo[1], SourceIdentifier.Parse( epgInfo[2] ), new DateTime( long.Parse( epgInfo[0] ), DateTimeKind.Utc ) );
            }

            // Information erzeugen
            return JobScheduleInfo.Create( job, schedule, epgEntry, profile );
        }

        /// <summary>
        /// Aktualisiert die Daten einer Aufzeichnung.
        /// </summary>
        /// <param name="detail">Die Referenz auf die Aufzeichnung.</param>
        /// <param name="data">Die neuen Daten von Auftrag und Aufzeichnung.</param>
        [HttpPut]
        public void UpdateRecording( string detail, [FromBody] JobScheduleData data )
        {
            // Parameter analysieren
            VCRJob job;
            var schedule = ServerRuntime.ParseUniqueWebId( detail, out job );

            // Validate
            if (schedule == null)
                throw new ArgumentException( "Job or Schedule not found" );

            // Take the new job data
            var newJob = data.Job.CreateJob( job.UniqueID.Value );
            var newSchedule = data.Schedule.CreateSchedule( schedule.UniqueID.Value, newJob );

            // All exceptions still active
            var activeExceptions = data.Schedule.Exceptions ?? Enumerable.Empty<PlanException>();
            var activeExceptionDates = new HashSet<DateTime>( activeExceptions.Select( exception => exception.ExceptionDate ) );

            // Copy over all exceptions
            newSchedule.Exceptions.AddRange( schedule.Exceptions.Where( exception => activeExceptionDates.Contains( exception.When ) ) );

            // See if we can use it
            if (!newSchedule.IsActive)
                throw new ArgumentException( Properties.Resources.ScheduleInPast );

            // Copy all schedules expect the one wie founr
            newJob.Schedules.AddRange( job.Schedules.Where( oldSchedule => !ReferenceEquals( oldSchedule, schedule ) ) );

            // Add the updated variant
            newJob.Schedules.Add( newSchedule );

            // Send to persistence
            ServerRuntime.VCRServer.UpdateJob( newJob, newSchedule.UniqueID.Value );

            // Update recently used channels
            UserProfileSettings.AddRecentChannel( data.Job.Source );
            UserProfileSettings.AddRecentChannel( data.Schedule.Source );
        }

        /// <summary>
        /// Entfernt eine Aufzeichnung.
        /// </summary>
        /// <param name="detail">Die Referenz auf die Aufzeichnung.</param>
        [HttpDelete]
        public void DeleteRecording( string detail )
        {
            // Parameter analysieren
            VCRJob job;
            var schedule = ServerRuntime.ParseUniqueWebId( detail, out job );

            // Validate
            if (schedule == null)
                throw new ArgumentException( "Job or Schedule not found" );

            // Remove schedule from job - since we are living in a separate application domain we only have a copy of it
            job.Schedules.Remove( schedule );

            // Send to persistence
            if (job.Schedules.Count < 1)
                ServerRuntime.VCRServer.DeleteJob( job );
            else
                ServerRuntime.VCRServer.UpdateJob( job, null );
        }

        /// <summary>
        /// Legt eine neue Aufzeichnung zu einem Auftrag an.
        /// </summary>
        /// <param name="detail">Die eindeutige Kennung des Auftrags.</param>
        /// <param name="data">Die Daten zum Auftrag und zur Aufzeichnung.</param>
        /// <returns>Die Identifikation des neuen Auftrags.</returns>
        [HttpPost]
        public string CreateNewRecording( string detail, [FromBody] JobScheduleData data )
        {
            // Parameter analysieren
            VCRJob job;
            ServerRuntime.ParseUniqueWebId( detail + Guid.NewGuid().ToString( "N" ), out job );

            // Validate
            if (job == null)
                throw new ArgumentException( "Job not found" );

            // Take the new job data
            var newJob = data.Job.CreateJob( job.UniqueID.Value );
            var newSchedule = data.Schedule.CreateSchedule( newJob );

            // See if we can use it
            if (!newSchedule.IsActive)
                throw new ArgumentException( Properties.Resources.ScheduleInPast );

            // Add all existing
            newJob.Schedules.AddRange( job.Schedules );

            // Add the new one
            newJob.Schedules.Add( newSchedule );

            // Send to persistence
            ServerRuntime.VCRServer.UpdateJob( newJob, newSchedule.UniqueID.Value );

            // Update recently used channels
            UserProfileSettings.AddRecentChannel( data.Job.Source );
            UserProfileSettings.AddRecentChannel( data.Schedule.Source );

            // Report
            return ServerRuntime.GetUniqueWebId( newJob, newSchedule );
        }
    }
}
