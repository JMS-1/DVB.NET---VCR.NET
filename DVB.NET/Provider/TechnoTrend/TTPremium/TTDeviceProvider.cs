using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Configuration;

using JMS.TechnoTrend;
using JMS.TechnoTrend.MFCWrapper;
using JMS.DVB.DeviceAccess.Enumerators;

using chman = JMS.ChannelManagement;


namespace JMS.DVB.Provider.TTPremium
{
    /// <summary>
    /// Implementation of the DVB abstraction for the TechnoTrend premium
    /// line hardware - based on the TTDVBACC.DLL API/SDK made available
    /// for .NET applications through the DVB.NET library.
    /// </summary>
    public class TTDeviceProvider : IDeviceProvider
    {
        /// <summary>
        /// List of stations available.
        /// <seealso cref="Channels"/>
        /// </summary>
        private chman.ChannelManager m_Channels = null;

        /// <summary>
        /// Set as soon as the TechnoTrend API/SDK 2.19b is initialized.
        /// </summary>
        private bool m_Registered = false;

        /// <summary>
        /// Do not register with the TechnoTrend API/SDK 2.19b.
        /// </summary>
        private bool m_NoRegister = false;

        /// <summary>
        /// The currently active profile.
        /// </summary>
        private chman.DeviceProfile m_Profile = null;

        /// <summary>
        /// Set to a log file when each method call should be logged.
        /// </summary>
        private string m_MethodLog = null;

        /// <summary>
        /// Device to reset after a wake up from hibernation.
        /// </summary>
        private string m_WakeupDevice = null;

        /// <summary>
        /// Instance name of the reset device.
        /// </summary>
        private string m_WakeupDeviceInstance = null;

        /// <summary>
        /// Create a new TechnoTrend DVB abstraction using the first device and
        /// the default station list.
        /// </summary>
        public TTDeviceProvider()
            : this( 0, null )
        {
        }

        /// <summary>
        /// Create a new TechnoTrend DVB abstraction using the default station list.
        /// </summary>
        /// <param name="args">Dynamic argument list.</param>
        public TTDeviceProvider( Hashtable args )
            : this( ArgumentToDevice( args ), null, false, ArgumentToNetwork( args ), (string) args["WakeupDevice"], (string) args["WakeupDeviceMoniker"] )
        {
        }

        /// <summary>
        /// Create a new TechnoTrend DVB abstraction using the default station list.
        /// </summary>
        /// <param name="device">The zero based index of the device to use.</param>
        public TTDeviceProvider( int device )
            : this( device, null )
        {
        }

        /// <summary>
        /// Create a new TechnoTrend DVB abstraction using the first device.
        /// </summary>
        /// <param name="manager">May preset list of stations to optimize loading.</param>
        public TTDeviceProvider( chman.ChannelManager manager )
            : this( 0, manager )
        {
        }

        /// <summary>
        /// Create a new TechnoTrend DVB abstraction.
        /// </summary>
        /// <param name="device">The zero based index of the device to use.</param>
        /// <param name="manager">May preset list of stations to optimize loading.</param>
        public TTDeviceProvider( int device, chman.ChannelManager manager )
            : this( device, manager, false, true, null, null )
        {
        }

        /// <summary>
        /// Create a new TechnoTrend DVB abstraction.
        /// </summary>
        /// <param name="device">The zero based index of the device to use.</param>
        /// <param name="manager">May preset list of stations to optimize loading.</param>
        /// <param name="noRegister">Set if this instance should not register with the TechnoTrend API (legacy mode).</param>
        /// <param name="withNetwork">Set to initialize including network access.</param>
        /// <param name="wakeupDevice">Device to reset after resume from hibernation.</param>
        protected TTDeviceProvider( int device, chman.ChannelManager manager, bool noRegister, bool withNetwork, string wakeupDevice )
            : this( device, manager, noRegister, withNetwork, wakeupDevice, null )
        {
        }

