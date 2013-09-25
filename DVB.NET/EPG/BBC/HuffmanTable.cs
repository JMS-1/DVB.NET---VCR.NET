using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace JMS.DVB.EPG.BBC
{
    /// <summary>
    /// Diese Klasse repräsentiert die Codierungsvorschrift für eine zweistufige ASCII
    /// Huffmann Tabelle. Sie kann als XML Datei gespeichert werden und unterstützt darüber
    /// hinaus die Umwandlung in verschiedene kompaktere Binärformate.
    /// </summary>
    [
        Serializable
    ]
    public class HuffmanPairTable : ICloneable
    {
        /// <summary>
        /// Die Abbildungsvorschriften aller Tabellen.
        /// </summary>
        [XmlElement( "Table" )]
        public readonly List<HuffmanTable> Tables = new List<HuffmanTable>();

        /// <summary>
        /// Beschreibt den Abdeckungsgrad dieser Tabelle. Eine <i>1</i> bedeutet, dass
        /// jede Bitsequenz erkannt und umgesetzt wird.
        /// </summary>
        [XmlAttribute( "coverage" )]
        public double FillFactor { get; set; }

        /// <summary>
        /// Erzeugt eine neue Tabelle.
        /// </summary>
        public HuffmanPairTable()
        {
        }

        /// <summary>
        /// Ermittelt eine Übergangstabelle.
        /// </summary>
        /// <param name="from">Das Ausgangszeichen.</param>
        /// <returns>Die gewünschte Tabelle.</returns>
        [XmlIgnore]
        public HuffmanTable this[char from]
        {
            get
            {
                // Validate
                if ((from < '\x0001') || (from > '\x007f'))
                    if (from != HuffmanItem.StartOrEnd)
                        throw new ArgumentException( from.ToString(), "from" );

                // Attach to the table
                HuffmanTable table = Tables.Find( t => t.Source[0] == from );

                // Found it
                if (null != table)
                    return table;

                // Create a new table
                table = new HuffmanTable { Source = new string( new char[] { from } ) };

                // Remember it
                Tables.Add( table );

                // Report
                return table;
            }
        }

        /// <summary>
        /// Liest oder setzt für einen einzelnen Übergang die Übergangssequenz.
        /// </summary>
        /// <param name="from">Das Zeichen, von dem aus der Übergang ausgeht.</param>
        /// <param name="to">Das Zielzeichen des Übergangs.</param>
        /// <exception cref="ArgumentException">Ein Zeichen oder die Sequenz sind ungültig.</exception>
        [XmlIgnore]
        public string this[char from, char to]
        {
            get
            {
                // Forward
                return this[from][to];
            }
            set
            {
                // Forward
                this[from][to] = value;
            }
        }

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Beschreibung.
        /// </summary>
        /// <returns>Die exakte Kopie.</returns>
        public HuffmanPairTable Clone()
        {
            // Lazy stuff
            using (MemoryStream stream = new MemoryStream())
            {
                // Store
                Save( stream );

                // Reposition
                stream.Seek( 0, SeekOrigin.Begin );

                // Reload
                return Load( stream );
            }
        }

        /// <summary>
        /// Speichert die Beschreibung in der XML Repräsentation.
        /// </summary>
        /// <param name="stream">Das Ziel für die Speicherung.</param>
        public void Save( Stream stream )
        {
            // Sort all tables
            foreach (HuffmanTable table in Tables)
                table.Sort();

            // Sort table list
            Tables.Sort( ( l, r ) => l.Source.CompareTo( r.Source ) );

            // Create serializer
            XmlSerializer serializer = new XmlSerializer( GetType(), "http://jochen-manns.de/DVB.NET/Huffman" );

            // Create settings
            XmlWriterSettings settings = new XmlWriterSettings();

            // Configure settings
            settings.Encoding = Encoding.GetEncoding( 1252 );
            settings.CheckCharacters = false;
            settings.Indent = true;

            // Create writer and process
            using (XmlWriter writer = XmlWriter.Create( stream, settings ))
                serializer.Serialize( writer, this );
        }

        /// <summary>
        /// Speichert die Beschreibung in eine XML Datei.
        /// </summary>
        /// <param name="path">Der volle Pfad zur Datei.</param>
        /// <param name="mode">Die gewünschte Art zum Anlegen der Datei.</param>
        public void Save( string path, FileMode mode )
        {
            // Forward
            using (FileStream stream = new FileStream( path, mode, FileAccess.Write, FileShare.None ))
                Save( stream );
        }

        /// <summary>
        /// Speichert die Beschreibung in eine XML Datei.
        /// </summary>
        /// <param name="path">Der volle Pfad zur Datei.</param>
        public void Save( string path )
        {
            // Forward
            Save( path, FileMode.Create );
        }

        /// <summary>
        /// Lädt eine Beschreibung aus einer XML Datei.
        /// </summary>
        /// <param name="path">Der volle Pfad zu XML Datei.</param>
        /// <returns>Die zur XML Datei gehörende Beschreibung.</returns>
        public static HuffmanPairTable Load( string path )
        {
            // Forward
            using (FileStream stream = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.Read ))
                return Load( stream );
        }

        /// <summary>
        /// Rekonstruiert eine Beschreibung aus der zugehörigen XML Repräsentation.
        /// </summary>
        /// <param name="stream">Die Quelle für die XML Repräsenatation.</param>
        /// <returns>Die neu erzeugte zugehörige Beschreibungsinstanz.</returns>
        public static HuffmanPairTable Load( Stream stream )
        {
            // Create deserializer
            XmlSerializer deserializer = new XmlSerializer( typeof( HuffmanPairTable ), "http://jochen-manns.de/DVB.NET/Huffman" );

            // Create settings
            XmlReaderSettings settings = new XmlReaderSettings();

            // Configure settings
            settings.CheckCharacters = false;

            // Process
            using (XmlReader reader = XmlReader.Create( stream, settings ))
                return (HuffmanPairTable) deserializer.Deserialize( reader );
        }

        /// <summary>
        /// Erzeugt die Binärrepräsentationen dieser Tabelle.
        /// </summary>
        /// <returns>Die gewünschte Repräsentation.</returns>
        public TableCreator CreateBinary()
        {
            // Create new
            TableCreator creator = new TableCreator();

            // Process all
            foreach (HuffmanTable table in Tables)
            {
                // Get the raw code
                char from = (HuffmanItem.StartOrEnd == table.Source[0]) ? '\0' : table.Source[0];

                // Process all
                foreach (HuffmanItem item in table.Items)
                {
                    // Get the raw code
                    char to = (HuffmanItem.StartOrEnd == item.Target[0]) ? '\0' : ((HuffmanItem.PassThrough == item.Target[0]) ? char.MaxValue : item.Target[0]);

                    // Register all
                    foreach (string sequence in item.AllSequences)
                        creator.AddTransition( from, to, HuffmanItem.GetSequencePattern( sequence ), item.Sequence.Length );
                }
            }

            // Report
            return creator;
        }


        /// <summary>
        /// Fügt ein Zeichen in Binärrepresantion an einen <see cref="StringBuilder"/> an.
        /// </summary>
        /// <param name="builder">Die aktuell gesammelte Zeichenkette.</param>
        /// <param name="hex">Das gewünschte neue Zeichen.</param>
        private static void AppendHex( StringBuilder builder, byte hex )
        {
            // Process all
            for (int i = 8; i-- > 0; hex *= 2)
                builder.Append( (0 == (hex & 0x80)) ? '0' : '1' );
        }

        /// <summary>
        /// Ermittelt zu einer Sequence von UTF-8 codierten Zeichen die zugehörige Übergangssequenz.
        /// </summary>
        /// <param name="text">Die Zeichensequenz.</param>
        /// <param name="offset">Das erste auszuwertende Zeichen.</param>
        /// <param name="length">Die Anzahl der auszuwertenden Zeichen.</param>
        /// <param name="good">Meldet die Anzahl der bearbeiteten Zeichen.</param>
        /// <param name="blindEscape">Gesetzt um eine vorhandene Ausnahmesequenz so oft wie möglich zu verwenden.</param>
        /// <returns>Die ermittelte Übergangssequenz.</returns>
        public string GetSequence( byte[] text, int offset, int length, bool blindEscape, out int good )
        {
            // Result
            StringBuilder result = new StringBuilder();

            // Previous item
            char? prev = null;

            // Count processed items
            good = -1;

            // Process all
            while (length-- > 0)
            {
                // Load
                byte next = text[offset++];
                char item = (char) next;

                // Not allowed
                if (next < 1)
                    break;

                // Check for escape mode
                if ((next > 127) && (HuffmanItem.StartOrEnd != item))
                {
                    // Not allowed at the very beginning
                    if (!prev.HasValue)
                        break;

                    // Check out the escape sequence
                    string escape = this[prev.Value, HuffmanItem.PassThrough];
                    if (string.IsNullOrEmpty( escape ))
                        break;

                    // Enter the escape sequence and the current character
                    result.Append( escape );
                    AppendHex( result, next );

                    // Count it
                    ++good;

                    // Repeat
                    while (length-- > 0)
                    {
                        // Load
                        next = text[offset++];
                        item = (char) next;

                        // Forbidden
                        if (next < 1)
                            break;

                        // Always add
                        AppendHex( result, next );

                        // Count it
                        ++good;

                        // Done
                        if (next <= 127)
                            break;
                    }

                    // Forbidden
                    if (next < 1)
                        break;
                }
                else
                {
                    // Not for the first byte
                    if (prev.HasValue)
                    {
                        // Get the sequence
                        string sequence = this[prev.Value, item];

                        // Done
                        if (string.IsNullOrEmpty( sequence ))
                        {
                            // See if the current target is a confirmed escape character
                            HuffmanTable table = this[prev.Value];

                            // Respect client settings
                            if (!blindEscape)
                            {
                                // No confirmed escapes at all
                                if (null == table.ConfirmedEscapes)
                                    break;

                                // Not an confirmed escape
                                if (!table.ConfirmedEscapes.Contains( item ))
                                    break;
                            }

                            // Read the escape sequence
                            sequence = table[HuffmanItem.PassThrough];
                            if (string.IsNullOrEmpty( sequence ))
                                break;

                            // Did it all
                            if (item == HuffmanItem.StartOrEnd)
                            {
                                // Append escape
                                result.Append( sequence );

                                // Append terminator
                                result.Append( "00000000" );

                                // Count it
                                ++good;

                                // Done
                                break;
                            }

                            // Create new sequence
                            StringBuilder escape = new StringBuilder( sequence );

                            // Append the item
                            AppendHex( escape, (byte) item );

                            // Use it
                            sequence = escape.ToString();
                        }

                        // Append
                        result.Append( sequence );
                    }

                    // Count it
                    ++good;
                }

                // Just remember for next step
                prev = item;
            }

            // Report
            return result.ToString();
        }

        #region ICloneable Members

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Beschreibung.
        /// </summary>
        /// <returns>Die exakte Kopie.</returns>
        object ICloneable.Clone()
        {
            // Forward
            return Clone();
        }

        #endregion
    }

    /// <summary>
    /// Mit dieser Klasse wird die Abbildungsvorschrift für ASCII Zeichen (Zeichencodes
    /// 1 bis 127) beschrieben.
    /// </summary>
    [
        Serializable
    ]
    public class HuffmanTable
    {
        /// <summary>
        /// Das Zeichen, für das diese Übergangstabelle gültig ist.
        /// </summary>
        [XmlAttribute( "from" )]
        public string Source { get; set; }

        /// <summary>
        /// Die Übergangssequenzen für diverse Zeichen.
        /// </summary>
        [XmlElement( "Sequence" )]
        public readonly List<HuffmanItem> Items = new List<HuffmanItem>();

        /// <summary>
        /// Beschreibt den Abdeckungsgrad dieser Tabelle. Eine <i>1</i> bedeutet, dass
        /// jede Bitsequenz erkannt und umgesetzt wird.
        /// </summary>
        [XmlAttribute( "coverage" )]
        public double FillFactor { get; set; }

        /// <summary>
        /// Bezeichnet alle Zeichen, die in einer Fluchtsequenz verwendet werden dürfen.
        /// </summary>
        [XmlAttribute( "confirmedEscapes" )]
        public string ConfirmedEscapes { get; set; }

        /// <summary>
        /// Erzeugt eine neue Tabelle.
        /// </summary>
        public HuffmanTable()
        {
        }

        /// <summary>
        /// Ermittelt die Informationen zu einem Übergang.
        /// </summary>
        /// <param name="target">Der gewünschte Übergang.</param>
        /// <returns>Die Information zum Übergang oder <i>null</i>, wenn noch
        /// keiner angelegt ist.</returns>
        /// <exception cref="ArgumentException">Die Übergangssequenz ist ungültig.</exception>
        public HuffmanItem FindItem( char target )
        {
            // Validate
            if ((target < '\x0001') || (target > '\x007f'))
                if ((target != HuffmanItem.StartOrEnd) && (target != HuffmanItem.PassThrough))
                    throw new ArgumentException( target.ToString(), "target" );

            // Find the item
            return Items.Find( i => i.Target[0] == target );
        }

        /// <summary>
        /// Liest oder setzt für einen einzelnen Übergang die Übergangssequenz.
        /// </summary>
        /// <param name="target">Das Ziel des Übergangs.</param>
        /// <exception cref="ArgumentException">Das Ziel oder die Übergangssequenz
        /// sind ungültig.</exception>
        [XmlIgnore]
        public string this[char target]
        {
            get
            {
                // Find the item
                HuffmanItem item = FindItem( target );

                // Report
                return (null == item) ? null : item.Sequence;
            }
            set
            {
                // Validate
                if ((target < '\x0001') || (target > '\x007f'))
                    if ((target != HuffmanItem.StartOrEnd) && (target != HuffmanItem.PassThrough))
                        throw new ArgumentException( target.ToString(), "target" );

                // Check mode
                if (null == value)
                {
                    // Simply remove
                    Items.RemoveAll( i => i.Target[0] == target );
                }
                else
                {
                    // Deep validation
                    ValidateSequence( value );

                    // Find the item
                    HuffmanItem item = Items.Find( i => i.Target[0] == target );

                    // Check mode
                    if (null != item)
                    {
                        // Just update
                        item.Sequence = value;
                    }
                    else
                    {
                        // Create new and add to list
                        Items.Add( new HuffmanItem { Target = new string( new char[] { target } ), Sequence = value } );
                    }
                }
            }
        }

        /// <summary>
        /// Ordnet die Übergangssequenzen nach aufsteigender Länge an.
        /// </summary>
        internal void Sort()
        {
            // Process
            Items.Sort( ( l, r ) => l.CompareTo( r ) );
        }

        /// <summary>
        /// Prüft, ob eine Übergangssequenz ein gültiges Format besitzt.
        /// </summary>
        /// <param name="sequence">Die zu prüfende Sequenz.</param>
        /// <exception cref="ArgumentException">Die Sequenz ist ungültig.</exception>
        public static void ValidateSequence( string sequence )
        {
            // Validate all
            if (string.IsNullOrEmpty( sequence ))
                throw new ArgumentException( sequence, "sequence" );

            // Validate bits
            foreach (char test in sequence)
                if ((test != '0') && (test != '1'))
                    throw new ArgumentException( sequence, "sequence" );
        }
    }

    /// <summary>
    /// Hier ist die Komprimierungsvorschrift für ein einzelnes Zeichen festgehalten.
    /// </summary>
    [
        Serializable
    ]
    public class HuffmanItem
    {
        /// <summary>
        /// Dieses Zeichen markiert den Beginn oder das Ende einer Sequenz.
        /// </summary>
        public const char StartOrEnd = '\x00a6';

        /// <summary>
        /// Mit diesem Zeichen wird eine unkomprimierte Sequenz eingeleitet.
        /// </summary>
        public const char PassThrough = '\x00bb';

        /// <summary>
        /// Das Zeichen, für das die Huffmann Komprimierungssequenz definiert wird. 
        /// Erlaubt sind die Zeichencodes 1 bis 127.
        /// </summary>
        [XmlAttribute( "to" )]
        public string Target { get; set; }

        /// <summary>
        /// Die Komprimierungssequenz für das betroffene Zeichen. Sie besteht aus
        /// den Ziffern 0 und 1 und hat eine beliebige Länge.
        /// </summary>
        [XmlText]
        public string Sequence { get; set; }

        /// <summary>
        /// Eine Liste alternativer Komprimierungssequenzen.
        /// </summary>
        [XmlAttribute( "also" )]
        public string AlternateSequences { get; set; }

        /// <summary>
        /// Erzeugt eine neue Vorschrift.
        /// </summary>
        public HuffmanItem()
        {
        }

        /// <summary>
        /// Vergleicht die Übergangssequenzen erst nach Länge und dann nach Inhalt.
        /// </summary>
        /// <param name="other">Die andere Übergangsinstanz.</param>
        /// <returns>Ein Vergleichswert.</returns>
        internal int CompareTo( HuffmanItem other )
        {
            // Length
            int cmp = Sequence.Length.CompareTo( other.Sequence.Length );

            // No match
            if (0 != cmp)
                return cmp;

            // Use sequences themselves
            int delta = Sequence.CompareTo( other.Sequence );

            // Process
            if (0 != delta)
                return delta;
            else
                return string.Compare( AlternateSequences, other.AlternateSequences );
        }

        /// <summary>
        /// Meldet alle Übergangssequenzen
        /// </summary>
        [XmlIgnore]
        public IEnumerable<string> AllSequences
        {
            get
            {
                // Main
                if (!string.IsNullOrEmpty( Sequence ))
                    yield return Sequence;

                // Alternates
                if (!string.IsNullOrEmpty( AlternateSequences ))
                    foreach (string sequence in AlternateSequences.Split( ';' ))
                        yield return sequence;
            }
        }

        /// <summary>
        /// Ergänzt eine alternative Auflösungssequenz.
        /// </summary>
        /// <param name="sequence">Die neue Sequenz.</param>
        public void AddAlternateSequence( string sequence )
        {
            // Validate
            HuffmanTable.ValidateSequence( sequence );

            // All of it
            Dictionary<string, string> sequences = new Dictionary<string, string>();

            // Load
            if (!string.IsNullOrEmpty( AlternateSequences ))
                sequences = AlternateSequences.Split( ';' ).ToDictionary( s => s );

            // Add the new one
            sequences[sequence] = sequence;

            // Push back
            AlternateSequences = string.Join( ";", sequences.Keys.ToArray() );
        }

        /// <summary>
        /// Ermittelt die Binärdarstellung zu einem Übergang.
        /// </summary>
        /// <param name="sequence">Der gewünschte Übergang.</param>
        /// <returns>Die zugehörige Binärdarstellung.</returns>
        public static uint GetSequencePattern( string sequence )
        {
            // Reset
            uint pattern = 0;

            // Fill
            for (int i = sequence.Length; i-- > 0; )
            {
                // Adjust pattern
                pattern >>= 1;

                // Merge in
                if ('1' == sequence[i])
                    pattern |= 0x80000000;
            }

            // Report
            return pattern;
        }

        /// <summary>
        /// Erzeugt eine binäre Darstellung aus der Übergangssequenz.
        /// </summary>
        [XmlIgnore]
        public uint SequencePattern
        {
            get
            {
                // Forward
                return GetSequencePattern( Sequence );
            }
        }
    }
}
