using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Die primäre Schnittstelle eines DirectShow Graphen.
    /// </summary>
    [
        ComImport,
        Guid( "56a8689f-0ad4-11ce-b03a-0020af0ba770" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IFilterGraph
    {
        /// <summary>
        /// Ergänzt einen Filter mit einem bestimmten Namen.
        /// </summary>
        /// <param name="filter">Die COM Schnittstelle des Filters.</param>
        /// <param name="name">Der Name des Filters.</param>
        void AddFilter( IntPtr filter, [MarshalAs( UnmanagedType.LPWStr )] string name );

        /// <summary>
        /// Entfernt einen Filter.
        /// </summary>
        /// <param name="filter">Die COMSchnittstelle des Filters.</param>
        void RemoveFilter( IntPtr filter );

        /// <summary>
        /// Meldet eine Auflistung über alle Filter im Graphen.
        /// </summary>
        /// <returns>Die angeforderte Auflistung.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IEnumFilters EnumFilters();

        /// <summary>
        /// Sucht einen Filter nach seinem Namen.
        /// </summary>
        /// <param name="name">Der Name des Filters.</param>
        /// <returns>Die COM Schnittstelle zum angeforderten Filter.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IntPtr FindFilterByName( [MarshalAs( UnmanagedType.LPWStr )] string name );

        /// <summary>
        /// Verbindet zwei Endpunkte.
        /// </summary>
        /// <param name="pinOut">Der Endpunkt, der Daten produziert.</param>
        /// <param name="pinIn">Der Endpunkt, der Daten konsumiert.</param>
        /// <param name="mediaType">Die Art der Daten.</param>
        void ConnectDirect( IntPtr pinOut, IntPtr pinIn, IntPtr mediaType );

        /// <summary>
        /// Rekonfiguriert einen Endpunkt.
        /// </summary>
        /// <param name="pPin">Die COM Schnittstelle des Endpunktes.</param>
        void Reconnect( IntPtr pPin );

        /// <summary>
        /// Entfernt alle Verbindungen eines Endpunktes.
        /// </summary>
        /// <param name="pPin">Der betroffene Endpunkt.</param>
        void Disconnect( IntPtr pPin );

        /// <summary>
        /// Legt die Zeitbasis auf einen Vorgabewert fest.
        /// </summary>
        void SetDefaultSyncSource();
    }

    /// <summary>
    /// Erweiterungen zur einfacheren Nutzung der <see cref="IFilterGraph"/> Schnittstelle.
    /// </summary>
    public static class IFilterGraphExtensions
    {
        /// <summary>
        /// Wertet alle Filter in einem Graphen aus.
        /// </summary>
        /// <typeparam name="T">Konkret verwendete Schnittstelle.</typeparam>
        /// <param name="graph">Der zu untersuchende Graph.</param>
        /// <param name="action">Wird für jeden gefundenen Filter aktiviert.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Graph angegeben.</exception>
        public static void InspectFilters<T>( this T graph, Action<IBaseFilter> action ) where T : IFilterGraph
        {
            // Forward
            InspectFilters( graph, filter => { action( filter ); return true; } );
        }

        /// <summary>
        /// Wertet alle Filter in einem Graphen aus.
        /// </summary>
        /// <typeparam name="T">Konkret verwendete Schnittstelle.</typeparam>
        /// <param name="graph">Der zu untersuchende Graph.</param>
        /// <param name="action">Wird für jeden gefundenen Filter aktiviert.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Graph angegeben.</exception>
        public static void InspectFilters<T>( this T graph, Func<IBaseFilter, bool> action ) where T : IFilterGraph
        {
            // Validate
            if (graph == null)
                throw new ArgumentNullException( "graph" );

            // Get enumerator
            var filters = graph.EnumFilters();
            if (filters == null)
                return;

            // List to process
            var process = new List<IBaseFilter>();
            try
            {
                // Process all
                for (; ; )
                    using (var filterArray = new COMArray( 1 ))
                    {
                        // Load
                        uint n;
                        if (filters.Next( 1, filterArray.Address, out n ) != 0)
                            break;
                        if (n != 1)
                            break;

                        // Load
                        process.Add( filterArray.GetObject<IBaseFilter>( 0 ) );
                    }
            }
            catch
            {
                // Disable action
                action = null;

                // Forward
                throw;
            }
            finally
            {
                // Cleanup enumeration
                BDAEnvironment.Release( ref filters );

                // Safe process
                try
                {
                    // Test all
                    if (action != null)
                        foreach (var filter in process)
                            if (!action( filter ))
                                break;
                }
                finally
                {
                    // Cleanup collected filters
                    foreach (var filter in process)
                        if (Marshal.IsComObject( filter ))
                            Marshal.ReleaseComObject( filter );
                }
            }
        }
    }
}
