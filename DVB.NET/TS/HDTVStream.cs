using System;

namespace JMS.DVB.TS
{
	/// <summary>
	/// Represents a single H.264 video stream.
	/// </summary>
	public class HDTVStream: VideoStream
	{
		/// <summary>
		/// Create a new instance.
		/// </summary>
		/// <param name="consumer">Related transport stream.</param>
		/// <param name="pid">Transport stream identifier for this video stream.</param>
		/// <param name="isPCR">Set if this stream supplies the PCR.</param>
		public HDTVStream(IStreamConsumer consumer, short pid, bool isPCR)
			: base(consumer, pid, isPCR)
		{
		}

		/// <summary>
		/// Report that streaming must wait until the first key frame has been seen.
		/// </summary>
		protected override bool AwaitKeyFrame
		{
			get
			{
				// Wait for the first full frame
				return true;
			}
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
			// Size in header repecting the extension block
			int header = 9 + buffer[start + 8];

			// Correct
			length -= header;
			start += header;

			// For performance reasons we only check for the first 200 bytes
			if (length > 200) length = 200;

			// Find 00 00 00 01 09
			while (length-- >= 10)
				if (0x00 == buffer[start++])
					if (0x00 != buffer[start++])
						length -= 1;
					else if (0x00 != buffer[start++])
						length -= 2;
					else if (0x01 != buffer[start++])
						if (0x00 == buffer[start - 1])
							start -= 3;
						else
							length -= 3;
					else
					{
						// Adjust
						length -= 4;

						// Get code
						byte code = buffer[start++];

						// Test
						if (0x09 != code)
						{
							//// Check for freezed frame
							//if (0x67 == code)
							//    return true;
							//else if (0x27 == code)
							//    return true;

							// Next
							continue;
						}

						// Find 00 00 00 01 27 or 00 00 00 01 67
						while (length-- >= 5)
							if (0x00 == buffer[start++])
								if (0x00 != buffer[start++])
									length -= 1;
								else if (0x00 != buffer[start++])
									length -= 2;
								else if (0x01 != buffer[start++])
									if (0x00 == buffer[start - 1])
										start -= 3;
									else
										length -= 3;
								else if (0x67 == buffer[start])
									return true;
								else if (0x27 == buffer[start++])
									return true;
								else
									break;

						// Done
						break;
					}

			// Not found
			return false;
		}
	}
}
