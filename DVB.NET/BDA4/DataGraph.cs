using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using JMS.DVB.DeviceAccess.BDAElements;
using JMS.DVB.DeviceAccess.Enumerators;
using JMS.DVB.DeviceAccess.Interfaces;
using JMS.DVB.DeviceAccess.Pipeline;


namespace JMS.DVB.DeviceAccess
{
    /// <summary>
    /// Beschreibt den DirectShow BDA Graphen, der die DVB Daten empfängt und verteilt.
    /// </summary>
    public partial class DataGraph : IDisposable
    {
        /// <summary>
        /// Ein leeres Feld.
        /// </summary>
        private static readonly SourceIdentifier[] s_NoSources = { };

        /// <summary>
        /// Der zu verwendenden Filters zur Auswahl der Quellgruppe.
        /// </summary>
        public FilterInformation TunerInformation { get; set; }

        /// <summary>
        /// Optional der Filter zur Übernahme der DVB Daten in den Graphen.
        /// </summary>
        public FilterInformation CaptureInformation { get; set; }

        /// <summary>
        /// Detaileinstellungen zu diesem Graphen.
        /// </summary>
        public GraphConfiguration Configuration { get; private set; }

        /// <summary>
        /// Die Art des Empfangs.
        /// </summary>
        public DVBSystemType DVBType { get; set; }

        /// <summary>
        /// Der hier verwaltete DirectShow Graph.
        /// </summary>
        private IMediaFilter m_Graph;

        /// <summary>
        /// Optional eine externe Registrierung des Graphen zu Testzwecken.
        /// </summary>
        private ROTRegister m_ExternalRegistration;

        /// <summary>
        /// Optional eine Protokolldatei.
        /// </summary>
        private FileStream m_LogFile;

        /// <summary>
        /// Der Filter, über den der Auswahlvorgang der Quellgruppe gesteuert wird.
        /// </summary>
        public TypedComIdentity<IBaseFilter> NetworkProvider { get; private set; }

        /// <summary>
        /// Der Gerätefilter zur Steuerung der Quellgruppe.
        /// </summary>
        public TypedComIdentity<IBaseFilter> TunerFilter { get; private set; }

        /// <summary>
        /// Optional der Gerätefilter zur Speisung der Nutzdaten in den Graphen.
        /// </summary>
        public TypedComIdentity<IBaseFilter> CaptureFilter { get; private set; }

        /// <summary>
        /// Alle zusätzlich geladenen Filter.
        /// </summary>
        public readonly List<TypedComIdentity<IBaseFilter>> AdditionalFilters = new List<TypedComIdentity<IBaseFilter>>();

        /// <summary>
        /// Der <i>Transport Information Filter</i> dieses Graphen.
        /// </summary>
        private TypedComIdentity<IBaseFilter> m_TIF;

        /// <summary>
        /// Der primäre Filter zur Analyse des Rohdatenstroms.
        /// </summary>
        public InputFilter TransportStreamAnalyser { get; private set; }

        /// <summary>
        /// Der aktuelle Empfänger für eine angeforderte PMT.
        /// </summary>
        private Action<EPG.Tables.PMT> m_PMTSink;

        /// <summary>
        /// Eine Liste von zusätzlich in den Graphen aufzunehmenden Filtern.
        /// </summary>
        public readonly List<FilterInformation> AdditionalFilterInformations = new List<FilterInformation>();

        /// <summary>
        /// Die Liste der gerade entschlüsselten Quellen.
        /// </summary>
        private SourceIdentifier[] m_currentDecryption = s_NoSources;

        /// <summary>
        /// Gesetzt um zu verhindern, dass bei einem fehlgeschlagenen Wechsel der Quellgruppe die Entschlüsselung
        /// zurückgesetzt wird. Diese Einstellung kann den Sendersuchlauf erheblich beschleunigen.
        /// </summary>
        public bool DisableCIResetOnTuneFailure { get; set; }

