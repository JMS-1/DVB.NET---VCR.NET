using System;

namespace JMS.DVB.EPG.Descriptors
{
	/// <summary>
	/// An extended event <see cref="Descriptor"/>.
	/// </summary>
	/// <remarks>
	/// For details please refer to the original documentation,
	/// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
	/// </remarks>
	public class ExtendedEvent: Descriptor
	{
		/// <summary>
		/// Last descriptor in this group.
		/// </summary>
		public readonly int LastDescriptorNumber;

		/// <summary>
		/// Current descriptor in the related group.
		/// </summary>
		public readonly int DescriptorNumber;

		/// <summary>
		/// Language for the related event.
		/// </summary>
		public readonly string Language;

		/// <summary>
		/// Name of the event.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// Partial description for the related event.
		/// </summary>
		public readonly string Text;

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
        public ExtendedEvent(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
		{
			// Validate size
			if ( length < 6 ) return;

			// Attach to data
            Section section = container.Section;

			// Read statics
			byte number = section[offset + 0];

			// Decode
			DescriptorNumber = (number>>4)&0xf;
			LastDescriptorNumber = number&0xf;

			// Load length of items
			int itemLength = section[offset + 4];

			// Verify
			if ( length < (6 + itemLength) ) return;

			// Load length of text
			int textLength = section[offset + 5 + itemLength];

			// Verify
			if ( length != (6 + itemLength + textLength) ) return;

			// Read strings
			Language = section.ReadString(offset + 1, 3);
			Text = section.ReadEncodedString(offset + 6 + itemLength , textLength);

			// Items are currently not processed so we are done
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
			return (DescriptorTags.ExtendedEvent == (DescriptorTags)tag);
		}
	}
}
