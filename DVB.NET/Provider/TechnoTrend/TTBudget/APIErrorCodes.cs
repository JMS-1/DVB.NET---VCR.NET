using System;


namespace JMS.DVB.Provider.TTBudget
{
    /// <summary>
    /// Fehlercodes aus der propertiären BDA API.
    /// </summary>
    public enum APIErrorCodes
    {
        /// <summary>
        /// 
        /// </summary>
        Success,

        /// <summary>
        /// 
        /// </summary>
        NotImplemented,

        /// <summary>
        /// 
        /// </summary>
        NotSupported,

        /// <summary>
        /// 
        /// </summary>
        ErrorHandle,

        /// <summary>
        /// 
        /// </summary>
        NoDeviceHandle,

        /// <summary>
        /// 
        /// </summary>
        Failed,

        /// <summary>
        /// 
        /// </summary>
        IRAlreadyOpen,

        /// <summary>
        /// 
        /// </summary>
        IRNotOpened,

        /// <summary>
        ///
        /// </summary>
        TooManyBytes,

        /// <summary>
        /// 
        /// </summary>
        CIHardwareError,

        /// <summary>
        /// 
        /// </summary>
        CIAlreadyOpen,

        /// <summary>
        /// 
        /// </summary>
        TimeOut,

        /// <summary>
        /// 
        /// </summary>
        ReadPSIFailed,

        /// <summary>
        /// 
        /// </summary>
        NotSet,

        /// <summary>
        /// 
        /// </summary>
        Error,

        /// <summary>
        /// 
        /// </summary>
        BadPointer
    }
}
