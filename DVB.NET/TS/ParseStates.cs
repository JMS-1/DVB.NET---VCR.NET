using System;

namespace JMS.DVB.TS
{
	/// <summary>
	/// States of the PES parser in <see cref="StreamBase"/>.
	/// </summary>
	internal enum ParseStates
	{
		/// <summary>
		/// Awaiting the first zero of a PES start code.
		/// </summary>
		Synchronize,

		/// <summary>
		/// Awaiting the second zero of a PES start code.
		/// </summary>
		Found0,

		/// <summary>
		/// Awaiting the one of a PES start code.
		/// </summary>
		Found00,

		/// <summary>
		/// Awaiting the start code byte of a PES start code.
		/// </summary>
		Found001,

		/// <summary>
		/// Awaiting the length high byte of a PES start code.
		/// </summary>
		LenHigh,

		/// <summary>
		/// Awaiting the length low byte of a PES start code.
		/// </summary>
		LenLow,

		/// <summary>
		/// PES start code including length has been detected.
		/// </summary>
		Found
	}
}
