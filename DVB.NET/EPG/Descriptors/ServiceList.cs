using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
	public class ServiceList: Descriptor
	{
		public readonly Dictionary<ushort, ServiceTypes> Services = new Dictionary<ushort, ServiceTypes>();

		public ServiceList(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
		{
			// Attach to section
			Section section = container.Section;

			// Process all
			while (length > 0)
			{
				// Not possible
				if (length < 3) return;

				// Load key
				ushort serviceIdentifier = Tools.MergeBytesToWord(section[offset + 1], section[offset + 0]);
				
				// Remember
				Services[serviceIdentifier] = (ServiceTypes)section[offset + 2];

				// Advance
				offset += 3;
				length -= 3;
			}

			// We are valid
			m_Valid = true;
		}

        public static bool IsHandlerFor(byte tag)
		{
			// Check it
			return (DescriptorTags.SeviceList == (DescriptorTags)tag);
		}
	}
}
