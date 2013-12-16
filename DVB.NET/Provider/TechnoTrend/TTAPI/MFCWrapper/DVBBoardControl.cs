using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;


namespace JMS.TechnoTrend.MFCWrapper
{
    /// <summary>
    /// Wraps the <i>CDVBBoardControl</i> from the TTAPI C++ SDK. For details of the functionality
    /// see there.
    /// </summary>
    /// <remarks>
    /// The client is recommended to use the <see cref="Dispose"/> method for cleanup as
    /// soon as the C++ instance is not longer needed.
    /// </remarks>
    public class DVBBoardControl : IDisposable
    {
        /// <summary>
        /// The version information structure packed for use in a C++ method invocation.
        /// Clients will only use the <see cref="BoardVersion"/>.
        /// </summary>
        [StructLayout( LayoutKind.Sequential )]
        private struct BEVersion
        {
            /// <summary>
            /// Firmware version.
            /// </summary>
            public UInt32 Firmware;

            /// <summary>
            /// Version of the Runtime Support Library.
            /// </summary>
            public UInt32 RTSLib;

            /// <summary>
            /// Video Microcode version.
            /// </summary>
            public UInt32 VideoDec;

            /// <summary>
            /// Compilation date.
            /// </summary>
            [MarshalAs( UnmanagedType.ByValArray, SizeConst = 16 )]
            public sbyte[] CompDate;

            /// <summary>
            /// Compilation time.
            /// </summary>
            [MarshalAs( UnmanagedType.ByValArray, SizeConst = 10 )]
            public sbyte[] CompTime;
        };

        /// <summary>
        /// Read the version.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?GetBEVersion@CDVBBoardControl@@QAE?AW4DVB_ERROR@@AAU_BE_VERSION@1@@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBBoardControl_GetBEVersion( IntPtr pData, out BEVersion rInfo );

        /// <summary>
        /// Manipulate the use of DMA.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?EnableDataDMA@CDVBBoardControl@@QAE?AW4DVB_ERROR@@H@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBBoardControl_EnableDataDMA( IntPtr pData, bool bEnable );

        /// <summary>
        /// Boot the card.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?BootARM@CDVBBoardControl@@QAE?AW4DVB_ERROR@@PAV?$CStringT@DV?$StrTraitMFC_DLL@DV?$ChTraitsCRT@D@ATL@@@@@ATL@@@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBBoardControl_BootARM( IntPtr pData, IntPtr pString );

        /// <summary>
        /// InitializeBoot the card.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?ResetInit@CDVBBoardControl@@QAE?AW4DVB_ERROR@@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBBoardControl_ResetInit( IntPtr pData );

        /// <summary>
        /// Construct a C++ instance.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "??0CDVBBoardControl@@QAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBBoardControl_Construct( IntPtr pData );

        /// <summary>
        /// Destruct a C++ instance.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "??1CDVBBoardControl@@UAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBBoardControl_Destruct( IntPtr pData );

        /// <summary>
        /// Hold the C++ instance.
        /// </summary>
        private ClassHolder m_Class = null;

        /// <summary>
        /// Creates the C++ instance using the default <see cref="ClassHolder"/> constructor.
        /// </summary>
        public DVBBoardControl()
        {
            // Create helper
            m_Class = new ClassHolder( LegacySize.CDVBBoardControl );

            // Construct 
            CDVBBoardControl_Construct( m_Class.ClassPointer );

            // Attach destructor
            m_Class.Destructor = new ClassHolder.DestructHandler( CDVBBoardControl_Destruct );
        }

        /// <summary>
        /// Simply forwards to <see cref="Dispose"/>.
        /// </summary>
        ~DVBBoardControl()
        {
            // Detach
            Dispose();
        }

