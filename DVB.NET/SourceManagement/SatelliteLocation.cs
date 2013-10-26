using System;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt eine DiSEqC LNB Auswahl für den Satellitenempfang.
    /// </summary>
    [Serializable]
    [XmlType( "DiSEqC" )]
    public class SatelliteLocation : GroupLocation<SatelliteGroup>
    {
        /// <summary>
        /// Die Standardwerte für die LNB Frequenzen.
        /// </summary>
        public static readonly SatelliteLocation Defaults = new SatelliteLocation() { Frequency1 = 9750000, Frequency2 = 10600000, SwitchFrequency = 11700000, UsePower = true };

        /// <summary>
        /// Die untere Frequenz in kHz.
        /// </summary>
        public uint Frequency1 { get; set; }

        /// <summary>
        /// Die obere Frequenz in kHz.
        /// </summary>
        public uint Frequency2 { get; set; }

        /// <summary>
        /// Die Wechselfrequenz zur Bandumschaltung in kHz.
        /// </summary>
        public uint SwitchFrequency { get; set; }

        /// <summary>
        /// Gesetzt, wenn eine Steuerspannung benötigt wird.
        /// </summary>
        public bool UsePower { get; set; }

        /// <summary>
        /// Die Auswahl der Antenne.
        /// </summary>
        public DiSEqCLocations LNB { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public SatelliteLocation()
        {
            // Reset all
            LNB = DiSEqCLocations.None;
        }

        /// <summary>
        /// Erzeugt einen Schlüssel zu diesem Ursprung. Der Schlüssel enthält
        /// keine Quellgruppen, ist aber ansonsten identisch mit dieser Instanz.
        /// </summary>
        /// <returns>Ein Schlüssel zu diesem Ursprung.</returns>
        public SatelliteLocation CreateKey()
        {
            // Just create
            return new SatelliteLocation() { LNB = this.LNB, Frequency1 = this.Frequency1, Frequency2 = this.Frequency2, SwitchFrequency = this.SwitchFrequency, UsePower = this.UsePower };
        }

        /// <summary>
        /// Ermittelt ein Kürzel für diesen Ursprung.
        /// </summary>
        /// <returns>Der gewünschte Kürzel.</returns>
        public override int GetHashCode()
        {
            // Report
            return Frequency1.GetHashCode() ^ Frequency2.GetHashCode() ^ SwitchFrequency.GetHashCode() ^ UsePower.GetHashCode() ^ LNB.GetHashCode();
        }

        /// <summary>
        /// Vergleicht zwei Instanzen.
        /// </summary>
        /// <param name="obj">Ein anderer Ursprung.</param>
        /// <returns>Gesetzt, wenn beide Instanzen den gleichen Ursprung beschreiben.</returns>
        public override bool Equals( object obj )
        {
            // Change type
            var other = obj as SatelliteLocation;

            // By identity
            if (ReferenceEquals( other, null ))
                return false;
            if (ReferenceEquals( other, this ))
                return true;

            // If two instance are different than in the selection of the LNB
            if (LNB != other.LNB)
                return false;

            // Standard LNB settings - no difference expected
            if (Frequency1 != other.Frequency1)
                return false;
            if (Frequency2 != other.Frequency2)
                return false;
            if (SwitchFrequency != other.SwitchFrequency)
                return false;
            if (UsePower != other.UsePower)
                return false;

            // Same
            return true;
        }

        /// <summary>
        /// Erzeugt einen Anzeigenamen für diesen Ursprung.
        /// </summary>
        /// <returns>Der gewünschte Anzeigename.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "{4}({0}, {1}, {2}, {3})", Frequency1, Frequency2, SwitchFrequency, UsePower, LNB );
        }

        /// <summary>
        /// Versucht, die Textdarstellung eines Ursprungs in eine umzuwandeln.
        /// </summary>
        /// <param name="text">Die Textdarstellung, wie über <see cref="ToString"/>
        /// erzeugt.</param>
        /// <param name="location">Die zugehörige Instanz.</param>
        /// <returns>Gesetzt, wenn eine Umwandlung möglich war.</returns>
        public static bool TryParse( string text, out SatelliteLocation location )
        {
            // Reset
            location = null;

            // None
            if (string.IsNullOrEmpty( text ))
                return false;

            // Check it
            Match match = Regex.Match( text, @"^([^\(]+)\(([ 0-9]+),([ 0-9]+),([ 0-9]+),([^\)]+)\)$" );

            // No match at all
            if (!match.Success)
                return false;

            // Type
            DiSEqCLocations lnb;
            try
            {
                // Load
                lnb = (DiSEqCLocations) Enum.Parse( typeof( DiSEqCLocations ), match.Groups[1].Value.Trim() );
            }
            catch (FormatException)
            {
                // Not valid
                return false;
            }

            // Read first frequency
            uint frequency1;
            if (!uint.TryParse( match.Groups[2].Value.Trim(), out frequency1 ))
                return false;

            // Read second frequency
            uint frequency2;
            if (!uint.TryParse( match.Groups[3].Value.Trim(), out frequency2 ))
                return false;

            // Read switch frequency
            uint switchFrquency;
            if (!uint.TryParse( match.Groups[4].Value.Trim(), out switchFrquency ))
                return false;

            // Power mode
            bool power;
            if (!bool.TryParse( match.Groups[5].Value.Trim(), out power ))
                return false;

            // Just create
            location = new SatelliteLocation
                {
                    LNB = lnb,
                    Frequency1 = frequency1,
                    Frequency2 = frequency2,
                    SwitchFrequency = switchFrquency,
                    UsePower = power
                };

            // Did it
            return true;
        }

        /// <summary>
        /// Wandelt eine Textdarstellung eines Ursprungs Instanz um.
        /// </summary>
        /// <param name="text">Die Textdarstellung, wie über <see cref="ToString"/>
        /// erzeugt.</param>
        /// <returns>Die rekonstruierte Instanz.</returns>
        public static SatelliteLocation Parse( string text )
        {
            // Helper
            SatelliteLocation location;

            // Try it
            if (!TryParse( text, out location ))
                throw new FormatException( text );

            // Report
            return location;
        }

        /// <summary>
        /// Erzeugt eine Kopie dieses Ursprungs, allerdings ohne die enthaltenen Quellgruppen.
        /// </summary>
        /// <returns>Die Kopie des Ursprungs selbst.</returns>
        public new SatelliteLocation Clone()
        {
            // Forward
            return (SatelliteLocation) CreateClone();
        }
    }
}
