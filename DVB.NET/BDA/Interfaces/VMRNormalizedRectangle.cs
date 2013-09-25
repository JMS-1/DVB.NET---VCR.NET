using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{

    /// <summary>
    /// Beschreibt ein normalisiertes Rechteck für ein Videobild.
    /// </summary>
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    internal struct VMRNormalizedRectangle
    {
        /// <summary>
        /// Linker Rand.
        /// </summary>
        public float Left;

        /// <summary>
        /// Oberer Rand.
        /// </summary>
        public float Top;

        /// <summary>
        /// Rechter Rand.
        /// </summary>
        public float Right;

        /// <summary>
        /// Unterer Rand.
        /// </summary>
        public float Bottom;
    }
}
