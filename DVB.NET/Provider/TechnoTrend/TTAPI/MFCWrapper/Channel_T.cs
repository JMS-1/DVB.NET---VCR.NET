using System;
using System.Runtime.InteropServices;


namespace JMS.TechnoTrend.MFCWrapper
{
	/// <summary>
	/// Structure used to set a terrestrial channel.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)] public struct Channel_T
	{
		/// <summary>
		/// Frequency to use.
		/// </summary>
		public UInt32 Frequency;
			
		/// <summary>
		/// Inversion to use.
		/// </summary>
		public SpectrumInversion Inversion;
			
		/// <summary>
		/// [Don't know]
		/// </summary>
		public bool Scan;

		/// <summary>
		/// [Don't know]
		/// </summary>
		public BandwidthType Bandwidth;

		/// <summary>
		/// Must have same size as <see cref="Channel_S"/>.
		/// </summary>
		private UInt32 _pad_1;

		/// <summary>
		/// Must have same size as <see cref="Channel_S"/>.
		/// </summary>
		private UInt32 _pad_2;

		/// <summary>
		/// Must have same size as <see cref="Channel_S"/>.
		/// </summary>
		private UInt32 _pad_3;
	}
}
