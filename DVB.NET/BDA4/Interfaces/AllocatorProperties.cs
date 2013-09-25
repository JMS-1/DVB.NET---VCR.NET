using System;
using System.Runtime.InteropServices;


namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Memory allocator properties.
    /// </summary>
    [StructLayout( LayoutKind.Sequential, Pack = 4 )]
    public struct AllocatorProperties
    {
        /// <summary>
        /// Number of buffers.
        /// </summary>
        public Int32 cBuffers;

        /// <summary>
        /// Size of a buffer.
        /// </summary>
        public Int32 cbBuffer;

        /// <summary>
        /// Alignment information.
        /// </summary>
        public Int32 cbAlign;

        /// <summary>
        /// Size of prefix data.
        /// </summary>
        public Int32 cbPrefix;
    }
}
