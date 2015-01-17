using System;
using System.Runtime.InteropServices;

using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Interfaces;
using JMS.DVB.DeviceAccess.BDAElements;


namespace JMS.DVB.DirectShow.Filters
{
    /// <summary>
    /// Dieser Filter ist die Kernkomponente der DVB.NET <i>Direct Show</i> Integration.
    /// Er kann sowohl als Demultiplexer für einen <i>Transport Stream</i> als auch
    /// als Quelle für Bild- und Tonströme dienen.
    /// </summary>
    public partial class TSFilter : TypedComIdentity<IBaseFilter>, IBaseFilter, IMediaFilter, IAMFilterMiscFlags
    {
        /// <summary>
        /// Optional der zugehörige <i>Direct Show</i> Graph, der allerdings nur eingesetzt
        /// wird, wenn der Filter als Datenstromquelle genutzt wird.
        /// </summary>
        public readonly DisplayGraph DisplayGraph;

        /// <summary>
        /// Der aktuelle Zustand des Filters.
        /// </summary>
        private FilterStates m_State = FilterStates.Stopped;

        /// <summary>
        /// Die Kernschnittstelle des zugehörigen Graphen.
        /// </summary>
        private IntPtr m_Graph = IntPtr.Zero;

        /// <summary>
        /// Die Einspielkomponente für das Bildsignal.
        /// </summary>
        private ESInjector m_Video = null;

        /// <summary>
        /// Die Einspielkomponente für das Tonsignal.
        /// </summary>
        private ESInjector m_Audio = null;

        /// <summary>
        /// Die Einspielkomponente für ein in einem <i>Transport Stream</i>
        /// kombiniertes Bild- und Tonsignal.
        /// </summary>
        private Injector m_TSInjector;

        /// <summary>
        /// Der Name dieses Filters im Graphen.
        /// </summary>
        private string m_Name = null;

        /// <summary>
        /// Die aktuelle Referenzuhr im Graphen.
        /// </summary>
        private IntPtr m_Clock;

        /// <summary>
        /// Der Eingang zu diesem Filter.
        /// </summary>
        private InputPin m_Pin;

        /// <summary>
        /// Erzeugt einen neuen Filter.
        /// </summary>
        /// <param name="graph">Optional für den Fall, dass der Filter als
        /// Datenstromquelle genutzt wird.</param>
        public TSFilter( DisplayGraph graph )
        {
            // Remember
            DisplayGraph = graph;

            // Create pin helper
            m_Pin = new InputPin( this, false );

            // Create injectors
            m_TSInjector = new Injector( 50, 512 * TS.Manager.FullSize, m_Pin.Receive );
        }

        /// <summary>
        /// Meldet, ob für den Bilddatenstrom Synchronisationspunkte
        /// generiert werden sollen.
        /// </summary>
        public bool EnforceVideoSynchronisation
        {
            get
            {
                // Report
                return DisplayGraph.EnforceVideoSynchronisation;
            }
        }

        /// <summary>
        /// Meldet den künstlich erzwungenen Zeitversatz für den Beginn
        /// der Darstellung (in Millisekunden).
        /// </summary>
        public int AVDelay
        {
            get
            {
                // Report
                return DisplayGraph.AVDelay;
            }
        }

        /// <summary>
        /// Meldet die aktuelle Referenzzeit im Graphen.
        /// </summary>
        public long? SystemClock
        {
            get
            {
                // Report
                return DisplayGraph.SystemClock;
            }
        }

        /// <summary>
        /// Meldet den Namen dieses Filters im Graphen.
        /// </summary>
        public string Name
        {
            get
            {
                // Report
                return m_Name;
            }
        }

        /// <summary>
        /// Meldet die Anzahl der bisher verarbeiten Bytes in allen
        /// Datenströmen.
        /// </summary>
        public long AVBytesReceived
        {
            get
            {
                // Report
                return (AudioBytesReceived + VideoBytesReceived + TSBytesReceived);
            }
        }

        /// <summary>
        /// Meldet die bisher erzeugten Bytes in der Tondatenspur.
        /// </summary>
        public long AudioBytesReceived
        {
            get
            {
                // Read
                if (m_Audio == null)
                    return 0;
                else
                    return m_Audio.BytesReceived;
            }
        }

