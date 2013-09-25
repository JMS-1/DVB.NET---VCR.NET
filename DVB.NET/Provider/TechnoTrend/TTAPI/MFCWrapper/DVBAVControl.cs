using System;
using System.Security;
using System.Runtime.InteropServices;


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
        /// Set the AC3 PID.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?SetAC3PID@CDVBAVControl@@QAE?AW4DVB_ERROR@@G@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBAVControl_SetAC3PID( IntPtr pData, UInt16 ac3PID );

        /// <summary>
        /// Set the audio PID.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?SetAudioPID@CDVBAVControl@@QAE?AW4DVB_ERROR@@G@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBAVControl_SetAudioPID( IntPtr pData, UInt16 uAudio );

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
        /// Load the current decoder state.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?GetVideoDecoderStatus@CDVBAVControl@@QAE?AW4DVB_ERROR@@AAU_VIDEOSTATE@1@@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBAVControl_GetVideoDecoderStatus( IntPtr pData, out DVBVideoState pState );

        /// <summary>
        /// Set the input source.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?SetADSwitch@CDVBAVControl@@QAE?AW4DVB_ERROR@@W4DEVICE_INPUT@1@@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBAVControl_SetADSwitch( IntPtr pData, DeviceInput input );

        /// <summary>
        /// Set the volume.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?SetVolume@CDVBAVControl@@QAE?AW4DVB_ERROR@@EE@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBAVControl_SetVolume( IntPtr pData, byte left, byte right );

        /// <summary>
        /// Hold the C++ instance.
        /// </summary>
        private ClassHolder m_Class = null;

        /// <summary>
        /// Internal information on the video state.
        /// </summary>
        /// <remarks>
        /// Currently most fields are unused. Some fields will be mapped to
        /// a <see cref="VideoState"/> instance.
        /// </remarks>
        [StructLayout( LayoutKind.Sequential )]
        private struct DVBVideoState
        {
            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 ProcessingState;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 CommandID;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 dummy1;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 dummy2;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 RateBuffFullness;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 dummy3;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 BytesDecoded;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 SkippedPictures;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 RepeatedPictures;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 MostRecentPTS;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 LastPicture;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 InitDone;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 FreezeIndex;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 FindIndex;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 DistanceI;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 ThresholdPTS;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 dummy4;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 dummy5;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 DisablePTSFilt;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 HSize;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 VSize;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 AspectRatio;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 FrameRate;

            /// <summary>
            /// Current bit rate.
            /// </summary>
            public UInt16 BitRate;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 VBVBuffSize;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 ConstParamFlag;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 IntraQ;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 NonIntraQ;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 FrameInterval;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 HeaderBackup;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 RedHeaderFlag;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 SeqExtension;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 GOPTimeCode1;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 GOPTimeCode2;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 ClosedGOP;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 PICHeadTempRef;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 SegHeaderExt;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 ColorDesc;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 ColorPrim;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 TransferChar;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 MatrixCoeff;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 DisplayHSize;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 DisplayVSize;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 GOPHeader;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 TimeCodeW1;

            /// <summary>
            /// Not used.
            /// </summary>
            public UInt16 TimeCodeW2;
        };


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
            if (null != m_Class) m_Class.Dispose();

            // Detach
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
        public AVCapabilities Capabilities
        {
            get
            {
                // Load it
                return CDVBAVControl_GetCapabilities( m_Class.ClassPointer );
            }
        }

        /// <summary>
        /// Sets the current video output mode.
        /// </summary>
        /// <exception cref="DVBException">
        /// Thrown when the C++ method invocation reports some <see cref="DVBError"/>.
        /// </exception>
        public VideoOutput VideoOutput
        {
            set
            {
                // Execute
                DVBException.ThrowOnError( CDVBAVControl_SetVideoOutputMode( m_Class.ClassPointer, value ), "Can not set Video Output Mode to " + value.ToString() );
            }
        }

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
        /// Update the audio PID only. This is normally used to choose another 
        /// language on an active program.
        /// </summary>
        public ushort AudioPID
        {
            set
            {
                // Execute
                DVBException.ThrowOnError( CDVBAVControl_SetAudioPID( m_Class.ClassPointer, value ), "Unable to change audio PID" );
            }
        }

        /// <summary>
        /// Update the Dolby Digital/AC3 audio PID only. 
        /// </summary>
        /// <remarks>
        /// Currently disabled.
        /// </remarks>
        public ushort AC3PID
        {
            set
            {
                // Check availabilty of AC3
                //if (AVCapabilities.HardWareAC3 != (AVCapabilities.HardWareAC3 & Capabilities)) return;

                // Disable AC3PID
                //if (0 == value) value = 0xffff;

                // Planed for a future version
                //DVBException.ThrowOnError(CDVBAVControl_SetAC3PID(m_Class.ClassPointer, value), "Unable to change Dolby Digital (AC3) PID");
            }
        }

        /// <summary>
        /// Load the current video decoder status.
        /// </summary>
        /// <remarks>
        /// Currently only very view fields from the full <see cref="VideoState"/> are
        /// reported to the client. The implementation is advisory and currently not
        /// very usefull.
        /// </remarks>
        /// <exception cref="DVBException">When it's not possible to retrieve the status
        /// from the hardware.</exception>
        public VideoState CurrentState
        {
            get
            {
                // Helper
                DVBVideoState rState;

                // Execute
                DVBException.ThrowOnError( CDVBAVControl_GetVideoDecoderStatus( m_Class.ClassPointer, out rState ), "Could not retrieve Video State" );

                // Report result
                return new VideoState( rState.BitRate );
            }
        }

        /// <summary>
        /// Change the volume
        /// </summary>
        public byte Volume
        {
            set
            {
                // Process
                DVBException.ThrowOnError( CDVBAVControl_SetVolume( m_Class.ClassPointer, value, value ), "Can not change volume" );
            }
        }

        /// <summary>
        /// Update the Analog/Digital input mode.
        /// </summary>
        public DeviceInput ADSwitch
        {
            set
            {
                // Process
                DVBException.ThrowOnError( CDVBAVControl_SetADSwitch( m_Class.ClassPointer, value ), "Can not change A/D switch" );
            }
        }
    }
}
