using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Mit dieser Schnittstelle k�nnen �berladungen in der Bilddarstellung erzeugt werden.
    /// </summary>
    [
        ComImport,
        Guid( "ced175e5-1935-4820-81bd-ff6ad00c9108" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    internal interface IVMRMixerBitmap
    {
        /// <summary>
        /// Legt eine �berladung fest.
        /// </summary>
        /// <param name="bmp">Beschreibt die gew�nschte �berladung.</param>
        void SetAlphaBitmap( ref VMRAlphaBitmap bmp );

        /// <summary>
        /// Aktualisiert eine �berladung.
        /// </summary>
        /// <param name="bmp">Die Daten zur �nderung.</param>
        void UpdateAlphaBitmapParameters( ref VMRAlphaBitmap bmp );

        /// <summary>
        /// Meldet die Parameter einer �berladung.
        /// </summary>
        /// <param name="bmp">Die aktuellen Parameter.</param>
        void GetAlphaBitmapParameters( ref VMRAlphaBitmap bmp );
    }
}
