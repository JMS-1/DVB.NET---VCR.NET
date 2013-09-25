using System;


namespace JMS.DVB.DeviceAccess.Enumerators
{
    /// <summary>
    /// Beschreibt ein einzelnes Gerät.
    /// </summary>
    public class DeviceInformation : IDeviceOrFilterInformation
    {
        /// <summary>
        /// Der vollständige Pfad zum Gerät.
        /// </summary>
        public string DevicePath { get; private set; }

        /// <summary>
        /// Der Anzeigename des Gerätes.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="displayName">Der Anzeigename des Gerätes.</param>
        /// <param name="devicePath">Der Pfad zum Gerät.</param>
        /// <exception cref="ArgumentNullException">Ein Parameter wurde nicht angegeben.</exception>
        internal DeviceInformation( string displayName, string devicePath )
        {
            // Validate
            if (string.IsNullOrEmpty( displayName ))
                throw new ArgumentNullException( "displayName" );
            if (string.IsNullOrEmpty( devicePath ))
                throw new ArgumentNullException( "devicePath" );

            // Remember
            DisplayName = displayName;
            DevicePath = devicePath;
        }

        /// <summary>
        /// Erzeugt einen Anzeigetext zu Testzwecken.
        /// </summary>
        /// <returns>Der gewünschte Anzeigetext.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "{0} ({1})", DisplayName, DevicePath );
        }

        #region IDeviceOrFilterInformation Members

        /// <summary>
        /// Der Name, der dem Anwender zur Auswahl angeboten wird.
        /// </summary>
        string IDeviceOrFilterInformation.DisplayName
        {
            get
            {
                // Report
                return DisplayName;
            }
        }

        /// <summary>
        /// Der eindeutige Name zur Auswahl.
        /// </summary>
        string IDeviceOrFilterInformation.UniqueName
        {
            get
            {
                // Report
                return DevicePath;
            }
        }

        #endregion
    }
}
