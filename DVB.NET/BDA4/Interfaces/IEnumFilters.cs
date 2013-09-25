using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Meldet eine Liste von Filtern in einem DirectShow Graphen.
    /// </summary>
    [
        ComImport,
        Guid( "56a86893-0ad4-11ce-b03a-0020af0ba770" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IEnumFilters
    {
        /// <summary>
        /// Meldet die nächsten Filter.
        /// </summary>
        /// <param name="filters">Die maximale Anzahl von zu meldenden Filtern.</param>
        /// <param name="filterArray">Ein Speicherbereich zur Aufnahme der Filter.</param>
        /// <param name="fetched">Die Anzahl der tatsächlich gemeldeten Filter.</param>
        /// <returns>Das Ergebnis der Abfrage, wobei negative Werte auf einen Fehler hindeuten.</returns>
        [PreserveSig]
        Int32 Next( uint filters, IntPtr filterArray, out uint fetched );

        /// <summary>
        /// Überspringt Filter in der Liste.
        /// </summary>
        /// <param name="filters">Die Anzahl der zu überspringenden Filter.</param>
        void Skip( uint filters );

        /// <summary>
        /// Beginnt die Auflistung erneut mit dem ersten Filter.
        /// </summary>
        void Reset();

        /// <summary>
        /// Erzeugt eine exakte Kopier der Auflistung im aktuellen Zustand der Abfrage.
        /// </summary>
        /// <returns></returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IEnumFilters Clone();
    }
}
