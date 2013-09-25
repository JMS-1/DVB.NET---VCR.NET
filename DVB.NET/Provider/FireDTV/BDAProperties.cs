using System;


namespace JMS.DVB.Provider.FireDTV
{
    /// <summary>
    /// Eigenschaften des BDA Treibers, die programmatisch beeinflusst werden können.
    /// </summary>
    internal enum BDAProperties
    {
        /// <summary>
        /// 
        /// </summary>
        SelectMultiplexSatellite,

        /// <summary>
        /// 
        /// </summary>
        SelectServiceSatellite,

        /// <summary>
        /// 
        /// </summary>
        SelectPIDSSatellite,

        /// <summary>
        /// 
        /// </summary>
        GetSignalStrength,

        /// <summary>
        /// 
        /// </summary>
        GetDriverVersion,

        /// <summary>
        /// 
        /// </summary>
        SelectMultiplexTerrestrial,

        /// <summary>
        /// 
        /// </summary>
        SelectPIDSTerrestrial,

        /// <summary>
        /// 
        /// </summary>
        SelectMultiplexCable,

        /// <summary>
        /// 
        /// </summary>
        SelectPIDSCable,

        /// <summary>
        /// 
        /// </summary>
        GetFrontEndStatus,

        /// <summary>
        /// 
        /// </summary>
        GetSystemInfp,

        /// <summary>
        /// 
        /// </summary>
        GetFirmwareVersion,

        /// <summary>
        /// 
        /// </summary>
        LNBControl,

        /// <summary>
        /// 
        /// </summary>
        GetLNB,

        /// <summary>
        /// 
        /// </summary>
        SetLNB,

        /// <summary>
        /// 
        /// </summary>
        SetPowerStatus,

        /// <summary>
        /// 
        /// </summary>
        SetAutoTuneStatus,

        /// <summary>
        /// 
        /// </summary>
        UpdateFirmware,

        /// <summary>
        /// 
        /// </summary>
        GetFirmwareUpdateStatus,

        /// <summary>
        /// CI initialisieren.
        /// </summary>
        ResetCI,

        /// <summary>
        /// 
        /// </summary>
        WriteTPDU,

        /// <summary>
        /// 
        /// </summary>
        ReadTPDU,

        /// <summary>
        /// Einen Befehl an das CI senden.
        /// </summary>
        SendCA,

        /// <summary>
        /// Daten vom CI entgegen nehmen.
        /// </summary>
        ReceiveCA,

        /// <summary>
        /// 
        /// </summary>
        GetBoardTemp,

        /// <summary>
        /// 
        /// </summary>
        TuneQPSK,

        /// <summary>
        /// 
        /// </summary>
        RemoteControlRegister,

        /// <summary>
        /// 
        /// </summary>
        RemoteControlCancel,

        /// <summary>
        /// Zustand des CI abfragen.
        /// </summary>
        GetCIStatus,

        /// <summary>
        /// Prüfen, ob der richtige Treiber vorliegt.
        /// </summary>
        TestInterface
    }
}
