using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Konfiguriert das Seitenverh�ltnis der Darstellung.
    /// </summary>
    [
        ComImport,
        Guid( "00d96c29-bbde-4efc-9901-bb5036392146" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    internal interface IVMRAspectRatioControl9
    {
        /// <summary>
        /// Meldet das aktuelle Seitenverh�ltnis.
        /// </summary>
        /// <returns>Die gew�nschten Daten.</returns>
        [return: MarshalAs( UnmanagedType.U4 )]
        AspectRatioModes GetAspectRatioMode();

        /// <summary>
        /// �ndert das Seitenverh�ltnis.
        /// </summary>
        /// <param name="mode">Das neue Seitenverh�ltnis.</param>
        void SetAspectRatioMode( AspectRatioModes mode );
    }
}
