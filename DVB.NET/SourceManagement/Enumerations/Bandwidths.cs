namespace JMS.DVB
{
    /// <summary>
    /// Bandbreiten für Kabel- und terrestrischen Empfang.
    /// </summary>
    public enum Bandwidths
    {
        /// <summary>
        /// 8 MHz.
        /// </summary>
        Eight = 0,

        /// <summary>
        /// 7 MHz.
        /// </summary>
        Seven = 1,

        /// <summary>
        /// 6 MHz.
        /// </summary>
        Six = 2,

        /// <summary>
        /// 5 MHz.
        /// </summary>
        Five = 3,

        /// <summary>
        /// Reserviert.
        /// </summary>
        Reserved100 = 4,

        /// <summary>
        /// Reserviert.
        /// </summary>
        Reserved101 = 5,

        /// <summary>
        /// Reserviert.
        /// </summary>
        Reserved110 = 6,

        /// <summary>
        /// Reserviert.
        /// </summary>
        Reserved111 = 7,

        /// <summary>
        /// Unbekannte oder noch nicht gesetzte Bandbreite.
        /// </summary>
        NotDefined = -1
    }
}
