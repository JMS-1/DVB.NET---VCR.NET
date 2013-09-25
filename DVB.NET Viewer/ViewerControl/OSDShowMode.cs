using System;


namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Beschreibt, was gerade im OSD angezeigt wird.
    /// </summary>
    public enum OSDShowMode
    {
        /// <summary>
        /// Das OSD ist nicht aktiv.
        /// </summary>
        Nothing,

        /// <summary>
        /// Das OSD zeigt den Videotext.
        /// </summary>
        Videotext,

        /// <summary>
        /// Der zugehöroge Eintrag der Programm
        /// </summary>
        ProgramGuide,

        /// <summary>
        /// Ein Kontextmenü mit möglichen Operationen.
        /// </summary>
        ContextMenu,

        /// <summary>
        /// Die Liste der verfügbaren Sender.
        /// </summary>
        SourceList,

        /// <summary>
        /// Die Liste der verfügbaren NVOD Kanäle.
        /// </summary>
        Services,

        /// <summary>
        /// Die Liste aller Tonspuren.
        /// </summary>
        AudioTracks,

        /// <summary>
        /// Der Lautstärkeregler.
        /// </summary>
        Volume,

        /// <summary>
        /// Eine Position in der Sendung oder in der Datei.
        /// </summary>
        Position,

        /// <summary>
        /// Es wird irgendetwas anderes angezeigt.
        /// </summary>
        Other,
    }

}
