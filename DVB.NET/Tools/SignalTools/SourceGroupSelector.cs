using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using JMS.DVB.Administration.SourceScanner;

namespace JMS.DVB.Administration.Tools
{
    /// <summary>
    /// Mit Hilfe dieser Klasse werden für die unterschiedlichen DVB Empfangsarten
    /// Auswahlklassen erstellt, mit deren Hilfe eine Gruppierung für die Anzeige
    /// erfolgt.
    /// </summary>
    public abstract class SourceGroupSelector
    {
        /// <summary>
        /// Liest oder setzt das zugehörige Anzeigeelement.
        /// </summary>
        public GroupDisplay Display { get; set; }

        /// <summary>
        /// Erzeugt eine neue Auswahlinstanz.
        /// </summary>
        protected SourceGroupSelector()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Auswahlinstanz abhängig von der jeweiligen Empfangsart.
        /// </summary>
        /// <param name="source">Die zu betrachtende Quelle.</param>
        /// <returns>Eine passende Auswahlinstanz.</returns>
        public static SourceGroupSelector Create( SourceSelection source )
        {
            // Attach to the related profile
            var groupType = source.GetProfile().GetGroupType();
            if (groupType == typeof( SatelliteGroup ))
                return new SatelliteGroupSelector( source );
            else if (groupType == typeof( CableGroup ))
                return new CableGroupSelector( source );
            else if (groupType == typeof( TerrestrialGroup ))
                return new TerrestrialGroupSelector( source );
            else
                throw new ArgumentException( groupType.Name, source.ProfileName );
        }

        /// <summary>
        /// Meldet eine relative Nummer einer Quellgruppe bezüglich dieser Auswahl.
        /// </summary>
        /// <param name="group">Die zu prüfende Quellgruppe.</param>
        /// <returns>Eine bei 0 beginnenden laufende Nummer, die in eine Farbcodierung umgesetzt wird.</returns>
        public abstract int GetSubGroupIndex( SourceGroup group );
    }

    /// <summary>
    /// Gruppiert DVB-S Quellgruppen.
    /// </summary>
    public class SatelliteGroupSelector : SourceGroupSelector
    {
        /// <summary>
        /// Der zugehörige Ursprung.
        /// </summary>
        private SatelliteLocation m_Location;

        /// <summary>
        /// Erzeugt eine neue Auswahlinstanz.
        /// </summary>
        /// <param name="source">Die zu betrachtende Quelle.</param>
        public SatelliteGroupSelector( SourceSelection source )
        {
            // Remember
            m_Location = (SatelliteLocation) source.Location;
        }

        /// <summary>
        /// Meldet einen Kürzel für diesen Schlüssel.
        /// </summary>
        /// <returns>Der gewünschte Kürzel.</returns>
        public override int GetHashCode()
        {
            // Just forward
            return m_Location.LNB.GetHashCode();
        }

        /// <summary>
        /// Vergleicht zwei Gruppierungen.
        /// </summary>
        /// <param name="obj">Die andere Gruppierung.</param>
        /// <returns>Gesetzt, wenn beide Gruppierungen identisch sind.</returns>
        public override bool Equals( object obj )
        {
            // Change type
            SatelliteGroupSelector other = obj as SatelliteGroupSelector;

            // Not possible
            if (null == other)
                return false;

            // Ask location
            return (m_Location.LNB == other.m_Location.LNB);
        }

        /// <summary>
        /// Zeigt eine Kurzbezeichnung an.
        /// </summary>
        /// <returns>Die gewünschte Bezeichnung für diese Auswahl.</returns>
        public override string ToString()
        {
            // Forward
            return m_Location.LNB.ToString();
        }

        /// <summary>
        /// Meldet eine relative Nummer einer Quellgruppe bezüglich dieser Auswahl.
        /// </summary>
        /// <param name="group">Die zu prüfende Quellgruppe.</param>
        /// <returns>Eine bei 0 beginnenden laufende Nummer, die in eine Farbcodierung umgesetzt wird.</returns>
        public override int GetSubGroupIndex( SourceGroup group )
        {
            // Convert
            SatelliteGroup sat = (SatelliteGroup) group;

            // Check mode
            switch (sat.Polarization)
            {
                case Polarizations.Horizontal: return 0;
                case Polarizations.Vertical: return 1;
                default: return 2;
            }
        }
    }

