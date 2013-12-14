using System;
using JMS.DVB;
using JMS.DVB.CardServer;


namespace JMS.DVBVCR.RecordingService
{
    partial class VCRServer
    {
        /// <summary>
        /// Wird zur Prüfung auf neue Bearbeitungen aufgerufen.
        /// </summary>
        public void BeginNewPlan()
        {
            // Forward
            var profiles = Profiles;
            if (profiles != null)
                profiles.BeginNewPlan();
        }

        /// <summary>
        /// Fordert eine baldmögliche Aktualisierung der Programmzeitschrift an.
        /// </summary>
        public void ForceProgramGuideUpdate()
        {
            // Forward to profile collection
            Profiles.ForceProgramGuideUpdate();

            // Forward
            BeginNewPlan();
        }

        /// <summary>
        /// Erzwingt eine baldige Aktualisierung aller Listen von Quellen in allen
        /// Geräteprofilen.
        /// </summary>
        public void ForceSoureListUpdate()
        {
            // Forward to profile collection
            Profiles.ForceSoureListUpdate();

            // Forward
            BeginNewPlan();
        }

        /// <summary>
        /// Meldet, ob auf irgendeinem Geräteprofil ein Zugriff aktiv ist.
        /// </summary>
        public bool IsActive { get { return Profiles.IsActive; } }

        /// <summary>
        /// Verändert die Endzeit der aktuellen Aufzeichnung auf einem Geräteprofil.
        /// </summary>
        /// <param name="profile">Der Name des betroffenen Geräteprofils.</param>
        /// <param name="streamIdentifier">Die eindeutige Kennung des zu verwendenden Datenstroms.</param>
        /// <param name="newEndTime">Der neue Endzeitpunkt.</param>
        /// <param name="disableHibernation">Gesetzt, wenn der Übergang in den Schlafzustand deaktiviert werden soll.</param>
        public void ChangeRecordingStreamEndTime( string profile, Guid streamIdentifier, DateTime newEndTime, bool disableHibernation )
        {
            // Forward
            var state = FindProfile( profile );
            if (state != null)
                state.ChangeStreamEnd( streamIdentifier, newEndTime, disableHibernation );
        }

        /// <summary>
        /// Aktiviert oder deaktiviert den Netzwerkversand für eine Quelle.
        /// </summary>
        /// <param name="profile">Das zugehörige DVB.NET Geräteprofil.</param>
        /// <param name="source">Die betroffene Quelle.</param>
        /// <param name="uniqueIdentifier">Die eindeutige Kennung der Teilaufzeichnung.</param>
        /// <param name="target">Das neue Ziel des Netzwerkversands.</param>
        public void SetStreamTarget( string profile, SourceIdentifier source, Guid uniqueIdentifier, string target )
        {
            // Attach to the profile and process
            var state = FindProfile( profile );
            if (state != null)
                state.SetStreamTarget( source, uniqueIdentifier, target );
        }

        /// <summary>
        /// Steuert den Zapping Modus.
        /// </summary>
        /// <typeparam name="TStatus">Die Art der Zustandsinformation.</typeparam>
        /// <param name="profile">Das betroffene DVB.NET Geräteprofil.</param>
        /// <param name="active">Gesetzt, wenn der Zapping Modus aktiviert werden soll.</param>
        /// <param name="connectTo">Die TCP/IP UDP Adresse, an die alle Daten geschickt werden sollen.</param>
        /// <param name="source">Die zu aktivierende Quelle.</param>
        /// <param name="factory">Methode zum Erstellen einer neuen Zustandsinformation.</param>
        /// <returns>Der aktuelle Zustand des Zapping Modus oder <i>null</i>, wenn dieser nicht ermittelt
        /// werden konnte.</returns>
        public TStatus LiveModeOperation<TStatus>( string profile, bool active, string connectTo, SourceIdentifier source, Func<string, ServerInformation, TStatus> factory )
        {
            // Attach to the profile and process
            var state = FindProfile( profile );
            if (state == null)
                return default( TStatus );
            else
                return state.LiveModeOperation( active, connectTo, source, factory );
        }
    }
}
