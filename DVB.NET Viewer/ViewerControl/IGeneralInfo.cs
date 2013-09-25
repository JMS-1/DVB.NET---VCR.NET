using System;

using JMS.DVB.DirectShow;


namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Zugriff auf allgemeine Einstellungen.
    /// </summary>
    public interface IGeneralInfo
    {
        /// <summary>
        /// Maximale Anzeigezeit einer OSD Einblendung in Sekunden.
        /// </summary>
        int OSDLifeTime { get; }

        /// <summary>
        /// Lautstärke in Prozent von 0.0 bis 1.0.
        /// </summary>
        double Volume { get; set; }

        /// <summary>
        /// Vollbildmodus verlassen.
        /// </summary>
        void LeaveFullScreen();

        /// <summary>
        /// Gesetzt, wenn der spezielle Formatcode für Cyberlink / PowerDVD verwendet werden soll.
        /// </summary>
        bool UseCyberlinkCodec { get; }

        /// <summary>
        /// Der zu verwendende H.264 Decoder.
        /// </summary>
        string H264Decoder { get; }

        /// <summary>
        /// Der zu verwendende MPEG-2 Decoder.
        /// </summary>
        string MPEG2Decoder { get; }

        /// <summary>
        /// Der zu verwendende Dolby Digital Decoder.
        /// </summary>
        string AC3Decoder { get; }

        /// <summary>
        /// Der zu verwendende Audio Decoder.
        /// </summary>
        string MP2Decoder { get; }

        /// <summary>
        /// Überträgt die Einstellungen für die Bildparameter.
        /// </summary>
        /// <param name="parameters"></param>
        void SetPictureParameters( PictureParameters parameters );

        /// <summary>
        /// Legt die Fensterüberschrift fest.
        /// </summary>
        /// <param name="title">Die neue Überschrift des Huaptfensters.</param>
        void SetWindowTitle( string title );

        /// <summary>
        /// Zeigt die Maus an.
        /// </summary>
        /// <returns>Verbirgt die Maus wieder, wenn die Operation abgeschlossen ist.</returns>
        IDisposable ShowCursor();

        /// <summary>
        /// Gesetzt, wenn die Fernsteuerung verwendet werden soll.
        /// </summary>
        bool UseRemoteControl { get; }
    }
}
