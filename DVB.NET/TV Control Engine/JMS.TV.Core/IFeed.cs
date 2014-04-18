

namespace JMS.TV.Core
{
    /// <summary>
    /// Beschreibt einen einzelnen Sender - in der ersten Version wird es nur Fernsehsender
    /// geben.
    /// </summary>
    public interface IFeed
    {
    }

    /// <summary>
    /// Beschreibt einen einzelnen Sender - in der ersten Version wird es nur Fernsehsender
    /// geben.
    /// </summary>
    /// <typeparam name="TRecordingType">Die Art der Identifikation für Aufzeichnungen.</typeparam>
    public interface IFeed<TRecordingType> : IFeed
    {
    }
}
