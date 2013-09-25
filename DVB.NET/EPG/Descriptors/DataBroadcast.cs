using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// A data broadcast descriptor.
    /// </summary>
	public class DataBroadcast: Descriptor
	{
        /// <summary>
        /// The data broadcast identifier.
        /// </summary>
        public readonly ushort DataBroadcastID;

        /// <summary>
        /// The tag of the component.
        /// </summary>
        public readonly byte ComponentTag;

        /// <summary>
        /// The selector bytes.
        /// </summary>
        public readonly byte[] Selector;

        /// <summary>
        /// The ISO language code for this broadcast.
        /// </summary>
        public readonly string ISOLanguage;

        /// <summary>
        /// The text description for the broadcast.
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
        public DataBroadcast(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
		{
			// Check minimum length
			if ( length < 8 ) return;

            // Attach to the section
            Section section = container.Section;

            // Load statics
            DataBroadcastID = Tools.MergeBytesToWord(section[offset + 1], section[offset + 0]);
            ComponentTag = section[offset + 2];

            // Read the length of the selector
            int selector = section[offset + 3];

            // Correct
            length -= 4 + selector;

            // Test
            if (length < 1) return;

            // Load 
            Selector = section.ReadBytes(offset + 4, selector);

            // Correct
            offset += 4 + selector;

            // Read the ISO language
            ISOLanguage = section.ReadString(offset, 3);

            // Read the length
            int text = section[offset + 3];

            // Correct
            length -= 4 + text;

            // Test
            if (0 != length) return;

            // Load
            Text = section.ReadEncodedString(offset + 4, text);

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
            return (DescriptorTags.DataBroadcast == (DescriptorTags)tag);
		}
	}
}
