using System;
using System.IO;
using System.Text;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// The main parser for a <i>Service Information (SI)</i> DVB stream. 
    /// </summary>
    /// <remarks>
    /// Special functionality is provided to directly attach a parser
    /// instance to a PID filter to parse <see cref="Tables.EIT"/>
    /// data of the <i>Electronic Program Guide (EPG)</i>.
    /// </remarks>
    public class Parser
    {
        /// <summary>
        /// Methode signature for a notification on a successfully parsed
        /// <see cref="Section"/>.
        /// <seealso cref="SectionFound"/>
        /// </summary>
        public delegate void SectionFoundHandler( Section section );

        /// <summary>
        /// Will be fired whenever <see cref="OnData(byte[])"/> detects a
        /// <see cref="Section"/> with <see cref="Section.IsValid"/>
        /// set.
        /// </summary>
        public event SectionFoundHandler SectionFound;

        /// <summary>
        /// The filter data for EPG filtering.
        /// <seealso cref="FilterMask"/>
        /// </summary>
        static public byte[] FilterData = { 0x40 };

        /// <summary>
        /// The filter mask for EPG filtering.
        /// </summary>
        /// <remarks>
        /// Together with <see cref="FilterData"/> this will make
        /// a DVB filter sensitive to the table identifiers
        /// <i>0x40</i> to <i>0x7f</i> which includes all variants of 
        /// <see cref="Tables.EIT"/> tables.
        /// </remarks>
        static public byte[] FilterMask = { 0xc0 };

        /// <summary>
        /// Local buffer.
        /// </summary>
        private byte[] m_Buffer = null;

        /// <summary>
        /// Next byte to read from the current buffer.
        /// </summary>
        private int m_BufferPos = 0;

        /// <summary>
        /// Current number of bytes in the buffer.
        /// </summary>
        private int m_BufferSize = 0;

        /// <summary>
        /// Set to check out sections with CRC errors, too.
        /// </summary>
        public bool IgnoreCRCErrors = false;

        /// <summary>
        /// Count the number of data overruns.
        /// </summary>
        public long OverrunErrors = 0;

        /// <summary>
        /// Count the number of times section length was corrupted.
        /// </summary>
        public long WrongLength = 0;

        /// <summary>
        /// Count all CRC errors.
        /// </summary>
        public long CRCErrors = 0;

        /// <summary>
        /// Add some data to the current buffer.
        /// </summary>
        /// <remarks>
        /// The buffer may be readjusted or even recreated to fit the
        /// data reported. There is a minimum size of 250kBytes allocated
        /// for the buffer.
        /// </remarks>
        /// <param name="data">The external buffer where the new data can be found.</param>
        /// <param name="offset">First byte in the external buffer.</param>
        /// <param name="length">Number of bytes to add to our current buffer.</param>
        /// <exception cref="ArgumentNullException">The external buffer must not be empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The parameter set is inconsistent.</exception>
        /// <exception cref="OverflowException">If the input data is not decodable it may be possible
        /// in very rare situations that the current buffer never shrinks. In this case an upper
        /// bound of 250kBytes will be applied.
        /// </exception>
        public void Add( byte[] data, int offset, int length )
        {
            // Check parameters
            if (null == data) throw new ArgumentNullException( "data" );
            if (offset < 0) throw new ArgumentOutOfRangeException( "offset", offset, "must not be negative" );
            if (offset > data.Length) throw new ArgumentOutOfRangeException( "offset", offset, "out of array" );
            if (length < 0) throw new ArgumentOutOfRangeException( "length", length, "must not be negative" );
            if (length > data.Length) throw new ArgumentOutOfRangeException( "length", length, "out of array" );
            if ((offset + length) > data.Length) throw new ArgumentOutOfRangeException( "length", length, "not enough space in source" );

            // Validate
            if (0 == length) return;

            // Size at the end of the buffer
            int rest = ((null == m_Buffer) ? 0 : m_Buffer.Length) - m_BufferSize;

            // See if this can be added to the current buffer
            if (length <= rest)
            {
                // Move in
                Array.Copy( data, offset, m_Buffer, m_BufferSize, length );

                // Adjust
                m_BufferSize += length;

                // Done
                return;
            }

            // Size needed
            int dataSize = m_BufferSize - m_BufferPos, size = dataSize + length;

            // Above minimum
            if (size < 250000) size = 250000;

            // Protect against overload
            if (size > 250000)
            {
                // Set error code
                ++OverrunErrors;

                // Full reset
                m_BufferPos = 0;
                m_BufferSize = 0;

                // Report error - filter stream will normally reconnect
                throw new OverflowException( "Buffer overrun - Stream may be corrupted" );
            }

            // New buffer
            byte[] newBuffer = ((null == m_Buffer) || (size > m_Buffer.Length)) ? new byte[size] : m_Buffer;

            // Copy
            if (null != m_Buffer) Array.Copy( m_Buffer, m_BufferPos, newBuffer, 0, dataSize );

            // Append
            Array.Copy( data, offset, newBuffer, dataSize, length );

            // Remember
            m_Buffer = newBuffer;
            m_BufferPos = 0;
            m_BufferSize = dataSize + length;
        }

        /// <summary>
        /// Try to read the next <see cref="Section"/> from the current buffer.
        /// </summary>
        /// <remarks>
        /// If the next <see cref="Section"/> has <see cref="Section.IsValid"/> unset
        /// it is automatically skipped.
        /// </remarks>
        /// <returns><i>null</i> if there is not enough data available for a next
        /// <see cref="Section"/> instance. In this case this method should not be
        /// called again before more data is added to the current buffer using <see cref="Add"/>.
        /// If a <see cref="Section"/> is returned its <see cref="Section.IsValid"/> will
        /// always be set.</returns>
        public virtual Section ReadSection()
        {
            // As long as necessary
            for (; ; )
            {
                // Try to create section at the indicated position
                Section section = Section.Create( m_Buffer, m_BufferPos, m_BufferSize - m_BufferPos, this );

                // Need more data
                if (null == section) return null;

                // Advance
                m_BufferPos += section.Length;

                // Check for error
                if (!section.IsValid)
                {
                    // Count
                    ++CRCErrors;

                    // Not allowed
                    if (!IgnoreCRCErrors) continue;
                }

                // Report
                return section;
            }
        }

        /// <summary>
        /// Process incoming data.
        /// </summary>
        /// <remarks>
        /// It will forward to <see cref="OnData(byte[], int, int)"/>. But in contrast
        /// to the other variant this method will ignore any kind of <see cref="Exception"/>
        /// generated while parsing the SI data stream. This makes the filter thread stable
        /// against corrupted streams.
        /// </remarks>
        /// <param name="aData">Some data to be addeded.</param>
        public void OnData( byte[] aData )
        {
            // Make sure that errors do not propagate to filter thread
            try
            {
                // Process
                OnData( aData, 0, aData.Length );
            }
            catch
            {
            }
        }

        /// <summary>
        /// Add the indicated bytes to the current buffer using <see cref="Add"/> and
        /// process as much data as possible.
        /// </summary>
        /// <remarks>
        /// <see cref="ReadSection"/> is called as long as the result is not <i>null</i>.
        /// Each <see cref="Section"/> found is reported to <see cref="OnSectionFound"/>.
        /// </remarks>
        /// <param name="data">The external buffer where the new data can be found.</param>
        /// <param name="offset">First byte in the external buffer.</param>
        /// <param name="length">Number of bytes to add to our current buffer.</param>
        public void OnData( byte[] data, int offset, int length )
        {
            // Merge in
            Add( data, offset, length );

            // Read all sections
            for (Section section; null != (section = ReadSection()); )
                if (section.IsValid || IgnoreCRCErrors)
                    OnSectionFound( section );
        }

        /// <summary>
        /// Fire <see cref="SectionFound"/>.
        /// </summary>
        /// <param name="section">A <see cref="Section"/> constructed from
        /// the raw input stream.</param>
        protected virtual void OnSectionFound( Section section )
        {
            // Load
            SectionFoundHandler handler = SectionFound;

            // Send
            if (null != handler) handler( section );
        }
    }

    /// <summary>
    /// Überwacht SI Tabellen einer bestimmten Art.
    /// </summary>
    /// <typeparam name="T">Die Art der SI Tabelle.</typeparam>
    public class TypedSIParser<T> : Parser where T : Table
    {
        /// <summary>
        /// Signatur einer Methode zur Information über eine erfolgreich empfangene SI Tabelle.
        /// </summary>
        /// <param name="table">Die empfangene Tabelle.</param>
        public delegate void TableFoundHandler( T table );

        /// <summary>
        /// Methode, die beim Empfang einer SI Tabelle aktiviert wird.
        /// </summary>
        public event TableFoundHandler TableFound;

        /// <summary>
        /// Erzeugt eine neue Analyseinstanz.
        /// </summary>
        public TypedSIParser()
        {
        }

        /// <summary>
        /// Analysiert eine Tabelle.
        /// </summary>
        /// <param name="section">Die SI Tabelle.</param>
        protected override void OnSectionFound( Section section )
        {
            // See if table has the rigth type
            if ((null != section) && section.IsValid)
            {
                // Load table
                T table = section.Table as T;
                if ((null != table) && table.IsValid)
                {
                    // Forward
                    TableFoundHandler callback = TableFound;
                    if (null != callback) callback( table );
                }
            }

            // Forward
            base.OnSectionFound( section );
        }
    }
}
