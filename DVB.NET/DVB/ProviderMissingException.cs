using System;

namespace JMS.DVB
{
	/// <summary>
	/// This is exception is thrown when a <see cref="IDeviceProvider"/> sould
	/// be created from the configuration file but this file is either missing
	/// or invalid.
	/// </summary>
	[Serializable] public class ProviderMissingException: Exception
	{
		/// <summary>
		/// Create a new exception instance.
		/// </summary>
		public ProviderMissingException()
		{
		}

		/// <summary>
		/// Create a new exception instance with additional information.
		/// </summary>
		/// <param name="message">More information on th exception.</param>
		public ProviderMissingException(string message) : base(message)
		{
		}
	}
}
