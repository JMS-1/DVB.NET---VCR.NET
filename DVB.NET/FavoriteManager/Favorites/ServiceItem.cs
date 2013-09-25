using System;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.Favorites
{
	internal class ServiceItem: IComparable	
	{
		public char Index = '\0';

		public readonly object Context;

		public readonly string ServiceName;

		public ServiceItem(string serviceName, object context)
		{
			// Remember all
			ServiceName = serviceName;
			Context = context;
		}

		public override string ToString()
		{
			// Report inner name
			if ('\0' == Index) return ServiceName;

			// Merge
			return string.Format(ChannelSelector.IndexFormat, Index, ServiceName);
		}

		#region IComparable Members

		public int CompareTo(object obj)
		{
			// Change type
			ServiceItem other = obj as ServiceItem;

			// Not comparable
			if (null == other) return -1;

			// By name
			return ToString().CompareTo(other.ToString());
		}

		#endregion
	}
}
