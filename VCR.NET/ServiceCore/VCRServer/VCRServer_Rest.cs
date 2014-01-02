using System;
using System.Collections.Generic;
using System.Linq;
using JMS.DVB;
using JMS.DVB.Algorithms.Scheduler;
using JMS.DVBVCR.RecordingService.Persistence;
using JMS.DVBVCR.RecordingService.Planning;
using JMS.DVBVCR.RecordingService.ProgramGuide;
using JMS.DVBVCR.RecordingService.Status;


namespace JMS.DVBVCR.RecordingService
{
    partial class VCRServer
    {
        /// <summary>
        /// Ermittelt den aktuellen Aufzeichnungsplan.
        /// </summary>
        /// <typeparam name="TActivity">Die Art der Information für einen Eintrag der Planung.</typeparam>
        /// <param name="end">Es werden nur Aufzeichnungen betrachtet, die nicht nach diesem Zeitpunkt starten.</param>
        /// <param name="limit">Die maximale Anzahl von Ergebniszeilen.</param>
        /// <param name="factory">Methode zum Erstellen einer einzelnen Planungsinformation.</param>
        /// <returns>Der gewünschte Aufzeichnungsplan.</returns>
        public TActivity[] GetPlan<TActivity>( DateTime end, int limit, Func<IScheduleInformation, PlanContext, ProfileStateCollection, TActivity> factory )
        {
            // Result
            var activities = new List<TActivity>();

            // Resulting mapping
            var context = Profiles.GetPlan();

            // Load list
            foreach (var schedule in context)
            {
                // See if we reached the very end
                if (schedule.Time.Start >= end)
                    break;

                // Create
                var activity = factory( schedule, context, Profiles );
                if (activity == null)
                    continue;

                // Register
                activities.Add( activity );

                // Check limit
                if (activities.Count >= limit)
                    break;
            }

            // Report
            return activities.ToArray();
        }

        /// <summary>
        /// Ermittelt den passendsten Eintrag aus der Programmzeitschrift.
        /// </summary>
        /// <typeparam name="TTarget">Die Art der Rückgabewerte.</typeparam>
        /// <param name="profileName">Das zu betrachtende Geräteprofil.</param>
        /// <param name="source">Die zugehörige Quelle.</param>
        /// <param name="from">Der Beginn eines Zeitraums.</param>
        /// <param name="to">Das Ende eines Zeitraums.</param>
        /// <param name="factory">Name der Metjode zum Erzeugen eines externen Eintrags.</param>
        /// <returns>Der am besten passende Eintrag.</returns>
        public TTarget FindProgramGuideEntry<TTarget>( string profileName, SourceIdentifier source, DateTime from, DateTime to, Func<ProgramGuideEntry, string, TTarget> factory )
        {
            // See if profile exists
            var profile = Profiles[profileName];
            if (profile == null)
                return default( TTarget );
            else
                return profile.ProgramGuide.FindBestEntry( source, from, to, factory );
        }

        /// <summary>
        /// Ermittelt einen bestimmten Eintrag.
        /// </summary>
        /// <param name="profileName">Das zu betrachtende Geräteprofil.</param>
        /// <param name="source">Die Quelle, deren Eintrag ermittelt werden soll.</param>
        /// <param name="start">Der exakte Startzeitpunkt.</param>
        /// <returns>Der gewünschte Eintrag.</returns>
        public ProgramGuideEntry FindProgramGuideEntry( string profileName, SourceIdentifier source, DateTime start )
        {
            // See if profile exists
            var profile = Profiles[profileName];
            if (profile == null)
                return null;
            else
                return profile.ProgramGuide.FindEntry( source, start );
        }

        /// <summary>
        /// Verändert eine Ausnahme.
        /// </summary>
        /// <param name="jobIdentifier">Die eindeutige Kennung des Auftrags.</param>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
        /// <param name="when">Der betroffene Tag.</param>
        /// <param name="startDelta">Die Verschiebung der Startzeit in Minuten.</param>
        /// <param name="durationDelta">Die Änderung der Aufzeichnungsdauer in Minuten.</param>
        public void ChangeException( Guid jobIdentifier, Guid scheduleIdentifier, DateTime when, int startDelta, int durationDelta )
        {
            // Locate the job
            var job = JobManager[jobIdentifier];
            if (job == null)
                return;
            var schedule = job[scheduleIdentifier];
            if (schedule == null)
                return;

            // Validate
            if (durationDelta < -schedule.Duration)
                return;

            // Create description
            var exception = new VCRScheduleException { When = when.Date };

            // Fill all data 
            if (startDelta != 0)
                exception.ShiftTime = startDelta;
            if (durationDelta != 0)
                exception.Duration = schedule.Duration + durationDelta;

            // Process
            schedule.SetException( exception.When, exception );

            // Store
            JobManager.Update( job, null );

            // Recalculate plan
            BeginNewPlan();
        }

