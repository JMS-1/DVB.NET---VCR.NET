using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Tables
{
    /// <summary>
    /// Diese Tabelle beschreibt Sendungen auf Direktkanälen. Es handelt sich um ein 
    /// poperitäres Format des deutschen PayTV Anbieters PREMIERE.
    /// </summary>
	public class CITPremiere : Table
	{
        /// <summary>
        /// Eine eindeutige Kennung zu diesem Eintrag.
        /// </summary>
        public uint Identifier;

        /// <summary>
        /// Dauer der Sendung in Sekunden.
        /// </summary>
        public TimeSpan Duration;

        /// <summary>
        /// Descriptors for this program.
        /// </summary>
        public readonly Descriptor[] Descriptors;

        /// <summary>
        /// Meldet, ob es sich bei einer Tabelle um eine <i>PREMIERE Content
        /// Information Table</i> handelt.
        /// </summary>
        /// <param name="tableIdentifier">Die zu prüfende Tabellenkennung.</param>
        /// <returns>Gesetzt, wenn die Tabellenkennung <i>160 (0xA0)</i> ist.</returns>
		public static bool IsHandlerFor(byte tableIdentifier)
		{
			// Check all
			return (0xa0 == tableIdentifier);
		}

        /// <summary>
        /// Erzeugt eine neue CIT.
        /// </summary>
        /// <param name="section">Die zugehörige gesamte SI-Tabelle inklusive Kopfdaten und
        /// Prüfsumme.</param>
        public CITPremiere(Section section)
			: base(section)
		{
            // Load length
            int length = section.Length - 7;

            // Check for minimum length required
            if (length < 14) return;

            // Adjust
            length -= 14;

            // Read the identifier
            uint id3 = section[5], id2 = section[6], id1 = section[7], id0 = section[8];

            // Constrict the identifier
            Identifier = id0 + 256 * (id1 + 256 * (id2 + 256 * id3));

            // Read the duration
            Duration = Tools.DecodeDuration(section, 9);

            // Length of descriptors
            int infoLength = 0xfff & Tools.MergeBytesToWord(Section[13], Section[12]);

            // Validate
            if (length < infoLength) return;

            // Create my descriptors
            Descriptors = Descriptor.Load(this, 14, infoLength);
            
            // Done
			m_IsValid = true;
		}
	}
}
