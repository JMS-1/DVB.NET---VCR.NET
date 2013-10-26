using System;
using System.Xml.Serialization;

namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt eine Gruppe von Quellen, die über Satellit empfangen werden.
    /// </summary>
    [Serializable]
    public class SatelliteGroup : SourceGroup
    {
        /// <summary>
        /// Die Symbolrate der digitalen Übertragung in Symbolen pro Sekunde.
        /// </summary>
        public uint SymbolRate { get; set; }

        /// <summary>
        /// Beschreibt die Polarisation des verwendeten Signals.
        /// </summary>
        public Polarizations Polarization { get; set; }

        /// <summary>
        /// Beschreibt die verwendete innere Methode zur Fehlerkorrektur.
        /// </summary>
        public InnerForwardErrorCorrectionModes InnerFEC { get; set; }

        /// <summary>
        /// Gesetzt, wenn eine DVB-S2 und keine DVB-S Modulation verwendet wird.
        /// </summary>
        public bool UsesS2Modulation { get; set; }

        /// <summary>
        /// Beschreibt die Modulation des Digitalsignals.
        /// </summary>
        public SatelliteModulations Modulation { get; set; }

        /// <summary>
        /// Der Roll-Off Faktor für DVB-S2 Empfang.
        /// </summary>
        public S2RollOffs RollOff { get; set; }

        /// <summary>
        /// Die Orbitalposition des Satelliten in der Notation mit vier Zeichen (0192 für 19.2°).
        /// </summary>
        public string OrbitalPosition { get; set; }

        /// <summary>
        /// Gesetzt, wenn <see cref="OrbitalPosition"/> eine westliche Position angibt.
        /// </summary>
        public bool IsWestPosition { get; set; }

        /// <summary>
        /// Erzeugt eine neue Gruppe von Quellen.
        /// </summary>
        public SatelliteGroup()
        {
            // Set up
            InnerFEC = InnerForwardErrorCorrectionModes.NotDefined;
            Modulation = SatelliteModulations.NotDefined;
            Polarization = Polarizations.NotDefined;
            RollOff = S2RollOffs.NotDefined;
        }

        /// <summary>
        /// Meldet, ob alle benötigten Parameter gesetzt sind.
        /// </summary>
        public override bool IsComplete
        {
            get
            {
                // No frequency
                if (0 == Frequency)
                    return false;

                // No symbol rate
                if (0 == SymbolRate)
                    return false;

                // No polarisation
                if (Polarizations.NotDefined == Polarization)
                    return false;

                // No modulation
                if (SatelliteModulations.NotDefined == Modulation)
                    return false;

                // Error correction
                if (InnerForwardErrorCorrectionModes.NotDefined == InnerFEC)
                    return false;

                // Special DVB-S2
                if (UsesS2Modulation)
                {
                    // No roll-off
                    if (S2RollOffs.NotDefined == RollOff)
                        return false;
                }

                // Looks good
                return true;
            }
        }

        /// <summary>
        /// Ermittelt ein Kürzel für diese Gruppe.
        /// </summary>
        /// <returns>Das gewünschte Kürzel.</returns>
        public override int GetHashCode()
        {
            // Core
            int hash = (null == OrbitalPosition) ? 0 : OrbitalPosition.GetHashCode();

            // Merge all
            return hash ^ Frequency.GetHashCode() ^ SymbolRate.GetHashCode() ^ Polarization.GetHashCode() ^ InnerFEC.GetHashCode() ^ UsesS2Modulation.GetHashCode() ^ Modulation.GetHashCode() ^ RollOff.GetHashCode() ^ IsWestPosition.GetHashCode();
        }

        /// <summary>
        /// Vergleicht diese Quellgruppe (Transponder) mit einer anderen.
        /// </summary>
        /// <param name="group">Die andere Quellgruppe.</param>
        /// <param name="legacy">Gesetzt, wenn ein partieller Vergleich erfolgen soll.</param>
        /// <returns>Gesetzt, wenn die Gruppen identisch sind.</returns>
        protected override bool OnCompare( SourceGroup group, bool legacy )
        {
            // Change type
            SatelliteGroup other = (SatelliteGroup) group;

            // Most groups can be uniquely identified by the frequency
            if (legacy)
            {
                // Allow shift by 5 MHz
                if (Math.Abs( (long) Frequency - (long) other.Frequency ) > 5000)
                    return false;
            }
            else
            {
                // Exact match
                if (Frequency != other.Frequency)
                    return false;
            }

            // In rare cases both polarisations are needed
            if (Polarization != other.Polarization)
                return false;

            // Location in the sky
            if (!legacy)
            {
                // All of it
                if (IsWestPosition != other.IsWestPosition)
                    return false;
                if (!Equals( (null == OrbitalPosition) ? string.Empty : OrbitalPosition, (null == other.OrbitalPosition) ? string.Empty : other.OrbitalPosition ))
                    return false;
            }

            // The rest is more or less for completeness
            if (UsesS2Modulation != other.UsesS2Modulation)
                return false;

            // See if we should include the modulation
            if (!legacy)
            {
                // All of it
                if (SymbolRate != other.SymbolRate)
                    return false;
                if (Modulation != other.Modulation)
                    return false;
                if (RollOff != other.RollOff)
                    return false;
                if (InnerFEC != other.InnerFEC)
                    return false;
            }

            // Same
            return true;

        }

        /// <summary>
        /// Vergleicht zwei Gruppen.
        /// </summary>
        /// <param name="obj">Die andere Gruppe.</param>
        /// <returns>Gesetzt, wenn die Konfiguration der Gruppen identisch ist.</returns>
        public override bool Equals( object obj )
        {
            // Change type
            var other = obj as SatelliteGroup;

            // By identity
            if (ReferenceEquals( other, null ))
                return false;
            if (ReferenceEquals( other, this ))
                return true;

            // Forward
            return OnCompare( other, false );
        }

        /// <summary>
        /// Meldet eine Zeichenkette für die orbitale Position des Ursprungs zu
        /// dieser Quelle.
        /// </summary>
        /// <returns>Eine Zeichenkette zur orbitalen Position.</returns>
        public string GetOrbitalPosition()
        {
            // Get the orbital position
            string pos;
            if ((null != OrbitalPosition) && (4 == OrbitalPosition.Length))
                pos = string.Format( "{0}.{1}°", OrbitalPosition.Substring( 0, 3 ).TrimStart( '0' ), OrbitalPosition[3] );
            else
                pos = OrbitalPosition;

            // Merge
            return string.Format( "{0}{1}", pos, IsWestPosition ? "W" : "E" );
        }

        /// <summary>
        /// Erstellt einen Anzeigenamen für diese Gruppe.
        /// </summary>
        /// <returns>Der zugehörige Anzeigename.</returns>
        public override string ToString()
        {
            // Create
            return string.Format( "DVB-S{3},{7},{0},{2},{1},{4},{6},{5}", Frequency, SymbolRate, Polarization, UsesS2Modulation ? "2" : string.Empty, InnerFEC, RollOff, Modulation, GetOrbitalPosition() );
        }

        /// <summary>
        /// Versucht, die Textdarstellung einer Gruppe von Quellen in eine
        /// Gruppeninstanz umzuwandeln.
        /// </summary>
        /// <param name="text">Die Textdarstellung, wie über <see cref="ToString"/>
        /// erzeugt.</param>
        /// <param name="group">Die zugehörige Instanz.</param>
        /// <returns>Gesetzt, wenn eine Umwandlung möglich war.</returns>
        public static bool TryParse( string text, out SatelliteGroup group )
        {
            // Reset
            group = null;

            // None
            if (string.IsNullOrEmpty( text ))
                return false;

            // Split
            string[] parts = text.Split( ',' );
            if (8 != parts.Length)
                return false;

            // Modulation type
            bool standardModulation = parts[0].Trim().Equals( "DVB-S" );

            // Validate other
            if (!standardModulation)
                if (!parts[0].Trim().Equals( "DVB-S2" ))
                    return false;

            // Get full
            string orbital = parts[1].Trim();

            // Primary orbital position
            bool west = orbital.EndsWith( "W" );

            // Validate other
            if (!west)
                if (!orbital.EndsWith( "E" ))
                    return false;

            // Cut off
            orbital = orbital.Substring( 0, orbital.Length - 1 );

            // Test for special notation
            if ((orbital.Length >= 4) && (orbital.Length <= 6))
                if ('.' == orbital[orbital.Length - 3])
                    if ('°' == orbital[orbital.Length - 1])
                        orbital = ("00" + orbital.Substring( 0, orbital.Length - 3 ) + orbital.Substring( orbital.Length - 2, 1 )).Substring( orbital.Length - 4, 4 );

            // Read frequency
            uint frequency;
            if (!uint.TryParse( parts[2].Trim(), out frequency ))
                return false;

            // Polarization
            Polarizations polarisation;
            try
            {
                // Load
                polarisation = (Polarizations) Enum.Parse( typeof( Polarizations ), parts[3].Trim() );
            }
            catch (FormatException)
            {
                // Not valid
                return false;
            }

            // Read symbol rate
            uint symbolrate;
            if (!uint.TryParse( parts[4].Trim(), out symbolrate ))
                return false;

            // Error correction
            InnerForwardErrorCorrectionModes correction;
            try
            {
                // Load
                correction = (InnerForwardErrorCorrectionModes) Enum.Parse( typeof( InnerForwardErrorCorrectionModes ), parts[5].Trim() );
            }
            catch (FormatException)
            {
                // Not valid
                return false;
            }

            // Modulation
            SatelliteModulations modulation;
            try
            {
                // Load
                modulation = (SatelliteModulations) Enum.Parse( typeof( SatelliteModulations ), parts[6].Trim() );
            }
            catch (FormatException)
            {
                // Not valid
                return false;
            }

            // Roll-Off
            S2RollOffs rolloff;
            try
            {
                // Load
                rolloff = (S2RollOffs) Enum.Parse( typeof( S2RollOffs ), parts[7].Trim() );
            }
            catch (FormatException)
            {
                // Not valid
                return false;
            }

            // Just create
            group = new SatelliteGroup
                {
                    Frequency = frequency,
                    InnerFEC = correction,
                    IsWestPosition = west,
                    Modulation = modulation,
                    OrbitalPosition = orbital,
                    Polarization = polarisation,
                    RollOff = rolloff,
                    SymbolRate = symbolrate,
                    UsesS2Modulation = !standardModulation
                };

            // Did it
            return true;
        }

        /// <summary>
        /// Wandelt eine Textdarstellung einer Gruppe von Quellen in eine
        /// Gruppeninstanz um.
        /// </summary>
        /// <param name="text">Die Textdarstellung, wie über <see cref="ToString"/>
        /// erzeugt.</param>
        /// <returns>Die rekonstruierte Instanz.</returns>
        public static SatelliteGroup Parse( string text )
        {
            // Helper
            SatelliteGroup group;

            // Try it
            if (!TryParse( text, out group ))
                throw new FormatException( text );

            // Report
            return group;
        }
    }
}
