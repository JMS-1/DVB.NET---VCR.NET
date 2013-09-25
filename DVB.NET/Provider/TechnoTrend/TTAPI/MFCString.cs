using System;
using System.Security;
using System.Runtime.InteropServices;


namespace JMS.TechnoTrend
{
    /// <summary>
    /// Wraps a MFC ANSI <i>CString</i> - located in the MFC71.dll. Only few methods are implemented
    /// right now.
    /// </summary>
    /// <remarks>
    /// The client must call <see cref="Dispose"/> as sonn as the <i>CString</i>
    /// is no longer needed.
    /// </remarks>
    public class MFCString : IDisposable
    {
        /// <summary>
        /// Construct a new <i>CString</i> instance from a <see cref="string"/>.
        /// </summary>
        [DllImport( "MFC71.dll", EntryPoint = "#300", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CString_Construct( IntPtr pData, string sData );

        /// <summary>
        /// Destruct a <i>CString</i> instance.
        /// </summary>
        [DllImport( "MFC71.dll", EntryPoint = "#578", CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CString_Destruct( IntPtr pData );

        /// <summary>
        /// The interoperability mapper. 
        /// </summary>
        /// <remarks>
        /// Will be created using four bytes of C++ instance memory and zero
        /// virtual jump table entries.
        /// </remarks>
        private ClassHolder m_Class = null;

        /// <summary>
        /// Create a new <i>CString</i> initialized with the indicated data.
        /// </summary>
        /// <param name="sData">Initial data.</param>
        public MFCString( string sData )
        {
            // Create memory
            m_Class = new ClassHolder( 4, 0 );

            // Construct it
            CString_Construct( m_Class.ClassPointer, sData );

            // Prepare for shut down
            m_Class.Destructor = new ClassHolder.DestructHandler( CString_Destruct );
        }

        /// <summary>
        /// Proper cleanup by calling <see cref="Dispose"/> if the client forgets
        /// to do so.
        /// </summary>
        ~MFCString()
        {
            // Shut down
            Dispose();
        }

        /// <summary>
        /// On the first call <see cref="ClassHolder.Dispose"/> is called on the
        /// <see cref="m_Class"/> wrapper.
        /// </summary>
        public void Dispose()
        {
            // Forward
            if (null != m_Class) m_Class.Dispose();

            // Once
            m_Class = null;

            // No need to finalize
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Forward call to <see cref="ClassHolder.ClassPointer"/> of the <see cref="m_Class"/>.
        /// This allows a client to call MFC extension DLL methods which require a
        /// <i>CString *</i> or <i>CString &amp;</i> parameter.
        /// </summary>
        /// <remarks>
        /// Using <i>CString</i> by-value parameters is somewhat tricky and not
        /// yet supported.
        /// </remarks>
        public IntPtr ClassPointer
        {
            get
            {
                // Report
                return m_Class.ClassPointer;
            }
        }
    }
}
