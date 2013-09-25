using System;

namespace JMS.DVB.DirectShow.Interfaces
{
	/// <summary>
	/// How the VMR respects aspect ratios.
	/// </summary>
	internal enum AspectRatioModes
	{
		/// <summary>
		/// Provide any scale.
		/// </summary>
		None,

		/// <summary>
		/// Keep source scale and create a letter box as needed.
		/// </summary>
		LetterBox,
	}
}
