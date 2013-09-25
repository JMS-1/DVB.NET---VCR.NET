namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt die Art des Bildsignals eines Senders.
    /// </summary>
    public enum VideoTypes
    {
        /// <summary>
        /// Es ist nicht bekannt, ob überhaupt ein Bildsignal angeboten wird.
        /// </summary>
        Unknown,

        /// <summary>
        /// Der Sender ist ein Radiosender und bietet gar kein Bildsignal an.
        /// </summary>
        NoVideo,

        /// <summary>
        /// Das Bild wird im MPEG-2 SDTV Format übermittelt.
        /// </summary>
        MPEG2,

        /// <summary>
        /// Das Bild wird im H.264 HDTV Format übermittelt.
        /// </summary>
        H264
    }
}
