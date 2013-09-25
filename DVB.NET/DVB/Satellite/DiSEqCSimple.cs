using System;

namespace JMS.DVB.Satellite
{
	/// <summary>
	/// Represents a configuration where a tone burst signal selects between two
	/// satellites receivers.
	/// </summary>
	[Serializable] public class DiSEqCSimple: DiSEqC
	{        
        /// <summary>
		/// The LNB to select.
		/// </summary>
		public bool Position = false;

		/// <summary>
		/// Create a configuration item.
		/// </summary>
		/// <param name="position">LNB to select.</param>
		/// <param name="lnb">LNB parameters to use.</param>
		public DiSEqCSimple(bool position, LNBSettings lnb) : base(lnb)
		{
			// Remember
			Position = position;
		}

		/// <summary>
		/// Create a new instance.
		/// </summary>
		public DiSEqCSimple()
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
				// Create tone burst entry
				return new byte[] { (byte)(Position ? 0 : 1) };
			}
		}

        /// <summary>
        /// Create a DiSEqC message information instance.
        /// </summary>
        /// <remarks>
        /// No command is sent but the burst state is provided.
        /// </remarks>
        /// <param name="channel">The channel to tune upon.</param>
        /// <returns>A DiSEqC message.</returns>
        public override DiSEqCMessage CreateMessage(SatelliteChannel channel)
        {
            // Send the burst
            return new DiSEqCMessage(new byte[0], Data[0], 1);
        }

		/// <summary>
		/// Show some textual representation of this DiSEqC setting.
		/// </summary>
		/// <returns>Primary used for debug purposes.</returns>
		public override string ToString()
		{
			// Report
			return Position ? "off" : "on";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			// Construct
			return base.GetHashCode() ^ Position.GetHashCode();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			// Change
			DiSEqCSimple other = obj as DiSEqCSimple;

			// Never
			if (null == other) return false;

			// Forward
			return (Position == other.Position) && base.Equals(obj);
		}
	}
}
