extern alias oldVersion;

using System;
using System.Security;
using System.Threading;
using System.Runtime.InteropServices;

using JMS.TechnoTrend;
using JMS.TechnoTrend.MFCWrapper;

using legacy = oldVersion.JMS.DVB;


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
        private static extern legacy.FrontendType _GetType( IntPtr classPointer );

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

        private legacy.Satellite.DiSEqCMessage m_LastMessage = null;

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

        public legacy.FrontendType FrontendType
        {
            get
            {
                // Load it
                return _GetType( m_Class.ClassPointer );
            }
        }

        public FrontendCapabilities Capabilities
        {
            get
            {
                // Load it
                return _GetCapabilities( m_Class.ClassPointer );
            }
        }

        public void Tune( legacy.Transponder transponder, legacy.Satellite.DiSEqC diseqc )
        {
            // Forward
            Tune( transponder, diseqc, false );
        }

        public void Tune( legacy.Transponder transponder, legacy.Satellite.DiSEqC diseqc, bool powerOnly )
        {
            // May repeat some times
            for (int retryCount = 3; retryCount-- > 0; Thread.Sleep( 250 ))
            {
                // Try terrestrial
                var terrestrial = transponder as legacy.Terrestrial.TerrestrialChannel;

                // Process
                if (null != terrestrial)
                {
                    // Call it
                    Tune( terrestrial );
                }
                else
                {
                    // Try cable
                    var cable = transponder as legacy.Cable.CableChannel;

                    // Process
                    if (null != cable)
                    {
                        // Call it
                        Tune( cable );
                    }
                    else
                    {
                        // Try satellite
                        var satellite = transponder as legacy.Satellite.SatelliteChannel;

                        // Process
                        if (null != satellite)
                        {
                            // Call it
                            Tune( satellite, diseqc, powerOnly );
                        }
                        else
                        {
                            // Unsupported
                            throw new DVBException( "Unsupported channel type " + transponder.GetType().FullName );
                        }
                    }
                }

                // Done if locked
                if (SignalStatus.Locked) break;
            }
        }

        private void CheckChannel( DVBError errorCode )
        {
            // Validate
            DVBException.ThrowOnError( errorCode, "Unable to set channel" );
        }

        private Channel_S Tune( legacy.Satellite.SatelliteChannel transponder, legacy.Satellite.DiSEqC diseqc, bool powerOnly )
        {
            // Validate
            if (null == diseqc) throw new ArgumentNullException( "diseqc" );

            // Validate
            if (legacy.FrontendType.Satellite != FrontendType) throw new DVBException( "Expected " + FrontendType.ToString() + " channel" );

            // Helper
            Channel_S data = new Channel_S();

            // Fill
            data.Mode = transponder.S2Modulation ? DVBSMode.DVB_S2 : DVBSMode.DVB_S;
            data.Inversion = transponder.SpectrumInversion;
            data.SymbolRate = transponder.SymbolRate;
            data.Frequency = transponder.Frequency;

            // Request the message to send
            var message = diseqc.CreateMessage( transponder );

            // See if the message is different from the last one
            if (!message.Equals( m_LastMessage ))
            {
                // Remember
                m_LastMessage = message.Clone();

                // As long as necessary
                for (int n = message.Repeat; n-- > 0; Thread.Sleep( 120 ))
                {
                    // Send it
                    DVBException.ThrowOnError( _SendDiSEqCMsg( m_Class.ClassPointer, message.Request, (byte) message.Request.Length, message.Burst ), "Could not send DiSEqC message" );

                    // Set repeat flag
                    if (message.Request.Length > 0) message.Request[0] |= 1;
                }
            }

            // Calculated items
            data.LNBPower = diseqc.UsePower ? transponder.Power : legacy.Satellite.PowerMode.Off;
            data.b22kHz = diseqc.Use22kHz( transponder.Frequency ) ? 1 : 0;
            data.LOF = (0 == data.b22kHz) ? diseqc.LOF1 : diseqc.LOF2;

            // Process
            return data.SetChannel( this, powerOnly );
        }

        private Channel_C Tune( legacy.Cable.CableChannel transponder )
        {
            // Validate
            if (legacy.FrontendType.Cable != FrontendType) throw new DVBException( "Expected " + FrontendType.ToString() + " channel" );

            // Helper
            Channel_C data = new Channel_C();

            // Fill
            data.Inversion = transponder.SpectrumInversion;
            data.SymbolRate = transponder.SymbolRate;
            data.Frequency = transponder.Frequency;
            data.Bandwidth = transponder.Bandwidth;
            data.Qam = transponder.QAM;

            // Process
            return data.SetChannel( this );
        }

        private Channel_T Tune( legacy.Terrestrial.TerrestrialChannel transponder )
        {
            // Validate
            if (legacy.FrontendType.Terrestrial != FrontendType) throw new DVBException( "Expected " + FrontendType.ToString() + " channel" );

            // Helper
            Channel_T data = new Channel_T();

            // Fill
            data.Inversion = transponder.SpectrumInversion;
            data.Frequency = transponder.Frequency;
            data.Bandwidth = transponder.Bandwidth;
            data.Scan = transponder.FullRescan;

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
