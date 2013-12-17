using System;
using System.Runtime.InteropServices;


namespace JMS.TechnoTrend.MFCWrapper
{
    /// <summary>
    /// Die Beschreibung der Signalstärke, so wie die TechnoTrend Schnittstellen sie anbieten.
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public struct SignalStatus
    {
        /// <summary>
        /// Frequency derivation in kHz.
        /// </summary>
        private Int32 AFC;

        /// <summary>
        /// Viterbi error rate.
        /// </summary>
        public double BER;

        /// <summary>
        /// Signal level.
        /// </summary>
        private byte AGC;

        /// <summary>
        /// C/N of the channel.
        /// </summary>
        private byte SNRSQE;

        /// <summary>
        /// C/N in dB - must be divided by 10 (DVB-S only).
        /// </summary>
        private UInt16 SNRdB;

        /// <summary>
        /// Signal quality in % derived from <see cref="SNRSQE"/>.
        /// </summary>
        private UInt16 SNR100;

        /// <summary>
        /// 
        /// </summary>
        public double dBLevel { get { return SNRdB / 10.0; } }

        /// <summary>
        /// 
        /// </summary>
        public double Level { get { return AGC / 255.0; } }

        /// <summary>
        /// 
        /// </summary>
        public double Quality { get { return SNR100 / 100.0; } }

        /// <summary>
        /// 
        /// </summary>
        public double RawQuality { get { return SNRSQE / 255.0; } }
    }
}
