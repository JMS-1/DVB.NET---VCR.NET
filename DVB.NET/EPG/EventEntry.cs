using System;

namespace JMS.DVB.EPG
{
	/// <summary>
	/// This class describes a single event in a <see cref="Tables.EIT"/> table.
	/// </summary>
	/// <remarks>
	/// The following is the physical layout of the raw data as received in the
	/// DVB stream. For details please refer to the original documentation,
	/// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
	/// <list type="table">
	/// <listheader><term>Byte/Bit(s)</term><description>Value/Meaning</description></listheader>
	/// <item><term>0/7-0</term><description>Bits 8 to 15 of <see cref="EventIdentifier"/></description></item>
	/// <item><term>1/7-0</term><description>Bits 0 to 7 of <see cref="EventIdentifier"/></description></item>
	/// <item><term>2/7-0</term><description>Bits 32 to 39 of <see cref="StartTime"/></description></item>
	/// <item><term>3/7-0</term><description>Bits 24 to 31 of <see cref="StartTime"/></description></item>
	/// <item><term>4/7-0</term><description>Bits 16 to 23 of <see cref="StartTime"/></description></item>
	/// <item><term>5/7-0</term><description>Bits 8 to 15 of <see cref="StartTime"/></description></item>
	/// <item><term>6/7-0</term><description>Bits 0 to 7 of <see cref="StartTime"/></description></item>
	/// <item><term>7/7-0</term><description>Bits 16 to 23 of <see cref="Duration"/></description></item>
	/// <item><term>8/7-0</term><description>Bits 8 to 15 of <see cref="Duration"/></description></item>
	/// <item><term>9/7-0</term><description>Bits 0 to 7 of <see cref="Duration"/></description></item>
	/// <item><term>10/7-5</term><description>Running <see cref="Status"/></description></item>
	/// <item><term>10/4</term><description><see cref="FreeCA"/> Mode</description></item>
	/// <item><term>10/3-0</term><description>Bits 8 to 11 of Loop Length</description></item>
	/// <item><term>11/7-0</term><description>Bits 0 to 7 of Loop Length</description></item>
	/// </list>
	/// Then a number of <see cref="Descriptor"/> instances follow filling up the
	/// indicated loop length number of bytes.
	/// </remarks>
	public class EventEntry: EntryBase
	{
		/// <summary>
		/// Some unique identifier for this event.
		/// </summary>
		public readonly ushort EventIdentifier;

		/// <summary>
		/// Set if the event data is consistent.
		/// </summary>
		public readonly bool IsValid = false;

		/// <summary>
		/// The start time of the event in GMT/UTC notation.
		/// <seealso cref="Tools.DecodeTime"/>
		/// </summary>
		private DateTime? m_StartTime = null;

		/// <summary>
		/// The status of this event.
		/// </summary>
		public readonly EventStatus Status;

		/// <summary>
		/// The duration for this event.
		/// <seealso cref="Tools.DecodeDuration"/>
		/// </summary>
		private TimeSpan? m_Duration = null;

		/// <summary>
		/// Report if this event is encrypted or not.
		/// </summary>
		public readonly bool FreeCA;

		/// <summary>
		/// The total length of the event in bytes.
		/// </summary>
		public readonly int Length;

		/// <summary>
		/// Component to delay load descriptors.
		/// </summary>
		private Descriptors.DescriptorLoader m_Loader = null;

		/// <summary>
		/// Initial offset to data.
		/// </summary>
		private int m_Offset;

		/// <summary>
		/// Create a new event instance.
		/// </summary>
		/// <param name="table">The related <see cref="Tables.EIT"/> table.</param>
		/// <param name="offset">The first byte of this event in the <see cref="EPG.Table.Section"/>
		/// for the related <see cref="Table"/>.</param>
		/// <param name="length">The maximum number of bytes available. If this number
		/// is greater than the <see cref="Length"/> of this event another event will
		/// follow in the same table.</param>
		internal EventEntry(Table table, int offset, int length) : base(table)
		{
			// Access section
			Section section = Section;

			// Helper
			byte flags = section[offset + 10];

			// Decode
			int highLoop = flags&0xf;
			int lowLoop = section[offset + 11];

			// Construct
			EventIdentifier = Tools.MergeBytesToWord(section[offset + 1], section[offset + 0]);

			// Remember offset to delay load times
			m_Offset = offset;

			// Direct
			FreeCA = (0 != (flags&0x10));
			Status = (EventStatus)((flags>>5)&0x7);

			// Number of descriptors
			int loop = lowLoop + 256 * highLoop;
			
			// Caluclate the total length
			Length = 12 + loop;

			// Verify
			if ( Length > length ) return;

			// Install delay load of descriptors
			m_Loader = new Descriptors.DescriptorLoader(this, offset + 12, loop);

			// Can use it
			IsValid = true;
		}

		/// <summary>
		/// Create a new event instance.
		/// </summary>
		/// <param name="table">The related <see cref="Tables.EIT"/> table.</param>
		/// <param name="offset">The first byte of this event in the <see cref="EPG.Table.Section"/>
		/// for the related <see cref="Table"/>.</param>
		/// <param name="length">The maximum number of bytes available. If this number
		/// is greater than the <see cref="Length"/> of this event another event will
		/// follow in the same table.</param>
		/// <returns>A new event instance or <i>null</i> if there are less than
		/// 12 bytes available.</returns>
		static internal EventEntry Create(Table table, int offset, int length)
		{
			// Validate
			if ( length < 12 ) return null;

			// Create
			return new EventEntry(table, offset, length);
		}

		/// <summary>
		/// The <see cref="Descriptor"/> instances related to this event.
		/// </summary>
		/// <remarks>
		/// Please refer to the original documentation to find out which descriptor
		/// type is allowed in a <see cref="Tables.EIT"/> table.
		/// </remarks>
		public Descriptor[] Descriptors
		{
			get
			{
				// Report
				return (null == m_Loader) ? new Descriptor[0] : m_Loader.Descriptors;
			}
		}

		/// <summary>
		/// The start time of the event in GMT/UTC notation.
		/// <seealso cref="Tools.DecodeTime"/>
		/// </summary>
		public DateTime StartTime
		{
			get
			{
				// Load once
				if (!m_StartTime.HasValue) m_StartTime = Tools.DecodeTime(Section, m_Offset + 2);

				// Report
				return m_StartTime.Value;
			}
		}


		/// <summary>
		/// The duration for this event.
		/// <seealso cref="Tools.DecodeDuration"/>
		/// </summary>
		public TimeSpan Duration
		{
			get
			{
				// Load once
				if (!m_Duration.HasValue) m_Duration = Tools.DecodeDuration(Section, m_Offset + 7);

				// Report
				return m_Duration.Value;
			}
		}
	}
}
