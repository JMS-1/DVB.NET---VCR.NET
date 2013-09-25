using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace JMS.DVB.DirectShow.RawDevices
{
    /// <summary>
    /// Meldet die Liste der unter Windows angebotenen <i>Raw Devices</i>, zu denen
    /// auch Fernbedienungen gehören.
    /// </summary>
    public class RawDeviceCollection : IDisposable, IEnumerable<RawDevice>
    {
        /// <summary>
        /// Beschreibt ein einzelnes Eingabegerät.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        internal struct _RawDevice
        {
            /// <summary>
            /// Die serialisierte Größe eines Strukturelementes.
            /// </summary>
            public static readonly int SizeOf = Marshal.SizeOf( typeof( _RawDevice ) );

            /// <summary>
            /// Die Windows Referenz zum Gerät.
            /// </summary>
            public IntPtr hDevice;

            /// <summary>
            /// Die Art des Gerätes.
            /// </summary>
            public RawDeviceType dwType;
        };

        /// <summary>
        /// Alle bekannten Geräte.
        /// </summary>
        private _RawDevice[] m_Devices;

        /// <summary>
        /// Erzeugt eine neue Liste.
        /// </summary>
        /// <param name="devices">Alle bekannten Geräte.</param>
        private RawDeviceCollection( _RawDevice[] devices )
        {
            // Remember
            m_Devices = devices;
        }

        /// <summary>
        /// Ermittelt die Liste der Eingabegeräte.
        /// </summary>
        /// <param name="rawInputDeviceList">Die zu befüllende Liste.</param>
        /// <param name="uiNumDevices">Die tatsächliche Anzahl in der Liste.</param>
        /// <param name="cbSize">Die Größe eines jeden Listenelementes.</param>
        /// <returns>Die benötigte Größe der Liste.</returns>
        [DllImport( "user32.dll" )]
        private static extern Int32 GetRawInputDeviceList( IntPtr rawInputDeviceList, ref Int32 uiNumDevices, Int32 cbSize );

        /// <summary>
        /// Erzeugt eine neue Liste.
        /// </summary>
        /// <returns>Die gewünschte Liste.</returns>
        public static RawDeviceCollection Create()
        {
            // Number of devices known to wienodws
            Int32 numDevices = 0;

            // Find out how may devices we have
            if (GetRawInputDeviceList( IntPtr.Zero, ref numDevices, _RawDevice.SizeOf ) == 0)
            {
                // Allocate list
                var devices = new _RawDevice[numDevices];

                // Lock in memory
                var locked = GCHandle.Alloc( devices, GCHandleType.Pinned );
                try
                {
                    // Read the list and process
                    if (GetRawInputDeviceList( locked.AddrOfPinnedObject(), ref numDevices, _RawDevice.SizeOf ) != -1)
                        return new RawDeviceCollection( devices );
                }
                finally
                {
                    // Release memory
                    locked.Free();
                }
            }

            // Create empty
            return new RawDeviceCollection( new _RawDevice[0] );
        }


        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region IEnumerable<RawDevice> Members

        /// <summary>
        /// Liefert eine Auflistung über alle Geräte.
        /// </summary>
        /// <returns>Die gewünschte Auflistung.</returns>
        public IEnumerator<RawDevice> GetEnumerator()
        {
            // Process
            return m_Devices.Select( d => new RawDevice( d ) ).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Liefert eine Auflistung über alle Geräte.
        /// </summary>
        /// <returns>Die gewünschte Auflistung.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            // Forward
            return GetEnumerator();
        }

        #endregion
    }
}
