using JMS.DVB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace JMS.DVBVCR.RecordingService.ProgramGuide
{
    /// <summary>
    /// Verwaltet die elektronischen Programmzeitschrift von VCR.NET.
    /// </summary>
    public class ProgramGuideManager
    {
        /// <summary>
        /// Die zugehörige Auftragsverwaltung.
        /// </summary>
        public JobManager JobManager { get; }

        /// <summary>
        /// Der Name des zugehörigen Geräteprofils.
        /// </summary>
        public string ProfileName { get; }

        /// <summary>
        /// Meldet die zugehörige Datei mit den Daten der Programmzeitschrift.
        /// </summary>
        public FileInfo ProgramGuideFile { get; }

        /// <summary>
        /// Die Daten der Programmzeitschrift.
        /// </summary>
        private volatile ProgramGuideEntries m_Events = new ProgramGuideEntries();

        /// <summary>
        /// Erzeugt eine neue Verwaltungsinstanz.
        /// </summary>
        /// <param name="jobs">Die zugehörige Auftragsverwaltung.</param>
        /// <param name="profileName">Der Name des verwalteten DVB.NET Geräteprofils.</param>
        public ProgramGuideManager( JobManager jobs, string profileName )
        {
            // Remember
            ProfileName = profileName;
            JobManager = jobs;

            // Calculate file
            ProgramGuideFile = new FileInfo( Path.Combine( JobManager.CollectorDirectory.FullName, $"EPGData for {ProfileName}.xml" ) );

            // See if profile has it's own program guide
            if (!HasProgramGuide)
                return;

            // Report
            Tools.ExtendedLogging( "Looking for Program Guide of {0}", ProfileName );

            // No such file - start empty
            if (!ProgramGuideFile.Exists)
                return;

            // Process
            var events = SerializationTools.Load<ProgramGuideEntries>( ProgramGuideFile );
            if (events != null)
            {
                // Use it
                m_Events = events;

                // Report
                Tools.ExtendedLogging( "Found valid Program Guide and using it" );

                // Done
                return;
            }

            // Report
            VCRServer.Log( LoggingLevel.Errors, Properties.Resources.EPGFileCorrupted, ProgramGuideFile.FullName );

            // Save delete
            try
            {
                // Process
                ProgramGuideFile.Delete();
            }
            catch
            {
                // Discard any error
                VCRServer.Log( LoggingLevel.Errors, Properties.Resources.EPGDeleteDenied, ProgramGuideFile.FullName );
            }
        }

        /// <summary>
        /// Ermittelt das Geräteprofil, das die elektronische Programmzeitschrift für dieses
        /// Profil zur Verfügung stellt.
        /// </summary>
        public Profile LeafGuideProfile
        {
            get
            {
                // Attach to us
                var me = Profile;

                // Cycle protection
                var done = new Dictionary<string, Profile>( ProfileManager.ProfileNameComparer );

                // Process using full profile chain
                for (var test = me; ; test = ProfileManager.FindProfile( test.UseSourcesFrom ))
                {
                    // None existing
                    if (null == test)
                        return me;

                    // Cycle
                    if (done.ContainsKey( test.Name ))
                        return me;

                    // See if this is active
                    if (VCRProfiles.FindProfile( test.Name ) != null)
                        if (string.IsNullOrEmpty( test.UseSourcesFrom ))
                            return test;

                    // Remember
                    done[test.Name] = test;
                }
            }
        }

        /// <summary>
        /// Meldet, ob zu diesem Geräteprofil eine Programmzeitschrift geführt wird.
        /// </summary>
        public bool HasProgramGuide
        {
            get
            {
                // Attach to the profile
                var profile = LeafGuideProfile;
                if (null == profile)
                    return false;
                else
                    return (string.Compare( profile.Name, ProfileName, true ) == 0);
            }
        }

        /// <summary>
        /// Meldet das zugehörige Geräteprofil.
        /// </summary>
        public Profile Profile => VCRProfiles.FindProfile( ProfileName );

        /// <summary>
        /// Meldet den Namen des Wertes in der Registrierung von Windows, wo der Zeitpunkt
        /// der letzten Ausführung gespeichert wird.
        /// </summary>
        private string UpdateRegistryName => $"LastEPGRun {ProfileName}";

        /// <summary>
        /// Liest oder setzt den Zeitpunkt der letzen Aktualisierung der Programmzeitschrift.
        /// </summary>
        internal DateTime? LastUpdateTime
        {
            get { return Tools.GetRegistryTime( UpdateRegistryName ); }
            set { Tools.SetRegistryTime( UpdateRegistryName, value ); }
        }

        /// <summary>
        /// Aktualisiert die Programmzeitschrift mit neuen Daten.
        /// </summary>
        /// <param name="entries">Die neuen Daten.</param>
        internal void UpdateGuide( ProgramGuideEntries entries )
        {
            // Report
            Tools.ExtendedLogging( "Program Guide of {0} will be updated", ProfileName );

            // Did collection
            LastUpdateTime = DateTime.UtcNow;

            // Try to load resulting file
            try
            {
                // Create brand new
                var newData = m_Events.Clone();

                // Merge new
                newData.Merge( entries );

                // Cleanup
                newData.DiscardOld();

                // Save
                SerializationTools.Save( newData, ProgramGuideFile, Encoding.UTF8 );

                // Use it
                m_Events = newData;

                // Report
                Tools.ExtendedLogging( "Now using new Program Guide" );
            }
            catch (Exception e)
            {
                // Report
                VCRServer.Log( LoggingLevel.Errors, Properties.Resources.EPGMergeFailed, e );
            }
        }

        /// <summary>
        /// Prüft, ob für den gewählten Zeitraum ein Eintrag existiert.
        /// </summary>
        /// <param name="source">Die Quelle, deren Einträge untersucht werden sollen.</param>
        /// <param name="start">Der Beginn des Zeitraums (einschließlich).</param>
        /// <param name="end">Das Ende des Zeitraums (ausschließlich).</param>
        /// <returns>Gesetzt, wenn ein Eintrag existiert.</returns>
        public bool HasEntry( SourceIdentifier source, DateTime start, DateTime end )
        {
            // Forward
            var entries = LeafEntries;
            if (entries == null)
                return false;
            else
                return entries.HasEntry( source, start, end );
        }

        /// <summary>
        /// Ermittelt den am besten passenden Eintrag aus einem Zeitraum.
        /// </summary>
        /// <typeparam name="TTarget">Die Art der Rückgabewerte.</typeparam>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <param name="start">Der Beginn des Zeitraums.</param>
        /// <param name="end">Das Ende des Zeitraums.</param>
        /// <param name="factory">Methode zum Erzeugen eines Rückgabewertes.</param>
        /// <returns>Der am besten passende Eintrag.</returns>
        public TTarget FindBestEntry<TTarget>( SourceIdentifier source, DateTime start, DateTime end, Func<ProgramGuideEntry, string, TTarget> factory )
        {
            // Forward
            var entries = LeafEntries;
            if (entries == null)
                return default( TTarget );
            else
                return entries.FindBestEntry( source, start, end, entry => factory( entry, ProfileName ) );
        }

        /// <summary>
        /// Ermittelt einen bestimmten Eintrag.
        /// </summary>
        /// <param name="source">Die Quelle, deren Eintrag ermittelt werden soll.</param>
        /// <param name="start">Der exakte Startzeitpunkt.</param>
        /// <returns>Der gewünschte Eintrag.</returns>
        public ProgramGuideEntry FindEntry( SourceIdentifier source, DateTime start ) => LeafEntries?.FindEntry( source, start );

        /// <summary>
        /// Ermittelt einen Auszug aus der Programmzeitschrift.
        /// </summary>
        /// <typeparam name="TEntry">Die Art der externen Darstellung von Einträgen.</typeparam>
        /// <param name="filter">Der Filter in der internen Darstellung.</param>
        /// <param name="factory">Erstellt die externe Repräsentation eines Eintrags.</param>
        /// <returns>Die Liste aller passenden Einträge.</returns>
        public TEntry[] GetProgramGuideEntries<TEntry>( GuideEntryFilter filter, Func<ProgramGuideEntry, string, TEntry> factory )
        {
            // See if there is a guide
            var entries = LeafEntries;
            if (entries == null)
                return new TEntry[0];
            else
                return filter.Filter( entries.Events ).Select( entry => factory( entry, ProfileName ) ).ToArray();
        }

        /// <summary>
        /// Ermittelt einen Auszug aus der Programmzeitschrift.
        /// </summary>
        /// <param name="filter">Der Filter in der internen Darstellung.</param>
        /// <returns>Die Anzahl der passenden Einträge.</returns>
        public int GetProgramGuideEntries( GuideEntryFilter filter )
        {
            // See if there is a guide
            var entries = LeafEntries;
            if (entries == null)
                return 0;
            else
                return filter.Filter( entries.Events ).Count();
        }

        /// <summary>
        /// Meldet die tatsächliche Verwaltung der Programmzeitschrift.
        /// </summary>
        public ProgramGuideEntries LeafEntries
        {
            get
            {
                // See if we are already done
                var profiles = JobManager.Server.Profiles;
                if (profiles == null)
                    return null;

                // Get the profile holding the data
                var profile = LeafGuideProfile;
                if (profile == null)
                    return null;

                // Find it
                var state = profiles[profile.Name];
                if (state == null)
                    return null;

                // Forward
                return state.ProgramGuide.m_Events;
            }
        }

        /// <summary>
        /// Meldet alle Einträge der Programmzeitschrift zu einer Quelle.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Die gewünschte Liste.</returns>
        internal IEnumerable<ProgramGuideEntry> GetEntries( SourceIdentifier source )
        {
            // Find the real holder
            var entries = LeafEntries;
            if (null == entries)
                entries = new ProgramGuideEntries();

            // Report
            return entries.GetEntries( source );
        }
    }
}