        /// <summary>
        /// Meldet die bisher erzeugten Bytes in der Bilddatenspur.
        /// </summary>
        public long VideoBytesReceived
        {
            get
            {
                // Read
                if (m_Video == null)
                    return 0;
                else
                    return m_Video.BytesReceived;
            }
        }

        /// <summary>
        /// Meldet die bisher entgegen genommenen Bytes der DVB Quelle.
        /// </summary>
        public long TSBytesReceived
        {
            get
            {
                // Read
                if (m_TSInjector == null)
                    return 0;
                else
                    return m_TSInjector.BytesReceived;
            }
        }

        /// <summary>
        /// Letzt das Datenformat der Tonspur fest.
        /// </summary>
        /// <param name="type">Das gewünschte Datenformat.</param>
        public void SetAudioType( MediaType type )
        {
            // Validate
            using (ESInjector filter = m_Audio)
                if (filter != null)
                {
                    // Forget
                    m_Audio = null;

                    // Cleanup buffer
                    filter.Inject( null, 0, 0 );
                }

            // Create new
            if (type != null)
                m_Audio = new ESInjector( this, "Audio", type );
        }

        /// <summary>
        /// Meldet den letzten in den Graphen übermittelten Zeitstempel.
        /// </summary>
        public TimeSpan? LastPTS
        {
            get
            {
                // Report
                if (m_Audio != null)
                    return m_Audio.LastPTS;
                else if (m_Video != null)
                    return m_Video.LastPTS;
                else
                    return null;
            }
        }

        /// <summary>
        /// Überträgt Tondaten in den Graphen.
        /// </summary>
        /// <param name="buffer">Ein Speicherbereich mit Audiodaten.</param>
        /// <param name="offset">Das erste Byte im Speicherbereich, das ausgewertet werden soll.</param>
        /// <param name="length">Die Anzahl der auszuwertenden Bytes.</param>
        public void InjectAudio( byte[] buffer, int offset, int length )
        {
            // Stopped
            if (buffer != null)
                if (m_State != FilterStates.Running)
                    return;

            // Forward
            if (m_Audio != null)
                m_Audio.Inject( buffer, offset, length );
        }

        /// <summary>
        /// Überträgt Tondaten in den Graphen.
        /// </summary>
        /// <param name="version">Der aktuelle Änderungszähler.</param>
        /// <param name="buffer">Ein Speicherbereich mit Audiodaten.</param>
        /// <param name="offset">Das erste Byte im Speicherbereich, das ausgewertet werden soll.</param>
        /// <param name="length">Die Anzahl der auszuwertenden Bytes.</param>
        public void InjectAudio( int version, byte[] buffer, int offset, int length )
        {
            // Stopped
            if (buffer != null)
                if (m_State != FilterStates.Running)
                    return;

            // Forward
            if (m_Audio != null)
                m_Audio.Inject( version, buffer, offset, length );
        }

        /// <summary>
        /// Meldet den Änderungszähler der Tonspur.
        /// </summary>
        public int AudioInjectorVersion
        {
            get
            {
                // Forward
                if (null == m_Audio)
                    return 0;
                else
                    return m_Audio.CurrentVersion;
            }
        }

        /// <summary>
        /// Meldet den Änderungszähler der Bildspur.
        /// </summary>
        public int VideoInjectorVersion
        {
            get
            {
                // Forward
                if (null == m_Video)
                    return 0;
                else
                    return m_Video.CurrentVersion;
            }
        }

        /// <summary>
        /// Meldet den relativen Versatz der Anzeige seit Beginn des aktuellen Datenstroms.
        /// </summary>
        public long? StreamTimeOffset
        {
            get
            {
                // Audio not initialized
                if (null == m_Audio)
                    return null;
                else
                    return m_Audio.StreamTimeOffset;
            }
        }

        /// <summary>
        /// Legt die Art der Bilddaten fest.
        /// </summary>
        /// <param name="type">Die gewünschte Art der Bilddaten.</param>
        public void SetVideoType( MediaType type )
        {
            // Validate
            using (ESInjector filter = m_Video)
                if (filter != null)
                {
                    // Forget
                    m_Video = null;

                    // Cleanup buffer
                    filter.Inject( null, 0, 0 );
                }

            // Create new
            if (type != null)
                m_Video = new ESInjector( this, "Video", type );
        }

