using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JMS.DVBVCR.RecordingService.Persistence;


namespace JMS.DVBVCR.RecordingService
{
    /// <summary>
    /// Verwaltung aller Aufträge für alle DVB.NET Geräteprofile.
    /// </summary>
    public partial class JobManager
    {
        /// <summary>
        /// Dateiendung für Aufträge im SOAP Format.
        /// </summary>
        private const string OldFileSuffix = ".vnj";

        /// <summary>
        /// Das Format, in dem das reine Datum in den Dateinamen von Protokolleinträgen codiert wird.
        /// </summary>
        private const string LogEntryDateFormat = "yyyyMMdd";

        /// <summary>
        /// Das Format, in dem die Uhrzeit von Protokolleinträgen codiert ist.
        /// </summary>
        private const string LogEntryTimeFormat = "HHmmssfffffff";

        /// <summary>
        /// Ermittelt das Protokollverzeichnis vom VCR.NET.
        /// </summary>
        public DirectoryInfo LogDirectory { get { return new DirectoryInfo( Path.Combine( RootDirectory.FullName, "Logs" ) ); } }

        /// <summary>
        /// Ermittelt das EPG und Sendersuchlaufverzeichnis vom VCR.NET.
        /// </summary>
        public DirectoryInfo CollectorDirectory { get { return new DirectoryInfo( Path.Combine( RootDirectory.FullName, "EPG" ) ); } }

        /// <summary>
        /// Ermittelt das Verzeichnis aller aktiven Aufträge vom VCR.NET.
        /// </summary>
        public DirectoryInfo JobDirectory { get { return new DirectoryInfo( Path.Combine( RootDirectory.FullName, "Active" ) ); } }

        /// <summary>
        /// Meldet das Wurzelverzeichnis, unter dem Aufträge und Protokolle abgelegt werden.
        /// </summary>
        public DirectoryInfo RootDirectory { get; private set; }

        /// <summary>
        /// Meldet die zugehörige VCR.NET Instanz.
        /// </summary>
        public VCRServer Server { get; private set; }

        /// <summary>
        /// Vorhaltung aller Aufträge.
        /// </summary>
        private readonly Dictionary<Guid, VCRJob> m_Jobs = new Dictionary<Guid, VCRJob>();

        /// <summary>
        /// Erzeugt eine neue Verwaltungsinstanz und lädt die aktuellen Auftragsliste.
        /// </summary>
        /// <param name="rootDirectory">Meldet das Verzeichnis, unterhalb dessen alle
        /// Aufträge und Protokolle angelegt werden.</param>
        /// <param name="server">Die VCR.NET Instanz, der diese Verwaltung zugeordnet ist.</param>
        internal JobManager( DirectoryInfo rootDirectory, VCRServer server )
        {
            // Remember
            RootDirectory = rootDirectory;
            Server = server;

            // Create root directory
            RootDirectory.Create();

            // Create working directories
            CollectorDirectory.Create();
            ArchiveDirectory.Create();
            JobDirectory.Create();
            LogDirectory.Create();

            // Load all jobs
            foreach (var job in VCRJob.Load( JobDirectory ))
                if (job.UniqueID.HasValue)
                    m_Jobs[job.UniqueID.Value] = job;
        }

        /// <summary>
        /// Ermittelt alle Aufträge zu einem DVB.NET Geräteprofil.
        /// </summary>
        /// <returns>Alle Aufträge zum Geräteprofil</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Geräteprofil angegeben.</exception>
        internal List<VCRJob> GetActiveJobs()
        {
            // Filter out all jobs for the indicated profile
            lock (m_Jobs)
            {
                // All to delete
                m_Jobs.Values.Where( job => job.UniqueID.HasValue && !job.IsActive ).ToList().ForEach( Delete );

                // Report the rest
                return m_Jobs.Values.ToList();
            }
        }

