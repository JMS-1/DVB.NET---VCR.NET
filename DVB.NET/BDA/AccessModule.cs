using System;
using System.Threading;


namespace JMS.DVB.DirectShow
{
    /// <summary>
    /// Abgeleitete Klassen dieser Basisklassen werden <i>Zugriffsmodule</i> genannt. Ihre
    /// Aufgabe ist es, Transport Stream Pakete in einen DVB.NET DirectShow Graphen zur
    /// Anzeige von Bild und Ton einzuspielen.
    /// </summary>
    /// <remarks>
    /// Es werden hier explizit die Synchronisationsmechanismen von Windows verwendet, 
    /// da gewisse DVB Treiber mit den Mechanismen von .NET zurechtkommen. Bei Einsatz
    /// von <see cref="Monitor.Wait(object)"/> et al kommt es durch die damit verbundenen
    /// erzwungenen <see cref="Thread"/> Wechsel zu Aussetzern im Empfang.
    /// </remarks>
    public abstract class AccessModule : IDisposable
    {
        /// <summary>
        /// Wird gesetzt, sobald das Zugriffsmodul endgültig beendet wird.
        /// <see cref="IsDisposing"/>
        /// </summary>
        private volatile bool m_Disposed = false;

        /// <summary>
        /// Ist gesetzt, wenn Daten vom Zugriffsmodul in den Graphen übertragen
        /// werden sollen.
        /// </summary>
        private volatile bool m_Running = false;

        /// <summary>
        /// Der DVB.NET DirectShow Graph zur Anzeige von Bild und Ton.
        /// </summary>
        private DisplayGraph m_Graph;

        /// <summary>
        /// Befüllt die Tonspur mit Daten.
        /// </summary>
        private Thread m_AudioThread;

        /// <summary>
        /// Befüllt die Bildspur mit Daten.
        /// </summary>
        private Thread m_VideoThread;

        /// <summary>
        /// Dient zur Benachrichtigung, ob neue Daten auf der Tonspur bereit stehen.
        /// </summary>
        private object m_AudioSync = new object();

        /// <summary>
        /// Gesetzt, wenn Daten auf der Tonspur bereit stehen.
        /// </summary>
        private volatile bool m_HasAudioData = false;

        /// <summary>
        /// Dient zur Benachrichtigung, ob neue Daten auf der Bildspur bereit stehen.
        /// </summary>
        private object m_VideoSync = new object();

        /// <summary>
        /// Gesetzt, wenn Daten auf der Bildspur bereit stehen.
        /// </summary>
        private volatile bool m_HasVideoData = false;

        /// <summary>
        /// Wird gesetzt, wenn der Datenstrom ohne weiteteres Zutun 
        /// in diese Instanz fließt.
        /// </summary>
        private volatile bool m_HasExternalFeed;

        /// <summary>
        /// Initialisiert ein Zugriffsmodul.
        /// </summary>
        protected AccessModule()
        {
        }

        /// <summary>
        /// Verbindet ein Zugriffsmodul mit einem DVB.NET DirectShow Graphen zur Anzeige
        /// von Bild und Ton.
        /// </summary>
        /// <remarks>
        /// Dabei wird der Thread zur Befüllung des Graphen erzeugt.
        /// </remarks>
        /// <param name="graph">Der DVB.NET DirectShow Graph.</param>
        /// <exception cref="ArgumentNullException">Der Parameter darf nicht <i>null</i> sein.</exception>
        /// <exception cref="NotSupportedException">Die Methode darf nur einmal aufgerufen werden.</exception>
        internal void SetGraph( DisplayGraph graph )
        {
            // Validate
            if (null == graph)
                throw new ArgumentNullException( "graph" );
            if (null != m_Graph)
                throw new NotSupportedException();

            // Remember
            m_Graph = graph;

            // Create threads
            m_AudioThread = new Thread( AudioFeed ) { Name = "DVB.NET Audio", IsBackground = true };
            m_VideoThread = new Thread( VideoFeed ) { Name = "DVB.NET Video", IsBackground = true };

            // Set apartment - just in case
            m_AudioThread.SetApartmentState( ApartmentState.MTA );
            m_VideoThread.SetApartmentState( ApartmentState.MTA );

            // Start threads
            m_AudioThread.Start();
            m_VideoThread.Start();
        }

