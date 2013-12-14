extern alias oldVersion;

using System;
using System.Security;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using JMS.TechnoTrend;
using JMS.DVB.DeviceAccess.Enumerators;

using legacy = oldVersion.JMS.DVB;


namespace JMS.DVB.Provider.TTBudget
{
    public class BudgetProvider : legacy.IDeviceProvider
    {
        [DllImport( "ttlcdacc.dll", EntryPoint = "?InitDvbApiDll@@YAXXZ", ExactSpelling = true )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void InitDvbApiDll();

        private static bool m_Initialized = false;

        private Dictionary<ushort, Filter> m_Filters = new Dictionary<ushort, Filter>();

        private Common m_DeviceManager = null;

        private BoardControl m_Board = null;

        private Frontend m_Frontend = null;

        private CommonInterface m_CI = null;

        private int m_DeviceIndex = 0;

        private string m_WakeupDevice = null;

        private string m_WakeupDeviceInstance = null;

        public BudgetProvider()
            : this( 0 )
        {
        }

        public BudgetProvider( Hashtable args )
            : this( ArgumentToDevice( args ), (string) args["WakeupDevice"], (string) args["WakeupDeviceMoniker"] )
        {
        }

        public BudgetProvider( int device )
            : this( device, null, null )
        {
        }

        public BudgetProvider( int device, string wakeupDevice )
            : this( device, wakeupDevice, null )
        {
        }

        public BudgetProvider( int device, string wakeupDevice, string wakeupDeviceMoniker )
        {
            // Remember
            m_WakeupDeviceInstance = wakeupDeviceMoniker;
            m_WakeupDevice = wakeupDevice;
            m_DeviceIndex = device;
        }

        public legacy.SignalStatus SignalStatus
        {
            get
            {
                // Must access hardware
                Open();

                // Load
                DVBSignalStatus status = m_Frontend.SignalStatus;

                // Convert
                return new legacy.SignalStatus( status.Locked, status.Strength, status.Quality );
            }
        }

        private static int ArgumentToDevice( Hashtable args )
        {
            // Load
            string device = (string) args["Device"];

            // Process
            return (null == device) ? 0 : int.Parse( device );
        }

        private void Open()
        {
            // Already did it
            if (null != m_DeviceManager) return;

            // Once only
            if (!m_Initialized)
            {
                // Lock out
                m_Initialized = true;

                // Call initializer
                InitDvbApiDll();
            }

            // Create device manager
            m_DeviceManager = new Common();

            // Open the hardware channel
            m_DeviceManager.Open( m_DeviceIndex );

            // Attach to board
            m_Board = new BoardControl();

            // Do a full reset
            m_Board.Initialize();

            // Attach to frontend
            m_Frontend = new Frontend();

            // Start it up
            m_Frontend.Initialize();

            // Enable DMA access
            m_Board.EnableDMA();

            // Create CI accessor
            m_CI = new CommonInterface();
        }

        public override string ToString()
        {
            // Must access hardware
            Open();

            // Report
            return string.Format( "TechnoTrend Budget #{0}/{1} {2}", m_DeviceIndex, m_DeviceManager.Count, ReceiverType );
        }

        #region IDeviceProvider Members

        public void BeginRegister()
        {
        }

        public void Decrypt( legacy.Station station )
        {
            // Connect to hardware
            Open();

            // Forward
            if (null == station)
            {
                // Switch off
                m_CI.Decrypt( 0 );
            }
            else
            {
                // Switch on
                m_CI.Decrypt( station.ServiceIdentifier );
            }
        }

        public void EndRegister()
        {
        }

        public legacy.ProviderFeatures Features
        {
            get
            {
                // Report
                return legacy.ProviderFeatures.Decryption;
            }
        }

        public int FilterLimit
        {
            get
            {
                // Report
                return 200;
            }
        }

        public long GetFilterBytes( ushort pid )
        {
            // Ask filter
            return m_Filters[pid].Length;
        }

        public legacy.FrontendType ReceiverType
        {
            get
            {
                // Connect to hardware
                Open();

                // Report
                return m_Frontend.FrontendType;
            }
        }

        public void RegisterPipingFilter( ushort pid, bool video, bool smallBuffer, legacy.FilterHandler callback )
        {
            // Attach to hardware
            Open();

            // See if filter already exists
            if (m_Filters.ContainsKey( pid )) return;

            // Create new
            m_Filters[pid] = new FilterToCode( pid, callback );
        }

        public void SetVideoAudio( ushort videoPID, ushort audioPID, ushort ac3PID )
        {
            // Validate
            if (0 != videoPID) throw new ArgumentException( videoPID.ToString(), "videoPID" );
            if (0 != audioPID) throw new ArgumentException( audioPID.ToString(), "audioPID" );
            if (0 != ac3PID) throw new ArgumentException( ac3PID.ToString(), "ac3PID" );

            // Startup API
            Open();
        }

        public void StartFileFilter( ushort pid, bool video, string path )
        {
            // Attach to hardware
            Open();

            // Stop if running
            StopFilter( pid );

            // Create filter
            FilterToFile filter = new FilterToFile( pid, path );

            // Remember
            m_Filters[pid] = filter;

            // Start at once
            filter.Start();
        }

        public void StartFilter( ushort pid )
        {
            // Forward
            m_Filters[pid].Start();
        }

        public void StartSectionFilter( ushort pid, legacy.FilterHandler callback, byte[] filterData, byte[] filterMask )
        {
            // Attach to hardware
            Open();

            // Stop if running
            StopFilter( pid );

            // Create new
            FilterToCode filter = new FilterToCode( pid, callback );

            // Remember
            m_Filters[pid] = filter;

            // Start at once
            filter.Start( filterData, filterMask );
        }

        public void StopFilter( ushort pid )
        {
            // Load it
            Filter filter;
            if (!m_Filters.TryGetValue( pid, out filter )) return;

            // Remove
            m_Filters.Remove( pid );

            // Shut down
            filter.Dispose();
        }

        public void StopFilters()
        {
            // Forward
            StopFilters( false );
        }

        public void StopFilters( bool initialize )
        {
            // Load
            List<Filter> filters = new List<Filter>( m_Filters.Values );

            // Reset
            m_Filters.Clear();

            // Process
            foreach (Filter filter in filters)
                try
                {
                    // Process
                    filter.Dispose();
                }
                catch
                {
                    // Ignore any error
                }
        }

        public void Tune( legacy.Transponder transponder, legacy.Satellite.DiSEqC diseqc )
        {
            // Attach to hardware
            Open();

            // Invalidate all filters
            StopFilters();

            // Send request
            m_Frontend.Tune( transponder, diseqc );
        }

        public void WakeUp()
        {
            // Process
            if (!string.IsNullOrEmpty( m_WakeupDeviceInstance ))
                MediaDevice.WakeUpInstance( m_WakeupDeviceInstance );
            else if (!string.IsNullOrEmpty( m_WakeupDevice ))
                MediaDevice.WakeUp( m_WakeupDevice );
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            // Stop all filters
            StopFilters();

            // Forward to all
            if (null != m_CI)
            {
                // Try to close
                m_CI.Dispose();

                // Forget
                m_CI = null;
            }
            if (null != m_Frontend)
            {
                // Try to close
                m_Frontend.Dispose();

                // Forget
                m_Frontend = null;
            }
            if (null != m_Board)
            {
                // Try to close
                m_Board.Dispose();

                // Forget
                m_Board = null;
            }
            if (null != m_DeviceManager)
            {
                // Try to close
                m_DeviceManager.Dispose();

                // Forget
                m_DeviceManager = null;
            }
        }

        #endregion
    }
}
