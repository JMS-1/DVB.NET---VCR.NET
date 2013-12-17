using System;


namespace JMS.TechnoTrend
{
    /// <summary>
    /// Capabilities of a <see cref="JMS.TechnoTrend.MFCWrapper.DVBAVControl"/>.
    /// </summary>
    [Flags]
    public enum AVCapabilities
    {
        /// <summary>
        /// Can control volume.
        /// </summary>
        Volume = 0x000001,

        /// <summary>
        /// [Don't know]
        /// </summary>
        Loop = 0x000010,

        /// <summary>
        /// [Don't know]
        /// </summary>
        MSP = 0x000100,

        /// <summary>
        /// Analog input source.
        /// </summary>
        Analog = 0x010000,

        /// <summary>
        /// [Don't know]
        /// </summary>
        Switch = 0x100000,

        /// <summary>
        /// Hardware AC3 decoder.
        /// </summary>
        HardWareAC3 = 0x01000000
    }
}
