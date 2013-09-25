using System;

namespace JMS.DVB.EPG
{
	/// <summary>
	/// This base class abstract entries defined in <see cref="Table"/> instances and
	/// lets it connect to <see cref="Descriptor"/> instances.
	/// </summary>
    public abstract class EntryBase : IDescriptorContainer
	{
		/// <summary>
		/// The <see cref="Tables.EIT"/> table this event belongs to.
		/// </summary>
		public readonly Table Table;

		/// <summary>
		/// Initialize a new entry.
		/// </summary>
		/// <param name="table">The related <see cref="Table"/>.</param>
		protected EntryBase(Table table)
		{
			// Remember
            Table = table;
		}

        #region IDescriptorContainer Members

        /// <summary>
        /// Report the corresponding SI table.
        /// </summary>
        public Table Container
        {
            get
            {
                // Report
                return Table;
            }
        }

        /// <summary>
        /// Report the corresponding section.
        /// </summary>
        public Section Section
        {
            get
            {
                // Use table
                return Table.Section;
            }
        }

        #endregion
    }
}