        /// <summary>
        /// Löscht einen aktiven oder archivierten Auftrag.
        /// </summary>
        /// <param name="job">Der zu löschende Auftrag.</param>
        public void Delete( VCRJob job )
        {
            // Check unique identifier
            if (!job.UniqueID.HasValue)
                throw new InvalidJobDataException( Properties.Resources.BadUniqueID );

            // Report
            Tools.ExtendedLogging( "Deleting Job {0}", job.UniqueID );

            // Must synchronize
            lock (m_Jobs)
            {
                // Load from the map
                VCRJob internalJob = this[job.UniqueID.Value];

                // See if this is active
                if (null != internalJob)
                {
                    // Delete it
                    internalJob.Delete( JobDirectory );

                    // Remove from map
                    m_Jobs.Remove( internalJob.UniqueID.Value );

                    // Save to file
                    internalJob.Save( ArchiveDirectory );
                }
                else
                {
                    // Report
                    Tools.ExtendedLogging( "Job not found in Active Directory - trying Archive" );

                    // Must be archived               
                    job.Delete( ArchiveDirectory );
                }
            }
        }

        /// <summary>
        /// Liefert einen bestimmten aktiven Auftrag.
        /// </summary>
        /// <param name="jobIdentifier">Die eindeutige Kennung des Auftrags.</param>
        /// <returns>Ein aktiver Auftrag oder <i>null</i>.</returns>
        public VCRJob this[Guid jobIdentifier]
        {
            get
            {
                // The result
                VCRJob result;

                // Cut off
                lock (m_Jobs)
                    if (m_Jobs.TryGetValue( jobIdentifier, out result ))
                        return result;

                // Report
                return null;
            }
        }

        /// <summary>
        /// Aktualisiert einen Auftrag oder legt einen Auftrag neu an.
        /// </summary>
        /// <param name="job">Der neue oder veränderte Auftrag.</param>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der veränderten Aufzeichnung.</param>
        public void Update( VCRJob job, Guid? scheduleIdentifier )
        {
            // Report
            if (job != null)
                Tools.ExtendedLogging( "Updating Job {0}", job.UniqueID );

            // Load default profile name
            job.SetProfile();

            // Validate
            job.Validate( scheduleIdentifier );

            // Cleanup schedules
            job.CleanupExceptions();

            // Remove from archive - if job has been recovered
            job.Delete( ArchiveDirectory );

            // Try to store to disk - actually this is inside the lock because the directory virtually is part of our map
            lock (m_Jobs)
                if (job.Save( JobDirectory ).GetValueOrDefault())
                    m_Jobs[job.UniqueID.Value] = job;
                else
                    throw new ArgumentException( string.Format( Properties.Resources.SaveJobFailed, job.UniqueID ), "job" );
        }

        /// <summary>
        /// Ermittelt einen Auftrag nach seiner eindeutigen Kennung.
        /// </summary>
        /// <param name="jobIdentifier">Die Kennung des Auftrags.</param>
        /// <returns>Der gewünschte Auftrag oder <i>null</i>, wenn kein derartiger
        /// Auftrag existiert.</returns>
        public VCRJob FindJob( Guid jobIdentifier )
        {
            // The result
            VCRJob result = null;

            // Synchronize
            lock (m_Jobs)
            {
                // Try map
                if (m_Jobs.TryGetValue( jobIdentifier, out result ))
                    return result;

                // Create file name
                FileInfo jobFile = new FileInfo( Path.Combine( ArchiveDirectory.FullName, jobIdentifier.ToString( "N" ).ToUpper() + VCRJob.FileSuffix ) );
                if (!jobFile.Exists)
                    return null;

                // Load
                result = SerializationTools.Load<VCRJob>( jobFile );
                if (null == result)
                    return null;
            }

            // Check identifier and finalize settings - for pre-3.0 files
            if (!result.UniqueID.HasValue)
                return null;
            if (!jobIdentifier.Equals( result.UniqueID.Value ))
                return null;

            // Finish
            result.SetProfile();

            // Found in archive
            return result;
        }

