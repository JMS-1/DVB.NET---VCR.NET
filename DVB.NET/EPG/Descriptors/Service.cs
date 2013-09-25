using System;

namespace JMS.DVB.EPG.Descriptors
{
	/// <summary>
	/// A service <see cref="Descriptor"/>.
	/// </summary>
	/// <remarks>
	/// For details please refer to the original documentation,
	/// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
	/// </remarks>
	public class Service: Descriptor
	{
        /// <summary>
        /// The type of this service.
        /// </summary>
        public readonly ServiceTypes ServiceType;

        /// <summary>
        /// Name of the provider.
        /// </summary>
        public readonly string ProviderName;

        /// <summary>
        /// Name of the service.
        /// </summary>
        public readonly string ServiceName;

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
        public Service(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
		{
			// Validate size
			if ( length < 1 ) return;

            // Attach to data
            Section section = container.Section;

            // Load static
            ServiceType = (ServiceTypes)section[offset++];

            // Adjust
            length -= 1;

            // Validate size
            if (length < 1) return;

            // Load length
            int providerLen = section[offset++];

            // Adjust
            length -= 1 + providerLen;

            // Validate
            if (length < 1) return;

            // Read
            ProviderName = Section.ReadEncodedString(offset, providerLen, true);

            // Adjust
            offset += providerLen;

            // Load length
            int serviceLen = section[offset++];

            // Adjust
            length -= 1 + serviceLen;

            // Validate
            if (0 != length) return;

            // Read
            ServiceName = Section.ReadEncodedString(offset, serviceLen, true);

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
			return (DescriptorTags.Service == (DescriptorTags)tag);
		}
	}
}
