using System;

namespace JMS.DVB.Satellite
{
	/// <summary>
	/// Describe LNB settings.
	/// </summary>
	[Serializable] public class LNBSettings
	{
		/// <summary>
		/// Default with <see cref="LOF1"/> <i>9750000</i>, <see cref="LOF2"/> <i>10600000</i>, 
		/// <see cref="Switch"/> <i>11700000</i> and <see cref="UsePower"/> <i>true</i>.
		/// </summary>
		public static readonly LNBSettings DefaultSettings = new LNBSettings(9750000, 10600000, 11700000, true);

		/// <summary>
		/// Low frequency.
		/// </summary>
		public uint LOF1 = 0;

		/// <summary>
		/// High frequency. 
		/// </summary>
		public uint LOF2 = 0;

		/// <summary>
		/// Switch frequency. 
		/// </summary>
		public uint Switch = 0;

		/// <summary>
		/// Use power.
		/// </summary>
		public bool UsePower = false;

		/// <summary>
		/// Parameterless constructor used for deserialization.
		/// </summary>
		public LNBSettings()
		{
		}

		/// <summary>
		/// Initialize instance.
		/// </summary>
		/// <param name="uLOF1">Low frequency.</param>
		/// <param name="uLOF2">High frequency. </param>
		/// <param name="uSwitch">Switch frequency. </param>
		/// <param name="bPower">Use power.</param>
		public LNBSettings(uint uLOF1, uint uLOF2, uint uSwitch, bool bPower)
		{
			// Load all
			LOF1 = uLOF1;
			LOF2 = uLOF2;
			Switch = uSwitch;
			UsePower = bPower;
		}

		/// <summary>
		/// Initialize from other settings.
		/// </summary>
		/// <param name="pLNB">Data will be copied to our fields.</param>
		public LNBSettings(LNBSettings pLNB)
		{
			// Load all
			LOF1 = pLNB.LOF1;
			LOF2 = pLNB.LOF2;
			Switch = pLNB.Switch;
			UsePower = pLNB.UsePower;
		}

		/// <summary>
		/// Check parameter against <see cref="Switch"/>.
		/// </summary>
		/// <param name="uFrequency">Some frequency.</param>
		/// <returns>Success if parameter is not less than switch frequency.</returns>
		public bool Use22kHz(uint uFrequency)
		{
			// Check against switch
			return (uFrequency >= Switch);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			// Construct
			return LOF1.GetHashCode() ^ LOF2.GetHashCode() ^ Switch.GetHashCode() ^ UsePower.GetHashCode();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			// Change type
			LNBSettings settings = obj as LNBSettings;

			// Test
			if (null == settings) return false;

			// Full compare
			return (LOF1 == settings.LOF1) && (LOF2 == settings.LOF2) && (Switch == settings.Switch) && (UsePower == settings.UsePower);
		}
	}
}
