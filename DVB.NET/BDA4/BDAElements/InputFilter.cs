using System;
using System.Runtime.InteropServices;

using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.Interfaces;
using JMS.DVB.DeviceAccess.BDAElements;


namespace JMS.DVB.DeviceAccess.BDAElements
{
    /// <summary>
    /// Dieser Filter ist die Kernkomponente der DVB.NET <i>Direct Show</i> Integration.
    /// Er kann sowohl als Demultiplexer für einen <i>Transport Stream</i> als auch
    /// als Quelle für Bild- und Tonströme dienen.
    /// </summary>
    public class InputFilter : TypedComIdentity<IBaseFilter>, IBaseFilter, IMediaFilter, IAMFilterMiscFlags
    {
        /// <summary>
        /// Der aktuelle Zustand des Filters.
        /// </summary>
        private FilterStates m_State = FilterStates.Stopped;

        /// <summary>
        /// Die Kernschnittstelle des zugehörigen Graphen.
        /// </summary>
        private IntPtr m_Graph = IntPtr.Zero;

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
        public InputPin DataManager { get; private set; }

        /// <summary>
        /// Erzeugt einen neuen Filter.
        /// </summary>
        public InputFilter()
        {
            // Create pin helper
            DataManager = new InputPin( this, true );

            // Initializte
            DataManager.EnableStatistics = BDASettings.GenerateTSStatistics;
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

        #region IBaseFilter Members

        /// <summary>
        /// Meldet die Klassenkennung dieses Filters.
        /// </summary>
        /// <param name="classID">Die gewünschte Klassenkennung.</param>
        public void GetClassID( out Guid classID )
        {
            // Report
            classID = new Guid( "{9F8AFD1C-D470-4F5C-906F-61FD27963148}" );
        }

        /// <summary>
        /// Stellt die Bearbeitung eingehender Daten ein.
        /// </summary>
        /// <returns>Ergebnis der Operation.</returns>
        public Int32 Stop()
        {
            // Forward to all
            if (DataManager != null)
                DataManager.Stop();

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
            // Blind transition 
            m_State = FilterStates.Running;

            // Forward to all
            if (DataManager != null)
                DataManager.Start();

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
            var pinEnum = new PinEnum();

            // Fill pins
            if (DataManager != null)
            {
                // Report all
                pinEnum.Add( DataManager );
                pinEnum.Add( DataManager.TIFConnector );
            }

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
            return "DVB.NET 4.3 DVB Receiver Filter";
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        public override void Dispose()
        {
            // Stop all
            Stop();

            // Forward to input pin
            using (DataManager)
                DataManager = null;

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
