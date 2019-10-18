using System;
using System.Runtime.InteropServices;

namespace JMS.DVB.Provider.Ubuntu
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct FrontendResponse
    {
        public FrontendResponseType type;
        public UInt16 pid;
        public Int32 len;
    }
}