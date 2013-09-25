using System;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// A single entry in a <see cref="Tables.SDT"/>.
    /// </summary>
    public class ServiceEntry : EntryBase
    {
        /// <summary>
        /// The <see cref="Descriptor"/> instances related to this event.
        /// </summary>
        /// <remarks>
        /// Please refer to the original documentation to find out which descriptor
        /// type is allowed in a <see cref="Tables.SDT"/> table.
        /// </remarks>
        public readonly Descriptor[] Descriptors;

        /// <summary>
        /// Set if the event data is consistent.
        /// </summary>
        public readonly bool IsValid = false;

        /// <summary>
        /// The total length of the event in bytes.
        /// </summary>
        public readonly int Length;

        /// <summary>
        /// The service identifier for this service.
        /// </summary>
        public readonly ushort ServiceIdentifier;

        /// <summary>
        /// 
        /// </summary>
        public readonly bool Scrambled;

        /// <summary>
        /// Create a new service instance.
        /// </summary>
        /// <param name="table">The related <see cref="Tables.SDT"/> table.</param>
        /// <param name="offset">The first byte of this service in the <see cref="EPG.Table.Section"/>
        /// for the related <see cref="Table"/>.</param>
        /// <param name="length">The maximum number of bytes available. If this number
        /// is greater than the <see cref="Length"/> of this service another event will
        /// follow in the same table.</param>
        internal ServiceEntry(Table table, int offset, int length)
            : base(table)
        {
            // Access section
            Section section = Section;

            // Read statics
            ServiceIdentifier = Tools.MergeBytesToWord(section[offset + 1], section[offset + 0]);
            Scrambled = (0 != (section[offset + 3] & 0x10));

            // Decode
            int highLoop = section[offset + 3] & 0xf;
            int lowLoop = section[offset + 4];

            // Number of descriptors
            int loop = lowLoop + 256 * highLoop;

            // Caluclate the total length
            Length = 5 + loop;

            // Verify
            if (Length > length) return;

            // Try to load descriptors
            Descriptors = Descriptor.Load(this, offset + 5, loop);

            // Can use it
            IsValid = true;
        }

        /// <summary>
        /// Create a new service instance.
        /// </summary>
        /// <param name="table">The related <see cref="Tables.SDT"/> table.</param>
        /// <param name="offset">The first byte of this service in the <see cref="EPG.Table.Section"/>
        /// for the related <see cref="Table"/>.</param>
        /// <param name="length">The maximum number of bytes available. If this number
        /// is greater than the <see cref="Length"/> of this event another event will
        /// follow in the same table.</param>
        /// <returns>A new service instance or <i>null</i> if there are less than
        /// 5 bytes available.</returns>
        static internal ServiceEntry Create(Table table, int offset, int length)
        {
            // Validate
            if (length < 5) return null;

            // Create
            return new ServiceEntry(table, offset, length);
        }
    }
}
