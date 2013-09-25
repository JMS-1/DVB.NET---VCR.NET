using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Kennzeichnet einen Filter als den Erzeuger von Daten.
    /// </summary>
    [
        ComImport,
        Guid( "f185fe76-e64e-11d2-b76e-00c04fb6bd3d" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    internal interface IAMPushSource // : IAMLatency
    {
        /// <summary>
        /// Meldet die aktuelle Latenz.
        /// </summary>
        /// <returns>Der angeforderte Wert.</returns>
        [return: MarshalAs( UnmanagedType.I8 )]
        long GetLatency();

        /// <summary>
        /// Erfragt Detailinformationen zum Erzeuger.
        /// </summary>
        /// <returns>Die gewünschten Detailinformationen.</returns>
        [return: MarshalAs( UnmanagedType.U4 )]
        UInt32 GetPushSourceFlags();

        /// <summary>
        /// Legt Detailinformationen des Erzeugers fest.
        /// </summary>
        /// <param name="flags">Die neuen Detailinformationen.</param>
        void SetPushSourceFlags( UInt32 flags );

        /// <summary>
        /// Legt den Versatz fest.
        /// </summary>
        /// <param name="offset">Der neue Versatz.</param>
        void SetStreamOffset( long offset );

        /// <summary>
        /// Meldet den aktuellen Versatz.
        /// </summary>
        /// <returns>Der angeforderte Wert.</returns>
        [return: MarshalAs( UnmanagedType.I8 )]
        long GetStreamOffset();

        /// <summary>
        /// Meldet den maximalen Versatz.
        /// </summary>
        /// <returns>Der aktuelle maximale Versatz.</returns>
        [return: MarshalAs( UnmanagedType.I8 )]
        long GetMaxStreamOffset();

        /// <summary>
        /// Legt den maximalen Versatz fest.
        /// </summary>
        /// <param name="offset">Der neue maximale Versatz.</param>
        void SetMaxStreamOffset( long offset );
    }
}
