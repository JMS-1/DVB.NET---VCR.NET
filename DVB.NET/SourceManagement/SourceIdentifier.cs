using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;


namespace JMS.DVB
{
    /// <summary>
    /// Enthält die eindeutige Kennung einer Quelle, i.a. eines Senders.
    /// </summary>
    [Serializable]
    [XmlType( "Source" )]
    public class SourceIdentifier
    {
        /// <summary>
        /// Die eindeutige Nummer des Ursprungsnetzwerks.
        /// </summary>
        [XmlAttribute( "net" )]
        public ushort Network { get; set; }

        /// <summary>
        /// Die eindeutige Nummer des Datenstroms.
        /// </summary>
        [XmlAttribute( "ts" )]
        public ushort TransportStream { get; set; }

        /// <summary>
        /// Die eindeutige Nummer des Dienstes.
        /// </summary>
        [XmlAttribute( "svc" )]
        public ushort Service { get; set; }

        /// <summary>
        /// Erzeugt eine neue eindeutige Kennung.
        /// </summary>
        public SourceIdentifier()
        {
        }

        /// <summary>
        /// Erzeugt eine exakte Kopie einer eindeutigen Kennung.
        /// </summary>
        /// <param name="other">Die Kennung, die kopiert werden soll.</param>
        public SourceIdentifier( SourceIdentifier other )
            : this( other.Network, other.TransportStream, other.Service )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Kennung.
        /// </summary>
        /// <param name="network">Die Kennung des Netzwerk.</param>
        /// <param name="transportStream">Die laufende Nummer des Datenstroms im Netzwerk.</param>
        /// <param name="service">Die eindeutige Dienstkennung im Datenstrom.</param>
        public SourceIdentifier( ushort network, ushort transportStream, ushort service )
        {
            // Remember
            Network = network;
            TransportStream = transportStream;
            Service = service;
        }

        /// <summary>
        /// Meldet eine textuelle Darstellung dieser eindeutigen Kennung.
        /// </summary>
        /// <returns>Eine Textdarstellung zu dieser Kennung, zusammengesetzt
        /// aus den einzelnen Fragementen.</returns>
        public override string ToString()
        {
            // Forward
            return ToString( this );
        }

        /// <summary>
        /// Meldet die textuelle Darstellung einer eindeutigen Kennung.
        /// </summary>
        /// <param name="source">Die gewünschte eindeutige Kennung.</param>
        /// <returns>Eine Textdarstellung, aufgebaut aus den einzelnen Fragmenten.</returns>
        public static string ToString( SourceIdentifier source )
        {
            // Forward
            if (null == source)
                return null;
            else
                return string.Format( "({0}, {1}, {2})", source.Network, source.TransportStream, source.Service );
        }

        /// <summary>
        /// Ermittelt einen Kürzel zu dieser Kennung.
        /// </summary>
        /// <returns>Ein Kürzel, zusammengesesetzt aus den einzelnen Fragmenten
        /// der Kennung.</returns>
        public override int GetHashCode()
        {
            // Create
            return Network.GetHashCode() ^ TransportStream.GetHashCode() ^ Service.GetHashCode();
        }

        /// <summary>
        /// Prüft, ob zwei eindeutige Kennungen semantisch äquivalent sind und
        /// somit die gleiche Quelle bezeichnen.
        /// </summary>
        /// <param name="obj">Eine andere eindeutige Kennung.</param>
        /// <returns>Gesetzt, wenn es sich bei dem Parameter um eine semanantisch
        /// äquivalente Kennung handelt.</returns>
        public override bool Equals( object obj )
        {
            // Check type
            var other = obj as SourceIdentifier;

            // By identity
            if (ReferenceEquals( other, null ))
                return false;
            if (ReferenceEquals( other, this ))
                return true;

            // Check all of it starting with the most variable part
            return (Service == other.Service) && (TransportStream == other.TransportStream) && (Network == other.Network);
        }

        /// <summary>
        /// Versucht eine eindeutige Kennung aus der <see cref="ToString()"/> Textdarstellung
        /// zu rekonstruieren.
        /// </summary>
        /// <param name="text">Die vorliegende Textdarstellung.</param>
        /// <param name="identifier">Die eindeutige Kennung zur Textdarstellung.</param>
        /// <returns>Gesetzt, wenn eine eindeutige Kennung erstellt werden konnte.</returns>
        public static bool TryParse( string text, out SourceIdentifier identifier )
        {
            // Reset
            identifier = null;

            // None
            if (null == text)
                return false;

            // Check it
            Match match = Regex.Match( text, @"^\(([ 0-9]+),([ 0-9]+),([ 0-9]+)\)$" );

            // No match at all
            if (!match.Success)
                return false;

            // Try to read all parts
            ushort net, ts, svc;
            if (!ushort.TryParse( match.Groups[1].Value.Trim(), out net ))
                return false;
            if (!ushort.TryParse( match.Groups[2].Value.Trim(), out ts ))
                return false;
            if (!ushort.TryParse( match.Groups[3].Value.Trim(), out svc ))
                return false;

            // Create
            identifier = new SourceIdentifier { Network = net, TransportStream = ts, Service = svc };

            // Did it
            return true;
        }

        /// <summary>
        /// Wandelt eine <see cref="ToString()"/> Textdarstellung in eine eindeutige Kennung
        /// um.
        /// </summary>
        /// <param name="text">Die Textdarstellung der eindeutigen Kennung.</param>
        /// <returns>Die zugehörige eindeutige Kennung.</returns>
        public static SourceIdentifier Parse( string text )
        {
            // Helper
            SourceIdentifier identifier;

            // Try it
            if (!TryParse( text, out identifier ))
                throw new FormatException( text );

            // Report
            return identifier;
        }

        /// <summary>
        /// Meldet, ob alle DVB Parameter der Kennung <i>0</i> sind.
        /// </summary>
        [XmlIgnore]
        public bool IsEmpty { get { return (Service == 0) && (TransportStream == 0) && (Network == 0); } }

        /// <summary>
        /// Rekonstruiert einen eindeutigen Namen aus den Teilen der DVB Kennung.
        /// </summary>
        /// <returns>Eine Zeichenkette, die diese Quelle bezeichnet.</returns>
        public string ToStringKey()
        {
            // Report
            return string.Format( "{{{0}-{1}-{2}}}", Service, TransportStream, Network );
        }
    }
}
