using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess
{
    /// <summary>
    /// Diese Klasse verwaltet eine COM Schnittstelle.
    /// </summary>
    public abstract class ComIdentity : IDisposable
    {
        /// <summary>
        /// Initialisiert die Basisklasse.
        /// </summary>
        protected ComIdentity()
        {
        }

        /// <summary>
        /// Meldet die Schnittstelle in der Rohform.
        /// </summary>
        public abstract IntPtr Interface { get; }

        /// <summary>
        /// Erhöht die Referenz auf der COM Instanz und meldet diese.
        /// </summary>
        /// <returns>Die aktuelle COM Referenz.</returns>
        public IntPtr AddRef()
        {
            // Process
            if (Interface != IntPtr.Zero)
                Marshal.AddRef( Interface );

            // Report
            return Interface;
        }

        /// <summary>
        /// Erzeugt eine neue Referenz.
        /// </summary>
        /// <typeparam name="T">Die zugehörige .NET Schnittstelle.</typeparam>
        /// <param name="instance">Eine .NET Referenz.</param>
        /// <returns>Die verwaltete COM Referenz.</returns>
        public static TypedComIdentity<T> Create<T>( object instance ) where T : class
        {
            // Test mode and create accordingly
            var identity = instance as ComIdentity;
            if (identity == null)
                return new TypedComIdentity<T>( instance );
            else
                return new TypedComIdentity<T>( QueryInterface( identity.Interface, typeof( T ) ) );
        }

        /// <summary>
        /// Erzeugt eine neue Referenz.
        /// </summary>
        /// <typeparam name="T">Die Art der Referenz.</typeparam>
        /// <param name="instance">Ein beliebiges Objekt.</param>
        /// <returns>Die gewünschte Referenz.</returns>
        public static TypedComIdentity<T> Create<T>( T instance ) where T : class
        {
            // Test mode and create accordingly
            return new TypedComIdentity<T>( instance );
        }

        /// <summary>
        /// Erzeugt eine neue Referenz unter Angabe eines eindeutigen Namens.
        /// </summary>
        /// <typeparam name="T">Die Art des verwalteten Objektes.</typeparam>
        /// <param name="moniker">Der eindeutige Namen zum Objekt.</param>
        /// <returns>Die gewünschte Referenz.</returns>
        public static TypedComIdentity<T> Create<T>( string moniker ) where T : class
        {
            // Create
            var instance = Marshal.BindToMoniker( moniker );
            try
            {
                // Report
                return Create( (T) instance );
            }
            finally
            {
                // Cleanup
                BDAEnvironment.Release( ref instance );
            }
        }

        /// <summary>
        /// Ermittelt eine bestimmte COM Schnittstelle.
        /// </summary>
        /// <param name="com">Eine COM Schnittstelle.</param>
        /// <param name="type">Die .NET Schnittstelle, deren COM Gegenstück erfragt werden soll.</param>
        /// <returns>Die angeforderte COM Schnittstelle.</returns>
        /// <exception cref="ArgumentException">Die .NET Schnittstelle verwendet nicht das <see cref="GuidAttribute"/> oder
        /// die COM Schnittstelle bietet die .NET Schnittstelle nicht zum alternativen Zugriff an.</exception>
        public static IntPtr QueryInterface( IntPtr com, Type type )
        {
            // Forward
            var qi = TryQueryInterface( com, type );
            if (qi != IntPtr.Zero)
                return qi;
            else
                throw new ArgumentException( "com" );
        }

        /// <summary>
        /// Ermittelt eine bestimmte COM Schnittstelle.
        /// </summary>
        /// <param name="com">Eine COM Schnittstelle.</param>
        /// <param name="type">Die .NET Schnittstelle, deren COM Gegenstück erfragt werden soll.</param>
        /// <returns>Die angeforderte COM Schnittstelle.</returns>
        /// <exception cref="ArgumentException">Die .NET Schnittstelle verwendet nicht das <see cref="GuidAttribute"/> oder
        /// die COM Schnittstelle bietet die .NET Schnittstelle nicht zum alternativen Zugriff an.</exception>
        public static IntPtr TryQueryInterface( IntPtr com, Type type )
        {
            // Get the Guid
            var attributes = type.GetCustomAttributes( typeof( GuidAttribute ), false );

            // None
            if ((attributes == null) || (attributes.Length != 1))
                throw new ArgumentException( "instance" );

            // Attach
            var guidAttr = (GuidAttribute) attributes[0];
            var guid = new Guid( guidAttr.Value );

            // Create
            IntPtr qi;
            if (Marshal.QueryInterface( com, ref guid, out qi ) >= 0)
                return qi;
            else
                return IntPtr.Zero;
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        public abstract void Dispose();

        #endregion
    }
}
