using System;
using System.Drawing;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Beschreib die Parameter einer fensterlosen Darstellung.
    /// </summary>
    [
        ComImport,
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown ),
        Guid( "8f537d09-f85e-4414-b23b-502e54c79927" )
    ]
    internal interface IVMRWindowlessControl
    {
        /// <summary>
        /// Meldet die nat�rliche Gr��e.
        /// </summary>
        /// <param name="width">Die Breite.</param>
        /// <param name="height">Die H�he.</param>
        /// <param name="aspectWidth">Der Breitenanteil des Bildverh�ltnisses.</param>
        /// <param name="aspectHeight">Der H�henanteil des Bildverh�ltnisses.</param>
        void GetNativeVideoSize( out int width, out int height, out int aspectWidth, out int aspectHeight );

        /// <summary>
        /// Meldet die minimale ideale Gr��e.
        /// </summary>
        /// <param name="width">Die Breite.</param>
        /// <param name="height">Die H�he.</param>
        void GetMinIdealVideoSize( out int width, out int height );

        /// <summary>
        /// Meldet die maximale ideale Gr��e.
        /// </summary>
        /// <param name="width">Die Breite.</param>
        /// <param name="height">Die H�he.</param>
        void GetMaxIdealVideoSize( out int width, out int height );

        /// <summary>
        /// Legt die Darstellungsbereiche fest.
        /// </summary>
        /// <param name="source">Der Quellbereich.</param>
        /// <param name="destination">Der Zielbereich.</param>
        void SetVideoPosition( ref Rectangle source, ref Rectangle destination );

        /// <summary>
        /// Meldet die Darstellungsbereiche.
        /// </summary>
        /// <param name="source">Der Quellbereich.</param>
        /// <param name="destination">Der Zielbereich.</param>
        void GetVideoPosition( out Rectangle source, out Rectangle destination );

        /// <summary>
        /// Meldet die Art des Bildverh�ltnisses.
        /// </summary>
        /// <returns>Die aktuelle Art.</returns>
        [return: MarshalAs( UnmanagedType.I4 )]
        AspectRatioModes GetAspectRatioMode();

        /// <summary>
        /// Legt die Art des Bildverh�ltnisses fest.
        /// </summary>
        /// <param name="mode">Die ab nun zu verwendende Art.</param>
        void SetAspectRatioMode( AspectRatioModes mode );

        /// <summary>
        /// Legt das Referenzfenster fest.
        /// </summary>
        /// <param name="videoWindow">Das zu verwendende Referenzfenster.</param>
        void SetVideoClippingWindow( IntPtr videoWindow );

        /// <summary>
        /// Fordert zur erneuten Darstellung auf.
        /// </summary>
        /// <param name="videoWindow">Das Darstellungsfenster.</param>
        /// <param name="deviceContext">Der zu verwendende Ger�tekontext.</param>
        void RepaintVideo( IntPtr videoWindow, IntPtr deviceContext );

        /// <summary>
        /// Teilt mit, dass sich der Anzeigemodus ver�ndert hat.
        /// </summary>
        void DisplayModeChanged();

        /// <summary>
        /// Ermittelt das aktuelle Bild.
        /// </summary>
        /// <param name="dib">Das aktuell angezeigte Bild.</param>
        void GetCurrentImage( out IntPtr dib );

        /// <summary>
        /// Setzt die Farbe der Umrandung.
        /// </summary>
        /// <param name="color">Die gew�nschte Farbe.</param>
        void SetBorderColor( uint color );

        /// <summary>
        /// Meldet die Farbe der Umrandung.
        /// </summary>
        /// <returns>Die aktuell verwendete Farbe.</returns>
        [return: MarshalAs( UnmanagedType.U4 )]
        uint GetBorderColor();

        /// <summary>
        /// Legt die Schl�sselfarbe fest.
        /// </summary>
        /// <param name="color">Die zu verwendende Schl�sselfarbe.</param>
        void SetColorKey( uint color );

        /// <summary>
        /// Meldet die Schl�sselfarbe.
        /// </summary>
        /// <returns>Die aktuelle Schl�sselfarbe.</returns>
        [return: MarshalAs( UnmanagedType.U4 )]
        uint GetColorKey();
    }
}
