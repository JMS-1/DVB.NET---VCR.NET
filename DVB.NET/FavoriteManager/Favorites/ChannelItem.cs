using System;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.Favorites
{
	public class ChannelItem: IComparable	
	{
		public string Index = null;

		public readonly string ChannelName;

		public readonly object Context;

		public ChannelItem(string channelName, object context)
		{
			// Remember all
			ChannelName = channelName;
			Context = context;
		}

		public override string ToString()
		{
			// Report inner name
			if (null == Index) return ChannelName;

			// Merge
			return string.Format(ChannelSelector.IndexFormat, Index, ChannelName);
		}

		#region IComparable Members

		public int CompareTo(object obj)
		{
			// Change type
			ChannelItem other = obj as ChannelItem;

			// Not comparable
			if (null == other) return -1;

			// By name
			return ToString().CompareTo(other.ToString());
		}

		#endregion
	}
}
