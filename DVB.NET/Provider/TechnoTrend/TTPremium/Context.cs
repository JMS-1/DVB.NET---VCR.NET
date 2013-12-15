using System;
using System.Collections;
using System.IO;
using JMS.TechnoTrend;
using JMS.TechnoTrend.MFCWrapper;


namespace JMS.DVB.Provider.TTPremium
{
    /// <summary>
    /// Manages access to the low level C++ wrappers.
    /// </summary>
    internal class Context : IDisposable
    {
        /// <summary>
        /// The one and only context. It will be created once.
        /// </summary>
        static private Context m_TheContext = null;

        /// <summary>
        /// All boot directories temporarily created.
        /// </summary>
        static private ArrayList m_Directories = new ArrayList();

        /// <summary>
        /// The device to use.
        /// </summary>
        static public int DefaultDevice = 0;

        /// <summary>
        /// Clear to initialize without network access.
        /// </summary>
        static public bool WithNetwork = true;

        /// <summary>
        /// Registered clients of the TTAPI.
        /// </summary>
        private Hashtable m_Users = new Hashtable();

        /// <summary>
        /// Cached A/V control.
        /// </summary>
        private DVBAVControl m_AudioVideo = null;

        /// <summary>
        /// Cached board control.
        /// </summary>
        private DVBBoardControl m_Board = null;

        /// <summary>
        /// Cached frontend instance.
        /// </summary>
        private DVBFrontend m_Frontend = null;

        /// <summary>
        /// Cached CI/CAM interface.
        /// </summary>
        private DVBCommonInterface m_CI = null;

        /// <summary>
        /// Remembers if device has been opened.
        /// </summary>
        private bool m_MustClose = false;

        /// <summary>
        /// Must not create instances outside this class.
        /// </summary>
        private Context()
        {
        }

        /// <summary>
        /// Forward to <see cref="Dispose"/>.
        /// </summary>
        ~Context()
        {
            // Done
            Dispose();
        }

        /// <summary>
        /// Report the <see cref="m_TheContext"/>. This property is protected by
        /// a class lock.
        /// </summary>
        /// <remarks>
        /// The <see cref="m_TheContext"/> is created once. Before the instance
        /// is constructed <see cref="Library.Initialize"/> is called.
        /// </remarks>
        static public Context TheContext
        {
            get
            {
                // Synchronize
                lock (typeof( Context ))
                {
                    // Create once
                    if (null == m_TheContext)
                    {
                        // Initialize library
                        Library.Initialize();

                        // Create singleton
                        m_TheContext = new Context();
                    }
                }

                // Report
                return m_TheContext;
            }
        }

        /// <summary>
        /// Cleanup all access to the various cached instances.
        /// </summary>
        /// <remarks>
        /// The most important task is to call <see cref="Dispose"/> on
        /// all objects cached. After that all references are released.
        /// If <see cref="m_MustClose"/> is set finally <see cref="CloseDevice"/>
        /// is called.
        /// </remarks>
        public void Dispose()
        {
            // Shutdown all
            if (m_AudioVideo != null)
                using (m_AudioVideo)
                {
                    // Cancel AC3
                    AC3PID = 0;

                    // Shut up
                    SetPIDs( 0, 0 );

                    // Done
                    m_AudioVideo = null;
                }
            using (m_CI)
                m_CI = null;
            using (m_Frontend)
                m_Frontend = null;
            using (m_Board)
                m_Board = null;

            // Detach
            m_Users.Clear();
            m_AudioVideo = null;

            // Check for close
            if (m_MustClose) CloseDevice();
        }

        /// <summary>
        /// Read <see cref="m_Board"/>. This property is synchronized.
        /// </summary>
        /// <remarks>
        /// The instance will be created on the first call.
        /// </remarks>
        private DVBBoardControl Board
        {
            get
            {
                // Synchronizes
                lock (this)
                {
                    // Create once
                    if (null == m_Board) m_Board = new DVBBoardControl();
                }

                // Report
                return m_Board;
            }
        }

        /// <summary>
        /// Report the singleton common interface instance.
        /// </summary>
        private DVBCommonInterface CI
        {
            get
            {
                // Synchronize
                lock (this)
                    if (null == m_CI)
                        m_CI = new DVBCommonInterface();

                // Report
                return m_CI;
            }
        }

        /// <summary>
        /// Read <see cref="m_Frontend"/>. This property is synchronized.
        /// </summary>
        /// <remarks>
        /// On the first call the instance is created and <see cref="DVBFrontend.Initialize"/>
        /// will be called automatically.
        /// </remarks>
        private DVBFrontend Frontend
        {
            get
            {
                // Synchronize
                lock (this)
                {
                    // Create once
                    if (null == m_Frontend)
                    {
                        // Attach
                        m_Frontend = new DVBFrontend();

                        // Startup
                        m_Frontend.Initialize();
                    }
                }

                // Report
                return m_Frontend;
            }
        }

