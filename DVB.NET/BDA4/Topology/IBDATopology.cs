using System;
using System.Runtime.InteropServices;

using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DeviceAccess.Topology
{
    /// <summary>
    /// Diese Schnittstelle erlaubt den Zugriff auf die interne Struktur eine
    /// DVB BDA Komponenten. Sie liefert Informationen über die verwendeten
    /// Hardwaregeräte und deren Zusammenspiel.
    /// </summary>
    [
        ComImport,
        InterfaceType( ComInterfaceType.InterfaceIsIUnknown ),
        Guid( "79b56888-7fea-4690-b45d-38fd3c7849be" )
    ]
    public interface IBDATopology
    {
        /// <summary>
        /// Ermittelt die einzelnen Hardwaregeräte, die von der Komponente verwendet werden.
        /// </summary>
        /// <param name="count">Anzahl der ermittelten Geräte.</param>
        /// <param name="sizeOfArray">Größe des folgenden Feldes zur Aufnahme der Gerätenummern.</param>
        /// <param name="typeArray">Liste, in der die Gerätenummern abgelegt werden.</param>
        void GetNodeTypes( [In, Out] ref UInt32 count, [In] UInt32 sizeOfArray, [Out, MarshalAs( UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4, SizeParamIndex = 1 )] UInt32[] typeArray );

        /// <summary>
        /// Ermittelt weitergehende Informationen zu den einzelnen Hardwaregeärten innerhalb einer Komponent.
        /// </summary>
        /// <param name="count">Anzahl der ermittelten Informationen.</param>
        /// <param name="sizeOfArray">Die Größe der folgenden Liste.</param>
        /// <param name="infoArray">Liste, die mit den Informationen zu füllen ist.</param>
        void GetNodeDescriptors( [In, Out] ref UInt32 count, [In] UInt32 sizeOfArray, [Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 1 )] NodeDescriptor[] infoArray );

        /// <summary>
        /// Ermittelt alle Schnittstellen, die ein Hardwaregerät anbietet.
        /// </summary>
        /// <param name="nodeType">Die Kennung des betroffenen Gerätes.</param>
        /// <param name="count">Meldet die Anzahl der gefundenen Geräte.</param>
        /// <param name="sizeOfArray">Die Größe der folgenden Liste.</param>
        /// <param name="typeArray">Eine Liste zur Aufnahme der eindeutigen Kennungen der unterstützen Schnittstellen.</param>
        void GetNodeInterfaces( [In] UInt32 nodeType, [In, Out] ref UInt32 count, [In] UInt32 sizeOfArray, [Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 1 )] Guid[] typeArray );

        /// <summary>
        /// Ermittel die Arten aller Anschlussstellen.
        /// </summary>
        /// <param name="count">Die Anzahl der vorhandenen Anschlussstellen.</param>
        /// <param name="sizeOfArray">Die Größe der folgenden Liste.</param>
        /// <param name="typeArray">Diese Liste wird mit den Kennungen aller Anschlussstellen gefüllt.</param>
        void GetPinTypes( [In, Out] ref UInt32 count, [In] UInt32 sizeOfArray, [Out, MarshalAs( UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4, SizeParamIndex = 1 )] UInt32[] typeArray );

        /// <summary>
        /// Ermittelt alle Verbindungen zwischen Anschlüssen der Hardwaregeräte.
        /// </summary>
        /// <param name="count">Die Anzahl der ermittelten Verbindungen.</param>
        /// <param name="sizeOfArray">Die Größe der folgenden Liste.</param>
        /// <param name="infoArray">Liste, in der alle ermittelten Verbindungen eingetragen werden.</param>
        void GetTemplateConnections( [In, Out] ref UInt32 count, [In] UInt32 sizeOfArray, [Out, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 1 )] TemplateConnection[] infoArray );

        /// <summary>
        /// Legt einen neuen Anschluss an.
        /// </summary>
        /// <param name="pinType">Die Art des Anschlusses.</param>
        /// <param name="pinIdentifier">Die laufende Nummer des Anschlusses.</param>
        void CreatePin( UInt32 pinType, ref UInt32 pinIdentifier );

        /// <summary>
        /// Entfernt einen Anschluss.
        /// </summary>
        /// <param name="pinIdentifier">Die laufende Nummer des betroffenen Anschlusses.</param>
        void DeletePin( UInt32 pinIdentifier );

        /// <summary>
        /// Legt das Datenformat eines Anschlusses fest.
        /// </summary>
        /// <param name="pinIdentifier">Die laufende Nummer des betroffenen Anschlusses.</param>
        /// <param name="mediaType">Das an diesem Anschluss zu verwendende Datenformat.</param>
        void SetMediaType( UInt32 pinIdentifier, ref RawMediaType mediaType );

        /// <summary>
        /// Setzt Details zu einem Anschluss.
        /// </summary>
        /// <param name="pinIdentifier">Die laufende Nummer des betroffenen Anschlusses.</param>
        /// <param name="medium">Die Detailinformationen zum Anschluss</param>
        void SetMedium( UInt32 pinIdentifier, ref RegPinMedium medium );

        /// <summary>
        /// Erzeugt eine Verbindung zwischen zwei Anschlüssen.
        /// </summary>
        /// <param name="inputPinIdentifier">Die laufende Nummer des Quellanschlusses.</param>
        /// <param name="outputPinIdentifier">Die laufende Nummer des Zielanschlusses.</param>
        void CreateTopology( UInt32 inputPinIdentifier, UInt32 outputPinIdentifier );

        /// <summary>
        /// Ermittelt eine Steuerschnittstelle.
        /// </summary>
        /// <param name="inputPinIdentifier">Die laufende Nummer des Quellanschlusses.</param>
        /// <param name="outputPinIdentifier">Die laufende Nummer des Zielanschlusses.</param>
        /// <param name="nodeType">Die laufende Nummer des Hardwaregerätes.</param>
        /// <returns>Die gewünschte Schnittstelle.</returns>
        [return: MarshalAs( UnmanagedType.Interface )]
        object GetControlNode( UInt32 inputPinIdentifier, UInt32 outputPinIdentifier, UInt32 nodeType );
    }
}
