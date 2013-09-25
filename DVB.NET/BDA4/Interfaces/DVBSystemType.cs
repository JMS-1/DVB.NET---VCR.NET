using System;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Die verschiedenen Arten des DVB Empfangs.
    /// </summary>
    public enum DVBSystemType
    {
        /// <summary>
        /// Kabelanschluss.
        /// </summary>
        Cable = 0,

        /// <summary>
        /// Terrestrische Antenne.
        /// </summary>
        Terrestrial = 1,

        /// <summary>
        /// Satellitenempfang.
        /// </summary>
        Satellite = 2,

        /// <summary>
        /// Die Art des Empfangs ist nicht bekannt.
        /// </summary>
        Unknown = -1,
    }
}