        /// <summary>
        /// Read <see cref="m_AudioVideo"/>. This property is synchronized.
        /// </summary>
        /// <remarks>
        /// The instance will be created on the first call. In addition 
        /// <see cref="DVBAVControl.Initialize"/> will be called
        /// automatically.
        /// </remarks>
        private DVBAVControl AudioVideo
        {
            get
            {
                // Synchronize
                lock (this)
                {
                    // Create once
                    if (null == m_AudioVideo)
                    {
                        // Attach
                        m_AudioVideo = new DVBAVControl();

                        // Starup
                        m_AudioVideo.Initialize();
                    }
                }

                // Report
                return m_AudioVideo;
            }
        }

        /// <summary>
        /// Uses <see cref="DVBCommon.CloseDevice"/> to close the current device.
        /// </summary>
        /// <exception cref="DVBException">
        /// Will be called if <see cref="m_MustClose"/> is not set.
        /// </exception>
        public void CloseDevice()
        {
            // Check it
            if (!m_MustClose) throw new DVBException( "No Device open" );

            // Once
            m_MustClose = false;

            // Process
            DVBCommon.CloseDevice();

            // Time to cleanup directories
            foreach (DirectoryInfo dir in m_Directories)
                if (dir.Exists)
                    dir.Delete( true );

            // Forget about it all
            m_Directories.Clear();
        }

        /// <summary>
        /// Open the indicated device using <see cref="DVBCommon.OpenDevice"/>.
        /// </summary>
        /// <param name="lIndex">Zero-based index of the device.</param>
        /// <param name="sApp">Name of the current application - may be null.</param>
        /// <param name="bNoNet">[Not (yet) known]</param>
        /// <exception cref="DVBException">
        /// Thrown when <see cref="m_MustClose"/> is set.
        /// </exception>
        public void OpenDevice( int lIndex, string sApp, bool bNoNet )
        {
            // Already did it
            if (m_MustClose) throw new DVBException( "Must close Device first" );

            // Forward
            DVBCommon.OpenDevice( lIndex, sApp, bNoNet );

            // Remember
            m_MustClose = true;
        }

        /// <summary>
        /// Call the <see cref="OpenDevice(int, string, bool)"/> overload with <i>false</i> as the last parameter.
        /// </summary>
        /// <param name="lIndex">Zero-based index of the device.</param>
        /// <param name="sApp">Name of the current application - may be null.</param>
        public void OpenDevice( int lIndex, string sApp )
        {
            // Forward
            OpenDevice( lIndex, sApp, false );
        }

        /// <summary>
        /// Call the <see cref="OpenDevice(int, string)"/> overload with <i>null</i> as the last parameter.
        /// </summary>
        /// <param name="lIndex">Zero-based index of the device.</param>
        public void OpenDevice( int lIndex )
        {
            // Forward
            OpenDevice( lIndex, null );
        }

        /// <summary>
        /// Forwards to <see cref="DVBCommon.GetNumberOfDevices"/>.
        /// </summary>
        public int NumberOfDevices
        {
            get
            {
                // Forward
                return DVBCommon.GetNumberOfDevices();
            }
        }

        /// <summary>
        /// Forwards to <see cref="DVBBoardControl.Version"/> on <see cref="Board"/>.
        /// </summary>
        public BoardVersion Version
        {
            get
            {
                // Forward
                return Board.Version;
            }
        }

        /// <summary>
        /// Forwards to <see cref="DVBBoardControl.BootARM(out DirectoryInfo)"/> on <see cref="Board"/>.
        /// </summary>
        public void BootARM()
        {
            // New directory to cleanup later on
            DirectoryInfo bootDir = null;

            // Load code
            Board.BootARM( out bootDir );

            // Remember
            if (null != bootDir) m_Directories.Add( bootDir );
        }

        /// <summary>
        /// Prepare a standard connection.
        /// </summary>
        /// <param name="sApp">Current application - may be <i>null</i>.</param>
        public void StartDefault( string sApp )
        {
            // Open default device
            OpenDevice( DefaultDevice, sApp, !WithNetwork );

            // Load code
            BootARM();

            // Time to set up frontend - will automatically initialize
            DVBFrontend pLoad = Frontend;

            // Run with DMA on
            Board.DataDMA = true;

            // On all
            AudioVideo.VideoOutput = JMS.TechnoTrend.VideoOutput.CVBS_RGB;

            // Choose digital source
            if (AVCapabilities.Analog == (AVCapabilities.Analog & AudioVideo.Capabilities)) AudioVideo.ADSwitch = DeviceInput.DVB;

            // Switch off picture and MP2
            SetPIDs( 0, 0 );

            // Switch off dolby
            AC3PID = 0;
        }

