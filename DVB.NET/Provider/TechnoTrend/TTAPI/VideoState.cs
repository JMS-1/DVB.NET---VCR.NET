using System;

namespace JMS.TechnoTrend
{
	/// <summary>
	/// Report state on the current audio and video streams selected.
	/// <seealso cref="JMS.TechnoTrend.MFCWrapper.DVBAVControl"/>
	/// </summary>
	/// <remarks>
	/// Implemented as a placeholder - simply takes some time to complete
	/// it to full functionality.
	/// </remarks>
	public class VideoState
	{
		/// <summary>
		/// Current bit rate.
		/// </summary>
		private ushort m_BitRate;

		/// <summary>
		/// Create a new instance.
		/// </summary>
		/// <param name="uBitRate">Current bit rate.</param>
		internal VideoState(ushort uBitRate)
		{
			// Copy over
			m_BitRate = uBitRate;
		}

		/// <summary>
		/// Read <see cref="m_BitRate"/>.
		/// </summary>
		public ushort BitRate
		{
			get
			{
				// Report it
				return m_BitRate;
			}
		}
	}
}
