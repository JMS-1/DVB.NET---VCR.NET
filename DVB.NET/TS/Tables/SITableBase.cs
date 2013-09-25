using System;


namespace JMS.DVB.TS.Tables
{
    /// <summary>
    /// Helper to construct a table. This class is not thread-safe concerning
    /// changes.
    /// </summary>
    public abstract class SITableBase
    {
        /// <summary>
        /// Cached table data.
        /// </summary>
        private byte[] TableData = null;

        /// <summary>
        /// Change count on the table.
        /// <seealso cref="Changed"/>
        /// </summary>
        private int Version = 0;

        /// <summary>
        /// Transport stream packet counter.
        /// </summary>
        private int Counter = 0;

        /// <summary>
        /// The transport stream identifier used for this table.
        /// </summary>
        public readonly short PID;

        /// <summary>
        /// Initialize this instance.
        /// </summary>
        /// <param name="pid">The transposrt stream identifier for this table.</param>
        protected SITableBase( short pid )
        {
            // Remember
            PID = pid;
        }

        /// <summary>
        /// Report the table identifier.
        /// </summary>
        protected abstract byte TableIdentifier { get; }

        /// <summary>
        /// Report the optional field in the table header.
        /// </summary>
        protected abstract short PrivateData { get; }

        /// <summary>
        /// Create the table data.
        /// </summary>
        /// <returns>The inner table data excluding the header.</returns>
        protected abstract byte[] CreateTable();

        /// <summary>
        /// Called if the table has changed so that the cached table data
        /// has to be recalculated.
        /// </summary>
        /// <remarks>
        /// The <see cref="Version"/> will be incremented.
        /// </remarks>
        protected void Changed()
        {
            // Must reload on next call
            TableData = null;

            // Count version
            ++Version;
        }

        /// <summary>
        /// Report the version of the table.
        /// </summary>
        public int TableVersion
        {
            get
            {
                // Report
                return Version;
            }
            set
            {
                // Validate
                if (value < 0) throw new ArgumentOutOfRangeException( "value" );

                // Must reload on next call
                TableData = null;

                // Remember
                Version = value;
            }
        }

        /// <summary>
        /// Send the current table into the transport stream.
        /// <seealso cref="Manager.SendTable"/>
        /// <seealso cref="EPG.CRC32.GetCRC"/>
        /// </summary>
        /// <remarks>
        /// If there is no current cached version the table will be reconstructed
        /// using <see cref="CreateTable"/>. A table header will be automatically
        /// added and a valid CRC32 checksum appended.
        /// </remarks>
        /// <param name="target">The transport stream where to add the table to.</param>
        /// <exception cref="NotImplementedException">Currently the size of a table
        /// may not exceed a single transport stream package - <see cref="CreateTable"/>
        /// may not report more than 171 bytes.</exception>
        public void Send( Manager target )
        {
            // Create table
            if (TableData == null)
            {
                // Retrieve the raw data
                var inner = CreateTable();
                var outer = new byte[9 + inner.Length + 4];

                // Merge inner
                inner.CopyTo( outer, 9 );

                // Load private data
                int priv = PrivateData;

                // Cut
                priv &= 0xffff;

                // Get section length
                int len = outer.Length - 4;

                // Fill table header
                outer[1] = TableIdentifier;
                outer[2] = (byte) (0xb0 | (len / 256));
                outer[3] = (byte) (len & 0xff);
                outer[4] = (byte) (priv / 256);
                outer[5] = (byte) (priv & 0xff);
                outer[6] = (byte) (0x01 | ((Version & 0x1f) * 2));

                // Calculate CRC32
                uint crc32 = EPG.CRC32.GetCRC( outer, 1, len - 1 );

                // At the very end
                int crcIndex = outer.Length;

                // Fill in
                outer[--crcIndex] = (byte) (crc32 & 0xff);
                outer[--crcIndex] = (byte) ((crc32 >> 8) & 0xff);
                outer[--crcIndex] = (byte) ((crc32 >> 16) & 0xff);
                outer[--crcIndex] = (byte) (crc32 >> 24);

                // Use table
                TableData = outer;
            }

            // Send table
            target.SendTable( ref Counter, PID, TableData );
        }
    }
}
