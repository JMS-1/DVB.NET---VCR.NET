using System;
using System.Xml;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;

using JMS.DVB.DirectShow.RawDevices;


namespace JMS.DVB.DirectShow.UI
{
    /// <summary>
    /// Beschreibt die Konfiguration der Zustandsübergänge.
    /// </summary>
    [Serializable]
    public partial class TransitionConfiguration : IDisposable
    {
        /// <summary>
        /// Konfiguriert die Protokollierung aller Eingaben.
        /// </summary>
        public static readonly BooleanSwitch KeyLogger = new BooleanSwitch( Properties.Resources.Switch_LogInputKeys_Name, Properties.Resources.Switch_LogInputKeys_Description );

        /// <summary>
        /// Die Liste aller Zustände dieser Konfiguration.
        /// </summary>
        [XmlElement( "State" )]
        public readonly List<TransitionState> States = new List<TransitionState>();

        /// <summary>
        /// Der Name des Anfangszustandes.
        /// </summary>
        public string InitialStateIdentifier { get; set; }

        /// <summary>
        /// Alle verwalteten Zustände - wird ausschließlich über <see cref="Load"/>
        /// initialisiert.
        /// </summary>
        private Dictionary<string, TransitionState> m_States;

        /// <summary>
        /// Der aktuelle Zustand - dieser wird erst über <see cref="Reset"/>
        /// erstmalig befüllt und dann geeignet fortgeführt.
        /// </summary>
        private TransitionState m_Current;

        /// <summary>
        /// Die zugehörige Arbeitsumgebung. Diese stellt dynamische Werte und Methoden zur Verfügung.
        /// </summary>
        private Control m_Site;

        /// <summary>
        /// Wird aufgerufen, wenn sich der Zustand verändert hat.
        /// </summary>
        public event Action<TransitionState> OnNewState;

        /// <summary>
        /// Wird aufgerufen, wenn ein Fehler ausgelöst wurde.
        /// </summary>
        public event Action<Exception> OnException;

        /// <summary>
        /// Erzeugt eine neue Konfiguration.
        /// </summary>
        public TransitionConfiguration()
        {
        }

        /// <summary>
        /// Setzt den internen Zustand der Konfiguration zurück.
        /// </summary>
        /// <param name="site">Die zugehörige Arbeitsumgebung.</param>
        public void Reset( Control site )
        {
            // Get rid of timer et al
            Dispose();

            // Report
            if (NumberLogger.Enabled)
                if (m_Composer != null)
                    Trace.TraceInformation( Properties.Resources.Trace_Number_Off );

            // Back to initial state
            m_Current = m_States[InitialStateIdentifier];
            m_Composer = null;
            m_Site = site;

            // Reinstall timer
            if (m_Site != null)
            {
                // Check mode
                if (m_Site.InvokeRequired)
                    throw new NotSupportedException( Properties.Resources.Exception_WrongThread );

                // Create
                m_Timer = new Timer { Interval = 100 };

                // Attach handler
                m_Timer.Tick += OnTimer;

                // Start up
                m_Timer.Enabled = true;
            }

            // Fire
            var onState = OnNewState;
            if (onState != null)
                onState( m_Current );
        }

