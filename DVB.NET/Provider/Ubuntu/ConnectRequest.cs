using System;
using System.Runtime.InteropServices;

namespace JMS.DVB.Provider.Ubuntu
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ConnectRequest
    {
        public Int32 adapter;
        public Int32 frontend;
    }
}
