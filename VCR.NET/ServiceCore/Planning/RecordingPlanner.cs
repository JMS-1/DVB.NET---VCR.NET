using JMS.DVB;
using JMS.DVB.Algorithms.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace JMS.DVBVCR.RecordingService.Planning
{
    /// <summary>
    /// Die globale Aufzeichnungsplanung.
    /// </summary>
    /// <threadsafety static="true" instance="false">Diese Klasse kann nicht <see cref="Thread"/> 
    /// übergreifend verwendet werden. Der Aufrufer hat für eine entsprechende Synchronisation zu 
    /// sorgen.</threadsafety>
    public class RecordingPlanner : IDisposable
    {
        /// <summary>
        /// Die zugehörige Arbeitsumgebung.
        /// </summary>
        private readonly IRecordingPlannerSite m_site;

        /// <summary>
        /// Verwaltet alle verwendeten Geräteprofile.
        /// </summary>
        private readonly Dictionary<string, IScheduleResource> m_resources = new Dictionary<string, IScheduleResource>( ProfileManager.ProfileNameComparer );

        /// <summary>
        /// Alle aktuellen periodischen Aufgaben.
        /// </summary>
        private readonly List<PeriodicScheduler> m_tasks = new List<PeriodicScheduler>();

        /// <summary>
        /// Die Verwaltung der Geräteprofile.
        /// </summary>
        private IResourceManager m_manager;

        /// <summary>
        /// Alle laufenden Aufzeichnungen.
        /// </summary>
        private readonly Dictionary<Guid, ScheduleInformation> m_started = new Dictionary<Guid, ScheduleInformation>();

        /// <summary>
        /// Erstellt eine neue Planung.
        /// </summary>
        /// <param name="site">Die zugehörige Arbeitsumgebung.</param>
        private RecordingPlanner( IRecordingPlannerSite site )
        {
            // Remember
            m_site = site;

            // Process all profiles
            foreach (var profileName in site.ProfileNames)
            {
                // Look up the profile
                var profile = ProfileManager.FindProfile( profileName );
                if (profile == null)
                    continue;

                // Create the resource for it
                var profileResource = ProfileScheduleResource.Create( profileName );

                // Remember
                m_resources.Add( profileName, profileResource );

                // See if this is a leaf profile
                if (!string.IsNullOrEmpty( profile.UseSourcesFrom ))
                    continue;

                // See if we should process guide updates
                var guideTask = site.CreateProgramGuideTask( profileResource, profile );
                if (guideTask != null)
                    m_tasks.Add( guideTask );

                // See if we should update the source list
                var scanTask = site.CreateSourceScanTask( profileResource, profile );
                if (scanTask != null)
                    m_tasks.Add( scanTask );
            }

            // Make sure we report all errors
            try
            {
                // Create the manager
                m_manager = ResourceManager.Create( site.ScheduleRulesPath, ProfileManager.ProfileNameComparer );
            }
            catch (Exception e)
            {
                // Report
                VCRServer.LogError( Properties.Resources.BadRuleFile, e.Message );

                // Use standard rules
                m_manager = ResourceManager.Create( ProfileManager.ProfileNameComparer );
            }

            // Safe configure it
            try
            {
                // All all resources
                foreach (var resource in m_resources.Values)
                    m_manager.Add( resource );
            }
            catch (Exception e)
            {
                // Cleanup
                Dispose();

                // Report
                VCRServer.Log( e );
            }
        }

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        public void Dispose()
        {
            (Interlocked.Exchange( ref m_manager, null ))?.Dispose();
        }

        /// <summary>
        /// Ermittelt zu einem Geräteprofil die zugehörige Ressourcenverwaltung.
        /// </summary>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <returns>Die zugehörige Ressource, sofern bekannt.</returns>
        public IScheduleResource GetResourceForProfile( string profileName )
        {
            // None
            if (string.IsNullOrEmpty( profileName ))
                return null;

            // Ask map
            if (m_resources.TryGetValue(profileName, out IScheduleResource resource))
                return resource;

            // Not found
            return null;
        }

        /// <summary>
        /// Erstellt eine neue Planung.
        /// </summary>
        /// <param name="site">Die zugehörige Arbeitsumgebung.</param>
        /// <returns>Die gewünschte Planungsumgebung.</returns>
        public static RecordingPlanner Create( IRecordingPlannerSite site )
        {
            // Validate
            if (site == null)
                throw new ArgumentNullException( nameof( site ) );

            // Forward
            return new RecordingPlanner( site );
        }

        /// <summary>
        /// Ermittelt die nächste Aufgabe.
        /// </summary>
        /// <param name="referenceTime">Der Bezugspunkt für die Analyse.</param>
        public void DispatchNextActivity( DateTime referenceTime )
        {
            // As long as necessary
            for (var skipped = new HashSet<Guid>(); ;)
            {
                // The plan context we created
                PlanContext context = null;

                // Request activity - we only look 200 plan items into the future to reduce execution time at least a bit
                var activity = m_manager.GetNextActivity( referenceTime, ( scheduler, time ) => context = GetPlan( scheduler, time, skipped.Contains, 200 ) );
                if (activity == null)
                    return;

                // The easiest case is a wait
                var wait = activity as WaitActivity;
                if (wait != null)
                {
                    // Report to site
                    m_site.Idle( wait.RetestTime );

                    // Done
                    return;
                }

                // Start processing
                var start = activity as StartActivity;
                if (start != null)
                {
                    // Check mode of operation
                    var schedule = start.Recording;
                    var definition = schedule.Definition;
                    if (schedule.Resource == null)
                    {
                        // Report to site
                        m_site.Discard( definition );

                        // Add to exclusion list
                        skipped.Add( definition.UniqueIdentifier );

                        // Try again without
                        continue;
                    }

                    // Forward to site
                    m_site.Start( schedule, this, context );

                    // Done
                    return;
                }

                // End processing
                var stop = activity as StopActivity;
                if (stop != null)
                {
                    // Lookup the item and report to site
                    if (!m_started.TryGetValue(stop.UniqueIdentifier, out ScheduleInformation schedule))
                        return;

                    // Report to site
                    m_site.Stop( schedule.Schedule, this );

                    // Done
                    return;
                }

                // Must be some wrong version
                throw new NotSupportedException( activity.GetType().AssemblyQualifiedName );
            }
        }

        /// <summary>
        /// Startet eine Aufzeichnung oder eine Aufgabe.
        /// </summary>
        /// <param name="item">Die Beschreibung der Aufgabe.</param>
        /// <returns>Gesetzt, wenn der Vorgang erfolgreich war.</returns>
        public bool Start( IScheduleInformation item )
        {
            // Validate
            if (item is ScheduleInformation)
                VCRServer.LogError( Properties.Resources.BadScheduleInformation, item.Definition.UniqueIdentifier, item.Definition.Name );

            // Try start
            if (!m_manager.Start( item ))
                return false;

            // Remember
            m_started.Add( item.Definition.UniqueIdentifier, new ScheduleInformation( item ) );

            // Did it
            return true;
        }

        /// <summary>
        /// Beendet eine Aufzeichnung oder eine Aufgabe.
        /// </summary>
        /// <param name="itemIdentifier">Die gewünschte Aufgabe.</param>
        public void Stop( Guid itemIdentifier )
        {
            // Unregister
            if (!m_started.Remove( itemIdentifier ))
                return;

            // Forward
            m_manager.Stop( itemIdentifier );
        }

        /// <summary>
        /// Verändert den Endzeitpunkt einer Aufzeichnung.
        /// </summary>
        /// <param name="itemIdentifier">Die zugehörige Aufzeichnung.</param>
        /// <param name="newEndTime">Die gewünschte Verschiebung des Endzeitpunktes.</param>
        /// <returns>Gesetzt, wenn die Änderung ausgeführt werden konnte.</returns>
        public bool SetEndTime( Guid itemIdentifier, DateTime newEndTime )
        {
            // Find the recording
            var recording = m_manager.CurrentAllocations.FirstOrDefault( plan => plan.UniqueIdentifier.Equals( itemIdentifier ) );
            if (recording == null)
                return true;

            // New end time
            var newEndLimit = DateTime.UtcNow;
            if (newEndTime < newEndLimit)
                newEndTime = newEndLimit;

            // Forward
            if (!m_manager.Modify( itemIdentifier, newEndTime ))
                return false;

            // See if we know it
            if (!m_started.TryGetValue(itemIdentifier, out ScheduleInformation started))
                return true;

            // Try to get the new schedule data
            recording = m_manager.CurrentAllocations.FirstOrDefault( plan => plan.UniqueIdentifier.Equals( itemIdentifier ) );

            // Update
            if (recording != null)
                started.RealTime = recording.Time;

            // Did it
            return true;
        }

        /// <summary>
        /// Ermittelt den aktuellen Aufzeichnungsplan.
        /// </summary>
        /// <param name="scheduler">Die zu verwendende Zeitplanungsinstanz.</param>
        /// <param name="referenceTime">Der Bezugspunkt für die Planung.</param>
        /// <param name="disabled">Alle deaktivierte Aufträge und Aufgaben.</param>
        /// <param name="limit">Die Anzahl der zu berücksichtigenden Planungselemente.</param>
        /// <returns>Die Liste der nächsten Aufzeichnungen.</returns>
        private PlanContext GetPlan( RecordingScheduler scheduler, DateTime referenceTime, Func<Guid, bool> disabled, int limit )
        {
            // Create a new plan context
            var context = new PlanContext( m_started.Values );

            // Configure it
            m_site.AddRegularJobs( scheduler, disabled, this, context );

            // Enable all
            if (disabled == null)
                disabled = identifier => false;

            // Do the sort
            context.LoadPlan( scheduler.GetSchedules( referenceTime, m_tasks.Where( task => !disabled( task.UniqueIdentifier ) ).ToArray() ).Take( limit ) );

            // Report
            return context;
        }

        /// <summary>
        /// Ermittelt den aktuellen Aufzeichnungsplan.
        /// </summary>
        /// <param name="referenceTime">Der Bezugspunkt für die Planung.</param>
        /// <returns>Die Liste der nächsten Aufzeichnungen.</returns>
        public PlanContext GetPlan( DateTime referenceTime ) => GetPlan( m_manager.CreateScheduler( false ), referenceTime, null, 1000 );

        /// <summary>
        /// Entfernt alle aktiven Aufzeichnungen.
        /// </summary>
        public void Reset()
        {
            // Remove all
            foreach (var active in m_manager.CurrentAllocations)
                m_manager.Stop( active.UniqueIdentifier );

            // Forget what we did
            m_started.Clear();
        }
    }
}
