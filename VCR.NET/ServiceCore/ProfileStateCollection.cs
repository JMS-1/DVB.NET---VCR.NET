using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using JMS.DVB;
using JMS.DVB.Algorithms.Scheduler;
using JMS.DVBVCR.RecordingService.Persistence;
using JMS.DVBVCR.RecordingService.Planning;
using JMS.DVBVCR.RecordingService.Requests;
using JMS.DVBVCR.RecordingService.Win32Tools;


namespace JMS.DVBVCR.RecordingService
{
    /// <summary>
    /// Verwaltet den Arbeitszustand aller Geräteprofile.
    /// </summary>
    public class ProfileStateCollection : IRecordingPlannerSite, IDisposable
    {
        /// <summary>
        /// Meldet die primäre VCR.NET Instanz.
        /// </summary>
        public VCRServer Server { get; private set; }

        /// <summary>
        /// Alle von dieser Instanz verwalteten Geräteprofile.
        /// </summary>
        private readonly Dictionary<string, ProfileState> m_profiles;

        /// <summary>
        /// Erzeugt eine neue Verwaltungsinstanz.
        /// </summary>
        /// <param name="server">Die primäre VCR.NET Instanz.</param>
        internal ProfileStateCollection( VCRServer server )
        {
            // Remember
            Server = server;

            // Profiles to use
            var profileNames = VCRProfiles.ProfileNames.ToArray();
            var nameReport = string.Join( ", ", profileNames );

            // Log
            VCRServer.Log( LoggingLevel.Full, Properties.Resources.LoadProfiles, nameReport );

            // Report
            Tools.ExtendedLogging( "Loading Profile Collection: {0}", nameReport );

            // Load current profiles
            m_profiles = profileNames.ToDictionary( profileName => profileName, profileName => new ProfileState( this, profileName ), ProfileManager.ProfileNameComparer );

            // Now we can create the planner
            m_planner = RecordingPlanner.Create( this );
            m_planThread = new Thread( PlanThread ) { Name = "Recording Planner", IsBackground = true };

            // Configure timer
            m_timer.OnTimerExpired += BeginNewPlan;

            // Start planner
            m_planThread.Start();
        }

        /// <summary>
        /// Meldet Informationen zu allen Geräteprofilen.
        /// </summary>
        /// <typeparam name="TInfo">Die Art der gemeldeten Informationen.</typeparam>
        /// <param name="factory">Methode zum Erstellen der Information zu einem einzelnen Geräteprofil.</param>
        /// <returns>Die Informationen zu den Profilen.</returns>
        public IEnumerable<TInfo> InspectProfiles<TInfo>( Func<ProfileState, TInfo> factory ) => m_profiles.Values.Select( factory );

        /// <summary>
        /// Meldet die Anzahl der aktiven Aufzeichnungen.
        /// </summary>
        public int NumberOfActiveRecordings => (m_profiles.Count < 1) ? 0 : m_profiles.Values.Sum( profile => profile.NumberOfActiveRecordings );

        /// <summary>
        /// Ermittelt den Zustand eines einzelnen Geräteprofils.
        /// </summary>
        /// <param name="profileName">Das gewünschte Profil.</param>
        /// <returns>Die Zustandsbeschreibung oder <i>null</i>.</returns>
        public ProfileState this[string profileName]
        {
            get
            {
                // Validate
                if (string.IsNullOrEmpty( profileName ) || profileName.Equals( "*" ))
                {
                    // Attach to the default profile
                    var defaultProfile = VCRProfiles.DefaultProfile;
                    if (defaultProfile == null)
                        return null;

                    // Use it
                    profileName = defaultProfile.Name;
                }

                // Load
                if (m_profiles.TryGetValue(profileName, out ProfileState profile))
                    return profile;
                else
                    return null;
            }
        }

        /// <summary>
        /// Fordert eine baldmögliche Aktualisierung der Programmzeitschrift an.
        /// </summary>
        public void ForceProgramGuideUpdate()
        {
            // Report
            Tools.ExtendedLogging( "Full immediate Program Guide Update requested" );

            // Forward to all
            ForEachProfile( state => state.ProgramGuide.LastUpdateTime = null );
        }

