using System;

namespace JMS.TechnoTrend
{
	/// <summary>
	/// The various DVB types supported by DVB.NET.
	/// </summary>
	/// <remarks>
	/// Do not change layout - will be mapped to TT API constants.
	/// </remarks>
	public enum FrontendType
	{
		/// <summary>
		/// Don't known.
		/// </summary>
		Unknown,

		/// <summary>
		/// Satellite.
		/// </summary>
		Satellite,

		/// <summary>
		/// Cable.
		/// </summary>
		Cable,

		/// <summary>
		/// Terrestral.
		/// </summary>
		Terrestrial
	}
}
