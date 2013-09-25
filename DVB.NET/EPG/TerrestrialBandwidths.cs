using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// Bandbreitenangaben für den terrestrischen Empfang.
    /// </summary>
    public enum TerrestrialBandwidths
    {
        /// <summary>
        /// 8 MHz.
        /// </summary>
        Eight,

        /// <summary>
        /// 7 MHz.
        /// </summary>
        Seven,

        /// <summary>
        /// 6 MHz.
        /// </summary>
        Six,

        /// <summary>
        /// 5 MHz.
        /// </summary>
        Five,

        /// <summary>
        /// Noch nicht festgelegt.
        /// </summary>
        Reserved100,

        /// <summary>
        /// Noch nicht festgelegt.
        /// </summary>
        Reserved101,
        
        /// <summary>
        /// Noch nicht festgelegt.
        /// </summary>
        Reserved110,

        /// <summary>
        /// Noch nicht festgelegt.
        /// </summary>
        Reserved111,
    }
}