        /// <summary>
        /// Erzwingt eine baldige Aktualisierung aller Listen von Quellen in allen
        /// Geräteprofilen.
        /// </summary>
        public void ForceSoureListUpdate()
        {
            // Report
            Tools.ExtendedLogging( "Full immediate Source List Update requested" );

            // Forward
            ForEachProfile( state => state.LastSourceUpdateTime = null );
        }

        /// <summary>
        /// Meldet, ob auf irgendeinem Geräteprofil ein Zugriff aktiv ist.
        /// </summary>
        public bool IsActive { get { return m_profiles.Values.Any( s => s.IsActive ); } }

        /// <summary>
        /// Wendet eine Methode auf alle verwalteten Profile an.
        /// </summary>
        /// <param name="method">Die gewünschte Methode.</param>
        /// <param name="ignoreErrors">Gesetzt, wenn Fehler ignoriert werden sollen.</param>
        private void ForEachProfile( Action<ProfileState> method, bool ignoreErrors = false )
        {
            // Forward to all
            foreach (var state in m_profiles.Values)
                try
                {
                    // Forward
                    method( state );
                }
                catch (Exception e)
                {
                    // Report
                    VCRServer.Log( e );

                    // See if we are allowed to ignore
                    if (!ignoreErrors)
                        throw;
                }
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung der zugehörigen Geräteprofile.
        /// </summary>
        public void Dispose()
        {
            // Load plan thread
            var planThread = m_planThread;

            // Full disable planner - make sure that order or lock is compatible with other scenarios
            lock (m_planner)
                lock (m_newPlanSync)
                {
                    // Full deactivation
                    m_plannerActive = false;
                    m_planThread = null;

                    // Wake up call
                    Monitor.Pulse( m_newPlanSync );
                }

            // Wait for thread to end
            if (planThread != null)
                planThread.Join();

            // Release hibernation lock
            AllowHibernation();

            // Forget timer
            using (m_timer)
                m_timer = null;

            // Forget planner
            using (m_planner)
                m_planner = null;

            // Be safe
            try
            {
                // Forward
                ForEachProfile( state => state.Dispose(), true );
            }
            finally
            {
                // Forget
                m_profiles.Clear();
            }
        }

        #endregion

        #region Schlafzustand

        /// <summary>
        /// Bereitet den Übergang in den Schlafzustand vor.
        /// </summary>
        public void PrepareSuspend()
        {
            // Report
            Tools.ExtendedLogging( "Notifying all Profiles of upcoming Suspend" );

            // Disallow starting new recordings
            var planner = m_planner;
            if (planner != null)
                lock (planner)
                    m_plannerActive = false;

            // Forward
            ForEachProfile( state => state.PrepareSuspend(), true );
        }

        /// <summary>
        /// Führt den Übergang in den Schlafzustand durch.
        /// </summary>
        public void Suspend()
        {
            // Report
            Tools.ExtendedLogging( "Suspending all Profiles" );

            // Forward
            ForEachProfile( state => state.Suspend(), true );
        }

        /// <summary>
        /// Reaktiviert die Planung der Aufzeichnungen nach der Rückkehr aus dem Schlafzustand.
        /// </summary>
        public void Resume()
        {
            // Report
            Tools.ExtendedLogging( "Resuming Recording Planner" );

            // Reactivate
            var planner = m_planner;
            if (planner != null)
                lock (planner)
                    m_plannerActive = true;
        }

        #endregion

        #region Die Aufzeichnungsplanung und die zugehörige Infrastruktur

        /// <summary>
        /// Die zugehörige Aufzeichnungsplanung über alle Geräteprofile hinweg.
        /// </summary>
        private RecordingPlanner m_planner;

        /// <summary>
        /// Gesetzt, wenn die Aufzeichnungsplanung aktiv ist.
        /// </summary>
        private bool m_plannerActive = true;

        /// <summary>
        /// Erstellt einen neuen Wartezähler.
        /// </summary>
        private WaitableTimer m_timer = new WaitableTimer();

        /// <summary>
        /// Die aktuell ausstehende Operation.
        /// </summary>
        private IScheduleInformation m_pendingSchedule;

        /// <summary>
        /// Gesetzt, wenn auf das Starten einer Aufzeichnung gewartet wird.
        /// </summary>
        private bool m_pendingStart;

        /// <summary>
        /// Verhindert den Übergang in den Schlafzustand.
        /// </summary>
        private IDisposable m_hibernationBlocker;

        /// <summary>
        /// Aktualisiert ständig die Planung.
        /// </summary>
        private volatile Thread m_planThread;

        /// <summary>
        /// Gesetzt, sobald die Berechnung eines neues Plans erwünscht wird.
        /// </summary>
        private volatile bool m_newPlan = true;

        /// <summary>
        /// Synchronisiert den Zugriff auf die Planungsbefehle.
        /// </summary>
        private readonly object m_newPlanSync = new object();

        /// <summary>
        /// Synchronisiert die Freigabe auf Planungsbefehle.
        /// </summary>
        private readonly object m_planAvailableSync = new object();

        /// <summary>
        /// Sammelt Startvorgänge.
        /// </summary>
        private Action m_pendingActions;

        /// <summary>
        /// Aktualisiert ständig die Planung.
        /// </summary>
        private void PlanThread()
        {
            // Always loop
            while (m_planThread != null)
            {
                // See if a new plan is requested
                lock (m_newPlanSync)
                    while (!m_newPlan)
                        if (m_planThread == null)
                            return;
                        else
                            Monitor.Wait( m_newPlanSync );

                // At least we accepted the request
                m_newPlan = false;

                // See if we still have a planner
                var planner = m_planner;
                if (planner == null)
                    break;

                // Just take a look what to do next
                using (PowerManager.StartForbidHibernation())
                    try
                    {
                        // Reset start actions
                        m_pendingActions = null;

                        // Protect planning.
                        lock (planner)
                        {
                            // Report
                            Tools.ExtendedLogging( "Woke up for Plan Calculation at {0}", DateTime.Now );

                            // Retest
                            if (m_planThread == null)
                                break;

                            // See if we are allowed to take the next step in plan - we schedule only one activity at a time
                            if (m_pendingSchedule == null)
                            {
                                // Release our lock
                                AllowHibernation();

                                // Reset timer
                                if (PowerManager.IsSuspended && VCRConfiguration.Current.SuppressDelayAfterForcedHibernation)
                                    Tools.ExtendedLogging( "VCR.NET is suspending - not updating Timer" );
                                else
                                    m_timer.SecondsToWait = 0;

                                // Analyse plan
                                planner.DispatchNextActivity( DateTime.UtcNow );

                                // If we are shutting down better forget anything we did in the previous step - only timer setting matters!
                                if (!m_plannerActive)
                                {
                                    // Reset to initial state
                                    m_pendingSchedule = null;
                                    m_pendingActions = null;
                                    m_pendingStart = false;

                                    // And forget all allocations
                                    planner.Reset();

                                    // Just in case allow hibernation again
                                    AllowHibernation();
                                }
                            }
                        }

                        // Run start and stop actions outside the planning lock (to avoid deadlocks) but inside the hibernation protection (to forbid hibernation)
                        var toStart = m_pendingActions;
                        if (toStart != null)
                            toStart();
                    }
                    catch (Exception e)
                    {
                        // Report and ignore - we do not expect any error to occur
                        VCRServer.Log( e );
                    }

                // New plan is now available - beside termination this will do nothing at all but briefly aquiring an idle lock
                lock (m_planAvailableSync)
                    Monitor.PulseAll( m_planAvailableSync );
            }
        }

        /// <summary>
        /// Ermittelt den aktuellen Ablaufplan.
        /// </summary>
        /// <returns>Alle Einträge des aktuellen Plans, maximal aber <i>1000</i>.</returns>
        public PlanContext GetPlan()
        {
            // See if we are still up and running
            var planner = m_planner;
            if (planner == null)
                return new PlanContext( null );

            // Ensure proper synchronisation with planning thread
            lock (planner)
                return planner.GetPlan( DateTime.UtcNow );
        }

        /// <summary>
        /// Aktualisiert den Aufzeichnungsplan.
        /// </summary>
        public void BeginNewPlan()
        {
            // Check flag
            if (m_newPlan)
                return;

            // Synchronize
            lock (m_newPlanSync)
            {
                // Test again - this may avoid some wake ups of the planning thread
                if (m_newPlan)
                    return;

                // Set it
                m_newPlan = true;

                // Fire
                Monitor.Pulse( m_newPlanSync );
            }
        }

        /// <summary>
        /// Fordert eine Aktualisierung an und berechnet einen neuen Plan.
        /// </summary>
        public void EnsureNewPlan()
        {
            // Make sure that we use an up do date plan
            lock (m_planAvailableSync)
                for (int i = 2; i-- > 0;)
                {
                    // Enforce calculation
                    BeginNewPlan();

                    // Wait
                    Monitor.Wait( m_planAvailableSync );
                }
        }

        /// <summary>
        /// Verbietet den Übergang in den Schlafzustand. Der Aufruf hält bereits eine geeignete Sperre.
        /// </summary>
        private void ForbidHibernation()
        {
            // Forbid hibernation
            using (m_hibernationBlocker)
                m_hibernationBlocker = PowerManager.StartForbidHibernation();
        }

        /// <summary>
        /// Erlaubt den Übergang in den Schlafzustand. Der Aufruf hält bereits eine geeignete Sperre.
        /// </summary>
        private void AllowHibernation()
        {
            // Allow hibernation if it has been forbidden
            using (m_hibernationBlocker)
                m_hibernationBlocker = null;
        }

        /// <summary>
        /// Verändert den Endzeitpunkt einer Aufzeichnung.
        /// </summary>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
        /// <param name="newEndTime">Der gewünschte Verschiebung des Endzeitpunkts.</param>
        /// <returns>Gesetzt, wenn die Änderung möglich war.</returns>
        public bool ChangeEndTime( Guid scheduleIdentifier, DateTime newEndTime )
        {
            // See if planner exists
            var planner = m_planner;
            if (planner == null)
                return false;

            // While forwarding make sure that we don't interfere with the planning thread
            lock (planner)
                if (!planner.SetEndTime( scheduleIdentifier, newEndTime ))
                    return false;

            // Spawn new check on schedules
            BeginNewPlan();

            // Report that we did it
            return true;
        }

        /// <summary>
        /// Bestätigt den Abschluss einer Operation und die Bereitschaft, die nächste Operation 
        /// zu starten.
        /// </summary>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der Operation.</param>
        /// <param name="isStart">Gesetzt, wenn es sich um einen Startbefehl handelt.</param>
        internal void ConfirmOperation( Guid scheduleIdentifier, bool isStart )
        {
            // Protect and check
            var planner = m_planner;
            if (planner == null)
                return;

            // Make sure that we synchronize with the planning thread
            lock (planner)
                if (m_pendingSchedule == null)
                {
                    // Report
                    VCRServer.Log( LoggingLevel.Errors, "There is no outstanding asynchronous Recording Request for Schedule '{0}'", scheduleIdentifier );
                }
                else if (m_pendingSchedule.Definition.UniqueIdentifier != scheduleIdentifier)
                {
                    // Report
                    VCRServer.Log( LoggingLevel.Errors, "Confirmed asynchronous Recording Request for Schedule '{0}' but waiting for '{1}'", scheduleIdentifier, m_pendingSchedule.Definition.UniqueIdentifier );
                }
                else
                {
                    // Report
                    VCRServer.Log( LoggingLevel.Schedules, "Confirmed asynchronous Recording Request for Schedule '{0}'", scheduleIdentifier );

                    // Check mode
                    if (isStart != m_pendingStart)
                        VCRServer.Log( LoggingLevel.Errors, "Recording Request confirmed wrong Type of Operation" );

                    // Finish
                    if (m_pendingStart)
                        planner.Start( m_pendingSchedule );
                    else
                        planner.Stop( scheduleIdentifier );

                    // Reset
                    m_pendingSchedule = null;
                }

            // See what to do next
            BeginNewPlan();
        }

        /// <summary>
        /// Ermittelt den Zeitpunkt, an dem zum nächsten Mal eine Aufzeichnung stattfinden soll.
        /// </summary>
        public DateTime? NextRecordingStart { get { return m_timer.NextEventTime; } }

        #endregion

        #region Verarbeitung des nächsten Arbeitsschrittes der Aufzeichnungsplanung

        /// <summary>
        /// Der volle Pfad zu dem Regeldatei der Aufzeichnungsplanung.
        /// </summary>
        public string ScheduleRulesPath { get { return Path.Combine( Tools.ApplicationDirectory.FullName, "SchedulerRules.cmp" ); } }

        /// <summary>
        /// Meldet die Namen der verwendeten Geräteprofile.
        /// </summary>
        IEnumerable<string> IRecordingPlannerSite.ProfileNames { get { return m_profiles.Keys; } }

        /// <summary>
        /// Erstellt einen periodischen Auftrag für die Aktualisierung der Programmzeitschrift.
        /// </summary>
        /// <param name="resource">Die zu verwendende Ressource.</param>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        /// <returns>Der gewünschte Auftrag.</returns>
        PeriodicScheduler IRecordingPlannerSite.CreateProgramGuideTask( IScheduleResource resource, Profile profile )
        {
            // Protect against misuse
            if (m_profiles.TryGetValue(profile.Name, out ProfileState state))
                return new ProgramGuideTask(resource, state);
            else
                return null;
        }

        /// <summary>
        /// Erstellt einen periodischen Auftrag für die Aktualisierung der Liste der Quellen.
        /// </summary>
        /// <param name="resource">Die zu verwendende Ressource.</param>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        /// <returns>Der gewünschte Auftrag.</returns>
        PeriodicScheduler IRecordingPlannerSite.CreateSourceScanTask( IScheduleResource resource, Profile profile )
        {
            // Protect against misuse
            if (m_profiles.TryGetValue(profile.Name, out ProfileState state))
                return new SourceListTask(resource, state);
            else
                return null;
        }

        /// <summary>
        /// Ergänzt alle bekannten Aufzeichnungen zu einer Planungsinstzanz.
        /// </summary>
        /// <param name="scheduler">Die Verwaltung der Aufzeichnungen.</param>
        /// <param name="disabled">Alle nicht zu verwendenden Aufzeichnungen.</param>
        /// <param name="planner">Die Gesamtplanung.</param>
        /// <param name="context">Zusätzliche Informationen zur aktuellen Planung.</param>
        void IRecordingPlannerSite.AddRegularJobs( RecordingScheduler scheduler, Func<Guid, bool> disabled, RecordingPlanner planner, PlanContext context )
        {
            // Retrieve all jobs related to this profile
            foreach (var job in Server.JobManager.GetActiveJobs())
                foreach (var schedule in job.Schedules)
                {
                    // No longer in use
                    if (!schedule.IsActive)
                        continue;

                    // Resolve source
                    var source = schedule.Source ?? job.Source;
                    if (source == null)
                        continue;

                    // Resolve profile
                    var resource = planner.GetResourceForProfile( source.ProfileName );
                    if (resource == null)
                        continue;

                    // Register single item
                    schedule.AddToScheduler( scheduler, job, new[] { resource }, VCRProfiles.FindSource, disabled, context );

                    // Remember - even if we skipped it
                    context.RegisterSchedule( schedule, job );
                }
        }

        /// <summary>
        /// Es sind keine weiteren Aktionen notwendig.
        /// </summary>
        /// <param name="until">Zu diesem Zeitpunkt soll die nächste Prüfung stattfinden.</param>
        void IRecordingPlannerSite.Idle( DateTime until )
        {
            // Get the wait time
            var secondsToWait = Math.Max( (until - DateTime.UtcNow).TotalSeconds, 0.0 );

            // Want to respect the hibernation delay - in case we stay online nothing much happens but one additional plan check
            var startupDelay = VCRConfiguration.Current.HibernationDelay;
            if (secondsToWait > startupDelay)
                secondsToWait -= startupDelay;

            // If we are activated inside the grace period try to forbid hibernation - in case we are suspending we are not allowed to wake up early!
            var minimumSleep = VCRConfiguration.Current.DelayAfterForcedHibernation.TotalSeconds;
            var suspending = PowerManager.IsSuspended;
            if (secondsToWait <= minimumSleep)
                if (suspending)
                    if (VCRConfiguration.Current.SuppressDelayAfterForcedHibernation)
                        Tools.ExtendedLogging( "Forced Hibernation Delay is disabled" );
                    else
                        secondsToWait = minimumSleep;
                else
                    ForbidHibernation();

            // Activate timer
            if (suspending && VCRConfiguration.Current.SuppressDelayAfterForcedHibernation)
                Tools.ExtendedLogging( "VCR.NET is suspending - Timer will not be changed" );
            else
                m_timer.SecondsToWait = secondsToWait + 1.0;

            // Report
            var waitEnd = m_timer.NextEventTime;
            if (waitEnd.HasValue)
                Tools.ExtendedLogging( "Do nothing until {0}", waitEnd.Value.ToLocalTime() );
            else
                Tools.ExtendedLogging( "Quite unexpected but it seems as if we are fully idle" );
        }

        /// <summary>
        /// Meldet, dass eine Aufzeichnung nicht ausgeführt werden kann.
        /// </summary>
        /// <param name="item">Die nicht ausgeführte Aufzeichnung.</param>
        void IRecordingPlannerSite.Discard( IScheduleDefinition item )
        {
            // Report
            VCRServer.Log( LoggingLevel.Schedules, "Could not record '{0}'", item.Name );
        }

        /// <summary>
        /// Meldet, dass eine Aufzeichnung nun beendet werden kann.
        /// </summary>
        /// <param name="item">Die betroffene Aufzeichnung.</param>
        /// <param name="planner">Die Planungsinstanz.</param>
        void IRecordingPlannerSite.Stop( IScheduleInformation item, RecordingPlanner planner )
        {
            // Report
            VCRServer.Log( LoggingLevel.Schedules, "Done recording '{0}'", item.Definition.Name );

            // Locate the profile - if we don't find it we are in big trouble!
            if (!m_profiles.TryGetValue(item.Resource.Name, out ProfileState profile))
                return;

            // Mark as pending
            m_pendingSchedule = item;
            m_pendingStart = false;

            // Forward request to profile manager
            m_pendingActions += () => profile.EndRecording( item.Definition.UniqueIdentifier );
        }

        /// <summary>
        /// Meldet, dass eine Aufzeichnung nun beginnen sollte.
        /// </summary>
        /// <param name="item">Die zu startende Aufzeichnung.</param>
        /// <param name="planner">Die Planungsinstanz.</param>
        /// <param name="context">Zusatzinformationen zur Aufzeichnungsplanung.</param>
        void IRecordingPlannerSite.Start( IScheduleInformation item, RecordingPlanner planner, PlanContext context )
        {
            // We are no longer active - simulate start and do nothing
            if (!m_plannerActive)
            {
                // Make planner believe we did it
                planner.Start( item );

                // Make sure that we wake up after the grace period
                if (PowerManager.IsSuspended && VCRConfiguration.Current.SuppressDelayAfterForcedHibernation)
                    Tools.ExtendedLogging( "Hibernation Delay is disabled and can not be enforced" );
                else
                    m_timer.SecondsToWait = VCRConfiguration.Current.DelayAfterForcedHibernation.TotalSeconds;

                // Done
                return;
            }

            // Report
            VCRServer.Log( LoggingLevel.Schedules, "Start recording '{0}'", item.Definition.Name );

            // Locate the profile - if we don't find it we are in big trouble!
            if (!m_profiles.TryGetValue(item.Resource.Name, out ProfileState profile))
                return;

            // Mark as pending
            m_pendingSchedule = item;
            m_pendingStart = true;

            // Create the recording
            var recording = VCRRecordingInfo.Create( item, context );

            // Check for EPG
            var guideUpdate = item.Definition as ProgramGuideTask;
            if (guideUpdate != null)
            {
                // Start a new guide collector
                m_pendingActions += ProgramGuideProxy.Create( profile, recording ).Start;
            }
            else
            {
                // Check for PSI
                var sourceUpdate = item.Definition as SourceListTask;
                if (sourceUpdate != null)
                {
                    // Start a new update
                    m_pendingActions += SourceScanProxy.Create( profile, recording ).Start;
                }
                else
                {
                    // Start a regular recording - profile will decide if to join an existing recording
                    m_pendingActions += () => profile.StartRecording( recording );
                }
            }
        }

        #endregion
    }
}
