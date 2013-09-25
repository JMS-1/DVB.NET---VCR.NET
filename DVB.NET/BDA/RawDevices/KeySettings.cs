using System;
using System.Xml;
using System.Windows.Forms;
using System.Xml.Serialization;


namespace JMS.DVB.DirectShow.RawDevices
{
    /// <summary>
    /// Beschreibt die Abbildung einer Taste auf einen Befehl.
    /// </summary>
    [Serializable]
    public class KeySetting
    {
        /// <summary>
        /// Die verwendete Taste.
        /// </summary>
        [XmlText]
        public Keys Key { get; set; }

        /// <summary>
        /// Die Umsetzung des Codes in einen Befehl.
        /// </summary>
        [XmlAttribute( "id" )]
        public InputKey Meaning { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public KeySetting()
        {
        }
    }
}