        /// <summary>
        /// Create a new TechnoTrend DVB abstraction.
        /// </summary>
        /// <param name="device">The zero based index of the device to use.</param>
        /// <param name="manager">May preset list of stations to optimize loading.</param>
        /// <param name="noRegister">Set if this instance should not register with the TechnoTrend API (legacy mode).</param>
        /// <param name="withNetwork">Set to initialize including network access.</param>
        /// <param name="wakeupDevice">Device to reset after resume from hibernation.</param>
        /// <param name="wakeupDeviceMoniker">Instance name of the reset device.</param>
        protected TTDeviceProvider( int device, chman.ChannelManager manager, bool noRegister, bool withNetwork, string wakeupDevice, string wakeupDeviceMoniker )
        {
            // Remember
            m_WakeupDeviceInstance = wakeupDeviceMoniker;
            m_WakeupDevice = wakeupDevice;
            m_NoRegister = noRegister;
            m_Channels = manager;

            // Forward global settings
            Context.WithNetwork = withNetwork;
            Context.DefaultDevice = device;
        }

        /// <summary>
        /// Set if method call logging is enabled.
        /// </summary>
        private bool MethodLog
        {
            get
            {
                // Check name
                if (null == m_MethodLog)
                {
                    // Load
                    m_MethodLog = ConfigurationManager.AppSettings["ProviderMethodLogPath"];

                    // Set default
                    if (null == m_MethodLog) m_MethodLog = string.Empty;
                }

                // Report
                return (m_MethodLog.Length > 0);
            }
        }

        /// <summary>
        /// Report a method call.
        /// </summary>
        /// <param name="format">Message format.</param>
        /// <param name="args">Message parameters.</param>
        private void LogMessage( string format, params object[] args )
        {
            // Forward
            LogMessage( string.Format( format, args ) );
        }

        /// <summary>
        /// Report a message call.
        /// </summary>
        /// <param name="message">Message to report.</param>
        private void LogMessage( string message )
        {
            // Load the file name
            if (!MethodLog) return;

            // Synchronize access
            lock (m_MethodLog)
                using (StreamWriter writer = new StreamWriter( m_MethodLog, true, Encoding.GetEncoding( 1252 ) ))
                    writer.WriteLine( string.Format( "{0} {1}", DateTime.Now, message ) );
        }

        /// <summary>
        /// Register with TechnoTrend drivers if needed.
        /// </summary>
        protected void Register()
        {
            // Did it
            if (m_NoRegister || m_Registered) return;

            // Initialize API
            Context.TheContext.Register( this );

            // We are registered
            m_Registered = true;

            // Attach the DISEqC
            Context.TheContext.DiSEqCConfiguration = Channels.DiSEqCConfiguration;
        }

        /// <summary>
        /// Find a station definition identified by the unique network
        /// identification.
        /// </summary>
        /// <param name="key">The unique identification.</param>
        /// <returns>Will be <i>null</i> if no such station is known.</returns>
        public Station FindStation( Identifier key )
        {
            // Report
            if (MethodLog) LogMessage( "FindStation {0}", key );

            // Forward
            return Channels.Find( key );
        }

        /// <summary>
        /// Stop all filters.
        /// </summary>
        public void StopFilters()
        {
            // Report
            if (MethodLog) LogMessage( "StopFilters" );

            // Forward
            StopFilters( false );
        }

        /// <summary>
        /// Stop all filters.
        /// </summary>
        /// <param name="initialize">Set to reinitialize the hardware.</param>
        public void StopFilters( bool initialize )
        {
            // Report
            if (MethodLog) LogMessage( "StopFilters {0}", initialize );

            // Must register
            Register();

            // Forward
            Context.TheContext.StopFilters( initialize );
        }

        /// <summary>
        /// Switch reception to a given transponder.
        /// </summary>
        /// <param name="transponder">Transponder to use.</param>
        /// <param name="diseqc">Optional DiSEqC configuration for DVB-S.</param>
        public void Tune( Transponder transponder, Satellite.DiSEqC diseqc )
        {
            // Report
            if (MethodLog) LogMessage( "Tune {0} {1}", transponder, diseqc );

            // Must register
            Register();

            // Forward
            Context.TheContext.SetChannel( (Channel) transponder, diseqc );
        }

