using System;

namespace JMS.ChannelManagement
{
	/// <summary>
	/// Each channel list provider has to implement this interface.
	/// </summary>
	public interface IChannelListProvider
	{
		/// <summary>
		/// Load the channel list and report all channels to the <see cref="ChannelManager"/>.
		/// </summary>
		/// <param name="pMan">The related manager.</param>
		void Load(ChannelManager pMan);
	}
}
