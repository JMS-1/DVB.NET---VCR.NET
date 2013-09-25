using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JMS.DVBVCR.RecordingService.Win32Tools;


namespace JMS.DVBVCR.RecordingService
{
    /// <summary>
    /// Verwaltet Erweiterungen und deren Instanzen.
    /// </summary>
    public class ExtensionManager : MarshalByRefObject
    {
        /// <summary>
        /// Eine leere Liste von Prozessen.
        /// </summary>
        private static readonly Process[] s_noProcesses = { };

        /// <summary>
        /// Alle vom <i>VCR.NET Recording Service</i> gestarteten und noch aktiven Prozesse.
        /// </summary>
        private readonly List<Process> m_activeProcesses = new List<Process>();

        /// <summary>
        /// Gesetzt, sobald der Übergang in den Schlafzustand ansteht.
        /// </summary>
        private volatile bool m_isSuspended;

        /// <summary>
        /// Gesetzt, wenn der Übergang in den Schlafzustand verboten ist.
        /// </summary>
        private IDisposable m_forbidHibernation;

        /// <summary>
        /// Startet Erweiterungen und ergänzt die zugehörigen Prozesse in der Verwaltung.
        /// </summary>
        /// <param name="extensionName">Der Name der Erweiterung.</param>
        /// <param name="environment">Die für die Erweiterung zu verwendenden Umgebungsvariablen.</param>
        public void AddWithCleanup( string extensionName, Dictionary<string, string> environment )
        {
            // Lookup, start and forward
            AddWithCleanup( Tools.RunExtensions( extensionName, environment ).ToArray() );
        }

        /// <summary>
        /// Prüft, ob sich irgendwelchen aktiven Prozesse in der Verwaltung befinden.
        /// </summary>
        public bool HasActiveProcesses
        {
            get
            {
                // Check protected
                lock (m_activeProcesses)
                    return m_activeProcesses.Count > 0;
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn das System aus dem Schlafzustand erwacht ist.
        /// </summary>
        public void Resume()
        {
            // Reset
            m_isSuspended = false;

            // Enforce hibernation lock
            lock (m_activeProcesses)
                if (m_activeProcesses.Count > 0)
                    if (m_forbidHibernation == null)
                        m_forbidHibernation = PowerManager.StartForbidHibernation();
        }

        /// <summary>
        /// Wird aufgerufen, bevor das System in den Schlafzustand geht.
        /// </summary>
        public void Suspend()
        {
            // No longer stopping hibernation
            m_isSuspended = true;

            // Release hibernation lock
            lock (m_activeProcesses)
                using (m_forbidHibernation)
                    m_forbidHibernation = null;
        }

        /// <summary>
        /// Prüft alle ausstehenden Erweiterungen.
        /// </summary>
        public void Cleanup()
        {
            // Forward
            AddWithCleanup( s_noProcesses );
        }

        /// <summary>
        /// Ergänzt eine Liste von Prozessen nachdem alle bereits beendeten Prozesse aus der 
        /// Verwaltung entfernt wurden.
        /// </summary>
        /// <param name="processes">Eine neue Liste von Processen.</param>
        private void AddWithCleanup( Process[] processes )
        {
            // Synchronize
            lock (m_activeProcesses)
            {
                // Add the list
                m_activeProcesses.AddRange( processes );

                // Do the cleanup
                m_activeProcesses.RemoveAll( process =>
                    {
                        // Be aware of any error
                        try
                        {
                            // Check for it
                            if (!process.HasExited)
                                return false;

                            // Forget it not really necessary but speeds up handle cleanup a bit
                            process.Dispose();
                        }
                        catch
                        {
                            // Ignore any error - especially do not re-add the process
                        }

                        // Get rid of it
                        return true;
                    } );

                // May want to change hibernation
                if (m_activeProcesses.Count < 1)
                    using (m_forbidHibernation)
                        m_forbidHibernation = null;
                else if (m_forbidHibernation == null)
                    if (!m_isSuspended)
                        m_forbidHibernation = PowerManager.StartForbidHibernation();
            }
        }
    }
}