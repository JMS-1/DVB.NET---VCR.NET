using System;
using System.Runtime.InteropServices;


namespace JMS.TechnoTrend.MFCWrapper
{
    /// <summary>
    /// Structure used to set a satellite channel.
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public struct Channel_S
    {
        /// <summary>
        /// Frequency to use.
        /// </summary>
        public UInt32 Frequency;

        /// <summary>
        /// Inversion to use.
        /// </summary>
        public SpectrumInversion Inversion;

        /// <summary>
        /// Related symbol rate.
        /// </summary>
        public UInt32 SymbolRate;

        /// <summary>
        /// [Don't know]
        /// </summary>
        public UInt32 LOF;

        /// <summary>
        /// [Don't know]
        /// </summary>
        public Int32 b22kHz;

        /// <summary>
        /// Power to switch the antenna.
        /// </summary>
        public PowerMode LNBPower;

        /// <summary>
        /// [Don't know]
        /// </summary>
        public Viterbi Viterbi;
    }
}
