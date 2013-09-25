using System;
using System.Xml;
using System.Windows.Forms;
using System.Xml.Serialization;


namespace JMS.DVB.DirectShow.RawDevices
{
    /// <summary>
    /// Beschreibt eine einzelne Eingabeinformation.
    /// </summary>
    [Serializable]
    public abstract class MappingItem
    {
        /// <summary>
        /// Die eigentliche Eingabeinformation in Rohform.
        /// </summary>
        [XmlText]
        public string Value { get; set; }

        /// <summary>
        /// Initialisiert eine Beschreibung.
        /// </summary>
        protected MappingItem()
        {
        }

        /// <summary>
        /// Ein Wert, wie er für Vergleiche genutzt werden kann.
        /// </summary>
        protected abstract int NumericValue { get; }

        /// <summary>
        /// Meldet einen Kurzschlüssel für die Eingabeinformation.
        /// </summary>
        /// <returns>Der gewünschte Schlüssel.</returns>
        public override int GetHashCode()
        {
            // Base
            return (GetType().GetHashCode() << 2) ^ NumericValue.GetHashCode();
        }

        /// <summary>
        /// Vergleicht zwei Eingabeinformationen.
        /// </summary>
        /// <param name="obj">Ein beliebiges Objekt.</param>
        /// <returns>Gesetzt, wenn es sich bei dem Parameter um eine äquivalente Eingabeinformation handelt.</returns>
        public override bool Equals( object obj )
        {
            // Direct
            if (ReferenceEquals( this, obj ))
                return true;

            // Change type
            var other = obj as MappingItem;
            if (other == null)
                return false;

            // Process
            if (GetType() != other.GetType())
                return false;
            else
                return (NumericValue == other.NumericValue);
        }

        /// <summary>
        /// Erzeugt einen Anzeigenamen zu Testzwecken.
        /// </summary>
        /// <returns>Der gewünschte Anzeigename.</returns>
        public override string ToString()
        {
            // Process
            return string.Format( "{0}:{1}", GetType().Name, Value );
        }

        /// <summary>
        /// Erstellt eine Beschreibung für Informationen von der Fernbedienung.
        /// </summary>
        /// <param name="code">Der gesendete Code.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public static implicit operator MappingItem( ushort code )
        {
            // Forward
            return new RCMapping { RCKey = code };
        }

        /// <summary>
        /// Erstellt eine Beschreibung für eine Tastatureingabe.
        /// </summary>
        /// <param name="key">Die empfangene Taste.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public static implicit operator MappingItem( Keys key )
        {
            // Forward
            return new KeyMapping { Key = key };
        }
    }
}