        /// <summary>
        /// Erzeugt einen neuen Graphen.
        /// </summary>
        public DataGraph()
        {
            // Finish
            DecryptionPipeline = new ActionPipeline<DecryptToken>( this, Properties.Resources.Pipeline_Decrypt, null );
            TunePipeline = new ActionPipeline<TuneToken>( this, Properties.Resources.Pipeline_Tune, SetTuneRequest );
            SignalPipeline = new ActionPipeline<SignalToken>( this, Properties.Resources.Pipeline_Signal, null );
            Configuration = new GraphConfiguration( this );
            DVBType = DVBSystemType.Unknown;
        }

        /// <summary>
        /// Wählt eine neue Quellgruppe aus.
        /// </summary>
        /// <param name="location">Der Ursprung, über den die Quellgruppe empfangen wird.</param>
        /// <param name="group">Die gewünschte Quellgruppe.</param>
        /// <exception cref="NotSupportedException">Der Graph wurde noch nicht erzeugt.</exception>
        public void Tune( GroupLocation location, SourceGroup group )
        {
            // Validate
            if (NetworkProvider == null)
                throw new NotSupportedException( Properties.Resources.Exception_NotStarted );

            // PAT wait configuration - will use 1/10sec per retry
            var patRetries = Configuration.MinimumPATCountWaitTime;
            var patsExpected = Configuration.MinimumPATCount;

            // Attach to helper
            var analyser = TransportStreamAnalyser;
            var manager = (analyser == null) ? null : analyser.DataManager;
            var parser = (manager == null) ? null : manager.TSParser;

            // May want to retry at least four times
            for (var outerRetry = 4; outerRetry-- > 0; patsExpected += 5, patRetries += 25)
            {
                // Always stop raw dump
                if (manager != null)
                    manager.StopDump();

                // Report
                if (BDASettings.BDATraceSwitch.Enabled)
                    Trace.WriteLine( string.Format( Properties.Resources.Trace_TuneStart, location, group ), BDASettings.BDATraceSwitch.DisplayName );

                // Route request through the current pipeline
                using (var tune = TuneToken.Create( TunePipeline, location, group ))
                    tune.Execute();

                // Not yet up
                if (analyser == null)
                    return;

                // Attach to statistics interface
                var statistics = TunerFilter.GetSignalStatistics();
                if (statistics != null)
                    try
                    {
                        // Set counter
                        var tuneRetry = 10;

                        // Wait
                        for (; tuneRetry-- > 0; Thread.Sleep( 100 ))
                        {
                            // Load the signal information
                            var locked = statistics.SignalLocked;
                            if (locked != 0)
                                break;
                        }

                        // Report
                        if (BDASettings.BDATraceSwitch.Enabled)
                            Trace.WriteLine( string.Format( Properties.Resources.Trace_Lock, 10 - tuneRetry, tuneRetry, BDASettings.FastTune ), BDASettings.BDATraceSwitch.DisplayName );

                        // Final check
                        if (BDASettings.FastTune)
                            if (statistics.SignalLocked == 0)
                                return;
                    }
                    catch
                    {
                        // Just ignore any error
                    }
                    finally
                    {
                        // Back to COM
                        BDAEnvironment.Release( ref statistics );
                    }

                // Check for dump
                StartDump();

                // Reset PAT detection
                parser.RestartPATCounter();

                // Request the number of bytes currently processed
                var bytes = parser.BytesReceived;
                var limit = patsExpected;
                var retry = patRetries;

                // Wait a bit
                for (; (parser.ValidPATCount < limit) && (retry-- > 0); )
                    Thread.Sleep( 100 );

                // Report
                if (BDASettings.BDATraceSwitch.Enabled)
                    Trace.WriteLine( string.Format( Properties.Resources.Trace_StreamOk, patRetries - retry, retry, parser.ValidPATCount, parser.BytesReceived - bytes ), BDASettings.BDATraceSwitch.DisplayName );

                // Disable retry if PATs are received 
                if (parser.ValidPATCount >= limit)
                    break;

                // Disable retry if no data is on the transponder
                if (bytes == parser.BytesReceived)
                    if (outerRetry != 3)
                        break;

                // Report for debugging purposes
                if (BDASettings.BDATraceSwitch.Enabled)
                    Trace.WriteLine( Properties.Resources.Trace_TuneFailed, BDASettings.BDATraceSwitch.DisplayName );

                // Inform all helpers on reset
                SendEmptyTuneRequest( DisableCIResetOnTuneFailure );
            }
        }

