using JMS.DVB;
using JMS.DVBVCR.RecordingService.WebServer;
using System;
using System.Globalization;
using System.Linq;
using System.Web.Http;


namespace JMS.DVBVCR.RecordingService.RestWebApi
{
    /// <summary>
    /// Erlaubt den Zugriff auf den Aufzeichnungsplan.
    /// </summary>
    public class PlanController : ApiController
    {
        /// <summary>
        /// Meldet den aktuellen Aufzeichnungsplan.
        /// </summary>
        /// <param name="limit">Die maximale Anzahl von Einträgen im Ergebnis.</param>
        /// <param name="end">Es werden nur Aufzeichnungen betrachtet, die vor diesem Zeitpunkt beginnen.</param>
        /// <returns>Alle Einträge des Aufzeichnungsplans.</returns>
        [HttpGet]
        public PlanActivity[] GetPlan( string limit, string end )
        {
            // Get the limit
            int maximum;
            if (!int.TryParse( limit, out maximum ) || (maximum <= 0))
                maximum = 1000;

            // Get the date
            DateTime endTime;
            if (!DateTime.TryParse( end, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out endTime ))
                endTime = DateTime.MaxValue;

            // Route from Web AppDomain into service AppDomain
            var activities = ServerRuntime.VCRServer.GetPlan( endTime, maximum, PlanActivity.Create );

            // Must resort to correct for running entries
            Array.Sort( activities, PlanActivity.ByStartComparer );

            // Report
            return activities;
        }

        /// <summary>
        /// Meldet den aktuellen Aufzeichnungsplan der nächsten 4 Wochen für mobile Geräte, wobei Aufgaben ausgeschlossen sind.
        /// </summary>
        /// <param name="limit">Die maximale Anzahl von Einträgen im Ergebnis.</param>
        /// <param name="mobile">Schalter zum Umschalten auf die Liste für mobile Geräte.</param>
        /// <returns>Alle Einträge des Aufzeichnungsplans.</returns>
        [HttpGet]
        public PlanActivityMobile[] GetPlanMobile( string limit, string mobile )
        {
            // Get the limit
            int maximum;
            if (!int.TryParse( limit, out maximum ) || (maximum <= 0))
                maximum = 1000;

            // Use helper
            return
                GetPlan( null, DateTime.UtcNow.AddDays( 28 ).ToString( "o" ) )
                    .Where( plan => !string.IsNullOrEmpty( plan.Source ) )
                    .Take( maximum )
                    .Select( PlanActivityMobile.Create )
                    .ToArray();
        }

        /// <summary>
        /// Ermittelt Informationen zu allen Geräteprofilen.
        /// </summary>
        /// <returns>Die gewünschte Liste.</returns>
        [HttpGet]
        public PlanCurrent[] GetCurrent()
        {
            // Forward
            return
                ServerRuntime
                    .VCRServer
                    .GetCurrentRecordings( PlanCurrent.Create, PlanCurrent.Create, PlanCurrent.Create )
                    .OrderBy( current => current.StartTime )
                    .ThenBy( current => current.Duration )
                    .ToArray();
        }

        /// <summary>
        /// Ermittelt Informationen zu allen Geräteprofilen.
        /// </summary>
        /// <param name="mobile">Schalter zum Umschalten auf die Liste für mobile Geräte.</param>
        /// <returns>Die gewünschte Liste.</returns>
        [HttpGet]
        public PlanCurrentMobile[] GetCurrent( string mobile )
        {
            // Forward
            return
                ServerRuntime
                    .VCRServer
                    .GetCurrentRecordings( PlanCurrent.Create )
                    .Where( current => !string.IsNullOrEmpty( current.SourceName ) )
                    .Select( PlanCurrentMobile.Create )
                    .OrderBy( current => current.StartTime )
                    .ThenBy( current => current.Duration )
                    .ToArray();
        }

        /// <summary>
        /// Fordert die Aktualisierung der Quellen an.
        /// </summary>
        /// <param name="sourceScan">Wird zur Unterscheidung der Methoden verwendet.</param>
        [HttpPost]
        public void StartSourceScan( string sourceScan ) => ServerRuntime.VCRServer.ForceSoureListUpdate();

        /// <summary>
        /// Fordert die Aktualisierung der Programmzeitschrift an.
        /// </summary>
        /// <param name="guideUpdate">Wird zur Unterscheidung der Methoden verwendet.</param>
        [HttpPost]
        public void StartGuideUpdate( string guideUpdate ) => ServerRuntime.VCRServer.ForceProgramGuideUpdate();

        /// <summary>
        /// Ändert den Netzwerkversand.
        /// </summary>
        /// <param name="detail">Der Name eines Geräteprofils.</param>
        /// <param name="source">Eine Quelle.</param>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung einer Aufzeichnung.</param>
        /// <param name="target">Das neue Ziel des Netzwerkversands.</param>
        [HttpPost]
        public void SetStreamTarget( string detail, string source, Guid scheduleIdentifier, string target )
        {
            // Analyse
            var sourceIdentifier = SourceIdentifier.Parse( source );

            // Process
            ServerRuntime.VCRServer.SetStreamTarget( detail, sourceIdentifier, scheduleIdentifier, target );
        }
    }
}
