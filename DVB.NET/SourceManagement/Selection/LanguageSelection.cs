using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt, welche Sprachen verwendet werden soll.
    /// </summary>
    public enum LanguageModes
    {
        /// <summary>
        /// Es wird eine explizite Auswahl der Sprachen angegeben.
        /// </summary>
        Selection,

        /// <summary>
        /// Nur die primäre Sprache ist von Interesse.
        /// </summary>
        Primary,

        /// <summary>
        /// Alle Sprachvarianten sollen berücksichtigt werden.
        /// </summary>
        All
    }

    /// <summary>
    /// Beschreibt für ein Tonformat, wie dieses auszuwerten ist.
    /// </summary>
    [Serializable]
    public class LanguageSelection : ICloneable
    {
        /// <summary>
        /// Meldet oder legt fest, ob alle Sprachvarianten von Interesse sind. Wenn
        /// nicht, so wird <see cref="Languages"/> verwendet, um eine dedizierte Auswahl
        /// zu treffen.
        /// </summary>
        [XmlAttribute( "mode" )]
        public LanguageModes LanguageMode { get; set; }

        /// <summary>
        /// Liest oder legt fest, welche Sprachvarianten aufgezeichnet werden sollen, wenn
        /// <see cref="LanguageMode"/> nicht alle Varianten bezeichnet.
        /// </summary>
        [XmlElement( "Language" )]
        public readonly List<string> Languages = new List<string>();

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public LanguageSelection()
        {
            // Reset fields
            LanguageMode = LanguageModes.Selection;
        }

        /// <summary>
        /// Erzeugt eine Kopie diese Auswahlbeschreibung.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        public LanguageSelection Clone()
        {
            // Create core
            LanguageSelection clone = new LanguageSelection();

            // Fill
            clone.Languages.AddRange( Languages );
            clone.LanguageMode = LanguageMode;

            // Report
            return clone;
        }

        /// <summary>
        /// Erzeugt einen Anzeigetext für diese Auswahl.
        /// </summary>
        /// <returns>Ein Anzeigetext gemäß der aktuellen Konfiguration.</returns>
        public override string ToString()
        {
            // Simply all
            if (LanguageMode == LanguageModes.All)
                return "all";

            // Simply all
            if (LanguageMode == LanguageModes.Primary)
                return "primary";

            // Simply none
            if (Languages.Count < 1)
                return "none";

            // Report
            return string.Format( "({0})", string.Join( ", ", Languages.ToArray() ) );
        }

        /// <summary>
        /// Meldet, ob eine bestimmte Sprache berücksichtigt werden soll.
        /// </summary>
        /// <param name="language">Der Name der Sprache.</param>
        /// <returns>Gesetzt, wenn die Sprache berücksichtigt werden soll.</returns>
        public bool Contains( string language )
        {
            // Not possible
            if (null == language)
                return false;

            // Always
            if (LanguageMode == LanguageModes.All)
                return true;

            // Test
            if (LanguageMode == LanguageModes.Selection)
                foreach (string enabled in Languages)
                    if (0 == string.Compare( enabled, language, true ))
                        return true;

            // No, forbidden
            return false;
        }

        #region ICloneable Members

        /// <summary>
        /// Erzeugt eine Kopie diese Auswahlbeschreibung.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        object ICloneable.Clone()
        {
            // Forward
            return Clone();
        }

        #endregion
    }
}
