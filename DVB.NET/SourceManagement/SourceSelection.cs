using System;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace JMS.DVB
{
    /// <summary>
    /// Diese Klasse beschreibt die eindeutige Auswahl einer Quelle. Neben der
    /// eigentlichen Identifikation <see cref="SourceIdentifier"/> können optional
    /// weitere Beschreibungen ergänzt werden, bis hin zur Festlegung eines
    /// DVB.NET Geräteprofils.
    /// </summary>
    [Serializable]
    [XmlType( "Selection" )]
    public class SourceSelection : ISerializable
    {
        /// <summary>
        /// Ein Anzeigename für diese Auswahl.
        /// </summary>
        [XmlAttribute( "name" )]
        public string DisplayName { get; set; }

        /// <summary>
        /// Der otpional Name des zu verwendenden Geräteprofils.
        /// </summary>
        [XmlIgnore]
        public string ProfileName { get { return m_ProfileName; } set { m_ProfileName = value; } }

        /// <summary>
        /// Der otpional Name des zu verwendenden Geräteprofils.
        /// </summary>
        [NonSerialized]
        private string m_ProfileName;

        /// <summary>
        /// Die ausgewählte Quelle.
        /// </summary>
        [XmlIgnore]
        public SourceIdentifier Source { get { return m_Source; } set { m_Source = value; } }

        /// <summary>
        /// Die ausgewählte Quelle.
        /// </summary>
        [NonSerialized]
        private SourceIdentifier m_Source;

        /// <summary>
        /// Optional die Gruppe, in der die Quelle zu finden sein sollte.
        /// </summary>
        [XmlIgnore]
        public SourceGroup Group { get { return m_Group; } set { m_Group = value; } }

        /// <summary>
        /// Optional die Gruppe, in der die Quelle zu finden sein sollte.
        /// </summary>
        [NonSerialized]
        private SourceGroup m_Group;

        /// <summary>
        /// Optional der Ursprung, in der die Quelle selbst oder die
        /// zugehörige Gruppe zu finden sein sollte.
        /// </summary>
        [XmlIgnore]
        public GroupLocation Location
        {
            get
            {
                // Report as is
                return m_Location;
            }
            set
            {
                // See if this is a null location
                if ((null != value) && value.Equals( null ))
                    m_Location = null;
                else
                    m_Location = value;
            }
        }

        /// <summary>
        /// Optional der Ursprung, in der die Quelle selbst oder die
        /// zugehörige Gruppe zu finden sein sollte.
        /// </summary>
        [NonSerialized]
        private GroupLocation m_Location;

        /// <summary>
        /// Erzeugt eine neue Auswahl.
        /// </summary>
        public SourceSelection()
        {
        }

        /// <summary>
        /// Wird zur Deserialisierung aufgerufen.
        /// </summary>
        /// <param name="info">Die Informationen aus der Serialisierung.</param>
        /// <param name="context">Die aktuelle Aufrufumgebung.</param>        
        protected SourceSelection( SerializationInfo info, StreamingContext context )
        {
            // Read
            DisplayName = info.GetString( "NAME" );
            SelectionKey = info.GetString( "KEY" );
        }

        /// <summary>
        /// Eine Textdarstellung für die Quelle samt den optionalen Parametern.
        /// </summary>
        /// <exception cref="ArgumentNullException">Es wurde keine Textdarstellung angegeben.</exception>
        /// <exception cref="FormatException">Eine Rekonstruktion aus der Textdarstellung war nicht 
        /// möglich.</exception>
        [XmlText]
        public string SelectionKey
        {
            get
            {
                // Just create
                return string.Format( "{0}@{1}@{2}@{3}", SourceIdentifier.ToString( Source ), Group, Location, ProfileName );
            }
            set
            {
                // Not set
                if (string.IsNullOrEmpty( value ))
                    throw new ArgumentNullException( "value" );

                // Split
                string[] parts = value.Split( '@' );
                if (4 != parts.Length)
                    throw new FormatException( value );

                // Load source
                if (!string.IsNullOrEmpty( parts[0] ))
                    Source = SourceIdentifier.Parse( parts[0] );

                // Load the rest
                ProfileName = parts[3];
                Group = SourceGroup.FromString<SourceGroup>( parts[1] );
                Location = GroupLocation.FromString<GroupLocation>( parts[2] );

                // Finish
                if (ProfileName.Length < 1)
                    ProfileName = null;
            }
        }

        /// <summary>
        /// Prüft, ob zwei Quellauswahlen die selbe Quelle bezeichnen.
        /// </summary>
        /// <param name="other">Die andere Quelle.</param>
        /// <param name="transponderOnly">Gesetzt, wenn die eigentliche Quelle bei der Prüfung
        /// nicht berücksichtigt werden soll</param>
        /// <returns>Gesetzt, wenn beide Auswahlen die selbe Quelle bezeichnen.</returns>
        public bool CompareTo( SourceSelection other, bool transponderOnly )
        {
            // Not possible
            if (null == other)
                return false;

            // Profile
            if (0 != string.Compare( ProfileName, other.ProfileName, true ))
                return false;

            // Source
            if (!transponderOnly)
                if (!Equals( Source, other.Source ))
                    return false;

            // Use semantic compare
            if (!Equals( Location, other.Location ))
                return false;

            // Group pre-test
            if ((null == Group) || (null == other.Group))
                return false;

            // Group tame test - more or less frequency only
            return Group.CompareTo( other.Group, true );
        }

        /// <summary>
        /// Prüft, ob zwei Quellauswahlen die selbe Quelle bezeichnen.
        /// </summary>
        /// <param name="other">Die andere Quelle.</param>
        /// <returns>Gesetzt, wenn beide Auswahlen die selbe Quelle bezeichnen.</returns>
        public bool CompareTo( SourceSelection other )
        {
            // Forward
            return CompareTo( other, false );
        }

        /// <summary>
        /// Meldet eine Kombination aus Anzeigename und Dienstbezeichner.
        /// </summary>
        [XmlIgnore]
        public string QualifiedName
        {
            get
            {
                // Merge
                if (null == Source)
                    return string.Format( "{0} {{}}", DisplayName );
                else
                    return string.Format( "{0} {1}", DisplayName, Source.ToStringKey() );
            }
        }

        #region ISerializable Members

        /// <summary>
        /// Wird zur Serialisierung aufgerufen.
        /// </summary>
        /// <param name="info">Die zu befüllenden Informationen.</param>
        /// <param name="context">Die aktuelle Aufrufumgebung.</param>        
        void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
        {
            // Store
            info.AddValue( "KEY", SelectionKey );
            info.AddValue( "NAME", DisplayName );
        }

        #endregion
    }
}
