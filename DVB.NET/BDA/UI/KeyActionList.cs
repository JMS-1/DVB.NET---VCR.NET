using System;
using System.Xml.Serialization;
using System.Collections.Generic;

using JMS.DVB.DirectShow.RawDevices;


namespace JMS.DVB.DirectShow.UI
{
    /// <summary>
    /// Beschreibt die Reaktion auf eine einzelne Eingabe.
    /// </summary>
    [Serializable]
    public class KeyActionList
    {
        /// <summary>
        /// Beschreibt die zugehörige Eingabe.
        /// </summary>
        [XmlAttribute( "key" )]
        public InputKey Key { get; set; }

        /// <summary>
        /// Beschreibt die zugehörigen Aktionen.
        /// </summary>
        [XmlElement( "Action" )]
        public readonly List<string> Actions = new List<string>();

        /// <summary>
        /// Optional der nächste Zustand nach erfolgreicher Ausführung aller Aktionen.
        /// </summary>
        [XmlAttribute( "state" )]
        public string NextState { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public KeyActionList()
        {
        }

        /// <summary>
        /// Erstellt einen Anzeigetext zu Testzwecken.
        /// </summary>
        /// <returns>Der gewünschte Anzeigetext.</returns>
        public override string ToString()
        {
            // Create
            return string.Format( "{0}=>[{1}]", Key, string.Join( ",", Actions.ToArray() ) );
        }

        /// <summary>
        /// Meldet alle Fehleinträge in der Konfiguration.
        /// </summary>
        /// <param name="configuration">Die zugehörige Konfigration.</param>
        /// <returns>Die gewünschte Liste.</returns>
        internal IEnumerable<Exception> GetValidationExceptions( TransitionConfiguration configuration )
        {
            // State
            if (!string.IsNullOrEmpty( NextState ))
                if (!configuration.HasState( NextState ))
                    yield return new ArgumentException( NextState, "NextState" );

            // All actions
            foreach (var action in Actions)
                if (string.IsNullOrEmpty( action ))
                    yield return new ArgumentNullException( "Actions" );
                else if (configuration.GetSiteMethod( action ) == null)
                    yield return new ArgumentException( action, "Actions" );
        }
    }
}
