using System;
using System.IO;
using System.Text;

namespace EasyCut
{
	/// <summary>
	/// Interface used to integrate a cutting engine into the
	/// <i>EasyCut</i> application.
	/// </summary>
	public interface ICutter: IDisposable
	{
		/// <summary>
		/// Get or set the minimum duration of a sub-title fragment in seconds.
		/// </summary>
		double MinDuration { get; set; }

		/// <summary>
		/// Get or set the video framerate.
		/// </summary>
		double Framerate { get; set; }

		/// <summary>
		/// Cut from the indicated sub-title source into the memory buffer.
		/// </summary>
		/// <param name="source">Sub-title file name.</param>
		/// <param name="startPos">First frame to cut.</param>
		/// <param name="endPos">Last frame to cut.</param>
		/// <param name="pos">Current frame in resulting file.</param>
		void Cut(string source, long startPos, long endPos, long pos);

		/// <summary>
		/// Save the cut sub-titles from memory to the indicated summary file.
		/// </summary>
		/// <param name="target">Path to the sub-title file.</param>
		void Save(string target);
	}

	public interface ICutter2 : ICutter
	{
		/// <summary>
		/// Correction to apply to each frame.
		/// </summary>
		double TimeCorrection { get; set; }
	}
}
