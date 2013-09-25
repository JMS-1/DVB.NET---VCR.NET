using System;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Beschreibt, welche Einstellungen aktiv sind.
    /// </summary>
    [Flags]
    internal enum VMRProcAmpControlFlags
    {
        /// <summary>
        /// Die Helligkeit ist gesetzt.
        /// </summary>
        Brightness = 0x1,

        /// <summary>
        /// Der Kontrast ist gesetzt.
        /// </summary>
        Contrast = 0x2,

        /// <summary>
        /// Der Farbton ist gesetzt.
        /// </summary>
        Hue = 0x4,

        /// <summary>
        /// Die Sättigung ist gesetzt.
        /// </summary>
        Saturation = 0x8,

        /// <summary>
        /// Alle möglichen Einstellungen.
        /// </summary>
        Mask = Brightness | Contrast | Hue | Saturation
    }
}