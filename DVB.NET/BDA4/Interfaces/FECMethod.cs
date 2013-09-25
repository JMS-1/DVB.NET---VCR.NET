using System;


namespace JMS.DVB.DeviceAccess.Interfaces
{
	/// <summary>
	/// DirectShow FEC method to use.
	/// </summary>
    public enum FECMethod
	{
		/// <summary>
		/// Not defined.
		/// </summary>
		NotDefined = 0,

		/// <summary>
		/// Viterbi.
		/// </summary>
		Viterbi = 1,

		/// <summary>
		/// RS 208.
		/// </summary>
		RS204_188 = 2,

		/// <summary>
		/// Maximum possible.
		/// </summary>
		Maxmium = 3,

		/// <summary>
		/// Mark the related parameter as not set.
		/// </summary>
		NotSet = -1
	}
}
