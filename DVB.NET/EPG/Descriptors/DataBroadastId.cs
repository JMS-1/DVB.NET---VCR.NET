using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// Data Broadcast Identifier <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
    /// </remarks>
    public class DataBroadastId : Descriptor
    {
        /// <summary>
        /// Identification of the broadcast.
        /// </summary>
        public ushort Identifier;

        /// <summary>
        /// Selector Bytes.
        /// </summary>
        public readonly List<byte> Selectors = new List<byte>();

        /// <summary>
        /// 
        /// </summary>
        public DataBroadastId(ushort identifier, params byte[] selectors)
            : base(DescriptorTags.DataBroadcastId)
        {
            Identifier = identifier;

            Selectors.AddRange(selectors);
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
        public DataBroadastId(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
        {
            // Validate size
            if (length < 2) return;

            // Attach to data
            Section section = container.Section;

            // Load
            var idH = section[offset++];
            var idL = section[offset++];

            Identifier = (ushort)((idH << 8) + idL);

            // Load the selector bytes.
            for (length -= 2; length > 0; length--)
            {
                Selectors.Add(section[offset++]);
            }

            // We are valid
            m_Valid = true;
        }

        /// <summary>
        /// Check if this class is responsible for a given descriptor tag.
        /// </summary>
        /// <param name="tag">The tag to test for.</param>
        /// <returns>Set if this class can handle the payload for the given tag.</returns>
        public static bool IsHandlerFor(byte tag) => DescriptorTags.DataBroadcastId == (DescriptorTags)tag;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        protected override void CreatePayload(TableConstructor buffer)
        {
            buffer.Add(Identifier);
            buffer.Add(Selectors.ToArray());
        }
    }
}
