using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using JMS.DVB;
using JMS.DVB.DeviceAccess;
using ScanLog = JMS.DVB.EPG.Tools;


namespace JMS.TechnoTrend.MFCWrapper
{
    /// <summary>
    /// Wraps the <i>CDVBFrontend</i> from the TTAPI C++ SDK. For details of the functionality
    /// see there.
    /// </summary>
    /// <remarks>
    /// The client is recommended to use the <see cref="Dispose"/> method for cleanup as
    /// soon as the C++ instance is not longer needed.
    /// </remarks>
    public class DVBFrontend : IDisposable
    {
        /// <summary>
        /// Send a DiSEqC message.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?SendDiSEqCMsg@CDVBFrontend@@QAE?AW4DVB_ERROR@@PAEEE@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBFrontend_SendDiSEqCMsg( IntPtr pData, byte[] pMessage, byte bLen, byte bToneBurst );

        /// <summary>
        /// Choose a satellite channel.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?SetChannel@CDVBFrontend@@QAE?AW4DVB_ERROR@@U_CHANNEL_TYPE@1@H@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBFrontend_SetChannel( IntPtr pData, Channel_S rChannel, bool bPowerOnly );

        /// <summary>
        /// Retrieve the current satellite channel.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?GetChannel@CDVBFrontend@@QAE?AW4DVB_ERROR@@AAU_CHANNEL_TYPE@1@@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBFrontend_GetChannel( IntPtr pData, out Channel_S rChannel );

        /// <summary>
        /// Choose a cable channel.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?SetChannel@CDVBFrontend@@QAE?AW4DVB_ERROR@@U_CHANNEL_TYPE@1@H@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBFrontend_SetChannel( IntPtr pData, Channel_C rChannel, bool bPowerOnly );

        /// <summary>
        /// Retrieve the current cable channel.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?GetChannel@CDVBFrontend@@QAE?AW4DVB_ERROR@@AAU_CHANNEL_TYPE@1@@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBFrontend_GetChannel( IntPtr pData, out Channel_C rChannel );

        /// <summary>
        /// Choose a terrestrial channel.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?SetChannel@CDVBFrontend@@QAE?AW4DVB_ERROR@@U_CHANNEL_TYPE@1@H@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBFrontend_SetChannel( IntPtr pData, Channel_T rChannel, bool bPowerOnly );

        /// <summary>
        /// Retrieve the current terrestrical channel.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?GetChannel@CDVBFrontend@@QAE?AW4DVB_ERROR@@AAU_CHANNEL_TYPE@1@@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBFrontend_GetChannel( IntPtr pData, out Channel_T rChannel );

        /// <summary>
        /// Read the capabilities.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?GetCapabilities@CDVBFrontend@@QAEKXZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern FrontendCapabilities CDVBFrontend_GetCapabilities( IntPtr pData );

        /// <summary>
        /// Load the type of the frontend.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?GetType@CDVBFrontend@@QAE?AW4_FRONTEND_TYPE@1@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern FrontendType CDVBFrontend_GetType( IntPtr pData );

        /// <summary>
        /// Initalize the frontend.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?Init@CDVBFrontend@@QAE?AW4DVB_ERROR@@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBFrontend_Init( IntPtr pData );

        /// <summary>
        /// Construct the C++ instance.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "??0CDVBFrontend@@QAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBFrontend_Construct( IntPtr pData );

        /// <summary>
        /// Destruct the C++ instance.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "??1CDVBFrontend@@UAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void CDVBFrontend_Destruct( IntPtr pData );

