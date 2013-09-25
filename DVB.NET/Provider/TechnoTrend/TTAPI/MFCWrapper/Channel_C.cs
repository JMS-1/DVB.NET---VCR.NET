using System;
using JMS.DVB;
using JMS.DVB.Cable;
using System.Runtime.InteropServices;

namespace JMS.TechnoTrend.MFCWrapper
{
	/// <summary>
	/// Structure used to set a cable channel.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)] public struct Channel_C
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
		/// Related symbol rate.
		/// </summary>
		public UInt32 SymbolRate;
			
		/// <summary>
		/// [Don't know]
		/// </summary>
		public Qam Qam;

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
	}
}
