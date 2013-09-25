using System;
using System.Text;
using System.Security;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JMS.DVB.Provider.TTBudget
{
    internal class Common : IDisposable
    {
        [DllImport("ttlcdacc.dll", EntryPoint = "?OpenDevice@CDVBCommon@@SAHGPBDH@Z", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool _OpenDevice(UInt16 index, string applicationName, bool dummy);

        [DllImport("ttlcdacc.dll", EntryPoint = "?CloseDevice@CDVBCommon@@SAXXZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall)]
        [SuppressUnmanagedCodeSecurity]
        private static extern void _CloseDevice();

        [DllImport("ttlcdacc.dll", EntryPoint = "?IsOpen@CDVBCommon@@SAHXZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall)]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool _IsOpen();

        [DllImport("ttlcdacc.dll", EntryPoint = "?GetNumberOfDevices@CDVBCommon@@SAGXZ", ExactSpelling = true, CallingConvention = CallingConvention.ThisCall)]
        [SuppressUnmanagedCodeSecurity]
        private static extern UInt16 _GetNumberOfDevices();

        public Common()
        {
        }

        public int Count
        {
            get
            {
                // Ask
                return _GetNumberOfDevices();
            }
        }

        public void Open(int index)
        {
            // Terminate
            Close();

            // Reopen
            if (!_OpenDevice((ushort)index, null, false)) throw new ArgumentException(index.ToString(), "index");
        }

        public void Close()
        {
            // Terminate
            if (_IsOpen()) _CloseDevice();
        }

        #region IDisposable Members

        public void Dispose()
        {
            // Close if necessary
            Close();
        }

        #endregion
    }
}
