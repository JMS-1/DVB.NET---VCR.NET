using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Erlaubt den Zugriff auf die Latenzzeit im Datenstrom.
    /// </summary>
    [
        ComImport,
        Guid( "62ea93ba-ec62-11d2-b770-00c04fb6bd3d" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    internal interface IAMLatency
    {
        /// <summary>
        /// Meldet die aktuelle Latenz.
        /// </summary>
        /// <returns>Der angeforderte Wert.</returns>
        [return: MarshalAs( UnmanagedType.I8 )]
        long GetLatency();
    }
}
