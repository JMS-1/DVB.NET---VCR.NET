using JMS.DVB;


namespace JMS.TV.Core
{
    /// <summary>
    /// Wird von einer Komponente angeboten, die Sender zur Verfügung stellt
    /// </summary>
    public interface IFeedProvider
    {
        /// <summary>
        /// Meldet die maximale Anzahl von gleichzeitig empfangbaren Quellgruppen.
        /// </summary>
        int NumberOfDevices { get; }

        /// <summary>
        /// Reserviert ein Gerät.
        /// </summary>
        /// <param name="index">Die 0-basierte laufende Nummer des gewünschten Gerätes.</param>
        void AllocateDevice( int index );

        /// <summary>
        /// Stellt sicher, dass ein Geräte eine bestimmte Quelle empfängt.
        /// </summary>
        /// <param name="index">Die laufende Nummer des Gerätes.</param>
        /// <param name="source">Die geforderte Quelle.</param>
        /// <returns>Alle Quellen, die nun ohne Umschaltung von diesem gerät empfangen werden können.</returns>
        CancellableTask<SourceSelection[]> Activate( int index, SourceSelection source );

        /// <summary>
        /// Ermittelt zu einem Namen die zugehörige Quelle.
        /// </summary>
        /// <param name="sourceName">Der Name der Quelle.</param>
        /// <returns>Die Quelle.</returns>
        SourceSelection Translate( string sourceName );

        /// <summary>
        /// Beginnt mit der Abfrage der Daten zu einer Quelle.
        /// </summary>
        /// <param name="index">Die laufenden Nummer des zu verwendenden Gerätes.</param>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Eine bereits aktivierte Hintergrundaufgabe.</returns>
        CancellableTask<SourceInformation> GetSourceInformationAsync( int index, SourceSelection source );

        /// <summary>
        /// Stellt sicher, dass alle Quellinformationen neu ermittelt werden.
        /// </summary>
        /// <param name="index">Die laufende Nummer des zu verwendende Gerätes.</param>
        void RefreshSourceInformations( int index );
    }
}
