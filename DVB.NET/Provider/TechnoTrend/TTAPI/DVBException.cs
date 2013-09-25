using System;

namespace JMS.TechnoTrend
{
	/// <summary>
	/// Generic <see cref="Exception"/> used for all API calls.
	/// </summary>
	[Serializable] public class DVBException : Exception
	{
		/// <summary>
		/// Error code from API or <i>None</i>.
		/// </summary>
		private DVBError m_Code = DVBError.None;

		/// <summary>
		/// General error - no <see cref="DVBError"/> available.
		/// </summary>
		/// <param name="sText">Some error string.</param>
		public DVBException(string sText) : base(sText)
		{
		}

		/// <summary>
		/// Error from API call.
		/// </summary>
		/// <param name="sText">Some error string.</param>
		/// <param name="eCode">Return value from the C++ method invocation.</param>
		public DVBException(string sText, DVBError eCode) : base(sText + " [Error: " + eCode.ToString() + "]")
		{
			// Remember
			m_Code = eCode;
		}

		/// <summary>
		/// Read <see cref="m_Code"/>.
		/// </summary>
		public DVBError ErrorCode
		{
			get
			{
				// Report
				return m_Code;
			}
		}

		/// <summary>
		/// If the code is not <see cref="DVBError">None</see> a new
		/// <see cref="DVBException"/> is created.
		/// </summary>
		/// <param name="eCode">Error code from some API call.</param>
		/// <param name="sMessage">Message to report.</param>
		/// <exception cref="DVBException">If the error code is set.</exception>
		public static void ThrowOnError(DVBError eCode, string sMessage)
		{
			// Check and throw
			if ( DVBError.None != eCode ) throw new DVBException(sMessage, eCode);
		}
	}
}
