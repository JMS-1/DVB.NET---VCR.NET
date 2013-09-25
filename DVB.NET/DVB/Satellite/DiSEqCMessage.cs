using System;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.Satellite
{
    /// <summary>
    /// Describes a DiSEqC message that has to be sent.
    /// </summary>
	public class DiSEqCMessage
	{
        /// <summary>
        /// Data to sent.
        /// </summary>
        public readonly byte[] Request;

        /// <summary>
        /// Burst mode flag.
        /// </summary>
        public readonly byte Burst;

        /// <summary>
        /// Number of times the command should be resent.
        /// </summary>
        public readonly int Repeat;

        /// <summary>
        /// Create a new DiSEqC message description.
        /// </summary>
        /// <param name="data">Data to be sent.</param>
        /// <param name="burst">Burst mode flag.</param>
        /// <param name="repeat">Number of times the data should be repeated.</param>
        public DiSEqCMessage(byte[] data, byte burst, int repeat)
        {
            // Validate
            if (null == data) throw new ArgumentNullException("data");
            if (repeat <= 0) throw new ArgumentOutOfRangeException("repeat");

            // Remember
            Repeat = repeat;
            Request = data;
            Burst = burst;
        }

        /// <summary>
        /// Retrieve some hashcode for this message.
        /// </summary>
        /// <returns>A has number.</returns>
        public override int GetHashCode()
        {
            // Merge all
            return Request.GetHashCode() ^ Burst.GetHashCode() ^ Repeat.GetHashCode();
        }

        /// <summary>
        /// Compare two message.
        /// </summary>
        /// <param name="obj">The other message.</param>
        /// <returns>Set if the messages are logically equal.</returns>
        public override bool Equals(object obj)
        {
            // Convert
            DiSEqCMessage other = obj as DiSEqCMessage;

            // Not possible
            if (null == other) return false;

            // Pre-Test
            if ((Request.Length != other.Request.Length) || (Burst != other.Burst) || (Repeat != other.Repeat)) return false;

            // Compare message itself
            for (int i = Request.Length; i-- > 0; )
                if (Request[i] != other.Request[i])
                    return false;

            // Same message
            return true;
        }

        /// <summary>
        /// Create an exact copy of the message.
        /// </summary>
        /// <returns>A copy of this message.</returns>
        public DiSEqCMessage Clone()
        {
            // Process
            return new DiSEqCMessage((byte[])Request.Clone(), Burst, Repeat);
        }
    }
}
