using System;
using System.Runtime.InteropServices;
using System.Security;


namespace JMS.TechnoTrend.MFCWrapper
{
    /// <summary>
    /// Wraps the <i>CDVBAVControl</i> from the TTAPI C++ SDK. For details of the functionality
    /// see there.
    /// </summary>
    /// <remarks>
    /// The client is recommended to use the <see cref="Dispose"/> method for cleanup as
    /// soon as the C++ instance is not longer needed.
    /// </remarks>
    public class DVBAVControl : IDisposable
    {
        /// <summary>
        /// Set the video output mode.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?SetVideoOutputMode@CDVBAVControl@@QAE?AW4DVB_ERROR@@W4_VIDEOOUTPUTMODE@1@@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBAVControl_SetVideoOutputMode( IntPtr pData, VideoOutput eMode );

        /// <summary>
        /// Set the master PIDs.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?SetPIDs@CDVBAVControl@@QAE?AW4DVB_ERROR@@GGG@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBAVControl_SetPIDs( IntPtr pData, UInt16 uAudio, UInt16 uVideo, UInt16 uPCR );

        /// <summary>
        /// Read capabilities.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?GetCapabilities@CDVBAVControl@@QAEKXZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern AVCapabilities CDVBAVControl_GetCapabilities( IntPtr pData );

        /// <summary>
        /// Initialize the A/V control.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?Init@CDVBAVControl@@QAE?AW4DVB_ERROR@@H@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBAVControl_Init( IntPtr pData, bool bOctalVideoPort );

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "??0CDVBAVControl@@QAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBAVControl_Construct( IntPtr pData );

        /// <summary>
        /// Destruct the instance.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "??1CDVBAVControl@@UAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBAVControl_Destruct( IntPtr pData );

        /// <summary>
        /// Set the input source.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?SetADSwitch@CDVBAVControl@@QAE?AW4DVB_ERROR@@W4DEVICE_INPUT@1@@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBAVControl_SetADSwitch( IntPtr pData, DeviceInput input );

        /// <summary>
        /// Hold the C++ instance.
        /// </summary>
        private ClassHolder m_Class = null;

        /// <summary>
        /// Creates a new C++ instance using the standard <see cref="ClassHolder"/> constructor.
        /// </summary>
        public DVBAVControl()
        {
            // Create helper
            m_Class = new ClassHolder( LegacySize.CDVBAVControl );

            // Initialize
            CDVBAVControl_Construct( m_Class.ClassPointer );

            // Prepare destory
            m_Class.Destructor = new ClassHolder.DestructHandler( CDVBAVControl_Destruct );
        }

        /// <summary>
        /// Force proper shutdown.
        /// </summary>
        ~DVBAVControl()
        {
            // Detach
            Dispose();
        }

        /// <summary>
        /// On first call simply forwards to <see cref="ClassHolder.Dispose"/> on the
        /// <see cref="m_Class"/>.
        /// </summary>
        public void Dispose()
        {
            // Cleanup
            using (m_Class)
                m_Class = null;

            // No need to finalize - we are down
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Initialize the A/V control.
        /// </summary>
        /// <exception cref="DVBException">
        /// Thrown when the C++ method invocation reports some <see cref="DVBError"/>.
        /// </exception>
        public void Initialize()
        {
            // Execute
            DVBException.ThrowOnError( CDVBAVControl_Init( m_Class.ClassPointer, false ), "Can not initialize Audio/Video Control" );
        }

        /// <summary>
        /// Load the capabilities using a direct C++ member invocation.
        /// </summary>
        public AVCapabilities Capabilities { get { return CDVBAVControl_GetCapabilities( m_Class.ClassPointer ); } }

        /// <summary>
        /// Sets the current video output mode.
        /// </summary>
        /// <exception cref="DVBException">
        /// Thrown when the C++ method invocation reports some <see cref="DVBError"/>.
        /// </exception>
        public VideoOutput VideoOutput { set { DVBException.ThrowOnError( CDVBAVControl_SetVideoOutputMode( m_Class.ClassPointer, value ), "Can not set Video Output Mode to " + value.ToString() ); } }

        /// <summary>
        /// Set all the master PIDs at once. Future versions of this class may allow
        /// to update individual PIDs as in <see cref="AudioPID"/>.
        /// </summary>
        /// <param name="uAudio">Audio PID.</param>
        /// <param name="uVideo">Video PID.</param>
        /// <param name="uPCR">[Don't know what this is]</param>
        /// <exception cref="DVBException">
        /// Thrown when the C++ method invocation reports some <see cref="DVBError"/>.
        /// </exception>
        public void SetPIDs( ushort uAudio, ushort uVideo, ushort uPCR )
        {
            // Execute
            DVBException.ThrowOnError( CDVBAVControl_SetPIDs( m_Class.ClassPointer, uAudio, uVideo, uPCR ), "Unable to set all PIDs at once" );
        }

        /// <summary>
        /// Update the Analog/Digital input mode.
        /// </summary>
        public DeviceInput ADSwitch { set { DVBException.ThrowOnError( CDVBAVControl_SetADSwitch( m_Class.ClassPointer, value ), "Can not change A/D switch" ); } }
    }
}