        /// <summary>
        /// Read the state of the tuner.
        /// </summary>
        [DllImport( "ttdvbacc.dll", EntryPoint = "?GetState@CDVBFrontend@@QAE?AW4DVB_ERROR@@AAHPAU_SIGNAL_TYPE@1@PAU_LOCK_STATE@1@@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError CDVBFrontend_GetState( IntPtr pData, out bool locked, out SignalStatus signal, out LockState state );

        /// <summary>
        /// Makes <see cref="DVBFrontend.Filter"/> look like an <see cref="Array"/>.
        /// </summary>
        public class _Filters
        {
            /// <summary>
            /// The map we manage.
            /// </summary>
            private Dictionary<ushort, DVBRawFilter> m_Filters;

            /// <summary>
            /// The related frontend.
            /// </summary>
            private DVBFrontend m_Frontend;

            /// <summary>
            /// Create the accessor.
            /// </summary>
            /// <param name="pFilters">The map we manage.</param>
            /// <param name="frontend">Das zugehörige Frontend.</param>
            internal _Filters( Dictionary<ushort, DVBRawFilter> pFilters, DVBFrontend frontend )
            {
                // Remember
                m_Frontend = frontend;
                m_Filters = pFilters;
            }

            /// <summary>
            /// Create the indicated <see cref="DVBRawFilter"/> from
            /// <see cref="m_Filters"/>. If none exists a new one will
            /// be created and added to the <see cref="Hashtable"/>.
            /// </summary>
            public DVBRawFilter this[ushort uPID]
            {
                get
                {
                    // Synchronize
                    lock (m_Filters)
                    {
                        // Retrieve
                        DVBRawFilter pFilter;
                        if (!m_Filters.TryGetValue( uPID, out pFilter ))
                        {
                            // Create
                            pFilter = new DVBRawFilter( uPID, m_Frontend );

                            // Remember
                            m_Filters[uPID] = pFilter;

                            // Report
                            ScanLog.WriteToScanLog( "Adding Filter {0}", uPID );
                        }

                        // Report
                        return pFilter;
                    }
                }
            }
        }

        /// <summary>
        /// Empty data helper for DiSEqC messages.
        /// </summary>
        private static readonly byte[] m_NoData = { };

        /// <summary>
        /// Holder of the C++ instance.
        /// </summary>
        private ClassHolder m_Class = null;

        /// <summary>
        /// All <see cref="DVBRawFilter"/> instances indexed by its <see cref="DVBRawFilter.FilterPID"/>.
        /// </summary>
        private Dictionary<ushort, DVBRawFilter> m_Filters = new Dictionary<ushort, DVBRawFilter>();

        /// <summary>
        /// Die zuletzt versendete Antennensteuerung.
        /// </summary>
        private StandardDiSEqC m_lastMessage;

        /// <summary>
        /// Simply use the <see cref="ClassHolder"/> default constructor to create
        /// the C++ instance.
        /// </summary>
        public DVBFrontend()
        {
            // Create holder
            m_Class = new ClassHolder( LegacySize.CDVBFrontend );

            // Construct it
            CDVBFrontend_Construct( m_Class.ClassPointer );

            // Prepare for destruction
            m_Class.Destructor = new ClassHolder.DestructHandler( CDVBFrontend_Destruct );
        }

        /// <summary>
        /// Forward to <see cref="Dispose"/>.
        /// </summary>
        ~DVBFrontend()
        {
            // Detach
            Dispose();
        }

        /// <summary>
        /// Initialize the frontend.
        /// </summary>
        /// <remarks>
        /// When the C++ method invocation reports a hardware error the call is repeated
        /// up to three times with a <see cref="Thread.Sleep(int)"/>(500) between the calls. 
        /// If the error holds a <see cref="DVBException"/> is thrown.
        /// <see cref="StopFilters"/>
        /// </remarks>
        /// <exception cref="DVBException">
        /// Thrown when the C++ method invocation reports some <see cref="DVBError"/>.
        /// </exception>
        public void Initialize()
        {
            // Time to stop all filters
            StopFilters();

            // Execute
            DVBError eCode = CDVBFrontend_Init( m_Class.ClassPointer );

            // Retry
            for (int nRetry = 3; (DVBError.Hardware == eCode) && (nRetry-- > 0); )
            {
                // Delay
                Thread.Sleep( 500 );

                // Try again
                eCode = CDVBFrontend_Init( m_Class.ClassPointer );
            }

            // Validate
            if (DVBError.None != eCode) throw new DVBException( "Can not initialize Frontend", eCode );

            // Reset
            m_lastMessage = null;
        }

        /// <summary>
        /// Simply forward to <see cref="ClassHolder.Dispose"/> on <see cref="m_Class"/>
        /// when called for the first time.
        /// <see cref="StopFilters"/>
        /// </summary>
        public void Dispose()
        {
            // Time to stop all filters
            StopFilters();

            // Forward
            if (null != m_Class) m_Class.Dispose();

            // Once
            m_Class = null;

            // No need to finalize
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Retrieve the type of the frontend.
        /// </summary>
        public FrontendType FrontendType { get { return CDVBFrontend_GetType( m_Class.ClassPointer ); } }

        /// <summary>
        /// Retrieve the capabiltites of the frontend.
        /// </summary>
        public FrontendCapabilities Capabilities { get { return CDVBFrontend_GetCapabilities( m_Class.ClassPointer ); } }

        /// <summary>
        /// Wählt eine Quellgruppe aus.
        /// </summary>
        /// <param name="group">Díe Daten der Quellgruppe.</param>
        /// <param name="location">Die Wahl des Ursprungs, über den die Quellgruppe empfangen werden kann.</param>
        /// <returns>Gesetzt, wenn es sich um eine DVB-S Quellgruppe handelt.</returns>
        public void SetChannel( SourceGroup group, GroupLocation location )
        {
            // Stop all filters
            StopFilters();

            // May repeat some times
            for (int nRetry = 3; nRetry-- > 0; )
            {
                // Supported types in order of importance
                if (!SendChannel( group as SatelliteGroup, location as SatelliteLocation ))
                    if (!SendChannel( group as CableGroup ))
                        if (!SendChannel( group as TerrestrialGroup ))
                            throw new DVBException( "Unsupported Channel Type " + group.GetType().FullName );

                // Wait for the signal
                for (int n = 10; n-- > 0; Thread.Sleep( 50 ))
                    if (SignalStatus.Locked)
                        return;

                // Force to resend DiSEqC command
                m_lastMessage = null;
            }
        }

        public DVBSignalStatus SignalStatus
        {
            get
            {
                // Lock data
                SignalStatus signal;
                LockState state;
                bool locked;

                // Retrieve
                DVBException.ThrowOnError( CDVBFrontend_GetState( m_Class.ClassPointer, out locked, out signal, out state ), "Unable to request signal status" );

                // Report
                return new DVBSignalStatus( locked && state.FrontendLocked, signal.Level, signal.Quality, signal.BER, signal.dBLevel, signal.RawQuality );
            }
        }

        /// <summary>
        /// Generic helper for the overloads.
        /// </summary>
        /// <remarks>
        /// Actually forwards to <see cref="DVBException.ThrowOnError"/>.
        /// </remarks>
        /// <param name="eCode">Error code from some C++ method invocation.</param>
        /// <exception cref="DVBException">
        /// Thrown when the C++ method invocation reports some <see cref="DVBError"/>.
        /// </exception>
        private void CheckChannel( DVBError eCode )
        {
            // Validate
            DVBException.ThrowOnError( eCode, "Unable to Set Channel" );
        }

        /// <summary>
        /// Wählt eine Quellgruppe aus.
        /// </summary>
        /// <param name="group">Díe Daten der Quellgruppe.</param>
        /// <param name="location">Die Wahl des Ursprungs, über den die Quellgruppe empfangen werden kann.</param>
        /// <returns>Gesetzt, wenn es sich um eine DVB-S Quellgruppe handelt.</returns>
        private bool SendChannel( SatelliteGroup group, SatelliteLocation location )
        {
            // Not us
            if (location == null)
                return false;
            if (group == null)
                return false;

            // Validate
            if (FrontendType != FrontendType.Satellite)
                throw new DVBException( "Expected " + FrontendType.ToString() + " Channel" );

            // Create channel
            var channel =
                new Channel_S
                {
                    Inversion = SpectrumInversion.Auto,
                    SymbolRate = group.SymbolRate,
                    Frequency = group.Frequency,
                };

            // Attach to the DiSEqC setting
            var selector = StandardDiSEqC.FromSourceGroup( group, location );

            // See if the message is different from the last one
            if (!selector.Equals( m_lastMessage ))
            {
                // Remember
                m_lastMessage = selector.Clone();

                // As long as necessary
                for (int nCount = selector.Repeat; nCount-- > 0; Thread.Sleep( 120 ))
                {
                    // Send it
                    DVBException.ThrowOnError( CDVBFrontend_SendDiSEqCMsg( m_Class.ClassPointer, selector.Request, (byte) selector.Request.Length, selector.Burst ), "Could not send DiSEqC Message" );

                    // Set repeat flag
                    if (selector.Request.Length > 0)
                        selector.Request[0] |= 1;
                }
            }

            // Calculated items
            channel.b22kHz = (group.Frequency >= location.SwitchFrequency) ? 1 : 0;
            channel.LOF = (0 == channel.b22kHz) ? location.Frequency1 : location.Frequency2;

            // Power modes
            switch (group.Polarization)
            {
                case Polarizations.Horizontal: channel.LNBPower = PowerMode.Horizontal; break;
                case Polarizations.Vertical: channel.LNBPower = PowerMode.Vertical; break;
                case Polarizations.NotDefined: channel.LNBPower = PowerMode.Off; break;
                default: throw new ArgumentException( group.Polarization.ToString(), "Polarization" );
            }

            // Process
            CheckChannel( CDVBFrontend_SetChannel( m_Class.ClassPointer, channel, false ) );

            // Check up for synchronisation
            Channel_S val1, val2;

            // Get channel twice
            CheckChannel( CDVBFrontend_GetChannel( m_Class.ClassPointer, out val1 ) );
            CheckChannel( CDVBFrontend_GetChannel( m_Class.ClassPointer, out val2 ) );

            // Did it
            return true;
        }

        /// <summary>
        /// Wählt eine Quellgruppe an.
        /// </summary>
        /// <param name="group">Die Daten zur Quellgruppe.</param>
        /// <returns>Gesetzt, wenn es sich um eine DVB-C Quellgruppe handelt.</returns>
        private bool SendChannel( CableGroup group )
        {
            // Not us
            if (group == null)
                return false;

            // Validate
            if (FrontendType != FrontendType.Cable)
                throw new DVBException( "Expected " + FrontendType.ToString() + " Channel" );

            // Helper
            var channel =
                new Channel_C
                {
                    Frequency = group.Frequency,
                    SymbolRate = group.SymbolRate,
                };

            // Spectrum inversion
            switch (group.SpectrumInversion)
            {
                case SpectrumInversions.On: channel.Inversion = SpectrumInversion.On; break;
                case SpectrumInversions.Off: channel.Inversion = SpectrumInversion.Off; break;
                case SpectrumInversions.Auto: channel.Inversion = SpectrumInversion.Auto; break;
                default: channel.Inversion = SpectrumInversion.Auto; break;
            }

            // Modulation
            switch (group.Modulation)
            {
                case CableModulations.QAM16: channel.Qam = Qam.Qam16; break;
                case CableModulations.QAM32: channel.Qam = Qam.Qam32; break;
                case CableModulations.QAM64: channel.Qam = Qam.Qam64; break;
                case CableModulations.QAM128: channel.Qam = Qam.Qam128; break;
                case CableModulations.QAM256: channel.Qam = Qam.Qam256; break;
                default: channel.Qam = Qam.Qam64; break;
            }

            // Check supported modes
            switch (group.Bandwidth)
            {
                case Bandwidths.Six: channel.Bandwidth = BandwidthType.Six; break;
                case Bandwidths.Seven: channel.Bandwidth = BandwidthType.Seven; break;
                case Bandwidths.Eight: channel.Bandwidth = BandwidthType.Eight; break;
                case Bandwidths.NotDefined: channel.Bandwidth = BandwidthType.None; break;
                default: channel.Bandwidth = BandwidthType.Auto; break;
            }

            // Process
            CheckChannel( CDVBFrontend_SetChannel( m_Class.ClassPointer, channel, false ) );

            // Check up for synchronisation
            Channel_C val1, val2;

            // Get channel twice
            CheckChannel( CDVBFrontend_GetChannel( m_Class.ClassPointer, out val1 ) );
            CheckChannel( CDVBFrontend_GetChannel( m_Class.ClassPointer, out val2 ) );

            // Did it
            return true;
        }

        /// <summary>
        /// Wählt eine Quellgruppe an.
        /// </summary>
        /// <param name="group">Die Daten zur Quellgruppe.</param>
        /// <returns>Gesetzt, wenn es sich um eine DVB-T Quellgruppe handelt.</returns>
        private bool SendChannel( TerrestrialGroup group )
        {
            // Not us 
            if (group == null)
                return false;

            // Validate
            if (FrontendType != FrontendType.Terrestrial)
                throw new DVBException( "Expected " + FrontendType.ToString() + " Channel" );

            // Helper
            var channel =
                new Channel_T
                {
                    Frequency = group.Frequency,
                    Inversion = SpectrumInversion.Off,
                    Scan = false,
                };

            // Check supported modes
            switch (group.Bandwidth)
            {
                case Bandwidths.Six: channel.Bandwidth = BandwidthType.Six; break;
                case Bandwidths.Seven: channel.Bandwidth = BandwidthType.Seven; break;
                case Bandwidths.Eight: channel.Bandwidth = BandwidthType.Eight; break;
                case Bandwidths.NotDefined: channel.Bandwidth = BandwidthType.None; break;
                default: channel.Bandwidth = BandwidthType.Auto; break;
            }

            // Process
            CheckChannel( CDVBFrontend_SetChannel( m_Class.ClassPointer, channel, false ) );

            // Check up for synchronisation
            Channel_T rVal1, rVal2;

            // Get channel twice
            CheckChannel( CDVBFrontend_GetChannel( m_Class.ClassPointer, out rVal1 ) );
            CheckChannel( CDVBFrontend_GetChannel( m_Class.ClassPointer, out rVal2 ) );

            // Did it
            return true;
        }

        /// <summary>
        /// Calls <see cref="IDisposable.Dispose"/> on each entry in <see cref="m_Filters"/>
        /// followed by a <see cref="Hashtable.Clear"/>.
        /// </summary>
        public void StopFilters()
        {
            // Copy
            List<DVBRawFilter> filters = new List<DVBRawFilter>();

            // Synchronize
            lock (m_Filters)
            {
                // Copy over
                filters.AddRange( m_Filters.Values );

                // Reset
                m_Filters.Clear();

                // Report
                ScanLog.WriteToScanLog( "All Filters removed" );
            }

            // Process all filters
            foreach (IDisposable filter in filters) filter.Dispose();
        }

        /// <summary>
        /// Create an <see cref="_Filters"/> accessor instance on <see cref="m_Filters"/>.
        /// </summary>
        public _Filters Filter { get { return new _Filters( m_Filters, this ); } }

        /// <summary>
        /// Remove a filter from the map.
        /// </summary>
        /// <param name="pid">The identifier of the stream.</param>
        internal void RemoveFilter( ushort pid )
        {
            // Do it
            lock (m_Filters)
            {
                // Process
                m_Filters.Remove( pid );

                // Report
                ScanLog.WriteToScanLog( "Filter {0} removed", pid );
            }
        }
    }
}
