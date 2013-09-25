using System;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// Interface implemented by <see cref="Descriptor"/> containers.
    /// </summary>
	public interface IDescriptorContainer
	{
        /// <summary>
        /// Report the table.
        /// </summary>
        Table Container { get; }

        /// <summary>
        /// Report the section.
        /// </summary>
        Section Section { get; }
	}
}
