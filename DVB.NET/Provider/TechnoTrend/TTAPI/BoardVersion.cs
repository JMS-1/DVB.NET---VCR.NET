using System;

namespace JMS.TechnoTrend
{
	/// <summary>
	/// Information on the current board version.
	/// <seealso cref="JMS.TechnoTrend.MFCWrapper.DVBBoardControl"/>
	/// </summary>
	public class BoardVersion
	{
		/// <summary>
		/// Firmware version.
		/// </summary>
		public readonly uint Firmware = 0;

		/// <summary>
		/// Version of Runtime Support Library.
		/// </summary>
		public readonly uint RTSLibrary = 0;

		/// <summary>
		/// Version of video microcode.
		/// </summary>
		public readonly uint VideoDecoder = 0;

		/// <summary>
		/// Compilation date.
		/// </summary>
		public readonly string Date = null;

		/// <summary>
		/// Compilation time.
		/// </summary>
		public readonly string Time = null;

		/// <summary>
		/// Create a new instance.
		/// </summary>
		/// <param name="uiFirm">Firmware version.</param>
		/// <param name="uiLib">Version of Runtime Support Library.</param>
		/// <param name="uiVideo">Version of video microcode.</param>
		/// <param name="sDate">Compilation date.</param>
		/// <param name="sTime">Compilation date.</param>
		internal BoardVersion(uint uiFirm, uint uiLib, uint uiVideo, string sDate, string sTime)
		{
			// Remember all
			Firmware = uiFirm;
			RTSLibrary = uiLib;
			VideoDecoder = uiVideo;
			Date = sDate;
			Time = sTime;
		}
	}
}
