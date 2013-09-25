using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows.Forms;


namespace FTPWrap
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
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

            // Real parameter list
            List<string> realArgs = new List<string>(args);

            // Check mode
            int ixSlave = realArgs.IndexOf("/Slave");

            // Slave mode
            if (ixSlave >= 0)
            {
                // Remove it
                realArgs.RemoveAt(ixSlave);

                // Startup WinForms
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FTPMain(realArgs.ToArray()));
            }
            else
            {
                // Parameter helper
                StringBuilder cmdLine = new StringBuilder();

                // Append switch
                cmdLine.Append("/Slave");

                // Merge all
                foreach (string arg in realArgs) cmdLine.AppendFormat(" \"{0}\"", arg.Replace("\"", "\"\""));

                // Create start information
                ProcessStartInfo info = new ProcessStartInfo();

                // Configure
                info.Arguments = cmdLine.ToString();
                info.FileName = Application.ExecutablePath;
                info.LoadUserProfile = true;
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.RedirectStandardInput = true;

                // Create the process
                Process process = Process.Start(info);

                // Wait for it
                try
                {
                    // Read the process identifier of the sub process
                    int demuxId = int.Parse(process.StandardOutput.ReadLine());

                    // Attach to the process
                    Process demux;
                    
                    // Safe load
                    try
                    {
                        // Load
                        demux = Process.GetProcessById(demuxId);
                    }
                    catch
                    {
                        // Failed - fallback to standard behaviour
                        demux = null;
                    }

                    // Let it continue
                    process.StandardInput.WriteLine("OK");
                    process.StandardInput.Flush();

                    // Wait for outer
                    process.WaitForExit();

                    // Wait for inner
                    if (null != demux) 
                        demux.WaitForExit();
                }
                catch
                {
                }
            }
        }
	}
}