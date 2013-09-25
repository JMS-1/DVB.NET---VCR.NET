using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Schnittstelle zur Auflistung eindeutiger Namen.
    /// </summary>
    [
        ComImport,
        Guid( "00000102-0000-0000-c000-000000000046" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IEnumMoniker
    {
        /// <summary>
        /// Ermittelt die nächsten Namen.
        /// </summary>
        /// <param name="monikers">Die maximale Anzahl der zu meldenden Namen.</param>
        /// <param name="monikerArray">Der Speicherbereich für die Ablage der Namen.</param>
        /// <param name="fetched">Die Anzahl der tatsächlich ermittelten Namen.</param>
        /// <returns>Ergebnis der Abfrage, negative Werte bedeuten, dass ein Fehler aufgetreten ist.</returns>
        [PreserveSig]
        Int32 Next( uint monikers, IntPtr monikerArray, out uint fetched );

        /// <summary>
        /// Überspringt Namen in der Auflistung.
        /// </summary>
        /// <param name="monikers">Die Anzahl der zu überspringenden Namen.</param>
        void Skip( uint monikers );

        /// <summary>
        /// Beginnt die Auflistung wieder von vorne.
        /// </summary>
        void Reset();

        /// <summary>
        /// Erzeugt eine neue Auflistung im aktuellen Abfragestand.
        /// </summary>
        /// <returns></returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IEnumMoniker Clone();
    }
}
