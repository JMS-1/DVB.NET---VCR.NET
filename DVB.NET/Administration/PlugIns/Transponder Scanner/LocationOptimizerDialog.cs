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
    /// Mit diesem Dialog wird die Optimierung der Quellgruppen (Transponder) für den Sendersuchlauf
    /// angesteuert.
    /// </summary>
    public partial class LocationOptimizerDialog : UserControl, IPlugInControl
    {
        /// <summary>
        /// Die zugehörige Erweiterung.
        /// </summary>
        private ScanOptimizer m_PlugIn;

        /// <summary>
        /// Eine Methode, mit der das Administrationswerkzeug zur
        /// Aktualisierung der Schaltflächen aufgefordert werden kann.
        /// </summary>
        private IPlugInUISite m_Site;

        /// <summary>
        /// Die Steuerung der Aktualisierung.
        /// </summary>
        private TransponderScanner m_Scanner;

        /// <summary>
        /// Erzeugt eine neue Steuerung.
        /// </summary>
        /// <param name="scanner">Die Konfiguration für diese Optimierung.</param>
        /// <param name="site">Die Arbeitsumgebung der Erweiterung - üblicherweise das DVB.NET
        /// Administrationswerkzeug.</param>
        public LocationOptimizerDialog( ScanOptimizer scanner, IPlugInUISite site )
        {
            // Remember
            m_PlugIn = scanner;
            m_Site = site;

            // Overload designer settings
            InitializeComponent();

            // Finish debug helper
            lbFound.Text = null;
        }

        /// <summary>
        /// Wird während der Ausführung periodisch aufgerufen.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ticker_Tick( object sender, EventArgs e )
        {
            // See if scanner is done
            if (!m_Scanner.Done)
                return;

            // Stop timer
            ticker.Enabled = false;

            // Final update
            UpdateCounter( null );

            // Prepare to terminate
            using (TransponderScanner scanner = m_Scanner)
            {
                // Forget
                m_Scanner = null;

                // Must finish
                try
                {
                    // Process all scan locations found
                    foreach (ScanLocation location in scanner.ScanLocations)
                    {
                        // Configure
                        if (!string.IsNullOrEmpty( location.UniqueName ))
                            dlgSave.FileName = location.UniqueName + "." + dlgSave.DefaultExt;

                        // Ask user what to do
                        if (DialogResult.OK != dlgSave.ShowDialog( this ))
                            continue;

                        // Update inner name
                        if (string.IsNullOrEmpty( location.DisplayName ))
                            location.DisplayName = Path.GetFileNameWithoutExtension( dlgSave.FileName );

                        // Create wrapper
                        ScanLocations save = new ScanLocations();

                        // Add this one
                        save.Locations.Add( location );

                        // Save to disk
                        save.Save( dlgSave.FileName );
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
        /// Aktualisiert den aktuellen Zustand.
        /// </summary>
        /// <param name="hint">Detailinformationen zur aktuellen Position.</param>
        /// <returns>Gesetzt, wenn der aktuelle Suchlauf fortgesetzt werden darf.</returns>
        private bool UpdateCounter( string hint )
        {
            // Call self
            if (InvokeRequired)
                return (bool) Invoke( new Func<string, bool>( UpdateCounter ), hint );

            // Set it
            lbFound.Text = hint;

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
                prgProgress.Value = (int) newProgress;
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
        /// Meldet, ob dieser Vorgang abgebrochen werden kann.
        /// </summary>
        bool IPlugInControl.CanCancel
        {
            get
            {
                // Yes, we can
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

        /// <summary>
        /// Beginnt mit der Ausführung dieser Aufgabe.
        /// </summary>
        /// <returns>Gesetzt, wenn die Aufgabe abgeschlossen werden konnte.</returns>
        bool IPlugInControl.Start()
        {
            // Create scanner
            m_Scanner = new TransponderScanner( m_PlugIn.Profile );

            // Connect all events
            m_Scanner.OnStartGroup += ( l, g, s ) => UpdateCounter( string.Format( "{0} {1}", g, l ) );
            m_Scanner.OnStartLocation += ( l, s ) => UpdateCounter( null );
            m_Scanner.OnDoneGroup += ( l, g, s ) => UpdateCounter( null );
            m_Scanner.OnDoneLocation += ( l, s ) => UpdateCounter( null );

            // Start with counter
            UpdateCounter( null );

            // Start the scan
            m_Scanner.Analyse();

            // Start the timer
            ticker.Enabled = true;

            // Runs in background
            return false;
        }

        #endregion
    }
}
