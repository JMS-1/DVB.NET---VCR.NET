using System;
using System.Collections;

namespace JMS.DVB.EPG.Tables
{
	/// <summary>
	/// The class is used to describe a <i>Service Description Table</i> which can
	/// be found on PID <i>0x11</i> in a transport stream.
	/// </summary>
	public class SDT: Table
	{
		/// <summary>
		/// The original identifier of the network this <see cref="Table"/> refers to.
		/// </summary>
		public readonly ushort OriginalNetworkIdentifier;

		/// <summary>
		/// The transport stream identifier this <see cref="Table"/> refers to.
		/// </summary>
		public readonly ushort TransportStreamIdentifier;

		/// <summary>
		/// Services included in thie <see cref="Table"/>.
		/// </summary>
		public readonly ServiceEntry[] Services = {};

		/// <summary>
		/// Set for table identifiers <i>0x42</i> and <i>0x46</i>.
		/// </summary>
		/// <param name="tableIdentifier">The table identifier for which this <see cref="Type"/>
		/// should report its responsibility.</param>
		/// <returns>Set for table identifier <i>0x42</i> and <i>0x46</i>.</returns>
		public static bool IsHandlerFor(byte tableIdentifier)
		{
			// Check all
			return ((0x42 == tableIdentifier) || (0x46 == tableIdentifier));
		}

		/// <summary>
		/// Create a new <i>Service Description Table</i> instance.
		/// </summary>
		/// <param name="section">The section which is currently parsed.</param>
		public SDT(Section section) : base(section)
		{
			// Get the size of the service entry region
			int offset = 8, length = section.Length - 3 - offset - 4;

			// Minimum size
			if ( length < 0 ) return;

			// Construct
			TransportStreamIdentifier = Tools.MergeBytesToWord(section[1], section[0]);
			OriginalNetworkIdentifier = Tools.MergeBytesToWord(section[6], section[5]);

			// Create helper
			ArrayList services = new ArrayList();

			// Process
			for ( ServiceEntry entry ; null != (entry = ServiceEntry.Create(this, offset, length)) ; )
				if ( entry.IsValid )
				{
					// Remember
					services.Add(entry);

					// Correct
					offset += entry.Length;
					length -= entry.Length;
				}

			// Usefull
			m_IsValid = (0 == length);

			// Convert
			if ( m_IsValid ) Services = (ServiceEntry[])services.ToArray(typeof(ServiceEntry));
		}
	}
}
