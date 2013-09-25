using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Mit dieser Schnittstelle wird die Konfiguration eines BDA DVB Ger�tes ge�ndert.
    /// </summary>
    [
        ComImport,
        Guid( "fd0a5af3-b41d-11d2-9c95-00c04f7971e0" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IBDADeviceControl
    {
        /// <summary>
        /// �nderungen werden nun vorgenommen.
        /// </summary>
        void StartChanges();

        /// <summary>
        /// Pr�ft die vorgenommenen Ver�nderungen auf Konsistenz.
        /// </summary>
        void CheckChanges();

        /// <summary>
        /// Aktiviert alle Ver�nderungen als aktuellen Stand.
        /// </summary>
        void CommitChanges();

        /// <summary>
        /// Meldet den Zustand der Ver�nderungen.
        /// </summary>
        UInt32 ChangeState { get; }
    }
}
