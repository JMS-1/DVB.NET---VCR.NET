using System;
using System.Xml.Serialization;

namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt einen einzelnen Sender.
    /// </summary>
    [Serializable]
    public class Station : SourceIdentifier
    {
        /// <summary>
        /// Der Kurzname des Senders.
        /// </summary>
        [XmlAttribute( "name" )]
        public string Name { get; set; }

        /// <summary>
        /// Der Name des Dienstanbieters
        /// </summary>
        [XmlAttribute( "provider" )]
        public string Provider { get; set; }

        /// <summary>
        /// Gesetzt, wenn dieser Sender verschlüsselt ist.
        /// </summary>
        [XmlAttribute( "scrambled" )]
        public bool IsEncrypted { get; set; }

        /// <summary>
        /// Meldet oder legt fest, ob es sich um einen Dienstkanal ist.
        /// </summary>
        [XmlAttribute( "service" )]
        public bool IsService { get; set; }

        /// <summary>
        /// Liest oder setzt die Art der Quelle.
        /// </summary>
        [XmlAttribute( "type" )]
        public SourceTypes SourceType { get; set; }

        /// <summary>
        /// Erzeugt einen neuen Sender.
        /// </summary>
        public Station()
        {
        }

        /// <summary>
        /// Erzeugt eine exakte Kopie eines Senders.
        /// </summary>
        /// <param name="other">Der zu kopierende Sender.</param>
        public Station( Station other )
            : base( other )
        {
            // Copy over
            IsEncrypted = other.IsEncrypted;
            SourceType = other.SourceType;
            IsService = other.IsService;
            Provider = other.Provider;
            Name = other.Name;
        }

        /// <summary>
        /// Der volle Name des Senders, zusammengesetzt aus Kurzname <see cref="Name"/>
        /// und Name des Dienstanbieters <see cref="Provider"/>.
        /// </summary>
        [XmlIgnore]
        public string FullName
        {
            get
            {
                // Create
                return string.Format( "{0} [{1}]", Name, Provider );
            }
        }

        /// <summary>
        /// Meldet einen Anzeigenamen für diesen Sender.
        /// </summary>
        /// <returns>Der Anzeigename des Senders.</returns>
        public override string ToString()
        {
            // Create
            return string.Format( "{2} {0} {1}", FullName, base.ToString(), SourceType );
        }

        /// <summary>
        /// Meldet eine Kombination aus Anzeigename <see cref="FullName"/> und Dienstbezeichner.
        /// </summary>
        [XmlIgnore]
        public string QualifiedName
        {
            get
            {
                // Merge
                return string.Format( "{0} [{1}] {2}", Name, Provider, ToStringKey() );
            }
        }
    }
}
