

namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Die Signalübertragungsarten bei der DVB-S2 Übertragung.
    /// </summary>
    public enum DVBS2TransmissionMode
    {
        /// <summary>
        /// Unbekannt.
        /// </summary>
        NotDefined = 0,

        /// <summary>
        /// 2K FFT (1705 Trägersignal).
        /// </summary>
        Two = 1,

        /// <summary>
        /// 8K FFT (6817 Trägersignal).
        /// </summary>
        Eight = 2,

        /// <summary>
        /// 4K Modus (ETSI EN 300 744, Annex F).
        /// </summary>
        Four = 3,

        /// <summary>
        /// 2K.
        /// </summary>
        TwoInterleaved = 4,

        /// <summary>
        /// 4K.
        /// </summary>
        FourInterleaved = 5,

        /// <summary>
        /// Der erste verbotene Wert.
        /// </summary>
        Maximum = 6,

        /// <summary>
        /// Nicht festgelegt.
        /// </summary>
        NotSet = -1,
    }
}
