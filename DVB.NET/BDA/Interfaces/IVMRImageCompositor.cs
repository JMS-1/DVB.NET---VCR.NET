using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Schnittstelle einer Komponente zum Aufbau der Bilddarstellung.
    /// </summary>
    [
        ComImport,
        Guid( "4a5c89eb-df51-4654-ac2a-e48e02bbabf6" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    internal interface IVMRImageCompositor
    {
    }
}
