using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Topology
{
    /// <summary>
    /// Beschreibt eine einzelne Verbindung zwischen den Anschlusstellen von
    /// Hardwaregeräten innerhalb der DVB BDA Komponente.
    /// </summary>
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct TemplateConnection
    {
        /// <summary>
        /// Zeigt eine externe Verbindung an.
        /// </summary>
        public const UInt32 ExternalNode = UInt32.MaxValue;

        /// <summary>
        /// Das Gerät, von dem die Verbindung ausgeht.
        /// </summary>
        public UInt32 FromNode;

        /// <summary>
        /// Die Anschlussstelle des Gerätes, von dem die Verbindung ausgeht.
        /// </summary>
        public UInt32 FromPin;

        /// <summary>
        /// Das Gerät, an dem die Verbindung endet.
        /// </summary>
        public UInt32 ToNode;

        /// <summary>
        /// Die Anschlussstelle, an der die Verbindung endet.
        /// </summary>
        public UInt32 ToPin;
    }
}
