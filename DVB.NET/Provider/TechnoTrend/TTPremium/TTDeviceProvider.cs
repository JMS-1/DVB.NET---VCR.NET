using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Configuration;
using JMS.TechnoTrend;
using JMS.TechnoTrend.MFCWrapper;
using JMS.DVB.DeviceAccess.Enumerators;


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
        /// Set as soon as the TechnoTrend API/SDK 2.19b is initialized.
        /// </summary>
        private bool m_Registered = false;

        /// <summary>
        /// Do not register with the TechnoTrend API/SDK 2.19b.
        /// </summary>
        private bool m_NoRegister = false;

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
        /// Create a new TechnoTrend DVB abstraction using the default station list.
        /// </summary>
        /// <param name="args">Dynamic argument list.</param>
        public TTDeviceProvider( Hashtable args )
        {
            // Remember
            m_WakeupDeviceInstance = (string) args["WakeupDeviceMoniker"];
            m_WakeupDevice = (string) args["WakeupDevice"];
            m_NoRegister = false;

            // Forward global settings
            Context.WithNetwork = ArgumentToNetwork( args );
            Context.DefaultDevice = ArgumentToDevice( args );
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
        private void Register()
        {
            // Did it
            if (m_NoRegister || m_Registered) return;

            // Initialize API
            Context.TheContext.Register( this );

            // We are registered
            m_Registered = true;
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
        private void StopFilters( bool initialize )
        {
            // Report
            if (MethodLog) LogMessage( "StopFilters {0}", initialize );

            // Must register
            Register();

            // Forward
            Context.TheContext.StopFilters( initialize );
        }

        /// <summary>
        /// Wählt eine Quellgruppe an.
        /// </summary>
        /// <param name="group">Die Quellgruppe.</param>
        /// <param name="location">Der Ursprung zur Quellgruppe.</param>
        public void Tune( SourceGroup group, GroupLocation location )
        {
            // Report
            if (MethodLog)
                LogMessage( "Tune {0} {1}", group, location );

            // Must register
            Register();

            // Forward
            Context.TheContext.SetChannel( group, location );
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
        public void Decrypt( ushort? station )
        {
            // Report
            if (MethodLog) LogMessage( "Decrypt {0}", station );

            // Must register
            Register();

            // Forward
            if (station.HasValue)
            {
                // Switch on
                Context.TheContext.Decrypt( station.Value );
            }
            else
            {
                // Switch off
                Context.TheContext.Decrypt( 0 );
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
        ///  Report the type of the DVB hardware.
        /// </summary>
        private FrontendType ReceiverType
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
                var status = Context.TheContext.SignalStatus;

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
