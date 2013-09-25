using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Mit dieser Schnittstelle wird die Konfiguration eines BDA DVB Gerätes geändert.
    /// </summary>
    [
        ComImport,
        Guid( "fd0a5af3-b41d-11d2-9c95-00c04f7971e0" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IBDADeviceControl
    {
        /// <summary>
        /// Änderungen werden nun vorgenommen.
        /// </summary>
        void StartChanges();

        /// <summary>
        /// Prüft die vorgenommenen Veränderungen auf Konsistenz.
        /// </summary>
        void CheckChanges();

        /// <summary>
        /// Aktiviert alle Veränderungen als aktuellen Stand.
        /// </summary>
        void CommitChanges();

        /// <summary>
        /// Meldet den Zustand der Veränderungen.
        /// </summary>
        UInt32 ChangeState { get; }
    }
}
