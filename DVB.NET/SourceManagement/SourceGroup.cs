using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.DVB
{
    /// <summary>
    /// Instanzen dieser Klasse beschreiben eine ganze Gruppe von möglichen Quellen,
    /// die von einer Hardware gleichzeitig angeboten werden können. Sie entsprechend
    /// den <i>Transpondern</i> in der DVB Notation.
    /// </summary>
    [Serializable]
    public abstract class SourceGroup
    {
        /// <summary>
        /// Die Frequenz in kHz, über die diese Gruppe empfangen werden kann.
        /// </summary>
        public uint Frequency { get; set; }

        /// <summary>
        /// Alle Quellen, die über die Gruppe zur Verfügung stehen.
        /// </summary>        
        [XmlArray( "Stations" )]
        [XmlArrayItem( typeof( Station ) )]
        public readonly List<SourceIdentifier> Sources = new List<SourceIdentifier>();

        /// <summary>
        /// Initialisiert eine neue Beschreibung.
        /// </summary>
        protected SourceGroup()
        {
        }

        /// <summary>
        /// Vergleicht diese Quellgruppe (Transponder) mit einer anderen.
        /// </summary>
        /// <param name="group">Die andere Quellgruppe.</param>
        /// <param name="legacy">Gesetzt, wenn ein partieller Vergleich erfolgen soll.</param>
        /// <returns>Gesetzt, wenn die Gruppen identisch sind.</returns>
        protected abstract bool OnCompare( SourceGroup group, bool legacy );

        /// <summary>
        /// Meldet, ob alle benötigten Parameter gesetzt sind.
        /// </summary>
        public abstract bool IsComplete { get; }

        /// <summary>
        /// Vergleicht diese Quellgruppe (Transponder) mit einer anderen.
        /// </summary>
        /// <param name="group">Die andere Quellgruppe.</param>
        /// <returns>Gesetzt, wenn die Gruppen identisch sind.</returns>
        public bool CompareTo( SourceGroup group )
        {
            // Forward
            return CompareTo( group, false );
        }

        /// <summary>
        /// Vergleicht diese Quellgruppe (Transponder) mit einer anderen.
        /// </summary>
        /// <param name="group">Die andere Quellgruppe.</param>
        /// <param name="legacy">Gesetzt, wenn ein partieller Vergleich erfolgen soll.</param>
        /// <returns>Gesetzt, wenn die Gruppen identisch sind.</returns>
        public bool CompareTo( SourceGroup group, bool legacy )
        {
            // By identity
            if (ReferenceEquals( group, null ))
                return false;
            if (ReferenceEquals( group, this ))
                return true;

            // Wrong type
            if (group.GetType() != GetType())
                return false;

            // Forward
            return OnCompare( group, legacy );
        }

        /// <summary>
        /// Wandelt eine Textdarstellung einer Gruppe von Quellen in eine
        /// Gruppeninstanz um.
        /// </summary>
        /// <returns>Die rekonstruierte Instanz.</returns>
        /// <exception cref="FormatException">Es wurde keine gültige Textdarstellung angegeben.</exception>
        public static T FromString<T>( string text ) where T : SourceGroup
        {
            // None
            if (string.IsNullOrEmpty( text ))
                return null;

            // Helper
            T group;

            // Dispatch known types
            if (text.StartsWith( "DVB-C" ))
                group = CableGroup.Parse( text ) as T;
            else if (text.StartsWith( "DVB-T" ))
                group = TerrestrialGroup.Parse( text ) as T;
            else if (text.StartsWith( "DVB-S" ))
                group = SatelliteGroup.Parse( text ) as T;
            else
                group = null;

            // Invalid
            if (null == group)
                throw new FormatException( text );

            // Report
            return group;
        }
    }
}
