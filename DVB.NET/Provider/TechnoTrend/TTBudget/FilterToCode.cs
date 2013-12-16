using System;
using System.Runtime.InteropServices;
using System.Security;
using JMS.TechnoTrend;


namespace JMS.DVB.Provider.TTBudget
{
    internal class FilterToCode : Filter
    {
        [DllImport( "ttlcdacc.dll", EntryPoint = "??0CDVBTSFilter@@QAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void _Construct( IntPtr classPointer );

        [DllImport( "ttlcdacc.dll", EntryPoint = "??0CDVBTSFilter@@QAE@K@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void _Construct( IntPtr classPointer, UInt32 bufferSize );

        [DllImport( "ttlcdacc.dll", EntryPoint = "??1CDVBTSFilter@@UAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void _Destruct( IntPtr classPointer );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?SetFilter@CDVBTSFilter@@QAE?AW4DVB_ERROR@@W4FILTERTYPE@1@GPAE1E@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _SetFilter( IntPtr classPointer, BudgetFilterType eType, UInt16 wPID, byte[] pbData, byte[] pbMask, byte bLength );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?ResetFilter@CDVBTSFilter@@QAE?AW4DVB_ERROR@@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _ResetFilter( IntPtr classPointer );

        private delegate void InternalDataArrivalHandler( IntPtr pBuf, Int32 lBuf );

        private FilterQueue m_FilterQueue;

        private long m_Bytes = 0;

        public FilterToCode( ushort pid, Action<byte[]> callback )
            : base( pid )
        {
            // Remember
            m_FilterQueue = new FilterQueue( callback );

            // Create the MFC wrapper
            m_Class = new ClassHolder( TTBudget.LegacySize.CDVBTSFilter );

            // Construct C++ instance
            _Construct( m_Class.ClassPointer );

            // Attach destructor
            m_Class.Destructor = new ClassHolder.DestructHandler( _Destruct );

            // Overload virtual function
            m_Class[1] = new InternalDataArrivalHandler( OnDataArrival );
        }

        private void OnDataArrival( IntPtr pBuf, Int32 lBuf )
        {
            // Synchronize
            lock (this)
            {
                // Count
                m_Bytes += lBuf;

                // Already done
                if (null == m_Class) return;
            }

            // Allocate array
            byte[] data = new byte[lBuf];

            // Fill it
            Marshal.Copy( pBuf, data, 0, lBuf );

            // Process
            m_FilterQueue.Enqueue( data );
        }

        protected override void OnStart()
        {
            // Reset
            m_Bytes = 0;

            // Set up filter
            DVBException.ThrowOnError( _SetFilter( m_Class.ClassPointer, BudgetFilterType.Piping, FilterPID, null, null, (byte) PipeSize.Eight ), "Unable to filter PID (piping)" );
        }

        protected override void OnStart( byte[] filterData, byte[] filterMask )
        {
            // Reset
            m_Bytes = 0;

            // Start in memory filter
            DVBException.ThrowOnError( _SetFilter( m_Class.ClassPointer, BudgetFilterType.Section, FilterPID, filterData, filterMask, (byte) filterData.Length ), "Unable to filter PID (section)" );
        }

        protected override void OnStop()
        {
            // Execute
            _ResetFilter( m_Class.ClassPointer );

            // Disconnect from handler
            m_FilterQueue.Dispose();
        }

        public override long Length
        {
            get
            {
                // Report
                lock (this) return m_Bytes;
            }
        }
    }
}
