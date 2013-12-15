using System;
using System.Runtime.InteropServices;
using System.Security;
using JMS.TechnoTrend;


namespace JMS.DVB.Provider.TTBudget
{
    [StructLayout( LayoutKind.Sequential )]
    internal struct Channel_C
    {
        [DllImport( "ttlcdacc.dll", EntryPoint = "?SetChannel@CDVBFrontend@@QAE?AW4DVB_ERROR@@U_CHANNEL_TYPE@1@H@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _SetChannel( IntPtr classPointer, Channel_C rChannel, bool bPowerOnly );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?GetChannel@CDVBFrontend@@QAE?AW4DVB_ERROR@@AAU_CHANNEL_TYPE@1@@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _GetChannel( IntPtr classPointer, out Channel_C rChannel );

        public UInt32 Frequency;

        public SpectrumInversion Inversion;

        public UInt32 SymbolRate;

        public Qam Qam;

        public BandwidthType Bandwidth;

        private UInt32 _pad_1;

        private UInt32 _pad_2;

        private UInt32 _pad_3;

        internal Channel_C SetChannel( Frontend frontend )
        {
            // Process
            DVBException.ThrowOnError( _SetChannel( frontend.ClassPointer, this, false ), "Unable to tune" );

            // Get channel once
            GetChannel( frontend );

            // Report second
            return GetChannel( frontend );
        }

        internal static Channel_C GetChannel( Frontend frontend )
        {
            // Create
            Channel_C channel;

            // Process
            DVBException.ThrowOnError( _GetChannel( frontend.ClassPointer, out channel ), "Unable to request tune status" );

            // Report
            return channel;
        }
    }
}
