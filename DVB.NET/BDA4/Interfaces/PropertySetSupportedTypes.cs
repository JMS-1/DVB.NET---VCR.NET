using System;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt, in welchem Umfang eine Eigenschaft unterst�tzt wird.
    /// </summary>
    [Flags]
    public enum PropertySetSupportedTypes
    {
        /// <summary>
        /// Die Eigenschaft kann ausgelesen werden.
        /// </summary>
        Get = 1,

        /// <summary>
        /// Die Eigenschaft kann ver�ndert werden.
        /// </summary>
        Set = 2
    }
}
