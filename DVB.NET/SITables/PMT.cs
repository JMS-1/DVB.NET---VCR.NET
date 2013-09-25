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
    /// Mit dieser Klasse werden die Datenströme einer einzelnen Quelle beschrieben.
    /// </summary>
    public class PMT : LegacyTable<legacy.EPG.Tables.PMT>
    {
        /// <summary>
        /// Erzeugt eine neue Quellbeschreibung.
        /// </summary>
        /// <param name="table">Die empfangene Tabelle.</param>
        public PMT( legacy.EPG.Tables.PMT table )
            : base( table )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Quellbeschreibung.
        /// </summary>
        public PMT()
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
                return new byte[] { 0x02 };
            }
        }
    }
}
