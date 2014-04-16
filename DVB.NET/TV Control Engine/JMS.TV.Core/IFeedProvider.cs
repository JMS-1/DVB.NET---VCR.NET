

namespace JMS.TV.Core
{
    /// <summary>
    /// Wird von einer Komponente angeboten, die Sender zur Verfügung stellt
    /// </summary>
    /// <typeparam name="TSourceType">Die Art der Quellen.</typeparam>
    public interface IFeedProvider<TSourceType>
    {
        /// <summary>
        /// Meldet die maximale Anzahl von gleichzeitig empfangbaren Quellgruppen.
        /// </summary>
        int NumberOfDevices { get; }

        /// <summary>
        /// Ein Gerät wird nicht mehr benötigt.
        /// </summary>
        /// <param name="device">Die 0-basierte laufenden Nummer des Gerätes.</param>
        void Stop( int device );

        /// <summary>
        /// Aktiviert ein Gerät.
        /// </summary>
        /// <param name="device">Die 0-basierte laufende Nummer des Gerätes.</param>
        /// <param name="source">Die Quelle, die angesteuert werden soll.</param>
        TSourceType[] Start( int device, TSourceType source );
    }
}
