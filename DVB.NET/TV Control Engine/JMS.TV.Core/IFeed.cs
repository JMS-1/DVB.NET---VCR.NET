using JMS.DVB;


namespace JMS.TV.Core
{
    /// <summary>
    /// Beschreibt einen einzelnen Sender - in der ersten Version wird es nur Fernsehsender
    /// geben.
    /// </summary>
    public interface IFeed
    {
        /// <summary>
        /// Meldet die Hintergrundaufgabe, mit der die Daten der zugehörigen Quelle ermittelt werden.
        /// </summary>
        CancellableTask<SourceInformation> SourceInformationReader { get; }

        /// <summary>
        /// Stellt sicher, dass beim nächsten Aufruf von <see cref="SourceInformationReader"/> eine neue
        /// Hintergrundaufgabe gestartet wird.
        /// </summary>
        void RefreshSourceInformation();
    }
}
