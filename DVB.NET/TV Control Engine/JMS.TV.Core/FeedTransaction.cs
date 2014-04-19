using System;
using System.Collections.Generic;


namespace JMS.TV.Core
{
    /// <summary>
    /// Beschreibt eine Änderung am Empfang.
    /// </summary>
    /// <typeparam name="TSourceType">Die Art der Quellen.</typeparam>
    /// <typeparam name="TRecordingType">Die Art der Identifikation von Aufzeichnungen.</typeparam>
    internal class FeedTransaction<TSourceType, TRecordingType> : IDisposable where TSourceType : class
    {
        /// <summary>
        /// Alle Aktionen, die zur Korrektur ausgeführt werden müssen.
        /// </summary>
        private readonly List<Action> m_rollbackActions = new List<Action>();

        /// <summary>
        /// Alle Aktionen die bei einem sauberen Abschluss der Änderungen ausgeführt werden müssen.
        /// </summary>
        private readonly List<Action> m_commitActions = new List<Action>();

        /// <summary>
        /// Die zugehörige Senderverwaltung.
        /// </summary>
        private readonly FeedSet<TSourceType, TRecordingType> m_feedSet;

        /// <summary>
        /// Erstellt eine neue Änderungsumgebung.
        /// </summary>
        /// <param name="feedSet">Die zugehörige Senderverwaltung.</param>
        public FeedTransaction( FeedSet<TSourceType, TRecordingType> feedSet )
        {
            m_feedSet = feedSet;
        }

        /// <summary>
        /// Verändert den primären Sender.
        /// </summary>
        /// <param name="source">Der betroffenen Sender.</param>
        /// <param name="newState">Der gewünschte neue Zustand.</param>
        public void ChangePrimaryView( TSourceType source, bool newState )
        {
            ChangePrimaryView( m_feedSet.FindFeed( source ), newState );
        }

        /// <summary>
        /// Verändert den primären Sender.
        /// </summary>
        /// <param name="feed">Der betroffenen Sender.</param>
        /// <param name="newState">Der gewünschte neue Zustand.</param>
        public void ChangePrimaryView( Feed<TSourceType, TRecordingType> feed, bool newState )
        {
            // Validate
            if (feed.IsPrimaryView == newState)
                throw new ArgumentException( "Umschaltung des primären Senders unmöglich", "newState" );

            // Change
            feed.IsPrimaryView = newState;

            // Create rollback action
            m_commitActions.Add( () => m_feedSet.OnPrimaryViewChanged( feed, newState ) );
            m_rollbackActions.Add( () => feed.IsPrimaryView = !newState );
        }

        /// <summary>
        /// Verändert einen sekundären Sender.
        /// </summary>
        /// <param name="source">Der betroffenen Sender.</param>
        /// <param name="newState">Der gewünschte neue Zustand.</param>
        public void ChangeSecondaryView( TSourceType source, bool newState )
        {
            ChangeSecondaryView( m_feedSet.FindFeed( source ), newState );
        }

        /// <summary>
        /// Verändert einen sekundären Sender.
        /// </summary>
        /// <param name="feed">Der betroffenen Sender.</param>
        /// <param name="newState">Der gewünschte neue Zustand.</param>
        public void ChangeSecondaryView( Feed<TSourceType, TRecordingType> feed, bool newState )
        {
            // Validate
            if (feed.IsSecondaryView == newState)
                throw new ArgumentException( "Umschaltung des sekundären Senders unmöglich", "newState" );

            // Change
            feed.IsSecondaryView = newState;

            // Create rollback action
            m_commitActions.Add( () => m_feedSet.OnSecondaryViewChanged( feed, newState ) );
            m_rollbackActions.Add( () => feed.IsSecondaryView = !newState );
        }

        /// <summary>
        /// Verändert die Aufzeichnung eines Senders.
        /// </summary>
        /// <param name="feed">Der Sender.</param>
        /// <param name="key">Die Identifikation der Aufzeichung.</param>
        /// <param name="newState">Der gewünschte Aufzeichnungsstand.</param>
        public void ChangeRecording( Feed<TSourceType, TRecordingType> feed, TRecordingType key, bool newState )
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
            m_commitActions.Add( () => m_feedSet.OnRecordingChanged( feed, key, newState ) );
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
            m_commitActions.ForEach( commit => commit() );
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
            m_feedSet.TestIdle();
        }
    }
}
