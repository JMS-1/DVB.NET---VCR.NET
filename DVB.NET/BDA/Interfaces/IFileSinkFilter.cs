using System;
using System.Runtime.InteropServices;

using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Diese Schnittstelle wird von einem Verbraucher angeboten, der in eine Datei schreibt.
    /// </summary>
    [
        ComImport,
        Guid( "a2104830-7c70-11cf-8bce-00aa00a3f1a6" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    internal interface IFileSinkFilter
    {
        /// <summary>
        /// Legt den Namen der Datei fest.
        /// </summary>
        /// <param name="fileName">Der volle Pfad zur Datei.</param>
        /// <param name="mediaType">Das Datenformat für die Übertragung.</param>
        void SetFileName( [MarshalAs( UnmanagedType.LPWStr )] string fileName, ref RawMediaType mediaType );

        /// <summary>
        /// Meldet die aktuelle Datei.
        /// </summary>
        /// <param name="fileName">Der volle Pfad zur Datei.</param>
        /// <param name="mediaType">Das aktuell verwendete Datenformat.</param>
        void GetCurFile( [MarshalAs( UnmanagedType.LPWStr )] out string fileName, out RawMediaType mediaType );
    }
}
