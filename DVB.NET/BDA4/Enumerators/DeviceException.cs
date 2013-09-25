using System;

namespace JMS.DVB.DeviceAccess.Enumerators
{
	/// <summary>
	/// Reports errors from calls to the SetupAPI.
	/// </summary>
    public class DeviceException : Exception
	{
		/// <summary>
		/// Create a new instance of this exception.
		/// </summary>
		/// <param name="sReason">Describes the method which failed.</param>
		public DeviceException(string sReason) : base(sReason)
		{
		}
	}
}
