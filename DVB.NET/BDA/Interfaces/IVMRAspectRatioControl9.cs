using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Konfiguriert das Seitenverhältnis der Darstellung.
    /// </summary>
    [
        ComImport,
        Guid( "00d96c29-bbde-4efc-9901-bb5036392146" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    internal interface IVMRAspectRatioControl9
    {
        /// <summary>
        /// Meldet das aktuelle Seitenverhältnis.
        /// </summary>
        /// <returns>Die gewünschten Daten.</returns>
        [return: MarshalAs( UnmanagedType.U4 )]
        AspectRatioModes GetAspectRatioMode();

        /// <summary>
        /// Ändert das Seitenverhältnis.
        /// </summary>
        /// <param name="mode">Das neue Seitenverhältnis.</param>
        void SetAspectRatioMode( AspectRatioModes mode );
    }
}
