using System;
using System.Windows.Forms;
using System.Xml.Serialization;


namespace JMS.DVB.DirectShow.RawDevices
{
    /// <summary>
    /// Beschreibt eine Eingabe über die Tastatur.
    /// </summary>
    [Serializable, XmlType( "Key" )]
    public class KeyMapping : MappingItem
    {
        /// <summary>
        /// Die verwendete Taste.
        /// </summary>
        [XmlIgnore]
        public Keys Key
        {
            get
            {
                // Reconstruct
                return (Keys) Enum.Parse( typeof( Keys ), Value );
            }
            set
            {
                // Remember
                Value = value.ToString();
            }
        }

        /// <summary>
        /// Ein Wert, wie er für Vergleiche genutzt werden kann.
        /// </summary>
        protected override int NumericValue
        {
            get
            {
                // Report
                return (int) Key;
            }
        }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public KeyMapping()
        {
        }
    }
}
