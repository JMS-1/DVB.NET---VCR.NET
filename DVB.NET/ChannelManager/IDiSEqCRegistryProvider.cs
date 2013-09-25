using System;
using JMS.DVB.Satellite;

namespace JMS.ChannelManagement
{
	/// <summary>
	/// Each configuration list provider has to implement this interface.
	/// </summary>
	public interface IDiSEqCRegistryProvider
	{
		/// <summary>
		/// Fill the configuration from the registry.
		/// </summary>
		/// <param name="configuration">The configuration to be filled.</param>
		void Fill(DiSEqCConfiguration configuration);
	}
}
