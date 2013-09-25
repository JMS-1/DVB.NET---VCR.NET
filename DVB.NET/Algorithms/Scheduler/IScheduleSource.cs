using System;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Beschreibt eine einzelne Quelle.
    /// </summary>
    public interface IScheduleSource
    {
        /// <summary>
        /// Meldet, ob diese Quelle eine Entschlüsselung benötigt.
        /// </summary>
        bool IsEncrypted { get; }

        /// <summary>
        /// Meldet, ob es sich bei der Quelle um einen Radiosender handelt.
        /// </summary>
        bool IsAudioOnly { get; }

        /// <summary>
        /// Prüft, ob diese Quelle mit einer anderen parallel aufgezeichnet werden kann.
        /// </summary>
        /// <param name="source">Eine andere Quelle.</param>
        /// <returns>Gesetzt, wenn eine parallele Aufzeichnung theoretisch möglich ist.</returns>
        bool BelongsToSameSourceGroupAs( IScheduleSource source );

        /// <summary>
        /// Prüft ob zwei Quellen identisch sind.
        /// </summary>
        /// <param name="source">Eine andere Quelle.</param>
        /// <returns>Gesetzt, wenn die Quellen identisch sind.</returns>
        bool IsSameAs( IScheduleSource source );
    }
}
