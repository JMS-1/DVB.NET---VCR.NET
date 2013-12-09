using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JMS.DVBVCR.RecordingService.Win32Tools;


namespace JMS.DVBVCR.RecordingService
{
    partial class VCRServer
    {
        /// <summary>
        /// Gesetzt, wenn ein Übergang in den Schlafzustand aussteht.
        /// </summary>
        private volatile bool m_PendingHibernation;

        /// <summary>
        /// Allgemeine Sperre zum Zugriff auf veränderliche globale Eigenschaften.
        /// </summary>
        private object m_HibernateSync = new object();

        /// <summary>
        /// Gesetzt, wenn beim Übergang in den Schlafzustand auf interaktive Anwender geprüft werden soll.
        /// </summary>
        private volatile bool m_checkForInteractiveUserOnHibernationTest = true;

        /// <summary>
        /// Wird beim Aufwecken aus dem Schlafzustand aktiviert.
        /// </summary>
        public void Resume()
        {
            // Forward
            Profiles.Resume();

            // Allow recordings again
            PowerManager.IsSuspended = false;

            // Forward to extension manager
            ExtensionProcessManager.Resume();
        }

        /// <summary>
        /// Teilt dieser VCR.NET Instanz mit, dass ein Übergang in den Schlafzugrang
        /// erfolgt ist.
        /// </summary>
        public void Suspend()
        {
            // Forward
            Profiles.Suspend();

            // Make sure that we do not shut down the system after we wake up
            m_PendingHibernation = false;

            // Make sure that we forget anything on recording after wake up
            VCRConfiguration.Current.HasRecordedSomething = false;

            // Get the final plan respecting the fact that we are now hibernating
            Profiles.EnsureNewPlan();
        }

        /// <summary>
        /// Teilt dieser VCR.NET Instanz mit, dass ein Übergang in den Schlafzugrang
        /// erfolgen wird.
        /// </summary>
        public void PrepareSuspend()
        {
            // Block out any further recordings
            PowerManager.IsSuspended = true;

            // Forward to extension manager
            ExtensionProcessManager.Suspend();

            // Forward to hardware
            Profiles.PrepareSuspend();
        }

        /// <summary>
        /// Setzt diese Instanz in den Anfangszustand bezüglich der Überwachung des Schlafzustands zurück.
        /// </summary>
        public void ResetPendingHibernation()
        {
            // Clear flag
            m_PendingHibernation = false;
        }

        /// <summary>
        /// Teilt der Laufzeitumgebung mit, dass ein Geräteprofil eine Aufzeichnung abgeschlossen hat.
        /// </summary>
        /// <param name="disallowHibernate">Gesetzt, wenn diese Aufzeichnung keinen Übergang in
        /// den Schlafzustand auslösen soll.</param>
        /// <param name="realRecording">Gesetzt, wenn es sich um eine echte Aufzeichnung handelt.</param>
        internal void ReportRecordingDone( bool disallowHibernate, bool realRecording )
        {
            // Flag in volatile part of configuration
            if (realRecording)
                VCRConfiguration.Current.HasRecordedSomething = true;

            // Remember
            m_PendingHibernation = !disallowHibernate;

            // Reset user test flag
            m_checkForInteractiveUserOnHibernationTest = true;
        }

        /// <summary>
        /// Versucht, einen Übergang in den Schlafzustand auszulösen, dabei aber die Existenz interaktiver Anwender
        /// zu ignorieren.
        /// </summary>
        public void TryHibernateIgnoringInteractiveUsers()
        {
            // This is done using the regular power management infrastructure
            using (PowerManager.StartForbidHibernation())
                m_checkForInteractiveUserOnHibernationTest = false;
        }

        /// <summary>
        /// Prüft, ob der Übergang in einen Schlafzustand durchgeführt werden soll.
        /// </summary>
        public void TestForHibernation()
        {
            // Load test flag
            var checkInteractive = m_checkForInteractiveUserOnHibernationTest;

            // Reset test flag
            m_checkForInteractiveUserOnHibernationTest = true;

            // Not allowed
            if (!VCRConfiguration.Current.MayHibernateSystem)
                return;

            // Report
            Tools.ExtendedLogging( "Checking for Suspend{0}", checkInteractive ? string.Empty : " (ignore interactive users)" );

            // Extensions to run and the corresponding enviroment
            var environment = new Dictionary<string, string>();
            var extensions = Enumerable.Empty<FileInfo>();

            // Scope extension management
            try
            {
                // Load list of extensions
                var hibernateWithUserExtensions = Tools.GetExtensions( "HibernateWhenUserIsLoggedOn" );
                var hibernateExtensions = Tools.GetExtensions( "Hibernate" );

                // Cleanup
                lock (m_HibernateSync)
                {
                    // At least one job has to be processed
                    if (checkInteractive)
                        if (!m_PendingHibernation)
                        {
                            // Report
                            Tools.ExtendedLogging( "No Recording executed so far - not suspending" );

                            // Leave
                            return;
                        }

                    // Not allowed at all
                    if (!PowerManager.HibernationAllowed)
                        return;

                    // Next recording time
                    var startOfNextActivity = Profiles.NextRecordingStart;
                    var secondsToNextActivity = (long) (startOfNextActivity.HasValue ? Math.Max( 1, (startOfNextActivity.Value - DateTime.UtcNow).TotalSeconds ) : 0);

                    // Add to map
                    environment["%SecondsToWakeup%"] = (secondsToNextActivity > 0) ? secondsToNextActivity.ToString() : string.Empty;

                    // See if user is active
                    if (checkInteractive)
                        if (LogonManager.HasInteractiveUser)
                        {
                            // See if extensions are present
                            if (hibernateWithUserExtensions.Any())
                            {
                                // Simply run extensions
                                extensions = hibernateWithUserExtensions;

                                // Lock out
                                m_PendingHibernation = false;
                            }
                            else
                            {
                                // Report
                                Log( LoggingLevel.Full, EventLogEntryType.Warning, Properties.Resources.UserActive );
                            }

                            // Skip
                            return;
                        }

                    // See if extensions are present
                    if (hibernateExtensions.Any())
                    {
                        // Just run extensions
                        extensions = hibernateExtensions;

                        // Lock out
                        m_PendingHibernation = false;

                        // Done so far
                        return;
                    }

                    // There will be some job in the future
                    Log( LoggingLevel.Full, Properties.Resources.Hibernating );

                    // Lock out
                    m_PendingHibernation = false;
                }

                // Report
                Tools.ExtendedLogging( "Initiating Suspend" );

                // Process
                if (!Tools.SendSystemToSleep( VCRConfiguration.Current.UseS3ForHibernate ))
                    Log( LoggingLevel.Errors, EventLogEntryType.Warning, Properties.Resources.HibernateFailed );
            }
            finally
            {
                // Run extensions - lock is already released
                foreach (var process in Tools.RunExtensions( extensions, environment ))
                    try
                    {
                        // Try to cleanup
                        process.Dispose();
                    }
                    catch
                    {
                        // Ignore any error
                    }
            }
        }
    }
}
