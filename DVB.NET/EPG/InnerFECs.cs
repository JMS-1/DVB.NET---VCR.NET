

namespace JMS.DVB.EPG
{
    /// <summary>
    /// Die Arten des inneren Fehlerkorrekturverfahrens.
    /// </summary>
    public enum InnerFECs
    {
        /// <summary>
        /// Keine Angabe.
        /// </summary>
        NotDefined,

        /// <summary>
        /// 1:2
        /// </summary>
        Conv1_2,

        /// <summary>
        /// 2:3
        /// </summary>
        Conv2_3,

        /// <summary>
        /// 3:4
        /// </summary>
        Conv3_4,

        /// <summary>
        /// 5:6
        /// </summary>
        Conv5_6,

        /// <summary>
        /// 7:8
        /// </summary>
        Conv7_8,

        /// <summary>
        /// 8:9
        /// </summary>
        Conv8_9,

        /// <summary>
        /// Noch nicht in Verwendung
        /// </summary>
        Reserved0111,

        /// <summary>
        /// Noch nicht in Verwendung
        /// </summary>
        Reserved1000,

        /// <summary>
        /// Noch nicht in Verwendung
        /// </summary>
        Reserved1001,

        /// <summary>
        /// Noch nicht in Verwendung
        /// </summary>
        Reserved1010,

        /// <summary>
        /// Noch nicht in Verwendung
        /// </summary>
        Reserved1011,

        /// <summary>
        /// Noch nicht in Verwendung
        /// </summary>
        Reserved1100,

        /// <summary>
        /// Noch nicht in Verwendung
        /// </summary>
        Reserved1101,

        /// <summary>
        /// Noch nicht in Verwendung
        /// </summary>
        Reserved1110,

        /// <summary>
        /// Keine Korrektur
        /// </summary>
        NoConv
    }
}
