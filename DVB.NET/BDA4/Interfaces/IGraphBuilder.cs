using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Schnittstelle zum Aufbau eines DirectShow Graphen.
    /// </summary>
    [
        ComImport,
        Guid( "56a868a9-0ad4-11ce-b03a-0020af0ba770" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IGraphBuilder // : IFilterGraph
    {
        #region IFilterGraph

        /// <summary>
        /// Ergänzt einen Filter mit einem bestimmten Namen.
        /// </summary>
        /// <param name="filter">Die COM Schnittstelle des Filters.</param>
        /// <param name="name">Der Name des Filters.</param>
        void AddFilter( IntPtr filter, [MarshalAs( UnmanagedType.LPWStr )] string name );

        /// <summary>
        /// Entfernt einen Filter.
        /// </summary>
        /// <param name="filter">Die COMSchnittstelle des Filters.</param>
        void RemoveFilter( IntPtr filter );

        /// <summary>
        /// Meldet eine Auflistung über alle Filter im Graphen.
        /// </summary>
        /// <returns>Die angeforderte Auflistung.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IEnumFilters EnumFilters();

        /// <summary>
        /// Sucht einen Filter nach seinem Namen.
        /// </summary>
        /// <param name="name">Der Name des Filters.</param>
        /// <returns>Die COM Schnittstelle zum angeforderten Filter.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IntPtr FindFilterByName( [MarshalAs( UnmanagedType.LPWStr )] string name );

        /// <summary>
        /// Verbindet zwei Endpunkte.
        /// </summary>
        /// <param name="pinOut">Der Endpunkt, der Daten produziert.</param>
        /// <param name="pinIn">Der Endpunkt, der Daten konsumiert.</param>
        /// <param name="mediaType">Die Art der Daten.</param>
        void ConnectDirect( IntPtr pinOut, IntPtr pinIn, IntPtr mediaType );

        /// <summary>
        /// Rekonfiguriert einen Endpunkt.
        /// </summary>
        /// <param name="pPin">Die COM Schnittstelle des Endpunktes.</param>
        void Reconnect( IntPtr pPin );

        /// <summary>
        /// Entfernt alle Verbindungen eines Endpunktes.
        /// </summary>
        /// <param name="pPin">Der betroffene Endpunkt.</param>
        void Disconnect( IntPtr pPin );

        /// <summary>
        /// Legt die Zeitbasis auf einen Vorgabewert fest.
        /// </summary>
        void SetDefaultSyncSource();

        #endregion

        /// <summary>
        /// Verbindet zwei Endpunkte.
        /// </summary>
        /// <param name="pinOut">Der Endpunkt, der Daten produziert.</param>
        /// <param name="pinIn">Der Endpunkt, der Daten konsumiert.</param>
        void Connect( IntPtr pinOut, IntPtr pinIn );

        /// <summary>
        /// Erstellt alle notwendigen Filter und Verbindungen für einen Endpunkt.
        /// </summary>
        /// <param name="pinOut">Ein Endpunkt, der Daten produziert.</param>
        void Render( IntPtr pinOut );

        /// <summary>
        /// Erzeugt zu einer Datei einen Graphen.
        /// </summary>
        /// <param name="fileName">Der volle Pfad zur Datei.</param>
        /// <param name="playList">Informationen zum Abspielvorgang.</param>
        void RenderFile( [MarshalAs( UnmanagedType.LPWStr )] string fileName, [MarshalAs( UnmanagedType.LPWStr )] string playList );

        /// <summary>
        /// Erzeugt aus einer Datei einen Quellfilter im Graphen.
        /// </summary>
        /// <param name="fileName">Der volle Pfad zur Datei.</param>
        /// <param name="filterName">Der Name des zu erzeugenden Filters.</param>
        /// <returns>Die COM Schnittstelle des neuen Filters.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        IBaseFilter AddSourceFilter( [MarshalAs( UnmanagedType.LPWStr )] string fileName, [MarshalAs( UnmanagedType.LPWStr )] string filterName );

        /// <summary>
        /// Legt eine Protokolldatei fest.
        /// </summary>
        /// <param name="hFile">Das Win32 API <i>HANDLE</i> zu einer bereits geöffneten Datei.</param>
        void SetLogFile( SafeFileHandle hFile );

        /// <summary>
        /// Beendet den Aufbau eines Graphen.
        /// </summary>
        void Abort();

        /// <summary>
        /// Prüft, ob der Aufbau des Graphen fortgesetzt werden kann.
        /// </summary>
        void ShouldOperationContinue();
    }
}
