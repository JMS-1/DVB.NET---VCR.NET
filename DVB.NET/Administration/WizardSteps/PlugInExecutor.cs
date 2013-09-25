using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using JMS.DVB.Administration;
using System.Collections.Generic;
using JMS.DVB;

namespace DVBNETAdmin.WizardSteps
{
    /// <summary>
    /// In diesem Arbeitsschritt wird die eigentliche Aufgabe ausgeführt.
    /// </summary>
    public partial class PlugInExecutor : Step
    {
        /// <summary>
        /// Erzeugt eine neue Aufgabe.
        /// </summary>
        public PlugInExecutor()
        {
            // Copy from designer
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
                return MainForm.CurrentPlugIn.DisplayName;
            }
        }

        /// <summary>
        /// Zeigt die Benutzerschnittstelle zur aktuellen Aufgabe an.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void PlugInExecutor_Load( object sender, EventArgs e )
        {
            // Attach to the extension
            PlugIn plugIn = MainForm.CurrentPlugIn;

            // Load profile data from multi profile selector
            string[] profiles = null;

            // Load profile data from single profile selector
            if (null == profiles)
            {
                // Retrieve
                string profile = GetState<string, NewProfileSelector>();

                // Use
                if (null == profile)
                    profiles = new string[0];
                else
                    profiles = new string[] { profile };
            }

            // Add to extension
            plugIn.SelectedNewProfiles = Array.ConvertAll( profiles, p => ProfileManager.FindProfile( p ) );

            // Create the UI 
            Control ui = plugIn.CreateUserInferface( MainForm );

            // Configure
            ui.Dock = DockStyle.Fill;
            ui.Visible = true;

            // Add to us
            Controls.Add( ui );

            // Finish
            CheckButtons();

            // Finish
            MainForm.DisableAutoForward();
        }

        /// <summary>
        /// Meldet, ob der gesamte Arbeitsvorgang abgeschlossen werden kann.
        /// </summary>
        public override bool EnableFinish
        {
            get
            {
                // None
                return MainForm.CurrentPlugIn.IsReady;
            }
        }

        /// <summary>
        /// Meldet die Benutzerschnittstelle der aktuellen Aufgabe.
        /// </summary>
        public IPlugInControl CurrentControl
        {
            get
            {
                // Report
                return (IPlugInControl) Controls[0];
            }
        }

        /// <summary>
        /// Meldet, ob <see cref="StartOperation"/> aufgerufen werden darf.
        /// </summary>
        /// <returns>Gesetzt, wenn ein Aufruf erlaubt ist.</returns>
        public override bool CanStartOperation()
        {
            // Just forward
            return CurrentControl.TestStart();
        }

        /// <summary>
        /// Beginnt mit der Ausführung einer Aufgabe.
        /// </summary>
        /// <returns>Gesetzt, wenn die Aufgabe synchron abgeschlossen wurde.</returns>
        public override bool StartOperation()
        {
            // Just forward
            return CurrentControl.Start();
        }

        /// <summary>
        /// Meldet, ob eine einmal gestartete Operation auch wieder abgebrochen werden kann.
        /// </summary>
        public override bool CanCancel
        {
            get
            {
                // Report
                return CurrentControl.CanCancel;
            }
        }
    }
}
