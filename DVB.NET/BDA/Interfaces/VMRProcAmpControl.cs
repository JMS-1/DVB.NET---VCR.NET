using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Beschreibt die Variation der Bilddarstellung.
    /// </summary>
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    internal struct VMRProcAmpControl
    {
        /// <summary>
        /// Gr‰ﬂe dieser Struktur.
        /// </summary>
        private UInt32 m_Size;

        /// <summary>
        /// Die tats‰chlich verwendeten Einstellungen.
        /// </summary>
        private VMRProcAmpControlFlags m_Flags;

        /// <summary>
        /// Die Helligkeit.
        /// </summary>
        public float Brightness;

        /// <summary>
        /// Der Kontrast.
        /// </summary>
        public float Contrast;

        /// <summary>
        /// Der Farbton.
        /// </summary>
        public float Hue;

        /// <summary>
        /// Die S‰ttingung.
        /// </summary>
        public float Saturation;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="flags">Die in der Beschreibung eingesetzten Parameter.</param>
        /// <returns>Eine neue Beschreibung.</returns>
        public static VMRProcAmpControl Create( VMRProcAmpControlFlags flags )
        {
            // Create new
            return
                new VMRProcAmpControl
                {
                    m_Size = (UInt32) Marshal.SizeOf( typeof( VMRProcAmpControl ) ),
                    m_Flags = flags,
                };
        }
    }
}