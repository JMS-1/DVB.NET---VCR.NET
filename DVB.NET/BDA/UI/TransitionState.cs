using System;
using System.Linq;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Collections.Generic;

using JMS.DVB.DirectShow.RawDevices;


namespace JMS.DVB.DirectShow.UI
{
    /// <summary>
    /// Beschreibt einen einzelnen Arbeitszustand.
    /// </summary>
    [Serializable]
    public class TransitionState
    {
        /// <summary>
        /// Der eindeutige Name dieses Zustands.
        /// </summary>
        [XmlAttribute( "id" )]
        public string UniqueIdentifier { get; set; }

        /// <summary>
        /// Die Methode, die beim Betreten des Zustands aufgerufen werden soll.
        /// </summary>
        public string EnterAction { get; set; }

        /// <summary>
        /// Die Methode, die beim Verlassen des Zustands aufgerufen werden soll.
        /// </summary>
        public string ExitAction { get; set; }

        /// <summary>
        /// Beschreibt die Reaktion auf eine Eingabe.
        /// </summary>
        [XmlElement( "Input" )]
        public readonly List<KeyActionList> Actions = new List<KeyActionList>();

        /// <summary>
        /// Die zugehörige Gesamtkonfiguration.
        /// </summary>
        private TransitionConfiguration m_Configuration;

        /// <summary>
        /// Beschreibt zur Laufzeit die Abbildung der Eingaben auf Aktionen.
        /// </summary>
        private Dictionary<InputKey, KeyActionList> m_Actions;

        /// <summary>
        /// Die zugehörige Gesamtkonfiguration.
        /// </summary>
        [XmlIgnore]
        public TransitionConfiguration Configuration
        {
            get
            {
                // Report
                return m_Configuration;
            }
            internal set
            {
                // Time to create mappings
                m_Actions = Actions.ToDictionary( a => a.Key );

                // Connect helper
                if (NumberComposer != null)
                    NumberComposer.State = this;

                // Update
                m_Configuration = value;
            }
        }

        /// <summary>
        /// Die Komponenten zum Zusammenbau von Zahlen.
        /// </summary>
        public NumberComposer NumberComposer { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public TransitionState()
        {
        }

        /// <summary>
        /// Meldet den Namen dieses Zustandes.
        /// </summary>
        /// <returns>Der gewünschte Name.</returns>
        public override string ToString()
        {
            // Create
            return UniqueIdentifier ?? "<unnamed state>";
        }

        /// <summary>
        /// Meldet alle gültigen Eingaben zum aktuellen Zustand.
        /// </summary>
        public IEnumerable<InputKey> ActiveKeys
        {
            get
            {
                // Core
                var keys = m_Actions.Select( p => p.Key );

                // Check for number composer
                var composer = NumberComposer;
                if (composer != null)
                {
                    // Numbers
                    keys = keys.Concat( Enumerable.Range( 0, 10 ).Select( i => (InputKey) ((int) InputKey.Digit0 + i) ) );

                    // Lie a bit
                    if (composer.EndKey.HasValue)
                        keys = keys.Concat( Enumerable.Repeat( composer.EndKey.Value, 1 ) );
                }

                // Report
                return keys;
            }
        }

        /// <summary>
        /// Verwarbeitet eine Eingabe.
        /// </summary>
        /// <param name="key">Die auszuführende Aktion.</param>
        internal void Process( InputKey key )
        {
            // Report
            if (TransitionConfiguration.KeyLogger.Enabled)
                Trace.TraceInformation( Properties.Resources.Trace_LogInput_State, UniqueIdentifier );

            // Check for action
            KeyActionList list;
            if (m_Actions.TryGetValue( key, out list ))
                Configuration.Execute( list );
        }

        /// <summary>
        /// Ermittelt alle Problemstellen in der Konfiguration.
        /// </summary>
        internal IEnumerable<Exception> ValidationExceptions
        {
            get
            {
                // Self
                if (!string.IsNullOrEmpty( EnterAction ))
                    if (Configuration.GetSiteMethod( EnterAction ) == null)
                        yield return new ArgumentException( EnterAction, "EnterAction" );
                if (!string.IsNullOrEmpty( ExitAction ))
                    if (Configuration.GetSiteMethod( ExitAction ) == null)
                        yield return new ArgumentException( ExitAction, "ExitAction" );

                // Number composer
                if (NumberComposer != null)
                    foreach (var error in NumberComposer.ValidationExceptions)
                        yield return error;

                // Action list
                foreach (var error in m_Actions.Values.SelectMany( a => a.GetValidationExceptions( Configuration ) ))
                    yield return error;
            }
        }
    }
}
