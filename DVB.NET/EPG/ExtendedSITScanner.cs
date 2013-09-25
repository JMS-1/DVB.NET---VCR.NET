using System;
using JMS.DVB;
using System.Text;
using JMS.DVB.EPG;
using System.Threading;
using JMS.DVB.EPG.Tables;
using System.Collections.Generic;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// 
    /// </summary>
	public abstract class ExtendedSITScanner<TableType> : SITScanner where TableType : Table
	{
        /// <summary>
        /// All table fragments we collected.
        /// </summary>
        public TableType[] TableFragments = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device">The hardware abstraction to use</param>
        /// <param name="pid">The transport stream identifier of the PMT.</param>
        protected ExtendedSITScanner(IDeviceProvider device, ushort pid)
            : base(device, pid)
        {
        }

        /// <summary>
        /// See if we are valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                // Not possible
                if ((null == TableFragments) || (TableFragments.Length < 1)) return false;

                // Test all
                foreach (TableType table in TableFragments)
                    if (null == table)
                        return false;

                // We are
                return true;
            }
        }

        /// <summary>
        /// Process a SI table.
        /// </summary>
        /// <param name="table">The SI table.</param>
        /// <returns>Set if all PAT entries are parsed.</returns>
        protected override bool OnTableFound(Table table)
        {
            // Load the table
            TableType typedTable = table as TableType;

			// Report
			Tools.WriteToScanLog("Table {0}.{4} {1}/{2} Current={3}", table.GetType(), table.SectionNumber, table.LastSectionNumber, table.IsCurrent, table.Version);

            // Verify
			if (null == typedTable)
			{
				// Report
				Tools.WriteToScanLog("Discarded, expected Type {0}", typeof(TableType));

				// Done
				return false;
			}

            // Must reset
            if (!typedTable.IsCurrent)
            {
				// Report
				Tools.WriteToScanLog("Reset, not current");

                // Restart
                TableFragments = null;

                // Done
                return false;
            }

            // Create the resultant table
			if ((null == TableFragments) || (typedTable.SectionNumber >= TableFragments.Length) || (null != TableFragments[typedTable.SectionNumber]))
            {
                // Restart
                TableFragments = null;

                // Wait for the first section
				if (0 != typedTable.SectionNumber)
				{
					// Report
					Tools.WriteToScanLog("Discarded, waiting for Section #0");

					// Done
					return false;
				}

                // Create
                TableFragments = new TableType[typedTable.LastSectionNumber + 1];

				// Report
				Tools.WriteToScanLog("Collection (re)started");
            }

            // Remember
            TableFragments[typedTable.SectionNumber] = typedTable;

            // See if this is it
            for (int i = TableFragments.Length; i-- > 0; )
				if (null == TableFragments[i])
				{
					// Report
					Tools.WriteToScanLog("Table not yet complete, at least Section #{0} is missing", i);

					// Done
					return false;
				}

            // Signal the event
            return true;
        }
    }
}
