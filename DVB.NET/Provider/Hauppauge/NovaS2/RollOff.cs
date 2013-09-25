using System;


namespace JMS.DVB.Provider.NovaS2
{
    /// <summary>
    /// Die einzelnen Roll Off Faktoren.
    /// </summary>
    internal enum RollOff
    {
        /// <summary>
        /// Nicht gesetzt.
        /// </summary>
        NotSet = -1,

        /// <summary>
        /// Nicht definiert.
        /// </summary>
        NotDefined = 0,

        /// <summary>
        /// Alpha = 0.20.
        /// </summary>
        Offset20 = 1,

        /// <summary>
        /// Alpha = 0.25.
        /// </summary>
        Offset25 = 2,

        /// <summary>
        /// Alpha = 0.35.
        /// </summary>
        Offset35 = 3,

        /// <summary>
        /// Maximaler Wert dieser Auflistung.
        /// </summary>
        Maximum = 4
    }
}
