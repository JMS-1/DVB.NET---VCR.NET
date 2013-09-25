using System;
using System.Xml.Serialization;

namespace JMS.DVB.Satellite
{
	/// <summary>
	/// Describes the connection information of a satelite channel.
	/// </summary>
    [Serializable]
    [XmlType("Satellite")]
    public class SatelliteChannel : Channel
	{
		/// <summary>
		/// [Don't know]
		/// </summary>
        public Viterbi Viterbi = Viterbi.Auto;

		/// <summary>
		/// Symbol rate.
		/// </summary>
        public uint SymbolRate;

		/// <summary>
		/// Polarisation selection.
		/// </summary>
        public PowerMode Power = PowerMode.Off;

		/// <summary>
		/// The zero based index of the LNB the channel is found on.
		/// </summary>
		/// <remarks>
		/// The value must not be less than zero or greater than three.
		/// </remarks>
        public int LNBIndex;

		/// <summary>
		/// Will be set when the channel is DVB-S2 modulated.
		/// </summary>
		public bool S2Modulation = false;

		public SatelliteChannel(int lnbIndex, uint uFrequency, SpectrumInversion eInversion, uint uSymbolRate, PowerMode ePower, Viterbi eViterbi)
			: this(lnbIndex, uFrequency, eInversion, uSymbolRate, ePower, eViterbi, false)
		{
		}

		/// <summary>
		/// Create a new instance.
		/// </summary>
		/// <param name="lnbIndex">Zero based index of the LNB the channel should be bound to.</param>
		/// <param name="uFrequency">Frequency.</param>
		/// <param name="eInversion">Spectrum inversion.</param>
		/// <param name="uSymbolRate">Symbol rate.</param>
		/// <param name="ePower">Polarisation selection.</param>
		/// <param name="eViterbi">[Don't know]</param>
        /// <param name="DVBS2">Gesetzt, wenn es sich um einen DVB-S2 Transponder handelt.</param>
		/// <exception cref="ArgumentOutOfRangeException">The LNB index is negative or greater than 3.</exception>
		public SatelliteChannel(int lnbIndex, uint uFrequency, SpectrumInversion eInversion, uint uSymbolRate, PowerMode ePower, Viterbi eViterbi, bool DVBS2)
			: base(uFrequency, eInversion)
		{
			// Verify
			if ( (lnbIndex < 0) || (lnbIndex > 3) ) throw new ArgumentOutOfRangeException("lnbIndex", lnbIndex, "Must not be less than zero or greater than 3");

			// Remember
            SymbolRate = uSymbolRate;
			S2Modulation = DVBS2;
            LNBIndex = lnbIndex;
            Viterbi = eViterbi;
            Power = ePower;
		}

        /// <summary>
        /// Create a new instance.
        /// </summary>
        public SatelliteChannel()
        {
        }

		/// <summary>
		/// Compare with other satellite channel description.
		/// </summary>
		/// <param name="obj">The other channel.</param>
		/// <returns>Success if the other object is a satellite channel and all fields match
		/// exactly - including a call to <see cref="Channel.Equals"/> on the base class.</returns>
		public override bool Equals(object obj)
		{
			// Change type
			SatelliteChannel pOther = obj as SatelliteChannel;

			// Check
            return (null != pOther) && base.Equals(obj) && (LNBIndex == pOther.LNBIndex) && (Viterbi == pOther.Viterbi) && (SymbolRate == pOther.SymbolRate) && (Power == pOther.Power);
		}

		/// <summary>
		/// Derive a hash code.
		/// </summary>
		/// <returns>The hash code from <see cref="Channel.GetHashCode"/> merged with our fields.</returns>
		public override int GetHashCode()
		{
			// Calculate hint
            return base.GetHashCode() ^ LNBIndex.GetHashCode() ^ Viterbi.GetHashCode() ^ SymbolRate.GetHashCode() ^ Power.GetHashCode();
		}

		/// <summary>
		/// Create a new transponder from a string representation.
		/// </summary>
		/// <param name="settings">The string representation.</param>
        /// <param name="lnbIndex">Die laufende Nummer der zu verwendenden Antenne.</param>
		/// <returns>The new transponder instance.</returns>
		public static SatelliteChannel Create(string settings, int lnbIndex)
		{
			// Split off
			string[] items = settings.Split(',');

			// Not possible
			if (items.Length < 4) return null;

			// Parts
			string frequency = items[0].Trim();
			string polarisation = items[1].Trim();
			string symrate = items[2].Trim();
			string errorcorrection = items[3].Trim();
			bool DVBS2 = ((items.Length > 4) && items[4].Trim().Equals("S2"));

			// Fraction part
			int ix = frequency.IndexOf('.');
			
			// Remove
			if (ix >= 0) frequency = frequency.Remove(ix, 1);

			// Get numbers
			uint freq, rate;
			if (!uint.TryParse(frequency, out freq)) return null;
			if (!uint.TryParse(symrate, out rate)) return null;

			// Correct
			freq *= 1000;

			// Check for fraction part
			if (ix >= 0) freq /= (uint)Math.Pow(10, frequency.Length - ix);

			// Check polarisation
			string pol = polarisation;
			bool hor = (0 == string.Compare(pol, "H", true));
			if (!hor && (0 != string.Compare(pol, "V", true))) return null;

			// FEC
			string fec = errorcorrection;
			if (fec.Length < 2) return null;
			
			// Save parse
			Viterbi viterbi;
			try
			{
				// Process
				viterbi = (Viterbi)Enum.Parse(typeof(Viterbi), string.Format("Rate{0}_{1}", fec[0], fec.Substring(1)));
			}
			catch
			{
				// Fallback
				viterbi = Viterbi.Auto;
			}

			// Create
			return new SatelliteChannel(lnbIndex, freq, SpectrumInversion.Auto, rate * 1000, hor ? PowerMode.Horizontal : PowerMode.Vertical, viterbi, DVBS2);
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
            format = Equals(format, "1") ? "{0},{1},{2},{3},{5},DVB-S{4},LNB{6}" : "{0},{1},{2},{3},{5},DVB-S{4}";

            // Format our data
            return string.Format(format, Frequency, Power, SymbolRate, Viterbi, S2Modulation ? "2" : string.Empty, SpectrumInversion, 1 + LNBIndex);
		}
	}
}