        /// <summary>
        /// Überträgt Videodaten in den Graphen.
        /// </summary>
        /// <param name="buffer">Ein Speicherbereich mit Videodaten.</param>
        /// <param name="offset">Das erste Byte im Speicherbereich, das ausgewertet werden soll.</param>
        /// <param name="length">Die Anzahl der auszuwertenden Bytes.</param>
        public void InjectVideo( byte[] buffer, int offset, int length )
        {
            // Stopped
            if (buffer != null)
                if (m_State != FilterStates.Running)
                    return;

            // Forward
            if (m_Video != null)
                m_Video.Inject( buffer, offset, length );
        }

        /// <summary>
        /// Überträgt Videodaten in den Graphen.
        /// </summary>
        /// <param name="version">Der aktuelle Änderungszähler.</param>
        /// <param name="buffer">Ein Speicherbereich mit Videodaten.</param>
        /// <param name="offset">Das erste Byte im Speicherbereich, das ausgewertet werden soll.</param>
        /// <param name="length">Die Anzahl der auszuwertenden Bytes.</param>
        public void InjectVideo( int version, byte[] buffer, int offset, int length )
        {
            // Stopped
            if (buffer != null)
                if (m_State != FilterStates.Running)
                    return;

            // Forward
            if (m_Video != null)
                m_Video.Inject( version, buffer, offset, length );
        }

        /// <summary>
        /// Meldet die statistischen Informationen zu einem einzelnen Datenstrom.
        /// </summary>
        /// <param name="pid">Die Datenstromkennung.</param>
        /// <returns>Die gewünschten Informationen.</returns>
        public PinStatistics GetStatistics( ushort pid )
        {
            // Forward
            return m_Pin.GetStatistics( pid );
        }

        /// <summary>
        /// Meldet die zugehörige Instanz zur Zerlegung eines TS Datenstroms.
        /// </summary>
        public TS.TSParser TSParser
        {
            get
            {
                // Forward
                return m_Pin.TSParser;
            }
        }

        /// <summary>
        /// Meldet den Ausgang für die Tondaten.
        /// </summary>
        internal OutputPin AudioPin
        {
            get
            {
                // Report
                if (m_Audio == null)
                    return null;
                else
                    return m_Audio.Pin;
            }
        }

        /// <summary>
        /// Meldet den Ausgang für die Bilddaten.
        /// </summary>
        internal OutputPin VideoPin
        {
            get
            {
                // Report
                if (m_Video == null)
                    return null;
                else
                    return m_Video.Pin;
            }
        }

        /// <summary>
        /// Aktiviert oder deaktiviert das Sammeln von statistischen Daten
        /// für alle Datenströme.
        /// </summary>
        public bool EnableStatistics
        {
            get
            {
                // Forward
                return m_Pin.EnableStatistics;
            }
            set
            {
                // Forward
                m_Pin.EnableStatistics = value;
            }
        }

        /// <summary>
        /// Meldet einen Datenstrom zur Bearbeitung an.
        /// </summary>
        /// <param name="pid">Die zugehörige Datenstromkennung.</param>
        /// <param name="isSITable">Gesetzt, wenn es sich um Kontroll- und nicht Nutzdaten handelt.</param>
        /// <param name="handler">Die zu aktivierende Bearbeitungsmethode.</param>
        public void AddFilter( ushort pid, bool isSITable, Action<byte[]> handler )
        {
            // Forward
            m_Pin.AddFilter( pid, isSITable, handler );
        }

        /// <summary>
        /// Unterbricht die Auswertung für einen Datenstrom.
        /// </summary>
        /// <param name="pid">Die Kennung des betroffenen Datenstroms.</param>
        public void StopFilter( ushort pid )
        {
            // Forward
            m_Pin.StopFilter( pid );
        }

        /// <summary>
        /// Setzt die Auswertung eines Datenstrom fort.
        /// </summary>
        /// <param name="pid">Die Kennung des betroffenen Datenstroms.</param>
        public void StartFilter( ushort pid )
        {
            // Forward
            m_Pin.StartFilter( pid );
        }

        /// <summary>
        /// Entfernt die Bearbeitung eines Datenstroms.
        /// </summary>
        /// <param name="pid">Die Kennung des betroffenen Datenstroms.</param>
        public void RemoveFilter( ushort pid )
        {
            // Forward
            m_Pin.RemoveFilter( pid );
        }

        /// <summary>
        /// Entfernt die Bearbeitungsmethoden für sämtliche Datenströme.
        /// </summary>
        public void RemoveAllFilters()
        {
            // Forward
            m_Pin.RemoveAllFilters();
        }

