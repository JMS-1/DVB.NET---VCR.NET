using System;

namespace JMS.DVB.Satellite
{
	/// <summary>
	/// Power settings to use when switching <see cref="Channel"/>.
	/// </summary>
	/// <remarks>
	/// Do not change layout - will be mapped to TT API constants.
	/// </remarks>
	public enum PowerMode
	{
		/// <summary>
		/// No power.
		/// </summary>
		Off			=  0,

		/// <summary>
		/// Use vertical polarisation.
		/// </summary>
		Vertical	= 13,

		/// <summary>
		/// Use horizontal polarisation.
		/// </summary>
		Horizontal	= 18
	}
}
