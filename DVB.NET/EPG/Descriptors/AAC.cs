using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// AAC descriptor.
    /// </summary>
	public class AAC : Descriptor
    {
        /// <summary>
        /// Profile and level of the content.
        /// </summary>
        public byte ProfileAndLevel { get; set; }

        /// <summary>
        /// AAC Type of the content.
        /// </summary>
        public byte? Type { get; set; }

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
                m_AdditionalInformation = value ?? new byte[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public AAC(byte profileAndLevel, byte? type)
            : base(DescriptorTags.AAC)
        {
            ProfileAndLevel = profileAndLevel;
            Type = type;
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
        public AAC(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
        {
            // Attach to the section
            Section section = container.Section;

            // Check minimum length
            if (length-- < 1) return;

            // Read profile and level.
            ProfileAndLevel = section[offset++];

            // Check minimum length
            if (length-- < 1) return;

            // Read flags
            byte flags = section[offset++];

            // Load type.
            if (0x80 == (0x80 & flags))
            {
                // Check minimum length
                if (length-- < 1) return;

                Type = section[offset++];
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
        public static bool IsHandlerFor(byte tag) => (DescriptorTags.AAC == (DescriptorTags)tag);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        protected override void CreatePayload(TableConstructor buffer)
        {
            // Profile and level.
            buffer.Add(ProfileAndLevel);

            // Collected flags
            byte flags = 0;

            // Load flags
            if (Type.HasValue) flags |= 0x80;

            // Write flags
            buffer.Add(flags);

            // Write all
            if (Type.HasValue) buffer.Add(Type.Value);

            // Load the additional information
            buffer.Add(m_AdditionalInformation);
        }
    }
}
