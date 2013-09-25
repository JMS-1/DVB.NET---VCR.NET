using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
	/// <summary>
	/// A stream identifier <see cref="Descriptor"/>.
	/// </summary>
	/// <remarks>
	/// For details please refer to the original documentation,
	/// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
	/// </remarks>
	public class StreamIdentifier: Descriptor
	{
        /// <summary>
        /// The component identification.
        /// </summary>
        public readonly byte ComponentTag;

		/// <summary>
		/// 
		/// </summary>
		public StreamIdentifier(byte componentID)
			: base(DescriptorTags.StreamIdentifier)
		{
			// Remember
			ComponentTag = componentID;
		}

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
        public StreamIdentifier(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
		{
			// Validate size
			if ( 1 != length ) return;

            // Attach to data
            Section section = container.Section;

            // Load
            ComponentTag = section[offset];

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
			return (DescriptorTags.StreamIdentifier == (DescriptorTags)tag);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="buffer"></param>
		protected override void CreatePayload(TableConstructor buffer)
		{
			// Add
			buffer.Add(ComponentTag);
		}
	}
}
