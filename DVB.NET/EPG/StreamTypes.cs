using System;
using System.Collections.Generic;
using System.Text;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// The type of a transport stream.
    /// </summary>
	public enum StreamTypes
    {
        /// <summary>
        /// ITU-T | ISO/IEC Reserved
        /// </summary>
        Reserved,

        /// <summary>
        /// ISO/IEC 11172 Video
        /// </summary>
        Video11172,

        /// <summary>
        /// ITU-T Rec. H.262 | ISO/IEC 13818-2 Video or ISO/IEC 11172-2 constrained parameter video stream
        /// </summary>
        Video13818,

        /// <summary>
        /// ISO/IEC 11172 Audio
        /// </summary>
        Audio11172,

        /// <summary>
        /// ISO/IEC 13818-3 Audio
        /// </summary>
        Audio13818,

        /// <summary>
        /// ITU-T Rec. H.222.0 | ISO/IEC 13818-1 private_sections
        /// </summary>
        PrivateSections,

        /// <summary>
        /// ITU-T Rec. H.222.0 | ISO/IEC 13818-1 PES packets containing private data
        /// </summary>
        PrivateData,

        /// <summary>
        /// ISO/IEC 13522 MHEG
        /// </summary>
        MHEG,

        /// <summary>
        /// ITU-T Rec. H.222.0 | ISO/IEC 13818-1 Annex A DSM CC
        /// </summary>
        DSM,

        /// <summary>
        /// ITU-T Rec. H.222.1
        /// </summary>
        H222,

        /// <summary>
        /// ISO/IEC 13818-6 type A
        /// </summary>
        TypeA,

        /// <summary>
        /// ISO/IEC 13818-6 type B
        /// </summary>
        Carousel,

        /// <summary>
        /// ISO/IEC 13818-6 type C
        /// </summary>
        TypeC,

        /// <summary>
        /// ISO/IEC 13818-6 type D
        /// </summary>
        MultiProtocolDataStream,

        /// <summary>
        /// ISO/IEC 13818-1 auxiliary
        /// </summary>
        Auxiliary,

        /// <summary>
		/// Intermediate entry for AAC Audio.
		/// </summary>
		AAC = 0x11,

        /// <summary>
        /// Intermediate entry for H.264 video.
        /// </summary>
        H264 = 0x1b,

        /// <summary>
        /// ITU-T Rec. H.222.0 | ISO/IEC 13818-1 Reserved, lowest value
        /// </summary>
        ReservedLow = 0x0f,

        /// <summary>
        /// ITU-T Rec. H.222.0 | ISO/IEC 13818-1 Reserved, highest value
        /// </summary>
        ReservedHigh = 0x7f,

        /// <summary>
        /// User Private, lowest value
        /// </summary>
        UserPrivateLow = 0x80,

        /// <summary>
        /// User Private, highest value
        /// </summary>
        UserPrivateHigh = 0xff
    }
}
