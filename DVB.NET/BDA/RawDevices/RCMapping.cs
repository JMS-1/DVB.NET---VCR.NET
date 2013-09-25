using System;
using System.Xml.Serialization;


namespace JMS.DVB.DirectShow.RawDevices
{
    /// <summary>
    /// Beschreibt eine Information von der Fernbedienung.
    /// </summary>
    [Serializable, XmlType( "RC" )]
    public class RCMapping : MappingItem
    {
        /// <summary>
        /// Der Code, der von der Fernbedienung gesendet wird.
        /// </summary>
        [XmlIgnore]
        public ushort RCKey
        {
            get
            {
                // Load
                return ushort.Parse( Value );
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
                return RCKey;
            }
        }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public RCMapping()
        {
        }
    }
}
