using System;
using System.Collections.Generic;
using System.Linq;
using JMS.DVB;
using JMS.DVB.CardServer;
using JMS.DVBVCR.RecordingService.Persistence;
using JMS.DVBVCR.RecordingService.ProgramGuide;
using JMS.DVBVCR.RecordingService.Status;


namespace JMS.DVBVCR.RecordingService.Requests
{
    /// <summary>
    /// Beschreibt die Ausführung der Aktualisierung der Programmzeitschrift.
    /// </summary>
    public class ProgramGuideProxy : CardServerProxy
    {
        /// <summary>
        /// Beschreibt den Zugriff zum Starten der Sammlung der Programmzeitschrift.
        /// </summary>
        private IAsyncResult m_startPending;

        /// <summary>
        /// Beschreibt, welche Erweiterungen der Programmzeitschrift auch ausgewertet werden sollen.
        /// </summary>
        private readonly EPGExtensions m_extensions;

        /// <summary>
        /// Alle Quellen, die bei der Aktualisierung zu berücksichtigen sind.
        /// </summary>
        private readonly HashSet<SourceIdentifier> m_selected = new HashSet<SourceIdentifier>();

        /// <summary>
        /// Erstellt eine neue Aktualisierung.
        /// </summary>
        /// <param name="state">Das zugehörige Geräteprofil.</param>
        /// <param name="recording">Daten der primären Aufzeichnung.</param>
        private ProgramGuideProxy( ProfileState state, VCRRecordingInfo recording )
            : base( state, recording )
        {
            // Reset fields
            if (VCRConfiguration.Current.EnableFreeSat)
                m_extensions = EPGExtensions.FreeSatUK;
            else
                m_extensions = EPGExtensions.None;

            // All sources we know about
            var allSources = new Dictionary<string, SourceSelection>( StringComparer.InvariantCultureIgnoreCase );

            // Load all sources of this profile
            foreach (var source in VCRProfiles.GetSources( ProfileName ))
            {
                // Remember by direct name
                allSources[source.DisplayName] = source;

                // allSources by unique name
                allSources[source.QualifiedName] = source;
            }

            // Fill in all
            foreach (var legacyName in VCRConfiguration.Current.ProgramGuideSources)
            {
                // Skip if empty
                if (string.IsNullOrEmpty( legacyName ))
                    continue;

                // Locate
                SourceSelection realSource;
                if (allSources.TryGetValue( legacyName, out realSource ))
                    m_selected.Add( realSource.Source );
                else
                    VCRServer.Log( LoggingLevel.Full, Properties.Resources.BadEPGStation, legacyName );

            }
        }

        /// <summary>
        /// Erstellt eine neue Aktualisierung.
        /// </summary>
        /// <param name="state">Das zugehörige Geräteprofil.</param>
        /// <param name="recording">Beschreibt die Aufzeichnung.</param>
        /// <returns>Die gewünschte Steuerung.</returns>
        /// <exception cref="ArgumentNullException">Es wurden nicht alle Parameter angegeben.</exception>
        public static ProgramGuideProxy Create( ProfileState state, VCRRecordingInfo recording )
        {
            // Validate
            if (state == null)
                throw new ArgumentNullException( "state" );
            if (recording == null)
                throw new ArgumentNullException( "recording" );

            // Forward
            return new ProgramGuideProxy( state, recording );
        }

        /// <summary>
        /// Die Art dieser Aufzeichnung.
        /// </summary>
        protected override string TypeName { get { return "Update Program Guide"; } }

        /// <summary>
        /// Beginnt mit der Sammlung.
        /// </summary>
        protected override void OnStart()
        {
            // Collect sources
            var sources = m_selected.ToArray();

            // Report
            Tools.ExtendedLogging( "Start Program Guide Update for {0} with {1} Source(s) and Mode {2}", ProfileName, sources.Length, m_extensions );

            // Start
            m_startPending = Server.BeginStartEPGCollection( sources, m_extensions );
        }

        /// <summary>
        /// Prüft, ob noch Zugriffe ausstehen.
        /// </summary>
        protected override bool HasPendingServerRequest
        {
            get
            {
                // See if we are still waiting
                if (!WaitForEnd( ref m_startPending, "Program Guide Collection now active" ))
                    return true;

                // Nope, we are idle
                return false;
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn ein neuer Zustand verfügbar ist.
        /// </summary>
        /// <param name="state">Der neue Zustand.</param>
        protected override void OnNewStateAvailable( ServerInformation state )
        {
            // See if we are finished
            if (state.ProgramGuideProgress.GetValueOrDefault( 0 ) >= 1)
                ChangeEndTime( Representative.ScheduleUniqueID.Value, DateTime.UtcNow.AddDays( -1 ), false );
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
            ProfileState.ProgramGuide.LastUpdateTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Beendet die Sammlung endgültig.
        /// </summary>
        protected override void OnStop()
        {
            // At least we tried
            ProfileState.ProgramGuide.LastUpdateTime = DateTime.UtcNow;

            // Report
            Tools.ExtendedLogging( "Converting Program Guide Entries from Card Server to VCR.NET Format" );

            // Create result
            var result = new ProgramGuideEntries();

            // Fill it
            foreach (var item in Server.BeginEndEPGCollection().Result)
            {
                // Create event
                var epg =
                    new ProgramGuideEntry
                    {
                        TransportIdentifier = item.Source.TransportStream,
                        ShortDescription = item.ShortDescription,
                        NetworkIdentifier = item.Source.Network,
                        ServiceIdentifier = item.Source.Service,
                        Description = item.Description,
                        Duration = item.Duration,
                        Language = item.Language,
                        StartTime = item.Start,
                        Name = item.Name
                    };

                // Finish
                if (item.Content != null)
                    epg.Categories.AddRange( item.Content.Select( c => c.ToString() ) );
                if (item.Ratings != null)
                    epg.Ratings.AddRange( item.Ratings );

                // Resolve
                var source = VCRProfiles.FindSource( ProfileName, item.Source );
                if (source == null)
                {
                    // Load default
                    epg.StationName = item.Source.ToString();
                }
                else
                {
                    // Attach to the station
                    var station = (Station) source.Source;

                    // Load names
                    epg.StationName = station.Name;
                }

                // Add it
                result.Add( epg );
            }

            // Report
            ProfileState.ProgramGuide.UpdateGuide( result );
        }

        /// <summary>
        /// Ermittelt eine Beschreibung der aktuellen Aufzeichnung.
        /// </summary>
        /// <param name="info">Vorabinformation der Basisklasse.</param>
        /// <param name="finalCall">Gesetzt, wenn das Ergebnis zum Protokolleintrag wird.</param>
        /// <param name="state">Die zuletzt erhaltenen Zustandsinformationen.</param>
        protected override void OnFillInformation( FullInfo info, bool finalCall, ServerInformation state )
        {
            // Just copy the progress
            if (state == null)
                info.Recording.TotalSize = 0;
            else if (!state.ProgramGuideProgress.HasValue)
                info.Recording.TotalSize = 0;
            else
                info.Recording.TotalSize = (uint) state.CurrentProgramGuideItems;
        }
    }
}
