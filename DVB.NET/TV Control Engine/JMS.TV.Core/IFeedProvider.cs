

namespace JMS.TV.Core
{
    /// <summary>
    /// Wird von einer Komponente angeboten, die Sender zur Verfügung stellt
    /// </summary>
    /// <typeparam name="TSourceType">Die Art der Quellen.</typeparam>
    public interface IFeedProvider<TSourceType> where TSourceType : class
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
        /// Gibt ein Gerät wieder frei.
        /// </summary>
        /// <param name="index">Die 0-basierte laufende Nummer des gewünschten Gerätes.</param>
        void ReleaseDevice( int index );

        /// <summary>
        /// Stellt sicher, dass ein Geräte eine bestimmte Quelle empfängt.
        /// </summary>
        /// <param name="index">Die laufende Nummer des Gerätes.</param>
        /// <param name="source">Die geforderte Quelle.</param>
        /// <returns>Alle Quellen, die nun ohne Umschaltung von diesem gerät empfangen werden können.</returns>
        TSourceType[] Activate( int index, TSourceType source );
    }
}
