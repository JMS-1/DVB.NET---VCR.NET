using System.Collections.Generic;


namespace JMS.DVB.EPG.Tables
{
    /// <summary>
    /// Die Informationen zu einem Netzwerk.
    /// </summary>
    public class NIT : Table
    {
        /// <summary>
        /// Gesezt, wenn die Informationen zum aktiven Datenstrom gehören.
        /// </summary>
        public readonly bool ForThisStream;

        /// <summary>
        /// Die Kennung des Netzwerks.
        /// </summary>
        public readonly ushort NetworkIdentifier;

        /// <summary>
        /// Alle Detailbeschreibungen.
        /// </summary>
        public readonly Descriptor[] Descriptors;

        /// <summary>
        /// Die Komponenten des Netzwerks.
        /// </summary>
        public readonly NetworkEntry[] NetworkEntries;

        /// <summary>
        /// Prüft, ob eine Tabellenkennung zu einer Netzwerkbeschreibung gehört.
        /// </summary>
        /// <param name="tableIdentifier">Die zu prüfende Kennung.</param>
        /// <returns>Gesetzt, wenn die Kennung eine Netzwerkbeschreibung bezeichnet.</returns>
        public static bool IsHandlerFor( byte tableIdentifier )
        {
            // Check all
            return ((0x40 == tableIdentifier) || (0x41 == tableIdentifier));
        }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="section">Die Rohdaten.</param>
        public NIT( Section section )
            : base( section )
        {
            // Load length
            int length = section.Length - 7;

            // Check for minimum length required
            if (length < 7) return;

            // Load the length of the descriptors
            int deslen = Tools.MergeBytesToWord( section[6], section[5] ) & 0x0fff;

            // Where the service area starts
            int svcoff = 7 + deslen;

            // Correct
            length -= svcoff;

            // Check for minimum length required
            if (length < 2) return;

            // Load the length of the service information
            int svclen = Tools.MergeBytesToWord( section[svcoff + 1], section[svcoff + 0] ) & 0x0fff;

            // Correct
            length -= 2 + svclen;

            // Validate
            if (0 != length) return;

            // Load all descriptors
            Descriptors = Descriptor.Load( this, 7, deslen );

            // Load special
            NetworkIdentifier = Tools.MergeBytesToWord( section[1], section[0] );
            ForThisStream = (0x40 == section.TableIdentifier);

            // Result
            List<NetworkEntry> entries = new List<NetworkEntry>();

            // Fill
            for (svcoff += 2; svclen > 0; )
            {
                // Create next
                NetworkEntry entry = NetworkEntry.Create( this, svcoff, svclen );

                // Failed
                if ((null == entry) || !entry.IsValid) return;

                // Remember
                entries.Add( entry );

                // Adjust
                svcoff += entry.Length;
                svclen -= entry.Length;
            }

            // Use it
            NetworkEntries = entries.ToArray();

            // Done
            m_IsValid = true;
        }
    }
}
