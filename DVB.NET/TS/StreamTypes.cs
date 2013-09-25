using System;

namespace JMS.DVB.TS
{
	/// <summary>
	/// Internally used stream types for transport streams elements.
	/// </summary>
	public enum StreamTypes
	{
		/// <summary>
		/// Video stream.
		/// </summary>
		Video = 2,

		/// <summary>
		/// Audio stream.
		/// </summary>
		Audio = 3,

		/// <summary>
		/// Dolby Digital (AC3) stream mapped to a private stream.
		/// </summary>
		Private = 6,

		/// <summary>
		/// Teletext stream mapped to a private stream.
		/// </summary>
		TeleText = -1,

        /// <summary>
        /// DVB subtitle stream mapped to a private stream.
        /// </summary>
        SubTitles = -2
	}
}
