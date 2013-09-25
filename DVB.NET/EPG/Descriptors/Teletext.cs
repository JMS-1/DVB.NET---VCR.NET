using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// A teletext descriptor instance.
    /// </summary>
	public class Teletext: Descriptor
	{
        /// <summary>
        /// Informations on dedicated pages.
        /// </summary>
		public readonly List<TeletextItem> Items = new List<TeletextItem>();

		/// <summary>
		/// 
		/// </summary>
		public Teletext()
			: base(DescriptorTags.Teletext)
		{
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
        public Teletext(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
		{
			// Check minimum length
			if ( length < 0 ) return;

            // Attach to the section
            Section section = container.Section;

            // Helper
            List<TeletextItem> items = new List<TeletextItem>();

            // Load
            while (length > 0)
            {
                // Try load
                TeletextItem item = TeletextItem.Create(section, offset, length);

                // Done
                if (null == item) break;

                // Add
                items.Add(item);

                // Count
                offset += item.Length;
                length -= item.Length;
            }

			// Store
            m_Valid = (0 == length);

            // Use
            if (m_Valid) Items = items;
		}

        /// <summary>
        /// Check if this class is responsible for a given descriptor tag.
        /// </summary>
        /// <param name="tag">The tag to test for.</param>
        /// <returns>Set if this class can handle the payload for the given tag.</returns>
		public static bool IsHandlerFor(byte tag)
		{
			// Check it
            return (DescriptorTags.Teletext == (DescriptorTags)tag);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="buffer"></param>
		protected override void CreatePayload(TableConstructor buffer)
		{
			// Forward
			foreach (TeletextItem item in Items) item.CreatePayload(buffer);
		}
	}
}