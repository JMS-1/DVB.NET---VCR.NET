using System;
using System.Security;
using System.Runtime.InteropServices;


namespace JMS.TechnoTrend.MFCWrapper
{
    /// <summary>
    /// Since the related TTAPI C++ SDK class <i>CDVBCommon</i> will never be instantiated all
    /// methods are static.
    /// </summary>
    public sealed class DVBCommon
    {
        /// <summary>
        /// Open a device.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?OpenDevice@CDVBCommon@@SAHGPBDH@Z", ExactSpelling = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl )]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool CDVBCommon_OpenDevice( UInt16 wDevIdx, string sAppName, bool bNoNetwork );

        /// <summary>
        /// Get the number of devices installed on this system.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?GetNumberOfDevices@CDVBCommon@@SAGXZ", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl )]
        [SuppressUnmanagedCodeSecurity]
        private static extern int CDVBCommon_GetNumberOfDevices();

        /// <summary>
        /// Close a device.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?CloseDevice@CDVBCommon@@SAXXZ", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBCommon_CloseDevice();

        /// <summary>
        /// Get the Device handle.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?GetActDeviceHandle@CDVBCommon@@SAPAXXZ", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl )]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr CDVBCommon_GetActDeviceHandle();

        /// <summary>
        /// Synchronize access to the open device. The current version does only support
        /// a single device to be open at any time in a single process.
        /// </summary>
        /// <remarks>
        /// [Maybe only restricted to a single <see cref="AppDomain"/>]
        /// </remarks>
        static object m_Sync = new object();

        /// <summary>
        /// See if a device is open. A client may use this information to do a proper
        /// shutdown by calling <see cref="CloseDevice"/>.
        /// </summary>
        static bool m_Open = false;

        /// <summary>
        /// Avoid construction of instances.
        /// </summary>
        private DVBCommon()
        {
        }

        /// <summary>
        /// Retrieve the current number of devices installed on the system.
        /// </summary>
        /// <returns>Number of devices.</returns>
        static public int GetNumberOfDevices()
        {
            // Report
            return CDVBCommon_GetNumberOfDevices();
        }

        /// <summary>
        /// Open the indicated device. Access is synchronized using <see cref="m_Sync"/>.
        /// </summary>
        /// <param name="lIndex">Zero-based index of the device.</param>
        /// <param name="sApp">Name of the current application - may be null.</param>
        /// <param name="bNoNet">[Not (yet) known]</param>
        /// <exception cref="DVBException">
        /// Thrown when the C++ method invocation reports some <see cref="DVBError"/> or 
        /// a device is already open.
        /// </exception>
        static public void OpenDevice( int lIndex, string sApp, bool bNoNet )
        {
            // Synchronize
            lock (m_Sync)
            {
                // Once
                if (m_Open) throw new DVBException( "Device already opened" );

                // Forward
                if (!CDVBCommon_OpenDevice( (ushort) lIndex, sApp, bNoNet )) throw new DVBException( "Could not open Device #" + lIndex.ToString() );

                // Once
                m_Open = true;
            }
        }

        /// <summary>
        /// Close the current device. Accesses is synchronized using <see cref="m_Sync"/>.
        /// </summary>
        /// <exception cref="DVBException">
        /// Thrown when no current open device exists.
        /// </exception>
        static public void CloseDevice()
        {
            // Synchronize
            lock (m_Sync)
            {
                // Once
                if (!m_Open) throw new DVBException( "Device not open" );

                // Once
                m_Open = false;

                // Forward
                CDVBCommon_CloseDevice();
            }
        }

        /// <summary>
        /// Report if there is a currently opened device.
        /// Access is synchronized using <see cref="m_Sync"/> and then
        /// <see cref="m_Open"/> is returned.
        /// </summary>
        static public bool IsOpen
        {
            get
            {
                // Report
                lock (m_Sync) return m_Open;
            }
        }

        /// <summary>
        /// Get the Win32 <i>HANDLE</i> of the currently opened device.
        /// </summary>
        static public IntPtr ActiveDeviceHandle
        {
            get
            {
                // Report
                return CDVBCommon_GetActDeviceHandle();
            }
        }
    }
}