        /// <summary>
        /// Legt nach einer abgeschlossenen Aufzeichnung fest, wann frühestens eine Wiederholung
        /// stattfinden darf.
        /// </summary>
        /// <param name="recording">Alle Informationen zur ausgeführten Aufzeichnung.</param>
        public void SetRestartThreshold( VCRRecordingInfo recording )
        {
            // Forward
            if (null != recording)
                SetRestartThreshold( recording, recording.EndsAt );
        }

        /// <summary>
        /// Legt nach einer abgeschlossenen Aufzeichnung fest, wann frühestens eine Wiederholung
        /// stattfinden darf.
        /// </summary>
        /// <param name="recording">Alle Informationen zur ausgeführten Aufzeichnung.</param>
        /// <param name="endsAt">Der Endzeitpunkt der Aufzeichnung.</param>
        public void SetRestartThreshold( VCRRecordingInfo recording, DateTime endsAt )
        {
            // Forward
            if (recording != null)
                if (recording.JobUniqueID.HasValue)
                    if (recording.ScheduleUniqueID.HasValue)
                        SetRestartThreshold( recording.JobUniqueID.Value, recording.ScheduleUniqueID.Value, endsAt );
        }

        /// <summary>
        /// Legt nach einer abgeschlossenen Aufzeichnung fest, wann frühestens eine Wiederholung
        /// stattfinden darf.
        /// </summary>
        /// <param name="jobIdentifier">Die eindeutige Kennung des Auftrags.</param>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung im Auftrag.</param>
        /// <param name="endsAt">Der Endzeitpunkt der Aufzeichnung.</param>
        private void SetRestartThreshold( Guid jobIdentifier, Guid scheduleIdentifier, DateTime endsAt )
        {
            // Report
            Tools.ExtendedLogging( "Setting Restart Threshold of {0}/{1} to {2}", jobIdentifier, scheduleIdentifier, endsAt );

            // Synchronize
            lock (m_Jobs)
            {
                // Locate job
                var job = this[jobIdentifier];
                if (job == null)
                    return;

                // Locate schedule
                var schedule = job[scheduleIdentifier];
                if (schedule == null)
                    return;

                // Make sure that this schedule is not used again
                schedule.DoNotRestartBefore = endsAt;

                // Save to file
                Update( job, null );
            }
        }

        /// <summary>
        /// Ermittelt das Archivverzeichnis vom VCR.NET.
        /// </summary>
        public DirectoryInfo ArchiveDirectory { get { return new DirectoryInfo( Path.Combine( RootDirectory.FullName, "Archive" ) ); } }

        /// <summary>
        /// Ermittelt alle archivierten Aufträge zu allen DVB.NET Geräteprofilen.
        /// </summary>
        public VCRJob[] ArchivedJobs
        {
            get
            {
                // For legacy updates
                var profile = VCRProfiles.DefaultProfile;

                // Process
                lock (m_Jobs)
                    return
                        ArchiveDirectory
                        .GetFiles( "*" + VCRJob.FileSuffix )
                        .Select( file =>
                            {
                                // Load
                                var job = SerializationTools.Load<VCRJob>( file );

                                // Enrich legacy entries
                                if (job != null)
                                    if (profile != null)
                                        job.SetProfile( profile.Name );

                                // Report
                                return job;
                            } )
                        .Where( job => job != null )
                        .ToArray();
            }
        }

        /// <summary>
        /// Der Zeitpunkt, an dem das nächste Mal das Archiv bereinigt werden soll.
        /// </summary>
        private DateTime m_nextArchiveCleanup = DateTime.MinValue;

