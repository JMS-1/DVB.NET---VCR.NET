using System;
using System.Security;
using System.Threading;
using System.Runtime.InteropServices;

using JMS.TechnoTrend;


namespace JMS.DVB.Provider.TTBudget
{
    internal class BoardControl : IDisposable
    {
        [DllImport( "ttlcdacc.dll", EntryPoint = "??0CDVBBoardControl@@QAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void _Construct( IntPtr classPointer );

        [DllImport( "ttlcdacc.dll", EntryPoint = "??1CDVBBoardControl@@UAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void _Destruct( IntPtr classPointer );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?ResetInit@CDVBBoardControl@@QAE?AW4DVB_ERROR@@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _ResetInit( IntPtr classPointer );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?EnableDataDMA@CDVBBoardControl@@QAE?AW4DVB_ERROR@@H@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _EnableDataDMA( IntPtr classPointer, bool enable );

        private ClassHolder m_Class = null;

        public BoardControl()
        {
            // Create the MFC wrapper
            m_Class = new ClassHolder( TTBudget.LegacySize.CDVBBoardControl );

            // Construct C++ instance
            _Construct( m_Class.ClassPointer );

            // Attach destructor
            m_Class.Destructor = new ClassHolder.DestructHandler( _Destruct );
        }

        public void Initialize()
        {
            // Reset
            DVBException.ThrowOnError( _ResetInit( m_Class.ClassPointer ), "Unable to initialize board" );

            // Relax a bit
            Thread.Sleep( 100 );
        }

        public void EnableDMA()
        {
            // Enable DMA
            DVBException.ThrowOnError( _EnableDataDMA( m_Class.ClassPointer, true ), "Unable to activate DMA" );
        }

        #region IDisposable Members

        public void Dispose()
        {
            // Load the class
            ClassHolder instance;

            // Protected
            lock (this)
            {
                // Load
                instance = m_Class;

                // Forget
                m_Class = null;
            }

            // Wipe out
            if (null != instance)
            {
                // Try to stop
                try
                {
                    // Shut down
                    _EnableDataDMA( instance.ClassPointer, false );
                }
                catch
                {
                    // Ignore any error
                }

                // Discard
                instance.Dispose();
            }
        }

        #endregion
    }
}
