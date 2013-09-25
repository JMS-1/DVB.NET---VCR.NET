extern alias oldVersion;

using JMS.DVB.SI.ProgramGuide;
using System.Collections.Generic;

using legacy = oldVersion::JMS.DVB;

namespace JMS.DVB.SI
{
    /// <summary>
    /// Diese Klasse beschreibt eine Tabelle mit Daten für die elektronische
    /// Programmzeitschrift <i>EPG (Electronic Program Guide)</i>.
    /// </summary>
    public class EIT : WellKnownLegacyTable<legacy.EPG.Tables.EIT>
    {
        /// <summary>
        /// Die Quelle, deren Quellgruppe (Transponder) die Programmzeitschrift der englischen Sender enthält.
        /// </summary>
        public static readonly SourceIdentifier FreeSatEPGTriggerSource = new SourceIdentifier { Network = 59, TransportStream = 2315, Service = 10500 };

        /// <summary>
        /// Die Datenstromkennung für die properitäre Programmzeitschrift der englischen Sender.
        /// </summary>
        public const ushort FreeSatEPGPID = 0xbbb;

        /// <summary>
        /// Die Datenstromkennung (PID) der Programmzeitschrift.
        /// </summary>
        public const ushort DefaultStreamIdentifier = 0x12;

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        /// <param name="table">Die empfangene Tabelle.</param>
        public EIT( legacy.EPG.Tables.EIT table )
            : base( table )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public EIT()
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
                return new byte[]
                    {
                        0x4e, 0x4f,
                        0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a, 0x5b, 0x5c, 0x5d, 0x5e, 0x5f, 
                        0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c, 0x6d, 0x6e, 0x6f, 
                    };
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
                // Current or next
                if (0x4e == Table.Section.TableIdentifier)
                    return true;

                // Schedule
                if (Table.Section.TableIdentifier >= 0x50)
                    if (Table.Section.TableIdentifier <= 0x5f)
                        return true;

                // Other
                return false;
            }
        }

        /// <summary>
        /// Meldet, ob sich die Daten in der Tabelle auf eine gerade laufende oder
        /// unmittelbar bevorstehende Ausstrahlung beziehen.
        /// </summary>
        public bool ForCurrentOrNext
        {
            get
            {
                // This stream
                if (0x4e == Table.Section.TableIdentifier)
                    return true;

                // Other stream
                if (0x4f == Table.Section.TableIdentifier)
                    return true;

                // No, must be schedule
                return false;
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
                return DefaultStreamIdentifier;
            }
        }

        /// <summary>
        /// Meldet die Quelle zu diesen Informationen.
        /// </summary>
        public SourceIdentifier Source
        {
            get
            {
                // Be a bit more safe than necessary
                if ((null == Table) || !Table.IsValid)
                    return new SourceIdentifier();
                else
                    return new SourceIdentifier { Network = Table.OriginalNetworkIdentifier, TransportStream = Table.TransportStreamIdentifier, Service = Table.ServiceIdentifier };
            }
        }

        /// <summary>
        /// Meldet alle Sendungen, die in dieser Tabelle vorhanden sind.
        /// </summary>
        public IEnumerable<Event> Events
        {
            get
            {
                // Attach to source - will validate table
                var source = Source;

                // None
                if (source == null)
                    yield break;

                // Process all
                if (Table.Entries != null)
                    foreach (var entry in Table.Entries)
                        yield return new Event( source, entry );
            }
        }
    }
}
