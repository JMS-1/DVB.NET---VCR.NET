using System;
using System.Runtime.InteropServices;


namespace JMS.TechnoTrend.MFCWrapper
{
	/// <summary>
	/// Lock information with device type dependend bits in flags word.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 4)] public struct LockState
	{
		/// <summary>
		/// The type of the frontend.
		/// </summary>
		public FrontendType FrontendType;

		/// <summary>
		/// Set if the channel is locked.
		/// </summary>
		public bool FrontendLocked;

		/// <summary>
		/// Bits depend on the <see cref="FrontendType"/>.
		/// </summary>
		public UInt32 Flags;
	}
}
