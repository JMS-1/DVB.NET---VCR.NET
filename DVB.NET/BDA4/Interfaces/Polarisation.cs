using System;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Die unterschiedlichen Polarisationen, die ein Signal haben kann.
    /// </summary>
    public enum Polarisation
    {
        /// <summary>
        /// Not definied.
        /// </summary>
        NotDefined = 0,

        /// <summary>
        /// Horizontal.
        /// </summary>
        Horizontal = 1,

        /// <summary>
        /// Vertical.
        /// </summary>
        Vertical = 2,

        /// <summary>
        /// Circular left.
        /// </summary>
        Left = 3,

        /// <summary>
        /// Circular right.
        /// </summary>
        Right = 4,

        /// <summary>
        /// Maximum value allowed in this enumeration.
        /// </summary>
        Maximum = 5,

        /// <summary>
        /// Mark the parameter as not set.
        /// </summary>
        NotSet = -1
    }
}
