using System;
using System.Security;
using System.Runtime.InteropServices;
using JMS.TechnoTrend;


namespace JMS.DVB.Provider.TTBudget
{
    [StructLayout( LayoutKind.Sequential )]
    internal struct Channel_S
    {
        [DllImport( "ttlcdacc.dll", EntryPoint = "?SetChannel@CDVBFrontend@@QAE?AW4DVB_ERROR@@U_CHANNEL_TYPE@1@H@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _SetChannel( IntPtr classPointer, Channel_S rChannel, bool bPowerOnly );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?GetChannel@CDVBFrontend@@QAE?AW4DVB_ERROR@@AAU_CHANNEL_TYPE@1@@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _GetChannel( IntPtr classPointer, out Channel_S rChannel );

        public UInt32 Frequency;

        public SpectrumInversion Inversion;

        public UInt32 SymbolRate;

        public UInt32 LOF;

        public Int32 b22kHz;

        public PowerMode LNBPower;

        public Viterbi Viterbi;

        public DVBSMode Mode;

        internal Channel_S SetChannel( Frontend frontend, bool powerOnly )
        {
            // Process
            DVBException.ThrowOnError( _SetChannel( frontend.ClassPointer, this, powerOnly ), "Unable to tune" );

            // Get channel once
            GetChannel( frontend );

            // Report second
            return GetChannel( frontend );
        }

        internal static Channel_S GetChannel( Frontend frontend )
        {
            // Create
            Channel_S channel;

            // Process
            DVBException.ThrowOnError( _GetChannel( frontend.ClassPointer, out channel ), "Unable to request tune status" );

            // Report
            return channel;
        }
    }
}
