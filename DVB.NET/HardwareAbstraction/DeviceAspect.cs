using System;
using System.Xml;
using System.Xml.Serialization;


namespace JMS.DVB
{
    /// <summary>
    /// Ein Parameter, der zur Auflösung des Gerätes verwendet werden kann.
    /// </summary>
    [Serializable]
    public class DeviceAspect
    {
        /// <summary>
        /// Der Name des Parameters.
        /// </summary>
        [XmlAttribute( "aspect" )]
        public string Aspekt { get; set; }

        /// <summary>
        /// Der Wert des Parameters.
        /// </summary>
        [XmlText]
        public string Value { get; set; }
    }
}
