using System;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Detailinformationen zu einem Bild mit Transparenz.
    /// </summary>
    [Flags]
    internal enum VMRAlphaBitmapFlags
    {
        /// <summary>
        /// Deaktiviert.
        /// </summary>
        Disable = 0x00000001,

        /// <summary>
        /// Zeichenkontext verwenden.
        /// </summary>
        DeviceHandle = 0x00000002,

        /// <summary>
        /// Gesamte Zeichenfläche verwenden.
        /// </summary>
        EntireSurface = 0x00000004,

        /// <summary>
        /// Transparenzschlüssel ist verfügbar.
        /// </summary>
        SourceColorKey = 0x00000008,

        /// <summary>
        /// Quellbereich ist verfügbar.
        /// </summary>
        SourceRect = 0x00000010,

        /// <summary>
        /// Filterinformationen sind verfügbar.
        /// </summary>
        FilterMode = 0x00000020
    }
}