        /// <summary>
        /// Meldet Informationen zu allen Geräteprofilen.
        /// </summary>
        /// <typeparam name="TInfo">Die Art der gemeldeten Information.</typeparam>
        /// <param name="factory">Methode zum Erzeugen der Informationen zu einem einzelnen Geräteprofil.</param>
        /// <returns>Die Informationen zu den Profilen.</returns>
        public TInfo[] GetProfiles<TInfo>( Func<ProfileState, TInfo> factory )
        {
            // Forward
            return Profiles.InspectProfiles( factory ).ToArray();
        }

        /// <summary>
        /// Meldet alle Aufträge.
        /// </summary>
        /// <typeparam name="TJob">Die Art der externen Darstellung.</typeparam>
        /// <param name="factory">Methode zum Erstellen der externen Darstellung.</param>
        /// <returns>Die Liste der Aufträge.</returns>
        public TJob[] GetJobs<TJob>( Func<VCRJob, bool, TJob> factory )
        {
            // Report
            return
                JobManager
                    .GetActiveJobs()
                    .Select( job => factory( job, true ) )
                    .Concat( JobManager.ArchivedJobs.Select( job => factory( job, false ) ) )
                    .ToArray();
        }

        /// <summary>
        /// Ermittelt eine Übersicht über die aktuellen und anstehenden Aufzeichnungen
        /// aller Geräteprofile.
        /// </summary>
        /// <typeparam name="TInfo">Die Art der Informationen.</typeparam>
        /// <param name="fromActive">Erstellt eine Liste von Beschreibungen zu einer aktuellen Aufzeichnung.</param>
        /// <param name="fromPlan">Erstellt eine einzelne Beschreibung zu einer Aufzeichnung aus dem Aufzeichnungsplan.</param>
        /// <param name="forIdle">Erstellt eine Beschreibung für ein Gerät, für das keine Aufzeichnungen geplant sind.</param>
        /// <returns>Die Liste aller Informationen.</returns>
        public TInfo[] GetCurrentRecordings<TInfo>( Func<FullInfo, VCRServer, TInfo[]> fromActive, Func<IScheduleInformation, PlanContext, VCRServer, TInfo> fromPlan, Func<string, TInfo> forIdle )
        {
            // Validate
            if (fromActive == null)
                throw new ArgumentNullException( "fromActive" );
            if (fromPlan == null)
                throw new ArgumentNullException( "fromPlan" );

            // All profile we know
            var idleProfiles = new HashSet<string>( Profiles.InspectProfiles( profile => profile.ProfileName ), ProfileManager.ProfileNameComparer );

            // Collect per profile
            var perProfile =
                Profiles
                    .InspectProfiles( profile => profile.CurrentRecording )
                    .Where( current => current != null )
                    .ToDictionary( current => current.Recording.Source.ProfileName, current => fromActive( current, this ), idleProfiles.Comparer );

            // Check for idle profiles
            idleProfiles.ExceptWith( perProfile.Keys );

            // There are any
            if (idleProfiles.Count > 0)
            {
                // Attach to plan - will be internally limited to some reasonable count
                var context = Profiles.GetPlan();

                // Parse plan
                foreach (var schedule in context)
                {
                    // Will not be executed
                    var resource = schedule.Resource;
                    if (resource == null)
                        continue;

                    // Is not a regular recording
                    if (!(schedule.Definition is IScheduleDefinition<VCRSchedule>))
                        continue;

                    // See if this is one of our outstanding profiles
                    var profileName = resource.Name;
                    if (!idleProfiles.Remove( profileName ))
                        continue;

                    // Add entry
                    perProfile.Add( profileName, new[] { fromPlan( schedule, context, this ) } );

                    // Did it
                    if (idleProfiles.Count < 1)
                        break;
                }

                // Idle stuff
                foreach (var idleProfile in idleProfiles)
                    perProfile.Add( idleProfile, new[] { forIdle( idleProfile ) } );
            }

            // Report
            return perProfile.SelectMany( info => info.Value ).ToArray();
        }

        /// <summary>
        /// Ermittelt einen Auszug aus der Programmzeitschrift.
        /// </summary>
        /// <typeparam name="TFilter">Die Art des Filters.</typeparam>
        /// <typeparam name="TEntry">Die Art der externen Darstellung von Einträgen.</typeparam>
        /// <param name="filter">Der Filter in der externen Darstellung.</param>
        /// <param name="filterConverter">Methode zur Wandlung des Filters in die interne Darstellung.</param>
        /// <param name="factory">Erstellt die externe Repräsentation eines Eintrags.</param>
        /// <returns>Die Liste aller passenden Einträge.</returns>
        public TEntry[] GetProgramGuideEntries<TFilter, TEntry>( TFilter filter, Func<TFilter, GuideEntryFilter> filterConverter, Func<ProgramGuideEntry, string, TEntry> factory ) where TFilter : class
        {
            // Validate filter
            if (filter == null)
                return new TEntry[0];

            // Convert filter
            var filterIntern = filterConverter( filter );

            // Locate profile and forward call
            var profileName = filterIntern.ProfileName;
            if (string.IsNullOrEmpty( profileName ))
                return new TEntry[0];
            var profile = FindProfile( profileName );
            if (profile == null)
                return new TEntry[0];
            else
                return profile.ProgramGuide.GetProgramGuideEntries( filterIntern, factory );
        }

