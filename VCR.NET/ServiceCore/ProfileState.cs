using System;
using System.Threading;
using JMS.DVB;
using JMS.DVB.CardServer;
using JMS.DVBVCR.RecordingService.Persistence;
using JMS.DVBVCR.RecordingService.ProgramGuide;
using JMS.DVBVCR.RecordingService.Requests;
using JMS.DVBVCR.RecordingService.Status;


namespace JMS.DVBVCR.RecordingService
{
    /// <summary>
    /// Beschreibt den Arbeitszustand eines einzelnen aktiven Geräteprofils.
    /// </summary>
    public class ProfileState : IDisposable
    {
        /// <summary>
        /// Der Name des Geräteprofils.
        /// </summary>
        public string ProfileName { get; private set; }

        /// <summary>
        /// Die zugehörige Verwaltungsinstanz.
        /// </summary>
        public ProfileStateCollection Collection { get; private set; }

        /// <summary>
        /// Meldet die zugehörige Verwaltung der elektronischen Programmzeitschrift (EPG).
        /// </summary>
        public ProgramGuideManager ProgramGuide { get; private set; }

        /// <summary>
        /// Wird nach Aufwachen aus dem Schlafzustand gesetzt.
        /// </summary>
        internal volatile bool WakeUpRequired;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="collection">Die zugehörige Verwaltung der aktiven Geräteprofile.</param>
        /// <param name="profileName">Der Name des zugehörigen Geräteprofils.</param>
        public ProfileState( ProfileStateCollection collection, string profileName )
        {
            // Remember
            ProfileName = profileName;
            Collection = collection;

            // Create program guide manager
            ProgramGuide = new ProgramGuideManager( Collection.Server.JobManager, profileName );
        }

        /// <summary>
        /// Meldet das zugehörige Geräteprofil.
        /// </summary>
        public Profile Profile { get { return VCRProfiles.FindProfile( ProfileName ); } }

        /// <summary>
        /// Meldet die zugehörige VCR.NET Instanz.
        /// </summary>
        public VCRServer Server { get { return Collection.Server; } }

        /// <summary>
        /// Prüft, ob eine bestimmte Quelle zu diesem Geräteprofil gehört.
        /// </summary>
        /// <param name="source">Die zu prüfende Quelle.</param>
        /// <returns>Gesetzt, wenn es sich um eine Quelle dieses Geräteprofil handelt.</returns>
        public bool IsResponsibleFor( SourceSelection source )
        {
            // Process
            if (null == source)
                return false;
            else
                return ProfileManager.ProfileNameComparer.Equals( ProfileName, source.ProfileName );
        }

        /// <summary>
        /// Steuert den Zappping Modus.
        /// </summary>
        /// <typeparam name="TStatus">Die Art des Zustands.</typeparam>
        /// <param name="active">Gesetzt, wenn das Zapping aktiviert werden soll.</param>
        /// <param name="connectTo">Die TCP/IP UDP Adresse, an die alle Daten geschickt werden sollen.</param>
        /// <param name="source">Die zu aktivierende Quelle.</param>
        /// <param name="factory">Methode zum Erstellen einer neuen Zustandsinformation.</param>
        /// <returns>Der aktuelle Zustand des Zapping Modus oder <i>null</i>, wenn dieser nicht ermittelt
        /// werden kann.</returns>
        public TStatus LiveModeOperation<TStatus>( bool active, string connectTo, SourceIdentifier source, Func<string, ServerInformation, TStatus> factory )
        {
            // Check mode of operation
            if (!active)
            {
                // Deactivate
                var activeRequest = m_CurrentRequest as ZappingProxy;
                if (activeRequest != null)
                    activeRequest.Stop();
            }
            else if (!string.IsNullOrEmpty( connectTo ))
            {
                // Activate 
                ZappingProxy.Create( this, connectTo ).Start();
            }
            else if (source != null)
            {
                // Switch source
                var request = m_CurrentRequest as ZappingProxy;
                if (request != null)
                    return request.SetSource( source, factory );
            }

            // See if we have a current request
            var statusRequest = m_CurrentRequest as ZappingProxy;
            if (statusRequest == null)
                return factory( null, null );
            else
                return statusRequest.CreateStatus( factory );
        }

        /// <summary>
        /// Bereitet den Übergang in den Schlafzustand vor.
        /// </summary>
        public void PrepareSuspend()
        {
            // Stop request
            Stop();
        }

        /// <summary>
        /// Führt den Übergang in den Schlafzustand durch.
        /// </summary>
        public void Suspend()
        {
            // Wait for the current job to end
            var current = m_CurrentRequest;
            if (current != null)
                current.RequestFinished.WaitOne();

            // Will reset on next recording
            WakeUpRequired = true;
        }

