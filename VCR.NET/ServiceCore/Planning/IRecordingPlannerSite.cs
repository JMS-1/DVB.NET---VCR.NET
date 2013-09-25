using System;
using System.Collections.Generic;
using JMS.DVB;
using JMS.DVB.Algorithms.Scheduler;


namespace JMS.DVBVCR.RecordingService.Planning
{
    /// <summary>
    /// Stellt alle Daten zur Aufzeichnungsplanung zur Verfügung.
    /// </summary>
    public interface IRecordingPlannerSite
    {
        /// <summary>
        /// Die Namen aller Geräteprofile, die verwendet werden sollen.
        /// </summary>
        IEnumerable<string> ProfileNames { get; }

        /// <summary>
        /// Der volle Pfad zu dem Regeldatei der Aufzeichnungsplanung.
        /// </summary>
        string ScheduleRulesPath { get; }

        /// <summary>
        /// Erstellt eine periodische Aufgabe zum Aktualisieren der Programmzeitschrift.
        /// </summary>
        /// <param name="resource">Die zugehörige Ressource.</param>
        /// <param name="profile">Die vollen Informationen zum Geräteprofil.</param>
        /// <returns>Die Beschreibung der Aufgabe oder <i>null</i>.</returns>
        PeriodicScheduler CreateProgramGuideTask( IScheduleResource resource, Profile profile );

        /// <summary>
        /// Erstellt eine periodische Aufgabe zum Aktualisieren der Quellen.
        /// </summary>
        /// <param name="resource">Die zugehörige Ressource.</param>
        /// <param name="profile">Die vollen Informationen zum Geräteprofil.</param>
        /// <returns>Die Beschreibung der Aufgabe oder <i>null</i>.</returns>
        PeriodicScheduler CreateSourceScanTask( IScheduleResource resource, Profile profile );

        /// <summary>
        /// Überträgt alle Aufträge in einen Ablaufplanung.
        /// </summary>
        /// <param name="scheduler">Die zu befüllende Ablaufplanung.</param>
        /// <param name="disabled">Alle deaktivierten Aufträge.</param>
        /// <param name="planner">Die zugehörige Aufzeichnungsplanung.</param>
        /// <param name="context">Eine neue Umgebung für die Erstellung des aktuellen Plans.</param>
        void AddRegularJobs( RecordingScheduler scheduler, Func<Guid, bool> disabled, RecordingPlanner planner, PlanContext context );

        /// <summary>
        /// Meldet eine Warteperiode.
        /// </summary>
        /// <param name="until">Der Zeitpunkt, an dem eine erneute Abfrage notwendig ist.</param>
        void Idle( DateTime until );

        /// <summary>
        /// Meldet, dass eine Aufzeichnung nicht ausgeführt wird.
        /// </summary>
        /// <param name="item">Die verborgene Aufzeichnung.</param>
        void Discard( IScheduleDefinition item );

        /// <summary>
        /// Fordert zum Beenden einer Aufzeichnung oder Aufgabe auf.
        /// </summary>
        /// <param name="item">Alle notwendigen Informationen zur Aufzeichnung.</param>
        /// <param name="planner">Die zugehörige Aufzeichnungsplanung.</param>
        void Stop( IScheduleInformation item, RecordingPlanner planner );

        /// <summary>
        /// Fordert zum Starten einer Aufzeichnung oder Aufgabe auf.
        /// </summary>
        /// <param name="item">Die Beschreibung der Aufgabe.</param>
        /// <param name="planner">Die zugehörige Aufzeichnungsplanung.</param>
        /// <param name="context">Zusatzinformationen zur Aufzeichnungsplanung.</param>
        void Start( IScheduleInformation item, RecordingPlanner planner, PlanContext context );
    }
}
