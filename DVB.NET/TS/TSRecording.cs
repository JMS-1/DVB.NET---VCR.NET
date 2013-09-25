using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.TS
{
	/// <summary>
	/// Manage multi-channel transporst stream recording.
	/// </summary>
	public class TSRecording : IDisposable
	{
		/// <summary>
		/// All transport streams registered.
		/// </summary>
		private List<Manager> m_Streams = new List<Manager>();

		/// <summary>
		/// Set when we are currently recording.
		/// </summary>
		private bool Recording = false;

		/// <summary>
		/// The connected DVB hardware.
		/// </summary>
		private IDeviceProvider DVBDevice;

		/// <summary>
		/// Create a new instance.
		/// </summary>
		public TSRecording(IDeviceProvider device)
		{
			// Remember
			DVBDevice = device;
		}

		/// <summary>
		/// Create a new transport stream.
		/// </summary>
		/// <param name="filename">Full file name.</param>
		/// <returns>A new transport stream instance.</returns>
		public Manager Add(string filename)
		{
			// Forward
			return Add(filename, 0);
		}

		/// <summary>
		/// Create a new transport stream.
		/// </summary>
		/// <param name="filename">Full file name.</param>
		/// <param name="nextPID">Optional initial PID counter.</param>
		/// <returns>A new transport stream instance.</returns>
		public Manager Add(string filename, short nextPID)
		{
			// Create the manager
			Manager manager = new Manager(filename, nextPID);

			// Remember
			lock (m_Streams) m_Streams.Add(manager);

			// Report
			return manager;
		}

		/// <summary>
		/// Check if the instance is recording.
		/// </summary>
		public bool IsIdle
		{
			get
			{
				// Report
				return !Recording;
			}
		}

		/// <summary>
		/// Report the current number of bytes recorded.
		/// </summary>
		public long Length
		{
			get
			{
				// Sum
				long total = 0;

				// For all
				lock (m_Streams)
					foreach (Manager manager in m_Streams) 
						total += manager.Length;

				// Report
				return total;
			}
		}

		/// <summary>
		/// Report the current number of audio or video bytes recorded.
		/// </summary>
		public long AVLength
		{
			get
			{
				// Sum
				long total = 0;

				// For all
				lock (m_Streams) 
					foreach (Manager manager in m_Streams) 
						total += manager.AVLength;

				// Report
				return total;
			}
		}

		/// <summary>
		/// Stop current recording if running.
		/// </summary>
		public string StopRecording()
		{
			// Nothing to do
			if (IsIdle) return null;

			// Report
			StringBuilder rep = new StringBuilder();

			// List to process
			List<Manager> streams;

			// Load
			lock (m_Streams) streams = new List<Manager>(m_Streams);

			// Forward
			foreach (Manager manager in streams)
			{
				// Stop receiving data
				manager.StopFilters();

				// Separate
				if (rep.Length > 0) rep.Append("\r\n------------------------------------\r\n");

				// Flush data to disk and stream
				rep.Append(manager.Flush());
			}

			// Full stop
			DVBDevice.StopFilters();

			// Change
			Recording = false;

			// Report
			return rep.ToString();
		}

		/// <summary>
		/// Reset this instance - which may include stopping any current recordings.
		/// </summary>
		public string ClearChannels()
		{
			// Stop
			string rep = StopRecording();

			// To be processed
			List<Manager> streams;

			// Load synchronized
			lock (m_Streams) streams = new List<Manager>(m_Streams);

			// Forward to all
			foreach (Manager manager in streams) manager.Dispose();

			// Forget
			lock (m_Streams) m_Streams.Clear();

			// Report
			return rep;
		}

		/// <summary>
		/// See if any of our streams needs EPG data.
		/// </summary>
		/// <param name="section"></param>
		public void AddEventTable(EPG.Section section)
		{
			// All streams
			List<Manager> streams;

			// Load synchronized
			lock (m_Streams) streams = new List<Manager>(m_Streams);

			// Forward to all
			foreach (Manager manager in streams) manager.AddEventTable(section);
		}

		/// <summary>
		/// Remove a single stream from the manager.
		/// </summary>
		/// <param name="manager">The stream to remove.</param>
		public void Remove(Manager manager)
		{
			// Process synchronized
			lock (m_Streams) m_Streams.Remove(manager);
		}

		/// <summary>
		/// Start recording if not already active.
		/// </summary>
		/// <remarks>
		/// The caller must tune the channel before calling this method.
		/// </remarks>
		public void StartRecording()
		{
			// Already done
			if (!IsIdle) return;

			// To be processed
			List<Manager> streams = new List<Manager>(m_Streams);

			// Forward
			foreach (Manager manager in streams) manager.StartFilters();

			// Change
			Recording = true;
		}

		/// <summary>
		/// Report the number of streams currently in use.
		/// <seealso cref="Manager.StreamCount"/>
		/// </summary>
		public int StreamCount
		{
			get
			{
				// Start
				int sum = 0;

				// Sum up
				lock (m_Streams) 
					foreach (Manager manager in m_Streams) 
						sum += manager.StreamCount;

				// Report
				return sum;
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Cleanup this instance.
		/// </summary>
		public void Dispose()
		{
			// Finish
			ClearChannels();
		}

		#endregion
	}
}
