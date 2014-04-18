using System;
using System.Collections.Generic;


namespace JMS.TV.Core
{
    /// <summary>
    /// Beschreibt eine Änderung am Empfang.
    /// </summary>
    internal class FeedTransaction : IDisposable
    {
        /// <summary>
        /// Alle Aktionen, die zur Korrektur ausgeführt werden müssen.
        /// </summary>
        private readonly List<Action> m_rollbackActions = new List<Action>();

        /// <summary>
        /// Wird einmalig zum Beenden aufgerufen.
        /// </summary>
        private readonly Action m_termination;

        /// <summary>
        /// Erstellt eine neue Änderungsumgebung.
        /// </summary>
        /// <param name="termination">Methode zum Abschluss.</param>
        public FeedTransaction( Action termination )
        {
            m_termination = termination;
        }

        /// <summary>
        /// Verändert den primären Sender.
        /// </summary>
        /// <param name="feed">Der betroffenen Sender.</param>
        /// <param name="newState">Der gewünschte neue Zustand.</param>
        public void ChangePrimaryView( Feed feed, bool newState )
        {
            // Validate
            if (feed.IsPrimaryView == newState)
                throw new ArgumentException( "Umschaltung des primären Senders unmöglich", "newState" );

            // Change
            feed.IsPrimaryView = newState;

            // Create rollback action
            m_rollbackActions.Add( () => feed.IsPrimaryView = !newState );
        }

        /// <summary>
        /// Verändert einen sekundären Sender.
        /// </summary>
        /// <param name="feed">Der betroffenen Sender.</param>
        /// <param name="newState">Der gewünschte neue Zustand.</param>
        public void ChangeSecondaryView( Feed feed, bool newState )
        {
            // Validate
            if (feed.IsSecondaryView == newState)
                throw new ArgumentException( "Umschaltung des sekundären Senders unmöglich", "newState" );

            // Change
            feed.IsSecondaryView = newState;

            // Create rollback action
            m_rollbackActions.Add( () => feed.IsSecondaryView = !newState );
        }

        /// <summary>
        /// Verändert die Aufzeichnung eines Senders.
        /// </summary>
        /// <typeparam name="TRecordingType">Die Art der Identifikation von Aufzeichnungen.</typeparam>
        /// <param name="feed">Der Sender.</param>
        /// <param name="key">Die Identifikation der Aufzeichung.</param>
        /// <param name="newState">Gesetzt, wenn die Aufzeichnung begonnen werden soll.</param>
        public void ChangeRecording<TRecordingType>( Feed<TRecordingType> feed, TRecordingType key, bool newState )
        {
            // Validate
            if (feed.IsRecording( key ) == newState)
                throw new ArgumentException( "Aufzeichnung des Senders unmöglich", "newState" );

            // Change
            if (newState)
                feed.StartRecording( key );
            else
                feed.StopRecording( key );

            // Create rollback action
            m_rollbackActions.Add( () =>
            {
                // Reverse change
                if (newState)
                    feed.StopRecording( key );
                else
                    feed.StartRecording( key );
            } );
        }

        /// <summary>
        /// Stellt sicher, dass alle Änderungen übernommen werden.
        /// </summary>
        public void Commit()
        {
            m_rollbackActions.Clear();
        }

        /// <summary>
        /// Beendet die Transaktion und verwirft alle Operationen.
        /// </summary>
        public void Dispose()
        {
            // From last to first
            m_rollbackActions.Reverse();
            m_rollbackActions.ForEach( rollback => rollback() );

            // Final
            m_termination();
        }
    }
}
