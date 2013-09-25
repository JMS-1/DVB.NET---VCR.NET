using System;


namespace JMS.DVB.DirectShow
{
    /// <summary>
    /// Beschreibt eindeutige Kennung, die im Zusammenhang mit der Darstellung relevant sind.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Das Datenformat für die Bilddarstellung.
        /// </summary>
        public static readonly Guid KSDATAFORMAT_TYPE_VIDEO = new Guid( "73646976-0000-0010-8000-00aa00389b71" );

        /// <summary>
        /// Das Datenformat für den Ton.
        /// </summary>
        public static readonly Guid KSDATAFORMAT_TYPE_AUDIO = new Guid( "73647561-0000-0010-8000-00aa00389b71" );

        /// <summary>
        /// Das Bildformat für SDTV.
        /// </summary>
        public static readonly Guid KSDATAFORMAT_SUBTYPE_MPEG2_VIDEO = new Guid( "e06d8026-db46-11cf-b4d1-00805f6cbbea" );

        /// <summary>
        /// Das Bildformat für HDTV in der Cyberlink Variante.
        /// </summary>
        public static readonly Guid KSDATAFORMAT_SUBTYPE_H264_VIDEO_Cyberlink = new Guid( "8d2d71cb-243f-45e3-b2d8-5fd7967ec09b" );

        /// <summary>
        /// Das Bildformat für HDTV.
        /// </summary>
        public static readonly Guid KSDATAFORMAT_SUBTYPE_H264_VIDEO = new Guid( "34363248-0000-0010-8000-00AA00389B71" );

        /// <summary>
        /// Das Bildformat für HDTV un der AVC1 Variante.
        /// </summary>
        public static readonly Guid KSDATAFORMAT_SUBTYPE_AVC1_VIDEO = new Guid( "31435641-0000-0010-8000-00AA00389B71" );

        /// <summary>
        /// Das Tonformat für Stereodaten.
        /// </summary>
        public static readonly Guid KSDATAFORMAT_SUBTYPE_MPEG2_AUDIO = new Guid( "e06d802b-db46-11cf-b4d1-00805f6cbbea" );

        /// <summary>
        /// Das Tonformat für Dolby Digital (AC3).
        /// </summary>
        public static readonly Guid KSDATAFORMAT_SUBTYPE_AC3_AUDIO = new Guid( "e06d802c-db46-11cf-b4d1-00805f6cbbea" );

        /// <summary>
        /// Das Basisformat für MPEG2 Bilddaten.
        /// </summary>
        public static readonly Guid FORMAT_MPEG2_VIDEO = new Guid( "e06d80e3-db46-11cf-b4d1-00805f6cbbea" );

        /// <summary>
        /// Das Format für Bildinformation.
        /// </summary>
        public static readonly Guid FORMAT_VideoInfo = new Guid( "05589f80-c356-11ce-bf01-00aa0055595a" );

        /// <summary>
        /// Das Format für erweiterte Bildinformationen.
        /// </summary>
        public static readonly Guid FORMAT_VideoInfo2 = new Guid( "f72a76a0-eb0a-11d0-ace4-0000c0cc16ba" );

        /// <summary>
        /// Das Format für Toninformationen.
        /// </summary>
        public static readonly Guid FORMAT_WaveFormatEx = new Guid( "05589f81-c356-11ce-bf01-00aa0055595a" );

        /// <summary>
        /// Die eindeutige Kennung der Systemuhr.
        /// </summary>
        public static readonly Guid CLSID_SystemClock = new Guid( "e436ebb1-524f-11ce-9f53-0020af0ba770" );

        /// <summary>
        /// Die eindeutige Kennung der Speicherverwaltung.
        /// </summary>                             
        public static readonly Guid CLSID_MemoryAllocator = new Guid( "1e651cc0-b199-11d0-8212-00c04fc32c45" );

        /// <summary>
        /// Die eindeutige Kennung der Komponente zum Auslesen von Filterinformationen.
        /// </summary>
        public static readonly Guid CLSID_FilterMapper2 = new Guid( "cda42200-bd88-11d0-bd4e-00a0c911ce86" );
    }
}
