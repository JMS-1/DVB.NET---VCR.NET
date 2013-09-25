using System;

namespace JMS.DVB
{
	/// <summary>
	/// Bandwidth type for DVB-C.
	/// </summary>
	/// <remarks>
	/// Do not change layout - will be mapped to TT API constants.
	/// </remarks>
	public enum BandwidthType
	{
		/// <summary>
		/// 6 MHz.
		/// </summary>
		Six,

		/// <summary>
		/// 7 MHz.
		/// </summary>
		Seven,

		/// <summary>
		/// 8 MHz.
		/// </summary>
		Eight,

		/// <summary>
		/// Automatic.
		/// </summary>
		Auto,

		/// <summary>
		/// None.
		/// </summary>
		None
	}
}
