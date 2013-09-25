extern alias oldVersion;

using System;
using System.Security;
using System.Runtime.InteropServices;

using JMS.TechnoTrend;

using legacy = oldVersion.JMS.DVB;


namespace JMS.DVB.Provider.TTBudget
{
    [StructLayout( LayoutKind.Sequential )]
    internal struct Channel_T
    {
        [DllImport( "ttlcdacc.dll", EntryPoint = "?SetChannel@CDVBFrontend@@QAE?AW4DVB_ERROR@@U_CHANNEL_TYPE@1@H@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _SetChannel( IntPtr classPointer, Channel_T rChannel, bool bPowerOnly );

        [DllImport( "ttlcdacc.dll", EntryPoint = "?GetChannel@CDVBFrontend@@QAE?AW4DVB_ERROR@@AAU_CHANNEL_TYPE@1@@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall )]
        [SuppressUnmanagedCodeSecurity]
        private static extern DVBError _GetChannel( IntPtr classPointer, out Channel_T rChannel );

        public UInt32 Frequency;

        public legacy.SpectrumInversion Inversion;

        public bool Scan;

        public legacy.BandwidthType Bandwidth;

        private UInt32 _pad_1;

        private UInt32 _pad_2;

        private UInt32 _pad_3;

        private UInt32 _pad_4;

        internal Channel_T SetChannel( Frontend frontend )
        {
            // Process
            DVBException.ThrowOnError( _SetChannel( frontend.ClassPointer, this, false ), "Unable to tune" );

            // Get channel once
            GetChannel( frontend );

            // Report second
            return GetChannel( frontend );
        }

        internal static Channel_T GetChannel( Frontend frontend )
        {
            // Create
            Channel_T channel;

            // Process
            DVBException.ThrowOnError( _GetChannel( frontend.ClassPointer, out channel ), "Unable to request tune status" );

            // Report
            return channel;
        }
    }
}