        /// <summary>
        /// Legt fest, ob ein ständiger externer Datenstrom vorliegt oder nicht.
        /// </summary>
        /// <param name="hasFeed">Gesetzt, wenn ein externer Datenstrom
        /// vorliegt.</param>
        protected void SetExternalFeed( bool hasFeed )
        {
            // No change
            if (m_HasExternalFeed == hasFeed)
                return;

            // Just update
            m_HasExternalFeed = hasFeed;

            // Forward
            if (!hasFeed)
            {
                // Wakeup all
                ReportAudioAvailable();
                ReportVideoAvailable();
            }
        }

        /// <summary>
        /// Meldet, ob das Zugriffsmodul gerade beendet wird.
        /// </summary>
        protected bool IsDisposing
        {
            get
            {
                // Report
                return m_Disposed;
            }
        }

        /// <summary>
        /// Meldet, ob das Zugriffsmodul aktiv ist.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                // Report
                return m_Running;
            }
        }

        /// <summary>
        /// Meldet, dass Daten für die Tonspur bereit stehen.
        /// </summary>
        protected virtual void ReportAudioAvailable()
        {
            // See someone is waiting
            if (!m_HasAudioData)
                lock (m_AudioSync)
                {
                    // Update
                    m_HasAudioData = true;

                    // Signal
                    Monitor.Pulse( m_AudioSync );
                }
        }

        /// <summary>
        /// Meldet, dass Daten für die Bildspur bereit stehen.
        /// </summary>
        protected virtual void ReportVideoAvailable()
        {
            // See someone is waiting
            if (!m_HasVideoData)
                lock (m_VideoSync)
                {
                    // Update
                    m_HasVideoData = true;

                    // Signal
                    Monitor.Pulse( m_VideoSync );
                }
        }

        /// <summary>
        /// Befüllt die Tonspur mit Daten.
        /// </summary>
        private void AudioFeed()
        {
            // Load the graph to use
            DisplayGraph graph = m_Graph;

            // Died early
            if (graph == null)
                return;

            // Process
            for (; ; )
            {
                // Wait for audio data
                if (!m_Disposed)
                {
                    // Wait if necessary
                    if (!m_HasAudioData)
                        lock (m_AudioSync)
                            if (!m_HasAudioData)
                                if (m_HasExternalFeed)
                                    Monitor.Wait( m_AudioSync );
                                else
                                    Monitor.Wait( m_AudioSync, 1 );

                    // Reset
                    m_HasAudioData = false;
                }

                // Done
                if (m_Disposed)
                    break;

                // Skip
                if (!m_Running)
                    continue;

                // Get the next chunk
                byte[] chunk = GetNextChunk( false );

                // Check for data
                if (chunk == null)
                    continue;

                // Get the current version stamp - before testing for m_Running!
                int version = graph.InjectorFilter.AudioInjectorVersion;

                // Report
                if (m_Running)
                    graph.InjectorFilter.InjectAudio( version, chunk, 0, chunk.Length );
            }

            // Cleanup
            graph.InjectorFilter.InjectAudio( null, 0, 0 );
        }

        /// <summary>
        /// Befüllt die Bildspur mit Daten.
        /// </summary>
        private void VideoFeed()
        {
            // Load the graph to use
            DisplayGraph graph = m_Graph;

            // Died early
            if (graph == null)
                return;

            // Process
            for (; ; )
            {
                // Wait for video data
                if (!m_Disposed)
                {
                    // Wait if necessary
                    if (!m_HasVideoData)
                        lock (m_VideoSync)
                            if (!m_HasVideoData)
                                if (m_HasExternalFeed)
                                    Monitor.Wait( m_VideoSync );
                                else
                                    Monitor.Wait( m_VideoSync, 1 );

                    // Reset
                    m_HasVideoData = false;
                }

                // Done
                if (m_Disposed)
                    break;

                // Skip
                if (!m_Running)
                    continue;

                // Get the next chunk
                byte[] chunk = GetNextChunk( true );

                // Check for data
                if (chunk == null)
                    continue;

                // Read current version - before testing for m_Running!
                int version = graph.InjectorFilter.VideoInjectorVersion;

                // Process
                if (m_Running)
                    graph.InjectorFilter.InjectVideo( version, chunk, 0, chunk.Length );
            }

            // Cleanup
            graph.InjectorFilter.InjectVideo( null, 0, 0 );
        }

        /// <summary>
        /// Startet die Datenübertragung aus dem Zugriffsmodul in den DirectShow
        /// Graphen.
        /// </summary>
        /// <param name="mpeg4">Gesetzt für MPEG-4 Bilddaten, nicht gesetzt für MPEG-2 
        /// Bilddaten. Liegt kein Videosignal vor, so kann dieser Parameter <i>null</i>
        /// sein.</param>
        /// <param name="ac3">Gesetzt für eine AC3 Tonspur, nicht gesetzt für eine MP2
        /// Tonspur. Ist keine Tonspur vorhanden, so kann dieser Parameter <i>null</i>
        /// sein - es wird allerdings empfohlen, nie ohne Tonspur zu arbeiten.</param>
        public void StartGraph( bool mpeg4, bool ac3 )
        {
            // Reconfigure the graph
            if (null != m_Graph)
                m_Graph.Show( mpeg4, ac3 );

            // Startup
            Start();
        }

        /// <summary>
        /// Hält den DirectShow Graphen an.
        /// </summary>
        public void StopGraph()
        {
            // Forward
            m_Graph.Stop();
        }

        /// <summary>
        /// Stellt sicher, dass keine Daten mehr bearbeitet werden.
        /// </summary>
        public virtual void Start()
        {
            // Change flag
            m_Running = true;

            // Wakeup
            ReportAudioAvailable();
            ReportVideoAvailable();
        }

        /// <summary>
        /// Beendet die Datenübertragung in den DirectShow Graphen.
        /// </summary>
        public virtual void Stop()
        {
            // Change flag
            m_Running = false;
        }

        /// <summary>
        /// Fordert den DirectShow Graphen auf, alle zwischengespeicherten Daten zu
        /// ignorieren.
        /// </summary>
        /// <remarks>
        /// Ein Aufruf erfolgt im Allgemeinen nach einem Senderwechsel.
        /// </remarks>
        public virtual void ClearBuffers()
        {
            // Load graph
            DisplayGraph graph = m_Graph;

            // Forward
            if (graph != null)
                graph.InjectorFilter.ClearBuffers();
        }

        /// <summary>
        /// Wird von konkreten Zugriffsmodulen implementiert und meldet alle aktuell
        /// zur Übertragung in den DirectShow Graphen bereitstehenden Daten.
        /// </summary>
        /// <param name="video">Gesetzt für die Abfrage von Videodaten.</param>
        /// <returns>Die gewünschten Daten oder <i>null</i>.</returns>
        protected abstract byte[] GetNextChunk( bool video );

        /// <summary>
        /// Relativer Versatz zwischen der Zeit im Graphen und dem zuletzt gesesendeten
        /// Zeitstempel - der Wert ist 0, wenn der gleiche Versatz wie beim Starten des
        /// Graphen besteht.
        /// </summary>
        public long? StreamTimeOffset
        {
            get
            {
                // Load graph
                var graph = m_Graph;
                if (graph == null)
                    return null;
                else
                    return m_Graph.InjectorFilter.StreamTimeOffset;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet alle Aktivitäten dieses Zugriffsmoduls.
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        /// <summary>
        /// Beendet alle Aktivitäten dieses Zugriffsmoduls.
        /// </summary>
        public void Dispose()
        {
            // Prepare shutdown
            m_Disposed = true;

            // Stop displaying
            StopGraph();

            // Terminate
            Stop();

            // Forward
            OnDispose();

            // Wakeup all
            ReportAudioAvailable();
            ReportVideoAvailable();

            // Wait for threads
            if (m_AudioThread != null)
                m_AudioThread.Join();
            if (m_VideoThread != null)
                m_VideoThread.Join();

            // Forget all
            m_AudioThread = null;
            m_VideoThread = null;
        }

        #endregion
    }
}
