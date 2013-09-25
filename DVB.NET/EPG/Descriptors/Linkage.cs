using System;

namespace JMS.DVB.EPG.Descriptors
{
	/// <summary>
	/// A linkage <see cref="Descriptor"/>.
	/// </summary>
	/// <remarks>
	/// For details please refer to the original documentation,
	/// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
	/// </remarks>
	public class Linkage: Descriptor
	{
		/// <summary>
		/// The referenced original network identifier.
		/// </summary>
		public readonly ushort OriginalNetworkIdentifier;

		/// <summary>
		/// The references transport identifier.
		/// </summary>
		public readonly ushort TransportStreamIdentifier;

		/// <summary>
		/// The initial service identifier - not always set.
		/// </summary>
		public readonly ushort InitialServiceIdentifier;

		/// <summary>
		/// The referenced service identifier.
		/// </summary>
		public readonly ushort ServiceIdentifier;

		/// <summary>
		/// Some private data.
		/// </summary>
		public readonly byte[] PrivateData;

		/// <summary>
		/// The handover type - not always set.
		/// </summary>
		public readonly byte HandOverType;

		/// <summary>
		/// The network identifier - not always set.
		/// </summary>
		public readonly ushort NetworkID;

		/// <summary>
		/// The origin type - not always set.
		/// </summary>
		public readonly bool OriginType;

		/// <summary>
		/// The type of this linkage.
		/// </summary>
		public readonly byte LinkType;

        /// <summary>
        /// Create a new descriptor instance.
        /// </summary>
        /// <remarks>
        /// <see cref="Descriptor.IsValid"/> will only be set if the payload is 
        /// consistent.
        /// </remarks>
        /// <param name="container">The related container instance.</param>
        /// <param name="offset">First byte of the descriptor data - the first byte after the tag.</param>
        /// <param name="length">Number of payload bytes for this descriptor.</param>
        public Linkage(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
		{
			// Validate size
			if ( length < 7 ) return;

			// Attach to data
            Section section = container.Section;

			// Read head
			TransportStreamIdentifier = Tools.MergeBytesToWord(section[offset + 1], section[offset + 0]);
			OriginalNetworkIdentifier = Tools.MergeBytesToWord(section[offset + 3], section[offset + 2]);
			ServiceIdentifier = Tools.MergeBytesToWord(section[offset + 5], section[offset + 4]);

			// Read type
			LinkType = section[offset + 6];

			// Correct
			offset += 7;
			length -= 7;

			// Depends
			if ( 8 == LinkType )
			{
				// Check mode
				if ( length < 1 ) return;

				// Raw read
				byte temp = section[offset + 0];

				// Decode
				HandOverType = (byte)(temp>>4);
				OriginType = (0 != (temp&1));

				// Special
				if ( (1 == HandOverType) || (2 == HandOverType) || (3 == HandOverType) )
				{
					// Check mode
					if ( length < 3 ) return;

					// Read
					NetworkID = Tools.MergeBytesToWord(section[offset + 2], section[offset + 1]);

					// Move ahead
					offset += 2;
					length -= 2;
				}

				// Special
				if ( !OriginType )
				{
					// Check mode
					if ( length < 3 ) return;

					// Read
					InitialServiceIdentifier = Tools.MergeBytesToWord(section[offset + 2], section[offset + 1]);

					// Move ahead
					offset += 2;
					length -= 2;
				}

				// Adjust
				++offset;
				--length;
			}

			// The rest is private data
			PrivateData = section.ReadBytes(offset, length);

			// We are valid
			m_Valid = true;
		}

        /// <summary>
        /// Check if this class is responsible for a given descriptor tag.
        /// </summary>
        /// <param name="tag">The tag to test for.</param>
        /// <returns>Set if this class can handle the payload for the given tag.</returns>
        public static bool IsHandlerFor(byte tag)
		{
			// Check it
			return (DescriptorTags.Linkage == (DescriptorTags)tag);
		}
	}
}
