

namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Die <i>RollOff</i> Werte beim DVB-S2 Empfang.
    /// </summary>
    public enum RollOff
    {
        /// <summary>
        /// Unbekannt.
        /// </summary>
        NotDefined = 0,

        /// <summary>
        /// 20%.
        /// </summary>
        Twenty = 1,

        /// <summary>
        /// 25%.
        /// </summary>
        TwentyFive = 2,

        /// <summary>
        /// 35%.
        /// </summary>
        ThirtyFive = 3,

        /// <summary>
        /// Der erste nicht erlaubte Wert.
        /// </summary>
        Maximum = 4,

        /// <summary>
        /// Nicht festgelegt.
        /// </summary>
        NotSet = -1,
    }
}