        /// <summary>
        /// Entfernt alle temporären Informationen.
        /// </summary>
        public void ClearBuffers()
        {
            // Prepare for resync
            lock (m_CorrectionLock)
                m_Correction = null;

            // Forward
            if (m_TSInjector != null)
                m_TSInjector.Clear();
            if (m_Audio != null)
                m_Audio.Clear();
            if (m_Video != null)
                m_Video.Clear();
        }

        /// <summary>
        /// Überträge Rohdaten aus einem <i>Transport Stream</i> zur Auswertung.
        /// </summary>
        /// <param name="buffer">Ein Zwischenspeicher mit den Rohdaten.</param>
        /// <param name="offset">Die laufende Nummer des ersten Bytes der Rohdaten im Zwischenspeicher.</param>
        /// <param name="length">Die Anzahl der Bytes an Rohdaten im Zwischenspeicher.</param>
        public void InjectTSPackets( byte[] buffer, int offset, int length )
        {
            // Stopped
            if (buffer != null)
                if (m_State != FilterStates.Running)
                    return;

            // Forward
            if (m_TSInjector != null)
                m_TSInjector.Inject( buffer, offset, length, true, null );
        }

        /// <summary>
        /// Beendet die Protokollierung aller DVB Eingangsdaten.
        /// </summary>
        public void StopDump()
        {
            // Forward
            if (m_Pin != null)
                m_Pin.StopDump();
        }

        /// <summary>
        /// Beginnt mit der Protokollierung aller DVB Eingangsdaten.
        /// </summary>
        /// <param name="path">Der volle Pfad zu einer Protokolldatei - typisch werden mehr als
        /// 4 MBytes pro Sekunde geschrieben.</param>
        public void StartDump( string path )
        {
            // Forward
            if (m_Pin != null)
                m_Pin.StartDump( path );
        }

        #region IBaseFilter Members

        /// <summary>
        /// Meldet die Klassenkennung dieses Filters.
        /// </summary>
        /// <param name="classID">Die gewünschte Klassenkennung.</param>
        public void GetClassID( out Guid classID )
        {
            // Report
            classID = new Guid( "{0524140E-E4F8-480F-A492-5340AF9E4832}" );
        }

        /// <summary>
        /// Stellt die Bearbeitung eingehender Daten ein.
        /// </summary>
        /// <returns>Ergebnis der Operation.</returns>
        public Int32 Stop()
        {
            // Forward to all
            if (m_Audio != null)
                m_Audio.Stop();
            if (m_Video != null)
                m_Video.Stop();
            if (m_TSInjector != null)
                m_TSInjector.Stop();
            if (m_Pin != null)
                m_Pin.Stop();

            // Blind transition 
            m_State = FilterStates.Stopped;

            // Report
            return 0;
        }

        /// <summary>
        /// Unterbricht die Bearbeitung eingehender Daten.
        /// </summary>
        /// <returns>Ergebnis der Operation.</returns>
        public Int32 Pause()
        {
            // Blind transition 
            m_State = FilterStates.Paused;

            // Did it
            return 0;
        }

        /// <summary>
        /// Beginnt mit der Bearbeitung der Daten.
        /// </summary>
        /// <param name="start">Zeitinformation für den Beginn der Weitergabe
        /// von Nutzdaten in den Graphen.</param>
        /// <returns>Ergebnis der Operation.</returns>
        public Int32 Run( long start )
        {
            // Remember initial clock
            m_TimeOnStart = SystemClock;

            // Prepare for resync
            lock (m_CorrectionLock)
                m_Correction = null;

            // Blind transition 
            m_State = FilterStates.Running;

            // Forward to all
            if (m_Pin != null)
                m_Pin.Start();
            if (m_TSInjector != null)
                m_TSInjector.Start();
            if (m_Video != null)
                m_Video.Start();
            if (m_Audio != null)
                m_Audio.Start();

            // Did it
            return 0;
        }

        /// <summary>
        /// Ermittelt den aktuellen Bearbeitungszustand.
        /// </summary>
        /// <param name="millisecondsTimeout">Maximal erlaubte Verzögerung bis zur 
        /// Bereitstellung der Antwort (in Millisekunden).</param>
        /// <returns>Der aktuelle Bearbeitungszustand.</returns>
        public FilterStates GetState( uint millisecondsTimeout )
        {
            // Report
            return m_State;
        }

