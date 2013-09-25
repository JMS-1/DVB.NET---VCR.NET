using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Topology
{
    /// <summary>
    /// Diese Struktur beschreibt ein einzelnes Hardwaregerät innerhalb einer BDA
    /// Komponente.
    /// </summary>
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct NodeDescriptor
    {
        /// <summary>
        /// Die zugehörige Nummer des Hardwaregerätes.
        /// </summary>
        public UInt32 NodeType;

        /// <summary>
        /// Bezeichnet die Funktionalität des Gerätes, e.g. ob es sich um den Tuner handelt.
        /// </summary>
        public Guid Function;

        /// <summary>
        /// Eindeutige Kennung zur Ermittelung des Namens dieses Gerätes.
        /// </summary>
        public Guid Name;
    }
}
