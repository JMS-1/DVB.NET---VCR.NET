using System;
using System.Xml.Serialization;

namespace  JMS.DVB.Terrestrial
{
	/// <summary>
	/// Describes the connection information of a terrestrial channel.
	/// </summary>
    [Serializable]
    [XmlType("Terrestrial")]
	public class TerrestrialChannel : Channel
	{
		/// <summary>
		/// [Don't know]
		/// </summary>
        public bool FullRescan;

		/// <summary>
		/// Badwidth type.
		/// </summary>
        public BandwidthType Bandwidth = BandwidthType.Auto;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        public TerrestrialChannel()
        {
        }

		/// <summary>
		/// Create a new instance.
		/// </summary>
		/// <param name="uFrequency">Frequency.</param>
		/// <param name="eInversion">Spectrum inversion.</param>
		/// <param name="bScan">[Don't know]</param>
		/// <param name="bwType">Bandwidth type.</param>
		public TerrestrialChannel(uint uFrequency, SpectrumInversion eInversion, bool bScan, BandwidthType bwType) : base(uFrequency, eInversion)
		{
			// Remember
            FullRescan = bScan;
            Bandwidth = bwType;
		}

		/// <summary>
		/// Compare against some other terrestrial channel.
		/// </summary>
		/// <param name="obj">The other channel.</param>
		/// <returns>Success if the other object is a terrestrial channel and <see cref="FullRescan"/>
		/// matches in addition to a call <see cref="Channel.Equals"/> on the base class.
		/// </returns>
		public override bool Equals(object obj)
		{
			// Change type
			TerrestrialChannel pOther = obj as TerrestrialChannel;

			// Check
            return (null != pOther) && base.Equals(obj) && (FullRescan == pOther.FullRescan) && (Bandwidth == pOther.Bandwidth);
		}

		/// <summary>
		/// Derive hashcode.
		/// </summary>
		/// <returns>Merges <see cref="FullRescan"/> with <see cref="Channel.GetHashCode"/>
		/// of the base class.</returns>
		public override int GetHashCode()
		{
			// Calculate hint
            return base.GetHashCode() ^ FullRescan.GetHashCode() ^ Bandwidth.GetHashCode();
		}

        /// <summary>
        /// Ermittelt den Namen dieses Transponders.
        /// </summary>
        /// <param name="format">Unterstützt wird <i>0</i> für eine kurze Darstellung und
        /// <i>1</i> für eine volle Darstellung.</param>
        /// <returns>Der Name dieses Transponders.</returns>
        public override string ToString(string format)
		{
            // Translate format
            format = Equals(format, "1") ? "{0},{1},{2},{3}" : "{0},{1},{2}";

			// Format our data
			return string.Format(format, Frequency, SpectrumInversion, Bandwidth, FullRescan);
		}
	}
}
