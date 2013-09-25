using System;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// Instances of this class represent a single <i>Service Information (SI)</i> table 
    /// received from a DVB stream.
    /// <seealso cref="Tables.Generic"/>
    /// <seealso cref="Tables.EIT"/>
    /// </summary>
    public abstract class Table : IDescriptorContainer
    {
        /// <summary>
        /// This map will be created once when this <see cref="Type"/> is accessed
        /// for the first time. For each <see cref="byte"/> of a table indenfier
        /// the corresponding handler class is added.
        /// </summary>
        /// <remarks>
        /// If there is not yet a handler <see cref="Type"/> for a table identifier
        /// <see cref="Tables.Generic"/> will be used.
        /// </remarks>
        private static readonly Type[] m_HandlerForIdentifier = new Type[256];

        /// <summary>
        /// Must include all handler classes.
        /// </summary>
        /// <remarks>
        /// To allow a correct build of the lookup map <see cref="m_HandlerForIdentifier"/>
        /// the handler <see cref="Type"/> must provide a class method <i>IsHandlerFor</i>
        /// of type <see cref="bool"/> with a single <see cref="byte"/> parameter. This method
        /// has to report <i>true</i> for any table identifier it is responsible for.
        /// </remarks>
        private static readonly Type[] m_Handlers = 
		{ 
			typeof(Tables.EIT),
			typeof(Tables.SDT),
			typeof(Tables.PAT),
			typeof(Tables.PMT),
			typeof(Tables.NIT),
            typeof(Tables.CITPremiere),
            typeof(Tables.OpenTV),
            typeof(Tables.TOT),
            typeof(Tables.TDT),
		};

        /// <summary>
        /// The <see cref="Section"/> instance where this table is located.
        /// </summary>
        /// <remarks>
        /// If the table is valid the <see cref="JMS.DVB.EPG.Section.Table"/> field of
        /// the <see cref="Section"/> will point to it.
        /// </remarks>
        private Section m_Section;

        /// <summary>
        /// The last section number.
        /// </summary>
        public readonly byte LastSectionNumber;

        /// <summary>
        /// The current section number.
        /// </summary>
        public readonly byte SectionNumber;

        /// <summary>
        /// Set if this instance describes current events - not upcoming ones.
        /// </summary>
        public readonly bool IsCurrent;

        /// <summary>
        /// The version of this <see cref="Table"/> repetition.
        /// </summary>
        public readonly int Version;

        /// <summary>
        /// Set by derived classes in the constructor as soon as the table
        /// contents is validated.
        /// <seealso cref="IsValid"/>
        /// </summary>
        /// <remarks>
        /// The related <see cref="Section"/> will not set its <see cref="JMS.DVB.EPG.Section.Table"/>
        /// field until the corresponding table is valid.
        /// </remarks>
        protected bool m_IsValid = false;

        /// <summary>
        /// Populate the table identifier lookup map.
        /// </summary>
        static Table()
        {
            // Use helper
            Tools.InitializeDynamicCreate( m_Handlers, m_HandlerForIdentifier, typeof( Tables.Generic ) );
        }

        /// <summary>
        /// Initialize the instance.
        /// </summary>
        /// <param name="section">The related <see cref="Section"/>.</param>
        protected Table( Section section )
        {
            // Remember
            m_Section = section;

            // Direct load
            Version = (section[2] >> 1) & 0x1f;
            IsCurrent = (0 != (section[2] & 1));
            SectionNumber = section[3];
            LastSectionNumber = section[4];
        }

        /// <summary>
        /// Find the handler <see cref="Type"/> for a table identifier and create 
        /// a new instance of it.
        /// <seealso cref="JMS.DVB.EPG.Section.TableIdentifier"/>
        /// </summary>
        /// <param name="section">The related <see cref="Section"/>.</param>
        /// <returns>A new instance of the corresponding handler class. The <see cref="IsValid"/>
        /// of the instance reports if the table is consistent and should be used.</returns>
        static public Table Create( Section section )
        {
            // Attach to the type
            Type pHandler = (Type) m_HandlerForIdentifier[section.TableIdentifier];

            // Create
            return (Table) Activator.CreateInstance( pHandler, new object[] { section } );
        }

        /// <summary>
        /// Report if this table is valid.
        /// </summary>
        /// <remarks>
        /// If there is no current mapping of a table identifier to a handler <see cref="Type"/>
        /// a <see cref="Tables.Generic"/> instance will be created instead. Instance of this
        /// substiate class will never be valid.
        /// </remarks>
        public bool IsValid
        {
            get
            {
                // Report
                return m_IsValid;
            }
        }

        #region IDescriptorContainer Members

        /// <summary>
        /// Report this table.
        /// </summary>
        public Table Container
        {
            get
            {
                // Self
                return this;
            }
        }

        /// <summary>
        /// Report the section.
        /// </summary>
        public Section Section
        {
            get
            {
                // Report
                return m_Section;
            }
        }

        #endregion
    }
}
