using System;
using System.Runtime.InteropServices;
using System.Security;
using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.Provider.TTBudget
{
    /// <summary>
    /// Basisklasse zur Implementierung von Zugriffen auf die properitären BDA Erweiterungen.
    /// </summary>
    public abstract class TechnoTrendBDAAPIAccessor
    {
        /// <summary>
        /// Startet die Benutzung der propertitären BDA Schnittstelle.
        /// </summary>
        /// <param name="deviceType">Art des Gerätes.</param>
        /// <param name="deviceIdentifier">Die Nummer des Gerätes.</param>
        /// <returns>Ein Schlüssel zum Zugriff auf ein Gerät.</returns>
        [DllImport( "ttBdaDrvApi_Dll.dll" )]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr bdaapiOpenHWIdx( Int32 deviceType, UInt32 deviceIdentifier );

        /// <summary>
        /// Beendet die Nutzung der properitären Schnittstelle.
        /// </summary>
        /// <param name="device">Der Schlüssel zum aktuellen Gerät.</param>
        [DllImport( "ttBdaDrvApi_Dll.dll" )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void bdaapiClose( IntPtr device );

        /// <summary>
        /// Der Schlüssel zum aktuellen Gerät.
        /// </summary>
        private IntPtr m_Device;

        /// <summary>
        /// Meldet das aktuelle Gerät.
        /// </summary>
        protected IntPtr Device { get { return m_Device; } }

        /// <summary>
        /// Stellt die Verbindung zur properitären Schnittstelle her.
        /// </summary>
        /// <param name="tuner">Der Filter zur Hardware.</param>
        protected virtual void Open( TypedComIdentity<IBaseFilter> tuner )
        {
            // Connect once
            if (m_Device != IntPtr.Zero)
                return;

            // Attach to the tuner output pin to get the hardware index
            using (var pinRef = tuner.GetSinglePin( PinDirection.Output ))
            using (var pin = pinRef.MarshalToManaged())
            {
                // Get the information
                var info = pin.Object.GetMediumArray();
                if (info.Length < 1)
                    throw new InvalidOperationException( "no information on attached Hardware" );

                // Try to connect
                m_Device = bdaapiOpenHWIdx( 1, info[0].Parameter1 );
                if (m_Device.ToInt64() == -1)
                    m_Device = IntPtr.Zero;
                if (m_Device == IntPtr.Zero)
                    throw new InvalidOperationException( string.Format( "could not connect to properitary API ({0} Media reported, primary Medium is {1} ({2}, {3}))", info.Length, info[0].MediumIdentifier, info[0].Parameter1, info[0].Parameter2 ) );
            }
        }

        /// <summary>
        /// Beende die Nutzung der Hardware.
        /// </summary>
        protected virtual void Close()
        {
            // Check mode
            if (m_Device != IntPtr.Zero)
                try
                {
                    // Terminate usage
                    bdaapiClose( m_Device );
                }
                finally
                {
                    // Forget
                    m_Device = IntPtr.Zero;
                }
        }
    }
}
