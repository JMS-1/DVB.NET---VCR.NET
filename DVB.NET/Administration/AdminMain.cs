using System;
using JMS.DVB;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.ComponentModel;
using JMS.DVB.Administration;
using DVBNETAdmin.WizardSteps;
using System.Collections.Generic;

namespace DVBNETAdmin
{
    /// <summary>
    /// Das Hauptfenster für die Administration und Konfiguration von DVB.NET.
    /// </summary>
    public partial class AdminMain : Form, IPlugInUISite
    {
        /// <summary>
        /// Für jeden Arbeitsschritt individuelle Zustandsdaten.
        /// </summary>
        public readonly Dictionary<Type, object> States = new Dictionary<Type, object>();

        /// <summary>
        /// Die aktuell ausgewählte Erweiterung.
        /// </summary>
        public PlugIn CurrentPlugIn { get; private set; }

        /// <summary>
        /// Eine Liste aller bisher ausgeführten Schritte.
        /// </summary>
        private Stack<Step> m_Steps = new Stack<Step>();

        /// <summary>
        /// Vermerkt, ob gerade eine Ausgabe ausgeführt wird.
        /// </summary>
        private bool m_Running;

        /// <summary>
        /// Vermerkt, ob eine Aufgabe abgebrochen werden soll.
        /// </summary>
        private bool m_Cancelled;

        /// <summary>
        /// Wird gesetzt, wenn eine Erweiterung nach Beenden eine andere automatisch aufrufen
        /// möchte.
        /// </summary>
        private bool m_AutoForward = false;

        /// <summary>
        /// Erzeugt ein neues Hauptfenster.
        /// </summary>
        public AdminMain()
        {
            // Load preferred profile
            ((IPlugInUISite) this).SelectProfile( UserProfile.ProfileName );

            // Load designer settings
            InitializeComponent();

            // Finalize dynamic settings
            cmdFinish.Text = Properties.Resources.Button_Finish;
        }

        /// <summary>
        /// Zeigt einen Arbeitschritt der Administration an.
        /// </summary>
        /// <typeparam name="T">Die Art des Arbeitsschritts.</typeparam>
        public void ShowStep<T>() where T : Step, new()
        {
            // Forward
            ShowStep<T>( CurrentPlugIn );
        }

        /// <summary>
        /// Zeigt einen Arbeitschritt der Administration an.
        /// </summary>
        /// <typeparam name="T">Die Art des Arbeitsschritts.</typeparam>
        /// <param name="plugIn">Die ausgewählte Erweiterung.</param>
        public void ShowStep<T>( PlugIn plugIn ) where T : Step, new()
        {
            // Forwards
            ShowStep( typeof( T ), plugIn );
        }

        /// <summary>
        /// Zeigt einen Arbeitschritt der Administration an.
        /// </summary>
        /// <param name="stepType">Die Art des Arbeitsschritts.</param>
        /// <param name="plugIn">Die ausgewählte Erweiterung.</param>
        public void ShowStep( Type stepType, PlugIn plugIn )
        {
            // Wipe out current step
            foreach (IDisposable control in pnlWizard.Controls)
                control.Dispose();

            // Reset
            pnlWizard.Controls.Clear();

            // Reset buttons
            cmdPrev.Enabled = (m_Steps.Count > 0);
            cmdFinish.Enabled = false;
            cmdNext.Enabled = false;

            // Remember plug-in
            CurrentPlugIn = plugIn;

            // Create new
            Step step = (Step) Activator.CreateInstance( stepType );

            // Configure
            step.Dock = DockStyle.Fill;
            step.Visible = true;

            // Remember step
            pnlWizard.Controls.Add( step );

            // Load the headline
            lbCurrentPhase.Text = step.Headline;

            // Time to set caption
            if (null == CurrentPlugIn)
                Text = Properties.Resources.Caption_NoPlugIn;
            else
                Text = string.Format( Properties.Resources.Caption_PlugIn, Properties.Resources.Caption_NoPlugIn, CurrentPlugIn.ShortName );

            // Build step sequence
            m_Steps.Push( step );
        }

        /// <summary>
        /// Deaktiviert die automatische Auswahl einer Erweiterung.
        /// </summary>
        public void DisableAutoForward()
        {
            // Nothing to do
            if (!m_AutoForward)
                return;

            // Switch off
            m_AutoForward = false;

            // Disable selection
            States.Remove( typeof( TaskSelector ) );
        }

        /// <summary>
        /// Führt die automatisch Auswahl des nächsten Schritts aus.
        /// </summary>
        public void ProcessAutoForward()
        {
            // Not wanted
            if (!m_AutoForward)
                return;

            // Check mode
            if (cmdNext.Enabled)
            {
                // Process
                cmdNext_Click( cmdNext, EventArgs.Empty );
            }
            else
            {
                // Just disable
                m_AutoForward = false;
            }
        }

        /// <summary>
        /// Startet das Hauptfenster und damit die Administration.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void AdminMain_Load( object sender, EventArgs e )
        {
            // See if there is a task
            string taskArg = Environment.GetCommandLineArgs().FirstOrDefault( a => a.StartsWith( "/task=" ) );

            // Get the type
            if (!string.IsNullOrEmpty( taskArg ))
            {
                // Get the name
                string typeName = taskArg.Substring( 6 );

                // Preload
                foreach (PlugIn plugIn in PlugIn.CreateFactories())
                    if (plugIn.GetType().FullName.Equals( typeName ))
                    {
                        // Activate
                        SelectNextPlugIn( plugIn.GetType() );

                        // Done
                        break;
                    }
            }

            // Startup with selection of task
            ShowStep<TaskSelector>();
        }

