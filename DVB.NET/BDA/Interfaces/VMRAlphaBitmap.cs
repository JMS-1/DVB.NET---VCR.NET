using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Beschreibt ein Bild mit Transparent.
    /// </summary>
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    internal struct VMRAlphaBitmap
    {
        /// <summary>
        /// Detailinformationen zum Bild.
        /// </summary>
        public VMRAlphaBitmapFlags Flags;

        /// <summary>
        /// Der zugehörige Zeichenkontext.
        /// </summary>
        public IntPtr DeviceHandle;

        /// <summary>
        /// Die zugehörige Zeichenfläche.
        /// </summary>
        private IntPtr Surface;

        /// <summary>
        /// Der Quellbereich.
        /// </summary>
        public VMRRectangle Source;

        /// <summary>
        /// Der Zielbereich.
        /// </summary>
        public VMRNormalizedRectangle Target;

        /// <summary>
        /// Die Transparenz.
        /// </summary>
        public float Alpha;

        /// <summary>
        /// Der Transparenzschlüssel der Quelldaten.
        /// </summary>
        public UInt32 SourceColorKey;

        /// <summary>
        /// Einstellung zur Filterung.
        /// </summary>
        public UInt32 FilterMode;
    }
}
