using System;

namespace JMS.DVB.EPG.Descriptors
{
	/// <summary>
	/// A component <see cref="Descriptor"/>.
	/// </summary>
	/// <remarks>
	/// For details please refer to the original documentation,
	/// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
	/// </remarks>
	public class Component: Descriptor
	{
		/// <summary>
		/// Type of the component described.
		/// </summary>
		public readonly byte ComponentType;

		/// <summary>
		/// Tag for the component.
		/// </summary>
		public readonly byte ComponentTag;

		/// <summary>
		/// Language used in the component data.
		/// </summary>
		public readonly string Language;

		/// <summary>
		/// Content type.
		/// </summary>
		public readonly int Content;
	
		/// <summary>
		/// Some description of the component.
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
        public Component(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
		{
			// Check minimum length
			if ( length < 6 ) return;

			// Attach to data
            Section section = container.Section;

			// Read parts
			Content = section[offset + 0]&0xf;
			ComponentType = section[offset + 1];
			ComponentTag = section[offset + 2];

			// Load strings
			Language = section.ReadString(offset + 3, 3);
			Text = section.ReadEncodedString(offset + 6, length - 6);

			// Test
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
			return (DescriptorTags.Component == (DescriptorTags)tag);
		}
	}
}
