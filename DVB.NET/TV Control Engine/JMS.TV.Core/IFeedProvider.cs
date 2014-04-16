

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
        int SourceGroupLimit { get; }
    }
}
