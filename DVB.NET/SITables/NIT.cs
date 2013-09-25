extern alias oldVersion;

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using legacy = oldVersion::JMS.DVB;

namespace JMS.DVB.SI
{
    /// <summary>
    /// Diese Klasse beschreibt eine Tabelle mit der Beschreibung der
    /// Quellgruppen für ein Netzwerk.
    /// </summary>
    public class FullNIT : WellKnownLegacyTable<legacy.EPG.Tables.NIT>
    {
        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        /// <param name="table">Die empfangene Tabelle.</param>
        public FullNIT( legacy.EPG.Tables.NIT table )
            : base( table )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public FullNIT()
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
                return new byte[] { 0x40, 0x41 };
            }
        }

        /// <summary>
        /// Meldet, ob sich die Daten in der Tabelle auf die aktuell angewählte
        /// Quellgruppe bezieht.
        /// </summary>
        public bool ForCurrentGroup
        {
            get
            {
                // Check it
                return (0x40 == Table.Section.TableIdentifier);
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
                return 0x10;
            }
        }
    }

    /// <summary>
    /// Diese Klasse beschreibt eine Tabelle mit der Beschreibung der
    /// Quellgruppen für das aktuell angewählte Netzwerk.
    /// </summary>
    public class NIT : FullNIT
    {
        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        /// <param name="table">Die empfangene Tabelle.</param>
        public NIT( legacy.EPG.Tables.NIT table )
            : base( table )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public NIT()
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
                return new byte[] { 0x40 };
            }
        }
    }
}
