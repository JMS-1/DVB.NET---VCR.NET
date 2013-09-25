using System;

namespace JMS.DVB.EPG.Descriptors
{
	/// <summary>
	/// A short event <see cref="Descriptor"/>.
	/// </summary>
	/// <remarks>
	/// For details please refer to the original documentation,
	/// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
	/// </remarks>
	public class ShortEvent: Descriptor
	{
		/// <summary>
		/// The language for the event.
		/// </summary>
		public readonly string Language = null;

		/// <summary>
		/// The name of the event.
		/// </summary>
		public readonly string Name = null;

		/// <summary>
		/// Some short description for the event.
		/// </summary>
		public readonly string Text = null;

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
        public ShortEvent(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
		{
			// Validate size
			if ( length < 5 ) return;

			// Attach to data
            Section section = container.Section;

			// Load length of event name
			int nameLength = section[offset + 3];

			// Verify
			if ( length < (5 + nameLength) ) return;

			// Load length of text
			int textLength = section[offset + 4 + nameLength];

			// Verify
			if ( length != (5 + nameLength + textLength) ) return;

			// Read strings
			Language = section.ReadString(offset + 0, 3);
			Name = section.ReadEncodedString(offset + 4, nameLength);
			Text = section.ReadEncodedString(offset + 5 + nameLength , textLength);

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
			return (DescriptorTags.ShortEvent == (DescriptorTags)tag);
		}
	}
}