        /// <summary>
        /// Führt die Aktionen einer Eingabe aus.
        /// </summary>
        /// <param name="list">Die Liste der Aktionen.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Liste angegeben.</exception>
        internal void Execute( KeyActionList list )
        {
            // Validate
            if (list == null)
                throw new ArgumentNullException( "list" );

            // Report
            if (TransitionConfiguration.KeyLogger.Enabled)
                Trace.TraceInformation( Properties.Resources.Trace_LogInput_ActionList, list );

            // Process the list
            foreach (var action in list.Actions)
                if (!CallAction( action ))
                {
                    // Report
                    if (TransitionConfiguration.KeyLogger.Enabled)
                        Trace.TraceInformation( Properties.Resources.Trace_LogInput_Aborted, action );

                    // Done
                    return;
                }

            // Did it
            if (string.IsNullOrEmpty( list.NextState ))
                return;

            // Report
            if (TransitionConfiguration.KeyLogger.Enabled)
                Trace.TraceInformation( Properties.Resources.Trace_LogInput_LeaveState, m_Current );

            // Process leave action
            if (!string.IsNullOrEmpty( m_Current.ExitAction ))
                if (!CallAction( m_Current.ExitAction ))
                {
                    // Report
                    if (TransitionConfiguration.KeyLogger.Enabled)
                        Trace.TraceInformation( Properties.Resources.Trace_LogInput_Aborted, m_Current.ExitAction );

                    // Done
                    return;
                }

            // Find next state
            TransitionState nextState;
            if (!m_States.TryGetValue( list.NextState, out nextState ))
                throw new ArgumentException( string.Format( "{0} {1}", list, list.NextState ), "NextState" );

            // Report
            if (TransitionConfiguration.KeyLogger.Enabled)
                Trace.TraceInformation( Properties.Resources.Trace_LogInput_EnterState, nextState );

            // Process enter action
            if (!string.IsNullOrEmpty( nextState.EnterAction ))
                if (!CallAction( nextState.EnterAction ))
                {
                    // Report
                    if (TransitionConfiguration.KeyLogger.Enabled)
                        Trace.TraceInformation( Properties.Resources.Trace_LogInput_Aborted, nextState.EnterAction );

                    // Re-enter current state
                    if (!string.IsNullOrEmpty( m_Current.EnterAction ))
                        CallAction( m_Current.EnterAction );

                    // Done
                    return;
                }

            // Activate it
            m_Current = nextState;

            // Fire
            var onState = OnNewState;
            if (onState != null)
                onState( m_Current );
        }

        /// <summary>
        /// Verwarbeitet eine Eingabe.
        /// </summary>
        /// <param name="key">Die auszuführende Aktion.</param>
        public void Process( InputKey key )
        {
            // Be safe
            try
            {
                // Attach to the site
                if (m_Site == null)
                    throw new InvalidOperationException( Properties.Resources.Exception_NoSite );

                // Check mode
                if (m_Site.InvokeRequired)
                    throw new NotSupportedException( Properties.Resources.Exception_WrongThread );

                // Check it
                if (KeyLogger.Enabled)
                    Trace.TraceInformation( Properties.Resources.Trace_LogInput_Key, key );

                // Check for error
                if (m_Current == null)
                    throw new InvalidOperationException( Properties.Resources.Exception_NoState );

                // Check for number composer
                var composer = m_Composer;
                m_Composer = null;

                // See if we should activate number composer
                if (composer == null)
                    if (key >= InputKey.Digit0)
                        if (key <= InputKey.Digit9)
                            if ((composer = m_Current.NumberComposer) != null)
                                composer.Reset();

                // Dispatch
                if (composer != null)
                    if (composer.Process( key ))
                        return;

                // Fall back
                m_Current.Process( key );
            }
            catch (Exception e)
            {
                // Forward
                ReportException( e );
            }
        }

        /// <summary>
        /// Meldet eine Fehlersituation. 
        /// </summary>
        /// <param name="exception">Eine beobachtete Ausnahme.</param>
        internal void ReportException( Exception exception )
        {
            // Process
            var onError = OnException;
            if (onError != null)
                onError( exception );
        }

        /// <summary>
        /// Prüft, ob ein bestimmter Zustand bekannt ist.
        /// </summary>
        /// <param name="name">Der Name des Zustands.</param>
        /// <returns>Gesetzt, wenn der Zustand bekannt ist.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Name angegeben.</exception>
        internal bool HasState( string name )
        {
            // Test
            if (string.IsNullOrEmpty( name ))
                throw new ArgumentNullException( "name" );
            else
                return m_States.ContainsKey( name );
        }

        /// <summary>
        /// Ermittelt alle Problemstellen in der Konfiguration.
        /// </summary>
        public IEnumerable<Exception> ValidationExceptions
        {
            get
            {
                // The primary state is not available
                if (string.IsNullOrEmpty( InitialStateIdentifier ))
                    yield return new ArgumentNullException( "InitialStateIdentifier" );
                if (!HasState( InitialStateIdentifier ))
                    yield return new ArgumentException( InitialStateIdentifier, "InitialStateIdentifier" );

                // Forward to all
                foreach (var exception in m_States.Values.SelectMany( s => s.ValidationExceptions ))
                    yield return exception;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        public void Dispose()
        {
            // Get rid of timer
            using (var timer = m_Timer)
            {
                // Forget
                m_Timer = null;

                // Stop
                if (timer != null)
                    timer.Enabled = false;
            }

            // Disconnect
            m_Site = null;
        }

        #endregion
    }
}