        /// <summary>
        /// Entfernt veraltete Aufträge aus dem Archiv.
        /// </summary>
        internal void CleanupArchivedJobs()
        {
            // Not yet
            if (DateTime.UtcNow < m_nextLogCleanup)
                return;

            // Remember
            m_nextLogCleanup = DateTime.UtcNow.AddDays( 1 );

            // Access limit
            var firstValid = DateTime.UtcNow.AddDays( -7 * VCRConfiguration.Current.ArchiveLifeTime );
            var jobDirectory = ArchiveDirectory;

            // Protect to avoid parallel operations on the archive directory
            lock (m_Jobs)
                foreach (var file in jobDirectory.GetFiles( "*" + VCRJob.FileSuffix ))
                    if (file.LastWriteTimeUtc < firstValid)
                        try
                        {
                            // Delete the log entry
                            file.Delete();
                        }
                        catch (Exception e)
                        {
                            // Report error
                            VCRServer.Log( e );
                        }
        }

        /// <summary>
        /// Erzeugt einen Protokolleintrag.
        /// </summary>
        /// <param name="logEntry">Der Protokolleintrag.</param>
        public void CreateLogEntry( VCRRecordingInfo logEntry )
        {
            // Store
            if (logEntry.Source != null)
                if (!string.IsNullOrEmpty( logEntry.Source.ProfileName ))
                    SerializationTools.SafeSave( logEntry, Path.Combine( LogDirectory.FullName, DateTime.UtcNow.ToString( LogEntryDateFormat + LogEntryTimeFormat ) + logEntry.Source.ProfileName + VCRRecordingInfo.FileSuffix ) );
        }

        /// <summary>
        /// Ermittelt alle Protokolleinträge für einen bestimmten Zeitraum.
        /// </summary>
        /// <param name="firstDate">Erster zu berücksichtigender Tag.</param>
        /// <param name="lastDate">Letzter zu berücksichtigender Tag.</param>
        /// <param name="profile">Profile, dessen Protokolle ausgelesen werden sollen.</param>
        /// <returns>Liste aller Protokolleinträge für den gewünschten Zeitraum.</returns>
        internal List<VCRRecordingInfo> FindLogEntries( DateTime firstDate, DateTime lastDate, ProfileState profile )
        {
            // Create list
            var logs = new List<VCRRecordingInfo>();

            // Create search patterns
            var last = lastDate.AddDays( 1 ).ToString( LogEntryDateFormat );
            var first = firstDate.ToString( LogEntryDateFormat );

            // Load all jobs
            foreach (var file in LogDirectory.GetFiles( "*" + VCRRecordingInfo.FileSuffix ))
            {
                // Skip
                if (file.Name.CompareTo( first ) < 0)
                    continue;
                if (file.Name.CompareTo( last ) >= 0)
                    continue;

                // Load item
                var logEntry = SerializationTools.Load<VCRRecordingInfo>( file );
                if (logEntry == null)
                    continue;

                // Check
                if (profile != null)
                    if (!profile.IsResponsibleFor( logEntry.Source ))
                        continue;

                // Attach the name
                logEntry.LogIdentifier = file.Name.ToLower();

                // Remember
                logs.Add( logEntry );
            }

            // Sort by start time
            logs.Sort( VCRRecordingInfo.ComparerByStarted );

            // Report
            return logs;
        }

        /// <summary>
        /// Der Zeitpunkt, an dem die nächste Bereinigung stattfinden soll.
        /// </summary>
        private DateTime m_nextLogCleanup = DateTime.MinValue;

        /// <summary>
        /// Bereinigt alle veralteten Protokolleinträge.
        /// </summary>
        internal void CleanupLogEntries()
        {
            // Check time
            if (DateTime.UtcNow < m_nextLogCleanup)
                return;

            // Not again for now
            m_nextLogCleanup = DateTime.UtcNow.AddDays( 1 );

            // For cleanup
            var firstValid = DateTime.Now.Date.AddDays( -7 * VCRConfiguration.Current.LogLifeTime ).ToString( LogEntryDateFormat );

            // Load all jobs
            foreach (var file in LogDirectory.GetFiles( "*" + VCRRecordingInfo.FileSuffix ))
                if (file.Name.CompareTo( firstValid ) < 0)
                    try
                    {
                        // Delete the log entry
                        file.Delete();
                    }
                    catch (Exception e)
                    {
                        // Report error
                        VCRServer.Log( e );
                    }
        }
    }
}
