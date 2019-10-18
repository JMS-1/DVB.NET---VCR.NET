using System;
using System.Runtime.InteropServices;

namespace JMS.DVB.Provider.Ubuntu
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SatelliteTune
    {
        public DiSEqCModes lnbMode;
        public UInt32 lnb1;
        public UInt32 lnb2;
        public UInt32 lnbSwitch;
        public bool lnbPower;
        public FeModulation modulation;
        public UInt32 frequency;
        public UInt32 symbolrate;
        public bool horizontal;
        public FeCodeRate innerFEC;
        public bool s2;
        public FeRolloff rolloff;
    }
}