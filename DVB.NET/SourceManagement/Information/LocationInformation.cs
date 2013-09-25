using System;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.DVB
{
    /// <summary>
    /// Die Informationen zu einem Ursprung von Gruppen von Quellen, konkret etwa
    /// welche Transponder für eine DiSEq Einstellung verfügbar sind.
    /// </summary>
    [Serializable]
    public abstract class LocationInformation
    {
        /// <summary>
        /// Erzeugt eine neue Informationsinstanz.
        /// </summary>
        internal LocationInformation()
        {
        }

        /// <summary>
        /// Meldet alle Quellgruppen zu diesem Ursprung.
        /// </summary>
        public abstract IList Groups { get; }
    }

    /// <summary>
    /// Hilfsklasse zum Erstellen stark typisierter Informationen zu einem Ursprung.
    /// </summary>
    /// <typeparam name="G"></typeparam>
    [Serializable]
    public abstract class LocationInformation<G> : LocationInformation where G : SourceGroup, new()
    {
        /// <summary>
        /// Der zugehörige Ursprung.
        /// </summary>
        [XmlElement( typeof( SatelliteLocation ) )]
        [XmlElement( typeof( CableLocation ) )]
        [XmlElement( typeof( TerrestrialLocation ) )]
        public GroupLocation<G> Location { get; set; }

        /// <summary>
        /// Die Liste der Quellgruppen in diesem Ursprung.
        /// </summary>
        [XmlElement( typeof( SatelliteGroup ) )]
        [XmlElement( typeof( CableGroup ) )]
        [XmlElement( typeof( TerrestrialGroup ) )]
        public readonly List<G> SourceGroups = new List<G>();

        /// <summary>
        /// Initialisiert eine neue Informationsinstanz.
        /// </summary>
        protected LocationInformation()
        {
        }

        /// <summary>
        /// Meldet alle Quellgruppen zu diesem Ursprung.
        /// </summary>
        public override IList Groups
        {
            get
            {
                // Report
                return SourceGroups;
            }
        }
    }

    /// <summary>
    /// Liefert Informationen zu einem DVB-S(2) Ursprung.
    /// </summary>
    [Serializable]
    public class SatelliteLocationInformation : LocationInformation<SatelliteGroup>
    {
        /// <summary>
        /// Initialisiert eine neue Informationsinstanz.
        /// </summary>
        public SatelliteLocationInformation()
        {
        }

        /// <summary>
        /// Meldet den zugehörigen Ursprung.
        /// </summary>
        public new SatelliteLocation Location
        {
            get
            {
                // Forward
                return (SatelliteLocation) base.Location;
            }
        }
    }

    /// <summary>
    /// Liefert Informationen zu einem DVB-C Ursprung.
    /// </summary>
    [Serializable]
    public class CableLocationInformation : LocationInformation<CableGroup>
    {
        /// <summary>
        /// Initialisiert eine neue Informationsinstanz.
        /// </summary>
        public CableLocationInformation()
        {
        }
    }

    /// <summary>
    /// Liefert Informationen zu einem DVB-T Ursprung.
    /// </summary>
    [Serializable]
    public class TerrestrialLocationInformation : LocationInformation<TerrestrialGroup>
    {
        /// <summary>
        /// Initialisiert eine neue Informationsinstanz.
        /// </summary>
        public TerrestrialLocationInformation()
        {
        }
    }
}
