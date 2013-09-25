using System;
using System.IO;
using System.Collections.Generic;


namespace JMS.DVB.DirectShow.AccessModules
{
    /// <summary>
    /// Simuliert das Arbeiten mit einer Liste von Dateien.
    /// </summary>
    public class MultiFileStream : IVirtualStream
    {
        /// <summary>
        /// Alle Dateien, die hier verwaltet werden.
        /// </summary>
        private List<FileInfo> m_Files = new List<FileInfo>();

        /// <summary>
        /// Die letzte Datei der Liste, die immer geöffnet ist.
        /// </summary>
        private FileStream m_LastFile;

        /// <summary>
        /// Die aktuell geöffnete Datei.
        /// </summary>
        private FileStream m_CurrentFile;

        /// <summary>
        /// Der Name des Datenstroms abgeleitet aus dem Namen der ersten Datei.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Die aktuelle Position in der Gesamtdatei.
        /// </summary>
        private long m_Position;

        /// <summary>
        /// Erzeugt eine neue Verwaltungseinheit.
        /// </summary>
        /// <param name="streams">Die bereitgestellten Datenströme.</param>
        /// <exception cref="ArgumentNullException">Eine Datei wurde nicht angegeben.</exception>
        /// <exception cref="ArgumentException">Eine der angebenen Dateien existiert nicht.</exception>
        public MultiFileStream( IEnumerable<FileInfo> streams )
        {
            // Load all
            if (streams != null)
                m_Files.AddRange( streams );

            // Validate
            foreach (var file in m_Files)
                if (file == null)
                    throw new ArgumentNullException( "streams" );

            // Open the last
            if (m_Files.Count > 0)
                m_LastFile = Open( m_Files[m_Files.Count - 1] );

            // Set the name
            if (m_Files.Count > 0)
                Name = Path.GetFileNameWithoutExtension( m_Files[0].Name );
            else
                Name = string.Empty;
        }

        /// <summary>
        /// Öffnet eine Datei im korrekten Zugriffsmodus.
        /// </summary>
        /// <param name="path">Der volle Pfad zur Datei.</param>
        /// <returns>Die gewünschte Datei.</returns>
        private FileStream Open( FileInfo path )
        {
            // Forward
            return new FileStream( path.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 10000000 );
        }

        /// <summary>
        /// Ermittelt die gesamte Länge.
        /// </summary>
        /// <param name="count">Die Anzahl der Dateien, die berücksichtigt werden sollen.</param>
        /// <returns>Die gewünschte Länge.</returns>
        private long GetLength( int count )
        {
            // Overall counter
            long all = 0;

            // All but the last - may be growing
            for (int i = 0, imax = Math.Min( count, m_Files.Count - 1 ); i < imax; i++)
                all += m_Files[i].Length;

            // The last
            if (count >= m_Files.Count)
                if (m_LastFile != null)
                    all += m_LastFile.Length;

            // Report
            return all;
        }

        /// <summary>
        /// Ergänzt einen Datenstrom.
        /// </summary>
        /// <param name="stream">Ein beliebiger Datenstrom.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Datenstrom angegeben.</exception>
        public void Add( FileInfo stream )
        {
            // Validate
            if (stream == null)
                throw new ArgumentNullException( "stream" );

            // Open it
            var lastStream = Open( stream );
            try
            {
                // Just append one
                m_Files.Add( stream );

                // Reopen the last file
                using (m_LastFile)
                    m_LastFile = lastStream;
            }
            catch
            {
                // Release ressources
                lastStream.Dispose();

                // Forward
                throw;
            }

            // Update the name
            if (m_Files.Count == 1)
                Name = Path.GetFileNameWithoutExtension( stream.Name );
        }

        #region IVirtualStream Members

