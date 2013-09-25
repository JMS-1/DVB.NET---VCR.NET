using System;
using System.Security;
using System.Runtime.InteropServices;

using JMS.TechnoTrend;


namespace JMS.DVB.Provider.TTBudget
{
    internal class FilterToFile : Filter
    {
        [DllImport( "ttlcdacc.dll", EntryPoint = "??0CDVBFilterToFile@@QAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void _Construct( IntPtr classPointer );

        [DllImport( "ttlcdacc.dll", EntryPoint = "??1CDVBFilterToFile@@UAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void _Destruct( IntPtr classPointer );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?SetFileParams@CDVBFilterToFile@@QAEXABV?$CStringT@DV?$StrTraitMFC_DLL@DV?$ChTraitsCRT@D@ATL@@@@@ATL@@I@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void _SetFileParams( IntPtr classPointer, IntPtr namePointer, UInt32 openFlags );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?StartFilter@CDVBFilterToFile@@QAE?AW4DVB_ERROR@@W4FILTERTYPE@CDVBTSFilter@@GPAE1E@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _StartFilter( IntPtr classPointer, BudgetFilterType eType, UInt16 wPID, byte[] pbData, byte[] pbMask, byte bLength );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?StopFilter@CDVBFilterToFile@@QAE?AW4DVB_ERROR@@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _StopFilter( IntPtr classPointer );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?GetFileLen@CDVBFilterToFile@@QAEKXZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern UInt32 _GetFileLen( IntPtr classPointer );

        public FilterToFile( ushort pid, string filePath )
            : base( pid )
        {
            // Create the MFC wrapper
            m_Class = new ClassHolder( TTBudget.LegacySize.CDVBFilterToFile );

            // Construct C++ instance
            _Construct( m_Class.ClassPointer );

            // Attach destructor
            m_Class.Destructor = new ClassHolder.DestructHandler( _Destruct );

            // Create MFC string
            using (MFCString path = new MFCString( filePath ))
            {
                // Configure file - flags are Create and Write
                _SetFileParams( m_Class.ClassPointer, path.ClassPointer, 0x1001 );
            }
        }

        protected override void OnStart()
        {
            // Set up filter
            DVBException.ThrowOnError( _StartFilter( m_Class.ClassPointer, BudgetFilterType.Piping, FilterPID, null, null, (byte) PipeSize.Eight ), "Unable to filter PID (piping)" );
        }

        protected override void OnStart( byte[] filterData, byte[] filterMask )
        {
            // Start in memory filter
            DVBException.ThrowOnError( _StartFilter( m_Class.ClassPointer, BudgetFilterType.Section, FilterPID, filterData, filterMask, (byte) filterData.Length ), "Unable to filter PID (section)" );
        }

        protected override void OnStop()
        {
            // Execute
            _StopFilter( m_Class.ClassPointer );
        }

        public override long Length
        {
            get
            {
                // Report
                return _GetFileLen( m_Class.ClassPointer );
            }
        }
    }
}
