using System;
using System.Collections.Generic;


namespace JMS.TV.Core
{
    /// <summary>
    /// Beschreibt alle Sender, die zurzeit empfangen werden. 
    /// </summary>
    public interface IFeedSet
    {
        /// <summary>
        /// Meldet alle über diese Verwaltung verfügbaren Sender.
        /// </summary>
        /// <returns>Die Liste der Sender.</returns>
        IEnumerable<IFeed> Feeds { get; }

        /// <summary>
        /// Meldet den primären Sender.
        /// </summary>
        IFeed PrimaryView { get; }

        /// <summary>
        /// Meldet alle sekundären Sender.
        /// </summary>
        IEnumerable<IFeed> SecondaryViews { get; }

        /// <summary>
        /// Ermittelt einen Sender.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <returns>Der gesuchte Sender.</returns>
        IFeed FindFeed( string sourceName );

        /// <summary>
        /// Verändert die primäre Anzeige.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <returns>Gesetzt, wenn die Änderung erfolgreich war.</returns>
        bool TryStartPrimaryFeed( string sourceName );

        /// <summary>
        /// Aktiviert eine sekundäre Anzeige.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <returns>Gesetzt, wenn die Änderung erfolgreich war.</returns>
        bool TryStartSecondaryFeed( string sourceName );

        /// <summary>
        /// Beendet eine sekundäre Anzeige.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        void StopSecondaryFeed( string sourceName );

        /// <summary>
        /// Stellt sicher, dass von nun an alle Quelldaten neu ermittelt werden.
        /// </summary>
        void RefreshSourceInformations();

        /// <summary>
        /// Beendet alle Aufträge.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Wird ausgelöst, wenn sich die Sichtbarkeit des primären Senders tatsächlich verändert hat.
        /// </summary>
        event Action<IFeed, bool> PrimaryViewVisibilityChanged;

        /// <summary>
        /// Wird ausgelöst, wenn sich die Sichtbarkeit eines sekundären Senders tatsächlich verändert hat.
        /// </summary>
        event Action<IFeed, bool> SecondaryViewVisibilityChanged;

        /// <summary>
        /// Meldet alle Sender, auf denen Aufzeichnungen aktiv sind.
        /// </summary>
        IEnumerable<IFeed> Recordings { get; }

        /// <summary>
        /// Beginnt eine Aufzeichnung.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <param name="key">Die Identifikation der Aufzeichnung.</param>
        /// <returns>Gesetzt, wenn der Start erfolgreich war.</returns>
        bool TryStartRecordingFeed( string sourceName, string key );

        /// <summary>
        /// Beendet eine Aufzeichnung.
        /// </summary>
        /// <param name="sourceName">Der Name des Senders.</param>
        /// <param name="key">Die Identifikation der Aufzeichnung.</param>
        void StopRecordingFeed( string sourceName, string key );

        /// <summary>
        /// Wird ausgelöst, wenn sich eine Aufzeichnung verändert hat.
        /// </summary>
        event Action<IFeed, string, bool> RecordingStateChanged;
    }
}
