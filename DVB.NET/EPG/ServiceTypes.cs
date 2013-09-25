using System;
using System.Collections.Generic;
using System.Text;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// 
    /// </summary>
	public enum ServiceTypes
	{
        /// <summary>
        /// Reserved for future use.
        /// </summary>
        Reserved,

        /// <summary>
        /// Digital television service.
        /// </summary>
        DigitalTelevision,

        /// <summary>
        /// Digital radio service.
        /// </summary>
        DigitalRadio,

        /// <summary>
        /// Teletext service.
        /// </summary>
        Teletext,

        /// <summary>
        /// NVOD reference service.
        /// </summary>
        NVODReference,

        /// <summary>
        /// NVOD time-shift service.
        /// </summary>
        NVODTimeShift,

        /// <summary>
        /// Mosaic service.
        /// </summary>
        Mosaic,

        /// <summary>
        /// PAL coded signal.
        /// </summary>
        PALCoded,

        /// <summary>
        /// SECAM coded signal.
        /// </summary>
        SECAMCoded,

        /// <summary>
        /// D/D-2 MAC.
        /// </summary>
        DMAC,

        /// <summary>
        /// FM Radio.
        /// </summary>
        FMRadio,

        /// <summary>
        /// NTSC coded signal.
        /// </summary>
        NTSCCoded,

        /// <summary>
        /// Data broadcast service.
        /// </summary>
        DataBroadcast,

        /// <summary>
        /// Reserved for CI usage.
        /// </summary>
        CIReserved,

        /// <summary>
        /// RSC map.
        /// </summary>
        RSCMap,

        /// <summary>
        /// RSC FLS.
        /// </summary>
        RSCFLS,

        /// <summary>
        /// DVB MHP service.
        /// </summary>
        DVBMHP,

        /// <summary>
        /// Reserved for future use - lower bound.
        /// </summary>
        ReservedLow = 0x11,

        /// <summary>
        /// Advanced codec HD digital television service.
        /// </summary>
        SDTVDigitalTelevision = 0x16,

        /// <summary>
        /// Advanced codec HD digital television service.
        /// </summary>
        HDTVDigitalTelevision = 0x19,

        /// <summary>
        /// Reserved for future use - upper bound.
        /// </summary>
        ReservedHigh = 0x7f,

        /// <summary>
        /// User defined - lower bound.
        /// </summary>
        UserDefinedLow = 0x80,

        /// <summary>
        /// Used by BBC / ITV to indicate HDTV channels.
        /// </summary>
        SkyHDTV = 0x86,

        /// <summary>
        /// Used by SKY to indicated NVOD service channel
        /// </summary>
        SkyNVOD = 0xd3,

        /// <summary>
        /// User defined - upper bound.
        /// </summary>
        UserDefinedHigh = 0xfe,

        /// <summary>
        /// Reserver for future use.
        /// </summary>
        ReservedMaximum = 0xff
    }
}
