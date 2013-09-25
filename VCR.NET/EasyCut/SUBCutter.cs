using System;
using System.IO;
using System.Text;

namespace EasyCut
{
	/// <summary>
	/// Cutting algorithm for SUB format sub-title files.
	/// </summary>
	public class SUBCutter: CutterBase
	{
		/// <summary>
		/// Overall collection of the cut file.
		/// </summary>
		private MemoryStream Collector;

		/// <summary>
		/// Helper wrapper for formatted writing to <see cref="Collector"/>.
		/// </summary>
		private StreamWriter Target;

		/// <summary>
		/// Create a new cutting algorithm.
		/// </summary>
		public SUBCutter()
		{
			// Create result in memory
			Collector = new MemoryStream();

			// Result
			Target = new StreamWriter(Collector, Encoding.GetEncoding(1252));	
		}

		/// <summary>
		/// Cut from the indicated sub-title source into the memory buffer.
		/// </summary>
		/// <param name="source">Sub-title file name.</param>
		/// <param name="startPos">First frame to cut.</param>
		/// <param name="endPos">Last frame to cut.</param>
		/// <param name="pos">Current frame in resulting file.</param>
		public override void Cut(string source, long startPos, long endPos, long pos)
		{
			// Minimum length required
			long minFrames = (long)(MinDuration * Framerate);

			// Open the sub title file
			using (StreamReader ttx = new StreamReader(source, Encoding.GetEncoding(1252)))
			{
				// Overall correction
				long corr = startPos - pos;

				// Load all
				for ( string line ; null != (line = ttx.ReadLine()) ; )
				{
					// Break into peaces
					string[] split = line.Split('{', '}');

					// Validate
					if ( 5 != split.Length ) continue;
			
					// Load
					long ttxStart = long.Parse(split[1]) + FrameCorrection;
					long ttxEnd = long.Parse(split[3]) + FrameCorrection;

					// Validate
					if ( ttxStart > ttxEnd ) continue;

					// We are fully done
					if ( ttxStart > endPos ) break;

					// We didn't reach the beginning
					if ( ttxEnd < startPos ) continue;

					// Clip
					if ( ttxStart < startPos ) ttxStart = startPos;
					if ( ttxEnd > endPos ) ttxEnd = endPos;

					// We are too short to be shown
					if ( (ttxEnd - ttxStart + 1) < minFrames ) continue;

					// Shift
					ttxStart -= corr;
					ttxEnd -= corr;

					// Send
					Target.WriteLine("{3}{0}{4}{3}{1}{4}{2}", ttxStart, ttxEnd, split[4], '{', '}');
				}
			}
		}

		/// <summary>
		/// Save the cut sub-titles from memory to the indicated summary file.
		/// </summary>
		/// <param name="target">Path to the sub-title file.</param>
		public override void Save(string target)
		{
			// Open the file
			using (FileStream ttxAll = new FileStream(target, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				// Make sure that all data is transferred
				Target.Flush();

				// Store all
				ttxAll.Write(Collector.GetBuffer(), 0, (int)Collector.Length);
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Do proper cleanup.
		/// </summary>
		public override void Dispose()
		{
			// Cleanup
			if ( null != Target )
			{
				// Close
				Target.Close();

				// Done
				Target = null;
			}
			if ( null != Collector ) 
			{
				// Close
				Collector.Close();

				// Done
				Collector = null;
			}
		}

		#endregion
	}
}
