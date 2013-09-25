using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// A ISO language descriptor for an audio stream.
    /// </summary>
	public class ISOLanguage: Descriptor
	{
        /// <summary>
        /// An individual language item.
        /// </summary>
		public readonly List<LanguageItem> Languages = new List<LanguageItem>();

		/// <summary>
		/// 
		/// </summary>
		public ISOLanguage()
			: base(DescriptorTags.ISO639Language)
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
        public ISOLanguage(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
		{
			// Check minimum length
			if ( length < 0 ) return;

			// Attach to data
            Section section = container.Section;

            // Helper
            List<LanguageItem> items = new List<LanguageItem>();

            // Load
            while (length > 0)
            {
                // Create
                LanguageItem item = LanguageItem.Create(section, offset, length);

                // Done
                if (null == item) break;

                // Remember
                items.Add(item);

                // Correct 
                offset += item.Length;
                length -= item.Length;
            }

			// Test
            m_Valid = (0 == length);

            // Load
			if (m_Valid) Languages = items;
		}

        /// <summary>
        /// Check if this class is responsible for a given descriptor tag.
        /// </summary>
        /// <param name="tag">The tag to test for.</param>
        /// <returns>Set if this class can handle the payload for the given tag.</returns>
        public static bool IsHandlerFor(byte tag)
		{
			// Check it
            return (DescriptorTags.ISO639Language == (DescriptorTags)tag);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="buffer"></param>
		protected override void CreatePayload(TableConstructor buffer)
		{
			// Process all
			foreach (LanguageItem item in Languages) item.CreatePayload(buffer);
		}
	}
}
