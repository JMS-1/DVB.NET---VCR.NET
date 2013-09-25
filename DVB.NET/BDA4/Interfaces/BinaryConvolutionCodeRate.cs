using System;


namespace JMS.DVB.DeviceAccess.Interfaces
{
	/// <summary>
	/// Convolution code rates as used in DirectShow.
	/// </summary>
    public enum BinaryConvolutionCodeRate
	{
		/// <summary>
		/// Not defined.
		/// </summary>
		NotDefined = 0,

		/// <summary>
		/// 1/2.
		/// </summary>
		Rate1_2 = 1,

		/// <summary>
		/// 2/3.
		/// </summary>
		Rate2_3 = 2,

		/// <summary>
		/// 3/4.
		/// </summary>
		Rate3_4 = 3,

		/// <summary>
		/// 3/5.
		/// </summary>
		Rate3_5 = 4,

		/// <summary>
		/// 4/5.
		/// </summary>
		Rate4_5 = 5,

		/// <summary>
		/// 5/6.
		/// </summary>
		Rate5_6 = 6,

		/// <summary>
		/// 5/11.
		/// </summary>
		Rate5_11 = 7,

		/// <summary>
		/// 7/8.
		/// </summary>
		Rate7_8 = 8,

        /// <summary>
        /// 1/4.
        /// </summary>
        Rate1_4 = 9,

        /// <summary>
        /// 1/3.
        /// </summary>
        Rate1_3 = 10,

        /// <summary>
        /// 2/5.
        /// </summary>
        Rate2_5 = 11,

        /// <summary>
        /// 6/7.
        /// </summary>
        Rate6_7 = 12,

        /// <summary>
        /// 8/9.
        /// </summary>
        Rate8_9 = 13,

        /// <summary>
        /// 9/10.
        /// </summary>
        Rate9_10 = 14,

		/// <summary>
		/// Use maximum rate possible.
		/// </summary>
		Maximum = 15,

		/// <summary>
		/// Parameter should be regarded as not set.
		/// </summary>
		NotSet = -1
	}
}
