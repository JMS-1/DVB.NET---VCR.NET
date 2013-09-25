using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.Provider.Duoflex
{
    /// <summary>
    /// Beschreibt die Auswahl einer Eigenschaft.
    /// </summary>
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    internal struct KsEntity
    {
        /// <summary>
        /// Die Größe dieser Datenstruktur.
        /// </summary>
        public static readonly int SizeOf = Marshal.SizeOf( typeof( KsEntity ) );

        /// <summary>
        /// Die eindeutige Kennung der Eigenschaftsgruppe.
        /// </summary>
        public Guid Set;

        /// <summary>
        /// Die laufende Nummer der Eigenschaft.
        /// </summary>
        public UInt32 Id;

        /// <summary>
        /// Parameter zur gewünschten Operation auf der Eigenschaft.
        /// </summary>
        public UInt32 Flags;
    }
}
