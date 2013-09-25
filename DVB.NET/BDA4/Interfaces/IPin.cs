using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt einen Endpunkt eines Filters.
    /// </summary>
    [
        ComImport,
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown ),
        Guid( "56a86891-0ad4-11ce-b03a-0020af0ba770" )
    ]
    public interface IPin
    {
        /// <summary>
        /// Verbindet den Endpunkt mit einem anderen.
        /// </summary>
        /// <param name="receivePin">Ein Endpunkt, der Daten verbraucht.</param>
        /// <param name="mediaType">Die Art der Daten.</param>
        void Connect( IntPtr receivePin, IntPtr mediaType );

        /// <summary>
        /// Verbindet den Endpunkt mit einem anderen.
        /// </summary>
        /// <param name="connector">Ein Endpunkt, der Daten produziert.</param>
        /// <param name="mediaType">Die Art der Daten.</param>
        void ReceiveConnection( IntPtr connector, IntPtr mediaType );

        /// <summary>
        /// Löst alle Verbindungen zu diesem Endpunkt.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Prüft, ob dieser Endpunkt mit einem anderen verbunden ist.
        /// </summary>
        /// <param name="other">Der andere Endpunkt.</param>
        /// <returns>Ergebnis der Prüfung, negativ im Fehlerfalle.</returns>
        [PreserveSig]
        Int32 ConnectedTo( ref IntPtr other );

        /// <summary>
        /// Meldet die Art der Daten, die über diesen Endpunkt transportiert werden.
        /// </summary>
        /// <param name="mediaType">Die gewünschten Art der Daten.</param>
        void ConnectionMediaType( ref RawMediaType mediaType );

        /// <summary>
        /// Ermittelt weitergehende Informationen zu diesem Endpunkt.
        /// </summary>
        /// <param name="info">Die gewünschten Informationen.</param>
        void QueryPinInfo( ref PinInfo info );

        /// <summary>
        /// Meldet die Art dieses Endpunktes.
        /// </summary>
        /// <returns>Die gewünschte Art des Endpunktes.</returns>
        [return: MarshalAs( UnmanagedType.I4 )]
        PinDirection QueryDirection();

        /// <summary>
        /// Meldet den eindeutigen Namen des Endpunktes.
        /// </summary>
        /// <returns>Der gewünschte Name.</returns>
        [return: MarshalAs( UnmanagedType.LPWStr )]
        string QueryId();

        /// <summary>
        /// Prüft, ob der Endpunkt eine bestimmte Art von Daten verarbeiten kann.
        /// </summary>
        /// <param name="mediaType">Eine Art von Daten.</param>
        /// <returns>Das Ergebnis der Prüfung, negative Werte zeigen eine Fehlersituation an.</returns>
        [PreserveSig]
        Int32 QueryAccept( IntPtr mediaType );

        /// <summary>
        /// Listet alle Arten von Daten auf, mit denen dieser Endpunkt umgehen kann.
        /// </summary>
        /// <returns>Die gewünschte Auflistung.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IEnumMediaTypes EnumMediaTypes();

        /// <summary>
        /// Fragt interne Verbindungen im Filter ab.
        /// </summary>
        /// <param name="pin">Die COM Schnittstelle eines anderen Endpunktes.</param>
        /// <param name="pinIndex">Die laufende Nummer eines Endpunktes.</param>
        void QueryInternalConnections( out IPin pin, ref uint pinIndex );

        /// <summary>
        /// Meldet das Ende des Datenflusses.
        /// </summary>
        void EndOfStream();

        /// <summary>
        /// Beginnt mit dem Entleeren von Zwischenspeichern.
        /// </summary>
        void BeginFlush();

        /// <summary>
        /// Beendet das Entleeren von Zwischenspeichern.
        /// </summary>
        void EndFlush();

        /// <summary>
        /// Meldet den Beginn eines Abschnitts.
        /// </summary>
        /// <param name="tStart">Der Anfangszeitpunkt.</param>
        /// <param name="tStop">Der Endzeitpunkt.</param>
        /// <param name="dRate">Die Raten der Einheiten.</param>
        void NewSegment( long tStart, long tStop, double dRate );
    }

    /// <summary>
    /// Hilfsmethoden zum Arbeiten mit der <see cref="IPin"/> Schnittstelle.
    /// </summary>
    public static class IPinExtensions
    {
        /// <summary>
        /// Untersucht alle Datenformate eines Endpunktes.
        /// </summary>
        /// <param name="pin">Der zu betrachtende Endpunkt.</param>
        /// <param name="action">Optional eine Aktion, die pro Format ausgeführt werden soll.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Endpunkt angegeben.</exception>
        public static void InspectAllMediaTypes<T>( this T pin, Func<MediaType, bool> action ) where T : IPin
        {
            // Forward
            pin.InspectAllMediaTypes( null, action );
        }

        /// <summary>
        /// Untersucht alle Datenformate eines Endpunktes.
        /// </summary>
        /// <param name="pin">Der zu betrachtende Endpunkt.</param>
        /// <param name="selector">Optional eine Auswahlfunktion.</param>
        /// <param name="action">Optional eine Aktion, die pro Format ausgeführt werden soll.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Endpunkt angegeben.</exception>
        public static void InspectAllMediaTypes<T>( this T pin, Predicate<MediaType> selector, Func<MediaType, bool> action ) where T : IPin
        {
            // Validate
            if (pin == null)
                throw new ArgumentNullException( "pin" );

            // Attach to all pins
            var types = pin.EnumMediaTypes();
            try
            {
                // Process all
                for (; ; )
                    using (var array = new COMArray( 1, false ))
                    {
                        // Load
                        uint count;
                        if (types.Next( 1, array.Address, out count ) != 0)
                            break;

                        // Load object
                        if (count != 1)
                            continue;

                        // Load the one
                        using (var type = new MediaType( array[0], false ))
                        {
                            // Check predicate
                            if (selector != null)
                                if (!selector( type ))
                                    continue;

                            // Execute
                            if (action != null)
                                if (!action( type ))
                                    return;
                        }
                    }
            }
            finally
            {
                // Cleanup
                BDAEnvironment.Release( ref types );
            }
        }

        /// <summary>
        /// Ermittelt die von diesem Anschluss unterstützte Medien.
        /// </summary>
        /// <param name="pin">Ein Anschluss.</param>
        /// <returns>Die Liste der Medien, sofern verfügbar.</returns>
        public static RegPinMedium[] GetMediumArray( this IPin pin )
        {
            // Change the interface
            var kernelPin = pin as IKsPin;
            if (kernelPin == null)
                return new RegPinMedium[0];

            // Request the raw data
            var raw = kernelPin.KsQueryMediums();
            try
            {
                // Load the number of media
                var result = new RegPinMedium[Marshal.ReadInt32( raw, 4 )];

                // Load it all
                for (int i = result.Length, s = Marshal.SizeOf( typeof( RegPinMedium ) ); i-- > 0; )
                {
                    // Get the reference
                    var addr = new IntPtr( raw.ToInt64() + 8 + s * i );

                    // Reconstruct
                    result[i] = (RegPinMedium) Marshal.PtrToStructure( addr, typeof( RegPinMedium ) );
                }

                // Report
                return result;
            }
            finally
            {
                // Cleanup native memory
                if (raw != IntPtr.Zero)
                    Marshal.FreeCoTaskMem( raw );
            }
        }
    }
}
