using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.DVB
{
    /// <summary>
    /// Hilfsklasse zur Referenzierung eines Ursprungs für den Sendersuchlauf.
    /// </summary>
    [Serializable]
    public abstract class ScanTemplate
    {
        /// <summary>
        /// Die Liste der zugehörigen Referenzen in die allgemeine Konfiguration
        /// des Sendersuchlaufs.
        /// </summary>
        [XmlElement( "Scan" )]
        public readonly List<string> ScanLocations = new List<string>();

        /// <summary>
        /// Erzeugt eine neue Referenz.
        /// </summary>
        internal ScanTemplate()
        {
        }

        /// <summary>
        /// Meldet den zugehörigen Ursprung.
        /// </summary>
        [XmlIgnore]
        public abstract GroupLocation GroupLocation { get; }
    }

    /// <summary>
    /// Hilfsklasse zur Referenzierung eines Ursprungs für den Sendersuchlauf.
    /// </summary>
    public class ScanTemplate<T> : ScanTemplate where T : GroupLocation
    {
        /// <summary>
        /// Die Konfiguration des Ursprungs.
        /// </summary>
        public T Location { get; set; }

        /// <summary>
        /// Erzeugt eine neue Referenz.
        /// </summary>
        public ScanTemplate()
        {
        }

        /// <summary>
        /// Meldet den zugehörigen Ursprung.
        /// </summary>
        [XmlIgnore]
        public override GroupLocation GroupLocation
        {
            get
            {
                // Report
                return Location;
            }
        }
    }
}
