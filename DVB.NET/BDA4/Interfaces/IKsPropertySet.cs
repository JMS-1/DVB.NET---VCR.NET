using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Vermittelt den Zugriff auf Eigenschaften eines Ger�tes.
    /// </summary>
    [
        ComImport,
        Guid( "31efac30-515c-11d0-a9aa-00aa0061be93" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IKsPropertySet
    {
        /// <summary>
        /// �ndert eine Eigenschaft.
        /// </summary>
        /// <param name="PropSet">Die Gruppe der Eigenschaft.</param>
        /// <param name="Id">Die Kennung der Eigenschaft.</param>
        /// <param name="InstanceData">Die Identifikation der Instanz.</param>
        /// <param name="InstanceLength">Die Gr��e der Identifikation.</param>
        /// <param name="PropertyData">Die Daten der Eigenschaft.</param>
        /// <param name="DataLength">Die Gr��er der Daten.</param>
        void Set( ref Guid PropSet, UInt32 Id, IntPtr InstanceData, UInt32 InstanceLength, IntPtr PropertyData, UInt32 DataLength );

        /// <summary>
        /// Liest den Wert einer Eigenschaft.
        /// </summary>
        /// <param name="PropSet">Die Gruppe der Eigenschaft.</param>
        /// <param name="Id">Die Kennung der Eigenschaft.</param>
        /// <param name="InstanceData">Die Identifikation der Instanz.</param>
        /// <param name="InstanceLength">Die Gr��e der Identifikation.</param>
        /// <param name="PropertyData">Die Daten der Eigenschaft.</param>
        /// <param name="DataLength">Die Gr��er der Daten.</param>
        /// <returns>Das Ergebnis der Abfrage, negative Werte weisen auf eine Fehlersituation hin.</returns>
        [return: MarshalAs( UnmanagedType.U4 )]
        UInt32 Get( ref Guid PropSet, UInt32 Id, IntPtr InstanceData, UInt32 InstanceLength, IntPtr PropertyData, UInt32 DataLength );

        /// <summary>
        /// Pr�ft die Unterst�tzung einer Eigenschaft.
        /// </summary>
        /// <param name="PropSet">Die Gruppe der Eigenschaft.</param>
        /// <param name="Id">Die Kennung der Eigenschaft.</param>
        /// <returns>Die Unterst�tzung der Eigenschaft.</returns>
        [return: MarshalAs( UnmanagedType.I4 )]
        PropertySetSupportedTypes QuerySupported( ref Guid PropSet, UInt32 Id );
    }

    /// <summary>
    /// Vermittelt den Zugriff auf Eigenschaften eines Ger�tes.
    /// </summary>
    /// <typeparam name="DataType">Die Art der Daten.</typeparam>
    public interface IKsPropertySet<DataType> : IDisposable
        where DataType : struct
    {
        /// <summary>
        /// Pr�ft, in welchem Umfang eine bestimmte Eigenschaft unterst�tzt wird.
        /// </summary>
        /// <param name="identifier">Die Beschreibung der Eigenschaft.</param>
        /// <param name="types">Die ben�tigte Art der Unterst�tzung.</param>
        /// <returns>Gesetzt, wenn die gew�nschte Unterst�tzung m�glich ist.</returns>
        bool DoesSupport( KsIdentifier identifier, PropertySetSupportedTypes types );

        /// <summary>
        /// Setzt einen Wert.
        /// </summary>
        /// <param name="node">Die Beschreibung der Eigenschaft.</param>
        /// <param name="PropertyData">Die neuen Daten.</param>
        void Set( KsPNode node, DataType PropertyData );
    }

    /// <summary>
    /// Vermittelt den typisierten Zugriff auf die <i>IKsPropertySet</i> Schnittstelle.
    /// </summary>
    public sealed class KsPropertySet
    {
        /// <summary>
        /// Vermittelt den typisierten Zugriff auf die <i>IKsPropertySet</i> Schnittstelle.
        /// </summary>
        /// <typeparam name="DataType">Die Art der Daten.</typeparam>
        private class Typed<DataType> : IKsPropertySet<DataType>
            where DataType : struct
        {
            /// <summary>
            /// Die Gr��e der zu �bertragenden Bytes.
            /// </summary>
            private static readonly int SizeOf = typeof( DataType ).IsEnum ? sizeof( Int32 ) : Marshal.SizeOf( typeof( DataType ) );

            /// <summary>
            /// Die zugeh�rige Instanz eines Objektes.
            /// </summary>
            private TypedComIdentity<IKsPropertySet>.Incarnation m_Instance;

            /// <summary>
            /// Initialisiert eine neue Instanz.
            /// </summary>
            /// <param name="instance">Die zu verwaltende Instanz eines COM Objektes.</param>
            public Typed( TypedComIdentity<IKsPropertySet>.Incarnation instance )
            {
                // Remember
                m_Instance = instance;
            }

            /// <summary>
            /// Pr�ft, in welchem Umfang eine bestimmte Eigenschaft unterst�tzt wird.
            /// </summary>
            /// <param name="identifier">Die Beschreibung der Eigenschaft.</param>
            /// <param name="types">Die ben�tigte Art der Unterst�tzung.</param>
            /// <returns>Gesetzt, wenn die gew�nschte Unterst�tzung m�glich ist.</returns>
            public bool DoesSupport( KsIdentifier identifier, PropertySetSupportedTypes types )
            {
                // Unmap
                var propertySetIdentifier = identifier.Set;

                // Forward
                return ((m_Instance.Object.QuerySupported( ref propertySetIdentifier, identifier.Id ) & types) == types);
            }

            /// <summary>
            /// Setzt einen Wert.
            /// </summary>
            /// <param name="node">Die Beschreibung der Eigenschaft.</param>
            /// <param name="PropertyData">Die neuen Daten.</param>
            public void Set( KsPNode node, DataType PropertyData )
            {
                // Unmap
                var propertySetIdentifier = node.Property.Set;

                // Allocate the memory for explicit marshalling
                var dataMemory = Marshal.AllocHGlobal( SizeOf );
                try
                {
                    // Lock in memory
                    var nodeMemory = Marshal.AllocHGlobal( KsPNode.SizeOf );
                    try
                    {
                        // Fill - must box to support enumerations
                        Marshal.StructureToPtr( typeof( DataType ).IsEnum ? Convert.ToInt32( PropertyData ) : (object) PropertyData, dataMemory, false );
                        Marshal.StructureToPtr( node, nodeMemory, false );

                        // Update the property
                        m_Instance.Object.Set( ref propertySetIdentifier, node.Property.Id, nodeMemory, (uint) KsPNode.SizeOf, dataMemory, (uint) SizeOf );
                    }
                    finally
                    {
                        // Cleanup
                        Marshal.FreeHGlobal( nodeMemory );
                    }
                }
                finally
                {
                    // Cleanup
                    Marshal.FreeHGlobal( dataMemory );
                }
            }

            #region IDisposable Members

            /// <summary>
            /// Beendet die Nutzung dieser Instanz endg�ltig.
            /// </summary>
            public void Dispose()
            {
                // Cleanup
                using (m_Instance)
                    m_Instance = null;
            }

            #endregion
        }

        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        /// <typeparam name="DataType">Die Art der Daten.</typeparam>
        /// <param name="interface">Die zu verwendende Schnittstelle.</param>
        /// <returns>Eine neue Zugriffsinstanz.</returns>
        public static IKsPropertySet<DataType> Create<DataType>( IntPtr @interface )
            where DataType : struct
        {
            // None
            if (@interface == null)
                return null;

            // Try to access the interface
            var com = ComIdentity.TryQueryInterface( @interface, typeof( IKsPropertySet ) );
            if (com == IntPtr.Zero)
                return null;

            // Be safe
            try
            {
                // Create the wrapper
                using (var typed = new TypedComIdentity<IKsPropertySet>( com ))
                {
                    // No longer needed to release
                    com = IntPtr.Zero;

                    // Create wrapper
                    var wrapper = typed.MarshalToManaged();
                    try
                    {
                        // Safe process
                        return new Typed<DataType>( wrapper );
                    }
                    catch
                    {
                        // Cleanup
                        wrapper.Dispose();

                        // Forward
                        throw;
                    }
                }
            }
            finally
            {
                // Cleanup
                BDAEnvironment.Release( ref com );
            }
        }
    }

    /// <summary>
    /// Hilfsmethoden zur einfacheren Nutzung der <see cref="IKsPropertySet"/> Schnittstelle.
    /// </summary>
    public static class KsPropertySetExtensions
    {
        /// <summary>
        /// Pr�ft, in welchem Umfang eine bestimmte Eigenschaft unterst�tzt wird.
        /// </summary>
        /// <param name="set">Die zu erweiternde Schnittstelle.</param>
        /// <param name="node">Die Beschreibung der Eigenschaft.</param>
        /// <param name="types">Die ben�tigte Art der Unterst�tzung.</param>
        /// <returns>Gesetzt, wenn die gew�nschte Unterst�tzung m�glich ist.</returns>
        public static bool DoesSupport<T>( this IKsPropertySet<T> set, KsPNode node, PropertySetSupportedTypes types ) where T : struct
        {
            // Process
            if (set == null)
                throw new ArgumentNullException( "set" );
            else
                return set.DoesSupport( node.Property, types );
        }
    }
}
