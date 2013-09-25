using System;
using System.Xml.Serialization;

namespace JMS.DVB.Cable
{
	/// <summary>
	/// Describes the connection information of a cable channel.
	/// </summary>
	[Serializable]
    [XmlType("Cable")]
    public class CableChannel : Channel
	{
		/// <summary>
		/// [Don't know]
		/// </summary>
        public Qam QAM = Qam.Qam16;

		/// <summary>
		/// Symbol rate.
		/// </summary>
        public uint SymbolRate;

		/// <summary>
		/// Badwidth type.
		/// </summary>
        public BandwidthType Bandwidth = BandwidthType.Auto;

        /// <summary>
        /// Create a new cable channel descriptor.
        /// </summary>
        public CableChannel()
        {
        }

		/// <summary>
		/// Create a new cable channel descriptor.
		/// </summary>
		/// <param name="uFrequency">Frequency.</param>
		/// <param name="eInversion">Spectrum inversion.</param>
		/// <param name="uSymbolRate">Symbol rate.</param>
		/// <param name="eQam">[Don't know]</param>
		/// <param name="bwType">Bandwidth type.</param>
		public CableChannel(uint uFrequency, SpectrumInversion eInversion, uint uSymbolRate, Qam eQam, BandwidthType bwType) : base(uFrequency, eInversion)
		{
			// Remember
            SymbolRate = uSymbolRate;
            Bandwidth = bwType;
            QAM = eQam;
		}

		/// <summary>
		/// Compare two cable channel definitions.
		/// </summary>
		/// <param name="obj">Other cable channel.</param>
		/// <returns>Success if the other object is also a cable channel and all fields match.
		/// This includes calling <see cref="Channel.Equals"/> on the base class.
		/// </returns>
		public override bool Equals(object obj)
		{
			// Change type
			CableChannel pOther = obj as CableChannel;

			// Check
            return (null != pOther) && base.Equals(obj) && (QAM == pOther.QAM) && (SymbolRate == pOther.SymbolRate) && (Bandwidth == pOther.Bandwidth);
		}

		/// <summary>
		/// Derive hash code.
		/// </summary>
		/// <returns>Merges <see cref="Channel.GetHashCode"/> with our fields.</returns>
		public override int GetHashCode()
		{
			// Calculate hint
            return base.GetHashCode() ^ QAM.GetHashCode() ^ SymbolRate.GetHashCode() ^ Bandwidth.GetHashCode();
		}

        /// <summary>
        /// Ermittelt den Namen dieses Transponders.
        /// </summary>
        /// <param name="format">Unterstützt wird <i>0</i> für eine kurze Darstellung und
        /// <i>1</i> für eine volle Darstellung.</param>
        /// <returns>Der Name dieses Transponders.</returns>
        public override string ToString(string format)
		{
            // Format our data
            return string.Format("{0},{1},{2},{3},{4}", Frequency, QAM, SymbolRate, Bandwidth, SpectrumInversion);
		}
	}
}
