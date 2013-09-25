using System;


namespace JMS.DVB.Provider.Duoflex
{
    /// <summary>
    /// Befehl an die CI Einheit der Hardware.
    /// </summary>
    internal enum CommonInterface
    {
        /// <summary>
        /// Entschlüsselung aktivieren.
        /// </summary>
        Decrpyt = 0,

        /// <summary>
        /// Menütext auslesen.
        /// </summary>
        MenuTitle = 1,
    }
}
