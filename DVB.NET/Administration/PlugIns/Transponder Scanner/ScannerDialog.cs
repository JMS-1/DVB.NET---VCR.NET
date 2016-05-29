using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using JMS.DVB.Algorithms;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace JMS.DVB.Administration.SourceScanner
{
    /// <summary>
    /// Die Steuerung des Sendersuchlaufs.
    /// </summary>
    public partial class ScannerDialog : UserControl, IPlugInControl
    {
        /// <summary>
        /// Die zugehörige Erweiterung.
        /// </summary>
        private Scanner m_PlugIn;

        /// <summary>
        /// Eine Methode, mit der das Administrationswerkzeug zur
        /// Aktualisierung der Schaltflächen aufgefordert werden kann.
        /// </summary>
        private IPlugInUISite m_Site;

        /// <summary>
        /// Die Steuerung des aktuellen Suchlaufs.
        /// </summary>
        private TransponderScanner m_Scanner;

        /// <summary>
        /// Erzeugt eine neue Steuerung.
        /// </summary>
        /// <param name="scanner">Die Konfiguration für diesen Suchlauf.</param>
        /// <param name="site">Die Arbeitsumgebung der Erweiterung - üblicherweise das DVB.NET
        /// Administrationswerkzeug.</param>
        public ScannerDialog(Scanner scanner, IPlugInUISite site)
        {
            // Remember
            m_PlugIn = scanner;
            m_Site = site;

            // Fill data from designer
            InitializeComponent();

            // Finish debug helper
            lbFound.Text = null;
        }

        /// <summary>
        /// Bereits die Benutzerschnittstelle vor.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ScannerDialog_Load(object sender, EventArgs e)
        {
            // Set up the name of the selected profile
            lbProfile.Text = string.Format(lbProfile.Text, m_PlugIn.Profile.Name);
        }

        /// <summary>
        /// Wird in einem festen Interval aufgerufen und prüft, ob die Operation bereits abgeschlossen ist
        /// oder abgebrochen werden soll.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void tickEnd_Tick(object sender, EventArgs e)
        {
            // See if scanner is done
            if (!m_Scanner.Done)
                return;

            // Stop timer
            tickEnd.Enabled = false;

            // Final update
            UpdateCounter(null);

            // Prepare to terminate
            using (TransponderScanner scanner = m_Scanner)
            {
                // Forget
                m_Scanner = null;

                // Must finish
                try
                {
                    // Check for error
                    var error = scanner.Error;
                    if (error != null)
                        MessageBox.Show(this, error.ToString(), Properties.Resources.Scanner_Error, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    // Create question
                    string quest = string.Format(Properties.Resources.Save_Profile, scanner.Profile.Name);

                    // Ask user
                    DialogResult saveMode = MessageBox.Show(this, quest, Properties.Resources.Save_Profile_Titel, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                    // Profile
                    if (DialogResult.Cancel != saveMode)
                    {
                        // Check for reset
                        if (!ckMerge.Checked)
                            scanner.Profile.Locations.Clear();

                        // Update in memory
                        scanner.UpdateProfile();

                        // Save to disk
                        scanner.Profile.Save();
                    }

                    // Protocol
                    if (DialogResult.Yes == saveMode)
                        if (DialogResult.OK == saveProtocol.ShowDialog(this))
                            using (StreamWriter protocol = new StreamWriter(saveProtocol.FileName, false, Encoding.Unicode))
                            {
                                // Head line
                                protocol.WriteLine(ProtocolRecord.LineFormat);

                                // All lines
                                foreach (ProtocolRecord record in scanner.Protocol)
                                    protocol.WriteLine(record);
                            }
                }
                finally
                {
                    // Report to site
                    m_Site.OperationDone();
                }
            }
        }

        #region IPlugInControl Members

        /// <summary>
        /// Aktualisiert die Zähler der Quellen.
        /// </summary>
        /// <param name="hint">Detailinformationen zur aktuellen Position.</param>
        /// <returns>Gesetzt, wenn der aktuelle Suchlauf fortgesetzt werden darf.</returns>
        private bool UpdateCounter(string hint)
        {
            // Call self
            if (InvokeRequired)
                return (bool)Invoke(new Func<string, bool>(UpdateCounter), hint);

            // Private counters
            Dictionary<SourceTypes, int> found = new Dictionary<SourceTypes, int>();

            // Preset
            found[SourceTypes.Unknown] = 0;
            found[SourceTypes.Radio] = 0;
            found[SourceTypes.TV] = 0;

            // Total
            int total = 0;

            // Fill counters
            foreach (KeyValuePair<SourceTypes, int> source in m_Scanner.SourcesFound)
            {
                // General
                total += source.Value;

                // This type
                int sum;
                if (!found.TryGetValue(source.Key, out sum))
                    sum = 0;

                // Update
                found[source.Key] = sum + source.Value;
            }

            // Set it
            lbFound.Text = string.Format(Properties.Resources.Report, total, found[SourceTypes.TV], found[SourceTypes.Radio], found[SourceTypes.Unknown], "\r\n\r\n        " + hint);

            // Try to get a new progress value
            decimal newProgress = 0;

            // See if we are already on
            if (m_Scanner.TotalLocations > 0)
            {
                // Get the fraction per location
                decimal step = 1000m / m_Scanner.TotalLocations;

                // Load the bias
                newProgress = (m_Scanner.CurrentLocation - 1) * step;

                // Get the total number of groups
                int totalGroups = m_Scanner.CurrentLocationGroup + m_Scanner.CurrentLocationGroupsPending;

                // Be safe
                if (totalGroups > 0)
                    newProgress += step * m_Scanner.CurrentLocationGroup / totalGroups;
            }

            // Be safe
            try
            {
                // Update
                prgProgress.Value = (int)newProgress;
            }
            catch
            {
                // Ignore all
                prgProgress.Value = 0;
            }

            // Report
            return !m_Site.HasBeenCancelled;
        }

        /// <summary>
        /// Beginnt mit der Ausführung der Aufgabe.
        /// </summary>
        /// <returns>Gesetzt, wenn die Aufgabe synchron abgeschlossen wurde.</returns>
        bool IPlugInControl.Start()
        {
            // Create scanner
            m_Scanner = new TransponderScanner(m_PlugIn.Profile);

            // Connect all events
            m_Scanner.OnStartGroup += (l, g, s) => UpdateCounter(string.Format("{0} {1}", g, l));
            m_Scanner.OnStartLocation += (l, s) => UpdateCounter(null);
            m_Scanner.OnDoneGroup += (l, g, s) => UpdateCounter(null);
            m_Scanner.OnDoneLocation += (l, s) => UpdateCounter(null);

            // Start with counter
            UpdateCounter(null);

            // Start the scan
            m_Scanner.Scan();

            // Start the timer
            tickEnd.Enabled = true;

            // Runs in background
            return false;
        }

        /// <summary>
        /// Meldet, ob eine Aufgabe unterbrochen werden kann.
        /// </summary>
        bool IPlugInControl.CanCancel
        {
            get
            {
                // We can
                return true;
            }
        }

        /// <summary>
        /// Prüft, ob eine Ausführung möglich ist.
        /// </summary>
        /// <returns>Gesetzt, wenn <see cref="IPlugInControl.Start"/> aufgerufen werden darf.</returns>
        bool IPlugInControl.TestStart()
        {
            // Yes, we can
            return true;
        }

        #endregion
    }
}
