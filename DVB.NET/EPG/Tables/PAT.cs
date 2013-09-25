using System;
using System.Collections;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Tables
{
    /// <summary>
    /// The class is used to describe a <i>Program Association Table</i> which can
    /// be found on PID <i>0x00</i> in a transport stream.
    /// </summary>
    public class PAT : Table
    {
        /// <summary>
        /// The transport stream identifier this <see cref="Table"/> refers to.
        /// </summary>
        public readonly ushort TransportStreamIdentifier;

        /// <summary>
        /// The transport stream identifier for the related NIT.
        /// </summary>
        public readonly ushort NetworkInformationTable;

        /// <summary>
        /// Maps program numbers to the corresponding transport stream identifiers.
        /// </summary>
        public readonly Dictionary<ushort, ushort> ProgramIdentifier = new Dictionary<ushort, ushort>();

        /// <summary>
        /// Set for table identifiers <i>0x42</i> and <i>0x46</i>.
        /// </summary>
        /// <param name="tableIdentifier">The table identifier for which this <see cref="Type"/>
        /// should report its responsibility.</param>
        /// <returns>Set for table identifier <i>0x42</i> and <i>0x46</i>.</returns>
        public static bool IsHandlerFor(byte tableIdentifier)
        {
            // Check all
            return (0x00 == tableIdentifier);
        }

        /// <summary>
        /// Create a new <i>Program Association Table</i> instance.
        /// </summary>
        /// <param name="section">The section which is currently parsed.</param>
        public PAT(Section section)
            : base(section)
        {
            // Get position size of the program list
            int offset = 5, length = section.Length - 3 - offset - 4;

            // Minimum size
            if (length < 0) return;

            // Construct
            TransportStreamIdentifier = Tools.MergeBytesToWord(section[1], section[0]);

            // Process all
            for (; length >= 4; offset += 4, length -= 4)
            {
                // Load items
                ushort number = Tools.MergeBytesToWord(section[offset + 1], section[offset + 0]);
                ushort pid = (ushort)(0x1fff&Tools.MergeBytesToWord(section[offset + 3], section[offset + 2]));

                // Remember
                if (0 == number)
                {
                    // The NIT
                    NetworkInformationTable = pid;
                }
                else
                {
                    // Some program
                    ProgramIdentifier[number] = pid;
                }
            }

            // Usefull
            m_IsValid = (0 == length);
        }
    }
}
