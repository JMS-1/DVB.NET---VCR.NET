using System;
using System.Xml.Serialization;

namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt, welche Datenströme einer Quelle verwendet werden soll.
    /// </summary>
    [Serializable]
    public class StreamSelection : ICloneable
    {
        /// <summary>
        /// Legt fest, welche MP2 Sprachvarianten aufgezeichnet werden sollen.
        /// </summary>
        [XmlElement( "MP2" )]
        public LanguageSelection MP2Tracks { get; set; }

        /// <summary>
        /// Legt fest, welche AC3 Sprachvarianten aufgezeichnet werden sollen.
        /// </summary>
        [XmlElement( "AC3" )]
        public LanguageSelection AC3Tracks { get; set; }

        /// <summary>
        /// Legt fest, welche DVB Untertitelsprachvarianten aufgezeichnet werden sollen.
        /// </summary>
        [XmlElement( "DVBSubtitles" )]
        public LanguageSelection SubTitles { get; set; }

        /// <summary>
        /// Meldet oder legt fest, ob das Videotextsignal mit aufgezeichnet werden soll.
        /// </summary>
        [XmlAttribute( "ttx" )]
        public bool Videotext { get; set; }

        /// <summary>
        /// Meldet oder legt fest, ob die elektronische Programmzeitschrift (EPG) eingebettet
        /// werden soll - berücksichtigt werden nur aktuelle und folgende Ausstrahlungen
        /// der zugehörigen Quelle.
        /// </summary>
        [XmlAttribute( "epg" )]
        public bool ProgramGuide { get; set; }

        /// <summary>
        /// Erzeugt eine neue Auswahl.
        /// </summary>
        public StreamSelection()
        {
            // Create helper
            MP2Tracks = new LanguageSelection();
            AC3Tracks = new LanguageSelection();
            SubTitles = new LanguageSelection();
        }

        /// <summary>
        /// Erzeugt eine Kopie dieser Auswahlbeschreibung.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        public StreamSelection Clone()
        {
            // Create core
            StreamSelection clone = new StreamSelection();

            // Fill
            clone.AC3Tracks.Languages.AddRange( AC3Tracks.Languages );
            clone.MP2Tracks.Languages.AddRange( MP2Tracks.Languages );
            clone.SubTitles.Languages.AddRange( SubTitles.Languages );
            clone.AC3Tracks.LanguageMode = AC3Tracks.LanguageMode;
            clone.MP2Tracks.LanguageMode = MP2Tracks.LanguageMode;
            clone.SubTitles.LanguageMode = SubTitles.LanguageMode;
            clone.ProgramGuide = ProgramGuide;
            clone.Videotext = Videotext;

            // Report
            return clone;
        }

        #region ICloneable Members

        /// <summary>
        /// Erzeugt eine Kopie dieser Auswahlbeschreibung.
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
