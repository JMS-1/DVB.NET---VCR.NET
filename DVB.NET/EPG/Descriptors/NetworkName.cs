using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
	public class NetworkName: Descriptor
	{
        public readonly string Name;

		public NetworkName(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
		{
			// Load the string
			Name = container.Section.ReadEncodedString(offset, length);

			// We are valid
			m_Valid = true;
		}

        public static bool IsHandlerFor(byte tag)
		{
			// Check it
			return (DescriptorTags.NetworkName == (DescriptorTags)tag);
		}
	}
}
