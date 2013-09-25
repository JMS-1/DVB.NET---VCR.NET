using System;

namespace JMS.DVB.EPG
{
	/// <summary>
	/// Possible states of an airing event.
	/// <see cref="EventEntry"/>
	/// </summary>
	/// <remarks>
	/// For details please refer to the original documentation,
	/// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
	/// </remarks>
	public enum EventStatus
	{
		/// <summary>
		/// Not definied.
		/// </summary>
		Undefined,

		/// <summary>
		/// Not currently running and will not start in the near future.
		/// </summary>
		NotRunning,


		/// <summary>
		/// Not currently running and but will start in the near future.
		/// </summary>
		StartsSoon,

		/// <summary>
		/// Transmission is paused.
		/// </summary>
		Pausing,

		/// <summary>
		/// This event is currently running.
		/// </summary>
		Running,

		/// <summary>
		/// Reserved for future use.
		/// </summary>
		Reserved101,

		/// <summary>
		/// Reserved for future use.
		/// </summary>
		Reserved110,

		/// <summary>
		/// Reserved for future use.
		/// </summary>
		Reserved111
	}
}
