using System;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// The various descriptor types as defined in the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
    /// <seealso cref="Descriptor"/>
    /// </summary>
    /// <remarks>
    /// <i>0x80</i> to <i>0xfe</i> are user defined and <i>0xff</i>
    /// is forbidden.
    /// </remarks>
    public enum DescriptorTags
    {
        /// <summary>
        /// 0x00
        /// </summary>
        Reserved1,

        /// <summary>
        /// 0x01
        /// </summary>
        Reserved2,

        /// <summary>
        /// 0x02
        /// </summary>
        VideoStream,

        /// <summary>
        /// 0x03
        /// </summary>
        AudioStream,

        /// <summary>
        /// 0x04
        /// </summary>
        Hierarchy,

        /// <summary>
        /// 0x05
        /// </summary>
        Registration,

        /// <summary>
        /// 0x06
        /// </summary>
        DataStreamAlignment,

        /// <summary>
        /// 0x07
        /// </summary>
        TargetBackgroundGrid,

        /// <summary>
        /// 0x08
        /// </summary>
        VideoWindow,

        /// <summary>
        /// 0x09
        /// </summary>
        CA,

        /// <summary>
        /// 0x0a
        /// </summary>
        ISO639Language,

        /// <summary>
        /// 0x0b
        /// </summary>
        SystemClock,

        /// <summary>
        /// 0x0c
        /// </summary>
        MultiplexBufferUtilization,

        /// <summary>
        /// 0x0d
        /// </summary>
        Copyright,

        /// <summary>
        /// 0x0e
        /// </summary>
        MaximumBitrate,

        /// <summary>
        /// 0x0f
        /// </summary>
        PrivateDataIndicator,

        /// <summary>
        /// 0x10
        /// </summary>
        SmoothingBuffer,

        /// <summary>
        /// 0x11
        /// </summary>
        STD,

        /// <summary>
        /// 0x12
        /// </summary>
        IBP,

        /// <summary>
        /// 0x13
        /// </summary>
        CarouselIdentifier,

        /// <summary>
        /// 0x14
        /// </summary>
        ISOReservedLow = 0x14,

        /// <summary>
        /// 0x3f
        /// </summary>
        ISOReservedHigh = 0x3f,

        /// <summary>
        /// 0x40
        /// </summary>
        NetworkName,

        /// <summary>
        /// 0x41
        /// </summary>
        SeviceList,

        /// <summary>
        /// 0x42
        /// </summary>
        Stuffing,

        /// <summary>
        /// 0x43
        /// </summary>
        SatelliteDeliverySystem,

        /// <summary>
        /// 0x44
        /// </summary>
        CableDeliverySystem,

        /// <summary>
        /// 0x45
        /// </summary>
        VBIData,

        /// <summary>
        /// 0x46
        /// </summary>
        VBITeletext,

        /// <summary>
        /// 0x47
        /// </summary>
        BouquetName,

        /// <summary>
        /// 0x48
        /// </summary>
        Service,

        /// <summary>
        /// 0x49
        /// </summary>
        CountryAvailability,

        /// <summary>
        /// 0x4a
        /// </summary>
        Linkage,

        /// <summary>
        /// 0x4b
        /// </summary>
        NVODReference,

        /// <summary>
        /// 0x4c
        /// </summary>
        TimeShiftedService,

        /// <summary>
        /// 0x4d
        /// </summary>
        ShortEvent,

        /// <summary>
        /// 0x4e
        /// </summary>
        ExtendedEvent,

        /// <summary>
        /// 0x4f
        /// </summary>
        TimeShiftedEvent,

        /// <summary>
        /// 0x50
        /// </summary>
        Component,

        /// <summary>
        /// 0x51
        /// </summary>
        Mosaic,

        /// <summary>
        /// 0x52
        /// </summary>
        StreamIdentifier,

        /// <summary>
        /// 0x53
        /// </summary>
        CAIdentifier,

        /// <summary>
        /// 0x54
        /// </summary>
        Content,

        /// <summary>
        /// 0x55
        /// </summary>
        ParentalRating,

        /// <summary>
        /// 0x56
        /// </summary>
        Teletext,

        /// <summary>
        /// 0x57
        /// </summary>
        Telephone,

        /// <summary>
        /// 0x58
        /// </summary>
        LocalTimeOffset,

        /// <summary>
        /// 0x59
        /// </summary>
        Subtitling,

        /// <summary>
        /// 0x5a
        /// </summary>
        TerrestrialDeliverySystem,

        /// <summary>
        /// 0x5b
        /// </summary>
        MultilingualNetworkName,

        /// <summary>
        /// 0x5c
        /// </summary>
        MultilingualBouquetName,

        /// <summary>
        /// 0x5d
        /// </summary>
        MultilingualServiceName,

        /// <summary>
        /// 0x5e
        /// </summary>
        MultilingualComponent,

        /// <summary>
        /// 0x5f
        /// </summary>
        PrivateDataSpecifier,

        /// <summary>
        /// 0x60
        /// </summary>
        ServiceMove,

        /// <summary>
        /// 0x61
        /// </summary>
        ShortSmoothingBuffer,

        /// <summary>
        /// 0x62
        /// </summary>
        FrequencyList,

        /// <summary>
        /// 0x63
        /// </summary>
        PartialTransportStream,

        /// <summary>
        /// 0x64
        /// </summary>
        DataBroadcast,

        /// <summary>
        /// 0x65
        /// </summary>
        CASystem,

        /// <summary>
        /// 0x66
        /// </summary>
        DataBroadcastId,

        /// <summary>
        /// 0x67
        /// </summary>
        TransportStream,

        /// <summary>
        /// 0x68
        /// </summary>
        DSNG,

        /// <summary>
        /// 0x69
        /// </summary>
        PDC,

        /// <summary>
        /// 0x6a
        /// </summary>
        AC3,

        /// <summary>
        /// 0x6b
        /// </summary>
        AncillaryData,

        /// <summary>
        /// 0x6c
        /// </summary>
        CellList,

        /// <summary>
        /// 0x6d
        /// </summary>
        CellFrequencyLink,

        /// <summary>
        /// 0x6e
        /// </summary>
        AnnouncementSupport,

        /// <summary>
        /// 0x6f
        /// </summary>
        ApplicationSignalling,

        /// <summary>
        /// 0x70
        /// </summary>
        AdaptationFieldData,

        /// <summary>
        /// 0x71
        /// </summary>
        ServiceIdentifier,

        /// <summary>
        /// 0x72
        /// </summary>
        ServiceAvailability,

        /// <summary>
        /// 0x73
        /// </summary>
        DefaultAuthority,

        /// <summary>
        /// 0x74
        /// </summary>
        RelatedContent,

        /// <summary>
        /// 0x75
        /// </summary>
        TVAId,

        /// <summary>
        /// 0x76
        /// </summary>
        ContentIdentifier,

        /// <summary>
        /// 0x77
        /// </summary>
        TimeSliceFecIdentifier,

        /// <summary>
        /// 0x78
        /// </summary>
        ECMRepetitionRate,

        /// <summary>
		/// 0x7c
		/// </summary>
		AAC = 0x7c,

        /// <summary>
        /// Enthält die Ausstrahlungsdaten einer Sendung des deutschen PayTV
        /// Anbieters PREMIERE.
        /// </summary>
        ContentTransmissionPremiere = 0xf2
    }
}
