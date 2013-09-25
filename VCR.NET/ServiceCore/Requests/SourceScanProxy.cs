using System;
using JMS.DVB.CardServer;
using JMS.DVBVCR.RecordingService.Persistence;
using JMS.DVBVCR.RecordingService.Status;


namespace JMS.DVBVCR.RecordingService.Requests
{
    /// <summary>
    /// Beschreibt die Ausführung der Aktualisierung der Quellen eines Geräteprofils.
    /// </summary>
    public class SourceScanProxy : CardServerProxy
    {
        /// <summary>
        /// Beschreibt den Zugriff zum Starten der Aktualisierung.
        /// </summary>
        private IAsyncResult m_startPending;

        /// <summary>
        /// Gesetzt, um die Listen nach Abschluss des Suchlaufs zu kombinieren.
        /// </summary>
        private readonly bool m_mergeSources;

        /// <summary>
        /// Erstellt eine neue Sammlung.
        /// </summary>
        /// <param name="state">Das zugehörige Geräteprofil.</param>
        /// <param name="recording">Die Beschreibung der Aufgabe.</param>
        private SourceScanProxy( ProfileState state, VCRRecordingInfo recording )
            : base( state, recording )
        {
            // Finish
            m_mergeSources = VCRConfiguration.Current.MergeSourceListUpdateResult;
        }

        /// <summary>
        /// Erstellt eine neue Sammlung.
        /// </summary>
        /// <param name="state">Das zugehörige Geräteprofil.</param>
        /// <param name="recording">Die Beschreibung der Aufgabe.</param>
        /// <returns>Die gewünschte Steuerung.</returns>
        public static SourceScanProxy Create( ProfileState state, VCRRecordingInfo recording )
        {
            // Validate
            if (state == null)
                throw new ArgumentNullException( "state" );
            if (recording == null)
                throw new ArgumentNullException( "recording" );

            // Forward
            return new SourceScanProxy( state, recording );
        }

        /// <summary>
        /// Die Art dieser Aufzeichnung.
        /// </summary>
        protected override string TypeName { get { return "Update Source List"; } }

        /// <summary>
        /// Aktiviert die Aktualisierung der Quellen.
        /// </summary>
        protected override void OnStart()
        {
            // Report
            Tools.ExtendedLogging( "Starting Source List Update for {0}", ProfileName );

            // Just start
            m_startPending = Server.BeginStartScan();
        }

        /// <summary>
        /// Wird aufgerufen, sobald eine neuer Zustand verfügbar ist.
        /// </summary>
        /// <param name="state">Der neue Zustand.</param>
        protected override void OnNewStateAvailable( ServerInformation state )
        {
            // See if we are finished
            if (state.UpdateProgress.GetValueOrDefault( 0 ) >= 1)
                ChangeEndTime( Representative.ScheduleUniqueID.Value, DateTime.UtcNow.AddDays( -365 ), false );
        }

        /// <summary>
        /// Beendet die Aufzeichnung vorzeitig.
        /// </summary>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der gewünschten Aufzeichnung.</param>
        protected override void OnEndRecording( Guid scheduleIdentifier )
        {
            // Must be us
            if (scheduleIdentifier != Representative.ScheduleUniqueID.Value)
                return;

            // Set early to make sure that planner will not re-run immediately
            ProfileState.LastSourceUpdateTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Beendet den Suchlauf endgültig.
        /// </summary>
        protected override void OnStop()
        {
            // Remember the time of the last scan
            ProfileState.LastSourceUpdateTime = DateTime.UtcNow;

            // Log
            VCRServer.Log( LoggingLevel.Full, Properties.Resources.PSIReplace );

            // Finish
            ServerImplementation.EndRequest( Server.BeginEndScan( m_mergeSources ) );

            // Report
            Tools.ExtendedLogging( "Card Server has updated Profile {0} - VCR.NET will reload all Profiles now", ProfileName );

            // Time to refresh our lists
            VCRProfiles.Reset();
        }

        /// <summary>
        /// Ergänzt den aktuellen Zustand des Suchlaufs.
        /// </summary>
        /// <param name="info">Die bereits vorbereitete Informationsstruktur.</param>
        /// <param name="finalCall">Gesetzt, wenn dieser Zustand als Protokoll verwendet wird.</param>
        /// <param name="state">Der zuletzt erhaltene Zustand.</param>
        protected override void OnFillInformation( FullInfo info, bool finalCall, ServerInformation state )
        {
            // Copy current result
            if (state == null)
                info.Recording.TotalSize = 0;
            else if (!state.UpdateProgress.HasValue)
                info.Recording.TotalSize = 0;
            else
                info.Recording.TotalSize = (uint) state.UpdateSourceCount;
        }

        /// <summary>
        /// Prüft, ob noch Zugriffe ausstehen.
        /// </summary>
        protected override bool HasPendingServerRequest
        {
            get
            {
                // See if we are still waiting
                if (!WaitForEnd( ref m_startPending, "Source Scan now active" ))
                    return true;

                // Nope, we are idle
                return false;
            }
        }
    }
}
