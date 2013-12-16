using System;
using System.Runtime.InteropServices;
using System.Security;


namespace JMS.TechnoTrend.MFCWrapper
{
    /// <summary>
    /// Wraps the <i>CDVBComnIF</i> from the TTAPI C++ SDK. For details of the functionality
    /// see there.
    /// </summary>
    /// <remarks>
    /// The client is recommended to use the <see cref="Dispose"/> method for cleanup as
    /// soon as the C++ instance is not longer needed.
    /// </remarks>
    public class DVBCommonInterface : IDisposable
    {
        /// <summary>
        /// Construct the C++ instance.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "??0CDVBComnIF@@QAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBComnIF_Construct( IntPtr pData );

        /// <summary>
        /// Destruct the C++ instance.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "??1CDVBComnIF@@UAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBComnIF_Destruct( IntPtr pData );

        /// <summary>
        /// Start decrypting a program.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?ReadPSIFast@CDVBComnIF@@QAE?AW4DVB_ERROR@@G@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBComnIF_ReadPSIFast( IntPtr pData, UInt16 serviceID );

        /// <summary>
        /// Open the common interface,
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?Open@CDVBComnIF@@QAE?AW4DVB_ERROR@@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBComnIF_Open( IntPtr pData );

        /// <summary>
        /// Close thie common interface
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?Close@CDVBComnIF@@QAE?AW4DVB_ERROR@@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBComnIF_Close( IntPtr pData );

        /// <summary>
        /// Holder of the C++ instance.
        /// </summary>
        private ClassHolder m_Class = null;

        /// <summary>
        /// See if the CI is opened.
        /// </summary>
        private bool m_Open = false;

        /// <summary>
        /// Simply use the <see cref="ClassHolder"/> default constructor to create
        /// the C++ instance.
        /// </summary>
        public DVBCommonInterface()
        {
            // Create holder
            m_Class = new ClassHolder( LegacySize.CDVBComnIF );

            // Construct it
            CDVBComnIF_Construct( m_Class.ClassPointer );

            // Prepare for destruction
            m_Class.Destructor = new ClassHolder.DestructHandler( CDVBComnIF_Destruct );
        }

        /// <summary>
        /// Forward to <see cref="Dispose"/>.
        /// </summary>
        ~DVBCommonInterface()
        {
            // Detach
            Dispose();
        }

        /// <summary>
        /// Simply forward to <see cref="ClassHolder.Dispose"/> on <see cref="m_Class"/>
        /// when called for the first time.
        /// </summary>
        public void Dispose()
        {
            // Go down
            if (m_Open) Close();

            // Forward
            if (null != m_Class) m_Class.Dispose();

            // Once
            m_Class = null;

            // No need to finalize
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Check if the CI is idle.
        /// </summary>
        public bool IsIdle
        {
            get
            {
                // Report
                lock (this) return !m_Open;
            }
        }

        /// <summary>
        /// Open the CI device.
        /// </summary>
        /// <exception cref="DVBException">CI is already open or opening
        /// failed.</exception>
        public void Open()
        {
            // Synchronize
            lock (this)
            {
                // Test
                if (m_Open) throw new DVBException( "CI is already in use" );

                // Forward
                DVBException.ThrowOnError( CDVBComnIF_Open( m_Class.ClassPointer ), "CI could not be opened" );

                // Mark
                m_Open = true;
            }
        }

        /// <summary>
        /// Close the CI device.
        /// </summary>
        /// <exception cref="DVBException">CI is not open or shutting down
        /// failed.</exception>
        public void Close()
        {
            // Synchronize
            lock (this)
            {
                // Test
                if (!m_Open) throw new DVBException( "CI is already closed" );

                // Forward
                DVBException.ThrowOnError( CDVBComnIF_Close( m_Class.ClassPointer ), "CI could not be closed" );

                // Mark
                m_Open = false;
            }
        }

        /// <summary>
        /// Start decrypting a program.
        /// </summary>
        /// <param name="serviceIdentifier">The service identifier for the program.</param>
        public void Decrypt( ushort serviceIdentifier )
        {
            // Synchronize
            lock (this)
            {
                // Test
                if (!m_Open) throw new DVBException( "CI is closed" );

                // Forward
                DVBException.ThrowOnError( CDVBComnIF_ReadPSIFast( m_Class.ClassPointer, serviceIdentifier ), "unable to start decryption" );
            }
        }
    }
}
