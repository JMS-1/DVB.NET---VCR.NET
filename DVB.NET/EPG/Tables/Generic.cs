using System;

namespace JMS.DVB.EPG.Tables
{
	/// <summary>
	/// This class will be used by <see cref="Table.Create"/> substitue handler
	/// for unsupported table identifiers.
	/// </summary>
	/// <remarks>
	/// Since instances of this <see cref="Type"/> will always report to be invalid
	/// when <see cref="Table.IsValid"/> is called a client will never see any.
	/// </remarks>
	public class Generic: Table
	{
		/// <summary>
		/// Create the new instance.
		/// </summary>
		/// <param name="section">The corresponding <see cref="Section"/>.</param>
		public Generic(Section section) : base(section)
		{
		}
	}
}
