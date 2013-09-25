using System;
using JMS.DVB;
using System.Text;
using JMS.DVB.EPG;
using System.Threading;
using JMS.DVB.EPG.Tables;
using System.Collections.Generic;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// 
    /// </summary>
	public abstract class SITScanner : Parser, IDisposable
	{
		/// <summary>
		/// Check for cancel.
		/// </summary>
		/// <returns></returns>
		public delegate bool CancelHandler();

		/// <summary>
        /// The associated hardware abstraction.
        /// </summary>
        private IDeviceProvider m_Device;

        /// <summary>
        /// 
        /// </summary>
        private static byte[] m_FilterMask = { 0x80 };

        /// <summary>
        /// 
        /// </summary>
        private static byte[] m_FilterData = { 0x00 };

        /// <summary>
        /// Set as soon as the table is fully filled.
        /// </summary>
        private ManualResetEvent m_Complete = new ManualResetEvent(false);

        /// <summary>
        /// The stream we attach to.
        /// </summary>
        public readonly ushort PID;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device">The hardware abstraction to use</param>
        /// <param name="pid">Die zu verwendende Datenstromkennung.</param>
        protected SITScanner(IDeviceProvider device, ushort pid)
            : base(null)
        {
			// Report
			Tools.WriteToScanLog("SITScanner {1} started for {0}", pid, GetType().Name);

            // Register self
            device.StartSectionFilter(pid, new FilterHandler(OnData), m_FilterData, m_FilterMask);

            // Remember
            m_Device = device;
            PID = pid;
        }

        /// <summary>
        /// Wait for the table to get received.
        /// </summary>
        /// <param name="milliSeconds">Number of milliseconds to wait at most.</param>
        /// <returns>Set if the table is fully available.</returns>
        public bool Wait(int milliSeconds)
		{
			// Helper
			bool cancel = false;

			// Forward
			return Wait(milliSeconds, null, ref cancel);
		}
		
        /// <summary>
        /// Wait for the table to get received.
        /// </summary>
        /// <param name="milliSeconds">Number of milliseconds to wait at most.</param>
		/// <param name="cancelTest">Callback to allow a client to abort a scan.</param>
		/// <param name="cancel">Set if client requests a cancel.</param>
        /// <returns>Set if the table is fully available.</returns>
        public bool Wait(int milliSeconds, CancelHandler cancelTest, ref bool cancel)
        {
			// Short cut
			if (0 == milliSeconds) return HasFinished;

			// With cleanup
			try
			{
				// Wait in pieces
				for (DateTime end = DateTime.UtcNow.AddMilliseconds(milliSeconds); DateTime.UtcNow <= end; Thread.Sleep(100))
				{
					// Process
					if (HasFinished) return true;

					// Ask user
					if ((null == cancelTest) || !cancelTest()) continue;

					// Mark
					cancel = true;

					// Done
					return false;
				}

				// Done
				return false;
			}
			finally
			{
				// Can stop filter
				Dispose();
			}
        }

		/// <summary>
		/// See if the tables have been received.
		/// </summary>
		private bool HasFinished
		{
			get
			{
				// Process
				if (!m_Complete.WaitOne(0, false)) return false;

				// Can stop filter
				Dispose();

				// Report
				return true;
			}
		}
		
		/// <summary>
        /// Process a table.
        /// </summary>
        /// <param name="table">The SI table found.</param>
        /// <returns>Set if the filter can be stopped.</returns>
        protected abstract bool OnTableFound(Table table);

        /// <summary>
        /// Called for each section the driver parsed from the stream.
        /// </summary>
        /// <param name="section">Section parsed.</param>
        protected override void OnSectionFound(Section section)
        {
            // We are already done
            if (m_Complete.WaitOne(0, false)) return;

            // Not useable
            if ((null == section) || !section.IsValid) return;

            // Be safe
            try
            {
                // Nothing to do
                if ((null == section.Table) || !section.Table.IsValid) return;

                // Call implementation
				if (OnTableFound(section.Table))
				{
					// Report
					Tools.WriteToScanLog("SITScanner {0} on {1} completed", GetType().Name, PID);

					// Release
					m_Complete.Set();
				}
            }
            catch
            {
                // Ignore any error
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Stop the filter.
        /// </summary>
        public virtual void Dispose()
        {
            // Check mode
            if (null != m_Device)
            {
                // Load
                IDeviceProvider device = m_Device;

                // Once
                m_Device = null;

				// Try stop
				device.StopFilter(PID);
            }
        }

        #endregion
    }
}
