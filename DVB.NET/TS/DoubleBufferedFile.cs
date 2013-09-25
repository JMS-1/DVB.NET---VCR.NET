using System;
using System.IO;

namespace JMS.DVB.TS
{
    /// <summary>
    /// Helper class to write data to a <see cref="File"/> using double buffering
    /// technology and asynchronous writes.
    /// </summary>
    public class DoubleBufferedFile : IDisposable
    {
        /// <summary>
        /// The <see cref="File"/> we write the data to.
        /// </summary>
        private FileStream m_File = null;

        /// <summary>
        /// First buffer.
        /// <seealso cref="m_Buf2"/>
        /// </summary>
        private byte[] m_Buf1 = null;

        /// <summary>
        /// Second buffer.
        /// <seealso cref="m_Buf1"/>
        /// </summary>
        private byte[] m_Buf2 = null;

        /// <summary>
        /// Currently active operation
        /// </summary>
        private IAsyncResult m_Writer = null;

        /// <summary>
        /// Position in <see cref="m_Buf1"/> where next byte has to go to.
        /// </summary>
        private int m_Buf1Pos = 0;

        /// <summary>
        /// Number of times <see cref="FinishDiskWrite"/> has to wait for a disk operation
        /// to finish.
        /// </summary>
        private uint m_Waits = 0;

        /// <summary>
        /// Die gesamte Anzahl von Bytes, die von <see cref="Write"/> verarbeitet wurden. Dies
        /// entspricht etwa der Dateiposition, auch wenn diese durch die Doppelspeicherung
        /// evtl. noch nicht beschrieben wurde.
        /// </summary>
        public long TotalBytesWritten { get; private set; }

        /// <summary>
        /// Meldet den Namen der zugeordneten Zieldatei.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Allocate the buffers <see cref="m_Buf1"/> and <see cref="m_Buf2"/>
        /// and create <see cref="m_File"/> on the indicated <see cref="File"/>.
        /// </summary>
        /// <param name="sOutFileName">Output <see cref="File"/> - if the file alreay
        /// exists it will be overwritten.</param>
        /// <param name="nSize">Size of each buffer in bytes.</param>
        public DoubleBufferedFile( string sOutFileName, int nSize )
        {
            // Create buffers
            m_Buf1 = new byte[nSize];
            m_Buf2 = new byte[nSize];

            // Attach to file
            m_File = new FileStream( sOutFileName, FileMode.Create, FileAccess.Write, FileShare.Read );

            // Remember
            FilePath = m_File.Name;
        }

        /// <summary>
        /// Report <see cref="m_Waits"/> synchronized on the current instance.
        /// </summary>
        public uint DiskWaits
        {
            get
            {
                // Report
                lock (this)
                    return m_Waits;
            }
        }

        /// <summary>
        /// Finish the current disk operation attached to <see cref="m_Writer"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="IAsyncResult.IsCompleted"/> is not active and the parameter
        /// is set <see cref="m_Waits"/> will be incremented. If necessary
        /// <see cref="System.Threading.WaitHandle.WaitOne()"/> 
        /// on <see cref="IAsyncResult.AsyncWaitHandle"/>
        /// is used to synchronize with the disk.
        /// </remarks>
        /// <param name="bMoreData">The file is not finished yet.</param>
        private void FinishDiskWrite( bool bMoreData )
        {
            // Nothing to do
            if (null == m_Writer)
                return;

            // Must wait
            if (!m_Writer.IsCompleted)
            {
                // Increment indicator
                if (bMoreData)
                    lock (this)
                        ++m_Waits;

                // Wait
                m_Writer.AsyncWaitHandle.WaitOne();
            }

            // Finish
            if (null != m_File)
                m_File.EndWrite( m_Writer );

            // Clear
            m_Writer = null;
        }

        /// <summary>
        /// It's recommended to call this method after any call to <see cref="StartWrite"/>.
        /// </summary>
        /// <remarks>
        /// A client calls <see cref="StartWrite"/> to allocate some space in one of
        /// our buffers. Then the space is filled and the reserved space is finally
        /// allocated using this method. The parameter must not be negative and should
        /// normally not be greated than the number of bytes requested in <see cref="StartWrite"/>.
        /// <seealso cref="StartWrite"/>.
        /// </remarks>
        /// <param name="nBytes">Bytes used in the allocated area.</param>
        public void EndWrite( int nBytes )
        {
            // Invalid
            if ((nBytes < 0) || ((m_Buf1Pos + nBytes) > m_Buf1.Length)) throw new ArgumentOutOfRangeException( "Not enough Space in Buffer" );

            // Update index
            m_Buf1Pos += nBytes;
        }

