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
        /// Verändert den primären Sender.
        /// </summary>
        /// <param name="feed">Der betroffenen Sender.</param>
        /// <param name="isPrimary">Die neue Einstellung für den Sender.</param>
        public void ChangePrimary( Feed feed, bool isPrimary )
        {
            // Validate
            if (feed.IsPrimaryView == isPrimary)
                throw new ArgumentException( "Umschaltung des primären Senders unmöglich", "isPrimary" );

            // Change
            feed.IsPrimaryView = isPrimary;

            // Create rollback action
            m_rollbackActions.Add( () => feed.IsPrimaryView = !isPrimary );
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
