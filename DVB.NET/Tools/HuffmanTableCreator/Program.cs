using System;
using System.IO;
using System.Text;
using System.Globalization;

using JMS.DVB;
using JMS.DVB.EPG.BBC;


namespace HuffmanTableCreator
{
    /// <summary>
    /// Dieses Programm erzeugt eine interne Repräsentation für eine auf <i>Huffmann</i> Tabellenstruktur,
    /// bei der für jedes Zeichen eine eigene Tabelle existiert.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Installiert die Laufzeitumgebung.
        /// </summary>
        static Program()
        {
            // Activate dynamic loading
            RunTimeLoader.Startup();
        }

        /// <summary>
        /// Signatur einer Methode zur Auswertung einer Eingangszeile.
        /// </summary>
        /// <param name="table">Die Repräsentation der Tabelle zur Ablage als XML Datei.</param>
        /// <param name="line">Die aktuelle Zeile.</param>
        private delegate void ProcessLineHandler( HuffmanPairTable table, string line );

        /// <summary>
        /// Führt die Umsetzung für eine beliebige Anzahl von Dateien aus.
        /// </summary>
        /// <param name="args">Die Liste der gewünschten Dateien.</param>
        public static void Main( string[] args )
        {
            // Proces all
            foreach (string path in args)
                try
                {
                    // Run one file
                    ProcessFile( path );
                }
                catch (Exception e)
                {
                    // Report
                    Console.WriteLine( "ERROR: {0}", e.Message );
                }

            // Done
            Console.WriteLine( "Done" );
            Console.ReadLine();
        }

        /// <summary>
        /// Wandelt eine Texteingabedatei in eine interne Repräsentation um.
        /// </summary>
        /// <param name="path">Der volle Pfad zur Datei.</param>
        private static void ProcessFile( string path )
        {
            // See if this is a table
            bool isTable = (0 == string.Compare( Path.GetExtension( path ), ".xht", true ));

            // The huffman pair table
            HuffmanPairTable pairTable;

            // Check mode
            if (isTable)
            {
                // Load the table
                pairTable = HuffmanPairTable.Load( path );
            }
            else
            {
                // Create the huffman pair table
                pairTable = new HuffmanPairTable();

                // Open the file for line wiese reading
                using (StreamReader reader = new StreamReader( path, Encoding.GetEncoding( 1252 ) ))
                {
                    // Read the format
                    string format = reader.ReadLine();

                    // Create the delegate
                    ProcessLineHandler processor = (ProcessLineHandler) Delegate.CreateDelegate( typeof( ProcessLineHandler ), typeof( Program ), format );

                    // Process all lines
                    for (string line; null != (line = reader.ReadLine()); )
                    {
                        // Process the line
                        processor( pairTable, line );
                    }
                }
            }

            // Create the binary representation
            TableCreator table = pairTable.CreateBinary();

            // Core name
            string targetName = Path.GetFileNameWithoutExtension( path );

            // Report
            Console.WriteLine( "{0} ({1} Bytes): Lookup={2} Linked={3} [{6} Patterns total] Dead={4} Unsupported={5}", targetName, table.GetTable().Length, table.DirectTables, table.LinkedTables, table.DeadEnds, table.CharacterNotSupported, table.LinkedItems );

            // Load the table
            uint[] encoderTable = table.CreateEncoderTable();

            // See if we created the table
            if (!isTable)
            {
                // Overall usage
                double usage = 0.0;

                // Process all 
                for (int i = 127 + 1; i-- > 0; )
                {
                    // Attach to the table index
                    int index = i * (1 + 127 + 1) * 2;

                    // Overall weight
                    long weight = 0;

                    // Process all
                    for (int j = 1 + 127 + 1; j-- > 0; )
                    {
                        // Load items
                        uint width = encoderTable[index++];
                        uint pattern = encoderTable[index++];

                        // Check it
                        if (width > 0)
                            weight += ((long) 1) << (int) (32 - width);
                    }

                    // Get the private usage 
                    double privUsage = weight * 1.0 / (((long) 1) << 32);

                    // Report
                    pairTable[(0 == i) ? HuffmanItem.StartOrEnd : (char) (i - 1 + 1)].FillFactor = privUsage;

                    // Sum up
                    usage += privUsage;
                }

                // Correct
                usage /= (127 + 1);

                // Report usage
                pairTable.FillFactor = usage;

                // Save XML representation
                pairTable.Save( Path.ChangeExtension( path, ".xht" ) );
            }
        }

        /// <summary>
        /// Wertet eine Eingangszeile aus.
        /// </summary>
        /// <param name="table">Die Repräsentation der Tabelle zur Ablage als XML Datei.</param>
        /// <param name="line">Die aktuelle Eingabezeile.</param>
        private static void FormatCPP1( HuffmanPairTable table, string line )
        {
            // Remember
            string originalLine = line;

            // Prepare
            line = line.Replace( "','", "COMMA" ).Replace( "'{'", "OPEN_BRACE" ).Replace( "'}'", "CLOSE_BRACE" );

            // Split
            string[] parts = line.Trim().Split( new char[] { '{', ',', '}' }, StringSplitOptions.RemoveEmptyEntries );

            // Test hex number
            string hex = parts[1].Trim();
            if (!hex.StartsWith( "0x" ))
                throw new ArgumentException( hex, "line" );

            // Get all
            char from = FromQuoted( parts[0].Trim(), "START" );
            char to = FromQuoted( parts[3].Trim(), "STOP" );
            uint pattern = uint.Parse( hex.Substring( 2 ), NumberStyles.HexNumber ), test = pattern;
            int bits = int.Parse( parts[2].Trim() );

            // Create helper
            char[] sequence = new char[bits];

            // Fill helper
            for (int n = 0; n < bits; test <<= 1)
            {
                // Remember
                sequence[n++] = (0 == (test & 0x80000000)) ? '0' : '1';
            }

            // Check for duplicates
            if (null != table[from, to])
                table[from].FindItem( to ).AddAlternateSequence( new string( sequence ) );
            else
                table[from, to] = new string( sequence );
        }

        /// <summary>
        /// Wandelt ein Textdarstellung eines Zeichens in das Zeichen um.
        /// </summary>
        /// <param name="quoted">Die Textdarstellung.</param>
        /// <param name="special">Ein Spezialcode für Sonderzwecke.</param>
        /// <returns>Das zugehörige Zeichen.</returns>
        private static char FromQuoted( string quoted, string special )
        {
            // Special
            if (quoted == special)
                return HuffmanItem.StartOrEnd;

            // Special
            switch (quoted)
            {
                case "COMMA": return ',';
                case "OPEN_BRACE": return '{';
                case "CLOSE_BRACE": return '}';
                case "ESCAPE": return HuffmanItem.PassThrough;
                case @"'\''": return '\'';
                case @"'\\'": return '\\';
            }

            // Check
            if (3 == quoted.Length)
                if (quoted.StartsWith( "'" ))
                    if (quoted.EndsWith( "'" ))
                        return quoted[1];

            // Special
            if (quoted.StartsWith( "0x" ))
                return (char) int.Parse( quoted.Substring( 2 ), NumberStyles.HexNumber );

            // In error
            throw new ArgumentException( quoted, "quoted" );
        }
    }
}
