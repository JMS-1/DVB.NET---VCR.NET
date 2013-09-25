using System;

namespace EasyCut
{
	/// <summary>
	/// Helper class for implementing cutting algorithms.
	/// </summary>
	public abstract class CutterBase: ICutter2
	{
		/// <summary>
		/// The minimum duration of a sub-title fragment in seconds.
		/// <seealso cref="MinDuration"/>
		/// </summary>
		/// <remarks>
		/// The default is zero.
		/// </remarks>
		private double m_MinDuration = 0.0;

		/// <summary>
		/// The video framerate.
		/// <see cref="Framerate"/>
		/// </summary>
		/// <remarks>
		/// Defaults to 25Hz.
		/// </remarks>
		private double m_Framerate = 25.0;

		/// <summary>
		/// Time correction of each frame in seconds.
		/// </summary>
		private double m_TimeShift = 0.0;

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		protected CutterBase()
		{
		}

		protected long FrameCorrection
		{
			get
			{
				// Calculate
				return (long)(m_TimeShift * m_Framerate);
			}
		}

		#region ICutter Members

		/// <summary>
		/// Cut from the indicated sub-title source into the memory buffer.
		/// </summary>
		/// <param name="source">Sub-title file name.</param>
		/// <param name="startPos">First frame to cut.</param>
		/// <param name="endPos">Last frame to cut.</param>
		/// <param name="pos">Current frame in resulting file.</param>
		public abstract void Cut(string source, long startPos, long endPos, long pos);

		/// <summary>
		/// Save the cut sub-titles from memory to the indicated summary file.
		/// </summary>
		/// <param name="target">Path to the sub-title file.</param>
		public abstract void Save(string target);

		/// <summary>
		/// Get or set the video framerate.
		/// </summary>
		public double Framerate
		{
			get
			{
				// Report
				return m_Framerate;
			}
			set
			{
				// Change
				m_Framerate = value;
			}
		}

		/// <summary>
		/// Get or set the minimum duration of a sub-title fragment in seconds.
		/// </summary>
		public double MinDuration
		{
			get
			{
				// Report
				return m_MinDuration;
			}
			set
			{
				// Change
				m_MinDuration = value;
			}
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Do proper cleanup.
		/// </summary>
		public abstract void Dispose();

		#endregion

		#region ICutter2 Members

		/// <summary>
		/// Get or set the time to add to each frame.
		/// </summary>
		public double TimeCorrection
		{
			get
			{
				// Report
				return m_TimeShift;
			}
			set
			{
				// Update
				m_TimeShift = value;
			}
		}

		#endregion
	}
}
