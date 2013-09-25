using System;


namespace JMS.DVB.DeviceAccess.Topology
{
    /// <summary>
    /// Die Eigenschaften, die im Parametersatz des Tuners angeboten werden.
    /// </summary>
    public enum FrequencyFilterProperties
    {
        /// <summary>
        /// Die Frequenz.
        /// </summary>
        Frequency,

        /// <summary>
        /// Die Signalpolarität.
        /// </summary>
        Polarity,

        /// <summary>
        /// Bereichsauswahl, etwa für eine DiSEqC Ansteurung.
        /// </summary>
        Range,

        /// <summary>
        /// Der Transponder.
        /// </summary>
        Transponder,

        /// <summary>
        /// Die Bandbreite.
        /// </summary>
        Bandwidth,

        /// <summary>
        /// Der Faktor, der für Frequenzangaben zu verwenden ist.
        /// </summary>
        FrequencyMultiplier,

        /// <summary>
        /// Spezielle Funktionalitäten.
        /// </summary>
        Capabilities,

        /// <summary>
        /// Aktueller Zustand.
        /// </summary>
        ScanStatus,

        /// <summary>
        /// [Don't know]
        /// </summary>
        Standard,

        /// <summary>
        /// [Don't know]
        /// </summary>
        StandardMode
    }
}
