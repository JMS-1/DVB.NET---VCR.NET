using System;

namespace JMS.DVB.EPG.Descriptors
{
	/// <summary>
	/// A PDC <see cref="Descriptor"/>.
	/// </summary>
	/// <remarks>
	/// For details please refer to the original documentation,
	/// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
	/// </remarks>
	public class PDCDescriptor: Descriptor
	{
		/// <summary>
		/// The unique identification label for this event.
		/// </summary>
		public readonly string ProgramIdentificationLabel;

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
        public PDCDescriptor(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
		{
			// Validate size
			if ( 3 != length ) return;

			// Attach to data
            Section section = container.Section;

			// Read parts
			byte pil0 = section[offset + 0];
			byte pil1 = section[offset + 1];
			byte pil2 = section[offset + 2];

			// Split
			int dayHigh = pil0&0xf;
			int dayLow = pil1>>7;
			int hourHigh = pil1&0x7;
			int hourLow = pil2>>6;

			// Create label
			ProgramIdentificationLabel = string.Format("{0:00}-{1:00} {2:00}:{3:00}", (pil1>>3)&0xf, dayLow + 2 * dayHigh, hourLow + 4 * hourHigh, pil2&0x3f);

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
			return (DescriptorTags.PDC == (DescriptorTags)tag);
		}
	}
}
