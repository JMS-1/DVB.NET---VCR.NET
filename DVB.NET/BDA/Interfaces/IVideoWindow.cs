using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Diese Schnittstelle wird von einem Darstellungsfenster angeboten.
    /// </summary>
    [
        ComImport,
        Guid( "56a868b4-0ad4-11ce-b03a-0020af0ba770" ),
        InterfaceType( ComInterfaceType.InterfaceIsIDispatch )
    ]
    internal interface IVideoWindow
    {
        /// <summary>
        /// Der Fenstertext der Darstellung.
        /// </summary>
        string Caption { set; get; }

        /// <summary>
        /// Standardeigenschaften des Fensters.
        /// </summary>
        Int32 WindowStyle { set; get; }

        /// <summary>
        /// Erweiterte Eigenschaften des Fensters.
        /// </summary>
        Int32 WindowStyleEx { set; get; }

        /// <summary>
        /// Meldet oder legt fest, ob eine automatisch Anzeige erfolgen soll.
        /// </summary>
        Int32 AutoShow { set; get; }

        /// <summary>
        /// Der aktuelle Zustand des Fensters.
        /// </summary>
        Int32 WindowState { set; get; }

        /// <summary>
        /// Informationen zur Hintergrundfarbe.
        /// </summary>
        Int32 BackgroundPalette { set; get; }

        /// <summary>
        /// Meldet oder legt fest, ob das Fenster sichtbar ist.
        /// </summary>
        Int32 Visible { set; get; }

        /// <summary>
        /// Die horizontale Position der linken oberen Ecke des Fensters.
        /// </summary>
        Int32 Left { set; get; }

        /// <summary>
        /// Die Breite des Fensters.
        /// </summary>
        Int32 Width { set; get; }

        /// <summary>
        /// Die vertikale Position der linken oberen Ecke des Fensters.
        /// </summary>
        Int32 Top { set; get; }

        /// <summary>
        /// Die breite des Fensters.
        /// </summary>
        Int32 Height { set; get; }

        /// <summary>
        /// Eine Referenz auf den Eigentümer des Fensters.
        /// </summary>
        IntPtr Owner { set; get; }

        /// <summary>
        /// Ein Nachrichtenverbraucher für das Fenster.
        /// </summary>
        IntPtr MessageDrain { set; get; }

        /// <summary>
        /// Die Farbe des Rahmens.
        /// </summary>
        Int32 BorderColor { set; get; }

        /// <summary>
        /// Meldet oder legt fest, ob das Fenster im Vollbild angezeigt werden soll.
        /// </summary>
        Int32 FullScreenMode { set; get; }

        /// <summary>
        /// Legt die Vordergrundfarbe fest.
        /// </summary>
        /// <param name="focus"></param>
        void SetWindowForeground( Int32 focus );

        /// <summary>
        /// Verarbeitet Nachrichten für den Eigentümer.
        /// </summary>
        /// <param name="hWindow">Das ursprüngliche Zielfenster der Nachricht.</param>
        /// <param name="message">Die Art der Nachricht.</param>
        /// <param name="wParam">Der erste Parameter der Nachricht.</param>
        /// <param name="lParam">Der zweite Parameter der Nachricht.</param>
        void NotifyOwnerMessage( IntPtr hWindow, Int32 message, IntPtr wParam, IntPtr lParam );

        /// <summary>
        /// Legt die Position des Fensters fest.
        /// </summary>
        /// <param name="left">Die horizontale Position der linken oberen Ecke des Fensters.</param>
        /// <param name="top">Die vertikale Position der linken oberen Ecke des Fensters.</param>
        /// <param name="width">Die Breite des Fensters.</param>
        /// <param name="height">Die Höhe des Fensters.</param>
        void SetWindowPosition( Int32 left, Int32 top, Int32 width, Int32 height );

        /// <summary>
        /// Meldet die Position des Fensters.
        /// </summary>
        /// <param name="left">Die horizontale Position der linken oberen Ecke des Fensters.</param>
        /// <param name="top">Die vertikale Position der linken oberen Ecke des Fensters.</param>
        /// <param name="width">Die Breite des Fensters.</param>
        /// <param name="height">Die Höhe des Fensters.</param>
        void GetWindowPosition( out Int32 left, out Int32 top, out Int32 width, out Int32 height );

        /// <summary>
        /// Meldet die minimale Idealgröße des Fensters.
        /// </summary>
        /// <param name="width">Die Breite des Fensters.</param>
        /// <param name="height">Die Höhe des Fensters.</param>
        void GetMinIdealImageSize( out Int32 width, out Int32 height );

        /// <summary>
        /// Meldet die maximale Idealgröße des Fensters.
        /// </summary>
        /// <param name="width">Die Breite des Fensters.</param>
        /// <param name="height">Die Höhe des Fensters.</param>
        void GetMaxIdealImageSize( out Int32 width, out Int32 height );

        /// <summary>
        /// Ermittelt die Wiederherstellungsposition des Fensters.
        /// </summary>
        /// <param name="left">Die horizontale Position der linken oberen Ecke des Fensters.</param>
        /// <param name="top">Die vertikale Position der linken oberen Ecke des Fensters.</param>
        /// <param name="width">Die Breite des Fensters.</param>
        /// <param name="height">Die Höhe des Fensters.</param>
        void GetRestorePosition( out Int32 left, out Int32 top, out Int32 width, out Int32 height );

        /// <summary>
        /// Ändert die Anzeige des Mauszeigers.
        /// </summary>
        /// <param name="hide">Die neue Art der Anzeige.</param>
        void HideCursor( Int32 hide );

        /// <summary>
        /// Prüft, ob der Mauszeiger angezeigt wird.
        /// </summary>
        /// <returns>Der aktuelle Anzeigestand des Mauszeigers.</returns>
        [return: MarshalAs( UnmanagedType.I4 )]
        Int32 IsCursorHidden();
    }
}
