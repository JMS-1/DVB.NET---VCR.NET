extern alias oldVersion;

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using legacy = oldVersion::JMS.DVB;

namespace JMS.DVB.SI
{
    /// <summary>
    /// Diese Klasse beschreibt eine Tabelle mit der Beschreibung aller
    /// Dienste, die über die zugehörige Quellgruppe oder eine andere
    /// Quellgruppe mit dem selben Ursprung verfügbar sind.
    /// </summary>
    public class FullSDT : WellKnownLegacyTable<legacy.EPG.Tables.SDT>
    {
        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        /// <param name="table">Die empfangene Tabelle.</param>
        public FullSDT( legacy.EPG.Tables.SDT table )
            : base( table )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public FullSDT()
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
                return new byte[] { 0x42, 0x46 };
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
                return 0x11;
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

        /// <summary>
        /// Meldet die eindeutige Nummer des zugehörigen DVB Ursprungsnetzwerks.
        /// </summary>
        public ushort Network
        {
            get
            {
                // Forward
                return Table.OriginalNetworkIdentifier;
            }
        }
    }

    /// <summary>
    /// Diese Klasse beschreibt eine Tabelle mit der Beschreibung aller
    /// Dienste, die über die zugehörige Quellgruppe verfügbar sind.
    /// </summary>
    public class SDT : FullSDT
    {
        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        /// <param name="table">Die empfangene Tabelle.</param>
        public SDT( legacy.EPG.Tables.SDT table )
            : base( table )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public SDT()
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
                return new byte[] { 0x42 };
            }
        }
    }
}
