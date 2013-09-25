using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

namespace FTPWrap
{
	/// <summary>
	/// Diese Klasse verwaltet einen FTP Datenkanal.
	/// </summary>
	public class DataChannel: IDisposable
	{
		/// <summary>
		/// Rückrufmethode, die nach dem Schliessen des <see cref="Socket"/> aufgerufen wird.
		/// </summary>
		/// <param name="channel">Datenkanalverwaltung, die den Vorgang beendet wurde.</param>
		public delegate void FinishedHandler(DataChannel channel);

		/// <summary>
		/// Vermerkt die Methode zum Rückruf beim Beenden.
		/// </summary>
		private FinishedHandler m_OnFinished;

		/// <summary>
		/// Zwischenspeicher beim Übertragen von Dateiinhalten.
		/// </summary>
		private byte[] m_Buffer;

		/// <summary>
		/// Der <see cref="Socket"/> zum Datenkanal.
		/// </summary>
		private Socket m_Socket;

		/// <summary>
		/// Optional eine Datei, die zum Client zu senden ist.
		/// </summary>
		private Stream m_File;

		/// <summary>
		/// Wird gesetzt, wenn mindestens ein Byte aus der Datei ausgelesen wurde.
		/// </summary>
		private bool? m_GotData = null;

		/// <summary>
		/// Wird bei einem vorzeitigen Abbruch gesetzt.
		/// </summary>
		private ManualResetEvent m_Abort = new ManualResetEvent(false);

		/// <summary>
		/// Erzeugt eine neue Datenkanalinstanz.
		/// </summary>
		/// <param name="socket">Der zugehörige Datenkanal.</param>
		/// <param name="onFinished">Methode, die nach dem Schliessen des Datenkanals aufgerufen werden soll.</param>
		public DataChannel(Socket socket, FinishedHandler onFinished)
		{
			// Remember
			m_OnFinished = onFinished;

			// Attach to socket
			m_Socket = socket;

			// Use asynchronous
			m_Socket.Blocking = false;

            // Report
            FTPMain.Log("DataChannel created");
		}

		/// <summary>
		/// Versendet eine Zeichenkette über den Datenkanal.
		/// </summary>
		/// <param name="text">Der Inhalt der zu verwendenden Zeichenkette.</param>
		public void Send(string text)
		{
            // Report
            FTPMain.Log("Send {0}", text);

            // To byte array
			byte[] toSend = Encoding.Default.GetBytes(text);

            // Be safe
			try
			{
				// Start sending
				m_Socket.BeginSend(toSend, 0, toSend.Length, SocketFlags.None, FinishString, m_Socket);
			}
			catch
			{
				// Terminate on any error
				Close();
			}
		}

		/// <summary>
		/// Sendet die nächsten Bytes der angegebenen Datei über den Datenkanal. Wurde dieser Teil
		/// erfolgreich verschickt, dann wird der nächste Teil abgerufen.
		/// </summary>
		/// <param name="file">Zu versendende Datei.</param>
		public void Send(Stream file)
		{
            // Report
            FTPMain.Log("Send Stream");
            
            // At least we tried
			if (!m_GotData.HasValue) m_GotData = false;

			// Create buffer
			if (null == m_Buffer) m_Buffer = new byte[100000];

			// May retry - give us 30 seconds to detect end of live recording
			for (int retry = FTPMain.ShutdownDelay; retry-- > 0; )
			{
                // Report
                FTPMain.Log("Awaiting Data");
                
                // Read bytes
				int bytes = file.Read(m_Buffer, 0, m_Buffer.Length);

                // Report
                FTPMain.Log("Got {0} Bytes", bytes);
                
                // Wait a bit
				if (bytes < 1)
				{
					// Delay
					if (m_Abort.WaitOne(1000, true)) break;

					// Next
					continue;
				}

                // Report
                FTPMain.ReportDataFromFile(bytes);

				// At least anything
				m_GotData = true;

				// Restart retry counter
                retry = FTPMain.ShutdownDelay;

				// Try to send this chunk
				try
				{
					// Remember file for next chunk
					m_File = file;

					// Try to send
					m_Socket.BeginSend(m_Buffer, 0, bytes, SocketFlags.None, FinishChunk, m_Socket);

                    // Report
                    FTPMain.Log("BeginSend {0} called", bytes);
                    
                    // Wait for next chunk
					return;
				}
				catch
				{
					// Terminate on any error
					break;
				}
			}

			// Terminate
			Close();
		}

		/// <summary>
		/// Beendet dieses Datenkanal vorzeitig.
		/// </summary>
		public void Abort()
		{
            // Report
            FTPMain.Log("DataChannel aborted");
            
            // Signal the very end
			m_Abort.Set();
		}

		/// <summary>
		/// Erkennt das Ende des Versendens eines Stücks aus einer Datei.
		/// </summary>
		/// <param name="result">Informationen zum Ergebnis des Versendens.</param>
		private void FinishChunk(IAsyncResult result)
		{
            // Report
            FTPMain.Log("FinishChunk");
            
            // Be safe
			try
			{
				// Attach to socket
				Socket socket = (Socket)result.AsyncState;

				// Terminate request
				socket.EndSend(result);

				// Send next chunk
				if (m_Abort.WaitOne(0, false))
				{
					// Stop right now 
					Close();
				}
				else
				{
					// Continue with next chunk
					Send(m_File);
				}
			}
			catch
			{
				// Close all
				Close();
			}
		}

		/// <summary>
		/// Erkennt das Ende des Versendens einer Zeichenkette.
		/// </summary>
		/// <param name="result">Informationen über das Ergebnis des Versendens.</param>
		private void FinishString(IAsyncResult result)
		{
            // Report
            FTPMain.Log("FinishString");
            
            // Be safe
			try
			{
				// Attach to socket
				Socket socket = (Socket)result.AsyncState;

				// Terminate request
				socket.EndSend(result);
			}
			catch
			{
				// Ignore any error
			}

			// Close all
			Close();
		}

		/// <summary>
		/// Meldet, ob mindestens ein Byte aus der Datei ausgelesen wurde.
		/// </summary>
		public bool GotData
		{
			get
			{
				// Report
				return m_GotData ?? true;
			}
		}

		/// <summary>
		/// Beendet die Nutzung dieses Datenkanals endgültig.
		/// </summary>
		private void Close()
		{
            // Report
            FTPMain.Log("DataChannel closing");
            
            // Process
			using (Socket cleanup = m_Socket)
				if (null != cleanup)
				{
					// Wipe out
					m_Socket = null;

					// Shutdown
					cleanup.Shutdown(SocketShutdown.Both);
					cleanup.Close();

					// Report
					if (null != m_OnFinished) m_OnFinished(this);
				}

			// Check file
			using (Stream file = m_File)
				if (null != file)
				{
					// Forget
					m_File = null;

					// Close
					file.Close();
				}
		}

		#region IDisposable Members

		/// <summary>
		/// Beendet die Nutzung dieser .NET Instanz endgültig.
		/// <seealso cref="Close"/>
		/// </summary>
		public void Dispose()
		{
			// Terminate
			Close();
		}

		#endregion
	}
}
