

namespace JMS.DVB.EPG
{
    /// <summary>
    /// Beschreibt einen Datenstrom mit VideoText Informationen.
    /// </summary>
    public class TeletextItem
    {
        /// <summary>
        /// Die verwendete Sprache in der <i>ISO</i> Notation.
        /// </summary>
        public readonly string ISOLanguage;

        /// <summary>
        /// Die Art des VideoTextes.
        /// </summary>
        public readonly TeletextTypes Type;

        /// <summary>
        /// Die Nummer des Textblocks.
        /// </summary>
        public readonly byte MagazineNumber;

        /// <summary>
        /// Die Nummer der Seite.
        /// </summary>
        public readonly byte PageNumberBCD;

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="language">Die verwendete Sprache in <i>ISO</i> Notation.</param>
        /// <param name="type">Die Art der VideoText Information.</param>
        /// <param name="magazine">Die Nummer des Textblocks.</param>
        /// <param name="pageBCD">Die Nummer der Seite im Textblock.</param>
        public TeletextItem( string language, TeletextTypes type, byte magazine, byte pageBCD )
        {
            // Remember
            MagazineNumber = magazine;
            PageNumberBCD = pageBCD;
            ISOLanguage = language;
            Type = type;
        }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="section">Die Rohdaten.</param>
        /// <param name="offset">Das erste Byte in den Rohdaten, das zu einer Beschreibung gehört.</param>
        private TeletextItem( Section section, int offset )
        {
            // Load
            ISOLanguage = section.ReadString( offset + 0, 3 );
            Type = (TeletextTypes) (section[offset + 3] >> 3);
            MagazineNumber = (byte) (section[offset + 3] & 0x7);

            // Decode
            PageNumberBCD = section[offset + 4];
        }

        /// <summary>
        /// Die Größe der zugehörigen Rohdaten in Bytes.
        /// </summary>
        public int Length { get { return 5; } }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="section">Die Rohdaten.</param>
        /// <param name="offset">Das erste Byte in den Rohdaten, das zu einer Beschreibung gehört.</param>
        /// <param name="length">Die Größe der Beschreibung in den Rohdaten.</param>
        /// <returns>Die angeforderte neue Beschreibung.</returns>
        internal static TeletextItem Create( Section section, int offset, int length )
        {
            // Check 
            if (length < 5) return null;

            // Create
            return new TeletextItem( section, offset );
        }

        /// <summary>
        /// Rekonstruiert eine VideoText Beschreibung.
        /// </summary>
        /// <param name="buffer">Sammelt Detailbeschreibungen.</param>
        internal void CreatePayload( TableConstructor buffer )
        {
            // The language
            buffer.AddLanguage( ISOLanguage );

            // Code field
            buffer.Add( (byte) ((MagazineNumber & 0x7) | (((int) Type) << 3)) );
            buffer.Add( PageNumberBCD );
        }
    }
}
