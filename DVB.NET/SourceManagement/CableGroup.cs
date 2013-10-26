using System;
using System.Xml.Serialization;

namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt eine Gruppe von Quellen, die über Kabel empfangen werden.
    /// </summary>
    [Serializable]
    public class CableGroup : SourceGroup
    {
        /// <summary>
        /// Die Symbolrate der digitalen Übertragung in Symbolen pro Sekunde.
        /// </summary>
        public uint SymbolRate { get; set; }

        /// <summary>
        /// Die Bandbreite des Signals.
        /// </summary>
        public Bandwidths Bandwidth { get; set; }

        /// <summary>
        /// Die verwendete Modulation.
        /// </summary>
        public CableModulations Modulation { get; set; }

        /// <summary>
        /// Die zu verwendende spektrale Inversion.
        /// </summary>
        public SpectrumInversions SpectrumInversion { get; set; }

        /// <summary>
        /// Erzeugt eine neue Gruppe von Quellen.
        /// </summary>
        public CableGroup()
        {
            // Set up
            SpectrumInversion = SpectrumInversions.Auto;
            Modulation = CableModulations.NotDefined;
            Bandwidth = Bandwidths.NotDefined;
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

                // No bandwith
                if (Bandwidths.NotDefined == Bandwidth)
                    return false;

                // No modulation
                if (CableModulations.NotDefined == Modulation)
                    return false;

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
            // Merge all
            return Frequency.GetHashCode() ^ SymbolRate.GetHashCode() ^ SpectrumInversion.GetHashCode() ^ Bandwidth.GetHashCode() ^ Modulation.GetHashCode();
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
            var other = (CableGroup) group;

            // Most groups can be uniquely identified by the frequency
            if (Frequency != other.Frequency)
                return false;

            // Check for near exact comparision
            if (!legacy)
            {
                // Not so probable
                if (Modulation != other.Modulation)
                    return false;

                // The rest is more or less for completeness
                if (SymbolRate != other.SymbolRate)
                    return false;
                if (Bandwidth != other.Bandwidth)
                    return false;
                if (SpectrumInversion != other.SpectrumInversion)
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
            var other = obj as CableGroup;

            // By identity
            if (ReferenceEquals( other, null ))
                return false;
            if (ReferenceEquals( other, this ))
                return true;

            // Forward
            return OnCompare( other, false );
        }

        /// <summary>
        /// Erstellt einen Anzeigenamen für diese Gruppe.
        /// </summary>
        /// <returns>Der zugehörige Anzeigename.</returns>
        public override string ToString()
        {
            // Create
            return string.Format( "DVB-C,{0},{2},{3},{1},{4}", Frequency, SymbolRate, SpectrumInversion, Bandwidth, Modulation );
        }

        /// <summary>
        /// Versucht, die Textdarstellung einer Gruppe von Quellen in eine
        /// Gruppeninstanz umzuwandeln.
        /// </summary>
        /// <param name="text">Die Textdarstellung, wie über <see cref="ToString"/>
        /// erzeugt.</param>
        /// <param name="group">Die zugehörige Instanz.</param>
        /// <returns>Gesetzt, wenn eine Umwandlung möglich war.</returns>
        public static bool TryParse( string text, out CableGroup group )
        {
            // Reset
            group = null;

            // None
            if (string.IsNullOrEmpty( text ))
                return false;

            // Split
            string[] parts = text.Split( ',' );
            if (6 != parts.Length)
                return false;

            // Check type
            if (!parts[0].Trim().Equals( "DVB-C" ))
                return false;

            // Read frequency
            uint frequency;
            if (!uint.TryParse( parts[1].Trim(), out frequency ))
                return false;

            // Inversion
            SpectrumInversions inversion;
            try
            {
                // Load
                inversion = (SpectrumInversions) Enum.Parse( typeof( SpectrumInversions ), parts[2].Trim() );
            }
            catch (FormatException)
            {
                // Not valid
                return false;
            }

            // Bandwidth
            Bandwidths bandwidth;
            try
            {
                // Load
                bandwidth = (Bandwidths) Enum.Parse( typeof( Bandwidths ), parts[3].Trim() );
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

            // Modulation
            CableModulations modulation;
            try
            {
                // Load
                modulation = (CableModulations) Enum.Parse( typeof( CableModulations ), parts[5].Trim() );
            }
            catch (FormatException)
            {
                // Not valid
                return false;
            }

            // Just create
            group = new CableGroup
                {
                    Bandwidth = bandwidth,
                    Frequency = frequency,
                    Modulation = modulation,
                    SpectrumInversion = inversion,
                    SymbolRate = symbolrate
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
        public static CableGroup Parse( string text )
        {
            // Helper
            CableGroup group;

            // Try it
            if (!TryParse( text, out group ))
                throw new FormatException( text );

            // Report
            return group;
        }
    }
}
