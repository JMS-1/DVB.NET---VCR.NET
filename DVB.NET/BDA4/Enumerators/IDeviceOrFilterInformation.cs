using System;


namespace JMS.DVB.DeviceAccess.Enumerators
{
    /// <summary>
    /// Beschreibt ein Auswahlelement für ein Gerät.
    /// </summary>
    public interface IDeviceOrFilterInformation
    {
        /// <summary>
        /// Der Name, der dem Anwender zur Auswahl angeboten wird.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Der eindeutige Name zur Auswahl.
        /// </summary>
        string UniqueName { get; }
    }
}
