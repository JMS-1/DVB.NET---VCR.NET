using System;
using System.Collections;

namespace JMS.DVB.EPG.Descriptors
{
	/// <summary>
	/// A parential rating <see cref="Descriptor"/>.
	/// </summary>
	/// <remarks>
	/// For details please refer to the original documentation,
	/// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
	/// </remarks>
	public class ParentalRating: Descriptor
	{
		/// <summary>
		/// The ratings in an abbrivated string format.
		/// </summary>
		public readonly string[] Ratings;

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
        public ParentalRating(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
		{
			// Attach to data
            Section section = container.Section;

			// Collector
			ArrayList all = new ArrayList();

			// Process content
			for ( ; length > 3 ; length -= 4, offset += 4 )
			{
				// Read all
				string country = section.ReadString(offset + 0, 3);
				byte rating = section[offset + 3];

				// Check mode
				if ( 0 == rating )
				{
					// Remember
					all.Add(country);
				}
				else if ( rating < 0x10 )
				{
					// Remember
					all.Add(string.Format("{0}-{1}", country, rating + 3));
				}
				else
				{
					// Remember
					all.Add(string.Format("{0} [{1}]", country, rating));
				}
			}

			// Remember
			Ratings = (string[])all.ToArray(typeof(string));

			// We are valid
			m_Valid = (0 == length);
		}

        /// <summary>
        /// Check if this class is responsible for a given descriptor tag.
        /// </summary>
        /// <param name="tag">The tag to test for.</param>
        /// <returns>Set if this class can handle the payload for the given tag.</returns>
        public static bool IsHandlerFor(byte tag)
		{
			// Check it
			return (DescriptorTags.ParentalRating == (DescriptorTags)tag);
		}
	}
}
