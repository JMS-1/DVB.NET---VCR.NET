using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess
{
    /// <summary>
    /// Verwaltet eine COM Schnittstelle zu einer festgelegten .NET Schnittstelle.
    /// </summary>
    /// <typeparam name="T">Die verwaltete .NET Schnittstelle.</typeparam>
    public class TypedComIdentity<T> : ComIdentity where T : class
    {
        /// <summary>
        /// Verwaltet eine Rekonstruktion als .NET Instanz.
        /// </summary>
        public class Incarnation : IDisposable
        {
            /// <summary>
            /// Das zu verwaltende Objekt.
            /// </summary>
            private T m_Object;

            /// <summary>
            /// Meldet das zu verwaltende Objekt.
            /// </summary>
            public T Object
            {
                get
                {
                    // Report
                    return m_Object;
                }
            }

            /// <summary>
            /// Erzeugt eine neue Verwaltung.
            /// </summary>
            /// <param name="object">Das zu verwaltende Objekt.</param>
            internal Incarnation( T @object )
            {
                // Remember
                m_Object = @object;
            }

            #region IDisposable Members

            /// <summary>
            /// Beendet die Nutzung dieser Instanz endgültig.
            /// </summary>
            public void Dispose()
            {
                // Cleanup
                BDAEnvironment.Release( ref m_Object );
            }

            #endregion
        }

        /// <summary>
        /// Die zugehörige COM Schnittstelle.
        /// </summary>
        private IntPtr m_Interface;

        /// <summary>
        /// Initialisiert die Verwaltung.
        /// </summary>
        protected TypedComIdentity()
        {
            // Create CCW
            m_Interface = Marshal.GetComInterfaceForObject( this, typeof( T ) );
        }

        /// <summary>
        /// Erzeugt eine neue Verwaltung.
        /// </summary>
        /// <param name="instance">Ein beliebiges .NET Objekt.</param>
        public TypedComIdentity( object instance )
            : this( Marshal.GetComInterfaceForObject( instance, typeof( T ) ) )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Verwaltung.
        /// </summary>
        /// <param name="instance">Eine beliebige COM Schnittstelle.</param>
        public TypedComIdentity( IntPtr instance )
        {
            // Create CCW
            m_Interface = instance;
        }

        /// <summary>
        /// Erzeugt eine .NET Schnittstelle zum verwalteten COM Objekt.
        /// </summary>
        /// <returns>Das gewünschte Objekt für den .NET Zugriff</returns>
        public Incarnation MarshalToManaged()
        {
            // Create
            var instance = Marshal.GetObjectForIUnknown( Interface );
            try
            {
                // Just change type type
                return new Incarnation( (T) instance );
            }
            catch
            {
                // Cleanup
                BDAEnvironment.Release( ref instance );

                // Forward
                throw;
            }
        }

        /// <summary>
        /// Meldet die verwaltete COM Schnittstelle.
        /// </summary>
        public override IntPtr Interface
        {
            get
            {
                // Report
                return m_Interface;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        public override void Dispose()
        {
            // Check interface
            BDAEnvironment.Release( ref m_Interface );
        }

        #endregion
    }
}