        /// <summary>
        /// On first call the call is forwarded to <see cref="ClassHolder.Dispose"/> of
        /// <see cref="m_Class"/>. 
        /// </summary>
        /// <remarks>
        /// Prior to the destruction of the C++ instance <see cref="DataDMA"/>
        /// is used to disable DMA access. Any <see cref="Exception"/> from this
        /// call is ignored.
        /// </remarks>
        public void Dispose()
        {
            // Shutdown class
            if (null != m_Class)
            {
                // Reset DMA
                try { DataDMA = false; }
                catch (Exception) { }

                // Done
                m_Class.Dispose();
            }

            // Once
            m_Class = null;

            // No need to finalize
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Load the board ARM firmware from the indicated path. 
        /// </summary>
        /// <param name="firmwarePath">If the path does not end with a backslash
        /// a backslash is automatically added.</param>
        /// <exception cref="DVBException">
        /// Thrown when the C++ method invocation reports some <see cref="DVBError"/>.
        /// </exception>
        public void BootARM( string firmwarePath )
        {
            // Expand
            if (!firmwarePath.EndsWith( @"\" )) firmwarePath += @"\";

            // Create string
            using (MFCString pathString = new MFCString( firmwarePath ))
                for (int retry = 2; ; )
                {
                    // Try reset first
                    DVBException.ThrowOnError( CDVBBoardControl_ResetInit( m_Class.ClassPointer ), "Initialisation failed" );

                    try
                    {
                        // Process
                        DVBException.ThrowOnError( CDVBBoardControl_BootARM( m_Class.ClassPointer, pathString.ClassPointer ), "Unable to Boot " + firmwarePath );

                        // Done
                        break;
                    }
                    catch (DVBException e)
                    {
                        // Not a hardware error
                        if (DVBError.Hardware != e.ErrorCode) throw e;

                        // Retry through
                        if (--retry < 1) throw e;

                        // Delay before retry
                        System.Threading.Thread.Sleep( 1000 );
                    }
                }
        }

        /// <summary>
        /// Load the version information. Must not be called before <see cref="BootARM(string)"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="BEVersion"/> structure received from the C++ method invocation
        /// is converted to a <see cref="BoardVersion"/> instance.
        /// </remarks>
        /// <exception cref="DVBException">
        /// Thrown when the C++ method invocation reports some <see cref="DVBError"/>.
        /// </exception>
        public BoardVersion Version
        {
            get
            {
                // Helper
                BEVersion sVersion;

                // Process
                DVBException.ThrowOnError( CDVBBoardControl_GetBEVersion( m_Class.ClassPointer, out sVersion ), "Unable to get Version" );

                // Finish
                return new BoardVersion( sVersion.Firmware, sVersion.RTSLib, sVersion.VideoDec, InterOpTools.ByteArrayToString( sVersion.CompDate ), InterOpTools.ByteArrayToString( sVersion.CompTime ) );
            }
        }

        /// <summary>
        /// Calls <see cref="BootARM(string)"/> with the default software.
        /// </summary>
        /// <remarks>
        /// A default pack of boot files - from the BOOT/24 variante of the TTAPI C++ SDK 2.19b - is included
        /// in this <see cref="Assembly"/>. This method will copy all files to a temporary <see cref="Directory"/>
        /// and then forward the all. The temporary directory will be deleted after the call even if
        /// it fails.
        /// </remarks>
        public void BootARM()
        {
            // Direcory holder
            DirectoryInfo directory = null;

            // Must cleanup
            try
            {
                // Forward
                BootARM( out directory );
            }
            finally
            {
                // Destroy
                if (null != directory) directory.Delete( true );
            }
        }

        /// <summary>
        /// Calls <see cref="BootARM(string)"/> with the default software.
        /// </summary>
        /// <remarks>
        /// A default pack of boot files - from the BOOT/24 variante of the TTAPI C++ SDK 2.19b - is included
        /// in this <see cref="Assembly"/>. This method will copy all files to a temporary <see cref="Directory"/>
        /// and then forward the all. The temporary directory will be deleted after the call even if
        /// it fails.
        /// </remarks>
        /// <param name="directory">Temporary directory created.</param>
        public void BootARM( out DirectoryInfo directory )
        {
            // See if boot path is provided
            string bootPath = ConfigurationManager.AppSettings["ARMBootPath"];

            // Check mode
            bool staticBoot = ((null != bootPath) && (bootPath.Length > 0));

            // Create name
            if (staticBoot)
            {
                // Attach to file
                FileInfo exe = new FileInfo( Application.ExecutablePath );

                // Merge
                bootPath = Path.Combine( exe.DirectoryName, bootPath );

                // No temporary directory
                directory = null;
            }
            else
            {
                // Create temporary directory
                directory = Directory.CreateDirectory( Path.GetTempPath() + Guid.NewGuid().ToString() );

                // Remember
                bootPath = directory.FullName;

                // Attach to us
                Assembly pThis = Assembly.GetExecutingAssembly();

                // Our namespace
                string sNameSpace = "JMS.TechnoTrend.Boot.";

                // Buffer
                byte[] pBuf = new byte[100000];

                // Attach to resource names
                foreach (string sName in pThis.GetManifestResourceNames())
                {
                    // Must be prefix
                    if (!sName.StartsWith( sNameSpace )) continue;

                    // Open input and output
                    using (Stream pInput = pThis.GetManifestResourceStream( sName ))
                    using (FileStream pOutput = new FileStream( directory.FullName + "\\" + sName.Substring( sNameSpace.Length ), FileMode.CreateNew, FileAccess.Write, FileShare.None ))
                        for (int nLen = 0; (nLen = pInput.Read( pBuf, 0, pBuf.Length )) > 0; )
                            pOutput.Write( pBuf, 0, nLen );
                }
            }

            // Boot from it
            BootARM( bootPath );
        }

        /// <summary>
        /// Set the DMA usage.
        /// </summary>
        /// <exception cref="DVBException">
        /// Thrown when the C++ method invocation reports some <see cref="DVBError"/>.
        /// </exception>
        public bool DataDMA
        {
            set
            {
                // Process
                DVBException.ThrowOnError( CDVBBoardControl_EnableDataDMA( m_Class.ClassPointer, value ), "Could not set DataDMA to " + value.ToString() );
            }
        }
    }
}
