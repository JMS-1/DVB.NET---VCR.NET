using System;

namespace JMS.DVB.EPG.Descriptors
{
	/// <summary>
	/// A generic <see cref="Descriptor"/> class used for all unsupported
	/// descriptor tags.
	/// </summary>
	/// <remarks>
	/// <see cref="Descriptor.IsValid"/> will always be unset.
	/// </remarks>
	public class Generic: Descriptor
	{
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
        public Generic(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
		{
        }
	}
}
