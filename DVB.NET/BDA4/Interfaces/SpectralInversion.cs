using System;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Die Spielarten der spektralen Inversion.
    /// </summary>
    public enum SpectralInversion
    {
        /// <summary>
        /// Not definied.
        /// </summary>
        NotDefined = 0,

        /// <summary>
        /// Automatic detection.
        /// </summary>
        Automatic = 1,

        /// <summary>
        /// Normal.
        /// </summary>
        Normal = 2,

        /// <summary>
        /// Inverted.
        /// </summary>
        Inverted = 3,

        /// <summary>
        /// Maximum value allowed for this enumeration.
        /// </summary>
        Maximum = 4,

        /// <summary>
        /// Mark parameter as not set.
        /// </summary>
        NotSet = -1
    }
}
