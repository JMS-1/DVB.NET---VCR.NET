using System;


namespace JMS.DVB.Provider.NovaS2
{
    /// <summary>
    /// Die unterstützten DiSEqC Versionen.
    /// </summary>
    internal enum DiSEqCVersions
    {
        /// <summary>
        /// Undefiniert.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// DiSEqC 1.X.
        /// </summary>
        Version1 = 1,

        /// <summary>
        /// DiSEqC 2.X.
        /// </summary>
        Version2 = 2,

        /// <summary>
        /// Echostar - nicht mehr unterstützt.
        /// </summary>
        Legacy = 3
    }
}
