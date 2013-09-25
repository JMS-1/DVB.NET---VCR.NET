using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.EPG.BBC
{
    /// <summary>
    /// Erzeugt die interne Repräsentation einer <i>Huffman</i> Tabelle, bei der für
    /// jedes Zeichen eine individuelle Tabelle verwaltet wird.
    /// </summary>
    public class TableCreator
    {
        /// <summary>
        /// Zeigt an, dass ein Übergang der gewünschten Art nicht möglich ist.
        /// </summary>
        public const ushort ErrorIndicator = ushort.MaxValue;

        /// <summary>
        /// Zeigt an, dass nun unkomprimierte Zeichen folgen.
        /// </summary>
        public const ushort EscapeIndicator = (ushort) (ushort.MaxValue - 1);

        /// <summary>
        /// Zeigt das Ende einer Musterliste an.
        /// </summary>
        public const ushort EndIndicator = (ushort) (ushort.MaxValue - 2);

        /// <summary>
        /// Die maximale Anzahl von Codeeinträgen in allen Tabellen zusammen.
        /// </summary>
        private const int MaximumIndex = ushort.MaxValue - 3;

        /// <summary>
        /// Beschreibt das Zeil eines einzelnen Übergangs.
        /// </summary>
        private class TransitionInfo
        {
            /// <summary>
            /// Das Zielzeichen.
            /// </summary>
            public char To { get; set; }

            /// <summary>
            /// Das zu verwendete Bitmuster für den Übergang.
            /// </summary>
            public uint Pattern { get; set; }

            /// <summary>
            /// Die Breite des Bitmusters <see cref="Pattern"/> in Bits.
            /// </summary>
            public int PatternWidth { get; set; }

            /// <summary>
            /// Erzeugt eine neue Übergangsinformation.
            /// </summary>
            public TransitionInfo()
            {
            }
        }

        /// <summary>
        /// Beschreibt alle gemeldeten Übergänge.
        /// </summary>
        private Dictionary<char, List<TransitionInfo>> m_Transitions = new Dictionary<char, List<TransitionInfo>>();

        /// <summary>
        /// Die interne Repräsentations der Übrergänge.
        /// </summary>
        private List<ushort> m_Table;

        /// <summary>
        /// Meldet die Anzahl der Zeichen, für die keine weiteren Übergänge definiert sind.
        /// </summary>
        public uint CharacterNotSupported { get; private set; }

        /// <summary>
        /// Meldet die Anzahl der Stellen, an denen kein Übergang bekannt ist.
        /// </summary>
        public uint DeadEnds { get; private set; }

        /// <summary>
        /// Die Anzahl der Nachschlagetabellen für eine schnelle Dekomprimierung.
        /// </summary>
        public uint DirectTables { get; private set; }

        /// <summary>
        /// Die Anzahl der verketteten Tabellen für eine speicherreduzierte Dekomprimierung.
        /// </summary>
        public uint LinkedTables { get; private set; }

        /// <summary>
        /// Die Anzahl der Einträge in verketteten Tabellen.
        /// </summary>
        public uint LinkedItems { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Repräsentation.
        /// </summary>
        public TableCreator()
        {
        }

        /// <summary>
        /// Meldet eine Übergangssequenz an.
        /// </summary>
        /// <param name="from">Das Eingangszeichen, <i>0</i> zeigt den Beginn einer 
        /// Sequenz an.</param>
        /// <param name="to">Das Folgezeichen, <i>0</i> zeigt den Übergang in den
        /// Direktmodus an, <see cref="char.MaxValue"/> kennzeichnet das Auswerteende.</param>
        /// <param name="pattern">Das Bitmuster, das den Übergang zwischen den Zeichen
        /// beschreibt.</param>
        /// <param name="bits">Die Anzahl der Bits im Bitmuster.</param>
        public void AddTransition( char from, char to, uint pattern, int bits )
        {
            // Validate
            if (((int) from) > 127) 
                throw new ArgumentException( from.ToString(), "from" );
            if ((((int) to) > 127) && (char.MaxValue != to)) 
                throw new ArgumentException( to.ToString(), "to" );
            if ((((int) from) < 1) && ('\0' != from)) 
                throw new ArgumentException( ((int) from).ToString(), "from" );
            if ((((int) to) < 1) && ('\0' != to)) 
                throw new ArgumentException( ((int) to).ToString(), "to" );
            if ((bits < 1) || (bits > 32)) 
                throw new ArgumentException( bits.ToString(), "bits" );

            // Read the current list
            List<TransitionInfo> transitions;
            if (!m_Transitions.TryGetValue( from, out transitions ))
            {
                // Create new
                transitions = new List<TransitionInfo>();

                // Remember
                m_Transitions[from] = transitions;
            }

            // Extend
            transitions.Add( new TransitionInfo() { To = to, Pattern = pattern, PatternWidth = bits } );
        }

        /// <summary>
        /// Erzeugt die Tabelle zum Komprimieren von Zeichenketten.
        /// </summary>
        /// <returns>Die gewünschte Tabelle in Form von 128 x 129 Wertepaaren.</returns>
        public uint[] CreateEncoderTable()
        {
            // Create table
            uint[] table = new uint[(127 + 1) * (1 + 127 + 1) * 2];

            // Fill it
            for (int from = 0; from <= 127; ++from)
            {
                // Get the related character
                char fromChar = (char) from;

                // See if there are known transitions
                List<TransitionInfo> transitions;
                if (!m_Transitions.TryGetValue( fromChar, out transitions ))
                    continue;

                // Get the related index
                int fromIndex = from * (1 + 127 + 1) * 2;

                // Process all
                foreach (TransitionInfo transition in transitions)
                {
                    // The index for the target
                    int toIndex;

                    // Check mode
                    if ('\0' == transition.To)
                        toIndex = 0;
                    else if (char.MaxValue == transition.To)
                        toIndex = 128;
                    else
                        toIndex = (transition.To - '\x0001') + 1;

                    // Finish
                    toIndex = fromIndex + 2 * toIndex;

                    // Store width and pattern
                    table[toIndex + 0] = (uint) transition.PatternWidth;
                    table[toIndex + 1] = transition.Pattern;
                }
            }

            // Report
            return table;
        }

        /// <summary>
        /// Erzeugt oder meldet die interne Repräsentation der Übergänge.
        /// </summary>
        /// <returns>Die interne Repräsentation der Übergänge.</returns>
        public ushort[] GetTable()
        {
            // Once only
            if (null != m_Table)
                return m_Table.ToArray();

            // Create 
            m_Table = new List<ushort>();

            // Starter
            m_Table.Add( ErrorIndicator );

            // ASCII
            for (int ascii = 1; ascii <= 127; ++ascii)
                m_Table.Add( ErrorIndicator );

            // Set global error
            CharacterNotSupported = (uint) (m_Table.Count - m_Transitions.Count);

            // Process all
            foreach (KeyValuePair<char, List<TransitionInfo>> transitions in m_Transitions)
            {
                // Forward
                FillTransitions( transitions.Key, transitions.Value );
            }

            // Overflow
            if (m_Table.Count > MaximumIndex)
                throw new OutOfMemoryException();

            // Report
            return m_Table.ToArray();
        }

        /// <summary>
        /// Ermittelt den Tabellenindex für ein Zeichen.
        /// </summary>
        /// <param name="from">Das gewünschte Zeichen.</param>
        /// <returns>Der zugehörige Index, <i>-1</i> wird für ungültige Zeichen gemeldet.</returns>
        private int GetTableForCharacter( char from )
        {
            // Starter
            if ('\0' == from)
                return 0;

            // ASCII
            if (from >= '\x0001')
                if (from <= '\x7f')
                    return 1 + (from - '\x0001');

            // Not available
            return -1;
        }

        /// <summary>
        /// Erzeugt die Übergangstabellen für ein einzelnen Zeichen.
        /// </summary>
        /// <param name="from">Das betroffene Zeichen.</param>
        /// <param name="transitions">Alle Übergänge des Zeichens.</param>
        private void FillTransitions( char from, List<TransitionInfo> transitions )
        {
            // Process one level
            FillTransitions( from, GetTableForCharacter( from ), transitions, 0, 0, 0 );
        }

        /// <summary>
        /// Erzeugte eine Ebene der internen Repräsentation einer Übergangsliste.
        /// </summary>
        /// <param name="from">Das Zeichen, von dem aus der Übergang ausgeht.</param>
        /// <param name="codeIndex">Aktuelle Index in die Tabelle.</param>
        /// <param name="transitions">Alle Übergänge vom Ausgangszeichen aus.</param>
        /// <param name="bitSkip">Bereits ausgewertete Bits.</param>
        /// <param name="patternMask">Die zu prüfenden Musterbits.</param>
        /// <param name="patternValue">Der erwartete Wert der zu prüfenden Musterbits.</param>
        /// <returns>Gesetzt, wenn die interne Repräsentation gefüllt wurde.</returns>
        private bool FillTransitions( char from, int codeIndex, List<TransitionInfo> transitions, int bitSkip, uint patternMask, uint patternValue )
        {
            // See if this is a leaf
            char? leaf = null;

            // Check for leaf
            if (bitSkip > 0)
                foreach (TransitionInfo transition in transitions)
                    if (bitSkip == transition.PatternWidth)
                        if (patternValue == transition.Pattern)
                        {
                            // Remember
                            leaf = transition.To;

                            // Report the character
                            if (char.MaxValue == leaf.Value)
                                m_Table[codeIndex] = EscapeIndicator;
                            else if ('\0' == leaf.Value)
                                m_Table[codeIndex] = 0;
                            else
                                m_Table[codeIndex] = (ushort) (1 + (leaf.Value - '\x0001'));

                            // Done
                            break;
                        }

            // Next level to process
            int nextWidth = int.MaxValue;

            // Find it
            foreach (TransitionInfo transition in transitions)
                if (transition.PatternWidth > bitSkip)
                    if (transition.PatternWidth < nextWidth)
                        if (patternValue == (patternMask & transition.Pattern))
                            nextWidth = transition.PatternWidth;

            // We reached the end
            if (int.MaxValue == nextWidth)
                if (leaf.HasValue)
                {
                    // Found a leaf
                    return true;
                }
                else
                {
                    // Dead end
                    ++DeadEnds;

                    // Done
                    return false;
                }

            // Check for leaf mismatch
            if (leaf.HasValue)
                throw new ArgumentException( string.Format( "{0} => {1}", from, leaf.Value ), "from" );

            // Enter the index
            m_Table[codeIndex] = (ushort) m_Table.Count;

            // Correct
            nextWidth -= bitSkip;

            // Absolute limit
            if (nextWidth > 24)
                throw new ArgumentException( nextWidth.ToString(), "nextWidth" );

            // Report the number of bits in this level
            m_Table.Add( (ushort) nextWidth );
            m_Table.Add( EndIndicator );

            // Adjust the index
            codeIndex = m_Table.Count;

            // Extend the depth
            bitSkip += nextWidth;

            // Extend the mask
            patternMask |= (uint) (((1 << nextWidth) - 1) << (32 - bitSkip));

            // Check mode
            if (nextWidth < 7)
            {
                // Count
                ++DirectTables;

                // Create static area
                for (int i = 1 << nextWidth; i-- > 0; )
                    m_Table.Add( ErrorIndicator );

                // Process all pattern
                for (int pattern = 1 << nextWidth; pattern-- > 0; )
                {
                    // Process next level
                    FillTransitions( from, codeIndex + pattern, transitions, bitSkip, patternMask, patternValue | (uint) ((pattern << (32 - bitSkip))) );
                }
            }
            else
            {
                // Count
                ++LinkedTables;

                // Process all pattern
                for (int pattern = 1 << nextWidth; pattern-- > 0; )
                {
                    // Prepare the entry
                    m_Table.Add( (ushort) (pattern >> 16) );
                    m_Table.Add( (ushort) pattern );
                    m_Table.Add( ErrorIndicator );
                    m_Table.Add( m_Table[codeIndex - 1] );

                    // New position
                    int newCodeIndex = m_Table.Count;

                    // Process next level
                    if (FillTransitions( from, newCodeIndex - 2, transitions, bitSkip, patternMask, patternValue | (uint) ((pattern << (32 - bitSkip))) ))
                    {
                        // Count entry
                        ++LinkedItems;

                        // Update the link
                        m_Table[codeIndex - 1] = (ushort) newCodeIndex;

                        // Work with his
                        codeIndex = newCodeIndex;
                    }
                    else
                    {
                        // Just discard
                        m_Table.RemoveRange( newCodeIndex - 4, 4 );
                    }
                }
            }

            // Did something
            return true;
        }
    }
}
