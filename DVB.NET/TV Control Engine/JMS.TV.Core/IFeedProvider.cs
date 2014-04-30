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
        /// Meldet ein Gerät.
        /// </summary>
        /// <param name="index">Die 0-basierte laufenden Nummer des Gerätes.</param>
        /// <returns>Das gewünschte Gerät.</returns>
        IDevice GetDevice( int index );

        /// <summary>
        /// Ermittelt zu einem Namen die zugehörige Quelle.
        /// </summary>
        /// <param name="sourceName">Der Name der Quelle.</param>
        /// <returns>Die Quelle.</returns>
        SourceSelection Translate( string sourceName );
    }
}
