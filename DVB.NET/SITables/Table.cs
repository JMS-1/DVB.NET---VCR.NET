using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.SI
{
    /// <summary>
    /// Basisklasse zur Implementierung von SI Tabellen.
    /// </summary>
    public abstract class Table
    {
        /// <summary>
        /// Initialisiert die Basisklasse.
        /// </summary>
        protected Table()
        {
        }

        /// <summary>
        /// Meldet die Liste der SI Tabellenarten, die von dieser Klasse
        /// abgedeckt werden.
        /// </summary>
        public abstract byte[] TableIdentifiers { get; }

        /// <summary>
        /// Ermittelt zu einer Tabellenart die zugehörigen SI Arten.
        /// </summary>
        /// <typeparam name="T">Die betroffene Tabellenart.</typeparam>
        /// <returns>Die Liste der Tabellenarten.</returns>
        public static byte[] GetTableIdentifiers<T>() where T : Table
        {
            // Report
            return GetTableIdentifiers( typeof( T ) );
        }

        /// <summary>
        /// Ermittelt zu einer Tabellenart die zugehörigen SI Arten.
        /// </summary>
        /// <param name="tableType">Die betroffene Tabellenart.</param>
        /// /// <returns>Die Liste der Tabellenarten.</returns>
        public static byte[] GetTableIdentifiers( Type tableType )
        {
            // Create
            Table template = (Table) Activator.CreateInstance( tableType );

            // Report
            return template.TableIdentifiers;
        }

        /// <summary>
        /// Meldet die aktuelle laufende Nummer der Tabelle in einer Gruppe zusammengehörender Tabellen.
        /// </summary>
        public abstract int CurrentSection { get; }

        /// <summary>
        /// Meldet die letzte Nummer einer Tabelle in einer Gruppe zusammengehörender Tabellen
        /// </summary>
        public abstract int LastSection { get; }

        /// <summary>
        /// Meldet die Versionsnummer dieser Tabelle.
        /// </summary>
        public abstract int Version { get; }

        /// <summary>
        /// Meldet, ob diese Tabelle nur Tabellenkennungen oberhalb von 0x7f verwendet.
        /// </summary>
        public virtual bool IsExtendedTable
        {
            get
            {
                // Report
                return false;
            }
        }

        /// <summary>
        /// Ermittelt, ob eine bestimmte Art von Tabelle nur Tabellenkennungen oberhalb von 0x7f verwendet.
        /// </summary>
        /// <typeparam name="T">Die Art der Tabelle.</typeparam>
        /// <returns>Gesetzt, wenn Tabellenkennungen oberhalb von 0x7f verwendet werden.</returns>
        public static bool GetIsExtendedTable<T>() where T : Table
        {
            // Forward
            return GetIsExtendedTable( typeof( T ) );
        }

        /// <summary>
        /// Ermittelt, ob eine bestimmte Art von Tabelle nur Tabellenkennungen oberhalb von 0x7f verwendet.
        /// </summary>
        /// <param name="tableType">Die Art der Tabelle.</param>
        /// <returns>Gesetzt, wenn Tabellenkennungen oberhalb von 0x7f verwendet werden.</returns>
        public static bool GetIsExtendedTable( Type tableType )
        {
            // Create
            Table template = (Table) Activator.CreateInstance( tableType );

            // Report
            return template.IsExtendedTable;
        }
    }

    /// <summary>
    /// Basisklasse zur Implementierung von SI Tabellen, die fest an einen Datenstrom
    /// gebunden sind.
    /// </summary>
    public abstract class WellKnownTable : Table
    {
        /// <summary>
        /// Initialisiert die Basisklasse.
        /// </summary>
        protected WellKnownTable()
        {
        }

        /// <summary>
        /// Meldet den Datenstrom, an den dieser Typ von Tabelle fest gebunden ist.
        /// </summary>
        public abstract ushort WellKnownStream { get; }

        /// <summary>
        /// Ermittelt zu einer Tabelleart die laufende Nummer (PID) des Datenstroms, an den
        /// diese fest gekoppelt ist.
        /// </summary>
        /// <typeparam name="T">Die betroffene Tabellenart.</typeparam>
        /// <returns>Die gewünschte laufende Nummer.</returns>
        public static ushort GetWellKnownStream<T>() where T : WellKnownTable
        {
            // Forward
            return GetWellKnownStream( typeof( T ) );
        }

        /// <summary>
        /// Ermittelt zu einer Tabelleart die laufende Nummer (PID) des Datenstroms, an den
        /// diese fest gekoppelt ist.
        /// </summary>
        /// <param name="tableType">Die betroffene Tabellenart.</param>
        /// <returns>Die gewünschte laufende Nummer.</returns>
        public static ushort GetWellKnownStream( Type tableType )
        {
            // Create
            WellKnownTable template = (WellKnownTable) Activator.CreateInstance( tableType );

            // Report
            return template.WellKnownStream;
        }
    }
}
