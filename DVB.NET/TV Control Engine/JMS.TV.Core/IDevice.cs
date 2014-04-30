using JMS.DVB;


namespace JMS.TV.Core
{
    /// <summary>
    /// Bietet ein einzelnes Geräte an.
    /// </summary>
    public interface IDevice
    {
        /// <summary>
        /// Stellt sicher, dass dieses Geräte eine bestimmte Quelle empfängt.
        /// </summary>
        /// <param name="source">Die geforderte Quelle.</param>
        /// <returns>Alle Quellen, die nun ohne Umschaltung von diesem gerät empfangen werden können.</returns>
        CancellableTask<SourceSelection[]> Activate( SourceSelection source );

        /// <summary>
        /// Beginnt mit der Abfrage der Daten zu einer Quelle.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Eine bereits aktivierte Hintergrundaufgabe.</returns>
        CancellableTask<SourceInformation> GetSourceInformationAsync( SourceSelection source );

        /// <summary>
        /// Stellt sicher, dass alle Quellinformationen neu ermittelt werden.
        /// </summary>
        void RefreshSourceInformations();
    }
}
