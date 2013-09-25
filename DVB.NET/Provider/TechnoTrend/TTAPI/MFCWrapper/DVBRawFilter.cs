using System;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;

using JMS.DVB;


namespace JMS.TechnoTrend.MFCWrapper
{
    /// <summary>
    /// This class wraps all functionality to filter any data stream out of
    /// the DVB signal.
    /// </summary>
    /// <remarks>
    /// Concerning the underlying TT API/SDK this class will wrap both a filter using <see cref="Delegate"/>
    /// and a filter which directly writes to a file. The interface introduced here offers a common
    /// programming model which only differs by the initial call to <see cref="SetTarget(FilterHandler)"/>.
    /// </remarks>
    public unsafe class DVBRawFilter : IDisposable
    {
        /// <summary>
        /// Create a new <i>CDVBTSFilter</i> using the default buffer size.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "??0CDVBTSFilter@@QAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBTSFilter_Construct( IntPtr pData );

        /// <summary>
        /// Create a new <i>CDVBTSFilter</i> using a buffer of the indicated size.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "??0CDVBTSFilter@@QAE@K@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBTSFilter_Construct( IntPtr pData, UInt32 BufferSize );

        /// <summary>
        /// Destroy a <i>CDVBTSFilter</i> instance.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "??1CDVBTSFilter@@UAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBTSFilter_Destruct( IntPtr pData );

        /// <summary>
        /// Initialize the filter condition for a <i>CDVBTSFilter</i> instance and start the filter.
        /// <seealso cref="CDVBFilterToFile_StartFilter"/>
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?SetFilter@CDVBTSFilter@@QAE?AW4DVB_ERROR@@W4FILTERTYPE@1@GPAE1E@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBTSFilter_SetFilter( IntPtr pData, FilterType eType, UInt16 wPID, byte[] pbData, byte[] pbMask, byte bLength );

