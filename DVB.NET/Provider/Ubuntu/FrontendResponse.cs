using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack=4)]
public struct FrontendResponse
{
    public FrontendResponseType type;
    public UInt16 pid;
    public Int32 len;
}
