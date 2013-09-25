using System;

namespace JMS.TechnoTrend
{
	/// <summary>
	/// General error codes.
	/// </summary>
	public enum DVBError
	{
		/// <summary>
		/// Success.
		/// </summary>
		None,		

		/// <summary>
		/// Problems while accessing driver.
		/// </summary>
		Driver,

		/// <summary>
		/// General hardward problem.
		/// </summary>
		Hardware,

		/// <summary>
		/// Invalid parameter.
		/// </summary>
		Parameter,

		/// <summary>
		/// Time-Out occured.
		/// </summary>
		TimeOut,

		/// <summary>
		/// Call invalid in current state.
		/// </summary>
		State,

		/// <summary>
		/// Not enough resources.
		/// </summary>
		Resources,

		/// <summary>
		/// Other error.
		/// </summary>
		General
	}
}
