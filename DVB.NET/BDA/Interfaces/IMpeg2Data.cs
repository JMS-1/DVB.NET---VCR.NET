using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Schnittstelle zur Beschreibung von MPEG2 Datenströmen.
    /// </summary>
    [
        ComImport,
        Guid( "9b396d40-f380-4e3c-a514-1a82bf6ebfe6" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    internal interface IMpeg2Data
    {
    }
}
