using System;
using System.IO;
using System.Net;
using System.Data;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace FTPWrap
{
	/// <summary>
	/// Hauptformular der Anwendung.
	/// </summary>
	public partial class FTPMain : Form
	{
		/// <summary>
		/// Alle aktiven und vergangenen FTP Verbindungen - die aktuelle Version räumt nicht auf.
		/// </summary>
		private List<FTPClient> m_Clients = new List<FTPClient>();

		/// <summary>
		/// Die Aufzeichnungsdatei, die von diesem FTP Server verwaltet wird.
		/// </summary>
		private FileInfo m_File;

		/// <summary>
		/// Der gestartet Prozeß zum Trennen von Bild und Ton.
		/// </summary>
		private Process m_Demux;

		/// <summary>
		/// Der FTP Server <see cref="Socket"/>.
		/// </summary>
		private Socket m_Socket;

		/// <summary>
		/// Gesetzt, solange neue Verbindungen angenommen werden.
		/// </summary>
		private bool m_AllowAccept = true;

        /// <summary>
        /// Gesetzt, wenn ein Live Demux durchgeführt wird, aber noch keine Daten 
        /// aus der Datei ausgelesen werden konnten.
        /// </summary>
        private static bool m_LiveModeStarting = false;

        /// <summary>
        /// Synchronisiert den Zugriff auf die Protokolldatei.
        /// </summary>
        private static object m_Lock = new object();

        /// <summary>
        /// Die Protokolldatei.
        /// </summary>
        private static FileInfo m_LogFile = null;

        /// <summary>
        /// Der Zeitpunkt, an dem letztmalig Daten ausgelesen wurden.
        /// </summary>
        private static DateTime m_NoDataThreshold;

		/// <summary>
		/// Erzeugt ein neues Formular.
		/// </summary>
		/// <param name="args">Parameter zum Formular.</param>
		public FTPMain(string[] args)
		{
			// Test for first start
			if (Properties.Settings.Default.SecondsToWaitForEnd < 0)
			{
				// Save settings
				Properties.Settings.Default.SecondsToWaitForEnd = 30;
				Properties.Settings.Default.Save();
			}

            // See if this is a VCR.NET live demux
            m_LiveModeStarting = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PlannedFiles"));

            // Report
            if (m_LiveModeStarting) Log("Detected VCR.NET Live Demux");

            // Start timer
            ResetNoDataThreshold();

			// Index to file
			int fileIndex = 0;

			// Name of the batch file
			string batFile = System.Diagnostics.Debugger.IsAttached ? "FTPWrap_Debug.bat" : "FTPWrap.bat";

			// May overwrite
			if (args.Length > 0)
				if (args[fileIndex].StartsWith("/BAT="))
				{
					// Get the name
					batFile = args[fileIndex++].Substring(5);
				}

			// Attach to file
			m_File = new FileInfo(args[fileIndex]);

			// Setup GUI
			InitializeComponent();

            // Report
            Log("Creating Socket");

			// Create socket
			m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			// Asnychronous
			m_Socket.Blocking = false;

			// Make us an FTP server
			m_Socket.Bind(new IPEndPoint(IPAddress.Any, 0));

			// Report
			notifier.Text = string.Format("ftp://localhost:{0}", ((IPEndPoint)m_Socket.LocalEndPoint).Port);

			// Report
			Text = string.Format("{0}/{1}", notifier.Text, m_File.Name);

            // Report
            Log("Active for {0}", Text);

            // Start listening
			m_Socket.Listen(5);

			// Await first connection
			m_Socket.BeginAccept(OnAccept, null);

			// Create start information
			ProcessStartInfo info = new ProcessStartInfo();

			// Configure
			info.Arguments = string.Format("\"{0}\"", Text.Replace("\"", "\"\""));
			info.FileName = batFile;
			info.LoadUserProfile = true;
			info.UseShellExecute = true;
			info.WindowStyle = ProcessWindowStyle.Hidden;

            // Report
            Log("Starting {0}", info.FileName);

			// Create the process
			m_Demux = Process.Start(info);

            // Report
            Log("Synchronizing with Controller");

            // Send to controller
            Console.Out.WriteLine(m_Demux.Id);
            Console.Out.Flush();

            // Synchronize with caller
            Console.In.ReadLine();

            // Report
            Log("Controller allows us to Execute");
            
            // Start timer
			waitExit.Enabled = true;
		}

		/// <summary>
		/// Beim Schliessen des Fensters wird dieses nur unsichtbar gemacht.
		/// </summary>
		/// <param name="e">Parameter des Aufrufs.</param>
		protected override void OnClosing(CancelEventArgs e)
		{
            // Report
            Log("OnClosing");

			// Stop
			e.Cancel = true;

			// Hide
			Visible = false;

			// Forward
			base.OnClosing(e);
		}

		/// <summary>
		/// Signatur der Methode zum Beenden der Anwendung - normalerweise durch einen Doppelklick
		/// auf das Tray Icon ausgelöst.
		/// </summary>
		private delegate void TerminateHandler();

		/// <summary>
		/// Wird aufgerufen, wenn ein FTP Client die Sitzung beendet.
		/// </summary>
		/// <param name="client">Die zugehörige Sitzung, die beendet wurde.</param>
		private void OnClientFinished(FTPClient client)
		{
            // Report
            Log("OnClientFinished");

			// Terminate
			if (client.EmptyFile)
			{
				// No more connections
				m_AllowAccept = false;

				// Create a new thread for termination
				Thread terminator = new Thread(() => { Invoke(new TerminateHandler(OnTerminate)); });

				// Start it
				terminator.Start();
			}
		}

		/// <summary>
		/// Beginnt eine neue Sitzung mit einem FTP Client.
		/// </summary>
		/// <param name="result">Informationen zur neuen Sitzung.</param>
		private void OnAccept(IAsyncResult result)
		{
            // Report
            Log("OnAccept");

			// Already done
			if (null == m_Socket) return;

			// Not allowed
			if (!m_AllowAccept) return;

			// Be safe
			try
			{
				// Add a new client
				m_Clients.Add(new FTPClient(m_Socket.EndAccept(result), m_File, OnClientFinished));

				// Await next connection
				m_Socket.BeginAccept(OnAccept, null);
			}
			catch
			{
			}
		}

		/// <summary>
		/// Beendet alle Netzwerkverbindungen.
		/// </summary>
		private void EndFTPServer()
		{
            // Report
            Log("EndFTPServer");

			// Cleanup LISTENING port
			using (Socket cleanup = m_Socket)
				if (null != cleanup)
				{
					// Forget
					m_Socket = null;

					// Close it
					cleanup.Close();
				}

			// Preempty all client activities
			foreach (FTPClient client in m_Clients) client.Abort();

			// Delay a bit
			Thread.Sleep(500);

			// Finish all client activities
			foreach (FTPClient client in m_Clients) client.Dispose();

			// Reset client list
			m_Clients.Clear();
		}

		/// <summary>
		/// Beendet den FTP Server.
		/// </summary>
		private void OnTerminate()
		{
            // Report
            Log("OnTerminate");

			// Hide icon and stop watchdog
			waitExit.Enabled = false;
			notifier.Visible = false;

			// Terminate the server
			EndFTPServer();

			// Let it update
			Application.DoEvents();

			// Delay a bit
			Thread.Sleep(500);

			// Brute force termination
			Environment.Exit(1);
		}

		/// <summary>
		/// Beendet den FTP Server bei einem Doppelklick auf das Tray Icon.
		/// </summary>
		/// <param name="sender">Wird ignoriert.</param>
		/// <param name="e">Wird ignoriert.</param>
		private void OnTerminate(object sender, MouseEventArgs e)
		{
			// Brute force termination
			OnTerminate();
		}

		/// <summary>
		/// Prüft periodisch, ob die Trennung schon beendet wurde.
		/// </summary>
		/// <param name="sender">Wird ignoriert.</param>
		/// <param name="e">Wird ignoriert.</param>
		private void waitExit_Tick(object sender, EventArgs e)
		{
            // Report
            Log("waitExit_Tick");

			// Brute force termination
            if (m_Demux.HasExited)
            {
                // Report
                Log("Demux has exited");

                // Finish
                OnTerminate();
            }
            else if (DateTime.UtcNow > m_NoDataThreshold)
            {
                // Report
                Log("No more Data available");

                // Finish
                OnTerminate();
            }
		}

        /// <summary>
        /// Erzeugt einen Protokolleintrag.
        /// </summary>
        /// <param name="message">Der gewünschte Eintrag.</param>
        public static void Log(string message)
        {
            // Not configured
            if (string.IsNullOrEmpty(Properties.Settings.Default.LogDirectory)) return;

            // Protect
            lock (m_Lock)
            {
                // Create once
                if (null == m_LogFile)
                {
                    // Attach to directory
                    DirectoryInfo logDir = new DirectoryInfo(Properties.Settings.Default.LogDirectory);

                    // Create
                    if (!logDir.Exists) logDir.Create();

                    // Attach to file
                    m_LogFile = new FileInfo(Path.Combine(logDir.FullName, string.Format("FTPWrap{0:N}.log", Guid.NewGuid())));
                }

                // Create the entry
                using (StreamWriter writer = new StreamWriter(m_LogFile.FullName, true, Encoding.Unicode))
                {
                    // Send message
                    writer.WriteLine("{0} {1}", DateTime.Now, message);
                }
            }
        }

        /// <summary>
        /// Erzeugt einen Protokolleintrag.
        /// </summary>
        /// <param name="format">Format zum Erstellen des Eintrags.</param>
        /// <param name="args">Parameter zum Format.</param>
        public static void Log(string format, params object[] args)
        {
            // Not configured
            if (string.IsNullOrEmpty(Properties.Settings.Default.LogDirectory)) return;

            // Forward
            Log(string.Format(format, args));
        }

        /// <summary>
        /// Meldet, dass tatsächlich Daten ausgelesen wurden.
        /// </summary>
        /// <param name="bytes">Anzahl der ausgelesenen Bytes.</param>
        public static void ReportDataFromFile(int bytes)
        {
            // Report
            if (m_LiveModeStarting) Log("VCR.NET Live Demux Startup finished");

            // Disable live mode
            m_LiveModeStarting = false;

            // Start counter
            ResetNoDataThreshold();
        }

        /// <summary>
        /// Reaktiviert die Schutzprüfungen zur automatischen Terminierung wenn
        /// der Lesevorgang auf dem Ende der Datei blockiert.
        /// </summary>
        private static void ResetNoDataThreshold()
        {
            // Remember
            m_NoDataThreshold = DateTime.UtcNow.AddSeconds(ShutdownDelay + 30);
        }

        /// <summary>
        /// Meldet, wie lange bis zur automatischen Terminierung gewartet werden sollte,
        /// wenn keine Daten aus der Eingabedatei mehr erhalten werden.
        /// </summary>
        public static int ShutdownDelay
        {
            get
            {
                // Report
                return Properties.Settings.Default.SecondsToWaitForEnd + (m_LiveModeStarting ? 90 : 0);
            }
        }
	}
}