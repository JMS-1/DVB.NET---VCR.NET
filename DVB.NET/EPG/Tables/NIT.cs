using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Tables
{
	public class NIT : Table
	{
		public readonly bool ForThisStream;

		public readonly ushort NetworkIdentifier;

		public readonly Descriptor[] Descriptors;

		public readonly NetworkEntry[] NetworkEntries;

		public static bool IsHandlerFor(byte tableIdentifier)
		{
			// Check all
			return ((0x40 == tableIdentifier) || (0x41 == tableIdentifier));
		}

		public NIT(Section section)
			: base(section)
		{
			// Load length
			int length = section.Length - 7;

			// Check for minimum length required
			if (length < 7) return;

			// Load the length of the descriptors
			int deslen = Tools.MergeBytesToWord(section[6], section[5])&0x0fff;

			// Where the service area starts
			int svcoff = 7 + deslen;

			// Correct
			length -= svcoff;

			// Check for minimum length required
			if (length < 2) return;

			// Load the length of the service information
			int svclen = Tools.MergeBytesToWord(section[svcoff + 1], section[svcoff + 0])&0x0fff;

			// Correct
			length -= 2 + svclen;

			// Validate
			if (0 != length) return;

			// Load all descriptors
			Descriptors = Descriptor.Load(this, 7, deslen);

			// Load special
			NetworkIdentifier = Tools.MergeBytesToWord(section[1], section[0]);
			ForThisStream = (0x40 == section.TableIdentifier);

			// Result
			List<NetworkEntry> entries = new List<NetworkEntry>();

			// Fill
			for (svcoff += 2; svclen > 0; )
			{
				// Create next
				NetworkEntry entry = NetworkEntry.Create(this, svcoff, svclen);

				// Failed
				if ((null == entry) || !entry.IsValid) return;

				// Remember
				entries.Add(entry);

				// Adjust
				svcoff += entry.Length;
				svclen -= entry.Length;
			}

			// Use it
			NetworkEntries = entries.ToArray();

			// Done
			m_IsValid = true;
		}
	}
}