        /// <summary>
        /// Buffer management - call this method to allocate some space in one
        /// of the buffers.
        /// </summary>
        /// <remarks>
        /// The number of bytes must never be negative. If the number is zero the end
        /// of the operation is indicated: all buffers will be written to the disk - 
        /// the file will be closed in <see cref="Dispose"/>. Else the indicated
        /// number of bytes is reserved in the current buffer - if this is not 
        /// possible because the buffer is full it will be written to disk and
        /// the secondary buffer is used.
        /// <seealso cref="EndWrite"/>
        /// </remarks>
        /// <param name="nBytes">Number of bytes to reserve.</param>
        /// <param name="nIndex">Return index where to store the data.</param>
        /// <returns>Some buffer where to store the data.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Number of bytes
        /// is negative or exceeds <see cref="Array.Length"/> of <see cref="m_Buf1"/>.
        /// </exception>
        public byte[] StartWrite( int nBytes, out int nIndex )
        {
            // Invalid
            if ((nBytes < 0) || (nBytes > m_Buf1.Length))
                throw new ArgumentOutOfRangeException( "Invalid Buffer Size - only 0 to " + m_Buf1.Length.ToString() + " are allowed" );

            // Done
            bool bFinish = (0 == nBytes);

            // See if this fits into the buffer
            if (!bFinish)
                if ((m_Buf1Pos + nBytes) <= m_Buf1.Length)
                {
                    // Report index
                    nIndex = m_Buf1Pos;

                    // Report buffer
                    return m_Buf1;
                }

            // Write out buffers
            FinishDiskWrite( !bFinish );

            // Start writing
            if (m_Buf1Pos > 0)
            {
                // Start processing
                m_Writer = m_File.BeginWrite( m_Buf1, 0, m_Buf1Pos, null, null );
            }

            // Restart from scratch
            m_Buf1Pos = 0;

            // Swap buffers
            byte[] pSwap = m_Buf1;
            m_Buf1 = m_Buf2;
            m_Buf2 = pSwap;

            // Reset index
            nIndex = 0;

            // Report
            return m_Buf1;
        }

        /// <summary>
        /// Überträgt einen beliebigen Speicherbereich in die angeschlossene Datei.
        /// </summary>
        /// <param name="buffer">Der Speicherbereich.</param>
        /// <param name="offset">Der Index des ersten zu berücksichtigenden Bytes im Speicherbereich.</param>
        /// <param name="length">Die Anzahl der zu berücksichtigenden Bytes.</param>
        public void Write( byte[] buffer, int offset, int length )
        {
            // Process
            while (length > 0)
            {
                // See how many bytes can be written
                int trans = Math.Min( length, m_Buf1.Length - m_Buf1Pos );

                // Copy in
                if (trans > 0)
                {
                    // Fill buffer
                    Array.Copy( buffer, offset, m_Buf1, m_Buf1Pos, trans );

                    // Adjust
                    m_Buf1Pos += trans;

                    // Prepare for next
                    offset += trans;
                    length -= trans;

                    // Advance counter
                    TotalBytesWritten += trans;
                }

                // May need to flush
                if (m_Buf1Pos >= m_Buf1.Length)
                {
                    // Write out buffers
                    FinishDiskWrite( true );

                    // Always start a new write
                    m_Writer = m_File.BeginWrite( m_Buf1, 0, m_Buf1Pos, null, null );

                    // Restart from scratch
                    m_Buf1Pos = 0;

                    // Swap buffers
                    byte[] swap = m_Buf1;
                    m_Buf1 = m_Buf2;
                    m_Buf2 = swap;
                }
            }
        }

        /// <summary>
        /// Call <see cref="StartWrite"/> with zero bytes followed by
        /// <see cref="FinishDiskWrite"/> to save all data to disk 
        /// and synchronize with the last asynchronous operation.
        /// </summary>
        public void Flush()
        {
            // Helper
            int nIndex;

            // Force write of last chunk
            StartWrite( 0, out nIndex );

            // Wait for final data block
            FinishDiskWrite( false );
        }

        /// <summary>
        /// Finish all file operations and close file.
        /// <seealso cref="Flush"/>
        /// </summary>
        public void Dispose()
        {
            // Check mode
            if (null != m_File)
            {
                // Finish all
                Flush();

                // Close the file
                m_File.Close();

                // Detach all
                m_File = null;
            }
        }
    }
}