        /// <summary>
        /// Ermittelt den aktuellen Arbeitsschritt.
        /// </summary>
        public Step CurrentStep
        {
            get
            {
                // Report
                return (Step) pnlWizard.Controls[0];
            }
        }

        /// <summary>
        /// Prüft, ob die Schaltflächen aktiviert oder deaktiviert werden sollen.
        /// </summary>
        public void CheckButtons()
        {
            // Attach to the current step
            Step step = CurrentStep;

            // Forward
            cmdFinish.Enabled = step.EnableFinish;
            cmdNext.Enabled = step.EnableNext;
        }

        /// <summary>
        /// Wählt den nächsten Arbeitsschritt aus.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdNext_Click( object sender, EventArgs e )
        {
            // Forward
            CurrentStep.NextStep();
        }

        /// <summary>
        /// Zeigt den vorherigen Arbeitsschritt an.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdPrev_Click( object sender, EventArgs e )
        {
            // Discard current
            m_Steps.Pop();

            // Get the step
            Step previous = m_Steps.Pop();

            // Show the one before
            ShowStep( previous.GetType(), (m_Steps.Count < 1) ? null : CurrentPlugIn );
        }

        /// <summary>
        /// Aktiviert eine Aufgabe.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdFinish_Click( object sender, EventArgs e )
        {
            // We are active
            if (m_Running)
            {
                // Not again
                cmdFinish.Enabled = false;

                // Remember
                m_Cancelled = true;

                // Leave
                return;
            }

            // Be safe
            try
            {
                // Are we allowed to do this
                if (!CurrentStep.CanStartOperation())
                    return;

                // Forward
                bool finished = CurrentStep.StartOperation();

                // Set flags
                m_Cancelled = false;
                m_Running = true;

                // We are now runnig
                cmdNext.Enabled = false;
                cmdPrev.Enabled = false;

                // See if we can cancel
                if (CurrentStep.CanCancel)
                    cmdFinish.Text = Properties.Resources.Button_Cancel;
                else
                    cmdFinish.Enabled = false;

                // Auto terminate for synchronous operations
                if (finished)
                    ((IPlugInUISite) this).OperationDone();
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, ex.Message, Text );
            }
        }

        /// <summary>
        /// Prüft, ob das Schliessen der Anwendung gestattet ist.
        /// </summary>
        /// <param name="e">Wird geeignet modifiziert.</param>
        protected override void OnClosing( CancelEventArgs e )
        {
            // Not possible
            if (m_Running)
                e.Cancel = true;

            // Forward
            base.OnClosing( e );
        }

        #region IPlugInUISite Members

        /// <summary>
        /// Meldet, ob der Anwender die Aufgabe vorzeitig beenden will.
        /// </summary>
        bool IPlugInUISite.HasBeenCancelled
        {
            get
            {
                // Just report
                return m_Cancelled;
            }
        }

        /// <summary>
        /// Fordert zur Aktualisierung der Anzeige auf - betroffen sind im Allgemeinen Schaltflächen.
        /// </summary>
        void IPlugInUISite.UpdateGUI()
        {
            // Forward
            CheckButtons();
        }

        /// <summary>
        /// Wird aufgerufen, sobald eine Aufgabe abgeschlossen wurde.
        /// </summary>
        void IPlugInUISite.OperationDone()
        {
            // Reset
            m_Running = false;

            // Backup GUI - just in case
            cmdFinish.Text = Properties.Resources.Button_Finish;

            // Reset steps to the very beginning
            m_Steps.Clear();

            // Startup with selection of task
            ShowStep<TaskSelector>();
        }

        /// <summary>
        /// Wählt ein bestimmtes Geräteprofil für die nächsten Aufgaben.
        /// </summary>
        /// <param name="profileName">Der Name des gewünschten Profils.</param>
        void IPlugInUISite.SelectProfile( string profileName )
        {
            // Just set
            if (string.IsNullOrEmpty( profileName ))
                States.Remove( typeof( NewProfileSelector ) );
            else
                States[typeof( NewProfileSelector )] = profileName;
        }

        /// <summary>
        /// Wählt eine andere Erweiterung zur Ausführung aus.
        /// </summary>
        /// <param name="plugInType">Die Art der gwünschten Erweiterung.</param>
        void IPlugInUISite.SelectNextPlugIn( Type plugInType )
        {
            // Forward
            SelectNextPlugIn( plugInType );
        }

        /// <summary>
        /// Wählt eine andere Erweiterung zur Ausführung aus.
        /// </summary>
        /// <param name="plugInType">Die Art der gwünschten Erweiterung.</param>
        private void SelectNextPlugIn( Type plugInType )
        {
            // Check mode
            if (null == plugInType)
            {
                // Remove
                States.Remove( typeof( TaskSelector ) );

                // Disable
                m_AutoForward = false;
            }
            else
            {
                // First set
                States[typeof( TaskSelector )] = plugInType;

                // Remember
                m_AutoForward = true;
            }
        }

        #endregion
    }
}
