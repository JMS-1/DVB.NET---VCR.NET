using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt einen Endpunkt.
    /// </summary>
    [
        StructLayout( LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode )
    ]
    public struct PinInfo
    {
        /// <summary>
        /// Der Filter, zu dem dieser Endpunkt gehört.
        /// </summary>
        public IntPtr Filter;

        /// <summary>
        /// Die Art, wie dieser Endpunkt mit Daten umgeht.
        /// </summary>
        public PinDirection Direction;

        /// <summary>
        /// Der Name des Endpunktes.
        /// </summary>
        [MarshalAs( UnmanagedType.ByValTStr, SizeConst = 128 )]
        public string Name;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="direction">Die Übertragunsrichtung des Endpunktes.</param>
        /// <param name="name">Der eindeutige Name des Endpunktes.</param>
        /// <param name="filter">Der Filter, zu dem dieser Endpunkt gehört.</param>
        public PinInfo( PinDirection direction, string name, IBaseFilter filter )
        {
            // Remember all
            Filter = Marshal.GetComInterfaceForObject( filter, typeof( IBaseFilter ) );
            Direction = direction;
            Name = name;
        }

        /// <summary>
        /// Meldet den zugehörigen Filter und löst diesen aus der Struktur.
        /// </summary>
        /// <returns>Eine Referenz auf den Filter.</returns>
        public TypedComIdentity<IBaseFilter> GetAndDisposeFilter()
        {
            // Process
            try
            {
                // Check mode
                return new TypedComIdentity<IBaseFilter>( Filter );
            }
            finally
            {
                // Always forget
                DisposeFilter();
            }
        }

        /// <summary>
        /// Gibt die Filterreferenz zu diesem Endpunkt frei.
        /// </summary>
        public void DisposeFilter()
        {
            // Free COM reference
            BDAEnvironment.Release( ref Filter );
        }
    }
}
