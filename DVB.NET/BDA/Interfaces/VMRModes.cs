using System;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Auswahl der Darstellung eines Bildes.
    /// </summary>
    [Flags]
    internal enum VMRModes
    {
        /// <summary>
        /// Eigenst�ndiges Fenster.
        /// </summary>
        Windowed = 0x01,

        /// <summary>
        /// Darstellung ohne Fenster.
        /// </summary>
        Windowless = 0x02,

        /// <summary>
        /// Manuelle Darstellung.
        /// </summary>
        Renderless = 0x04,

        /// <summary>
        /// Alle m�glichen Einstellungen.
        /// </summary>
        Mask = Windowed | Windowless | Renderless
    }
}