        /// <summary>
        /// Ermittelt die Informationen zum Signal.
        /// </summary>
        public SignalInformation SignalStatus
        {
            get
            {
                // Run through pipeline
                using (var token = SignalToken.Create( SignalPipeline ))
                {
                    // Process
                    token.Execute();

                    // Report
                    return token.SignalInformation;
                }
            }
        }

        /// <summary>
        /// Aktiviert die Protokollierung des Rohdatenstroms.
        /// </summary>
        private void StartDump()
        {
            // See if we should dump
            var dumpDir = BDASettings.DumpDirectory;
            if (dumpDir == null)
                return;

            // Be safe
            try
            {
                // Attach to directory
                if (!dumpDir.Exists)
                    return;

                // Get the path
                var path = Path.Combine( dumpDir.FullName, string.Format( "{0:N}.ts", Guid.NewGuid() ) );

                // Change dump
                TransportStreamAnalyser.DataManager.StartDump( path );

                // Report
                if (BDASettings.BDATraceSwitch.Enabled)
                    Trace.WriteLine( string.Format( Properties.Resources.Trace_Dump, path ), BDASettings.BDATraceSwitch.DisplayName );
            }
            catch
            {
                // Ignore anything
            }
        }

        /// <summary>
        /// Sendet eine leere Anfrage zum Wechsel der Quellgruppe. Diese kann von allen Aktionen
        /// verwendet werden, um interne Zustände zu (re-)initialisieren.
        /// </summary>
        /// <param name="disableDecryptionReset">Gesetzt, um die Neuinitialisierung der Entschlüsselung
        /// zu deaktivieren.</param>
        private void SendEmptyTuneRequest( bool disableDecryptionReset = false )
        {
            // Process
            using (var tune = TuneToken.Create( TunePipeline, null, null ))
                tune.Execute();
            if (!disableDecryptionReset)
                using (var crypt = DecryptToken.Create( DecryptionPipeline, null ))
                    crypt.Execute();
            using (var sig = SignalToken.Create( SignalPipeline ))
                sig.Execute();

            // Reset
            m_currentDecryption = s_NoSources;
        }

