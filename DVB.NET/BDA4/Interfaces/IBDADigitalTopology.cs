using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Erlaubt den Zugriff auf die Topologie in einem BDA DVB Filter.
    /// </summary>
    [
        ComImport,
        Guid( "79b56888-7fea-4690-b45d-38fd3c7849be" ),
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown )
    ]
    public interface IBDADigitalTopology
    {
        /// <summary>
        /// Meldet die Knotenarten.
        /// </summary>
        /// <param name="count">Anzahl der ausgelesenen Arten.</param>
        /// <param name="arraySize">Maximale Anzahl von zu meldenden Arten.</param>
        /// <param name="typeArray">Feld zur Ablage der Arten.</param>
        void GetNodeTypes( out UInt32 count, Int32 arraySize, IntPtr typeArray );

        /// <summary>
        /// Ermittelt Knotenbeschreibungen.
        /// </summary>
        void GetNodeDescriptors();

        /// <summary>
        /// Meldet zu einem Knoten die Schnittstellen.
        /// </summary>
        /// <param name="nodeType">Die Art des Knotens.</param>
        /// <param name="count">Die gemeldete Anzahl von Schnittstellen.</param>
        /// <param name="arraySize">Maximale Anzahl von zu meldenden Schnittstellen.</param>
        /// <param name="guidArray">Feld zur Aufnahme der Schnittstellen.</param>
        void GetNodeInterfaces( UInt32 nodeType, out UInt32 count, Int32 arraySize, IntPtr guidArray );

        /// <summary>
        /// Meldet die Arten der Endpunkte.
        /// </summary>
        void GetPinTypes();

        /// <summary>
        /// Meldet alle Musterverbindungen.
        /// </summary>
        void GetTemplateConnections();

        /// <summary>
        /// Legt einen neuen Endpunkt an.
        /// </summary>
        void CreatePin();

        /// <summary>
        /// Entfernt einen Endpunkt.
        /// </summary>
        void DeletePin();

        /// <summary>
        /// Legt die Art der Daten fest.
        /// </summary>
        void SetMediaType();

        /// <summary>
        /// Legt das Medium fest.
        /// </summary>
        void SetMedium();

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        void CreateTopology();

        /// <summary>
        /// Ermittelt eine Schnittstelle zu einem Knoten.
        /// </summary>
        /// <param name="inputPin">Die laufenden Nummer des eingangsseitigen Endpunktes.</param>
        /// <param name="outputPin">Die laufende Nummer des ausgangsseitigen Endpunktes.</param>
        /// <param name="nodeType">Die Art des Knotens.</param>
        /// <returns>Die angeforderte COM Schnittstelle.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        object GetControlNode( UInt32 inputPin, UInt32 outputPin, UInt32 nodeType );
    };
}
