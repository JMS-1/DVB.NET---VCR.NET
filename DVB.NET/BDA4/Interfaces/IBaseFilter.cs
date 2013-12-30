using System;
using System.Runtime.InteropServices;
using JMS.DVB.DeviceAccess.Topology;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Eine erweiterte Schnittstelle, die von fast allen Filtern angeboten wird.
    /// </summary>
    [
        ComImport,
        Guid( "56a86895-0ad4-11ce-b03a-0020af0ba770" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IBaseFilter // : IMediaFilter
    {
        #region IMediaFilter

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

        #endregion

        /// <summary>
        /// Meldet alle Endpunkte des Filters.
        /// </summary>
        /// <returns>Eine Auflistung über alle Endpunkte.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IEnumPins EnumPins();

        /// <summary>
        /// Sucht einen bestimmten Endpunkt des Filters.
        /// </summary>
        /// <param name="ID">Der eindeutige Name des Endpunktes.</param>
        /// <returns>Die COM Schnittstelle des gewünschten Endpunktes.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IPin FindPin( [MarshalAs( UnmanagedType.LPWStr )] string ID );

        /// <summary>
        /// Ermittelt die Daten eines Filters,
        /// </summary>
        /// <param name="info">Die gewünschten Daten zum Filter.</param>
        void QueryFilterInfo( ref FilterInfo info );

        /// <summary>
        /// Meldet einen Filter in einem DirectShow Graphen an.
        /// </summary>
        /// <param name="graph">Der zu verwendende Graph.</param>
        /// <param name="name">Der Name, unter dem der Filter im Graphen erscheinen soll.</param>
        void JoinFilterGraph( IFilterGraph graph, [MarshalAs( UnmanagedType.LPWStr )] string name );

        /// <summary>
        /// Ermittelt den Namen des Herstellers des Filters.
        /// </summary>
        /// <returns>Der Name des Herstellers, der die Implementierung des Filters bereitgestellt hat.</returns>
        [return: MarshalAs( UnmanagedType.LPWStr )]
        string QueryVendorInfo();
    }

    /// <summary>
    /// Hilfsmethoden zum einfacheren Arbeiten mit der <see cref="IBaseFilter"/> Schnittstelle.
    /// </summary>
    public static class IBaseFilterExtensions
    {
        /// <summary>
        /// Untersucht alle Endpunkte eines Filters.
        /// </summary>
        /// <param name="filter">Der zu betrachtende Filter.</param>
        /// <param name="action">Optional eine Aktion, die pro Endpunkt ausgeführt werden soll.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Filter angegeben.</exception>
        public static void InspectAllPins( this TypedComIdentity<IBaseFilter> filter, Action<IPin> action )
        {
            // Forward
            filter.InspectAllPins( null, action );
        }

        /// <summary>
        /// Untersucht alle Endpunkte eines Filters.
        /// </summary>
        /// <param name="filter">Der zu betrachtende Filter.</param>
        /// <param name="selector">Optional eine Auswahlfunktion.</param>
        /// <param name="action">Optional eine Aktion, die pro Endpunkt ausgeführt werden soll.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Filter angegeben.</exception>
        public static void InspectAllPins( this TypedComIdentity<IBaseFilter> filter, Predicate<IPin> selector, Action<IPin> action )
        {
            // Forward
            filter.InspectAllPins( selector, pin => { action( pin ); return true; } );
        }

        /// <summary>
        /// Untersucht alle Endpunkte eines Filters.
        /// </summary>
        /// <param name="filter">Der zu betrachtende Filter.</param>
        /// <param name="selector">Optional eine Auswahlfunktion.</param>
        /// <param name="action">Optional eine Aktion, die pro Endpunkt ausgeführt werden soll.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Filter angegeben.</exception>
        public static void InspectAllPins( this TypedComIdentity<IBaseFilter> filter, Predicate<IPin> selector, Func<IPin, bool> action )
        {
            // Validate
            if (filter == null)
                throw new ArgumentNullException( "filter" );

            // Attach to the instance
            using (var filterInstance = filter.MarshalToManaged())
            {
                // Attach to all pins
                var pins = filterInstance.Object.EnumPins();
                try
                {
                    // Process all
                    for (; ; )
                        using (var array = new COMArray( 1 ))
                        {
                            // Load
                            uint count = 0;
                            if (pins.Next( 1, array.Address, ref count ) != 0)
                                break;

                            // Load object
                            if (count != 1)
                                continue;

                            // Load the one
                            var pin = array.GetObject<IPin>( 0 );
                            try
                            {
                                // Check predicate
                                if (selector != null)
                                    if (!selector( pin ))
                                        continue;

                                // Execute
                                if (action != null)
                                    if (!action( pin ))
                                        return;
                            }
                            finally
                            {
                                // Cleanup
                                BDAEnvironment.Release( ref pin );
                            }
                        }
                }
                finally
                {
                    // Cleanup
                    BDAEnvironment.Release( ref pins );
                }
            }
        }

        /// <summary>
        /// Ermittelt einen einzelnen Endpunkt.
        /// </summary>
        /// <param name="filter">Der zu betrachtende Filter.</param>
        /// <param name="direction">Die zu untersuchende Seite des Filters.</param>
        /// <returns>Eine Referenz auf den gewünschten Endpunkt.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Filter angegeben.</exception>
        /// <exception cref="InvalidOperationException">Der angegeben Endpunkt existiert nicht.</exception>
        public static TypedComIdentity<IPin> GetSinglePin( this TypedComIdentity<IBaseFilter> filter, PinDirection direction )
        {
            // Validate
            if (filter == null)
                throw new ArgumentNullException( "filter" );

            // Result to use
            TypedComIdentity<IPin> result = null;

            // Use helper to check all of it
            filter.InspectAllPins( p => p.QueryDirection() == direction, p =>
                {
                    // At most one hit is allowed
                    if (result != null)
                    {
                        // Cleanup
                        result.Dispose();

                        // Report error
                        throw new InvalidOperationException( Properties.Resources.Exception_DuplicateEndpoint );
                    }

                    // Remember the first one
                    result = ComIdentity.Create( p );
                } );

            // Ups
            if (result != null)
                return result;
            else
                throw new InvalidOperationException( Properties.Resources.Exception_NoEndpoint );
        }

        /// <summary>
        /// Ermittelt die Demodulatorschnittstelle aus der BDA Topologie des Filters.
        /// </summary>
        /// <param name="filter">Der zu verwendende Filter.</param>
        /// <returns>Die Schnittstelle auf den Demodulator, sofern vorhanden.</returns>
        public static IBDADigitalDemodulator GetDigitalDemodulator( this TypedComIdentity<IBaseFilter> filter )
        {
            // Forward
            return filter.GetOutputControlNode<IBDADigitalDemodulator>( 1 );
        }

        /// <summary>
        /// Ermittelt die Schnittstelle zur Feineinstellung der Quellgruppenanwahl aus der BDA Topologie des Filters.
        /// </summary>
        /// <param name="filter">Der zu verwendende Filter.</param>
        /// <returns>Die Schnittstelle für die Feineinstellungen, sofern vorhanden.</returns>
        public static IBDAFrequencyFilter GetFrequencyFilter( this TypedComIdentity<IBaseFilter> filter )
        {
            // Forward
            return filter.GetOutputControlNode<IBDAFrequencyFilter>( 0 );
        }

        /// <summary>
        /// Ermittelt die Schnittstelle mit den Signalinformationen aus der BDA Topologie des Filters.
        /// </summary>
        /// <param name="filter">Der zu verwendende Filter.</param>
        /// <returns>Die Schnittstelle auf die Signalinformationen, sofern vorhanden.</returns>
        public static IBDASignalStatistics GetSignalStatistics( this TypedComIdentity<IBaseFilter> filter )
        {
            // Forward
            return filter.GetOutputControlNode<IBDASignalStatistics>( 0 );
        }

        /// <summary>
        /// Ermittelt zu einem Filter einen ausgangsseitigen Steuerknoten der Topologie.
        /// </summary>
        /// <typeparam name="T">Die Art der benötigten Schnittstelle.</typeparam>
        /// <param name="filter">Ein Filter.</param>
        /// <param name="nodeType">Die Art des Knotens.</param>
        /// <returns>Der gewünschte Knoten.</returns>
        public static T GetOutputControlNode<T>( this TypedComIdentity<IBaseFilter> filter, uint nodeType ) where T : class
        {
            // Validate
            if (filter == null)
                throw new ArgumentNullException( "filter" );

            // Attach to alternate interface
            using (var instance = filter.MarshalToManaged())
            {
                // Change type  
                var topology = instance.Object as IBDATopology;
                if (topology == null)
                    return null;

                // Load
                var node = topology.GetControlNode( 0, 1, nodeType );
                var result = node as T;

                // Cleanup
                if (result == null)
                    BDAEnvironment.Release( ref node );

                // Report
                return result;
            }
        }
    }
}
