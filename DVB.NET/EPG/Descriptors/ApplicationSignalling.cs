using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// Application Signalling <see cref="Descriptor"/>.
    /// </summary>
    /// <remarks>
    /// For details please refer to the original documentation,
    /// e.g. <i>ETSI TS 102 809 V1.3.1 (2017-06)</i> or alternate versions.
    /// </remarks>
    public class ApplicationSignalling : Descriptor
    {
        /// <summary>
        /// Informationen zu einer einzelnen Anwendung.
        /// </summary>
        public struct Application
        {
            /// <summary>
            /// Die Art der Anwendung.
            /// </summary>
            readonly ushort Type;

            /// <summary>
            /// Die Version der Anwendung.
            /// </summary>
            readonly byte Version;

            /// <summary>
            /// Erstellt eine neue Information zu einer Anwendung.
            /// </summary>
            /// <param name="type">Die Art der Anwendung.</param>
            /// <param name="version">Die Version der Anwendung.</param>
            public Application(ushort type, byte version)
            {
                Type = type;
                Version = version;
            }

            /// <summary>
            /// Rekonstruiert die Anewnedungsinformation.
            /// </summary>
            /// <param name="buffer">Sammelt rekonstruierte Daten.</param>
            public void createPayload(TableConstructor buffer)
            {
                buffer.Add((byte)(Type >> 8));
                buffer.Add((byte)Type);
                buffer.Add((byte)(Version | 0xe0));
            }
        };

        /// <summary>
        /// Alle Anwendungen.
        /// </summary>
        public readonly List<Application> Applications = new List<Application>();

        /// <summary>
        /// 
        /// </summary>
        public ApplicationSignalling(params Application[] applications)
            : base(DescriptorTags.ApplicationSignalling)
        {
            Applications.AddRange(applications);
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
        public ApplicationSignalling(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
        {
            // Attach to data
            Section section = container.Section;

            // Readhte list
            for (; length >= 3; length -= 3)
            {
                // Type and version.
                var typeH = section[offset++] & 0x7f;
                var typeL = section[offset++];
                var version = section[offset++] & 0x1f;

                Applications.Add(new Application((ushort)((typeH << 8) + typeL), (byte)version));
            }

            // Validate size
            if (0 != length) return;

            // We are valid
            m_Valid = true;
        }

        /// <summary>
        /// Check if this class is responsible for a given descriptor tag.
        /// </summary>
        /// <param name="tag">The tag to test for.</param>
        /// <returns>Set if this class can handle the payload for the given tag.</returns>
        public static bool IsHandlerFor(byte tag) => DescriptorTags.ApplicationSignalling == (DescriptorTags)tag;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        protected override void CreatePayload(TableConstructor buffer)
        {
            // Das machen die Anwendungen selbst.
            Applications.ForEach(a => a.createPayload(buffer));
        }
    }
}
