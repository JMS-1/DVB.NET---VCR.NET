

namespace JMS.DVB.EPG
{
    /// <summary>
    /// A single entry in a <see cref="Tables.NIT"/>.
    /// </summary>
    public class NetworkEntry : EntryBase
    {
        /// <summary>
        /// Alle Detailbeschreibungen.
        /// </summary>
        public readonly Descriptor[] Descriptors;

        /// <summary>
        /// Gesetzt, wenn diese Beschreibung konsistent ist.
        /// </summary>
        public readonly bool IsValid = false;

        /// <summary>
        /// Die Größe der Beschreibung.
        /// </summary>
        public readonly int Length;

        /// <summary>
        /// Die Kennung des Datenstroms.
        /// </summary>
        public ushort TransportStreamIdentifier;

        /// <summary>
        /// Die Kennung des Netzwerks.
        /// </summary>
        public ushort OriginalNetworkIdentifier;

        /// <summary>
        /// Erstellt eine Beschreibung.
        /// </summary>
        /// <param name="table">Die gesamte <i>NIT</i>.</param>
        /// <param name="offset">Das erste Byte dieser Beschreibung in den Rohdaten.</param>
        /// <param name="length">Die Größe der Rohdaten zu dieser Beschreibung.</param>
        private NetworkEntry( Table table, int offset, int length )
            : base( table )
        {
            // Access section
            Section section = Section;

            // Load
            TransportStreamIdentifier = (ushort) Tools.MergeBytesToWord( section[offset + 1], section[offset + 0] );
            OriginalNetworkIdentifier = (ushort) Tools.MergeBytesToWord( section[offset + 3], section[offset + 2] );

            // Read the length
            int descrLength = 0xfff & Tools.MergeBytesToWord( section[offset + 5], section[offset + 4] );

            // Caluclate the total length
            Length = 6 + descrLength;

            // Verify
            if (Length > length) return;

            // Try to load descriptors
            Descriptors = Descriptor.Load( this, offset + 6, descrLength );

            // Can use it
            IsValid = true;
        }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="table">Die gesamte <i>NIT</i>.</param>
        /// <param name="offset">Das erste Byte dieser Beschreibung in den Rohdaten.</param>
        /// <param name="length">Die Größe der Rohdaten zu dieser Beschreibung.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        static internal NetworkEntry Create( Table table, int offset, int length )
        {
            // Validate
            if (length < 6) return null;

            // Create
            return new NetworkEntry( table, offset, length );
        }
    }
}
