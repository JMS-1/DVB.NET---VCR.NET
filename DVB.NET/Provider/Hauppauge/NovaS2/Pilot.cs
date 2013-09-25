using System;


namespace JMS.DVB.Provider.NovaS2
{
    /// <summary>
    /// Die einzelnen Pilot Varianten.
    /// </summary>
    internal enum Pilot
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
        /// Aktiv.
        /// </summary>
        Off = 1,

        /// <summary>
        /// Inaktiv.
        /// </summary>
        On = 2,

        /// <summary>
        /// Maximaler Wert dieser Auflistung.
        /// </summary>
        Maximum = 3
    }
}
