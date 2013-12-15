using System;
using System.Collections.Generic;


namespace JMS.DVB.EPG.Tables
{
    /// <summary>
    /// The class is used to describe a <i>Program Map Table</i> which can
    /// be found on various PIDs in a transport stream.
    /// </summary>
    public class PMT : Table
    {
        /// <summary>
        /// The PCR for this program.
        /// </summary>
        public readonly ushort PCRPID;

        /// <summary>
        /// The program number of this program.
        /// </summary>
        public readonly ushort ProgramNumber;

        /// <summary>
        /// Descriptors for this program.
        /// </summary>
        public readonly Descriptor[] Descriptors;

        /// <summary>
        /// 
        /// </summary>
        public readonly ProgramEntry[] ProgramEntries;

        /// <summary>
        /// Set for table identifiers <i>0x42</i> and <i>0x46</i>.
        /// </summary>
        /// <param name="tableIdentifier">The table identifier for which this <see cref="Type"/>
        /// should report its responsibility.</param>
        /// <returns>Set for table identifier <i>0x42</i> and <i>0x46</i>.</returns>
        public static bool IsHandlerFor( byte tableIdentifier )
        {
            // Check all
            return (0x02 == tableIdentifier);
        }

        /// <summary>
        /// Create a new <i>Program Map Table</i> instance.
        /// </summary>
        /// <param name="section">The section which is currently parsed.</param>
        public PMT( Section section )
            : base( section )
        {
            // Special recommendation
            if ((0 != SectionNumber) || (0 != LastSectionNumber)) return;

            // Get the overall length
            int offset = 9, length = section.Length - 3 - offset - 4;

            // Not possible
            if (length < 0) return;

            // Read statics
            ProgramNumber = Tools.MergeBytesToWord( Section[1], Section[0] );
            PCRPID = (ushort) (0x1fff & Tools.MergeBytesToWord( Section[6], Section[5] ));

            // Length
            int infoLength = 0xfff & Tools.MergeBytesToWord( Section[8], Section[7] );

            // Validate
            if (length < infoLength) return;

            // Create my descriptors
            Descriptors = Descriptor.Load( this, offset, infoLength );

            // Adjust
            offset += infoLength;
            length -= infoLength;

            // Result
            var entries = new List<ProgramEntry>();

            // Fill
            while (length > 0)
            {
                // Create next
                ProgramEntry entry = ProgramEntry.Create( this, offset, length );

                // Failed
                if ((null == entry) || !entry.IsValid) return;

                // Remember
                entries.Add( entry );

                // Adjust
                offset += entry.Length;
                length -= entry.Length;
            }

            // Use it
            ProgramEntries = entries.ToArray();

            // Usefull
            m_IsValid = true;
        }
    }
}
