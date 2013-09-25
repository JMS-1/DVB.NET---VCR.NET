using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Schnittstelle zur Auflistung von Datentypen.
    /// </summary>
    [
        ComImport,
        Guid( "89c31040-846b-11ce-97d3-00aa0055595a" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IEnumMediaTypes
    {
        /// <summary>
        /// Ermittelt den n�chsten Satz von Informationen.
        /// </summary>
        /// <param name="mediaTypes">Die maximale Anzahl von zu meldenden Informationen.</param>
        /// <param name="mediaTypeArray">Die Ablage der Antwort.</param>
        /// <param name="fetched">Die tats�chlich �bertragenen Informationen.</param>
        /// <returns>Das Ergebnis der Abfrage, negativ im Fehlerfall.</returns>
        [PreserveSig]
        Int32 Next( uint mediaTypes, IntPtr mediaTypeArray, out uint fetched );

        /// <summary>
        /// �berspringt Informationen.
        /// </summary>
        /// <param name="mediaTypes">Die Anzahl der zu �berspringenden Informationen.</param>
        void Skip( uint mediaTypes );

        /// <summary>
        /// Beginnt erneut mit der Auflistung.
        /// </summary>
        void Reset();

        /// <summary>
        /// Erzeugt eine exakte Kopie der Auflistung im aktuellen Stand der Abfrage.
        /// </summary>
        /// <returns></returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IEnumMediaTypes Clone();
    }
}
