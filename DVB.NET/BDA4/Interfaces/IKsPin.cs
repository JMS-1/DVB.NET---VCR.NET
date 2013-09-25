using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Erlaubt die Abfrage der unterstützten Datentypem.
    /// </summary>
	[
		ComImport,
        Guid("b61178d1-a2d9-11cf-9e53-00aa00a216a1"), 
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
	]
    public interface IKsPin
	{
        /// <summary>
        /// Meldet alle Unterstützten Datentypen.
        /// </summary>
        /// <returns>Die Adresse zu den Informationen.</returns>
        IntPtr KsQueryMediums();
    }
}
