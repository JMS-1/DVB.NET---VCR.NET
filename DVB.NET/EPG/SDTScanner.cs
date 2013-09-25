using System;
using JMS.DVB;
using JMS.DVB.EPG;
using System.Text;
using System.Threading;
using JMS.DVB.EPG.Tables;
using System.Collections;
using System.Globalization;
using JMS.DVB.EPG.Descriptors;
using System.Collections.Generic;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// 
    /// </summary>
	public class SDTScanner : ExtendedSITScanner<SDT>
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device">The hardware abstraction to use</param>
		public SDTScanner(IDeviceProvider device)
            : base(device, 0x11)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        protected override bool OnTableFound(Table table)
        {
            // Only accept same transport stream
            if (0x42 != table.Section.TableIdentifier) return false;

            // Forward
            return base.OnTableFound(table);
        }
    }
}
