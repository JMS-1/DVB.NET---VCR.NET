using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// AC3 descriptor.
    /// </summary>
	public class AC3: Descriptor
	{
        /// <summary>
        /// Set if <see cref="ComponentType"/> is valid.
        /// </summary>
		private bool m_HasComponentType = false;

		/// <summary>
		/// 
		/// </summary>
		public bool HasComponentType
		{
			get 
			{ 
				// Report
				return m_HasComponentType; 
			}
			set 
			{ 
				// Update
				m_HasComponentType = value; 
			}
		}

        /// <summary>
        /// The type of the component.
        /// </summary>
		private byte m_ComponentType = 0;

		/// <summary>
		/// 
		/// </summary>
		public byte ComponentType
		{
			get 
			{ 
				// Validate
				if (!m_HasComponentType) throw new NotSupportedException();

				// Report
				return m_ComponentType; 
			}
			set 
			{ 
				// Update and mark
				m_HasComponentType = true;
				m_ComponentType = value; 
			}
		}

        /// <summary>
        /// Set if <see cref="BSID"/> is valid.
        /// </summary>
		private bool m_HasBSID = false;

		/// <summary>
		/// 
		/// </summary>
		public bool HasBSID
		{
			get 
			{ 
				// Report
				return m_HasBSID; 
			}
			set 
			{ 
				// Update
				m_HasBSID = value; 
			}
		}

        /// <summary>
        ///  The BSID of this AC3 stream.
        /// </summary>
		private byte m_BSID = 0;

		/// <summary>
		/// 
		/// </summary>
		public byte BSID
		{
			get 
			{ 
				// Validate
				if (!m_HasBSID) throw new NotSupportedException();

				// Report
				return m_BSID; 
			}
			set 
			{ 
				// Update and mark
				m_HasBSID = true;
				m_BSID = value; 
			}
		}

        /// <summary>
        /// Set if <see cref="MainID"/> is valid.
        /// </summary>
		private bool m_HasMainID = false;

		/// <summary>
		/// 
		/// </summary>
		public bool HasMainID
		{
			get 
			{ 
				// Report
				return m_HasMainID; 
			}
			set 
			{ 
				// Change
				m_HasMainID = value; 
			}
		}

        /// <summary>
        /// The main ID of this AC3 stream.
        /// </summary>
		private byte m_MainID = 0;

		/// <summary>
		/// 
		/// </summary>
		public byte MainID
		{
			get 
			{ 
				// Validate
				if (!m_HasMainID) throw new NotSupportedException();

				// Report
				return m_MainID; 
			}
			set 
			{ 
				// Update and mark
				m_HasMainID = true;
				m_MainID = value; 
			}
		}

        /// <summary>
        /// Set if <see cref="AssociatedService"/> is valid.
        /// </summary>
		private bool m_HasAssociatedService = false;

		/// <summary>
		/// 
		/// </summary>
		public bool HasAssociatedService
		{
			get 
			{ 
				// Report
				return m_HasAssociatedService; 
			}
			set 
			{ 
				// Update
				m_HasAssociatedService = value; 
			}
		}

        /// <summary>
        /// The associated service for this AC3 stream.
        /// </summary>
		private byte m_AssociatedService = 0;

		/// <summary>
		/// 
		/// </summary>
		public byte AssociatedService
		{
			get 
			{ 
				// Validate
				if (!m_HasAssociatedService) throw new NotSupportedException();

				// Report
				return m_AssociatedService; 
			}
			set 
			{ 
				// Update and mark
				m_HasAssociatedService = true;
				m_AssociatedService = value; 
			}
		}

        /// <summary>
        /// Additional information.
        /// </summary>
		private byte[] m_AdditionalInformation = { };

		/// <summary>
		/// 
		/// </summary>
		public byte[] AdditionalInformation
		{
			get 
			{ 
				// Report
				return m_AdditionalInformation; 
			}
			set 
			{ 
				// Never null it
				if (null == value) value = new byte[0];

				// Update
				m_AdditionalInformation = value; 
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public AC3()
			: base(DescriptorTags.AC3)
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
        public AC3(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
		{
			// Check minimum length
			if ( length-- < 1 ) return;

            // Attach to the section
            Section section = container.Section;

            // Read flags
            byte flags = section[offset++];

            // Load flags
            m_HasComponentType = (0x80 == (0x80 & flags));
            m_HasBSID = (0x40 == (0x40 & flags));
            m_HasMainID = (0x20 == (0x20 & flags));
            m_HasAssociatedService = (0x10 == (0x10 & flags));

            // Read all
            if (m_HasComponentType)
            {
                // Test
                if (length-- < 1) return;

                // Load
                m_ComponentType = section[offset++];
            }
            if (m_HasBSID)
            {
                // Test
                if (length-- < 1) return;

                // Load
                m_BSID = section[offset++];
            }
            if (m_HasMainID)
            {
                // Test
                if (length-- < 1) return;

                // Load
                m_MainID = section[offset++];
            }
            if (m_HasAssociatedService)
            {
                // Test
                if (length-- < 1) return;

                // Load
                m_AssociatedService = section[offset++];
            }

            // Load the additional information
            m_AdditionalInformation = section.ReadBytes(offset, length);

            // Done
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
            return (DescriptorTags.AC3 == (DescriptorTags)tag);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="buffer"></param>
		protected override void CreatePayload(TableConstructor buffer)
		{
			// Collected flags
			byte flags = 0;

			// Load flags
			if (m_HasComponentType) flags |= 0x80;
			if (m_HasBSID) flags |= 0x40;
			if (m_HasMainID) flags |= 0x20;
			if (m_HasAssociatedService) flags |= 0x10;

			// Write flags
			buffer.Add(flags);

			// Write all
			if (m_HasComponentType) buffer.Add(m_ComponentType);
			if (m_HasBSID) buffer.Add(m_BSID);
			if (m_HasMainID) buffer.Add(m_MainID);
			if (m_HasAssociatedService) buffer.Add(m_AssociatedService);

			// Load the additional information
			buffer.Add(m_AdditionalInformation);
		}
	}
}
