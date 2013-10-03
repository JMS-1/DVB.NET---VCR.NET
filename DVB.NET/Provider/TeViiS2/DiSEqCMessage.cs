using System.Runtime.InteropServices;


namespace S24600DiSEqC
{

    /// <summary>
    /// Eine DiSEqC Steuernachricht
    /// </summary>
    [StructLayout( LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi )]
    internal struct DiSEqCMessage
    {
        /// <summary>
        /// Der zu sendende Befehl.
        /// </summary>
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 151 )]
        public byte[] Request;

        /// <summary>
        /// Die Anzahl der Zeichen im Befehl.
        /// </summary>
        public byte RequestLength;

        /// <summary>
        /// Bei dem letzten Befehl einer Kette gesetzt.
        /// </summary>
        public bool LastMessage;

        /// <summary>
        /// Schaltet die Stromversorgung um.
        /// </summary>
        public int Power;
    }
}
