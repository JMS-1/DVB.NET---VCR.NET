using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Meldet Informationen zu einer Liste von Endpunkten.
    /// </summary>
    [
        ComImport,
        Guid( "56a86892-0ad4-11ce-b03a-0020af0ba770" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IEnumPins
    {
        /// <summary>
        /// Meldet die nächsten Endpunkte.
        /// </summary>
        /// <param name="pins">Die maximal zu meldende Anzahl von Endpunkten.</param>
        /// <param name="pinArray">Ein Speicher zur Aufnahme der Informationen.</param>
        /// <param name="fetched">Die tatsächlich übermittelte Anzahl von Endpunkten.</param>
        /// <returns>Das Ergebnis der Abfrage, negative Werte zeigen einen Fehler an.</returns>
        [PreserveSig]
        Int32 Next( uint pins, IntPtr pinArray, IntPtr fetched );

        /// <summary>
        /// Überspring Endpunkte in der Auflistung.
        /// </summary>
        /// <param name="pins">Die Anzahl der zu überspringenden Endpunkte.</param>
        void Skip( uint pins );

        /// <summary>
        /// Beginnt die Auflistung erneut beim ersten Endpunkt.
        /// </summary>
        void Reset();

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Auflistung im aktuellen Abfragestand.
        /// </summary>
        /// <returns></returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IEnumPins Clone();
    }

    /// <summary>
    /// Hilfsmethoden zur einfacheren Nutzung der <see cref="IEnumPins"/> Schnittstelle.
    /// </summary>
    public static class IEnumPinsExtensions
    {
        /// <summary>
        /// Meldet die nächsten Endpunkte.
        /// </summary>
        /// <param name="enum">Die zu erweiternde Schnittstelle.</param>
        /// <param name="pins">Die maximal zu meldende Anzahl von Endpunkten.</param>
        /// <param name="pinArray">Ein Speicher zur Aufnahme der Informationen.</param>
        /// <param name="fetched">Die tatsächlich übermittelte Anzahl von Endpunkten.</param>
        /// <returns>Das Ergebnis der Abfrage, negative Werte zeigen einen Fehler an.</returns>
        /// <exception cref="ArgumentNullException">Es wurde keine Schnittstelle angegeben.</exception>
        public static Int32 Next( this IEnumPins @enum, uint pins, IntPtr pinArray, ref uint fetched )
        {
            // Validate
            if (@enum == null)
                throw new ArgumentNullException( "enum" );

            // Helper
            var asArray = new[] { fetched };

            // Lock in memory
            var l = GCHandle.Alloc( asArray, GCHandleType.Pinned );
            try
            {
                // Process
                return @enum.Next( pins, pinArray, l.AddrOfPinnedObject() );
            }
            finally
            {
                // Release the lock
                l.Free();

                // Copy back
                fetched = asArray[0];
            }
        }
    }
}
