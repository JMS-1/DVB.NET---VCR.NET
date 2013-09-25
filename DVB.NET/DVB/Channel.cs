using System;
using System.Xml.Serialization;

namespace JMS.DVB
{
	/// <summary>
	/// Base channel information common to all types of channels.
	/// </summary>
    [Serializable]
	public abstract class Channel: JMS.DVB.Transponder
	{
		/// <summary>
		/// Spectrum inversion settings.
		/// </summary>
        public SpectrumInversion SpectrumInversion = SpectrumInversion.Auto;

		/// <summary>
		/// Master frequency.
		/// </summary>
        public uint Frequency;

        /// <summary>
        /// Initialize the instance.
        /// </summary>
        protected Channel()
        {
        }

		/// <summary>
		/// Initialize the instance.
		/// </summary>
		/// <param name="uFrequency">Master frequency.</param>
		/// <param name="eInversion">Spectrum inversion settings.</param>
		protected Channel(uint uFrequency, SpectrumInversion eInversion)
		{
			// Remember
            SpectrumInversion = eInversion;
            Frequency = uFrequency;
		}

		/// <summary>
		/// Compare two channels.
		/// </summary>
		/// <param name="obj">The other channel.</param>
		/// <returns>Success if the other object is a channel and all fields match exactly.</returns>
		public override bool Equals(object obj)
		{
			// Change type
			Channel pOther = obj as Channel;

			// Check
            return (null != pOther) && (Frequency == pOther.Frequency) && (SpectrumInversion == pOther.SpectrumInversion);
		}

		/// <summary>
		/// Derive a hash code.
		/// </summary>
		/// <returns>Merges the fields using XOR on its hascodes.</returns>
		public override int GetHashCode()
		{
			// Calculate hint
            return Frequency.GetHashCode() ^ SpectrumInversion.GetHashCode();
		}
	}
}