        /// <summary>
        /// Erzeugt einen Graphen und startet ihn.
        /// </summary>
        /// <param name="location">Der Ursprung, über den die Quellgruppe empfangen wird.</param>
        /// <param name="group">Die gewünschte Quellgruppe.</param>
        /// <exception cref="ArgumentException">Es wurden nicht alle Parameter gesetzt.</exception>
        public void Create( GroupLocation location, SourceGroup group )
        {
            // Get rid of it
            Destroy();

            // Create new graph builder
            var graph = Activator.CreateInstance( Type.GetTypeFromCLSID( BDAEnvironment.GraphBuilderClassIdentifier ) );
            try
            {
                // Convert interface
                m_Graph = (IMediaFilter) graph;
            }
            catch
            {
                // Cleanup
                BDAEnvironment.Release( ref graph );

                // Forward
                throw;
            }

            // See if we should register the graph
            m_ExternalRegistration = BDASettings.RegisterBDAGRaph( m_Graph, false );

            // Attach to alternate interface
            var builder = (IGraphBuilder) m_Graph;

            // Check log 
            var logFile = BDASettings.BDALogPath;
            if (logFile != null)
            {
                // Open path
                m_LogFile = new FileStream( logFile.FullName, FileMode.Create, FileAccess.Write, FileShare.Read );

                // Enable logging on graph builder
                builder.SetLogFile( m_LogFile.SafeFileHandle );
            }

            // Start with network provider
            NetworkProvider = AddFilter( "Network Provider", BDAEnvironment.GetNetworkProviderMoniker( DVBType ) );

            // Initialize provider
            Tune( location, group );

            // Always create the tuner
            if (TunerInformation != null)
                TunerFilter = AddFilter( "Tuner", TunerInformation );
            else
                throw new ArgumentException( Properties.Resources.Exception_MissingTuner, "Tuner" );

            // Optionally create capture
            if (CaptureInformation != null)
                CaptureFilter = AddFilter( "Capture", CaptureInformation );

            // Add additional filter
            foreach (var additionalFilter in AdditionalFilterInformations)
                if (additionalFilter == null)
                    throw new ArgumentNullException( "AdditionalFilters" );
                else
                    AdditionalFilters.Add( AddFilter( additionalFilter.DisplayName, additionalFilter ) );

            // Connect network provider to streaming instance
            Connect( NetworkProvider, CaptureFilter ?? TunerFilter );

            // Initialize provider
            Tune( location, group );

            // Create the primary filter and add it
            AddFilter( "TS", TransportStreamAnalyser = new InputFilter() );

            // Connect device output for analysis
            Connect( CaptureFilter ?? TunerFilter, TransportStreamAnalyser );

            // Create the demultiplexer - needed to keep the infrastructure alive
            using (var demux = AddFilter( "TIF", BDAEnvironment.MicrosoftDemultiplexerMoniker ))
            {
                // Connect to the dedicated pin of our analyser
                TransportStreamAnalyser.DataManager.TIFConnector.Connect( demux, BDAEnvironment.TransportStreamMediaType1 );

                // Pins to remove
                var remove = new List<string>();

                // Prepare the demultiplexer pins
                demux.InspectAllPins( pin =>
                    {
                        // See if this is the SI pin
                        bool isSectionPin = false;
                        pin.InspectAllMediaTypes( type =>
                            {
                                // Check major
                                if (!type.MajorType.Equals( BDAEnvironment.DataFormatTypeSections ))
                                    return true;

                                // Check minor
                                isSectionPin = type.SubType.Equals( BDAEnvironment.DataFormatSubtypeSI );

                                // Report
                                return !isSectionPin;
                            } );

                        // Check the mode
                        if (isSectionPin)
                        {
                            // Connect
                            using (var comPin = ComIdentity.Create( pin ))
                                builder.Render( comPin.Interface );

                            // Load connection data
                            IntPtr tifIn = IntPtr.Zero;
                            if (pin.ConnectedTo( ref tifIn ) < 0)
                                throw new InvalidOperationException( Properties.Resources.Exception_TIF );

                            // Reconstruct
                            var tifPin = Marshal.GetObjectForIUnknown( tifIn );
                            try
                            {
                                // Request pin context
                                var info = new PinInfo();
                                ((IPin) tifPin).QueryPinInfo( ref info );

                                // Request from pin
                                m_TIF = info.GetAndDisposeFilter();
                            }
                            finally
                            {
                                // Cleanup
                                BDAEnvironment.Release( ref tifPin );
                            }
                        }
                        else if (pin.QueryDirection() == PinDirection.Output)
                        {
                            // Prepare to kill
                            remove.Add( pin.QueryId() );
                        }

                    } );

                // Prepare to remove all unconnected pins
                if (remove.Count > 0)
                    using (var demuxInstance = demux.MarshalToManaged())
                    {
                        // Change type
                        var mpeg2 = (IMpeg2Demultiplexer) demuxInstance.Object;

                        // Remove all
                        foreach (var id in remove)
                            mpeg2.DeleteOutputPin( id );
                    }
            }

            // Install the PMT watchdog
            TransportStreamAnalyser.DataManager.TSParser.PMTFound += ProcessPMT;
        }