        /// <summary>
        /// Ermittelt einen Auszug aus der Programmzeitschrift.
        /// </summary>
        /// <typeparam name="TFilter">Die Art des Filters.</typeparam>
        /// <param name="filter">Der Filter in der externen Darstellung.</param>
        /// <param name="filterConverter">Methode zur Wandlung des Filters in die interne Darstellung.</param>
        /// <returns>Die Anzahl der passenden Einträge.</returns>
        public int GetProgramGuideEntries<TFilter>( TFilter filter, Func<TFilter, GuideEntryFilter> filterConverter ) where TFilter : class
        {
            // Validate filter
            if (filter == null)
                return 0;

            // Convert filter
            var filterIntern = filterConverter( filter );

            // Locate profile and forward call
            var profileName = filterIntern.ProfileName;
            if (string.IsNullOrEmpty( profileName ))
                return 0;
            var profile = FindProfile( profileName );
            if (profile == null)
                return 0;
            else
                return profile.ProgramGuide.GetProgramGuideEntries( filterIntern );
        }

        /// <summary>
        /// Ermittelt die Eckdaten zu den Eintragungen eines Geräteprofils.
        /// </summary>
        /// <typeparam name="TInfo">Die Art der Informationen.</typeparam>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <param name="factory">Methode zur Erstellung der Informationen.</param>
        /// <returns>Die gewünschten Informationen.</returns>
        public TInfo GetProgramGuideInformation<TInfo>( string profileName, Func<ProgramGuideManager, TInfo> factory )
        {
            // Locate profile and forward call
            if (string.IsNullOrEmpty( profileName ))
                return default( TInfo );
            var profile = FindProfile( profileName );
            if (profile == null)
                return default( TInfo );
            else
                return factory( profile.ProgramGuide );
        }

        /// <summary>
        /// Ermittelt alle Geräteprofile.
        /// </summary>
        /// <typeparam name="TProfile">Die Art der Zielinformation.</typeparam>
        /// <param name="factory">Methode zum Erstellen der Zielinformation.</param>
        /// <param name="defaultProfile">Der Name des bevorzugten Geräteprofils.</param>
        /// <returns>Die gewünschte Liste.</returns>
        public TProfile[] GetProfiles<TProfile>( Func<Profile, bool, TProfile> factory, out string defaultProfile )
        {
            // Create map            
            var profiles = VCRProfiles.ProfileNames.ToArray();
            var active = new HashSet<string>( profiles, ProfileManager.ProfileNameComparer );

            // Set default
            defaultProfile = profiles.FirstOrDefault();

            // From DVB.NET
            return ProfileManager.AllProfiles.Select( profile => factory( profile, active.Contains( profile.Name ) ) ).ToArray();
        }

        /// <summary>
        /// Aktualisiert die Daten von Geräteprofilen.
        /// </summary>
        /// <typeparam name="TProfile">Die Art der Daten.</typeparam>
        /// <param name="profiles">Die Liste der Geräteprofile.</param>
        /// <param name="getName">Methode zum Auslesen des Profilnames.</param>
        /// <param name="updater">Die Aktualisierungsmethode.</param>
        /// <returns>Gesetzt, wenn eine Änderung durchgeführt wurde.</returns>
        public bool UpdateProfiles<TProfile>( TProfile[] profiles, Func<TProfile, string> getName, Func<TProfile, Profile, bool> updater )
        {
            // Result
            var changed = false;

            // Process
            Array.ForEach( profiles, profile => changed |= updater( profile, ProfileManager.FindProfile( getName( profile ) ) ) );

            // Report
            return changed;
        }

        /// <summary>
        /// Liest einen Auszug aus einem Protokoll.
        /// </summary>
        /// <typeparam name="TEntry">Die Art der Zielinformation.</typeparam>
        /// <param name="profileName">Der Name des betroffenen Geräteprofils.</param>
        /// <param name="start">Das Startdatum.</param>
        /// <param name="end">Das Enddatum.</param>
        /// <param name="factory">Methode zum Erzeugen der externen Darstellung aus den Protokolleinträgen.</param>
        /// <returns>Die angeforderten Protokolleinträge.</returns>
        public TEntry[] QueryLog<TEntry>( string profileName, DateTime start, DateTime end, Func<VCRRecordingInfo, TEntry> factory )
        {
            // Locate profile and forward call
            if (string.IsNullOrEmpty( profileName ))
                return new TEntry[0];
            var profile = FindProfile( profileName );
            if (profile == null)
                return new TEntry[0];
            else
                return JobManager.FindLogEntries( start, end, profile ).Select( factory ).ToArray();
        }
    }
}