        /// <summary>
        /// Connect a video and an audio signal to the output window.
        /// </summary>
        /// <param name="videoPID">The video stream identifier (PID).</param>
        /// <param name="audioPID">The audio stream identifier (PID).</param>
        /// <param name="ac3PID">The Dolby Digital (AC3) audio signal to activate.</param>
        public void SetVideoAudio( ushort videoPID, ushort audioPID, ushort ac3PID )
        {
            // Report
            if (MethodLog) LogMessage( "SetAudioVideo {0} {1} {2}", videoPID, audioPID, ac3PID );

            // Must register
            Register();

            // Disable AC3
            if (0 == ac3PID) Context.TheContext.AC3PID = ac3PID;

            // Forward
            Context.TheContext.SetPIDs( audioPID, videoPID );

            // Enable AC3
            if (0 != ac3PID) Context.TheContext.AC3PID = ac3PID;
        }

        /// <summary>
        /// Create a section filter to scan some SI table.
        /// </summary>
        /// <param name="pid">The stream identifier (PID) providing the table.</param>
        /// <param name="callback">Method to call when new data is available.</param>
        /// <param name="filterData">Filter data for pre-selection.</param>
        /// <param name="filterMask">Masks those bits in the filter data for pre-selection
        /// which are relevant for comparision.</param>
        public void StartSectionFilter( ushort pid, FilterHandler callback, byte[] filterData, byte[] filterMask )
        {
            // Report
            if (MethodLog) LogMessage( "StartSectionFilter {0}", pid );

            // Must register
            Register();

            // Attach to filter
            DVBRawFilter filter = Context.TheContext.RawFilter[pid];

            // Configure
            filter.FilterType = FilterType.Section;

            // Attach callback
            filter.SetTarget( callback );

            // Start it
            filter.Start( filterData, filterMask );
        }

        /// <summary>
        /// Filter a single stream to a file.
        /// </summary>
        /// <param name="pid">Stream identifier (PID) to filter.</param>
        /// <param name="video">Set for a video stream to use largest buffer possible.</param>
        /// <param name="path">Full path to the file.</param>
        public void StartFileFilter( ushort pid, bool video, string path )
        {
            // Report
            if (MethodLog) LogMessage( "StartFileFilter {0} {1} {2}", pid, video, path );

            // Must register
            Register();

            // Attach to filter
            DVBRawFilter filter = Context.TheContext.RawFilter[pid];

            // Configure
            filter.FilterType = FilterType.Piping;
            filter.UseSmallBuffer = !video;

            // Attach file
            filter.SetTarget( path );

            // Start it
            filter.Start();
        }

        /// <summary>
        /// Retrieve the number of bytes transferred through this filter.
        /// </summary>
        /// <param name="pid">Stream identifier (PID) to filter.</param>
        /// <returns>Bytes this filter has passed through.</returns>
        public long GetFilterBytes( ushort pid )
        {
            // Report
            if (MethodLog) LogMessage( "GetFilterBytes {0}", pid );

            // Must register
            Register();

            // Report
            return Context.TheContext.RawFilter[pid].Length;
        }

        /// <summary>
        /// Find a station description using the name of the station.
        /// </summary>
        /// <param name="station">Name of the station.</param>
        /// <param name="provider">Optional name of the transponder to use.</param>
        /// <returns>If there is not exactly one element in the <see cref="Array"/>
        /// the station is either unknown or the same name is used in different
        /// providers.</returns>
        public Station[] ResolveStation( string station, string provider )
        {
            // Report
            if (MethodLog) LogMessage( "ResolveStation {0} {1}", station, provider );

            // Forward
            return Channels.Find( station, provider );
        }

