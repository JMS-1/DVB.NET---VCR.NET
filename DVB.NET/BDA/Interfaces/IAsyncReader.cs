using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Beschreibt eine asynchrone Datenerzeugung.
    /// </summary>
    [
        ComImport,
        Guid( "56a868aa-0ad4-11ce-b03a-0020af0ba770" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    internal interface IAsyncReader
    {
    }
}
