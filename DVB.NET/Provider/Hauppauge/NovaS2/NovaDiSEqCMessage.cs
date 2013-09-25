using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.Provider.NovaS2
{
    /// <summary>
    /// Structure to send a DiSEqC command and receive some response.
    /// </summary>
    [StructLayout( LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi )]
    internal struct NovaDiSEqCMessage
    {
        /// <summary>
        /// Request data.
        /// </summary>
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 151 )]
        public byte[] Request;

        // Response data.
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 9 )]
        public byte[] Response;

        /// <summary>
        /// Number of bytes in <see cref="Request"/>.
        /// </summary>
        public UInt32 RequestLength;

        /// <summary>
        /// Number of bytes in <see cref="Response"/>.
        /// </summary>
        public UInt32 ResponseLength;

        /// <summary>
        /// The amplitude attenuation.
        /// </summary>
        public UInt32 AmplitudeAttenuation;

        /// <summary>
        /// The tone burst modulation mode.
        /// </summary>
        public ToneBurstModulationModes ToneBurstModulation;

        /// <summary>
        /// The DiSEqC version to use.
        /// </summary>
        public DiSEqCVersions DiSEqCVersion;

        /// <summary>
        /// The mode how the answer is received.
        /// </summary>
        public DiSEqCReceiveModes ResponseMode;

        /// <summary>
        /// Set if this is the last message in a DiSEqC command chain.
        /// </summary>
        public bool LastMessage;
    }
}
