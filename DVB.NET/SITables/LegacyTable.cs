extern alias oldVersion;

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using legacy = oldVersion::JMS.DVB;

namespace JMS.DVB.SI
{
    /// <summary>
    /// Hilfsklasse, die eine SI Tabelle auf Basis der DVB.NET 3.5 (oder früher)
    /// Implementierung vornimmt.
    /// </summary>
    /// <typeparam name="T">Die Art der SI Tabelle.</typeparam>
    public abstract class LegacyTable<T> : Table where T : legacy.EPG.Table
    {
        /// <summary>
        /// Liest oder setzt die DVB.NET 3.5 (oder früher) Tabelle.
        /// </summary>
        public T Table { get; private set; }

        /// <summary>
        /// Initialisiert eine Umsetzungsinstanz.
        /// </summary>
        /// <param name="table">Die DVB.NET 3.5 (oder früher) Tabelle.</param>
        public LegacyTable( T table )
        {
            // Remember
            Table = table;
        }

        /// <summary>
        /// Meldet die aktuelle laufende Nummer der Tabelle in einer Gruppe zusammengehörender Tabellen.
        /// </summary>
        public override int CurrentSection
        {
            get
            {
                // Forward
                return Table.SectionNumber;
            }
        }

        /// <summary>
        /// Meldet die letzte Nummer einer Tabelle in einer Gruppe zusammengehörender Tabellen
        /// </summary>
        public override int LastSection
        {
            get
            {
                // Forward
                return Table.LastSectionNumber;
            }
        }

        /// <summary>
        /// Meldet die Versionsnummer dieser Tabelle.
        /// </summary>
        public override int Version
        {
            get
            {
                // Forward
                return Table.Version;
            }
        }
    }

    /// <summary>
    /// Hilfsklasse, die eine SI Tabelle auf Basis der DVB.NET 3.5 (oder früher)
    /// Implementierung vornimmt.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class WellKnownLegacyTable<T> : WellKnownTable where T : legacy.EPG.Table
    {
        /// <summary>
        /// Liest oder setzt die DVB.NET 3.5 (oder früher) Tabelle.
        /// </summary>
        public T Table { get; private set; }

        /// <summary>
        /// Initialisiert eine Umsetzungsinstanz.
        /// </summary>
        /// <param name="table">Die DVB.NET 3.5 (oder früher) Tabelle.</param>
        public WellKnownLegacyTable( T table )
        {
            // Remember
            Table = table;
        }

        /// <summary>
        /// Meldet die aktuelle laufende Nummer der Tabelle in einer Gruppe zusammengehörender Tabellen.
        /// </summary>
        public override int CurrentSection
        {
            get
            {
                // Forward
                return Table.SectionNumber;
            }
        }

        /// <summary>
        /// Meldet die letzte Nummer einer Tabelle in einer Gruppe zusammengehörender Tabellen
        /// </summary>
        public override int LastSection
        {
            get
            {
                // Forward
                return Table.LastSectionNumber;
            }
        }

        /// <summary>
        /// Meldet die Versionsnummer dieser Tabelle.
        /// </summary>
        public override int Version
        {
            get
            {
                // Forward
                return Table.Version;
            }
        }
    }
}
