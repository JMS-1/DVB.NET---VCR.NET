using System;
using System.Collections;
using JMS.DVB.DeviceAccess.Enumerators;
using JMS.TechnoTrend;
using JMS.TechnoTrend.MFCWrapper;


namespace JMS.DVB.Provider.TTPremium
{
    /// <summary>
    /// Implementation of the DVB abstraction for the TechnoTrend premium
    /// line hardware - based on the TTDVBACC.DLL API/SDK made available
    /// for .NET applications through the DVB.NET library.
    /// </summary>
    public class TTDeviceProvider : ILegacyDevice
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
        /// Register with TechnoTrend drivers if needed.
        /// </summary>
        private void Register()
        {
            // Did it
            if (m_NoRegister)
                return;
            if (m_Registered)
                return;

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
            // Forward
            StopFilters( false );
        }

        /// <summary>
        /// Stop all filters.
        /// </summary>
        /// <param name="initialize">Set to reinitialize the hardware.</param>
        private void StopFilters( bool initialize )
        {
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
        public void SetVideoAudio( ushort videoPID, ushort audioPID )
        {
            // Must register
            Register();

            // Forward
            Context.TheContext.SetPIDs( audioPID, videoPID );
        }

        /// <summary>
        /// Create a section filter to scan some SI table.
        /// </summary>
        /// <param name="pid">The stream identifier (PID) providing the table.</param>
        /// <param name="callback">Method to call when new data is available.</param>
        /// <param name="filterData">Filter data for pre-selection.</param>
        /// <param name="filterMask">Masks those bits in the filter data for pre-selection
        /// which are relevant for comparision.</param>
        public void StartSectionFilter( ushort pid, Action<byte[]> callback, byte[] filterData, byte[] filterMask )
        {
            // Must register
            Register();

            // Attach to filter
            var filter = Context.TheContext.RawFilter[pid];

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
        public void RegisterPipingFilter( ushort pid, bool video, bool smallBuffer, Action<byte[]> callback )
        {
            // Must register
            Register();

            // Attach to the filter
            var filter = Context.TheContext.RawFilter[pid];

            // Already active - do not register again
            if (filter.FilterType != FilterType.None)
                return;

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
            bool network;
            if (bool.TryParse( (string) args["Network"], out network ))
                return network;
            else
                return false;
        }

        /// <summary>
        /// Get the device number from the argument list provided.
        /// </summary>
        /// <param name="args">Argeument list mapped.</param>
        /// <returns>Device number to use.</returns>
        private static int ArgumentToDevice( Hashtable args )
        {
            // Load
            int device;
            if (int.TryParse( (string) args["Device"], out device ))
                return device;
            else
                return 0;
        }

        /// <summary>
        /// Activate decrypting the indicated station.
        /// </summary>
        /// <param name="station">Some station.</param>
        public void Decrypt( ushort? station )
        {
            // Must register
            Register();

            // Forward
            if (station.HasValue)
                Context.TheContext.Decrypt( station.Value );
            else
                Context.TheContext.Decrypt( 0 );
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
            // Process
            if (!string.IsNullOrEmpty( m_WakeupDeviceInstance ))
                MediaDevice.WakeUpInstance( m_WakeupDeviceInstance );
            else if (!string.IsNullOrEmpty( m_WakeupDevice ))
                MediaDevice.WakeUp( m_WakeupDevice );
        }

        /// <summary>
        /// Meldet Informationen zum aktuellem Emfangsstatus.
        /// </summary>
        public JMS.DVB.DeviceAccess.Interfaces.SignalStatus SignalStatus
        {
            get
            {
                // Must register
                Register();

                // Load
                var status = Context.TheContext.SignalStatus;

                // Convert
                return new JMS.DVB.DeviceAccess.Interfaces.SignalStatus( status.Locked, status.Strength, status.Level );
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
