using System;
using System.Security;
using System.Runtime.InteropServices;

using JMS.TechnoTrend;


namespace JMS.DVB.Provider.TTBudget
{
    internal class CommonInterface : IDisposable
    {
        [DllImport( "ttlcdacc.dll", EntryPoint = "??0CDVBComnIF@@QAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void _Construct( IntPtr classPointer );

        [DllImport( "ttlcdacc.dll", EntryPoint = "??1CDVBComnIF@@UAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void _Destruct( IntPtr classPointer );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?Open@CDVBComnIF@@QAE?AW4DVB_ERROR@@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _Open( IntPtr classPointer );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?Close@CDVBComnIF@@QAE?AW4DVB_ERROR@@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _Close( IntPtr classPointer );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?ReadPSIFast@CDVBComnIF@@QAE?AW4DVB_ERROR@@G@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _ReadPSIFast( IntPtr classPointer, UInt16 serviceIdentifier );

        private ClassHolder m_Class = null;

        private bool m_Open = false;

        public CommonInterface()
        {
            // Create the MFC wrapper
            m_Class = new ClassHolder( TTBudget.LegacySize.CDVBComnIF );

            // Construct C++ instance
            _Construct( m_Class.ClassPointer );

            // Attach destructor
            m_Class.Destructor = new ClassHolder.DestructHandler( _Destruct );
        }

        public bool IsIdle
        {
            get
            {
                // Report
                return !m_Open;
            }
        }

        public void Decrypt( ushort serviceIdentifier )
        {
            // Not active
            if (!m_Open)
            {
                // Nothing to decrypt
                if (0 == serviceIdentifier) return;

                // Open access
                Open();
            }

            // Process
            DVBException.ThrowOnError( _ReadPSIFast( m_Class.ClassPointer, serviceIdentifier ), "Unable to activate or deactivate decryption" );
        }

        public void Open()
        {
            // Close first
            Close();

            // Process
            DVBException.ThrowOnError( _Open( m_Class.ClassPointer ), "Unable to open CI" );

            // Remember
            m_Open = true;
        }

        public void Close()
        {
            // Not necessary
            if (!m_Open) return;

            // Process
            DVBException.ThrowOnError( _Close( m_Class.ClassPointer ), "Unable to close CI" );

            // Remember
            m_Open = false;
        }

        #region IDisposable Members

        public void Dispose()
        {
            // Terminate
            Close();

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
            if (null != instance) instance.Dispose();
        }

        #endregion
    }
}
