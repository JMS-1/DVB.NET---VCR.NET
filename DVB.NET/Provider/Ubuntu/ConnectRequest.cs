using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct ConnectRequest {
    public Int32 adapter;
    public Int32 frontend;
}
