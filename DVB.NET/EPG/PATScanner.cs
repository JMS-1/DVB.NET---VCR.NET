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
	public class PATScanner : ExtendedSITScanner<JMS.DVB.EPG.Tables.PAT>
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device">The hardware abstraction to use</param>
		public PATScanner(IDeviceProvider device)
			: base(device, 0)
        {
        }
    }
}