        /// <summary>
        /// Legt die Referenzuhr für diesen Filter fest.
        /// </summary>
        /// <param name="clock">Die neue Referenzuhr.</param>
        public void SetSyncSource( IntPtr clock )
        {
            // Proper cleanup
            BDAEnvironment.Release( ref m_Clock );

            // Remember
            m_Clock = clock;

            // Proper lock
            if (m_Clock != IntPtr.Zero)
                Marshal.AddRef( m_Clock );
        }

        /// <summary>
        /// Meldet die aktuelle Referenzuhr.
        /// </summary>
        /// <returns>Die aktuelle Referenzuhr.</returns>
        public IntPtr GetSyncSource()
        {
            // Correct COM reference
            if (m_Clock != IntPtr.Zero)
                Marshal.AddRef( m_Clock );

            // Get the interface
            return m_Clock;
        }

        /// <summary>
        /// Meldet alle <i>Direct Show</i> Ein- und Ausgänge dieses Filters. Diese
        /// sind abhängig vom aktuellen Betriebsmodus als Demultiplexer oder
        /// Datenquelle.
        /// </summary>
        /// <returns>Eine Auflistung über die Ein- und Ausgänge.</returns>
        public IEnumPins EnumPins()
        {
            // Create
            PinEnum pinEnum = new PinEnum();

            // Audio 
            if (m_Audio != null)
                if (m_Audio.Pin != null)
                    pinEnum.Add( m_Audio.Pin );
            if (m_Video != null)
                if (m_Video.Pin != null)
                    pinEnum.Add( m_Video.Pin );

            // Report
            return pinEnum;
        }

        /// <summary>
        /// Ermittelt einen Ein- oder Ausgang nach dessen Namen.
        /// </summary>
        /// <param name="ID">Der zu verwendende Name.</param>
        /// <returns>In dieser Implementierung immer <i>null</i>.</returns>
        public IPin FindPin( string ID )
        {
            // Lookup
            return null;
        }

        /// <summary>
        /// Beschreibt diesen Filter.
        /// </summary>
        /// <param name="info">Die gewünschte Beschreibung.</param>
        public void QueryFilterInfo( ref FilterInfo info )
        {
            // Correct COM reference
            if (m_Graph != IntPtr.Zero)
                Marshal.AddRef( m_Graph );

            // Create
            info.Graph = m_Graph;
            info.Name = m_Name;
        }

        /// <summary>
        /// Ordnet dieser Instanz einem Graphen zu.
        /// </summary>
        /// <param name="graph">Der gewünschte Graph.</param>
        /// <param name="name">Der Name des Filters im Graphen.</param>
        public void JoinFilterGraph( IFilterGraph graph, string name )
        {
            // Cleanup
            BDAEnvironment.Release( ref m_Graph );

            // Remember
            m_Name = name;

            // Disconnect request
            if (graph == null)
                return;

            // Remember
            m_Graph = Marshal.GetIUnknownForObject( graph );

            // Release
            if (Marshal.IsComObject( graph ))
                Marshal.ReleaseComObject( graph );
        }

        /// <summary>
        /// Meldet die Versionsbezeichnung dieses Filters.
        /// </summary>
        /// <returns>Letztlich die DVB.NET Version.</returns>
        public string QueryVendorInfo()
        {
            // Some stuff
            return "DVB.NET 4.3 Injection Filter";
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        public override void Dispose()
        {
            // Stop and raw TS dump
            StopDump();

            // Stop all
            Stop();

            // Forward to injectors
            using (Injector ts = m_TSInjector)
                if (ts != null)
                {
                    // Forget
                    m_TSInjector = null;

                    // Shut down
                    ts.Inject( null, 0, 0, true, null );
                }

            // Clear AV
            SetAudioType( null );
            SetVideoType( null );

            // Forward to input pin
            using (var pin = m_Pin)
                m_Pin = null;

            // Forget the graph
            BDAEnvironment.Release( ref m_Graph );

            // Forward
            base.Dispose();
        }

        #endregion

        #region IAMFilterMiscFlags Members

        /// <summary>
        /// Meldet die <i>Direct Show</i> Parameter dieses Filters.
        /// </summary>
        /// <returns></returns>
        uint IAMFilterMiscFlags.GetMiscFlags()
        {
            // We are not a live source
            return 0;
        }

        #endregion
    }
}
