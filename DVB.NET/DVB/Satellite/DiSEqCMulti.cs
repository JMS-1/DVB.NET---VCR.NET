using System;
using System.Xml.Serialization;

namespace JMS.DVB.Satellite
{
	/// <summary>
	/// Represents a configuration which selects between four
	/// satellites receivers.
	/// </summary>
	[Serializable] public class DiSEqCMulti: DiSEqC
	{
		/// <summary>
		/// The LNB position to select.
		/// </summary>
		public bool AlternatePosition = false;

		/// <summary>
		/// The LNB option to select.
		/// </summary>
		public bool AlternateOption = false;

		/// <summary>
		/// The frequency band to use.
		/// </summary>
		[XmlIgnore] public bool UseHighBand = false;

		/// <summary>
		/// Use horizontal polarisation.
		/// </summary>
		[XmlIgnore] public bool HorizontalPolarization = false;

		/// <summary>
		/// Create a configuration item.
		/// </summary>
		/// <param name="position">LNB position to select.</param>
		/// <param name="option">LNB option to select.</param>
		/// <param name="lnb">LNB parameters to use.</param>
		public DiSEqCMulti(bool position, bool option, LNBSettings lnb) : base(lnb)
		{
			// Remember
			AlternatePosition = position;
			AlternateOption = option;
		}

		/// <summary>
		/// Create a new instance.
		/// </summary>
		public DiSEqCMulti()
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
				byte[] ret = new byte[] { 0xe0, 0x10, 0x38, 0xf0 };

				// Select the LNB
				if ( UseHighBand )				ret[3] |= 0x01;
				if ( HorizontalPolarization )	ret[3] |= 0x02;
				if ( AlternatePosition )		ret[3] |= 0x04;
				if ( AlternateOption )			ret[3] |= 0x08;

				// Report
				return ret;
			}
		}

        /// <summary>
        /// Create a DiSEqC message information instance.
        /// </summary>
        /// <remarks>
        /// Will create a <i>e0 10 38 fX</i> sequence which must be sent three times.
        /// </remarks>
        /// <param name="channel">The channel to tune upon.</param>
        /// <returns>A DiSEqC message.</returns>
        public override DiSEqCMessage CreateMessage(SatelliteChannel channel)
        {
            // Create tone burst entry
            byte[] cmd = new byte[] { 0xe0, 0x10, 0x38, 0xf0 };

            // Select the LNB
            if (Use22kHz(channel.Frequency))            cmd[3] |= 0x01;
            if (PowerMode.Horizontal == channel.Power)  cmd[3] |= 0x02;
            if (AlternatePosition)                      cmd[3] |= 0x04;
            if (AlternateOption)                        cmd[3] |= 0x08;

            // Create full message
            return new DiSEqCMessage(cmd, 0xff, 3);
        }

		/// <summary>
		/// Show some textual representation of this DiSEqC setting.
		/// </summary>
		/// <returns>Primary used for debug purposes.</returns>
		public override string ToString()
		{
			// Report
			return string.Format("Position/Option {0}{1}", AlternatePosition ? "B" : "A", AlternateOption ? "B" : "A");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			// Construct
			return base.GetHashCode() ^ AlternatePosition.GetHashCode() ^ AlternateOption.GetHashCode();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			// Change type
			DiSEqCMulti other = obj as DiSEqCMulti;

			// Never
			if (null == other) return false;

			// Process
			return (AlternatePosition == other.AlternatePosition) && (AlternateOption == other.AlternateOption) && base.Equals(obj);
		}
	}
}
