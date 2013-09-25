using System;

namespace JMS.TechnoTrend
{
	/// <summary>
	/// Capabilities of <see cref="JMS.TechnoTrend.MFCWrapper.DVBFrontend"/>.
	/// </summary>
	[Flags] public enum FrontendCapabilities
	{
		/// <summary>
		/// Can auto detect error correction used.
		/// </summary>
		VrAuto = 0x000001,

		/// <summary>
		/// Can auto detect spectral inversion.
		/// </summary>
		SiAuto = 0x000010,

		/// <summary>
		/// Can do channel scan.
		/// </summary>
		ChScan = 0x000100,

		/// <summary>
		/// Can auto detect bandwidth.
		/// </summary>
		BwAuto = 0x001000,

		/// <summary>
		/// Has antenna power supply.
		/// </summary>
		AntPower = 0x010000,

		/// <summary>
		/// Supports DVB-S2 receiption.
		/// </summary>
		DVBS2 = 0x100000
	}
}
