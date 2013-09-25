

namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt die Nutzung des DVB-S2 Pilottons.
    /// </summary>
    public enum PilotMode
    {
        /// <summary>
        /// Unbekannt.
        /// </summary>
        NotDefined = 0,

        /// <summary>
        /// Signalton aus.
        /// </summary>
        Off = 1,

        /// <summary>
        /// Signalton an.
        /// </summary>
        On = 2,

        /// <summary>
        /// Der erste nicht erlaubte Wert.
        /// </summary>
        Maximum = 3,

        /// <summary>
        /// Nicht festgelegt.
        /// </summary>
        NotSet = -1,
    }
}
