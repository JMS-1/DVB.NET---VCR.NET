using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack=2)]
public struct SignalInformation
{        
    public FeStatus status;
    public UInt16 snr;
    public UInt16 strength;
    public UInt32 ber;
}
