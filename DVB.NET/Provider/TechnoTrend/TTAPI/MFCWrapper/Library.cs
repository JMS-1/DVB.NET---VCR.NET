using System.Runtime.InteropServices;
using System.Security;


namespace JMS.TechnoTrend.MFCWrapper
{
    /// <summary>
    /// This class is used to initialize the TTAPI itself.
    /// </summary>
    public static class Library
    {
        /// <summary>
        /// Global initialisation method.
        /// </summary>
        [DllImport("ttdvbacc.dll", EntryPoint = "?InitDvbApiDll@@YAXXZ", ExactSpelling = true)]
        [SuppressUnmanagedCodeSecurity]
        private static extern void _Initialize();

        /// <summary>
        /// Initialize the TTAPI DLL - must be called only once per process.
        /// </summary>
        public static void Initialize()
        {
            // Run
            _Initialize();
        }
    }
}
