using System;
using System.Runtime.InteropServices;
using System.Security;


namespace JMS.TechnoTrend.MFCWrapper
{
    /// <summary>
    /// This class wraps all functionality to filter any data stream out of
    /// the DVB signal.
    /// </summary>
    /// <remarks>
    /// Concerning the underlying TT API/SDK this class will wrap both a filter using <see cref="Delegate"/>
    /// and a filter which directly writes to a file. The interface introduced here offers a common
    /// programming model which only differs by the initial call.
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
        /// Destroy a <i>CDVBTSFilter</i> instance.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "??1CDVBTSFilter@@UAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBTSFilter_Destruct( IntPtr pData );

        /// <summary>
        /// Initialize the filter condition for a <i>CDVBTSFilter</i> instance and start the filter.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?SetFilter@CDVBTSFilter@@QAE?AW4DVB_ERROR@@W4FILTERTYPE@1@GPAE1E@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBTSFilter_SetFilter( IntPtr pData, FilterType eType, UInt16 wPID, byte[] pbData, byte[] pbMask, byte bLength );

        /// <summary>
        /// Stop filtering on a <i>CDVBTSFilter</i> instance.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?ResetFilter@CDVBTSFilter@@QAE?AW4DVB_ERROR@@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBTSFilter_ResetFilter( IntPtr pData );

        /// <summary>
        /// Prototype <see cref="Delegate"/> for overloading the virtual function of the <i>CDVBTSFilter</i>
        /// base class.
        /// </summary>
        private delegate void InternalDataArrivalHandler( IntPtr pBuf, Int32 lBuf );

        /// <summary>
        /// Holds the C++ instance.
        /// </summary>
        private ClassHolder m_Class = null;

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

        /// <summary>
        /// Forward for converted input data.
        /// </summary>
        private Action<byte[]> m_DataHandler = null;

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
            FilterType = FilterType.None;
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
        /// Legt das Ziel des Datenempfangs fest.
        /// </summary>
        /// <param name="dataSink">Die Methode zur Bearbeitung aller Daten.</param>
        public void SetTarget( Action<byte[]> dataSink )
        {
            // Stop all current processings
            InternalStop( false );

            // Create helper
            m_Class = new ClassHolder( LegacySize.CDVBTSFilter );

            // Construct 
            CDVBTSFilter_Construct( m_Class.ClassPointer );

            // Attach destructor
            m_Class.Destructor = new ClassHolder.DestructHandler( CDVBTSFilter_Destruct );

            // Overload virtual function
            m_Class[1] = new InternalDataArrivalHandler( OnDataArrival );

            // Remember
            m_DataHandler = dataSink;
        } 

        /// <summary>
        /// Call the appropriate function to stop the filtering on <see cref="m_Class"/>.
        /// <seealso cref="InternalStop"/>
        /// </summary>
        /// <exception cref="DVBException">On any error.</exception>
        private void Suspend()
        {
            // Check operation
            DVBException.ThrowOnError( CDVBTSFilter_ResetFilter( m_Class.ClassPointer ), "Could not stop the Filter" );
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
                if (m_Frontend != null)
                {
                    // Remove
                    m_Frontend.RemoveFilter( FilterPID );

                    // Forget
                    m_Frontend = null;
                }

            // Discard registered handlers
            lock (this)
                m_DataHandler = null;

            // Nothing to do
            if (m_Class == null)
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
        /// filter instance the client has to call an appropriate overload again.
        /// </remarks>
        public void Stop()
        {
            // Forward
            InternalStop( true );
        }

        /// <summary>
        /// Nimmt Daten entgegen.
        /// </summary>
        /// <param name="buffer">Ein Speicherbereich mit Daten.</param>
        /// <param name="bytes">Die Größe des Bereichs.</param>
        private void OnDataArrival( IntPtr buffer, Int32 bytes )
        {
            // Synchronize
            lock (this)
            {
                // Already done
                if (m_Class == null)
                    return;

                // Load handler
                var std = m_DataHandler;
                if (std != null)
                {
                    // The data
                    var aData = new byte[bytes];

                    // Convert
                    Marshal.Copy( buffer, aData, 0, bytes );

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
            if ((aData == null) != (aMask == null))
                throw new ArgumentException( "Filter Data and Mask do not match" );
            if (aData != null)
            {
                // Validate
                if (aData.Length != aMask.Length)
                    throw new ArgumentException( "Filter Data and Mask are not of same Size" );
                if (aData.Length > 255)
                    throw new ArgumentException( "Filter Data and Mask are too large - a Maximum of 255 Bytes is allowed" );
            }

            // Length to send
            var bLenParam = (byte) ((aData == null) ? 0 : aData.Length);

            // Piping
            if (FilterType == FilterType.Piping)
            {
                // Validate
                if (aData != null)
                    throw new ArgumentException( "Piping does not allow Filter Data and Mask" );

                // Set buffer parameter
                bLenParam = (byte) (UseSmallBuffer ? PipeSize.Sixteen : ((UseExplicitBuffer != PipeSize.None) ? UseExplicitBuffer : PipeSize.ThirtyTwo));
            }

            // Try to start the current filter
            DVBException.ThrowOnError( CDVBTSFilter_SetFilter( m_Class.ClassPointer, FilterType, FilterPID, aData, aMask, bLenParam ), "Could not filter" );
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
            if (aData != null)
                if (aData.Length < 1)
                    aData = null;

            // New array
            byte[] aMask = null;

            // Fill
            if (aData != null)
            {
                // Allocate
                aMask = new byte[aData.Length];

                // Fill it
                for (int ix = aMask.Length; ix-- > 0; )
                    aMask[ix] = 255;
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
        /// Die Art des Filters.
        /// </summary>
        public FilterType FilterType { get; set; }
    }
}