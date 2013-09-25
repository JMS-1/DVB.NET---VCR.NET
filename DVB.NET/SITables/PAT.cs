extern alias oldVersion;

using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using legacy = oldVersion::JMS.DVB;

namespace JMS.DVB.SI
{
    /// <summary>
    /// Diese Klasse beschreibt eine Tabelle mit Daten für alle Dienste
    /// einer Quellgruppe - neben normalen Radio- und Fernsehquellen kann
    /// es auch Datendienste geben, die allerdings für DVB.NET nicht
    /// von Interesse sind.
    /// </summary>
    public class PAT : WellKnownLegacyTable<legacy.EPG.Tables.PAT>, IEnumerable<KeyValuePair<ushort, ushort>>
    {
        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        /// <param name="table">Die empfangene Tabelle.</param>
        public PAT( legacy.EPG.Tables.PAT table )
            : base( table )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public PAT()
            : this( null )
        {
        }

        /// <summary>
        /// Meldet die Liste der SI Tabellenarten, die von dieser Klasse
        /// abgedeckt werden.
        /// </summary>
        public override byte[] TableIdentifiers
        {
            get
            {
                // Report
                return new byte[] { 0x00 };
            }
        }

        /// <summary>
        /// Meldet den Datenstrom, an den dieser Typ von Tabelle fest gebunden ist.
        /// </summary>
        public override ushort WellKnownStream
        {
            get
            {
                // Report
                return 0x00;
            }
        }

        /// <summary>
        /// Meldet die Anzahl der verwalteten Assoziationen.
        /// </summary>
        public int Count
        {
            get
            {
                // Report
                return Table.ProgramIdentifier.Count;
            }
        }

        /// <summary>
        /// Meldet die Datenstromkennung (PID) der SI Tablle PMT eines Dienstes.
        /// </summary>
        /// <param name="serviceIdentifier">Die eindeutige Kennung des Dienstes.</param>
        /// <returns>Die zugehörige Datenstromkennung oder <i>null</i>, wenn
        /// der Dienst nicht bekannt ist.</returns>
        public ushort? this[ushort serviceIdentifier]
        {
            get
            {
                // Load
                ushort pmt;
                if (Table.ProgramIdentifier.TryGetValue( serviceIdentifier, out pmt ))
                    return pmt;

                // Not found
                return null;
            }
        }

        /// <summary>
        /// Meldet die eindeutigen Kennungen aller Dienste.
        /// </summary>
        public IEnumerable<ushort> Services
        {
            get
            {
                // Report
                return Table.ProgramIdentifier.Keys;
            }
        }

        /// <summary>
        /// Meldet die eindeutige Nummer des zugehörigen <i>Transport Streams</i>.
        /// </summary>
        public ushort TransportStream
        {
            get
            {
                // Forward
                return Table.TransportStreamIdentifier;
            }
        }


        #region IEnumerable<KeyValuePair<ushort,ushort>> Members

        /// <summary>
        /// Meldet eine Auflistung über alle Assoziationen.
        /// </summary>
        /// <returns>Eine neu erzeugte Auflistung.</returns>
        public IEnumerator<KeyValuePair<ushort, ushort>> GetEnumerator()
        {
            // Forward
            return Table.ProgramIdentifier.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Meldet eine Auflistung über alle Assoziationen.
        /// </summary>
        /// <returns>Eine neu erzeugte Auflistung.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            // Forward
            return GetEnumerator();
        }

        #endregion
    }
}
