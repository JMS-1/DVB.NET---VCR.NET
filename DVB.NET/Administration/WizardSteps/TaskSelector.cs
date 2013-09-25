using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using JMS.DVB.Administration;
using System.Collections.Generic;

namespace DVBNETAdmin.WizardSteps
{
    /// <summary>
    /// Die Eingangsschritt der Administration.
    /// </summary>
    public partial class TaskSelector : Step
    {
        /// <summary>
        /// Die aktuell ausgewählte Aufgabe.
        /// </summary>
        private PlugIn CurrentPlugIn;

        /// <summary>
        /// Erzeugt einen neuen Eingangsschritt.
        /// </summary>
        public TaskSelector()
        {
            // Call designer stuff
            InitializeComponent();
        }

        /// <summary>
        /// Meldet die Überschrift für diesen Arbeitsschritt.
        /// </summary>
        public override string Headline
        {
            get
            {
                // Report
                return Properties.Resources.Navigation_SelectTask;
            }
        }

        /// <summary>
        /// Meldet, ob der nächste Arbeitsschritt ausgelöst werden darf.
        /// </summary>
        public override bool EnableNext
        {
            get
            {
                // See if at least one option is selected
                foreach (RadioButton button in grpTasks.Controls)
                    if (button.Checked)
                        return (null != GetNextStep( button.Tag as PlugIn ));

                // No - do not enable
                return false;
            }
        }

        /// <summary>
        /// Ermittelt den nächsten Arbeitsschritt.
        /// </summary>
        /// <param name="selected">Die ausgewählte Erweiterung.</param>
        /// <returns>Der als nächstes auszuführende Arbeitsschritt.</returns>
        private Type GetNextStep( PlugIn selected )
        {
            // See if new profile is required
            if (selected.MaximumNewProfiles > 0)
            {
                // See if we can provide single selection
                if (NewProfileSelector.IsValidFor( selected ))
                    return typeof( NewProfileSelector );

                // No valid selection possible
                return null;
            }

            // Direct jump to extension executor
            return typeof( PlugInExecutor );
        }

        /// <summary>
        /// Bereitet das Hauptfenster mit der Auswahl der möglichen Aufgaben vor.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void TaskSelector_Load( object sender, EventArgs e )
        {
            // All controls to create
            List<RadioButton> buttons = new List<RadioButton>();

            // Get the current selection
            Type selected = CurrentSelection;

            // Process all known plugins
            foreach (PlugIn plugIn in PlugIn.CreateFactories())
            {
                // Create a selection button
                RadioButton button = new RadioButton();

                // Configure it
                button.Enabled = (null != GetNextStep( plugIn ));
                button.Checked = (plugIn.GetType() == selected);
                button.Text = plugIn.DisplayName;
                button.Click += TaskSelected;
                button.AutoSize = true;
                button.Visible = true;
                button.Tag = plugIn;

                // Remember
                buttons.Add( button );
            }

            // Sort
            buttons.Sort( ( l, r ) => { PlugIn left = (PlugIn)l.Tag, right = (PlugIn)r.Tag; return left.DisplayPriority.CompareTo(right.DisplayPriority); });

            // Position for selection
            Point next = new Point( grpTasks.Padding.Horizontal, grpTasks.Font.Height + 2 * grpTasks.Padding.Vertical );

            // Fix positions
            foreach (RadioButton button in buttons)
            {
                // Set position
                button.Location = next;

                // Move to next position
                next.Offset( 0, button.Height + grpTasks.Padding.Vertical );
            }

            // Add to GUI
            grpTasks.Controls.AddRange( buttons.ToArray() );

            // Force selection
            foreach (RadioButton button in buttons)
                if (button.Checked)
                {
                    // Select
                    TaskSelected( button, EventArgs.Empty );

                    // First only
                    break;
                }

            // May forward automatically
            MainForm.ProcessAutoForward();
        }

        /// <summary>
        /// Wird aktiviert, wenn eine Auswahl betätigt wird.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void TaskSelected( object sender, EventArgs e )
        {
            // Attach to selection
            RadioButton button = (RadioButton) sender;

            // Remember the selection
            CurrentSelection = button.Tag.GetType();
            CurrentPlugIn = (PlugIn) button.Tag;

            // Inform parent
            CheckButtons();
        }

        /// <summary>
        /// Meldet oder setzt die aktuelle Auswahl der Aufgabe.
        /// </summary>
        public Type CurrentSelection
        {
            get
            {
                // Forward
                return GetState<Type>();
            }
            set
            {
                // Forward
                SetState( value );
            }
        }

        /// <summary>
        /// Führt den nächsten Arbeitsschritt aus.
        /// </summary>
        public override void NextStep()
        {
            // Just show up the next one
            MainForm.ShowStep( GetNextStep( CurrentPlugIn ), CurrentPlugIn );
        }
    }
}
