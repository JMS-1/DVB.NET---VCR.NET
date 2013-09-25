using System;

namespace JMS.TechnoTrend
{
	/// <summary>
	/// Indicates the type of a <see cref="JMS.TechnoTrend.MFCWrapper.DVBRawFilter"/>.
	/// </summary>
	public enum FilterType
	{
		/// <summary>
		/// Filter inactive.
		/// </summary>
		None,

		/// <summary>
		/// Stream data.
		/// </summary>
		Streaming,

		/// <summary>
		/// Use pipe.
		/// </summary>
		Piping,

		/// <summary>
		/// Per sesion.
		/// </summary>
		Section,

		/// <summary>
		/// Don't know.
		/// </summary>
		MPESection,

		/// <summary>
		/// Don't know.
		/// </summary>
		MPESectionHighSpeed
	}
}
