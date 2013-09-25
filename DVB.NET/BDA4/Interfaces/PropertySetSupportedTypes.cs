using System;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt, in welchem Umfang eine Eigenschaft unterstützt wird.
    /// </summary>
    [Flags]
    public enum PropertySetSupportedTypes
    {
        /// <summary>
        /// Die Eigenschaft kann ausgelesen werden.
        /// </summary>
        Get = 1,

        /// <summary>
        /// Die Eigenschaft kann verändert werden.
        /// </summary>
        Set = 2
    }
}