        /// <summary>
        /// Report the current list of stations available.
        /// </summary>
        private chman.ChannelManager Channels
        {
            get
            {
                // Create once
                if (null == m_Channels)
                {
                    // The channel configuration
                    chman.ReceiverConfiguration config = null;

                    // Try profile first
                    if (null != m_Profile) config = m_Profile.RealChannels;

                    // Check mode
                    if (null != config)
                    {
                        // Use it
                        m_Channels = config.Channels;
                    }
                    else
                    {
                        // Conventional load
                        m_Channels = new chman.ChannelManager( null );
                    }
                }

                // Report
                return m_Channels;
            }
        }

        /// <summary>
        /// Prepare filtering a DVB stream.
        /// </summary>
        /// <remarks>
        /// Use <see cref="StartFilter"/> to start filtering.
        /// </remarks>
        /// <param name="pid">PID to filter upon.</param>
        /// <param name="video">Set if a video stream is used.</param>
        /// <param name="smallBuffer">Unset if the largest possible buffer should be used.</param>
        /// <param name="callback">Method to call when new data is available.</param>
        public void RegisterPipingFilter( ushort pid, bool video, bool smallBuffer, FilterHandler callback )
        {
            // Report
            if (MethodLog) LogMessage( "RegisterPipingFilter {0} {1} {2}", pid, video, smallBuffer );

            // Must register
            Register();

            // Attach to the filter
            DVBRawFilter filter = Context.TheContext.RawFilter[pid];

            // Already active - do not register again
            if (FilterType.None != filter.FilterType) return;

            // Set the filter type
            filter.FilterType = FilterType.Piping;

            // Comfigure the buffer
            filter.UseSmallBuffer = false;
            filter.UseExplicitBuffer = video ? (smallBuffer ? PipeSize.Sixteen : PipeSize.ThirtyTwo) : (smallBuffer ? PipeSize.Four : PipeSize.Sixteen);

            // Attach callback
            filter.SetTarget( callback );
        }

        /// <summary>
        /// Start filtering a DVB stream.
        /// </summary>
        /// <param name="pid">PID on which the filter runs.</param>
        public void StartFilter( ushort pid )
        {
            // Report
            if (MethodLog) LogMessage( "StartFilter {0}", pid );

            // Must register
            Register();

            // Process
            Context.TheContext.RawFilter[pid].Start();
        }

        /// <summary>
        /// Suspend filtering a DVB stream.
        /// </summary>
        /// <param name="pid">PID on which the filter runs.</param>
        public void StopFilter( ushort pid )
        {
            // Report
            if (MethodLog) LogMessage( "StopFilter {0}", pid );

            // Must register
            Register();

            // Process
            Context.TheContext.RawFilter[pid].Stop();
        }

        /// <summary>
        /// Report all known stations.
        /// </summary>
        public IEnumerable Stations
        {
            get
            {
                // Report
                if (MethodLog) LogMessage( "Stations" );

                // Forward
                return Channels.Stations;
            }
        }

        /// <summary>
        /// Use the argument list provided to check if network
        /// should be enabled.
        /// </summary>
        /// <param name="args">Argeument list mapped.</param>
        /// <returns>Set if network should be used.</returns>
        private static bool ArgumentToNetwork( Hashtable args )
        {
            // Load
            string network = (string) args["Network"];

            // Process - changed default for use with Vista
            return (null == network) ? false : bool.Parse( network );
        }

        /// <summary>
        /// Get the device number from the argument list provided.
        /// </summary>
        /// <param name="args">Argeument list mapped.</param>
        /// <returns>Device number to use.</returns>
        private static int ArgumentToDevice( Hashtable args )
        {
            // Load
            string device = (string) args["Device"];

            // Process
            return (null == device) ? 0 : int.Parse( device );
        }

        /// <summary>
        /// Activate decrypting the indicated station.
        /// </summary>
        /// <param name="station">Some station.</param>
        public void Decrypt( Station station )
        {
            // Report
            if (MethodLog) LogMessage( "Decrypt {0}", station );

            // Must register
            Register();

            // Forward
            if (null == station)
            {
                // Switch off
                Context.TheContext.Decrypt( 0 );
            }
            else
            {
                // Switch on
                Context.TheContext.Decrypt( station.ServiceIdentifier );
            }
        }

