using System;

namespace JMS.DVB.Satellite
{
	/// <summary>
	/// Empty DiSEqC message.
	/// </summary>
	[Serializable] public class DiSEqCNone : DiSEqC
	{
		/// <summary>
		/// Forward to base class.
		/// </summary>
		/// <param name="pLNB">LNB parameters.</param>
		public DiSEqCNone(LNBSettings pLNB) : base(pLNB)
		{
		}

		/// <summary>
		/// Forward to base class.
		/// </summary>
		public DiSEqCNone()
		{
		}

		/// <summary>
		/// Report the byte sequence to send.
		/// </summary>
		/// <remarks>
		/// The caller is allowed to modify the resulting <see cref="Array"/>.
		/// </remarks>
		public override byte[] Data
		{
			get
			{
				// Report disabled
				return new byte[] { 0xe0, 0x00, 0x00 }; 
			}
		}

        /// <summary>
        /// Create a DiSEqC message information instance.
        /// </summary>
        /// <remarks>
        /// Creates a <i>e0 00 00</i> reset sequence.
        /// </remarks>
        /// <param name="channel">The channel to tune upon.</param>
        /// <returns>A DiSEqC message.</returns>
        public override DiSEqCMessage CreateMessage(SatelliteChannel channel)
        {
            // Send a reset
            return new DiSEqCMessage(Data, 0xff, 1);
        }

		/// <summary>
		/// Show some textual representation of this DiSEqC setting.
		/// </summary>
		/// <returns>Primary used for debug purposes.</returns>
		public override string ToString()
		{
			// Report
			return "none";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			// Forward to base
			return base.GetHashCode();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			// Test
			return (obj is DiSEqCNone) && base.Equals(obj);
		}
	}
}
