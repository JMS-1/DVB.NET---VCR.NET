using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt ein einzelnes Medium für einen Anschluss.
    /// </summary>
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct RegPinMedium
    {
        /// <summary>
        /// Eindeutige Kennung des Mediums.
        /// </summary>
        public Guid MediumIdentifier;

        /// <summary>
        /// Der erste freie Parameter.
        /// </summary>
        public UInt32 Parameter1;

        /// <summary>
        /// Der zweite freie Parameter.
        /// </summary>
        public UInt32 Parameter2;
    }
}