        [DllImport( "ttdvbacc.dll", EntryPoint = "?GetFileLen@CDVBFilterToFile@@QAEKXZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern UInt32 CDVBFilterToFile_GetFileLen( IntPtr pData );

        /// <summary>
        /// Stop filtering on a <i>CDVBTSFilter</i> instance.
        /// <see cref="CDVBFilterToFile_StopFilter"/>
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?ResetFilter@CDVBTSFilter@@QAE?AW4DVB_ERROR@@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBTSFilter_ResetFilter( IntPtr pData );

        /// <summary>
        /// Create a new <i>CDVBFilterToFile</i> instance.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "??0CDVBFilterToFile@@QAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBFilterToFile_Construct( IntPtr pData );

        /// <summary>
        /// Destroy a <i>CDVBFilterToFile</i> instance.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "??1CDVBFilterToFile@@UAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBFilterToFile_Destruct( IntPtr pData );

        /// <summary>
        /// Configure the file to use in a <i>CDVBFilterToFile</i> instance.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?SetFileParams@CDVBFilterToFile@@QAEXABV?$CStringT@DV?$StrTraitMFC_DLL@DV?$ChTraitsCRT@D@ATL@@@@@ATL@@I@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBFilterToFile_SetFileParams( IntPtr pData, IntPtr Name, UInt32 nOpenFlags );

        /// <summary>
        /// Start filtering in a <i>CDVBFilterToFile</i> instance.
        /// <seealso cref="CDVBTSFilter_SetFilter"/>
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?StartFilter@CDVBFilterToFile@@QAE?AW4DVB_ERROR@@W4FILTERTYPE@CDVBTSFilter@@GPAE1E@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBFilterToFile_StartFilter( IntPtr pData, FilterType eType, UInt16 wPID, byte[] pbData, byte[] pbMask, byte bLength );

        /// <summary>
        /// Stop filtering in a <i>CDVBFilterToFile</i> instance.
        /// <seealso cref="CDVBTSFilter_ResetFilter"/>
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?StopFilter@CDVBFilterToFile@@QAE?AW4DVB_ERROR@@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBFilterToFile_StopFilter( IntPtr pData );

        /// <summary>
        /// Prototype <see cref="Delegate"/> for overloading the virtual function of the <i>CDVBTSFilter</i>
        /// base class.
        /// </summary>
        private delegate void InternalDataArrivalHandler( IntPtr pBuf, Int32 lBuf );

        /// <summary>
        /// Handler prototype to receive incoming data.
        /// </summary>
        /// <remarks>
        /// In contrast to the <see cref="FilterHandler"/> this handler will receive the incoming
        /// data in the format of the TT API/SDK which may avoid extra copy operations and 
        /// therefore be faster.
        /// </remarks>
        public delegate void RawDataHandler( byte* pData, uint lBytes );

        /// <summary>
        /// Holds the C++ instance.
        /// </summary>
        private ClassHolder m_Class = null;

        /// <summary>
        /// See whether <see cref="m_Class"/> is a <i>CDVBTSFilter</i> or a <i>CDVBFilterToFile</i>.
        /// </summary>
        private bool m_FilterToFile = false;

        /// <summary>
        /// If this field is set the <see cref="UseExplicitBuffer"/> will be ignored and
        /// the buffer size will become <see cref="PipeSize">Sixteen</see>.
        /// </summary>
        public bool UseSmallBuffer = false;

        /// <summary>
        /// When <see cref="UseSmallBuffer"/> is unset this field can be used to
        /// choose any buffer size for a piping filter. If the value is <see cref="PipeSize">None</see>
        /// the maximum <see cref="PipeSize">ThirtyTwo</see> will be used.
        /// </summary>
        public PipeSize UseExplicitBuffer = PipeSize.None;

        private DVBFrontend m_Frontend = null;
        private long m_LastFileSize = 0;
        private long m_AddFileSize = 0;

        /// <summary>
        /// Forward for raw input data.
        /// </summary>
        private RawDataHandler m_RawHandler = null;

        /// <summary>
        /// Forward for converted input data.
        /// </summary>
        private FilterHandler m_DataHandler = null;

        /// <summary>
        /// The type of the filter.
        /// </summary>
        private FilterType m_Type = FilterType.None;

        /// <summary>
        /// The PID to filter upon.
        /// </summary>
        public readonly ushort FilterPID = 0;

        /// <summary>
        /// Create a new filter instance attached to the indicated PID.
        /// </summary>
        internal DVBRawFilter( ushort uPID, DVBFrontend frontend )
        {
            // Remember
            m_Frontend = frontend;
            FilterPID = uPID;
        }

        /// <summary>
        /// Simply forwards to <see cref="Dispose"/> - just in case our
        /// client violates the <see cref="IDisposable"/> rules.
        /// </summary>
        ~DVBRawFilter()
        {
            // Forward
            Dispose();
        }

        /// <summary>
        /// Stop the underlying TT API/SDK filter and release all allocated resources.
        /// </summary>
        public void Dispose()
        {
            // Be safe
            try
            {
                // Clean up
                InternalStop( true );
            }
            catch (Exception e)
            {
                // Avoid any error while cleaning up
                System.Diagnostics.Debug.WriteLine( e );
            }

            // Remove from garbage collection
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Forwards to <see cref="CreateFilter"/> with <i>false</i> as a first parameter.
        /// </summary>
        /// <remarks>
        /// Always calls <see cref="Stop"/>.
        /// </remarks>
        /// <param name="pHandler">Receive <see cref="Array"/> of <see cref="byte"/>.</param>
        public void SetTarget( FilterHandler pHandler )
        {
            // Start it
            CreateFilter( false, 0 );

            // Remember
            m_DataHandler = pHandler;
        }

        /// <summary>
        /// Forwards to <see cref="CreateFilter"/> with <i>true</i> and the buffer
        /// size as parameters.
        /// </summary>
        /// <param name="pHandler">Receive <see cref="Array"/> of <see cref="byte"/>.</param>
        /// <param name="uBuffer">Size of buffer in bytes.</param>
        public void SetTarget( FilterHandler pHandler, uint uBuffer )
        {
            // Start it
            CreateFilter( true, uBuffer );

            // Remember
            m_DataHandler = pHandler;
        }

        /// <summary>
        /// Forwards to <see cref="CreateFilter"/> with <i>false</i> as a first parameter.
        /// </summary>
        /// <remarks>
        /// Always calls <see cref="Stop"/>.
        /// </remarks>
        /// <param name="pHandler">Receive raw <see cref="byte"/> data as coming
        /// from the TT API/SDK layer. In contrast to <see cref="SetTarget(FilterHandler)"/>
        /// using this target mode may help to reduce one level of extra copy.</param>
        public void SetTarget( RawDataHandler pHandler )
        {
            // Start it
            CreateFilter( false, 0 );

            // Remember
            m_RawHandler = pHandler;
        }

        /// <summary>
        /// Create a raw data filter.
        /// </summary>
        /// <param name="bUseSize">If set use the indicated buffer size.</param>
        /// <param name="uBuffer">Size of buffer in bytes.</param>
        private void CreateFilter( bool bUseSize, uint uBuffer )
        {
            // Stop all current processings
            InternalStop( false );

            // Set mode
            m_FilterToFile = false;

            // Create helper
            m_Class = new ClassHolder( LegacySize.CDVBTSFilter );

            // Construct 
            if (bUseSize)
            {
                // Set buffer
                CDVBTSFilter_Construct( m_Class.ClassPointer, uBuffer );
            }
            else
            {
                // Default buffer
                CDVBTSFilter_Construct( m_Class.ClassPointer );
            }

            // Attach destructor
            m_Class.Destructor = new ClassHolder.DestructHandler( CDVBTSFilter_Destruct );

            // Overload virtual function
            m_Class[1] = new InternalDataArrivalHandler( OnDataArrival );
        }

        /// <summary>
        /// Forwards to <see cref="CreateFilter"/> with <i>true</i> and the buffer
        /// size as parameters.
        /// </summary>
        /// <param name="pHandler">Receive raw <see cref="byte"/> data as coming
        /// from the TT API/SDK layer. In contrast to <see cref="SetTarget(FilterHandler)"/>
        /// using this target mode may help to reduce one level of extra copy.</param>
        /// <param name="uBuffer">Size of buffer in bytes.</param>
        public void SetTarget( RawDataHandler pHandler, uint uBuffer )
        {
            // Forward
            CreateFilter( true, uBuffer );

            // Remember
            m_RawHandler = pHandler;
        }

        /// <summary>
        /// When the filter is active incoming data will be written to the indicated
        /// file.
        /// </summary>
        /// <remarks>
        /// Always calls <see cref="Stop"/>.
        /// </remarks>
        /// <param name="sFileName">Full path to a file. The file will be deleted and
        /// the caller has to take any care to avoid overwriting existing files.</param>
        public void SetTarget( string sFileName )
        {
            // Find encoding
            Encoding ansi = Encoding.GetEncoding( 1252 );

            // Get as bytes
            byte[] ansiBytes = ansi.GetBytes( sFileName );

            // Get as string
            sFileName = ansi.GetString( ansiBytes ).Replace( '?', '_' );

            // Stop all current processings
            InternalStop( false );

            // Set mode
            m_FilterToFile = true;
            m_RawHandler = null;
            m_DataHandler = null;
            m_LastFileSize = 0;
            m_AddFileSize = 0;

            // Create helper
            m_Class = new ClassHolder( LegacySize.CDVBFilterToFile );

            // Construct 
            CDVBFilterToFile_Construct( m_Class.ClassPointer );

            // Attach destructor
            m_Class.Destructor = new ClassHolder.DestructHandler( CDVBFilterToFile_Destruct );

            // Create MFC string
            using (MFCString pFileName = new MFCString( sFileName ))
            {
                // Configure file - flags are Create and Write
                CDVBFilterToFile_SetFileParams( m_Class.ClassPointer, pFileName.ClassPointer, 0x1001 );
            }
        }

        /// <summary>
        /// Call the appropriate function to stop the filtering on <see cref="m_Class"/>.
        /// <seealso cref="InternalStop"/>
        /// </summary>
        /// <exception cref="DVBException">On any error.</exception>
        private void Suspend()
        {
            // Check operation
            if (m_FilterToFile)
            {
                // Stop 
                DVBException.ThrowOnError( CDVBFilterToFile_StopFilter( m_Class.ClassPointer ), "Could not stop the File Filter" );
            }
            else
            {
                // Stop
                DVBException.ThrowOnError( CDVBTSFilter_ResetFilter( m_Class.ClassPointer ), "Could not stop the Filter" );
            }
        }

        /// <summary>
        /// Stop the current filtering and destroy the C++ instance in <see cref="m_Class"/>.
        /// <see cref="Suspend"/>
        /// </summary>
        /// <param name="remove">Set to remove the filter from the collection.</param>
        private void InternalStop( bool remove )
        {
            // Remove from frontend
            if (remove)
                if (null != m_Frontend)
                {
                    // Remove
                    m_Frontend.RemoveFilter( FilterPID );

                    // Forget
                    m_Frontend = null;
                }

            // Discard registered handlers
            lock (this)
                m_DataHandler = null;
            lock (this)
                m_RawHandler = null;

            // Nothing to do
            if (null == m_Class)
                return;

            // With cleanup
            try
            {
                // Terminate
                Suspend();
            }
            finally
            {
                // Remove C++ instance
                m_Class.Dispose();

                // Detach from object
                m_Class = null;
            }
        }

        /// <summary>
        /// Stop the current filter.
        /// <seealso cref="InternalStop"/>
        /// </summary>
        /// <remarks>
        /// When calling this method <see cref="m_Class"/> is released. To reuse the
        /// filter instance the client has to call an appropriate overload of
        /// <see cref="SetTarget(FilterHandler)"/> again.
        /// </remarks>
        public void Stop()
        {
            // Forward
            InternalStop( true );
        }

        /// <summary>
        /// Called whenever there is some data coming from the filter.
        /// </summary>
        /// <remarks>
        /// Immediatly terminates if <see cref="m_Class"/> is not set or
        /// <see cref="m_FilterToFile"/> is active. Else the call is forwarded
        /// to <see cref="m_RawHandler"/> and/or <see cref="m_DataHandler"/>.
        /// </remarks>
        /// <param name="pBuf">Buffer of bytes.</param>
        /// <param name="lBuf">Size of the buffer.</param>
        private void OnDataArrival( IntPtr pBuf, Int32 lBuf )
        {
            // Synchronize
            lock (this)
            {
                // Already done
                if ((null == m_Class) || m_FilterToFile) return;

                // Load handlers
                RawDataHandler raw = m_RawHandler;
                FilterHandler std = m_DataHandler;

                // Call raw handler
                if (null != raw) raw( (byte*) (pBuf.ToPointer()), (uint) lBuf );

                // Call high level handler
                if (null != std)
                {
                    // The data
                    byte[] aData = new byte[lBuf];

                    // Convert
                    Marshal.Copy( pBuf, aData, 0, lBuf );

                    // Forward
                    std( aData );
                }
            }
        }

        /// <summary>
        /// Try to start the current filter.
        /// </summary>
        /// <param name="aData">The filter data bytes.</param>
        /// <param name="aMask">The filter mask - each set bit indicates a valid bit
        /// inside the filter data.</param>
        /// <exception cref="ArgumentException">If the arguments are incompatible.</exception>
        /// <exception cref="DVBException">On any error from the underlying API.</exception>
        public void Start( byte[] aData, byte[] aMask )
        {
            // Check size
            if ((null == aData) != (null == aMask)) throw new ArgumentException( "Filter Data and Mask do not match" );
            if (null != aData)
            {
                // Validate
                if (aData.Length != aMask.Length) throw new ArgumentException( "Filter Data and Mask are not of same Size" );
                if (aData.Length > 255) throw new ArgumentException( "Filter Data and Mask are too large - a Maximum of 255 Bytes is allowed" );
            }

            // Length to send
            byte bLenParam = (byte) ((null == aData) ? 0 : aData.Length);

            // Piping
            if (FilterType.Piping == m_Type)
            {
                // Validate
                if (null != aData) throw new ArgumentException( "Piping does not allow Filter Data and Mask" );

                // Set buffer parameter
                bLenParam = (byte) (UseSmallBuffer ? PipeSize.Sixteen : ((PipeSize.None != UseExplicitBuffer) ? UseExplicitBuffer : PipeSize.ThirtyTwo));
            }

            // Try to start the current filter
            if (m_FilterToFile)
            {
                // Start file filter
                DVBException.ThrowOnError( CDVBFilterToFile_StartFilter( m_Class.ClassPointer, m_Type, FilterPID, aData, aMask, bLenParam ), "Could not filter to File" );
            }
            else
            {
                // Start in memory filter
                DVBException.ThrowOnError( CDVBTSFilter_SetFilter( m_Class.ClassPointer, m_Type, FilterPID, aData, aMask, bLenParam ), "Could not filter" );
            }
        }

        /// <summary>
        /// Total number of bytes recorded by this filter so far.
        /// </summary>
        public long Length
        {
            get
            {
                // Must be a file
                if (!m_FilterToFile) throw new InvalidOperationException( "not a file filter" );

                // Load current
                long lSize = CDVBFilterToFile_GetFileLen( m_Class.ClassPointer );

                // Test for 32-bit truncation
                if (lSize < m_LastFileSize) m_AddFileSize += 0x100000000;

                // Remember
                m_LastFileSize = lSize;

                // Forward
                return (m_AddFileSize + lSize);
            }
        }

        /// <summary>
        /// Forward to <see cref="Start(byte[], byte[])"/> creating a second
        /// <see cref="Array"/> with the same size as the parameter but all
        /// elements set to <i>255</i>.
        /// </summary>
        /// <param name="aData">The filter data.</param>
        public void Start( byte[] aData )
        {
            // Short cut
            if ((null != aData) && (aData.Length < 1)) aData = null;

            // New array
            byte[] aMask = null;

            // Fill
            if (null != aData)
            {
                // Allocate
                aMask = new byte[aData.Length];

                // Fill it
                for (int ix = aMask.Length; ix-- > 0; aMask[ix] = 255) ;
            }

            // Forward
            Start( aData, aMask );
        }

        /// <summary>
        /// Forward to <see cref="Start(byte[], byte[])"/> with two <i>null</i> parameters.
        /// </summary>
        public void Start()
        {
            // Forward
            Start( null, null );
        }

        /// <summary>
        /// Access <see cref="m_Type"/>. Changes will take effect on the next call
        /// to <see cref="Start(byte[], byte[])"/>.
        /// </summary>
        public FilterType FilterType
        {
            get
            {
                // Report
                return m_Type;
            }
            set
            {
                // Change
                m_Type = value;
            }
        }
    }
}