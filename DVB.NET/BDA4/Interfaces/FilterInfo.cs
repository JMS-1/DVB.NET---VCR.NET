using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Meldet die Kerndaten eines Filters.
    /// </summary>
    [
        StructLayout( LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode )
    ]
    public struct FilterInfo
    {
        /// <summary>
        /// Der Name des Filters.
        /// </summary>
        [MarshalAs( UnmanagedType.ByValTStr, SizeConst = 128 )]
        public string Name;

        /// <summary>
        /// Die COM Schnittstelle des DirectShow Graphen, zu dem dieser Filter gehört.
        /// </summary>
        public IntPtr Graph;
    }
}
