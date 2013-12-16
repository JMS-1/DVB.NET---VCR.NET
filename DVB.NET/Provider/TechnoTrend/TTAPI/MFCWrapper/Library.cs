using System.Runtime.InteropServices;
using System.Security;


namespace JMS.TechnoTrend.MFCWrapper
{
    /// <summary>
    /// This class is used to initialize the TTAPI itself.
    /// </summary>
    public sealed class Library
    {
        /// <summary>
        /// Global initialisation method.
        /// </summary>
        [DllImport("ttdvbacc.dll", EntryPoint = "?InitDvbApiDll@@YAXXZ", ExactSpelling = true)]
        [SuppressUnmanagedCodeSecurity]
        private static extern void _Initialize();

        /// <summary>
        /// Forbid creating instances.
        /// </summary>
        private Library()
        {
        }

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