    /// <summary>
    /// Gruppiert DVB-C Quellgruppen.
    /// </summary>
    public class CableGroupSelector : SourceGroupSelector
    {
        /// <summary>
        /// Die zugehörige Quellgruppe.
        /// </summary>
        private CableGroup m_Group;

        /// <summary>
        /// Erzeugt eine neue Auswahlinstanz.
        /// </summary>
        /// <param name="source">Die zu betrachtende Quelle.</param>
        public CableGroupSelector( SourceSelection source )
        {
            // Remember
            m_Group = (CableGroup) source.Group;
        }

        /// <summary>
        /// Meldet einen Kürzel für diesen Schlüssel.
        /// </summary>
        /// <returns>Der gewünschte Kürzel.</returns>
        public override int GetHashCode()
        {
            // Just forward
            return 0;
        }

        /// <summary>
        /// Vergleicht zwei Gruppierungen.
        /// </summary>
        /// <param name="obj">Die andere Gruppierung.</param>
        /// <returns>Gesetzt, wenn beide Gruppierungen identisch sind.</returns>
        public override bool Equals( object obj )
        {
            // Change type
            CableGroupSelector other = obj as CableGroupSelector;

            // Not possible
            if (null == other)
                return false;

            // There is only one
            return true;
        }

        /// <summary>
        /// Zeigt eine Kurzbezeichnung an.
        /// </summary>
        /// <returns>Die gewünschte Bezeichnung für diese Auswahl.</returns>
        public override string ToString()
        {
            // Forward
            return "DVB-C";
        }

        /// <summary>
        /// Meldet eine relative Nummer einer Quellgruppe bezüglich dieser Auswahl.
        /// </summary>
        /// <param name="group">Die zu prüfende Quellgruppe.</param>
        /// <returns>Eine bei 0 beginnenden laufende Nummer, die in eine Farbcodierung umgesetzt wird.</returns>
        public override int GetSubGroupIndex( SourceGroup group )
        {
            // Convert
            CableGroup cab = (CableGroup) group;

            // Check mode
            switch (cab.Modulation)
            {
                case CableModulations.QAM64: return 0;
                case CableModulations.QAM256: return 1;
                default: return 2;
            }
        }
    }

    /// <summary>
    /// Gruppiert DVB-T Quellgruppen.
    /// </summary>
    public class TerrestrialGroupSelector : SourceGroupSelector
    {
        /// <summary>
        /// Die zugehörige Quellgruppe.
        /// </summary>
        private TerrestrialGroup m_Group;

        /// <summary>
        /// Erzeugt eine neue Auswahlinstanz.
        /// </summary>
        /// <param name="source">Die zu betrachtende Quelle.</param>
        public TerrestrialGroupSelector( SourceSelection source )
        {
            // Remember
            m_Group = (TerrestrialGroup) source.Group;
        }

        /// <summary>
        /// Meldet einen Kürzel für diesen Schlüssel.
        /// </summary>
        /// <returns>Der gewünschte Kürzel.</returns>
        public override int GetHashCode()
        {
            // Just forward
            return 0;
        }

        /// <summary>
        /// Vergleicht zwei Gruppierungen.
        /// </summary>
        /// <param name="obj">Die andere Gruppierung.</param>
        /// <returns>Gesetzt, wenn beide Gruppierungen identisch sind.</returns>
        public override bool Equals( object obj )
        {
            // Change type
            var other = obj as TerrestrialGroupSelector;
            if (null == other)
                return false;

            // There is only one
            return true;
        }

        /// <summary>
        /// Zeigt eine Kurzbezeichnung an.
        /// </summary>
        /// <returns>Die gewünschte Bezeichnung für diese Auswahl.</returns>
        public override string ToString()
        {
            // Forward
            return "DVB-T";
        }

        /// <summary>
        /// Meldet eine relative Nummer einer Quellgruppe bezüglich dieser Auswahl.
        /// </summary>
        /// <param name="group">Die zu prüfende Quellgruppe.</param>
        /// <returns>Eine bei 0 beginnenden laufende Nummer, die in eine Farbcodierung umgesetzt wird.</returns>
        public override int GetSubGroupIndex( SourceGroup group )
        {
            // Always one
            return 0;
        }
    }
}
