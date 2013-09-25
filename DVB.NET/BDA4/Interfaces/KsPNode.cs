using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt eine Eigenschaften eines Steuerelementes.
    /// </summary>
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct KsPNode
    {
        /// <summary>
        /// Die Speichergröße einer Beschreibung.
        /// </summary>
        public static readonly int SizeOf = Marshal.SizeOf( typeof( KsPNode ) );

        /// <summary>
        /// Die gewünschte Eigenschaft.
        /// </summary>
        public KsIdentifier Property;

        /// <summary>
        /// Die laufende Nummer des betroffenen Steuerelementes.
        /// </summary>
        public UInt32 NodeId;

        /// <summary>
        /// Reserviert für zukünftige Zwecke.
        /// </summary>
        private UInt32 Reserved;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <typeparam name="T">Die Art der Auswahl des Steuerelementes.</typeparam>
        /// <param name="identifier">Die Kennzeichnung der Eigenschaft.</param>
        /// <param name="node">Das zu verwendende Steuerelement.</param>
        /// <returns>Die gewünschte neue Beschreibung.</returns>
        public static KsPNode Create<T>( KsIdentifier identifier, T node ) where T : struct
        {
            // Create
            return new KsPNode { Property = identifier, NodeId = Convert.ToUInt32( node ) };
        }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <typeparam name="NodeType">Die Art der Auswahl des Steuerelementes.</typeparam>
        /// <typeparam name="IdentifierType">Die Art der Nummerierung von Eigenschaften.</typeparam>
        /// <param name="propertySet">Die Kennung der zu verwendenden Gruppe von Eigenschaften.</param>
        /// <param name="identifier">Die Kennzeichnung der Eigenschaft.</param>
        /// <param name="node">Das zu verwendende Steuerelement.</param>
        /// <returns>Die gewünschte neue Beschreibung.</returns>
        public static KsPNode Create<NodeType, IdentifierType>( Guid propertySet, IdentifierType identifier, NodeType node )
            where NodeType : struct
            where IdentifierType : struct
        {
            // Create
            return new KsPNode { Property = KsIdentifier.Create( propertySet, identifier ), NodeId = Convert.ToUInt32( node ) };
        }
    }
}
