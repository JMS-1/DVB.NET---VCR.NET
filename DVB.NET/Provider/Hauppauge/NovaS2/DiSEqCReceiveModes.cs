using System;


namespace JMS.DVB.Provider.NovaS2
{
    /// <summary>
    /// How answers to <see cref="BDATunerExtensions">DiSEqC</see> commands are
    /// recognized.
    /// </summary>
    internal enum DiSEqCReceiveModes
    {
        /// <summary>
        /// Use current register settings.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Expecting multiple devices attached.
        /// </summary>
        Interrogation = 1,

        /// <summary>
        /// Expecting one response.
        /// </summary>
        QuickReply = 2,

        /// <summary>
        /// Expecting no response.
        /// </summary>
        NoReply = 3
    }
}
