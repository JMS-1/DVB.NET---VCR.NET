using System;

namespace JMS.DVB.TS
{
	/// <summary>
	/// Represents a single Dobly Digital (AC3) audio stream
	/// </summary>
	public class DolbyStream: AudioStream
	{
		/// <summary>
		/// Create a new instance.
		/// </summary>
		/// <param name="consumer">Related transport stream.</param>
		/// <param name="pid">Transport stream identifier for this AC3 stream.</param>
		/// <param name="isPCR">Set if this stream supplies the PCR.</param>
		public DolbyStream(IStreamConsumer consumer, short pid, bool isPCR)
			: base(consumer, pid, isPCR)
		{
		}

		/// <summary>
		/// The only valid PES start code for an AC3 stream is normally <i>0x000001bd</i>.
        /// To support some special configurations like BBC HD other audio codes are
        /// accepted as well.
		/// </summary>
		/// <param name="code">The last byte of a start code.</param>
		/// <returns>Set if the start code represents an AC3 stream.</returns>
		protected override bool IsValidStartCode(byte code)
		{
			// Must be private data
            if (0xbd == code)
                return true;
            else
                return base.IsValidStartCode( code );
		}
	}
}
