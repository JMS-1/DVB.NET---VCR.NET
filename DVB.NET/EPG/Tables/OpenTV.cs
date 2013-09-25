using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Tables
{
    /// <summary>
    /// Beschreibt eine OpenTV Tabelle, die unter anderem für die Programmzeitschrift
    /// der englischen Sender BBC und ITV verwendet wird.
    /// </summary>
    public class OpenTV : Table
    {
        /// <summary>
        /// Meldet, ob es sich um eine OpenTV Tabelle handelt.
        /// </summary>
        /// <param name="tableIdentifier">Die erkannte Tabellenkennung.</param>
        /// <returns>Gesetzt, wenn eine OpenTV Tabellenkennung erkannt wurde.</returns>
        public static bool IsHandlerFor(byte tableIdentifier)
        {
            // Check all
            return ((0x9c == tableIdentifier) || (0xb5 == tableIdentifier) || (0xb6 == tableIdentifier) || (0xc4 == tableIdentifier));
        }

        /// <summary>
        /// Die eindeutige Kennung des zugehörigen Moduls.
        /// </summary>
        public ushort ModuleIdentifier;

        /// <summary>
        /// Versionsnummer zu diesem Modul.
        /// </summary>
        public uint OpenTVVersion;

        /// <summary>
        /// Eindeutige Kennung des Abschnitts.
        /// </summary>
        public uint SectionIdentifier;

        /// <summary>
        /// Erstes Byte für diesn Abschnitt.
        /// </summary>
        public uint SectionOffset;

        /// <summary>
        /// Gesamtlänge dieses Moduls.
        /// </summary>
        public uint ModuleLength;

        /// <summary>
        /// Anzahl der Nutzdaten in Bytes.
        /// </summary>
        public readonly uint DataLength;

        /// <summary>
        /// Erzeugt eine neue OpenTV Tabelle.
        /// </summary>
        /// <param name="section">Der zugehörige Bereich.</param>
        public OpenTV(Section section)
            : base(section)
        {
            // Get the inner length
            DataLength = (uint)(section.Length - 7 - 5 - 16);

            // Not enough space
            if (DataLength < 0) return;

            // Get parts
            ModuleIdentifier = Tools.MergeBytesToWord(section[1], section[0]);
            OpenTVVersion = Tools.MergeBytesToDoubleWord(section[8], section[7], section[6], section[5]);
            SectionIdentifier = Tools.MergeBytesToDoubleWord(section[12], section[11], section[10], section[9]);
            SectionOffset = Tools.MergeBytesToDoubleWord(section[16], section[15], section[14], section[13]);
            ModuleLength = Tools.MergeBytesToDoubleWord(section[20], section[19], section[18], section[17]);

            // Done
            m_IsValid = true;
        }

        /// <summary>
        /// Kopiert die Nutzdaten in ein Feld.
        /// </summary>
        /// <param name="target">Das gewünschte Zielfeld.</param>
        /// <param name="offset">Erstes Byte im Zielfeld, das beschrieben werden soll.</param>
        public void CopyTo(byte[] target, int offset)
        {
            // Process
            Section.CopyBytes(21, target, offset, (int)DataLength);
        }
    }
}
