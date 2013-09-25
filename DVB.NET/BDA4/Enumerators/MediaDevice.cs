using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Enumerators
{
    /// <summary>
    /// Helper class to start and stop multimedia devices. 
    /// </summary>
    public class MediaDevice : Device
    {
        /// <summary>
        /// The unique identifier of the multi-media device class.
        /// </summary>
        public const string DeviceClass = "{4D36E96C-E325-11CE-BFC1-08002BE10318}";

        /// <summary>
        /// Erstellt den Zugriff auf ein Multimediagerät nach dessen Namen.
        /// </summary>
        /// <param name="adaptorName">Der Anzeigename des Gerätes.</param>
        public MediaDevice( string adaptorName )
            : this( adaptorName, false )
        {
        }

        /// <summary>
        /// Erstellt den Zugriff auf ein Multimediagerät.
        /// </summary>
        /// <param name="instanceOrAdaptorName">Der Anzeige- oder Instanzenname.</param>
        /// <param name="nameIsInstance">Gesetzt, wenn es sich um den Instanzennamen handelt.</param>
        public MediaDevice( string instanceOrAdaptorName, bool nameIsInstance )
            : this( instanceOrAdaptorName, nameIsInstance ? uint.MaxValue : 0 )
        {
        }

        /// <summary>
        /// Create a new multimedia device management instance.
        /// </summary>
        /// <param name="sAdaptorName">The name of the device to manage.</param>
        /// <param name="filterIndex">The property index used for comparison.</param>
        private MediaDevice( string sAdaptorName, uint filterIndex )
            : base( sAdaptorName, DeviceClass, filterIndex )
        {
        }

        /// <summary>
        /// Report all multi-media devices.
        /// </summary>
        public static string[] DeviceNames
        {
            get
            {
                // Ask base class
                return GetDevices( DeviceClass );
            }
        }

        /// <summary>
        /// Report all multi-media devices.
        /// </summary>
        public static DeviceInformation[] DeviceInformations
        {
            get
            {
                // Ask base class
                return GetDeviceInformations( DeviceClass ).ToArray();
            }
        }

        /// <summary>
        /// Deaktiviert und Reaktiviert ein Gerät.
        /// </summary>
        /// <param name="adaptorName">Der Anzeigename des Gerätes.</param>
        public static void WakeUp( string adaptorName )
        {
            // Create
            var device = new MediaDevice( adaptorName );

            // Process
            device.WakeUp();
        }

        /// <summary>
        /// Deaktiviert und Reaktiviert ein Gerät.
        /// </summary>
        /// <param name="instanceName">Der Anzeigename des Gerätes.</param>
        public static void WakeUpInstance( string instanceName )
        {
            // Create
            var device = new MediaDevice( instanceName, true );

            // Process
            device.WakeUp();
        }
    }
}
