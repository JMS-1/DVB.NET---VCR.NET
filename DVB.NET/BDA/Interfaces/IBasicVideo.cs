using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.Interfaces
{
    /// <summary>
    /// Basisschnittstelle zum Zugriff auf die Darstellung der Bilddaten.
    /// </summary>
    [
        ComImport,
        Guid( "56a868b5-0ad4-11ce-b03a-0020af0ba770" ),
        InterfaceType( ComInterfaceType.InterfaceIsIDispatch )
    ]
    internal interface IBasicVideo
    {
        /// <summary>
        /// Meldet die mittlere Zeit pro Bild.
        /// </summary>
        double AvgTimePerFrame { get; }

        /// <summary>
        /// Meldet die Datenrate in Bits.
        /// </summary>
        Int32 BitRate { get; }

        /// <summary>
        /// Meldet die Fehlerrate der Daten.
        /// </summary>
        Int32 BitErrorRate { get; }

        /// <summary>
        /// Meldet die Breite des dargestellten Bildes.
        /// </summary>
        Int32 VideoWidth { get; }

        /// <summary>
        /// Meldet die Höhe des dargestellten Bildes.
        /// </summary>
        Int32 VideoHeight { get; }

        /// <summary>
        /// Meldet oder setzt die horizontale Position der oberen linken Ecke des Quellbildes.
        /// </summary>
        Int32 SourceLeft { set; get; }

        /// <summary>
        /// Meldet oder setzt die Breite des Quellbildes.
        /// </summary>
        Int32 SourceWidth { set; get; }

        /// <summary>
        /// Meldet oder setzt die vertikale Position der oberen linken Ecke des Quellbildes.
        /// </summary>
        Int32 SourceTop { set; get; }

        /// <summary>
        /// Meldet oder setzt die Höhe des Quellbildes.
        /// </summary>
        Int32 SourceHeight { set; get; }

        /// <summary>
        /// Meldet oder setzt die horizontale Position der oberen linken Ecke des Zielbereichs.
        /// </summary>
        Int32 DestinationLeft { set; get; }

        /// <summary>
        /// Meldet oder setzt die Breite des Zielbereichs.
        /// </summary>
        Int32 DestinationWidth { set; get; }

        /// <summary>
        /// Meldet oder setzt die vertikale Position der oberen linken Ecke des Zielbereichs.
        /// </summary>
        Int32 DestinationTop { set; get; }

        /// <summary>
        /// Meldet oder setzt die Höhe des Zielbereichs.
        /// </summary>
        Int32 DestinationHeight { set; get; }

        /// <summary>
        /// Legt den Quellbereich fest.
        /// </summary>
        /// <param name="left">Die horizontale Position der linken oberen Ecke.</param>
        /// <param name="top">Die vertikale Position der linken oberen Ecke.</param>
        /// <param name="width">Die Breite.</param>
        /// <param name="height">Die Höhe.</param>
        void SetSourcePosition( Int32 left, Int32 top, Int32 width, Int32 height );

        /// <summary>
        /// Meldet den Quellbereich.
        /// </summary>
        /// <param name="left">Die horizontale Position der linken oberen Ecke.</param>
        /// <param name="top">Die vertikale Position der linken oberen Ecke.</param>
        /// <param name="width">Die Breite.</param>
        /// <param name="height">Die Höhe.</param>
        void GetSourcePosition( out Int32 left, out Int32 top, out Int32 width, out Int32 height );

        /// <summary>
        /// Legt den bevorzugten Bereich des Quellbilds fest.
        /// </summary>
        void SetDefaultSourcePosition();

        /// <summary>
        /// Legt den Zielbereich fest.
        /// </summary>
        /// <param name="left">Die horizontale Position der linken oberen Ecke.</param>
        /// <param name="top">Die vertikale Position der linken oberen Ecke.</param>
        /// <param name="width">Die Breite.</param>
        /// <param name="height">Die Höhe.</param>
        void SetDestinationPosition( Int32 left, Int32 top, Int32 width, Int32 height );

        /// <summary>
        /// Meldet den Zielbereich.
        /// </summary>
        /// <param name="left">Die horizontale Position der linken oberen Ecke.</param>
        /// <param name="top">Die vertikale Position der linken oberen Ecke.</param>
        /// <param name="width">Die Breite.</param>
        /// <param name="height">Die Höhe.</param>
        void GetDestinationPosition( out Int32 left, out Int32 top, out Int32 width, out Int32 height );

        /// <summary>
        /// Legt den bevorzugten Bereich des Zielbilds fest.
        /// </summary>
        void SetDefaultDestinationPosition();

        /// <summary>
        /// Meldet den Darstellungsbereich.
        /// </summary>
        /// <param name="width">Die Breite.</param>
        /// <param name="height">Die Höhe.</param>
        void GetVideoSize( out Int32 width, out Int32 height );

        /// <summary>
        /// Meldet die verwendete Farbpalette.
        /// </summary>
        /// <param name="startIndex">Laufende Nummer des ersten Farbeintrags.</param>
        /// <param name="entries">Feld mit allen Einträgen.</param>
        /// <param name="retrieved">Anzahl der ausgelesenen Einträge.</param>
        /// <param name="palette">Adresse der Farbpalette.</param>
        void GetVideoPaletteEntries( Int32 startIndex, Int32 entries, out Int32 retrieved, IntPtr palette );

        /// <summary>
        /// Ermittelt das aktuelle Bild.
        /// </summary>
        /// <param name="bufferSize">Größe des Speicherbereichs.</param>
        /// <param name="dibImage">Speicherbereich zur Aufnahme des Bildes.</param>
        void GetCurrentImage( ref Int32 bufferSize, IntPtr dibImage );

        /// <summary>
        /// Meldet, ob die bevorzugte Quelle verwendet wurd.
        /// </summary>
        /// <returns>Ergbebnis der Prüfung, negative Werte zeigen einen Fehler an.</returns>
        [PreserveSig]
        Int32 IsUsingDefaultSource();

        /// <summary>
        /// Meldet, ob der bevorzugte Zielbereich verwendet wird.
        /// </summary>
        /// <returns>Ergbebnis der Prüfung, negative Werte zeigen einen Fehler an.</returns>
        [PreserveSig]
        Int32 IsUsingDefaultDestination();
    }
}
