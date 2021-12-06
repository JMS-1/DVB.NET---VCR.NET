using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// Carousel Identifier <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI TS 102 809 V1.3.1 (2017-06)</i> or alternate versions.
    /// </remarks>
    public class CarouselIdentifier : Descriptor
    {
        /// <summary>
        /// Identification of the carousel.
        /// </summary>
        public uint Identifier;

        /// <summary>
        /// Format of the Carousel.
        /// </summary>
        public byte Format;

        /// <summary>
        /// Nicht weiter ausgewertet Informationen zum Format.
        /// </summary>
        public readonly List<byte> RawFormat = new List<byte>();

        /// <summary>
        /// 
        /// </summary>
        public CarouselIdentifier(uint identifier, byte format, params byte[] rawFormat)
            : base(DescriptorTags.CarouselIdentifier)
        {
            Identifier = identifier;
            Format = format;

            RawFormat.AddRange(rawFormat);
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
        public CarouselIdentifier(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
        {
            // Validate size
            if (length < 5) return;

            // Attach to data
            Section section = container.Section;

            // Load
            var id3 = section[offset++];
            var id2 = section[offset++];
            var id1 = section[offset++];
            var id0 = section[offset++];

            Identifier = (uint)(id0 + 256 * (id1 + 256 * (id2 + 256 * id3)));
            Format = section[offset++];

            // Load the selector bytes.
            for (length -= 5; length > 0; length--)
            {
                RawFormat.Add(section[offset++]);
            }

            // We are valid
            m_Valid = true;
        }

        /// <summary>
        /// Check if this class is responsible for a given descriptor tag.
        /// </summary>
        /// <param name="tag">The tag to test for.</param>
        /// <returns>Set if this class can handle the payload for the given tag.</returns>
        public static bool IsHandlerFor(byte tag) => DescriptorTags.CarouselIdentifier == (DescriptorTags)tag;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        protected override void CreatePayload(TableConstructor buffer)
        {
            buffer.Add((byte)(Identifier >> 24));
            buffer.Add((byte)(Identifier >> 16));
            buffer.Add((byte)(Identifier >> 8));
            buffer.Add((byte)Identifier);

            buffer.Add(Format);

            buffer.Add(RawFormat.ToArray());
        }
    }
}
