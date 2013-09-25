using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Tables
{
	/// <summary>
	/// A <i>Event Information Table (EIT)</i> holding <see cref="EventEntry"/> instances
	/// for the <i>Electronic Program Guide (EPG)</i>.
	/// </summary>
	/// <remarks>
	/// Physically the table is represented as described in the following table. For details please
	/// refer to the related document, e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
	/// <list type="table">
	/// <listheader><term>Byte/Bit(s)</term><description>Value/Meaning</description></listheader>
	/// <item><term>0/7-0</term><description>Bits 8 to 15 of the <see cref="ServiceIdentifier"/></description></item>
	/// <item><term>1/7-0</term><description>Bits 0 to 7 of the <see cref="ServiceIdentifier"/></description></item>
	/// <item><term>2/7-6</term><description>Reserved</description></item>
	/// <item><term>2/5-1</term><description><see cref="Version"/> Number</description></item>
    /// <item><term>2/0</term><description>Current/Next Indicator, which is stored in <see cref="Table.IsCurrent"/></description></item>
    /// <item><term>3/7-0</term><description><see cref="Table.SectionNumber"/></description></item>
    /// <item><term>4/7-0</term><description><see cref="Table.LastSectionNumber"/></description></item>
	/// <item><term>5/7-0</term><description>Bits 8 to 15 of <see cref="TransportStreamIdentifier"/></description></item>
	/// <item><term>6/7-0</term><description>Bits 0 to 7 of <see cref="TransportStreamIdentifier"/></description></item>
	/// <item><term>7/7-0</term><description>Bits 8 to 15 of <see cref="OriginalNetworkIdentifier"/></description></item>
	/// <item><term>8/7-0</term><description>Bits 0 to 7 of <see cref="OriginalNetworkIdentifier"/></description></item>
	/// <item><term>9/7-0</term><description><see cref="SegmentLastSectionNumber"/></description></item>
	/// <item><term>10/7-0</term><description><see cref="LastTableIdentifier"/></description></item>
	/// </list>
	/// The rest of the <see cref="Section"/> is filled with <see cref="EventEntry"/> instances.
	/// </remarks>
	public class EIT : Table
	{
		/// <summary>
		/// Any airing event related to this <see cref="Table"/>.
		/// </summary>
		public readonly EventEntry[] Entries;

		/// <summary>
		/// The original identifier of the network this <see cref="Table"/> refers to.
		/// </summary>
		private ushort m_OriginalNetworkIdentifier;

		/// <summary>
		/// The transport stream identifier this <see cref="Table"/> refers to.
		/// </summary>
		private ushort m_TransportStreamIdentifier;

		/// <summary>
		/// The last section number for split tables.
		/// </summary>
		public readonly byte SegmentLastSectionNumber;

		/// <summary>
		/// The last table identifier for split tables.
		/// </summary>
		public readonly byte LastTableIdentifier;

		/// <summary>
		/// The service identifier for the program this <see cref="Table "/> refers to.
		/// </summary>
		private ushort m_ServiceIdentifier;

		/// <summary>
		/// Set for table identifier from <i>0x4e</i> to <i>0x6f</i>.
		/// </summary>
		/// <param name="tableIdentifier">The table identifier for which this <see cref="Type"/>
		/// should report its responsibility.</param>
		/// <returns>Set for table identifier from <i>0x4e</i> to <i>0x6f</i>.</returns>
		public static bool IsHandlerFor(byte tableIdentifier)
		{
			// Check all
			return ((tableIdentifier >= 0x4e) && (tableIdentifier <= 0x6f));
		}

		/// <summary>
		/// Load the fields of this instance from the <see cref="Section"/> raw data.
		/// If any validation is successful <see cref="Table.IsValid"/> will be set.
		/// </summary>
		/// <remarks>
		/// <see cref="Entries"/> will be <i>null</i> and not an empty <see cref="Array"/>
		/// if <see cref="Table.IsValid"/> ends up unset.
		/// </remarks>
		/// <param name="section">The related <see cref="Section"/>.</param>
		public EIT(Section section)
			: base(section)
		{
			// Get the size of the event entry region
			int offset = 11, length = section.Length - 3 - offset - 4;

			// Minimum size
			if (length < 0) return;

			// Direct
			SegmentLastSectionNumber = section[9];
			LastTableIdentifier = section[10];

			// Construct
			m_TransportStreamIdentifier = Tools.MergeBytesToWord(section[6], section[5]);
			m_OriginalNetworkIdentifier = Tools.MergeBytesToWord(section[8], section[7]);
			m_ServiceIdentifier = Tools.MergeBytesToWord(section[1], section[0]);

			// Create helper
			List<EventEntry> entries = new List<EventEntry>();

			// Process
			for (EventEntry entry; null != (entry = EventEntry.Create(this, offset, length)); )
				if (entry.IsValid)
				{
					// Remember
					entries.Add(entry);

					// Correct
					offset += entry.Length;
					length -= entry.Length;
				}

			// Usefull
			m_IsValid = (0 == length);

			// Convert
			if (m_IsValid) Entries = entries.ToArray();
		}

		/// <summary>
		/// Get or set the service identifier for the program this <see cref="Table "/> refers to.
		/// </summary>
		public ushort ServiceIdentifier
		{
			get
			{
				// Report
				return m_ServiceIdentifier;
			}
			set
			{
				// Store back
				Section[1] = (byte)(value & 0xff);
				Section[0] = (byte)(value >> 8);

				// Change
				m_ServiceIdentifier = value;
			}
		}

		/// <summary>
		/// Get or set the original identifier of the network this <see cref="Table"/> refers to.
		/// </summary>
		public ushort OriginalNetworkIdentifier
		{
			get
			{
				// Report
				return m_OriginalNetworkIdentifier;
			}
			set
			{
				// Store back
				Section[8] = (byte)(value & 0xff);
				Section[7] = (byte)(value >> 8);

				// Update
				m_OriginalNetworkIdentifier = value;
			}
		}

		/// <summary>
		/// Get or set the transport stream identifier this <see cref="Table"/> refers to.
		/// </summary>
		public ushort TransportStreamIdentifier
		{
			get
			{
				// Report
				return m_TransportStreamIdentifier;
			}
			set
			{
				// Store back
				Section[6] = (byte)(value & 0xff);
				Section[5] = (byte)(value >> 8);

				// Remember
				m_TransportStreamIdentifier = value;
			}
		}
	}
}