        /// <summary>
        /// Report the maximum number of PID filters available.
        /// </summary>
        public int FilterLimit
        {
            get
            {
                // Report
                if (MethodLog) LogMessage( "FilterLimit" );

                // Premium line is quite limited
                return 8;
            }
        }

        /// <summary>
        /// Report the special features supported by this provider.
        /// </summary>
        public virtual ProviderFeatures Features
        {
            get
            {
                // Report
                if (MethodLog) LogMessage( "Features" );

                // Report
                return ProviderFeatures.Decryption | ProviderFeatures.RecordMPEG2 | ProviderFeatures.RecordPVA | ProviderFeatures.VideoDisplayModes | ProviderFeatures.UsesLineIn | ProviderFeatures.CanReInitialize;
            }
        }

        /// <summary>
        /// Report the current hardware connection.
        /// </summary>
        /// <returns>A string representing the hardware connection.</returns>
        public override string ToString()
        {
            // Must register
            Register();

            // Create
            return string.Format( "TechoTrend Premium #{0}/{1} {2}", Context.DefaultDevice, Context.TheContext.NumberOfDevices, Context.TheContext.FrontendType );
        }

        /// <summary>
        /// Signal the begin of a filter registration session.
        /// </summary>
        public void BeginRegister()
        {
            // Report
            if (MethodLog) LogMessage( "BeginRegister" );

        }

        /// <summary>
        /// All filters are now registered.
        /// </summary>
        public void EndRegister()
        {
            // Report
            if (MethodLog) LogMessage( "EndRegister" );

        }

        /// <summary>
        ///  Report the type of the DVB hardware.
        /// </summary>
        public FrontendType ReceiverType
        {
            get
            {
                // Report
                if (MethodLog) LogMessage( "ReceiverType" );

                // Must register
                Register();

                // Report
                return Context.TheContext.FrontendType;
            }
        }

        /// <summary>
        /// Reload the channel definition file on next call.
        /// </summary>
        public void ReloadChannels()
        {
            // Report
            if (MethodLog) LogMessage( "ReloadChannels" );

            // Forget
            m_Channels = null;

            // Not yet connected
            if (!m_Registered) return;

            // Attach the DISEqC
            Context.TheContext.DiSEqCConfiguration = Channels.DiSEqCConfiguration;
        }

        /// <summary>
        /// Get or set the active profile.
        /// </summary>
        public IDeviceProfile Profile
        {
            get
            {
                // Report
                if (MethodLog) LogMessage( "Profile" );

                // Report
                return m_Profile;
            }
            set
            {
                // Update
                m_Profile = (chman.DeviceProfile) value;
            }
        }

        /// <summary>
        /// Führt das Aufwecken eines Gerätes aus.
        /// </summary>
        public void WakeUp()
        {
            // Report
            if (MethodLog) LogMessage( "WakeUp" );

            // Process
            if (!string.IsNullOrEmpty( m_WakeupDeviceInstance ))
                MediaDevice.WakeUpInstance( m_WakeupDeviceInstance );
            else if (!string.IsNullOrEmpty( m_WakeupDevice ))
                MediaDevice.WakeUp( m_WakeupDevice );
        }

        /// <summary>
        /// Meldet Informationen zum aktuellem Emfangsstatus.
        /// </summary>
        public SignalStatus SignalStatus
        {
            get
            {
                // Report
                if (MethodLog) LogMessage( "SignalStatus" );

                // Must register
                Register();

                // Load
                DVBSignalStatus status = Context.TheContext.SignalStatus;

                // Convert
                return new SignalStatus( status.Locked, status.Strength, status.Level );
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Do proper cleanup of related resources.
        /// </summary>
        public virtual void Dispose()
        {
            // Unregister once
            if (!m_Registered)
                return;

            // Be safe
            StopFilters();

            // Reset
            m_Registered = false;

            // Forward
            Context.TheContext.UnRegister( this );
        }

        #endregion
    }
}
