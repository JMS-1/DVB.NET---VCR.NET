using JMS.DVB.Algorithms.Scheduler;
using System;
using System.IO;


namespace JMS.DVBVCR.RecordingService.Planning
{
    /// <summary>
    /// Berechnete die Aktualisierungszeiten für die Programmzeitschrift.
    /// </summary>
    public class ProgramGuideTask : PeriodicScheduler
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
        /// <param name="forResource">Das Gerät, für das die Sammlung erfolgt.</param>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        public ProgramGuideTask( IScheduleResource forResource, ProfileState profile )
            : this( forResource, profile, () => profile.ProgramGuide.LastUpdateTime )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Verwaltung.
        /// </summary>
        /// <param name="forResource">Das Gerät, für das die Sammlung erfolgt.</param>
        /// <param name="lastUpdate">Methode zur Ermittelung des letzten Aktualisierungszeitpunktes.</param>
        public ProgramGuideTask( IScheduleResource forResource, Func<DateTime?> lastUpdate )
            : this( forResource, null, lastUpdate )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Verwaltung.
        /// </summary>
        /// <param name="forResource">Das Gerät, für das die Sammlung erfolgt.</param>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        /// <param name="lastUpdate">Methode zur Ermittelung des letzten Aktualisierungszeitpunktes.</param>
        /// <exception cref="ArgumentNullException">Der letzte Aktualisierungszeitpunkt ist nicht gesetzt.</exception>
        private ProgramGuideTask( IScheduleResource forResource, ProfileState profile, Func<DateTime?> lastUpdate )
            : base( "Programmzeitschrift", Guid.NewGuid() )
        {
            // Validate
            if (forResource == null)
                throw new ArgumentNullException( nameof( forResource ) );
            if (lastUpdate == null)
                throw new ArgumentNullException( nameof( lastUpdate ) );

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
        public override IScheduleResource[] Resources => m_Resources;

        /// <summary>
        /// Meldet, ob die Ausführung grundsätzlich aktiviert ist.
        /// </summary>
        public override bool IsEnabled => VCRConfiguration.Current.ProgramGuideUpdateEnabled;

        /// <summary>
        /// Meldet wenn möglich den Zeitpunkt, an dem letztmalig ein Durchlauf
        /// stattgefunden hat.
        /// </summary>
        public override DateTime? LastRun => m_LastUpdate();

        /// <summary>
        /// Meldet die maximale Dauer einer Ausführung.
        /// </summary>
        public override TimeSpan Duration => TimeSpan.FromMinutes( VCRConfiguration.Current.ProgramGuideUpdateDuration );

        /// <summary>
        /// Meldet die Zeitspanne nach der ein neuer Durchlauf gestartet werden darf,
        /// wenn der Computer sowieso gerade aktiv ist.
        /// </summary>
        public override TimeSpan? JoinThreshold => VCRConfiguration.Current.HasRecordedSomething ? VCRConfiguration.Current.ProgramGuideJoinThreshold : null;

        /// <summary>
        /// Meldet die Zeitspanne, die mindestens zwischen zwei Durchläufen liegen soll.
        /// </summary>
        public override TimeSpan DefaultInterval => VCRConfiguration.Current.ProgramGuideUpdateInterval ?? new TimeSpan( 1 );

        /// <summary>
        /// Meldet die bevorzugten Uhrzeiten für eine Ausführung. Die verwendeten Zeiten
        /// bezeichnen dabei Stunden in der lokalen Zeitzone.
        /// </summary>
        public override uint[] PreferredHours => VCRConfiguration.Current.ProgramGuideUpdateHours;
    }
}
