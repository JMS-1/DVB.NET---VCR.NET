using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Wird von einer Auflistung über eindeutige Kennungen angeboten.
    /// </summary>
    [
        ComImport,
        Guid( "0002e000-0000-0000-c000-000000000046" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IEnumGUID
    {
        /// <summary>
        /// Ermittelt die nächsten Kennungen.
        /// </summary>
        /// <param name="guids">Die maximale Anzahl der zu meldenden Kennungen.</param>
        /// <param name="guidArray">Die zu befüllende Liste mit Kennungen.</param>
        /// <param name="fetched">Die Anzahl der übertragenen Kennungen.</param>
        /// <returns>Das Ergebnis des Abrufs, negativ für Fehlersitationen.</returns>
        [PreserveSig]
        Int32 Next( uint guids, IntPtr guidArray, out uint fetched );

        /// <summary>
        /// Überspringt Kennungen in der Auflistung.
        /// </summary>
        /// <param name="guids">Die Zahl der zu überspringenden Kennungen.</param>
        void Skip( uint guids );

        /// <summary>
        /// Beginnt die Auflistung erneut bei der ersten Kennung.
        /// </summary>
        void Reset();

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Auflistung im aktuellen Abfragestand.
        /// </summary>
        /// <returns></returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IEnumGUID Clone();
    }
}
