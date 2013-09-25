using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;

namespace VCRControlCenter
{
	static class Program
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct TokenPrivileges
		{
			public UInt32 PrivilegeCount;

			public UInt64 Luid;

			public UInt32 Attributes;
		}

		[DllImport("Advapi32.dll", EntryPoint = "AdjustTokenPrivileges")]
        [SuppressUnmanagedCodeSecurity]
        static private extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, ref TokenPrivileges NewState, UInt32 BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

		[DllImport("Advapi32.dll", EntryPoint = "OpenProcessToken")]
        [SuppressUnmanagedCodeSecurity]
        static private extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

		[DllImport("Advapi32.dll", EntryPoint = "LookupPrivilegeValue")]
        [SuppressUnmanagedCodeSecurity]
        static private extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out UInt64 lpLuid);

		[DllImport("kernel32.dll", EntryPoint = "CloseHandle")]
        [SuppressUnmanagedCodeSecurity]
        static private extern bool CloseHandle(IntPtr hObject);

		[STAThread]
		static void Main(string[] args)
		{
            // Check settings
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (!version.Equals( Properties.Settings.Default.Version ))
            {
                // Upgrade
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.Version = version;
                Properties.Settings.Default.Save();
            }

            // Initial delay
			if (Properties.Settings.Default.StartupDelay > 0) Thread.Sleep(Properties.Settings.Default.StartupDelay * 1000);

			// Be fully safe
			try
			{
				// Get a token for this process
				IntPtr hToken;
				if (OpenProcessToken(Process.GetCurrentProcess().Handle, 0x28, out hToken))
				{
					// Helper
					TokenPrivileges pPriv = new TokenPrivileges();

					// Lookup the privilege value
					if (LookupPrivilegeValue(null, "SeShutdownPrivilege", out pPriv.Luid))
					{
						// Finish
						pPriv.PrivilegeCount = 1;
						pPriv.Attributes = 2;

						// Update
						AdjustTokenPrivileges(hToken, false, ref pPriv, 0, IntPtr.Zero, IntPtr.Zero);
					}

					// Release
					CloseHandle(hToken);
				}

				// Load language
				string language = Properties.Settings.Default.Language;
				if (!string.IsNullOrEmpty(language))
					try
					{
						// Choose it
						Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(language);
					}
					catch
					{
						// Ignore any error
					}

				// Initialize
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				// Create 
				using (VCRNETControl main = new VCRNETControl(args))
                    if ((1 == args.Length) && args[0].Equals("/install"))
                    {
                        // Run
                        Application.Run(main);
                    }
                    else
                    {
                        // Run
                        Application.Run();
                    }
			}
			catch (Exception e)
			{
				// Report and terminate
				MessageBox.Show(e.ToString());
			}
		}
	}
}