using System;
using System.Xml;


namespace JMS.DVB.Provider.Legacy
{
    /// <summary>
    /// Hilfsklasse zur Nutzung der Geräteprofile vor DVB.NET 3.9.
    /// </summary>
    public static class ProfileTools
    {
        /// <summary>
        /// Enthält die Beschreibung zu allen bekannten Geräten der alten DVB.NET Version.
        /// </summary>
        internal static readonly DeviceInformation[] LegacyDevices = DeviceInformation.Load();
    }
}
