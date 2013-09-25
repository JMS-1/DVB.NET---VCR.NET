using System;
using System.Linq;
using System.Text;
using System.Threading;
using JMS.DVB.Algorithms;
using System.Collections.Generic;

namespace JMS.DVB.CardServer
{
    partial class InMemoryCardServer
    {
        /// <summary>
        /// Beschreibt den aktuellen Fortschritt bei einem Sendersuchlauf.
        /// </summary>
        private volatile int m_ScanProgress = -1;

        /// <summary>
        /// Meldet den aktuellen Fortschritt bei einem Sendersuchlauf.
        /// </summary>
        public int ScanProgress
        {
            get
            {
                // Report
                return m_ScanProgress;
            }
        }

        /// <summary>
        /// Die Steuerung des Sendersuchlaufs.
        /// </summary>
        private TransponderScanner m_Scanner;

        /// <summary>
        /// Gesetzt, wenn der Suchlauf abgebrochen werden soll.
        /// </summary>
        private volatile Action m_ScanAbort;

        /// <summary>
        /// Die bisher gefundenen Quellen.
        /// </summary>
        private volatile int m_ScanSources;

        /// <summary>
        /// Prüft auf einen Abbruch des Sendersuchlaufs.
        /// </summary>
        private void CheckScanAbort()
        {
            // See if scan is aborting
            Action abort = m_ScanAbort;

            // Not pending
            if (null == abort)
                return;

            // Must be fully save
            try
            {
                // See if scanner is done
                if (!m_Scanner.Done)
                    return;

                // Forget
                m_ScanAbort = null;

                // Execute
                abort();
            }
            catch (Exception e)
            {
                // Terminate
                ActionDone( e, null );
            }
        }

        /// <summary>
        /// Beginnt einen Sendersuchlauf auf dem aktuellen Geräteprofil.
        /// </summary>
        protected override void OnStartScan()
        {
            // Process
            Start( device =>
                {
                    // Check mode
                    if (EPGProgress.HasValue)
                        CardServerException.Throw( new EPGActiveFault() );
                    if (m_ScanProgress >= 0)
                        CardServerException.Throw( new SourceUpdateActiveFault() );
                    if (!string.IsNullOrEmpty( Profile.UseSourcesFrom ))
                        CardServerException.Throw( new NoSourceListFault( Profile.Name ) );

                    // Stop all
                    RemoveAll();

                    // Create scanner
                    m_Scanner = new TransponderScanner( Profile );

                    // Configure
                    m_Scanner.OnDoneLocation += ( l, s ) => OnUpdateScan();
                    m_Scanner.OnDoneGroup += ( l, g, s ) => OnUpdateScan();

                    // Mark as started
                    m_ScanProgress = 0;
                    m_ScanSources = 0;

                    // Start it
                    m_Scanner.Scan();
                } );
        }

        /// <summary>
        /// Meldet, dass eine Quellgruppe (Transponder) vollständig untersucht wurde.
        /// </summary>
        /// <returns>Gesetzt, wenn der Suchlauf fortgesetzt werden soll.</returns>
        private bool OnUpdateScan()
        {
            // Attach to the scanner
            TransponderScanner scanner = m_Scanner;

            // Already done?
            if (null == scanner)
                return false;

            // Total
            int total = 0;

            // Fill counters
            foreach (int sources in scanner.SourcesFound.Values)
                total += sources;

            // Remember
            m_ScanSources = total;

            // Try to get a new progress value
            decimal newProgress = 0;

            // See if we are already on
            if (scanner.TotalLocations > 0)
            {
                // Get the fraction per location
                decimal step = 1000m / scanner.TotalLocations;

                // Load the bias
                newProgress = (scanner.CurrentLocation - 1) * step;

                // Get the total number of groups
                int totalGroups = scanner.CurrentLocationGroup + scanner.CurrentLocationGroupsPending;

                // Be safe
                if (totalGroups > 0)
                    newProgress += step * scanner.CurrentLocationGroup / totalGroups;
            }
            else
            {
                // Done when we started
                newProgress = 1000m;
            }

            // Store
            m_ScanProgress = (int) newProgress;

            // See if we are aborting
            return (null == m_ScanAbort);
        }

        /// <summary>
        /// Beendet einen Sendersuchlauf auf dem aktuellen Geräteprofil.
        /// </summary>
        /// <param name="updateProfile">Gesetzt, wenn das Geräteprofil aktualisiert werden soll.</param>
        protected override void OnEndScan( bool? updateProfile )
        {
            // Process
            Start( device =>
                {
                    // Check mode
                    if (m_ScanProgress < 0)
                        CardServerException.Throw( new SourceUpdateNotActiveFault() );
                    if (null != m_ScanAbort)
                        CardServerException.Throw( new SourceUpdateNotActiveFault() );

                    // Install action
                    m_ScanAbort = () =>
                        {
                            // With cleanup
                            using (TransponderScanner scanner = m_Scanner)
                            {
                                // Mark as done
                                m_ScanProgress = -1;
                                m_Scanner = null;

                                // Check mode
                                if (updateProfile.HasValue)
                                {
                                    // Do NOT merge
                                    if (!updateProfile.Value)
                                        scanner.Profile.Locations.Clear();

                                    // Process
                                    scanner.UpdateProfile();

                                    // Save it
                                    scanner.Profile.Save();
                                }
                            }

                            // Report
                            ActionDone( null, null );
                        };

                    // Done
                    return DelayedOperationTag;
                } );
        }
    }
}
