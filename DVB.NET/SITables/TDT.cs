extern alias oldVersion;

using System;

using legacy = oldVersion::JMS.DVB;

namespace JMS.DVB.SI
{
    /// <summary>
    /// Beschreibt eine <i>Time and Date Table</i> Tabelle.
    /// </summary>
    public class TDT : WellKnownLegacyTable<legacy.EPG.Tables.TDT>
    {
        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        /// <param name="table">Die empfangene Tabelle.</param>
        public TDT( legacy.EPG.Tables.TDT table )
            : base( table )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public TDT()
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
                return new byte[] { 0x70 };
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
                return 0x14;
            }
        }

        /// <summary>
        /// Meldet den dieser Tabelle zugeordneten Zeitpunkt.
        /// </summary>
        public DateTime TimeStamp
        {
            get
            {
                // Report
                return Table.Time;
            }
        }
    }
}
