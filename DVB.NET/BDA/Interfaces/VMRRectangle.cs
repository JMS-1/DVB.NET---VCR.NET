using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Beschreibt einen Bereich.
    /// </summary>
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    internal struct VMRRectangle
    {
        /// <summary>
        /// Linke Seite.
        /// </summary>
        public Int32 Left;

        /// <summary>
        /// Oberere Seite.
        /// </summary>
        public Int32 Top;

        /// <summary>
        /// Rechte Seite.
        /// </summary>
        public Int32 Right;

        /// <summary>
        /// Untere Seite.
        /// </summary>
        public Int32 Bottom;
    }
}
