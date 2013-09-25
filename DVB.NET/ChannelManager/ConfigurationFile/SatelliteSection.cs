using System;
using System.Text;
using JMS.DVB.Satellite;
using System.Collections.Generic;

namespace JMS.ChannelManagement.ConfigurationFile
{
	public class SatelliteSection: Section
	{
		public SatelliteSection()
		{
		}

		protected override void Finish(ConfigurationCollection collection)
		{
			// Base class
			base.Finish(collection);

			// Store
			collection.DVBSReceivers.Add(this);
		}
	}
}
