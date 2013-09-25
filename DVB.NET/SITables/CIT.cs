extern alias oldVersion;

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using legacy = oldVersion::JMS.DVB;

namespace JMS.DVB.SI
{
    /// <summary>
    /// Über diese Klasse wird die Programmzeitschrift der PREMIERE Dienste empfangen.
    /// </summary>
    public abstract class CIT : WellKnownLegacyTable<legacy.EPG.Tables.CITPremiere>
    {
        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        /// <param name="table">Die empfangene Tabelle.</param>
        public CIT( legacy.EPG.Tables.CITPremiere table )
            : base( table )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public CIT()
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
                        0xa0
                    };
            }
        }

        /// <summary>
        /// Meldet, ob diese Tabelle nur Tabellenkennungen oberhalb von 0x7f verwendet.
        /// </summary>
        public override bool IsExtendedTable
        {
            get
            {
                // Report
                return true;
            }
        }
    }

    /// <summary>
    /// Empfängt die Programmzeitschrift der PREMIERE Direkt Dienste.
    /// </summary>
    public class DirectCIT : CIT
    {
        /// <summary>
        /// Die Quelle, in deren Quellgruppe (Transponder) die Programmzeitschrift bereitgestellt wird.
        /// </summary>
        public static readonly SourceIdentifier TriggerSource = new SourceIdentifier { Network = 133, TransportStream = 4, Service = 18 };

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        /// <param name="table">Die empfangene Tabelle.</param>
        public DirectCIT( legacy.EPG.Tables.CITPremiere table )
            : base( table )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public DirectCIT()
            : this( null )
        {
        }

        /// <summary>
        /// Meldet den Datenstrom, an den dieser Typ von Tabelle fest gebunden ist.
        /// </summary>
        public override ushort WellKnownStream
        {
            get
            {
                // Report
                return 0xb11;
            }
        }
    }

    /// <summary>
    /// Empfängt die Programmzeitschrift der PREMIERE Sport Dienste.
    /// </summary>
    public class SportCIT : CIT
    {
        /// <summary>
        /// Die Quelle, in deren Quellgruppe (Transponder) die Programmzeitschrift bereitgestellt wird.
        /// </summary>
        public static readonly SourceIdentifier TriggerSource = new SourceIdentifier { Network = 133, TransportStream = 3, Service = 17 };

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        /// <param name="table">Die empfangene Tabelle.</param>
        public SportCIT( legacy.EPG.Tables.CITPremiere table )
            : base( table )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public SportCIT()
            : this( null )
        {
        }

        /// <summary>
        /// Meldet den Datenstrom, an den dieser Typ von Tabelle fest gebunden ist.
        /// </summary>
        public override ushort WellKnownStream
        {
            get
            {
                // Report
                return 0xb12;
            }
        }
    }
}
