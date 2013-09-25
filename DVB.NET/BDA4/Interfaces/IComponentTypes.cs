using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Meldet die Komponentenarten beim Wechsel einer Quellgruppe.
    /// </summary>
    [
		ComImport, 
		Guid("0dc13d4a-0313-11d3-9d8e-00c04f72d980"),
		InterfaceType(ComInterfaceType.InterfaceIsIDispatch)
	]
    public interface IComponentTypes
	{
	} 
}
