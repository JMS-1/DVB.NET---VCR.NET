using System;
using System.IO;
using JMS.DVB.Algorithms.Scheduler;


namespace JMS.DVBVCR.RecordingService.Planning
{
    /// <summary>
    /// Berechnete die Aktualisierungszeiten für die Liste der Quellen.
    /// </summary>
    public class SourceListTask : PeriodicScheduler
    {
        /// <summary>
        /// Die Methode zur Ermittelung des letzten Aktualisierungszeitpunktes.
        /// </summary>
        private Func<DateTime?> m_LastUpdate;

        /// <summary>
        /// Das Verzeichnis, in dem temporäre Dateien während der Sammlung abgelegt werden können.
        /// </summary>
        public DirectoryInfo CollectorDirectory { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Verwaltung.
        /// </summary>
        /// <param name="forResource">Das Geräteprofil, auf dem der Lauf erfolgen soll.</param>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        public SourceListTask( IScheduleResource forResource, ProfileState profile )
            : this( forResource, profile, () => profile.LastSourceUpdateTime )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Verwaltung.
        /// </summary>
        /// <param name="forResource">Das Geräteprofil, auf dem der Lauf erfolgen soll.</param>
        /// <param name="lastUpdate">Methode zur Ermittelung des letzten Aktualisierungszeitpunktes.</param>
        public SourceListTask( IScheduleResource forResource, Func<DateTime?> lastUpdate )
            : this( forResource, null, lastUpdate )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Verwaltung.
        /// </summary>
        /// <param name="forResource">Das Geräteprofil, auf dem der Lauf erfolgen soll.</param>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        /// <param name="lastUpdate">Methode zur Ermittelung des letzten Aktualisierungszeitpunktes.</param>
        /// <exception cref="ArgumentNullException">Der letzte Aktualisierungszeitpunkt ist nicht gesetzt.</exception>
        private SourceListTask( IScheduleResource forResource, ProfileState profile, Func<DateTime?> lastUpdate )
            : base( "Sendersuchlauf", Guid.NewGuid() )
        {
            // Validate
            if (forResource == null)
                throw new ArgumentNullException( "forResource" );
            if (lastUpdate == null)
                throw new ArgumentNullException( "lastUpdate" );

            // Remember
            m_Resources = new[] { forResource };
            m_LastUpdate = lastUpdate;

            // Set the job directory
            if (profile != null)
                CollectorDirectory = profile.Server.JobManager.CollectorDirectory;
        }

        /// <summary>
        /// Meldet das Gerät, für das diese Sammlung erfolgt.
        /// </summary>
        private readonly IScheduleResource[] m_Resources;

        /// <summary>
        /// Meldet das Gerät, für das diese Sammlung erfolgt.
        /// </summary>
        public override IScheduleResource[] Resources { get { return m_Resources; } }

        /// <summary>
        /// Meldet, ob die Ausführung grundsätzlich aktiviert ist.
        /// </summary>
        public override bool IsEnabled
        {
            get
            {
                // Report
                if (VCRConfiguration.Current.SourceListUpdateDuration < 1)
                    return false;
                else if (VCRConfiguration.Current.SourceListUpdateInterval == 0)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Meldet die maximale Dauer einer Ausführung.
        /// </summary>
        public override TimeSpan Duration { get { return TimeSpan.FromMinutes( VCRConfiguration.Current.SourceListUpdateDuration ); } }

        /// <summary>
        /// Meldet wenn möglich den Zeitpunkt, an dem letztmalig ein Durchlauf
        /// stattgefunden hat.
        /// </summary>
        public override DateTime? LastRun { get { return m_LastUpdate(); } }

        /// <summary>
        /// Meldet die Zeitspanne nach der ein neuer Durchlauf gestartet werden darf,
        /// wenn der Computer sowieso gerade aktiv ist.
        /// </summary>
        public override TimeSpan? JoinThreshold
        {
            get
            {
                // Ask configuration
                if (VCRConfiguration.Current.HasRecordedSomething)
                    return VCRConfiguration.Current.SourceListJoinThreshold;
                else
                    return null;
            }
        }

        /// <summary>
        /// Meldet die Zeitspanne, die mindestens zwischen zwei Durchläufen liegen soll.
        /// </summary>
        public override TimeSpan DefaultInterval
        {
            get
            {
                // Check for manual update mode
                var days = VCRConfiguration.Current.SourceListUpdateInterval;
                if (days < 1)
                    return TimeSpan.MaxValue;

                // There is exactly one hour we prefer do the best to match it
                if (VCRConfiguration.Current.SourceListUpdateHours.Length == 1)
                    return TimeSpan.FromDays( days - 1 ) + new TimeSpan( 1 );

                // There are none or multiple preferred hours so just use the full interval normally skipping the last hour choosen
                return TimeSpan.FromDays( days );
            }
        }

        /// <summary>
        /// Meldet die bevorzugten Uhrzeiten für eine Ausführung. Die verwendeten Zeiten
        /// bezeichnen dabei Stunden in der lokalen Zeitzone.
        /// </summary>
        public override uint[] PreferredHours { get { return VCRConfiguration.Current.SourceListUpdateHours; } }
    }
}
