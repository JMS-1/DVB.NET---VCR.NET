using System;
using System.Xml;
using System.Xml.Serialization;


namespace JMS.DVB.DirectShow.RawDevices
{
    /// <summary>
    /// Beschreibt einen einzelnen konfigurierten Tastendruck.
    /// </summary>
    [Serializable]
    public class RCSetting
    {
        /// <summary>
        /// Der Code, der von der Fernbedienung gesendet wird.
        /// </summary>
        [XmlText]
        public ushort RCKey { get; set; }

        /// <summary>
        /// Die Umsetzung des Codes in einen Befehl.
        /// </summary>
        [XmlAttribute( "id" )]
        public InputKey Meaning { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public RCSetting()
        {
        }
    }
}
