using System;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.Favorites
{
	internal class AudioItem: IComparable	
	{
		public char Index = '\0';

		public readonly string AudioName;

		public readonly string LanguageName;

		public AudioItem(string audioName)
		{
			// Try to split
			int i = audioName.IndexOf(" [");

			// Remember all
			LanguageName = (i < 0) ? audioName : audioName.Substring(0, i);
			AudioName = audioName;
		}

		public override string ToString()
		{
			// Report inner name
			if ('\0' == Index) return LanguageName;

			// Merge
			return string.Format(ChannelSelector.IndexFormat, Index, LanguageName);
		}

		#region IComparable Members

		public int CompareTo(object obj)
		{
			// Change type
			AudioItem other = obj as AudioItem;

			// Not comparable
			if (null == other) return -1;

			// By name
			return ToString().CompareTo(other.ToString());
		}

		#endregion
	}
}
