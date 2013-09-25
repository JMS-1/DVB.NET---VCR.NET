using System;
using JMS.DVB;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// 
    /// </summary>
	public class PMTManager : IDisposable
	{
		/// <summary>
		/// 
		/// </summary>
		private class Programme
		{
			/// <summary>
			/// 
			/// </summary>
			public readonly ushort PID;

			/// <summary>
			/// 
			/// </summary>
			public readonly ushort Number;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="pid"></param>
			/// <param name="number"></param>
			public Programme(ushort pid, ushort number)
			{
				// Remember
				Number = number;
				PID = pid;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public override int GetHashCode()
			{
				// Calculate
				return PID.GetHashCode() ^ Number.GetHashCode();
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			public override bool Equals(object obj)
			{
				// Type first
				Programme other = obj as Programme;

				// No match
				if (null == other) return false;

				// Compare
				return (PID == other.PID) && (Number == other.Number);
			}
		}

        /// <summary>
        /// The hardware we connect to
        /// </summary>
        private IDeviceProvider m_Device;

        /// <summary>
        /// All active scanners.
        /// </summary>
        private List<PMTScanner> m_Scanners = new List<PMTScanner>();

        /// <summary>
        /// Pending scanners.
        /// </summary>
		private List<Programme> m_Pending = new List<Programme>();

        /// <summary>
        /// 
        /// </summary>
        public readonly List<PMTScanner> Finished = new List<PMTScanner>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device">Hardware abstraction to use.</param>
		public PMTManager(IDeviceProvider device)
        {
			// Report
			Tools.WriteToScanLog("PMTManager started");

            // Remember 
            m_Device = device;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="program"></param>
        public void Add(ushort pid, ushort program)
        {
			// Report
			Tools.WriteToScanLog("Adding {0} (Program {1}) to Queue", pid, program);

            // Add to pending
            m_Pending.Add(new Programme(pid, program));

            // Try to make it running
            Cleanup(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="milliSeconds"></param>
		/// <param name="cancelTest"></param>
		/// <param name="cancel"></param>
		public bool Wait(int milliSeconds, SITScanner.CancelHandler cancelTest, ref bool cancel)
        {
            // Run as long as allowed
            for (DateTime end = DateTime.UtcNow.AddMilliseconds(milliSeconds); DateTime.UtcNow <= end; Thread.Sleep(100))
            {
                // Test for ready
                Cleanup(0);

                // We are done
                if ((m_Pending.Count < 1) && (m_Scanners.Count < 1)) return true;

				// Ask user
				if ((null == cancelTest) || !cancelTest()) continue;

				// Mark
				cancel = true;

				// Done
				return false;
            }

            // Final cleanup - brute force
			for (; m_Scanners.Count > 0; Thread.Sleep(100))
			{
				// Process
				Cleanup(100);

				// Ask user
				if ((null == cancelTest) || !cancelTest()) continue;

				// Mark
				cancel = true;

				// Done
				return false;
			}

			// Done
			return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="milliSeconds"></param>
        private void Cleanup(int milliSeconds)
        {
            // See if we can terminate some of these
            for (int i = m_Scanners.Count; i-- > 0; )
            {
                // Attach
                PMTScanner pmt = m_Scanners[i];

                // Still running
                if (!pmt.Wait(milliSeconds) && (0 == milliSeconds)) continue;

				// Report
				Tools.WriteToScanLog("Terminating {0}", pmt.PID);

                // Remove
                m_Scanners.RemoveAt(i);

                // Add to finished
                Finished.Add(pmt);
            }

            // Try to start others
            for (int i = 0; i < m_Pending.Count; ++i)
            {
                // No room left
                if (m_Scanners.Count >= m_Device.FilterLimit) break;

				// Load
				Programme programme = m_Pending[i];

				// See if the PID is already allocated
				foreach (PMTScanner scanner in m_Scanners)
					if (scanner.PID == programme.PID)
						return;

				// Report
				Tools.WriteToScanLog("Activate {0} (Program {1}) from Queue", programme.PID, programme.Number);
				
				// Start next
				m_Scanners.Add(new PMTScanner(m_Device, programme.PID, programme.Number));

                // Remove
                m_Pending.RemoveAt(i);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            // Stop all
			foreach (PMTScanner scanner in m_Scanners)
			{
				// Report
				Tools.WriteToScanLog("Unfinished {0}", scanner.PID);

				// Forward
				scanner.Dispose();
			}

            // Forget
            m_Scanners.Clear();
        }

        #endregion
    }
}
