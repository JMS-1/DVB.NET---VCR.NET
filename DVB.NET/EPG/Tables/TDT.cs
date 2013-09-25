using System;
using System.Collections;

namespace JMS.DVB.EPG.Tables
{
    /// <summary>
    /// Beschreibt eine <i>Time and Date Table</i>, mit der Uhrzeit im UTC / GMT
    /// Format.
    /// </summary>
    public class TDT : Table
    {
        /// <summary>
        /// Datum und Uhrzeit zu dieser Tabelle.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Meldet, für welche Tabellenkennungen diese Klasse zuständig ist.
        /// </summary>
        /// <param name="tableIdentifier">Eine zu prüfende Tabellenkennung.</param>
        /// <returns>Gesetzt, wenn diese Klasse die Tabelle analysieren kann.</returns>
        public static bool IsHandlerFor( byte tableIdentifier )
        {
            // Check all
            return (0x70 == tableIdentifier);
        }

        /// <summary>
        /// Erzeugt eine neue Tabelle.
        /// </summary>
        /// <param name="section">Der Bereich, in den die Tabelle eingebettet ist.</param>
        public TDT( Section section )
            : base( section )
        {
            // Load length
            int length = section.Length - 3;

            // Check for minimum length required
            if (length != 5)
                return;

            // Load the time
            Time = Tools.DecodeTime( section, 0 );

            // Done
            m_IsValid = true;
        }
    }
}
