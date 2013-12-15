extern alias oldVersion;

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using legacy = oldVersion::JMS.DVB;

namespace JMS.DVB.SI
{
    /// <summary>
    /// Basisklasse zur Analyse von Rohdatenströmen mit SI Tabellen.
    /// </summary>
    public class TableParser
    {
        /// <summary>
        /// Methode, die für jede empfangene SI Tabelle aufgerufen wird.
        /// </summary>
        private Action<Table> m_Consumer;

        /// <summary>
        /// Alle von dieser Analyseeinheiten auszuwertenden Arten von SI Tabellen - Tabellen
        /// anderer Art werden einfach verworfen.
        /// </summary>
        private Dictionary<byte, Type> m_Types = new Dictionary<byte, Type>();

        /// <summary>
        /// Übergangslösung für DVB.NET 3.5.1: es wird der SI Mechanismus von DVB.NET 3.5
        /// (oder früher) verwendet.
        /// </summary>
        private legacy.EPG.Parser m_Parser = new legacy.EPG.Parser();

        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        /// <param name="consumer">Methode zur Auswertung der SI Tabellen.</param>
        /// <param name="tableTypes">Die Liste der zu unterstützenden Arten von SI Tabellen.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Verbraucher oder keine Art angegeben.</exception>
        /// <exception cref="ArgumentException">Eine SI Tabellenart wird mehrfach verwendet oder einer der
        /// angegebenen .NET Klassen ist keine <see cref="Table"/>.</exception>
        private TableParser( Action<Table> consumer, List<Type> tableTypes )
        {
            // Validate
            if (null == consumer) throw new ArgumentNullException( "consumer" );
            if ((null == tableTypes) || (tableTypes.Count < 1)) throw new ArgumentNullException( "tableTypes" );

            // Remember
            m_Consumer = consumer;

            // Build up
            foreach (Type tableType in tableTypes)
            {
                // Validate
                if (null == tableType) throw new ArgumentNullException( "tableTypes" );
                if (!typeof( Table ).IsAssignableFrom( tableType )) throw new ArgumentException( tableType.FullName, "tableTypes" );

                // Process each type
                foreach (byte tableIdentifier in Table.GetTableIdentifiers( tableType ))
                    if (m_Types.ContainsKey( tableIdentifier ))
                        throw new ArgumentException( tableIdentifier.ToString(), "tableTypes" );
                    else
                        m_Types[tableIdentifier] = tableType;
            }

            // Attach analyser
            m_Parser.SectionFound += SectionFound;
        }

        /// <summary>
        /// Wird aufgerufen, sobald eine Tabelle vollständig analysiert wurde.
        /// </summary>
        /// <param name="section">Der SI Bereich zur Tabelle.</param>
        private void SectionFound( legacy.EPG.Section section )
        {
            // Attach to consumer
            Action<Table> consumer = m_Consumer;

            // Disabled
            if (null == consumer)
                return;

            // Skip if not valid
            if ((null == section) || !section.IsValid)
                return;

            // Skip if no table
            if ((null == section.Table) || !section.Table.IsValid)
                return;

            // Find the related type
            Type tableType;
            if (!m_Types.TryGetValue( section.TableIdentifier, out tableType ))
                return;

            // Be safe
            try
            {
                // Create wrapped version
                Table wrapper = (Table) Activator.CreateInstance( tableType, section.Table );

                // Forward
                consumer( wrapper );
            }
            catch
            {
                // Ignore any error
            }
        }

        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        /// <param name="consumer">Methode zur Auswertung der SI Tabellen.</param>
        /// <param name="tableType">Die primäre Art der verwendeten SI Tabelle.</param>
        /// <param name="tableTypes">Optional eine Liste weiterer zu unterstützender Arten von SI Tabellen.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Verbraucher oder keine Art angegeben.</exception>
        /// <returns>Die neu erzeugte Analyseinstanz.</returns>
        public static TableParser Create( Action<Table> consumer, Type tableType, params Type[] tableTypes )
        {
            // Create the list
            List<Type> types = new List<Type> { tableType };

            // Add optional types
            if (null != tableTypes)
                types.AddRange( tableTypes );

            // Forward
            return new TableParser( consumer, types );
        }

        /// <summary>
        /// Erzeugt einen neue Analyseinstanz für genau eine Art von SI Tabelle.
        /// </summary>
        /// <typeparam name="T">Die gewünschte Tabellenart.</typeparam>
        /// <param name="consumer">Die Methode, die mit jeder erkannten Tabelle aufgerufen 
        /// werden soll.</param>
        /// <returns>Die neu erzeugte Instanz.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Verbraucher angegeben.</exception>
        public static TableParser Create<T>( Action<T> consumer ) where T : Table
        {
            // Validate
            if (null == consumer) throw new ArgumentNullException( "consumer" );

            // Forward
            return Create( t => consumer( (T) t ), typeof( T ) );
        }

        /// <summary>
        /// Überträgt Rohdaten zur Analyse in diese Instanz.
        /// </summary>
        /// <param name="payload">Die zu analysierenden Rohdaten.</param>
        /// <exception cref="ArgumentNullException">Es wurden keine Rohdaten angegeben.</exception>
        public void AddPayload( byte[] payload )
        {
            // Forward
            AddPayload( payload, 0, payload.Length );
        }

        /// <summary>
        /// Überträgt Rohdaten zur Analyse in diese Instanz.
        /// </summary>
        /// <param name="payload">Die zu analysierenden Rohdaten.</param>
        /// <param name="offset">Die 0-basierte laufende Nummer des ersten zu analysierenden
        /// Bytes.</param>
        /// <param name="length">Die Anzahl der zu analyiserenden Bytes.</param>
        /// <exception cref="ArgumentNullException">Es wurden keine Rohdaten angegeben.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Der gewünschte Bereich innerhalb der Rohdaten
        /// existiert nicht.</exception>
        public void AddPayload( byte[] payload, int offset, int length )
        {
            // Just forward
            m_Parser.OnData( payload, offset, length );
        }
    }
}
