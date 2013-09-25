namespace JMS.DVB
{
    /// <summary>
    /// Die möglichen Arten von Untertiteln.
    /// </summary>
    public enum SubtitleTypes
    {
        /// <summary>
        /// Videotext.
        /// </summary>
        EBUTeletext = 0x01,

        /// <summary>
        /// Videotext (assoziiert).
        /// </summary>
        EBUTeletextAssociated = 0x02,

        /// <summary>
        /// VBI.
        /// </summary>
        VBIData = 0x03,

        /// <summary>
        /// DVB, kein Bildschirmverhältnis festgelegt.
        /// </summary>
        DVBNormal = 0x10,

        /// <summary>
        /// DVB, Bildschirmverhältnis 4:3.
        /// </summary>
        DVBRatio_4_3 = 0x11,

        /// <summary>
        /// DVB, Bildschirmverhältnis 16:9.
        /// </summary>
        DVBRatio_16_9 = 0x12,

        /// <summary>
        /// DVB, Bildschirmverhältnis 2,21:1.
        /// </summary>
        DVBRatio_221_100 = 0x13,

        /// <summary>
        /// DVB für Hörgeschädigte, kein Bildschirmverhältnis festgelegt.
        /// </summary>
        DVBImpairedNormal = 0x20,

        /// <summary>
        /// DVB für Hörgeschädigte, Bildschirmverhältnis 4:3.
        /// </summary>
        DVBImpairedRatio_4_3 = 0x21,

        /// <summary>
        /// DVB für Hörgeschädigte, Bildschirmverhältnis 16:9.
        /// </summary>
        DVBImpairedRatio_16_9 = 0x22,

        /// <summary>
        /// DVB für Hörgeschädigte, Bildschirmverhältnis 2,21:1.
        /// </summary>
        DVBImpairedRatio_221_100 = 0x23
    }
}
