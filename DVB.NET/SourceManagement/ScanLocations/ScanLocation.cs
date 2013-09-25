using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt einen möglichen Ursprung für den Sendersuchlauf.
    /// </summary>
    [Serializable]
    public abstract class ScanLocation
    {
        /// <summary>
        /// Der Anzeigename für den Anwender zur Auswahl.
        /// </summary>
        [XmlAttribute( "name" )]
        public string DisplayName { get; set; }

        /// <summary>
        /// Meldet oder legt fest, ob dieser Ursprung automatisch generiert oder bereits
        /// für DVB.NET 4.0 manuell nachbearbeitet wurde.
        /// </summary>
        [XmlAttribute( "generated" )]
        public bool AutoConvert { get; set; }

        /// <summary>
        /// Erzeugt einen neuen Ursprung.
        /// </summary>
        internal ScanLocation()
        {
        }

        /// <summary>
        /// Meldet einen eindeutigen Namen dieses Ursprungs im jeweiligen
        /// Kontext (DVB-S, DVB-C oder DVB-T).
        /// </summary>
        [XmlIgnore]
        public virtual string UniqueName
        {
            get
            {
                // Use unique name
                return DisplayName;
            }
        }

        /// <summary>
        /// Meldet die Konfiguration dieses Ursprungs.
        /// </summary>
        [XmlIgnore]
        public abstract IList Groups { get; }
    }

    /// <summary>
    /// Beschreibt einen möglichen Ursprung für den Sendersuchlauf
    /// für eine bestimmte Empfangsart.
    /// </summary>
    /// <typeparam name="T">Die Art der Quellgruppen (Transponder).</typeparam>
    [Serializable]
    public abstract class ScanLocation<T> : ScanLocation where T : SourceGroup
    {
        /// <summary>
        /// Alle Quellgruppen, die bei der Suche zu berücksichtigen sind.
        /// </summary>
        public readonly List<T> SourceGroups = new List<T>();

        /// <summary>
        /// Initialisiert eine neue Beschreibung.
        /// </summary>
        protected ScanLocation()
        {
        }

        /// <summary>
        /// Meldet die Konfiguration dieses Ursprungs.
        /// </summary>
        [XmlIgnore]
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
    /// Beschreibt einen DVB-S Ursprung für den Sendersuchlauf.
    /// </summary>
    [Serializable]
    [XmlType( "Satellite" )]
    public class SatelliteScanLocation : ScanLocation<SatelliteGroup>
    {
        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public SatelliteScanLocation()
        {
        }

        /// <summary>
        /// Meldet einen eindeutigen Namen dieses DVB-S Ursprungs.
        /// </summary>
        public override string UniqueName
        {
            get
            {
                // Check mode
                if (SourceGroups.Count > 0)
                    return SourceGroups[0].GetOrbitalPosition();
                else
                    return base.UniqueName;
            }
        }

        /// <summary>
        /// Meldet einen Ursprung mit einem bestimmten eindeutigem Namen.
        /// </summary>
        /// <param name="uniqueName">Der gewünschte Name.</param>
        /// <returns>Der Ursprung oder <i>null</i>, wenn kein socher existiert.</returns>
        public static SatelliteScanLocation Find( string uniqueName )
        {
            // Forward
            return ScanLocations.Default.Find<SatelliteScanLocation>( uniqueName );
        }

        /// <summary>
        /// Ermittelt einen bestimmten Ursprung.
        /// </summary>
        /// <param name="predicate">Methode zur Auswahl des Ursprungs.</param>
        /// <returns>Der gewünschte Ursprung oder <i>null</i>, wenn kein Ursprung der gewünschten
        /// Art existiert.</returns>
        public static SatelliteScanLocation Find( Predicate<SatelliteScanLocation> predicate )
        {
            // Forward
            return ScanLocations.Default.Find( predicate );
        }
    }

    /// <summary>
    /// Beschreibt einen DVB-C Ursprung für den Sendersuchlauf.
    /// </summary>
    [Serializable]
    [XmlType( "Cable" )]
    public class CableScanLocation : ScanLocation<CableGroup>
    {
        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public CableScanLocation()
        {
        }

        /// <summary>
        /// Meldet einen Ursprung mit einem bestimmten eindeutigem Namen.
        /// </summary>
        /// <param name="uniqueName">Der gewünschte Name.</param>
        /// <returns>Der Ursprung oder <i>null</i>, wenn kein socher existiert.</returns>
        public static CableScanLocation Find( string uniqueName )
        {
            // Forward
            return ScanLocations.Default.Find<CableScanLocation>( uniqueName );
        }

        /// <summary>
        /// Ermittelt einen bestimmten Ursprung.
        /// </summary>
        /// <param name="predicate">Methode zur Auswahl des Ursprungs.</param>
        /// <returns>Der gewünschte Ursprung oder <i>null</i>, wenn kein Ursprung der gewünschten
        /// Art existiert.</returns>
        public static CableScanLocation Find( Predicate<CableScanLocation> predicate )
        {
            // Forward
            return ScanLocations.Default.Find( predicate );
        }
    }

    /// <summary>
    /// Beschreibt einen DVB-T Ursprung für den Sendersuchlauf.
    /// </summary>
    [Serializable]
    [XmlType( "Terrestrial" )]
    public class TerrestrialScanLocation : ScanLocation<TerrestrialGroup>
    {
        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public TerrestrialScanLocation()
        {
        }

        /// <summary>
        /// Meldet einen Ursprung mit einem bestimmten eindeutigem Namen.
        /// </summary>
        /// <param name="uniqueName">Der gewünschte Name.</param>
        /// <returns>Der Ursprung oder <i>null</i>, wenn kein socher existiert.</returns>
        public static TerrestrialScanLocation Find( string uniqueName )
        {
            // Forward
            return ScanLocations.Default.Find<TerrestrialScanLocation>( uniqueName );
        }

        /// <summary>
        /// Ermittelt einen bestimmten Ursprung.
        /// </summary>
        /// <param name="predicate">Methode zur Auswahl des Ursprungs.</param>
        /// <returns>Der gewünschte Ursprung oder <i>null</i>, wenn kein Ursprung der gewünschten
        /// Art existiert.</returns>
        public static TerrestrialScanLocation Find( Predicate<TerrestrialScanLocation> predicate )
        {
            // Forward
            return ScanLocations.Default.Find( predicate );
        }
    }
}
