using System;
using System.Runtime.InteropServices;

using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Basisklasse für alle Filter.
    /// </summary>
    [
        ComImport,
        Guid( "56a86899-0ad4-11ce-b03a-0020af0ba770" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IMediaFilter // : IPersist
    {
        #region IPersist

        /// <summary>
        /// Meldet die eindeutige Kennung der Implementierungsklasse des Filters.
        /// </summary>
        /// <param name="classID">Die angeforderte Kennung.</param>
        void GetClassID( out Guid classID );

        #endregion

        /// <summary>
        /// Beendet den Datenfluss des Filters.
        /// </summary>
        /// <returns>Ergebnis der Operation, negativ bei Fehlern.</returns>
        [PreserveSig]
        Int32 Stop();

        /// <summary>
        /// Hält den Datenfluss im Filter an.
        /// </summary>
        /// <returns>Ergebnis der Operation, negativ bei Fehlern.</returns>
        [PreserveSig]
        Int32 Pause();

        /// <summary>
        /// Startet den Datenfluss des Filters.
        /// </summary>
        /// <param name="start">Die Systemzeit des Startzeitpunktes.</param>
        /// <returns>Ergebnis der Operation, negativ bei Fehlern.</returns>
        [PreserveSig]
        Int32 Run( long start );

        /// <summary>
        /// Meldet den aktuellen Zustand des Filters.
        /// </summary>
        /// <param name="millisecondsTimeout">Die maximale Wartezeit auf die Bereitstellung des Zustands.</param>
        /// <returns>Der Zustand des Filters.</returns>
        FilterStates GetState( uint millisecondsTimeout );

        /// <summary>
        /// Legt für einen Filter die Systemuhr fest.
        /// </summary>
        /// <param name="clock">Die COM Schnittstelle der Systemzeit.</param>
        void SetSyncSource( IntPtr clock );

        /// <summary>
        /// Meldet die Systemzeit eines Filters.
        /// </summary>
        /// <returns>Die COM Schnittstelle der Instanz, die für die Systemzeit verantwortlich ist.</returns>
        IntPtr GetSyncSource();
    }
}
