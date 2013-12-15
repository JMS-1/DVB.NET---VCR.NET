using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using JMS.DVB;
using JMS.DVB.DeviceAccess;
using JMS.TechnoTrend;
using JMS.TechnoTrend.MFCWrapper;


namespace JMS.DVB.Provider.TTBudget
{
    internal class Frontend : IDisposable
    {
        [DllImport( "ttlcdacc.dll", EntryPoint = "??0CDVBFrontend@@QAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void _Construct( IntPtr classPointer );

        [DllImport( "ttlcdacc.dll", EntryPoint = "??1CDVBFrontend@@UAE@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern void _Destruct( IntPtr classPointer );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?Init@CDVBFrontend@@QAE?AW4DVB_ERROR@@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _Init( IntPtr classPointer );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?GetType@CDVBFrontend@@QAE?AW4_FRONTEND_TYPE@1@XZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern FrontendType _GetType( IntPtr classPointer );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?GetCapabilities@CDVBFrontend@@QAEKXZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern FrontendCapabilities _GetCapabilities( IntPtr classPointer );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?GetState@CDVBFrontend@@QAE?AW4DVB_ERROR@@AAHPAU_SIGNAL_TYPE@1@PAU_LOCK_STATE@1@@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _GetState( IntPtr classPointer, out bool locked, out TechnoTrend.MFCWrapper.SignalStatus signal, out LockState state );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?SendDiSEqCMsg@CDVBFrontend@@QAE?AW4DVB_ERROR@@PAEEE@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _SendDiSEqCMsg( IntPtr classPointer, byte[] pMessage, byte bLen, byte bToneBurst );

        private ClassHolder m_Class = null;

        private StandardDiSEqC m_lastMessage = null;

        public Frontend()
        {
            // Create the MFC wrapper
            m_Class = new ClassHolder( TTBudget.LegacySize.CDVBFrontend );

            // Construct C++ instance
            _Construct( m_Class.ClassPointer );

            // Attach destructor
            m_Class.Destructor = new ClassHolder.DestructHandler( _Destruct );
        }

        internal IntPtr ClassPointer
        {
            get
            {
                // Report
                return m_Class.ClassPointer;
            }
        }

        public void Initialize()
        {
            // Execute
            DVBError errorCode = _Init( m_Class.ClassPointer );

            // Retry
            for (int retryCount = 3; (DVBError.Hardware == errorCode) && (retryCount-- > 0); )
            {
                // Delay
                Thread.Sleep( 500 );

                // Try again
                errorCode = _Init( m_Class.ClassPointer );
            }

            // Validate
            if (DVBError.None != errorCode) throw new DVBException( "Can not initialize frontend", errorCode );
        }

        public FrontendType FrontendType { get { return _GetType( m_Class.ClassPointer ); } }

        public FrontendCapabilities Capabilities { get { return _GetCapabilities( m_Class.ClassPointer ); } }

        /// <summary>
        /// Wählt eine Quellgruppe aus.
        /// </summary>
        /// <param name="group">Díe Daten der Quellgruppe.</param>
        /// <param name="location">Die Wahl des Ursprungs, über den die Quellgruppe empfangen werden kann.</param>
        public void Tune( SourceGroup group, GroupLocation location )
        {
            // May repeat some times
            for (int retryCount = 3; retryCount-- > 0; Thread.Sleep( 250 ))
            {
                // All supported in order of importance
                if (!Tune( group as SatelliteGroup, location as SatelliteLocation ).HasValue)
                    if (!Tune( group as CableGroup ).HasValue)
                        if (!Tune( group as TerrestrialGroup ).HasValue)
                            throw new DVBException( "Unsupported channel type " + group.GetType().FullName );

                // Done if locked
                if (SignalStatus.Locked)
                    break;

                // Enforce reset
                m_lastMessage = null;
            }
        }

        private void CheckChannel( DVBError errorCode )
        {
            // Validate
            DVBException.ThrowOnError( errorCode, "Unable to set channel" );
        }

        /// <summary>
        /// Wählt eine Quellgruppe aus.
        /// </summary>
        /// <param name="group">Díe Daten der Quellgruppe.</param>
        /// <param name="location">Die Wahl des Ursprungs, über den die Quellgruppe empfangen werden kann.</param>
        /// <returns>Gesetzt, wenn es sich um eine DVB-S Quellgruppe handelt.</returns>
        private Channel_S? Tune( SatelliteGroup group, SatelliteLocation location )
        {
            // Not us
            if (location == null)
                return null;
            if (group == null)
                return null;

            // Validate
            if (FrontendType != FrontendType.Satellite)
                throw new DVBException( "Expected " + FrontendType.ToString() + " Channel" );

            // Create channel
            var data =
                new Channel_S
                {
                    Mode = group.UsesS2Modulation ? DVBSMode.DVB_S2 : DVBSMode.DVB_S,
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
                    DVBException.ThrowOnError( _SendDiSEqCMsg( m_Class.ClassPointer, selector.Request, (byte) selector.Request.Length, selector.Burst ), "Could not send DiSEqC message" );

                    // Set repeat flag
                    if (selector.Request.Length > 0)
                        selector.Request[0] |= 1;
                }
            }

            // Calculated items
            data.b22kHz = (group.Frequency >= location.SwitchFrequency) ? 1 : 0;
            data.LOF = (0 == data.b22kHz) ? location.Frequency1 : location.Frequency2;

            // Power modes
            switch (group.Polarization)
            {
                case Polarizations.Horizontal: data.LNBPower = PowerMode.Horizontal; break;
                case Polarizations.Vertical: data.LNBPower = PowerMode.Vertical; break;
                case Polarizations.NotDefined: data.LNBPower = PowerMode.Off; break;
                default: throw new ArgumentException( group.Polarization.ToString(), "Polarization" );
            }

            // Process
            return data.SetChannel( this, false );
        }

        /// <summary>
        /// Wählt eine Quellgruppe an.
        /// </summary>
        /// <param name="group">Die Daten zur Quellgruppe.</param>
        /// <returns>Gesetzt, wenn es sich um eine DVB-C Quellgruppe handelt.</returns>
        private Channel_C? Tune( CableGroup group )
        {
            // Not us
            if (group == null)
                return null;

            // Validate
            if (FrontendType != FrontendType.Cable)
                throw new DVBException( "Expected " + FrontendType.ToString() + " Channel" );

            // Helper
            var data =
                new Channel_C
                {
                    Frequency = group.Frequency,
                    SymbolRate = group.SymbolRate,
                };

            // Spectrum inversion
            switch (group.SpectrumInversion)
            {
                case SpectrumInversions.On: data.Inversion = SpectrumInversion.On; break;
                case SpectrumInversions.Off: data.Inversion = SpectrumInversion.Off; break;
                case SpectrumInversions.Auto: data.Inversion = SpectrumInversion.Auto; break;
                default: data.Inversion = SpectrumInversion.Auto; break;
            }

            // Modulation
            switch (group.Modulation)
            {
                case CableModulations.QAM16: data.Qam = Qam.Qam16; break;
                case CableModulations.QAM32: data.Qam = Qam.Qam32; break;
                case CableModulations.QAM64: data.Qam = Qam.Qam64; break;
                case CableModulations.QAM128: data.Qam = Qam.Qam128; break;
                case CableModulations.QAM256: data.Qam = Qam.Qam256; break;
                default: data.Qam = Qam.Qam64; break;
            }

            // Check supported modes
            switch (group.Bandwidth)
            {
                case Bandwidths.Six: data.Bandwidth = BandwidthType.Six; break;
                case Bandwidths.Seven: data.Bandwidth = BandwidthType.Seven; break;
                case Bandwidths.Eight: data.Bandwidth = BandwidthType.Eight; break;
                case Bandwidths.NotDefined: data.Bandwidth = BandwidthType.None; break;
                default: data.Bandwidth = BandwidthType.Auto; break;
            }

            // Process
            return data.SetChannel( this );
        }

        /// <summary>
        /// Wählt eine Quellgruppe an.
        /// </summary>
        /// <param name="group">Die Daten zur Quellgruppe.</param>
        /// <returns>Gesetzt, wenn es sich um eine DVB-T Quellgruppe handelt.</returns>
        private Channel_T? Tune( TerrestrialGroup group )
        {
            // Not us 
            if (group == null)
                return null;

            // Validate
            if (FrontendType != FrontendType.Terrestrial)
                throw new DVBException( "Expected " + FrontendType.ToString() + " Channel" );

            // Helper
            var data =
                new Channel_T
                {
                    Frequency = group.Frequency,
                    Inversion = SpectrumInversion.Off,
                    Scan = false,
                };

            // Check supported modes
            switch (group.Bandwidth)
            {
                case Bandwidths.Six: data.Bandwidth = BandwidthType.Six; break;
                case Bandwidths.Seven: data.Bandwidth = BandwidthType.Seven; break;
                case Bandwidths.Eight: data.Bandwidth = BandwidthType.Eight; break;
                case Bandwidths.NotDefined: data.Bandwidth = BandwidthType.None; break;
                default: data.Bandwidth = BandwidthType.Auto; break;
            }

            // Process
            return data.SetChannel( this );
        }

        public DVBSignalStatus SignalStatus
        {
            get
            {
                // Lock data
                TechnoTrend.MFCWrapper.SignalStatus signal;
                LockState state;
                bool locked;

                // Retrieve
                DVBException.ThrowOnError( _GetState( m_Class.ClassPointer, out locked, out signal, out state ), "Unable to request signal status" );

                // Report
                return new DVBSignalStatus( locked && state.FrontendLocked, signal.Level, signal.Quality, signal.BER, signal.dBLevel, signal.RawQuality );
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            // Load the class
            ClassHolder instance;

            // Protected
            lock (this)
            {
                // Load
                instance = m_Class;

                // Forget
                m_Class = null;
            }

            // Wipe out
            if (null != instance) instance.Dispose();
        }

        #endregion
    }
}