        /// <summary>
        /// Beendet die Nutzung dieses Geräteprofils.
        /// </summary>
        public void Dispose()
        {
            // Stop request
            Stop();
        }

        #region Aktuelle Aufzeichnung

        /// <summary>
        /// Der aktuelle Zugriff auf die zugehörige DVB.NET Hardwareabstraktion.
        /// </summary>
        private volatile CardServerProxy m_CurrentRequest;

        /// <summary>
        /// Synchronisiert den Zugriff auf die aktuelle Operation.
        /// </summary>
        private object m_RequestLock = new object();

        /// <summary>
        /// Der aktuelle Zugriff auf die zugehörige DVB.NET Hardwareabstraktion. Der Aufrufer hält alle
        /// notwendigen Sperren.
        /// </summary>
        /// <param name="newRequest">Der neue Auftrag, der ab sofort ausgeführt wird.</param>
        private void ChangeCurrentRequest( CardServerProxy newRequest )
        {
            // Detach from previous
            var request = m_CurrentRequest;
            if (!ReferenceEquals( request, null ))
                request.Deactivate();

            // Update
            m_CurrentRequest = newRequest;

            // Attach to this one
            if (!ReferenceEquals( newRequest, null ))
                newRequest.Activate();
        }

        /// <summary>
        /// Beginnt eine Operation auf diesem Geräteprofil.
        /// </summary>
        /// <param name="request">Der zu aktivierende Zugriff.</param>
        /// <param name="throwOnBusy">Gesetzt um einen Fehler auszulösen, wenn bereits ein Zugriff aktiv ist.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Zugriff angegeben.</exception>
        /// <exception cref="ArgumentException">Der Zugriff gehört nicht zu diesem Geräteprofil.</exception>
        /// <exception cref="InvalidOperationException">Es ist bereits ein Zugriff aktiv.</exception>
        /// <returns>Gesetzt, wenn der neue Zugriff erfolgreich gestartet wurde.</returns>
        public bool BeginRequest( CardServerProxy request, bool throwOnBusy = true )
        {
            // Validate
            if (ReferenceEquals( request, null ))
                throw new ArgumentNullException( "request" );
            if (!ReferenceEquals( request.ProfileState, this ))
                throw new ArgumentException( request.ProfileName, "request.ProfileState" );

            // Synchronize
            lock (m_RequestLock)
            {
                // Wait for current request to end
                for (;;)
                {
                    // Load
                    var current = m_CurrentRequest;
                    if (ReferenceEquals( current, null ))
                        break;

                    // Failed
                    if (!current.IsShuttingDown)
                        if (current is ZappingProxy)
                            current.Stop( false );
                        else if (throwOnBusy)
                            throw new InvalidOperationException( "Profile is already running a Request" );
                        else
                            return false;

                    // Wait for it to end
                    Monitor.Wait( m_RequestLock );
                }

                // Take new
                ChangeCurrentRequest( request );
            }

            // Did it
            return true;
        }

        /// <summary>
        /// Beendet eine Operation auf diesem Geräteprofil.
        /// </summary>
        /// <param name="request">Der zu beendende Zugriff.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Zugriff angegeben.</exception>
        /// <exception cref="InvalidOperationException">Dieser Zugriff ist nicht der aktive Zugriff dieses
        /// Geräteprofils.</exception>
        public void EndRequest( CardServerProxy request )
        {
            // Validate
            if (ReferenceEquals( request, null ))
                throw new ArgumentNullException( "request" );

            // Synchronize
            lock (m_RequestLock)
            {
                // Process
                if (ReferenceEquals( m_CurrentRequest, request ))
                    ChangeCurrentRequest( null );
                else
                    throw new InvalidOperationException( "Wrong Request to End" );

                // Fire notification
                Monitor.PulseAll( m_RequestLock );
            }
        }

        /// <summary>
        /// Beendet die aktuelle Aktivität.
        /// </summary>
        private void Stop()
        {
            // Request the stop
            var request = m_CurrentRequest;
            if (request != null)
                request.Stop();
        }

        /// <summary>
        /// Meldet, ob gerade ein Zugriff auf diesem Geräteprofil stattfindet.
        /// </summary>
        public bool IsActive { get { return !ReferenceEquals( m_CurrentRequest, null ); } }

        /// <summary>
        /// Meldet die aktuelle Aufzeichnung oder <i>null</i>.
        /// </summary>
        public FullInfo CurrentRecording
        {
            get
            {
                // Report
                var current = m_CurrentRequest;
                if (ReferenceEquals( current, null ))
                    return null;
                else
                    return current.CreateFullInformation();
            }
        }

        #endregion

        #region Reguläre Aufzeichnungen

