using System;

namespace JMS.DVB.TS.Tables
{
    /// <summary>
    /// Represents the one and only <i>Process Association Section</i> in a
    /// transport stream.
    /// </summary>
    public class PAT : SITableBase
    {
        /// <summary>
        /// A random program number.
        /// </summary>
        private short m_ProgramNumber = 0x0102;

        /// <summary>
        /// A random transport stream identifier for the <see cref="PMT"/>.
        /// </summary>
        private short m_ProgramStream = 0x0100;

        /// <summary>
        /// A random network identifier.
        /// </summary>
        private short m_NetworkIdentifier = 1;

        /// <summary>
        /// Create a new instance which will be bound to the transport stream
        /// identifier <i>0</i>.
        /// </summary>
        public PAT() : base(0x0000)
        {
        }

        /// <summary>
        /// The table identifier for a PAT is <i>0</i>.
        /// </summary>
        protected override byte TableIdentifier => 0x00;

        /// <summary>
        /// The optional field in the table header will be filled with our network
        /// identifier.
        /// </summary>
        protected override short PrivateData => m_NetworkIdentifier;


        /// <summary>
        /// Create the PAT based on the current settings.
        /// </summary>
        /// <remarks>
        /// In this context the output is always fixed.
        /// </remarks>
        /// <returns>The table contents for the PAT describing a single program.</returns>
        protected override byte[] CreateTable()
        {
            // Load
            int prog = m_ProgramNumber;
            int stream = m_ProgramStream;

            // Cut
            prog &= 0xffff;
            stream &= 0xffff;

            // Construct
            return new byte[] { (byte)(prog / 256), (byte)(prog & 0xff), (byte)(0xe0 | (stream / 256)), (byte)(stream & 0xff) };
        }

        /// <summary>
        /// Get or set the related netowrk identifier.
        /// <seealso cref="SITableBase.Changed"/>
        /// </summary>
        public short NetworkIdentifier
        {
            get
            {
                // Report
                return m_NetworkIdentifier;
            }
            set
            {
                // Check
                if (value == m_NetworkIdentifier) return;

                // Update
                m_NetworkIdentifier = value;

                // Mark
                Changed();
            }
        }

        /// <summary>
        /// Get or set the number of the only program supported.
        /// <seealso cref="SITableBase.Changed"/>
        /// </summary>
        public short ProgramNumber
        {
            get
            {
                // Report
                return m_ProgramNumber;
            }
            set
            {
                // Check
                if (value == m_ProgramNumber) return;

                // Update
                m_ProgramNumber = value;

                // Mark
                Changed();
            }
        }

        /// <summary>
        /// Get or set the transport stream identifier of the only
        /// program supported.
        /// <seealso cref="SITableBase.Changed"/>
        /// </summary>
        public short ProgramStream
        {
            get
            {
                // Report
                return m_ProgramStream;
            }
            set
            {
                // Check
                if (value == m_ProgramStream) return;

                // Update
                m_ProgramStream = value;

                // Mark
                Changed();
            }
        }
    }
}
