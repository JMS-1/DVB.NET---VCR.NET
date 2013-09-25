using System;
using System.IO;
using System.Text;

namespace EasyCut
{
	/// <summary>
	/// Implementation of a SUB format sub-title cutter.
	/// </summary>
	public class SUPCutter : CutterBase
	{
		/// <summary>
		/// The path to the first source file which will be used to locate the 
		/// IFO color description.
		/// </summary>
		private string FirstSource = null;

		/// <summary>
		/// Overall collector of the cut result.
		/// </summary>
		private MemoryStream Collector;

		/// <summary>
		/// Create a new instance of the algorithm.
		/// </summary>
		public SUPCutter()
		{
			// Create result in memory
			Collector = new MemoryStream();
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
			// Remember
			if (null == FirstSource) FirstSource = source;

			// Open the sub title file
			using (FileStream ttx = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 100000))
			{
				// Minimum length required
				long minFrames = (long)Math.Round(MinDuration * Framerate);

				// Overall correction
				long corr = startPos - pos;

				// Load all
				for (byte[] head = new byte[12]; head.Length == ttx.Read(head, 0, head.Length); )
				{
					// Validate
					if (('S' != head[0]) || ('P' != head[1])) break;

					// Load PTS
					long pts0 = head[2];
					long pts1 = head[3];
					long pts2 = head[4];
					long pts3 = head[5];
					long pts4 = head[6];
					long pts5 = head[7];
					long pts6 = head[8];
					long pts7 = head[9];
					long pts = pts0 + 256 * (pts1 + 256 * (pts2 + 256 * (pts3 + 256 * (pts4 + 256 * (pts5 + 256 * (pts6 + 256 * pts7))))));

					// Convert
					TimeSpan start = new TimeSpan(pts * 1000 / 9), originalPTS = start;

					// To frame
					long ttxStart = (long)((start.TotalSeconds + TimeCorrection) * Framerate);

					// We are fully done
					if (ttxStart >= endPos) break;

					// Construct length
					int low = head[11];
					int high = head[10];
					int len = low + 256 * high;

					// Allocate
					byte[] frame = new byte[len - 2];

					// Load
					if (frame.Length != ttx.Read(frame, 0, frame.Length)) break;

					// We didn't reach the beginning
					if (ttxStart < startPos) continue;

					// Get the duration of the packet
					int duration = GetDuration(frame), originalDuaration = duration;

					// Invalid packet
					if (duration < 0) continue;

					// To frame
                    double rawFrames = duration * 1024.0 / 90000.0 * Framerate;

					// Check the limit
                    if (minFrames > (long)rawFrames) continue;

                    // Convert down
                    long frames = (long)Math.Round(rawFrames), originalFrames = frames;

                    // Get the end
					long ttxEnd = ttxStart + frames;

					// Clip if running beyond the end of the cut
					if (ttxEnd >= endPos) break;

					// Shift
					ttxStart -= corr;

					// Convert to PTS
					pts = (long)Math.Round(ttxStart * 90000 / Framerate);

					// Update header
					head[2] = (byte)(pts & 0xff);
					head[3] = (byte)((pts >> 8) & 0xff);
					head[4] = (byte)((pts >> 16) & 0xff);
					head[5] = (byte)((pts >> 24) & 0xff);
					head[6] = (byte)((pts >> 32) & 0xff);
					head[7] = (byte)((pts >> 40) & 0xff);
					head[8] = (byte)((pts >> 48) & 0xff);
					head[9] = (byte)((pts >> 56) & 0xff);

					// Write header
					Collector.Write(head, 0, head.Length);

					// Write frame
					Collector.Write(frame, 0, frame.Length);

					// Report
					Log("{0} ({1} == {2}) from {3}", new TimeSpan(pts * 1000 / 9), originalDuaration, frames, originalPTS);
				}
			}
		}

		/// <summary>
		/// Schreibt eine Zeile in die Logdatei.
		/// </summary>
		/// <param name="format">Format der Zeile.</param>
		/// <param name="args">Parameter für die Zeile.</param>
		private static void Log(string format, params object[] args)
		{
			// Read the log path
			string path = Properties.Settings.Default.SUPLogPath;

			// None
			if (string.IsNullOrEmpty(path)) return;

			// Write it
			using (StreamWriter stream = new StreamWriter(path, true, Encoding.Unicode)) stream.WriteLine(format, args);
		}

		/// <summary>
		/// Save the cut sub-titles from memory to the indicated summary file.
		/// </summary>
		/// <remarks>
		/// The color IFO will be taken from the first file reported to <see cref="Cut"/>.
		/// </remarks>
		/// <param name="target">Path to the sub-title file.</param>
		public override void Save(string target)
		{
			// Open the file
			using (FileStream ttxAll = new FileStream(target, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				// Make sure that all data is transferred
				Collector.Flush();

				// Store all
				ttxAll.Write(Collector.GetBuffer(), 0, (int)Collector.Length);
			}

			// Copy the IFO file
			if (null != FirstSource) File.Copy(FirstSource + ".ifo", target + ".ifo", true);
		}

		/// <summary>
		/// Calculate the duration from a SUP packet.
		/// </summary>
		/// <param name="buffer">The SUP packet excluding the length of the packet.</param>
		/// <returns>Overall delay inside the SUP packet in SUP units - multiply by 1024
		/// to get a 90kHz clock.</returns>
		private static int GetDuration(byte[] buffer)
		{
			// We expect no error but just in case be nice to the caller
			try
			{
				// Load offset to first packet
				int offH = buffer[0];
				int offL = buffer[1];
				int offset = offL + 256 * offH - 2;

				// Total duration
				int total = 0;

				// Process all sequences
				for (; ; )
				{
					// Load delay of command table
					int delH = buffer[offset + 0];
					int delL = buffer[offset + 1];
					int del = delL + 256 * delH;

					// Sum up
					total += del;

					// Start processing commands
					int nextH = buffer[offset + 2];
					int nextL = buffer[offset + 3];
					int next = nextL + 256 * nextH - 2;

					// We are done - points to self
					if (next == offset) return total;

					// Protect against invalid data leading to endless loops
					if (next < offset) return -1;

					// Advance
					offset = next;
				}
			}
			catch
			{
				// Somewhere failed
				return -1;
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Do proper cleanup.
		/// </summary>
		public override void Dispose()
		{
			// Cleanup
			if (null != Collector)
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
