using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Schnittstelle zum Zugriff auf die Entschl�sselung �ber ein BDA DVB Ger�t.
    /// </summary>
    [
        ComImport,
        Guid( "cd51f1e0-7be9-4123-8482-a2a796c0a6b0" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IBDAConditionalAccess
    {
    }
}
