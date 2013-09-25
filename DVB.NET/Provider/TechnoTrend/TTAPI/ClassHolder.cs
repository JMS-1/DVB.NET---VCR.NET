using System;
using System.Security;
using System.Collections;
using System.Runtime.InteropServices;


namespace JMS.TechnoTrend
{
    /// <summary>
    /// This class is a wrapper to a C++ class located in some MFC extension DLL.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The major task is to keep the memory allocated for the class pinned on a fixed
    /// address. The class will hold data for the C++ instance in <see cref="m_ClassMem"/>
    /// and a virtual jump table in <see cref="m_TableMem"/>. The caller is responsible
    /// to use the correct constructor with appropriate sizes for these areas. Currently
    /// on 32-Bit address spaces are supported.
    /// </para>
    /// <para>
    /// For proper memory and resource management the <see cref="IDisposable"/> interface
    /// is provided - in addition to a C# destructor which may not be of much help in
    /// most situations. The client is reponsible for calling the <see cref="Dispose"/>
    /// method when finished with the related C++ instance.
    /// </para>
    /// </remarks>
    /// <example>
    /// Normally a client would not directly use a <see cref="ClassHolder"/> instance. 
    /// Typically a special C++/.NET wrapper would provide all necessary interoperability
    /// functions. 
    /// <code>
    ///	[DllImport("MyDLL.dll", EntryPoint="??0MyClass@@QAE@XZ", CallingConvention=CallingConvention.ThisCall)] private static extern void MyClass_Construct(IntPtr pData);
    ///	[DllImport("MyDLL.dll", EntryPoint="??1MyClass@@UAE@XZ", CallingConvention=CallingConvention.ThisCall)] private static extern void MyClass_Destruct(IntPtr pData);
    ///
    ///	ClassHolder pWrapper = new ClassHolder()
    ///		
    ///	MyClass_Construct(pWrapper.ClassPointer);
    ///		
    ///	pWrapper.Destructor = new ClassHolder.DestructHandler(MyClass_Destruct);
    ///		
    ///	try
    ///	{
    ///		// Call methods on the wrapper using pWrapper.ClassPointer as the first parameter for instance calls
    ///	}
    ///	finally
    ///	{
    ///		// Can use the using() statement, too
    ///		pWrapper.Dispose();
    ///	}
    /// </code>
    /// To find the entry point names to be used with <see cref="DllImportAttribute.EntryPoint"/> the
    /// <i>Depends.EXE</i> from Visual Studio.NET can be used. An options allows switching between
    /// decorated and undecorated names.
    /// </example>
    public class ClassHolder : IDisposable
    {
        /// <summary>
        /// We use the <i>InterlockedExchangeAdd</i> in <see cref="AddressOf"/> to get
        /// the address of a <see cref="Delegate"/>. 
        /// </summary>
        /// <remarks>
        /// The address calculated will be used in <see cref="this"/> to update the
        /// virtual jump table <see cref="m_TableMem"/> with overloaded virtual
        /// functions.
        /// </remarks>
        [DllImport( "Kernel32.dll", EntryPoint = "InterlockedExchangeAdd", ExactSpelling = true )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void AddressHelper( out IntPtr plData, Delegate dAdd );

        /// <summary>
        /// This <see cref="Delegate"/> is called on the class memory <see cref="m_ClassMem"/>
        /// when <see cref="Dispose"/> is invoked for the first time.
        /// </summary>
        /// <remarks>
        /// The client is responsible to call the constructor of this class and the constructor
        /// of the C++ class prior to setting the <see cref="Destructor"/> property.
        /// </remarks>
        public delegate void DestructHandler( IntPtr pData );

        /// <summary>
        /// Make sure that all registrations with <see cref="this"/> are not recognized
        /// by the <see cref="GC"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="GCHandle.Alloc(object, GCHandleType)"/> is invoked 
        /// on the <see cref="Delegate"/> to prevent
        /// garbage collection. The <see cref="GCHandle"/> is then added to this <see cref="ArrayList"/>.
        /// </remarks>
        private ArrayList m_Delegates = new ArrayList();

        /// <summary>
        /// This <see cref="Delegate"/> is called when <see cref="Dispose"/> is activated for
        /// the first time. It must be the C++ destructor entry point for the class instance
        /// we hold.
        /// </summary>
        private DestructHandler m_Destruct = null;

        /// <summary>
        /// Original virtual jump table which will be replaced the first time <see cref="this"/>
        /// is called.
        /// </summary>
        private IntPtr m_OrgTable = IntPtr.Zero;

        /// <summary>
        /// Memory allocated to the C++ class instance. The memory will be pinned using
        /// the <see cref="m_Class"/> field.
        /// </summary>
        private byte[] m_ClassMem = null;

        /// <summary>
        /// Memory allocated for a copy of the virtual jump table pinned using the
        /// <see cref="m_Table"/> member. The memory will be allocated the first time 
        /// <see cref="this"/> is called.
        /// </summary>
        private Int32[] m_TableMem = null;

        /// <summary>
        /// Pins the <see cref="m_ClassMem"/> using <see cref="GCHandle.Alloc(object, GCHandleType)"/>
        /// with the type <see cref="GCHandleType"/><i>.Pinned</i>.
        /// </summary>
        private GCHandle m_Class;

        /// <summary>
        /// Pins the <see cref="m_Table"/> using <see cref="GCHandle.Alloc(object, GCHandleType)"/>
        /// with the type <see cref="GCHandleType"/><i>.Pinned</i>.
        /// </summary>
        private GCHandle m_Table;

        /// <summary>
        /// Creates a new instance with <i>1000</i> bytes reserved for
        /// C++ instance memory and <i>20</i> virtual jump table entries.
        /// </summary>
        public ClassHolder()
            : this( 1000 )
        {
        }

        /// <summary>
        /// Creates a new instance with the indicated number of bytes
        /// reserved for C++ instance memory and <i>20</i> virtual
        /// jump table entries.
        /// </summary>
        /// <param name="lClassSize">Size of a C++ instance.</param>
        public ClassHolder( int lClassSize )
            : this( lClassSize, 20 )
        {
        }

        /// <summary>
        /// Create a new instance which free choice of memory allocation.
        /// </summary>
        /// <remarks>
        /// The first parameter defines the number of bytes reserved for
        /// the related C++ class instance. The client is responsible to 
        /// choose a correct value - if in doubt sizeof() shoud be used
        /// in a C++ environment. The second parameter is the number of
        /// entries in the virtual jump table. This parameter must only be
        /// correct if virtual function overloading is activated by using
        /// the <see cref="this"/> indexer.
        /// </remarks>
        /// <param name="lClassSize">Size of a C++ instance - must be 
        /// zero or positive.</param>
        /// <param name="lVirtCount">Number of entries in the virtual
        /// jump table - must be zero or positive.</param>
        public ClassHolder( int lClassSize, int lVirtCount )
        {
            // Allocate all
            m_ClassMem = new byte[lClassSize];
            m_TableMem = new Int32[lVirtCount];

            // Fix it
            m_Class = GCHandle.Alloc( m_ClassMem, GCHandleType.Pinned );
            m_Table = GCHandle.Alloc( m_TableMem, GCHandleType.Pinned );
        }

        /// <summary>
        /// Simply call <see cref="Dispose"/>. It is strongly recommended that
        /// a client of this class calls <see cref="Dispose"/> itsself without
        /// relying on the .NET garbage collector.
        /// </summary>
        ~ClassHolder()
        {
            // Detach all
            Dispose();
        }

        /// <summary>
        /// Update the <see cref="m_Destruct"/> <see cref="Delegate"/> call
        /// when the C++ class instance should be destroyed.
        /// </summary>
        public DestructHandler Destructor
        {
            set
            {
                // Remember
                m_Destruct = value;
            }
        }

        /// <summary>
        /// Uses <see cref="GCHandle.AddrOfPinnedObject"/> of <see cref="m_Class"/>
        /// to get the pinned memory address of the C++ instance memory.
        /// </summary>
        public IntPtr ClassPointer
        {
            get
            {
                // Report
                return m_Class.AddrOfPinnedObject();
            }
        }

        /// <summary>
        /// Called to destroy the C++ instance. This method can be called more than
        /// once: only the first call will do some actual work.
        /// </summary>
        /// <remarks>
        /// If the virtual jump table has been replaced the original data will be
        /// restores using <see cref="VirtualJumpTable"/>. Then the <see cref="m_Destruct"/>
        /// <see cref="Delegate"/> is called on the <see cref="ClassPointer"/>. Finally
        /// all data removed from the garbage collection is reattached to it. This
        /// includes all <see cref="Delegate"/> <see cref="m_Delegates"/> from
        /// calls to <see cref="this"/> and the fields <see cref="m_Table"/> and
        /// <see cref="m_Class"/>.
        /// </remarks>
        public void Dispose()
        {
            // Restore table
            if (IntPtr.Zero != m_OrgTable)
            {
                // Process
                VirtualJumpTable = m_OrgTable;

                // Once
                m_OrgTable = IntPtr.Zero;
            }

            // Cleanup
            if (null != m_Destruct)
            {
                // Process
                m_Destruct( ClassPointer );

                // Done
                m_Destruct = null;
            }

            // Release delegates
            foreach (GCHandle hDel in m_Delegates) hDel.Free();

            // Release handles
            if (m_Table.IsAllocated) m_Table.Free();
            if (m_Class.IsAllocated) m_Class.Free();

            // Detach
            m_Delegates.Clear();
            m_TableMem = null;
            m_ClassMem = null;

            // No need to finalize
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Replaces one function in the virtual jump table with the indicated 
        /// <see cref="Delegate"/>. The client is responsible to call this member
        /// with resonable parameters.
        /// </summary>
        /// <remarks>
        /// <para>
        /// On first call the original virtual jump table is stored in <see cref="m_OrgTable"/>
        /// using <see cref="VirtualJumpTable"/>. Its contents will be copied to the
        /// <see cref="m_TableMem"/> using the indicated number of elements from the
        /// constructor. The copy is then installed as a new virtual jump table.
        /// </para>
        /// <para>
        /// The <see cref="Delegate"/> will be removed from the garbage collector
        /// using <see cref="GCHandle.Alloc(object)"/> and stored in <see cref="m_Delegates"/>.
        /// The <see cref="AddressOf"/> method will then be used to retrieve its address
        /// and update the related entry in <see cref="m_TableMem"/>.
        /// </para>
        /// <para>
        /// In contrast to methods called on the C++ instance a virtual function will not use
        /// the C++ <i>this</i> as a first parameter - this is due to restriction of the .NET
        /// interoperability support on a <see cref="Delegate"/>: it is not possible to choose
        /// a <see cref="CallingConvention"/>. The client has to enter a <see cref="Delegate"/>
        /// which either does not need access to the C++ instance or has some way to access
        /// the <see cref="ClassPointer"/> - for example to call the base class original
        /// method attach to the entry in the virtual jump table.
        /// </para>
        /// </remarks>
        public unsafe Delegate this[int ix]
        {
            set
            {
                // Attach to virtual jump table
                if (IntPtr.Zero == m_OrgTable)
                {
                    // Get old table
                    m_OrgTable = VirtualJumpTable;

                    // Attach to pointer
                    void** pOrgTable = (void**) m_OrgTable.ToPointer();

                    // Copy over
                    for (int i = m_TableMem.Length; i-- > 0; ) m_TableMem[i] = (Int32) pOrgTable[i];

                    // Use it
                    VirtualJumpTable = m_Table.AddrOfPinnedObject();
                }

                // Keep it
                m_Delegates.Add( GCHandle.Alloc( value ) );

                // Attach 
                m_TableMem[ix] = AddressOf( value );
            }
        }

        /// <summary>
        /// Uses the <see cref="AddressHelper"/> to get the linear address of the
        /// <see cref="Delegate"/>. 
        /// </summary>
        /// <param name="pDel">Some <see cref="Delegate"/>.</param>
        /// <returns>(32-Bit) Address of the function attached to the <see cref="Delegate"/>.</returns>
        private static Int32 AddressOf( Delegate pDel )
        {
            // Helper
            IntPtr lDummy = IntPtr.Zero;

            // Load
            AddressHelper( out lDummy, pDel );

            // Done
            return lDummy.ToInt32();
        }

        /// <summary>
        /// Allows easy access to the virtual jump table in the C++ instance itself. This
        /// will include a couple of typecast between different levels of indirections on
        /// <i>void *</i>.
        /// </summary>
        /// <remarks>
        /// The virtual jump table - if it exists - is the first pointer in the C++ instance
        /// memory. Each item is the pointer to a function with the <see cref="CallingConvention"/><i>.ThisCall</i>
        /// semantics.
        /// </remarks>
        private unsafe IntPtr VirtualJumpTable
        {
            get
            {
                // Load
                return (IntPtr) (*(void***) m_Class.AddrOfPinnedObject().ToPointer());
            }
            set
            {
                // Update
                *(void***) m_Class.AddrOfPinnedObject().ToPointer() = (void**) value.ToPointer();
            }
        }
    }
}