        /// <summary>
        /// Wertet eine PMT aus.
        /// </summary>
        /// <param name="pmt">Die angeforderte PMT.</param>
        private void ProcessPMT( EPG.Tables.PMT pmt )
        {
            // Skip
            if (pmt == null)
                return;
            if (!pmt.IsValid)
                return;

            // Load receiver
            var sink = Interlocked.Exchange( ref m_PMTSink, null );
            if (sink != null)
                try
                {
                    // Process
                    sink( pmt );
                }
                catch (Exception e)
                {
                    // Maybe log does not yet exist
                    try
                    {
                        // Report and ignore
                        EventLog.WriteEntry( "DVB.NET", e.ToString(), EventLogEntryType.Error );
                    }
                    catch
                    {
                        // At least trace it
                        Trace.WriteLine( e.ToString() );
                    }
                }
        }

        /// <summary>
        /// Aktiviert den Graphen, beendet aber den <i>Transport Information Filter</i> sofort wieder.
        /// </summary>
        public void Start()
        {
            // Try start
            var result = m_Graph.Run( 0 );
            if (result < 0)
                throw new InvalidOperationException( string.Format( "graph not started, error is 0x{0:x}", result ) );

            // Attach
            using (var tif = m_TIF.MarshalToManaged())
                if (tif.Object.Stop() < 0)
                    throw new InvalidOperationException( Properties.Resources.Exception_StopTIF );
        }

        /// <summary>
        /// Aktiviert die Entschlüsselung.
        /// </summary>
        /// <param name="sources">Die Liste der zu entschlüssenden Quellen.</param>
        public void Decrypt( params SourceIdentifier[] sources )
        {
            // Default it
            if (sources == null)
                sources = s_NoSources;

            // Not possible
            if (DecryptionPipeline.IsEmpty)
                if (sources.Length < 1)
                    return;
                else
                    throw new NotSupportedException();

            // See if we have to do anything - avoind changing decyrption if already active
            if (sources.Length > 0)
                if (sources.Length == m_currentDecryption.Length)
                    if (Enumerable.Range( 0, sources.Length ).All( index => Equals( sources[index], m_currentDecryption[index] ) ))
                        return;

            // Create the token
            using (var token = DecryptToken.Create( DecryptionPipeline, sources ))
                token.Execute();

            // Remember new decryption sources
            m_currentDecryption = sources;
        }

        /// <summary>
        /// Erstellt eine neue Überwachung der Nutzdatenströme.
        /// </summary>
        /// <param name="processor">Der Verarbeitungsalgorithmus.</param>
        /// <param name="services">Die Liste der Dienste.</param>
        public void ActivatePMTWatchDog( Func<EPG.Tables.PMT, bool> processor, params SourceIdentifier[] services )
        {
            // Forward
            PMTSequencer.Start( this, services, processor );
        }

        /// <summary>
        /// Aktiviert die Überwachung des PMT Empfangs.
        /// </summary>
        /// <param name="service">Der zu überwachende Dienst.</param>
        /// <param name="consumer">Der Empfänger für die PMT.</param>
        public void ActivatePMTWatchDog( SourceIdentifier service, Action<EPG.Tables.PMT> consumer )
        {
            // Attach to the transport stream parser unit
            var parser = TransportStreamAnalyser.DataManager.TSParser;

            // Deactivate the waiter - will finish all pending operations
            parser.RequestPMT( 0, true );

            // Activate sink
            Interlocked.Exchange( ref m_PMTSink, consumer );

            // Activate the waiter if necessary
            if (service != null)
                parser.RequestPMT( service.Service, true );
        }

        /// <summary>
        /// Hält den Graphen an.
        /// </summary>
        public void Stop()
        {
            // Will ignore any regular error
            m_Graph.Stop();
        }

        /// <summary>
        /// Verbindet zwei Filter. Es wird erwartet, dass jeweils ein einziger Ein-/Ausgang existiert.
        /// </summary>
        /// <param name="from">Der Filter, der die Daten produziert.</param>
        /// <param name="to">Der Filter, der die Daten entgegennimmt.</param>
        private void Connect( TypedComIdentity<IBaseFilter> from, TypedComIdentity<IBaseFilter> to )
        {
            // Attach to pins
            using (var fromPin = from.GetSinglePin( PinDirection.Output ))
            using (var toPin = to.GetSinglePin( PinDirection.Input ))
                ((IGraphBuilder) m_Graph).Connect( fromPin.Interface, toPin.Interface );
        }

