using System;

namespace JMS.DVB.TS
{
	/// <summary>
	/// Represents a single teletext stream.
	/// </summary>
	public class TTXStream: StreamBase
	{
		/// <summary>
		/// Create a new instance.
		/// </summary>
		/// <param name="consumer">Related transport stream.</param>
		/// <param name="pid">Transport stream identifier for this teletext stream.</param>
		/// <param name="isPCR">Set if this stream supplies the PCR.</param>
		public TTXStream(IStreamConsumer consumer, short pid, bool isPCR)
			: base(consumer, pid, isPCR)
		{
		}

		/// <summary>
		/// The only valid PES start code for teletext stream is <i>0x000001bd</i>.
		/// </summary>
		/// <param name="code">The last byte of a start code.</param>
		/// <returns>Set if the start code represents a teletext stream.</returns>
		protected override bool IsValidStartCode(byte code)
		{
			// Must be private data
			return (0xbd == code);
		}

		/// <summary>
		/// Accept a new package but do not process until the first PCR is sent.
		/// </summary>
		/// <param name="buffer">Buffer holding tha data.</param>
		/// <param name="start">First byte of the package.</param>
		/// <param name="length">Length of the package.</param>
		public override void AddPayload(byte[] buffer, int start, int length)
		{
			// Forward to base if PCR is sent to the stream
			if (Consumer.PCRAvailable) base.AddPayload(buffer, start, length);
		}
	}
}
