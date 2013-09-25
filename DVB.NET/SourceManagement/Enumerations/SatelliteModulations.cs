namespace JMS.DVB
{
    /// <summary>
    /// Die verwendete Modulation beim Satellitenempfang.
    /// </summary>
    public enum SatelliteModulations
    {
        /// <summary>
        /// Muss automatisch ermittelt werden.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// QPSK.
        /// </summary>
        QPSK = 1,

        /// <summary>
        /// 8PSK.
        /// </summary>
        PSK8 = 2,

        /// <summary>
        /// 16-QAM (nur DVB-S).
        /// </summary>
        QAM16 = 3,

        /// <summary>
        /// Unbekannt.
        /// </summary>
        NotDefined = -1
    }
}
