

namespace JMS.TV.Core
{
    /// <summary>
    /// Eine Quelle.
    /// </summary>
    public interface ISource
    {
        /// <summary>
        /// Der eindeutige Name der Quelle.
        /// </summary>
        string UniqueName { get; }
    }
}
