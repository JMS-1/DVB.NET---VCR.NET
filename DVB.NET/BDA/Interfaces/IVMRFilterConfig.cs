using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Konfiguriert den Anzeigefilter eines Graphen.
    /// </summary>
    [
        ComImport,
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown ),
        Guid( "5a804648-4f66-4867-9c43-4f5c822cf1b8" )
    ]
    internal interface IVMRFilterConfig
    {
        /// <summary>
        /// Legt den Bildaufbau fest.
        /// </summary>
        /// <param name="lpVMRImgCompositor">Komponente zum Bildaufbau.</param>
        void SetImageCompositor( IVMRImageCompositor lpVMRImgCompositor );

        /// <summary>
        /// Definiert die Anzahl der Datenströme.
        /// </summary>
        /// <param name="dwMaxStreams">Die maximale Anzahl von Datenströmen.</param>
        void SetNumberOfStreams( uint dwMaxStreams );

        /// <summary>
        /// Meldet die maximale Anzahl von Datenströmen.
        /// </summary>
        /// <returns>Die aktuelle Maximalzahl von Datenströmen.</returns>
        [return: MarshalAs( UnmanagedType.U4 )]
        uint GetNumberOfStreams();

        /// <summary>
        /// Setzt die bevorzugten Parameter zur Darstellung.
        /// </summary>
        /// <param name="preferences">Die neuen Parameter.</param>
        void SetRenderingPrefs( uint preferences );

        /// <summary>
        /// Meldet die aktuellen Parameter zur Darstellung.
        /// </summary>
        /// <returns>Die zurzeit verwendeten Parameter.</returns>
        [return: MarshalAs( UnmanagedType.U4 )]
        uint GetRenderingPrefs();

        /// <summary>
        /// Ändert den Darstellungsmodus.
        /// </summary>
        /// <param name="mode">Der neue Darstellungsmodus.</param>
        void SetRenderingMode( VMRModes mode );

        /// <summary>
        /// Meldet den aktuellen Darstellunsmodus.
        /// </summary>
        /// <returns>Der gerade verwendete Darstellungsmodus.</returns>
        [return: MarshalAs( UnmanagedType.I4 )]
        VMRModes GetRenderingMode();
    }
}
