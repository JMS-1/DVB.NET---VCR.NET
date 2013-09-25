using System;


namespace JMS.DVB.Provider.NovaS2
{
    /// <summary>
    /// Erweiterungseigenschaften für den Tuner.
    /// </summary>
    internal enum BDATunerExtensions
    {
        /// <summary>
        /// Digital Satellite Equipment Control.
        /// </summary>
        DiSEqC = 0,

        /// <summary>
        /// DVB-S2 Pilot.
        /// </summary>
        Pilot = 0x20,

        /// <summary>
        /// DVB-S2 Roll Off.
        /// </summary>
        RollOff = 0x21
    }
}