        /// <summary>
        /// Beginnt eine Aufzeichnung auf diesem Geräteprofil. Eventuell wird diese mit 
        /// einer anderen zusammen geführt.
        /// </summary>
        /// <param name="recording">Die gewünschte Aufzeichnung.</param>
        public void StartRecording( VCRRecordingInfo recording )
        {
            // Protect current request against transistions
            lock (m_RequestLock)
                for (;;)
                {
                    // Attach to the current request
                    var current = m_CurrentRequest;

                    // In best case we are just doing nothing
                    if (ReferenceEquals( current, null ))
                    {
                        // Create a brand new regular recording request
                        var request = new RecordingProxy( this, recording );

                        // Activate the request
                        request.Start();

                        // We dit it
                        break;
                    }
                    else if (current is ZappingProxy)
                    {
                        // Regular recordings have priority over LIVE mode so request stop and try again later
                        current.Stop( false );
                    }
                    else if (!current.IsShuttingDown)
                    {
                        // There is a current recording  which is not terminating so just join it
                        current.Start( recording );

                        // We dit it
                        break;
                    }

                    // Wait for transition notification
                    Monitor.Wait( m_RequestLock );
                }
        }

        /// <summary>
        /// Beendet eine Aufzeichnung.
        /// </summary>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
        public void EndRecording( Guid scheduleIdentifier )
        {
            // Protect current request against transistions
            lock (m_RequestLock)
            {
                // Attach to the current request
                var current = m_CurrentRequest;

                // See if it exists
                if (ReferenceEquals( current, null ))
                {
                    // Report
                    VCRServer.Log( LoggingLevel.Errors, "Request to end Recording '{0}' but there is no active Recording Process", scheduleIdentifier );

                    // Better do it ourselves
                    Collection.ConfirmOperation( scheduleIdentifier, false );
                }
                else
                {
                    // Just stop
                    current.Stop( scheduleIdentifier );
                }
            }
        }

        /// <summary>
        /// Verändert die Endzeit der aktuellen Aufzeichnung.
        /// </summary>
        /// <param name="streamIdentifier">Die eindeutige Kennung des zu verwendenden Datenstroms.</param>
        /// <param name="newEndTime">Der neue Endzeitpunkt.</param>
        /// <param name="disableHibernation">Gesetzt, wenn der Übergang in den Schlafzustand deaktiviert werden soll.</param>
        /// <returns>Die Aktivität auf dem Geräteprofil, die verändert wurde.</returns>
        public CardServerProxy ChangeStreamEnd( Guid streamIdentifier, DateTime newEndTime, bool disableHibernation )
        {
            // Be safe
            lock (m_RequestLock)
            {
                // Attach to current request
                var current = m_CurrentRequest;
                if (ReferenceEquals( current, null ))
                    return null;

                // Make sure that job is not restarted
                current.SetRestartThreshold( streamIdentifier );

                // Modify
                current.ChangeEndTime( streamIdentifier, newEndTime, disableHibernation );

                // Report success
                return current;
            }
        }

        /// <summary>
        /// Meldet die Anzahl der gerade aktiven Aufzeichnungen.
        /// </summary>
        public int NumberOfActiveRecordings => m_CurrentRequest?.NumberOfActiveRecordings ?? 0;

        /// <summary>
        /// Aktiviert oder deaktiviert den Netzwerkversand einer aktiven Quelle.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <param name="uniqueIdentifier">Die eindeutige Kennung der Teilaufzeichnung.</param>
        /// <param name="target">Das neue Ziel des Netzwerkversands.</param>
        /// <returns>Gesetzt, wenn die Operation ausgeführt wurde.</returns>
        public bool SetStreamTarget( SourceIdentifier source, Guid uniqueIdentifier, string target )
        {
            // Get the current request
            var request = m_CurrentRequest as RecordingProxy;
            if (ReferenceEquals( request, null ))
                return false;

            // Process
            request.SetStreamTarget( source, uniqueIdentifier, target );

            // Done
            return true;
        }

        #endregion

        #region Quellenlisten verwalten

        /// <summary>
        /// Meldet den Namen des Wertes in der Registrierung von Windows, wo der Zeitpunkt
        /// der letzten Aktualisierung der Liste der Quellen gespeichert wird.
        /// </summary>
        private string SourceUpdateRegistryName => $"LastPSIRun {ProfileName}";

        /// <summary>
        /// Liest oder setzt den Zeitpunkt der letzen Aktualisierung der Liste der Quellen
        /// dieses Geräteprofils durch den VCR.NET Recording Service.
        /// </summary>
        internal DateTime? LastSourceUpdateTime
        {
            get { return Tools.GetRegistryTime( SourceUpdateRegistryName ); }
            set { Tools.SetRegistryTime( SourceUpdateRegistryName, value ); }
        }

        #endregion
    }
}
