using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Beschreibt den Bereich von Parametern.
    /// </summary>
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    internal struct VMRProcAmpControlRange
    {
        /// <summary>
        /// Die Größe dieser Struktur.
        /// </summary>
        private UInt32 m_Size;

        /// <summary>
        /// Legt fest, für welche Einstellung diese Informationen gültig sind.
        /// </summary>
        private VMRProcAmpControlFlags m_Property;

        /// <summary>
        /// Der minimale Wert.
        /// </summary>
        public float MinValue;

        /// <summary>
        /// Der maximale Wert.
        /// </summary>
        public float MaxValue;

        /// <summary>
        /// Der voreingestellte Wert.
        /// </summary>
        public float DefaultValue;

        /// <summary>
        /// Die Granularität der Einstellung.
        /// </summary>
        public float StepSize;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="property">Die zugehörige Einstellung..</param>
        /// <returns>Die gewünschte neue Beschreibung.</returns>
        public static VMRProcAmpControlRange Create( VMRProcAmpControlFlags property )
        {
            // Create new
            return
                new VMRProcAmpControlRange
                {
                    m_Size = (UInt32) Marshal.SizeOf( typeof( VMRProcAmpControlRange ) ),
                    m_Property = property,
                };
        }
    }
}