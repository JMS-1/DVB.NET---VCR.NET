using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Meldet die Komponenten beim Wechsel einer Quellgruppe.
    /// </summary>
    [
        ComImport,
        Guid( "fcd01846-0e19-11d3-9d8e-00c04f72d980" ),
        InterfaceType( ComInterfaceType.InterfaceIsIDispatch )
    ]
    public interface IComponents
    {
    }
}
