

namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Die möglichen Werte für den Schutzbereich beim DVB-S2 Empfang.
    /// </summary>
    public enum DVBS2GuardInterval
    {
        /// <summary>
        /// Unbestimmt.
        /// </summary>
        NotDefined = 0,

        /// <summary>
        /// 1/32.
        /// </summary>
        Ratio32 = 1,

        /// <summary>
        /// 1/16.
        /// </summary>
        Ratio16 = 2,

        /// <summary>
        /// 1/8.
        /// </summary>
        Ratio8 = 3,

        /// <summary>
        /// 1/4.
        /// </summary>
        Ratio4 = 4,

        /// <summary>
        /// Der erste verbotene Wert.
        /// </summary>
        Maximum = 5,

        /// <summary>
        /// Nicht festgelegt.
        /// </summary>
        NotSet = -1,
    }
}
