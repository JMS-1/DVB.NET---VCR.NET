using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt eine Eigenschaft.
    /// </summary>
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct KsIdentifier
    {
        /// <summary>
        /// Die Gruppe der Eigenschaft.
        /// </summary>
        public Guid Set;

        /// <summary>
        /// Die Kennung der Eigenschaft.
        /// </summary>
        public UInt32 Id;

        /// <summary>
        /// Informationen zur Eigenschaft.
        /// </summary>
        public UInt32 Flags;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <typeparam name="T">Die Art der Auflistung.</typeparam>
        /// <param name="propertySetIdentifier">Die eindeutige Kennung der Gruppe von Eigenschaften.</param>
        /// <param name="identifier">Die gewünschte Eigenschaft.</param>
        /// <returns>Die neue Beschreibung.</returns>
        public static KsIdentifier Create<T>( Guid propertySetIdentifier, T identifier ) where T : struct
        {
            // Report
            return new KsIdentifier { Set = propertySetIdentifier, Id = Convert.ToUInt32( identifier ) };
        }
    }
}