        /// <summary>
        /// Erzeugt einen neuen Filter.
        /// </summary>
        /// <param name="name">Der Name des Filters.</param>
        /// <param name="filter">Der bereits geladene Filter.</param>
        /// <returns>Die COM Schnittstelle des neuen Filters.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Filter angegeben.</exception>
        private void AddFilter( string name, TypedComIdentity<IBaseFilter> filter )
        {
            // Validate
            if (filter == null)
                throw new ArgumentNullException( "filter" );

            // Process
            ((IGraphBuilder) m_Graph).AddFilter( filter.Interface, name );
        }

        /// <summary>
        /// Erzeugt einen neuen Filter.
        /// </summary>
        /// <param name="name">Der Name des Filters.</param>
        /// <param name="info">Die Informationen zum Filter.</param>
        /// <returns>Die COM Schnittstelle des neuen Filters.</returns>
        private TypedComIdentity<IBaseFilter> AddFilter( string name, FilterInformation info )
        {
            // Create it
            var filter = info.CreateFilter();
            try
            {
                // Register
                AddFilter( name, filter );

                // Report
                return filter;
            }
            catch
            {
                // Cleanup
                filter.Dispose();

                // Forward
                throw;
            }
        }

        /// <summary>
        /// Fügt einen Filter zum Graphen hinzu.
        /// </summary>
        /// <param name="name">Der Name des Filters.</param>
        /// <param name="moniker">Der eindeutige Name des Filters.</param>
        /// <returns>Eine Referenz auf den neuen Filter.</returns>
        private TypedComIdentity<IBaseFilter> AddFilter( string name, string moniker )
        {
            // Create it
            var filter = ComIdentity.Create<IBaseFilter>( moniker );
            try
            {
                // Register
                AddFilter( name, filter );

                // Report
                return filter;
            }
            catch
            {
                // Cleanup
                filter.Dispose();

                // Forward
                throw;
            }
        }

        /// <summary>
        /// Beendet den Graphen und gibt alle damit verbundenen Ressourcen frei. 
        /// </summary>
        public void Destroy()
        {
            // Disable decryption callback
            Interlocked.Exchange( ref m_PMTSink, null );

            // Pipelines
            DecryptionPipeline.Terminate();
            SignalPipeline.Terminate();
            TunePipeline.Terminate();

            // External registration
            using (m_ExternalRegistration)
                m_ExternalRegistration = null;

            // Extra filters
            var additionalFilters = AdditionalFilters.ToArray();

            // Reset
            AdditionalFilters.Clear();

            // Filter
            foreach (var additionalFilter in additionalFilters)
                if (additionalFilter != null)
                    additionalFilter.Dispose();
            using (m_TIF)
                m_TIF = null;
            using (TransportStreamAnalyser)
                TransportStreamAnalyser = null;
            using (CaptureFilter)
                CaptureFilter = null;
            using (TunerFilter)
                TunerFilter = null;
            using (NetworkProvider)
                NetworkProvider = null;

            // The graph itself
            if (m_Graph != null)
                try
                {
                    // Done with it
                    m_Graph.Stop();
                }
                catch (Exception e)
                {
                    // For now we ignore all errors during shutdown
                    Trace.WriteLine( e.Message );
                }
                finally
                {
                    // Get rid of it
                    BDAEnvironment.Release( ref m_Graph );
                }

            // Log file
            using (var logFile = m_LogFile)
            {
                // Forget
                m_LogFile = null;

                // Make sure that we have written it all out
                if (logFile != null)
                    logFile.Flush();
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        public void Dispose()
        {
            // Use helper
            Destroy();
        }

        #endregion
    }
}
