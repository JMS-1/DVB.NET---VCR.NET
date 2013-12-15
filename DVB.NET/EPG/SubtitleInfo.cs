using System;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// Die möglichen Arten von Untertiteln.
    /// </summary>
    public enum SubtitleTypes
    {
        /// <summary>
        /// Videotext.
        /// </summary>
        EBUTeletext = 0x01,

        /// <summary>
        /// Videotext (assoziiert).
        /// </summary>
        EBUTeletextAssociated = 0x02,

        /// <summary>
        /// VBI.
        /// </summary>
        VBIData = 0x03,

        /// <summary>
        /// DVB, kein Bildschirmverhältnis festgelegt.
        /// </summary>
        DVBNormal = 0x10,

        /// <summary>
        /// DVB, Bildschirmverhältnis 4:3.
        /// </summary>
        DVBRatio_4_3 = 0x11,

        /// <summary>
        /// DVB, Bildschirmverhältnis 16:9.
        /// </summary>
        DVBRatio_16_9 = 0x12,

        /// <summary>
        /// DVB, Bildschirmverhältnis 2,21:1.
        /// </summary>
        DVBRatio_221_100 = 0x13,

        /// <summary>
        /// DVB für Hörgeschädigte, kein Bildschirmverhältnis festgelegt.
        /// </summary>
        DVBImpairedNormal = 0x20,

        /// <summary>
        /// DVB für Hörgeschädigte, Bildschirmverhältnis 4:3.
        /// </summary>
        DVBImpairedRatio_4_3 = 0x21,

        /// <summary>
        /// DVB für Hörgeschädigte, Bildschirmverhältnis 16:9.
        /// </summary>
        DVBImpairedRatio_16_9 = 0x22,

        /// <summary>
        /// DVB für Hörgeschädigte, Bildschirmverhältnis 2,21:1.
        /// </summary>
        DVBImpairedRatio_221_100 = 0x23
    }

    /// <summary>
    /// Enthält die Informationen zu einem Satz DVB Untertitel eines Senders.
    /// </summary>
    public class SubtitleInfo
    {
        /// <summary>
        /// Die zugehörige Sprache.
        /// </summary>
        public readonly string ISOLanguage;

        /// <summary>
        /// Die primäre Seite.
        /// </summary>
        public readonly ushort CompositionPage;

        /// <summary>
        /// Die zusätzliche Seite.
        /// </summary>
        public readonly ushort AncillaryPage;

        /// <summary>
        /// Die konkrete Art der Untertitel.
        /// </summary>
        public readonly SubtitleTypes Type;

        /// <summary>
        /// Erzeugt eine neue Informationsinstanz.
        /// <param name="language">Die Sprache der Untertitel.</param>
        /// <param name="type">Die Art der Untertitel.</param>
        /// <param name="compositionPage">Die primäre Seite.</param>
        /// <param name="ancillaryPage">Die zusätzliche Seite.</param>
        /// </summary>
        public SubtitleInfo( string language, SubtitleTypes type, ushort compositionPage, ushort ancillaryPage )
        {
            // Remember
            CompositionPage = compositionPage;
            AncillaryPage = ancillaryPage;
            ISOLanguage = language;
            Type = type;
        }

        /// <summary>
        /// Erzeugt eine neue Informationsinstanz.
        /// </summary>
        /// <param name="section">Die SI Tabelle, in der die Informationen abgelegt sind.</param>
        /// <param name="offset">Das erste Byte, das Informationen zu Untertiteln enthält.</param>
        private SubtitleInfo( Section section, int offset )
        {
            // Load the language
            ISOLanguage = section.ReadString( offset, 3 );

            // Load the type
            Type = (SubtitleTypes) section[offset + 3];

            // Load pages
            CompositionPage = Tools.MergeBytesToWord( section[offset + 5], section[offset + 4] );
            AncillaryPage = Tools.MergeBytesToWord( section[offset + 7], section[offset + 6] );
        }

        /// <summary>
        /// Meldet die Größe dieser Informationsinstanz.
        /// </summary>
        public int Length
        {
            get
            {
                // Report static size
                return 8;
            }
        }

        /// <summary>
        /// Erzeugt eine neue Informationsinstanz.
        /// </summary>
        /// <param name="section">Die SI Tabelle, in der die Informationen abgelegt sind.</param>
        /// <param name="offset">Das erste Byte, das Informationen zu Untertiteln enthält.</param>
        /// <param name="length">Die Anzahl der Bytes, die für die Beschreibung zur Verfügung stehen.</param>
        /// <returns>Die Informationsinstanz oder <i>null</i>, wenn keine aus den Daten erstellt
        /// werden konnte.</returns>
        internal static SubtitleInfo Create( Section section, int offset, int length )
        {
            // Test for length
            if (length < 8) return null;

            // Create
            return new SubtitleInfo( section, offset );
        }

        /// <summary>
        /// Rekonstuiert die Daten für diese Informationsinstanz.
        /// </summary>
        /// <param name="buffer">Hilfskomponente zum Aufbau der Beschreibung.</param>
        internal void CreatePayload( TableConstructor buffer )
        {
            // Append language
            buffer.AddLanguage( ISOLanguage );

            // Append type
            buffer.Add( (byte) Type );

            // Append pages
            buffer.Add( CompositionPage );
            buffer.Add( AncillaryPage );
        }

        /// <summary>
        /// Meldet den Namen der Sprache in der jeweiligen Landessprache.
        /// </summary>
        public string Language
        {
            get
            {
                // Not possible
                if (string.IsNullOrEmpty( ISOLanguage ))
                    return ISOLanguage;

                // Try conversion
                return ProgramEntry.GetLanguageFromISOLanguage( ISOLanguage );
            }
        }
    }
}
