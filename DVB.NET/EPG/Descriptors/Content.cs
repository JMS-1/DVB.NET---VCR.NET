using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// A content <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
    /// </remarks>
    public class Content : Descriptor
    {
        /// <summary>
        /// List of categories for the content.
        /// </summary>
        public readonly int[] Categories;

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
        public Content( IDescriptorContainer container, int offset, int length )
            : base( container, offset, length )
        {
            // Attach to data
            Section section = container.Section;

            // Collector
            List<int> all = new List<int>();

            // Process content
            for (; length > 1; length -= 2)
            {
                // Read all
                int content = section[offset++];
                int user = section[offset++];

                // Remember
                all.Add( content );
            }

            // Remember
            Categories = all.ToArray();

            // Test
            m_Valid = (0 == length);
        }

        /// <summary>
        /// Check if this class is responsible for a given descriptor tag.
        /// </summary>
        /// <param name="tag">The tag to test for.</param>
        /// <returns>Set if this class can handle the payload for the given tag.</returns>
        public static bool IsHandlerFor( byte tag )
        {
            // Check it
            return (DescriptorTags.Content == (DescriptorTags) tag);
        }
    }
}
