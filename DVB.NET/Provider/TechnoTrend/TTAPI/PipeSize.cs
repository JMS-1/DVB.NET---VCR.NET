using System;

namespace JMS.TechnoTrend
{
	/// <summary>
	/// Packet size for a <see cref="FilterType">Piping</see> <see cref="MFCWrapper.DVBRawFilter"/>.
	/// </summary>
	public enum PipeSize
	{
		/// <summary>
		/// Default is 1 KByte.
		/// </summary>
		None,

		/// <summary>
		/// Packet (184 Bytes).
		/// </summary>
		Packet,

		/// <summary>
		/// Two packets (368 Bytes).
		/// </summary>
		Packets,

		/// <summary>
		/// 1 KBytes.
		/// </summary>
		One,

		/// <summary>
		/// 2 KBytes.
		/// </summary>
		Two,

		/// <summary>
		/// 4 KBytes.
		/// </summary>
		Four,

		/// <summary>
		/// 8 KBytes.
		/// </summary>
		Eight,

		/// <summary>
		/// 16 KBytes.
		/// </summary>
		Sixteen,

		/// <summary>
		/// 32 KBytes.
		/// </summary>
		ThirtyTwo
	}
}
