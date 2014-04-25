using System;
using System.Collections.Generic;
using JMS.DVB;


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
        /// Alle Aktionen die bei einem sauberen Abschluss der Änderungen ausgeführt werden müssen.
        /// </summary>
        private readonly List<Action> m_commitActions = new List<Action>();

        /// <summary>
        /// Die zugehörige Senderverwaltung.
        /// </summary>
        private readonly FeedSet m_feedSet;

        /// <summary>
        /// Erstellt eine neue Änderungsumgebung.
        /// </summary>
        /// <param name="feedSet">Die zugehörige Senderverwaltung.</param>
        public FeedTransaction( FeedSet feedSet )
        {
            m_feedSet = feedSet;
        }

        /// <summary>
        /// Verändert den primären Sender.
        /// </summary>
        /// <param name="feed">Der betroffenen Sender.</param>
        public void DisablePrimaryView( Feed feed )
        {
            // Validate
            if (!feed.IsPrimaryView)
                throw new ArgumentException( "Umschaltung des primären Senders unmöglich", "feed" );

            // Change
            feed.IsPrimaryView = false;

            // Create rollback action
            m_commitActions.Add( () => m_feedSet.OnPrimaryViewChanged( feed, false ) );
            m_rollbackActions.Add( () => feed.IsPrimaryView = true );
        }

        /// <summary>
        /// Verändert einen sekundären Sender.
        /// </summary>
        /// <param name="feed">Der betroffenen Sender.</param>
        public void DisableSecondaryView( Feed feed )
        {
            // Validate
            if (!feed.IsSecondaryView)
                throw new ArgumentException( "Umschaltung des sekundären Senders unmöglich", "feed" );

            // Change
            feed.IsSecondaryView = false;

            // Create rollback action
            m_commitActions.Add( () => m_feedSet.OnSecondaryViewChanged( feed, false ) );
            m_rollbackActions.Add( () => feed.IsSecondaryView = true );
        }

        /// <summary>
        /// Verändert die Aufzeichnung eines Senders.
        /// </summary>
        /// <param name="feed">Der Sender.</param>
        /// <param name="key">Die Identifikation der Aufzeichung.</param>
        /// <param name="newState">Der gewünschte Aufzeichnungsstand.</param>
        public void ChangeRecording( Feed feed, string key, bool newState )
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
        }
    }
}
