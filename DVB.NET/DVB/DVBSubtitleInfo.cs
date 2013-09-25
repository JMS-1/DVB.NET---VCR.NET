using System;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt eine Art von DVB Untertiteln.
    /// </summary>
    [Serializable]
    public class DVBSubtitleInfo
    {
        /// <summary>
        /// Die Datenstromkennung der Untertitelbilder.
        /// </summary>
        private ushort m_PID;

        /// <summary>
        /// Liest oder setzt die Datenstromkennung der Untertitelbilder.
        /// </summary>
        [XmlAttribute]
        public ushort PID
        {
            get { return m_PID; }
            set { m_PID = value; }
        }

        // Die Sprachinformtion zu diesen Untertiteln.
        private string m_Language;

        /// <summary>
        /// Liest oder setzt die Sprachinformationen zu diesen Untertiteln.
        /// </summary>
        public string Language
        {
            get { return m_Language; }
            set { m_Language = value; }
        }

        /// <summary>
        /// Die Art der Untertiteldarstellung wie in der SI Tabelle festgehalten.
        /// </summary>
        private byte m_Type;

        /// <summary>
        /// Liest oder setzt die Untertitelsarstellung gemäß der SI Tabellennotation.
        /// </summary>
        [XmlAttribute]
        public byte Type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

        /// <summary>
        /// Die Seitennummer.
        /// </summary>
        private ushort m_Page;

        /// <summary>
        /// Liest oder setzt die Seitennummer.
        /// </summary>
        [XmlElement("CompositionPage")]
        public ushort Page
        {
            get { return m_Page; }
            set { m_Page = value; }
        }

        /// <summary>
        /// Die zusätzliche Seitennummer.
        /// </summary>
        private ushort m_AncPage;

        /// <summary>
        /// Liest oder setzt die zusätzliche Seitennummer.
        /// </summary>
        [XmlElement("AncillaryPage")]
        public ushort AncillaryPage
        {
            get { return m_AncPage; }
            set { m_AncPage = value; }
        }

        /// <summary>
        /// Erzeugt eine neue Untertitelinformation.
        /// </summary>
        public DVBSubtitleInfo()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Untertitelinformation.
        /// </summary>
        /// <param name="pid">Die zugehörige Datenstromkennung.</param>
        /// <param name="language">Die Sprachinformation zu diesen Untertiteln.</param>
        /// <param name="type">Die Darstellungart der Untertitel, wie in den SI Tabellen festgehalten.</param>
        /// <param name="compositionPage">Die eigentliche Seite.</param>
        /// <param name="ancillaryPage">Die zusätzliche Seite.</param>
        public DVBSubtitleInfo(ushort pid, string language, byte type, ushort compositionPage, ushort ancillaryPage)
        {
            // Remember
            AncillaryPage = ancillaryPage;
            Page = compositionPage;
            Language = language;
            Type = type;
            PID = pid;
        }

        /// <summary>
        /// Ermittelt die normierte Sprachkennung zu diesem Untertitel.
        /// </summary>
        public string ISOLanguage
        {
            get
            {
                // Report
                return Station.FindISOLanguage(Language);
            }
        }
    }
}
