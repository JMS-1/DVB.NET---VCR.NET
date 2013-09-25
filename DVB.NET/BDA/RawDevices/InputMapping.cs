using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;


namespace JMS.DVB.DirectShow.RawDevices
{
    /// <summary>
    /// Beschreibt die Abbildung eines Befehls in der neuen Version.
    /// </summary>
    [Serializable]
    public class InputMapping
    {
        /// <summary>
        /// Der hier definierte Befehl.
        /// </summary>
        [XmlAttribute( "id" )]
        public InputKey Meaning { get; set; }

        /// <summary>
        /// Die Sequenz der Eingabeinformationen, die auf den Befehl abgebildet werden soll.
        /// </summary>
        [XmlElement( typeof( RCMapping ) )]
        [XmlElement( typeof( KeyMapping ) )]
        public readonly List<MappingItem> Items = new List<MappingItem>();

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public InputMapping()
        {
        }

        /// <summary>
        /// Meldet einen Kurzschlüssel für diese Abbildung.
        /// </summary>
        /// <returns>Der gewünschte Schlüssel.</returns>
        public override int GetHashCode()
        {
            // Base
            var hash = Items.Count.GetHashCode();

            // Merge
            foreach (var item in Items)
                if (item != null)
                    hash = (hash << 2) ^ item.GetHashCode();

            // Report
            return hash;
        }

        /// <summary>
        /// Vergleicht zwei Abbildungen.
        /// </summary>
        /// <param name="obj">Ein beliebiges Objekt.</param>
        /// <returns>Gesetzt, wenn der Parameter eine äquivalente Abbildung beschreibt.</returns>
        public override bool Equals( object obj )
        {
            // Direct
            if (ReferenceEquals( this, obj ))
                return true;

            // Change type
            var other = obj as InputMapping;
            if (other == null)
                return false;

            // Pre-test
            if (Items.Count != other.Items.Count)
                return false;

            // All
            for (int i = Items.Count; i-- > 0; )
                if (!Equals( Items[i], other.Items[i] ))
                    return false;

            // Yeah
            return true;
        }
    }
}
