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
	public class PMTScanner : ExtendedSITScanner<JMS.DVB.EPG.Tables.PMT>
	{
		/// <summary>
		/// 
		/// </summary>
		private ushort m_Programme;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device">The hardware abstraction to use</param>
        /// <param name="pid">The transport stream identifier of the PMT.</param>
		/// <param name="programme">The program number to serve.</param>
		internal PMTScanner(IDeviceProvider device, ushort pid, ushort programme)
            : base(device, pid)
        {
			// Remember
			m_Programme = programme;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="table"></param>
		/// <returns></returns>
		protected override bool OnTableFound(JMS.DVB.EPG.Table table)
		{
			// Check type
			JMS.DVB.EPG.Tables.PMT pmt = table as JMS.DVB.EPG.Tables.PMT;

			// Nut us
			if ((null != pmt) && (m_Programme != pmt.ProgramNumber)) return false;

			// Forward to base
			return base.OnTableFound(table);
		}
    }
}
