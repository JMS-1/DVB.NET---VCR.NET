using System;
using System.Xml.Serialization;

namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt eine Gruppe von Quellen, die über Antenne empfangen werden.
    /// </summary>
    [Serializable]
    public class TerrestrialGroup : SourceGroup
    {
        /// <summary>
        /// Die Bandbreite des Signals.
        /// </summary>
        public Bandwidths Bandwidth { get; set; }

        /// <summary>
        /// Erzeugt eine neue Gruppe von Quellen.
        /// </summary>
        public TerrestrialGroup()
        {
            // Set up
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

                // No bandwith
                if (Bandwidths.NotDefined == Bandwidth)
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
            return Frequency.GetHashCode() ^ Bandwidth.GetHashCode();
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
            var other = (TerrestrialGroup) group;

            // Most groups can be uniquely identified by the frequency
            if (Frequency != other.Frequency)
                return false;

            // See if full check is required
            if (!legacy)
            {
                // Not so probable
                if (Bandwidth != other.Bandwidth)
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
            var other = obj as TerrestrialGroup;

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
            return string.Format( "DVB-T,{0},{1}", Frequency, Bandwidth );
        }

        /// <summary>
        /// Versucht, die Textdarstellung einer Gruppe von Quellen in eine
        /// Gruppeninstanz umzuwandeln.
        /// </summary>
        /// <param name="text">Die Textdarstellung, wie über <see cref="ToString"/>
        /// erzeugt.</param>
        /// <param name="group">Die zugehörige Instanz.</param>
        /// <returns>Gesetzt, wenn eine Umwandlung möglich war.</returns>
        public static bool TryParse( string text, out TerrestrialGroup group )
        {
            // Reset
            group = null;

            // None
            if (string.IsNullOrEmpty( text ))
                return false;

            // Split
            string[] parts = text.Split( ',' );
            if (3 != parts.Length)
                return false;

            // Check type
            if (!parts[0].Trim().Equals( "DVB-T" ))
                return false;

            // Read frequency
            uint frequency;
            if (!uint.TryParse( parts[1].Trim(), out frequency ))
                return false;

            // Bandwidth
            Bandwidths bandwidth;
            try
            {
                // Load
                bandwidth = (Bandwidths) Enum.Parse( typeof( Bandwidths ), parts[2].Trim() );
            }
            catch (FormatException)
            {
                // Not valid
                return false;
            }

            // Just create
            group = new TerrestrialGroup
                {
                    Frequency = frequency,
                    Bandwidth = bandwidth
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
        public static TerrestrialGroup Parse( string text )
        {
            // Helper
            TerrestrialGroup group;

            // Try it
            if (!TryParse( text, out group ))
                throw new FormatException( text );

            // Report
            return group;
        }
    }
}
