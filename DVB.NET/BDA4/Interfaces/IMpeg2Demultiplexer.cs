using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Verwaltet die interne Datenverteilung des Microsoft BDA Demultiplexers.
    /// </summary>
    [
        ComImport,
        Guid( "436eee9c-264f-4242-90e1-4e330c107512" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IMpeg2Demultiplexer
    {
        /// <summary>
        /// Erzeugt einen neuen Endpunkt.
        /// </summary>
        /// <param name="mediaType">Das Datenformat des Endpunktes.</param>
        /// <param name="pinName">Der Name des Endpunktes.</param>
        /// <returns>Der neue Endpunkt.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IPin CreateOutputPin( IntPtr mediaType, [MarshalAs( UnmanagedType.LPWStr )] string pinName );

        /// <summary>
        /// Legt den Datentyp eines Endpunktes fest.
        /// </summary>
        /// <param name="pinName">Der eindeutige Name des Endpunktes.</param>
        /// <param name="mediaType">Das neue Datenformat.</param>
        void SetOutputPinMediaType( [MarshalAs( UnmanagedType.LPWStr )] string pinName, IntPtr mediaType );

        /// <summary>
        /// Entfernt einen Endpunkt.
        /// </summary>
        /// <param name="pinName">Der eindeutige Name des Endpunktes.</param>
        void DeleteOutputPin( [MarshalAs( UnmanagedType.LPWStr )] string pinName );
    }
}