        /// <summary>
        /// Liest Daten ab der aktuellen Position.
        /// </summary>
        /// <param name="buffer">Ein Speicherbereich, der zu befüllen ist.</param>
        /// <param name="offset">Die Position des ersten Bytes, das befüllt werden darf.</param>
        /// <param name="length">Die maximale Anzahl von zu befüllenden Bytes.</param>
        /// <returns>Die tatsächlich Anzahl der übertragenen Bytes.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Speicherbereich angegeben.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Es wurde ein ungültiger Bereich angegeben.</exception>
        public int Read( byte[] buffer, int offset, int length )
        {
            // Validate
            if (length < 0)
                throw new ArgumentOutOfRangeException( "length" );
            if (offset < 0)
                throw new ArgumentOutOfRangeException( "offset" );

            // Validate
            if (buffer == null)
            {
                // Only allowed when all parameters are zero - assume zero sized buffer
                if (length > 0)
                    throw new ArgumentNullException( "buffer" );
                if (offset > 0)
                    throw new ArgumentOutOfRangeException( "offset" );
            }
            else
            {
                // Check range
                if (offset > buffer.Length)
                    throw new ArgumentOutOfRangeException( "offset" );
                if (checked( offset + length ) > buffer.Length)
                    throw new ArgumentOutOfRangeException( "offset" );
            }

            // None
            if (m_LastFile == null)
                return 0;

            // Data read
            int read = 0;

            // Process
            while (length > 0)
            {
                // Get the current file
                while (m_CurrentFile == null)
                {
                    // Position left to scan
                    long position = m_Position;

                    // Try all but the last
                    for (int i = 0; i < m_Files.Count - 1; i++)
                    {
                        // Attach to the file
                        var file = m_Files[i];

                        // Check mode
                        if (position >= file.Length)
                            position -= file.Length;
                        else
                        {
                            // Be safe
                            try
                            {
                                // Open this one
                                m_CurrentFile = Open( file );
                            }
                            catch (IOException)
                            {
                                // Remove this chunk from the position counter
                                m_Position = GetLength( i );

                                // Remove from list
                                m_Files.RemoveAt( i );

                                // Try again without it
                                position = -1;
                            }

                            // Done
                            break;
                        }
                    }

                    // Retry
                    if (position < 0)
                        continue;

                    // Open last
                    if (m_CurrentFile == null)
                        if (position > m_LastFile.Length)
                            break;
                        else
                            m_CurrentFile = m_LastFile;

                    // Position modulo
                    m_CurrentFile.Position = position;
                }

                // End reached
                if (m_CurrentFile == null)
                    break;

                // Load some stuff
                int part = m_CurrentFile.Read( buffer, offset, length );

                // See if we got something - if not skip to the next file if not already at the very end
                if (part > 0)
                {
                    // Advance local pointers
                    offset += part;
                    length -= part;
                    read += part;

                    // Advance overall pointer
                    m_Position += part;
                }
                else if (ReferenceEquals( m_CurrentFile, m_LastFile ))
                    break;
                else
                    CloseCurrent();
            }

            // For now
            return read;
        }

        /// <summary>
        /// Meldet oder setzt die aktuelle Position im Datenstrom.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Die angegebene Position ist unzulässig.</exception>
        public long Position
        {
            get
            {
                // For now
                return m_Position;
            }
            set
            {
                // Validate
                if (value < 0)
                    throw new ArgumentOutOfRangeException( "value" );
                else if (value > Length)
                    throw new ArgumentOutOfRangeException( "value" );

                // Change
                m_Position = value;

                // Reload the current file
                CloseCurrent();
            }
        }

        /// <summary>
        /// Schließt die aktuelle Datei.
        /// </summary>
        private void CloseCurrent()
        {
            // Process
            if (m_CurrentFile != null)
                try
                {
                    // May just be a reference
                    if (!ReferenceEquals( m_CurrentFile, m_LastFile ))
                        m_CurrentFile.Dispose();
                }
                finally
                {
                    // Forget
                    m_CurrentFile = null;
                }
        }

        /// <summary>
        /// Meldet die aktuelle Größe des Datenstroms.
        /// </summary>
        public long Length
        {
            get
            {
                // All we have
                return GetLength( m_Files.Count );
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        public void Dispose()
        {
            // Forget files
            m_Files = null;

            // Close current stream
            CloseCurrent();

            // Close last stream
            using (m_LastFile)
                m_LastFile = null;
        }

        #endregion
    }
}
