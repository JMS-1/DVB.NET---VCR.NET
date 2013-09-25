using System;

namespace JMS.DVB.TS
{
	/// <summary>
	/// Represents a single video stream.
	/// </summary>
	public class VideoStream: StreamBase
	{
		/// <summary>
		/// Get or set if any PES length is accepted.
		/// </summary>
		public bool AcceptAnyLength = false;

		/// <summary>
		/// Create a new instance.
		/// </summary>
		/// <param name="consumer">Related transport stream.</param>
		/// <param name="pid">Transport stream identifier for this video stream.</param>
		/// <param name="isPCR">Set if this stream supplies the PCR.</param>
		public VideoStream(IStreamConsumer consumer, short pid, bool isPCR)
			: base(consumer, pid, isPCR)
		{
		}

		/// <summary>
		/// Valid PES start codes for video streams range from <i>0x000001e0</i>
		/// to <i>0x000001ef</i>.
		/// </summary>
		/// <param name="code">The last byte of a start code.</param>
		/// <returns>Set if the start code represents a video stream.</returns>
		protected override bool IsValidStartCode(byte code)
		{
			// Must be video
			return ((code >= 0xe0) && (code < 0xf0));
		}

		/// <summary>
		/// The only valid length for a DVB-S PES packet is <i>0</i>.
		/// </summary>
		/// <param name="length">Length of the current PES packet.</param>
		/// <returns>Set if the parameter is <i>0</i>.</returns>
		protected override bool IsValidLength(int length)
		{
			// Always accepted
			if (0 == length) return true;

			// See if other length is allowed - if so, do not check!
			return AcceptAnyLength;
		}

		/// <summary>
		/// See, if the indicated position in the buffer is a keyframe. Prior to
		/// calling this method the caller checks that it really starts with
		/// a PES header.
		/// </summary>
		/// <param name="buffer">Some buffer.</param>
		/// <param name="start">First byte of the packet.</param>
		/// <param name="length">Number of bytes in the packet.</param>
		/// <returns>Set if this packet starts a key frame.</returns>
		protected override bool IsKeyFrame(byte[] buffer, int start, int length)
		{
			// Size in header respecting the extension block
			int header = 9 + buffer[start + 8];

			// Correct
			length -= header;

			// See if there is enough data to test
			if (length < 4) return false;

			// Calculate packet start 
			start += header;

			// Check for GOP start
			return (((0 == buffer[start++]) && (0 == buffer[start++]) && (1 == buffer[start++]) && (0xb3 == buffer[start++])));
		}

		/// <summary>
		/// Video streams must use the largest buffer available.
		/// </summary>
		protected override bool IsVideo
		{
			get
			{
				// Report
				return true;
			}
		}
	}
}