        /// <summary>
        /// Forward to <see cref="DVBFrontend.FrontendType"/> on <see cref="Frontend"/>.
        /// </summary>
        public FrontendType FrontendType
        {
            get
            {
                // Forward
                return Frontend.FrontendType;
            }
        }

        /// <summary>
        /// Wählt eine Quellgruppe aus.
        /// </summary>
        /// <param name="group">Díe Daten der Quellgruppe.</param>
        /// <param name="location">Die Wahl des Ursprungs, über den die Quellgruppe empfangen werden kann.</param>
        /// <returns>Gesetzt, wenn es sich um eine DVB-S Quellgruppe handelt.</returns>
        public void SetChannel( SourceGroup group, GroupLocation location )
        {
            // Always stop CI
            Decrypt( 0 );

            // Store
            Frontend.SetChannel( group, location );
        }

        /// <summary>
        /// Start or stop decryption.
        /// </summary>
        /// <param name="serviceIdentifier">May be <i>0</i> to stop decryption. Normally
        /// this will be the service identifier for the program to decrypt.</param>
        public void Decrypt( ushort serviceIdentifier )
        {
            // Start if needed
            if (0 == serviceIdentifier) return;

            // Access CI once
            if (CI.IsIdle) CI.Open();

            // Start decryption
            CI.Decrypt( serviceIdentifier );
        }

        /// <summary>
        /// Legt die Datenströme für die Anzeige fest.
        /// </summary>
        /// <param name="uAudio">Datenstromkennung der Tonspur.</param>
        /// <param name="uVideo">Datenstromkennung der Bildspur.</param>
        public void SetPIDs( ushort uAudio, ushort uVideo )
        {
            // Forward
            AudioVideo.SetPIDs( uAudio, uVideo, 0 );
        }

        /// <summary>
        /// Forwarded to <see cref="DVBAVControl.AC3PID"/> on <see cref="AudioVideo"/>.
        /// </summary>
        public ushort AC3PID
        {
            set
            {
                // Forward
                AudioVideo.AC3PID = value;
            }
        }

        /// <summary>
        /// Add the client reference to the <see cref="m_Users"/>. This method
        /// is synchronized.
        /// </summary>
        /// <remarks>
        /// When the first client is added and <see cref="DVBCommon.IsOpen"/>
        /// reports <i>false</i> <see cref="StartDefault"/> is called with 
        /// a <i>null</i> argument.
        /// <seealso cref="UnRegister"/>
        /// </remarks>
        /// <param name="pClient">Some client used as key.</param>
        public void Register( object pClient )
        {
            // Synchronize
            lock (this)
            {
                // Remember
                m_Users[pClient] = pClient;

                // Already active
                if (m_Users.Count > 1) return;

                // Start us
                if (!DVBCommon.IsOpen) StartDefault( null );
            }
        }

        /// <summary>
        /// Removes a client registered with <see cref="Register"/>.
        /// </summary>
        /// <remarks>
        /// When <see cref="m_Users"/> becomes empty <see cref="Dispose"/>
        /// is called.
        /// </remarks>
        /// <param name="pClient">Some client used as key.</param>
        public void UnRegister( object pClient )
        {
            // Synchronize
            lock (this)
            {
                // Discard
                m_Users.Remove( pClient );

                // Clear
                if (m_Users.Count < 1) Dispose();
            }
        }

        /// <summary>
        /// Report <see cref="DVBFrontend.Filter"/> from our
        /// <see cref="Frontend"/>.
        /// </summary>
        public DVBFrontend._Filters RawFilter
        {
            get
            {
                // Forward
                return Frontend.Filter;
            }
        }

        /// <summary>
        /// Simply call <see cref="DVBFrontend.StopFilters"/> on our <see cref="Frontend"/>.
        /// </summary>
        public void StopFilters()
        {
            // Forward
            StopFilters( false );
        }

        /// <summary>
        /// Simply call <see cref="DVBFrontend.StopFilters"/> on our <see cref="Frontend"/>.
        /// </summary>
        /// <param name="initialize">Set to re-initialize the frontend.</param>
        public void StopFilters( bool initialize )
        {
            // Check mode
            if (initialize)
            {
                // Forward
                Frontend.Initialize();
            }
            else
            {
                // Forward
                Frontend.StopFilters();
            }
        }

        public DVBSignalStatus SignalStatus
        {
            get
            {
                // Forward
                return Frontend.SignalStatus;
            }
        }
    }
}
