using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Basisschnittstelle für alle persistierbaren Objekte.
    /// </summary>
    [
        ComImport,
        Guid( "0000010c-0000-0000-c000-000000000046" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IPersist
    {
        /// <summary>
        /// Meldet die eindeutige Kennung der Implementierungsklasse des Filters.
        /// </summary>
        /// <param name="classID">Die angeforderte Kennung.</param>
        void GetClassID( out Guid classID );
    }
}
