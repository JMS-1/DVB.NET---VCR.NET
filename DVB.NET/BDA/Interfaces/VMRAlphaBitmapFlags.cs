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
        /// Gesamte Zeichenfl�che verwenden.
        /// </summary>
        EntireSurface = 0x00000004,

        /// <summary>
        /// Transparenzschl�ssel ist verf�gbar.
        /// </summary>
        SourceColorKey = 0x00000008,

        /// <summary>
        /// Quellbereich ist verf�gbar.
        /// </summary>
        SourceRect = 0x00000010,

        /// <summary>
        /// Filterinformationen sind verf�gbar.
        /// </summary>
        FilterMode = 0x00000020
    }
}
