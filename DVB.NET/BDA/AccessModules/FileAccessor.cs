using System;
using System.IO;
using System.Threading;


namespace JMS.DVB.DirectShow.AccessModules
{
    /// <summary>
    /// Übermittelt den Inhalt einer <i>Transport Stream</i> Datei in einen <i>Direct Show</i>
    /// Graphen.
    /// </summary>
    public class FileAccessor : TransportStreamAccessor
    {
        /// <summary>
        /// Verwaltet eine einzelne Datei.
        /// </summary>
        private class _SingleFileVirtualStream : IVirtualStream
        {
            /// <summary>
            /// Der aktuelle Datenstrom.
            /// </summary>
            private volatile FileStream m_Stream;

            /// <summary>
            /// Erzeugt die Verwaltung einer einzelnen Datei.
            /// </summary>
            /// <param name="path">Der volle Pfad zu Datei.</param>
            public _SingleFileVirtualStream( string path )
            {
                // Attach to file
                m_Stream = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 10000000 );
            }

            /// <summary>
            /// Meldet den Namen dieses Datenstroms.
            /// </summary>
            public string Name
            {
                get
                {
                    // Attach to the stream
                    var stream = m_Stream;

                    // Report
                    if (stream == null)
                        return string.Empty;
                    else
                        return Path.GetFileNameWithoutExtension( stream.Name );
                }
            }

            /// <summary>
            /// Meldet die aktuelle Position im Datenstrom.
            /// </summary>
            public long Position
            {
                get
                {
                    // Attach to the stream
                    var stream = m_Stream;

                    // Report
                    if (stream == null)
                        return 0;
                    else
                        return stream.Position;
                }
                set
                {
                    // Attach to the stream
                    var stream = m_Stream;

                    // Forward
                    if (stream != null)
                        stream.Position = value;
                }
            }

            /// <summary>
            /// Meldet die aktuelle Größe des Datenstroms.
            /// </summary>
            public long Length
            {
                get
                {
                    // Attach to the stream
                    var stream = m_Stream;

                    // Report
                    if (stream == null)
                        return 0;
                    else
                        return stream.Length;
                }
            }

            /// <summary>
            /// Liest Daten ab der aktuellen Position.
            /// </summary>
            /// <param name="buffer">Ein Speicherbereich, der zu befüllen ist.</param>
            /// <param name="offset">Die Position des ersten Bytes, das befüllt werden darf.</param>
            /// <param name="length">Die maximale Anzahl von zu befüllenden Bytes.</param>
            /// <returns>Die tatsächlich Anzahl der übertragenen Bytes.</returns>
            public int Read( byte[] buffer, int offset, int length )
            {
                // Attach to the stream
                var stream = m_Stream;

                // Report
                if (stream == null)
                    return 0;
                else
                    return stream.Read( buffer, offset, length );
            }

            /// <summary>
            /// Beendet die Nutzung dieser Instanz endgültig.
            /// </summary>
            public void Dispose()
            {
                // Just forward
                using (m_Stream)
                    m_Stream = null;
            }
        }

        /// <summary>
        /// Zwischenspeicher zum Auslesen der Datei.
        /// </summary>
        private byte[] m_Buffer = new byte[100000];

        /// <summary>
        /// Der aktuelle Datenstrom.
        /// </summary>
        private IVirtualStream m_Stream;

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz.
        /// </summary>
        /// <param name="path">Der volle Pfad zur anzuzeigenden Datei.</param>
        public FileAccessor( string path )
            : this( new _SingleFileVirtualStream( path ) )
        {
        }

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz.
        /// </summary>
        /// <param name="stream">Die Verwaltung eines Datenstroms.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Datenstrom angegeben.</exception>
        public FileAccessor( IVirtualStream stream )
        {
            // Validate
            if (stream == null)
                throw new ArgumentNullException( "stream" );

            // Remember
            m_Stream = stream;

            // Must be called periodically
            SetExternalFeed( false );
        }

        /// <summary>
        /// Ändert den abzuspielenden Datenstrom.
        /// </summary>
        /// <param name="stream">Der gewünschte neue Datenstrom.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Datenstrom angegeben.</exception>
        public void ChangeStream( IVirtualStream stream )
        {
            // Validate
            if (stream == null)
                throw new ArgumentNullException( "stream" );

            // Change
            var oldStream = Interlocked.Exchange( ref m_Stream, stream );

            // Release
            if (oldStream != null)
                lock (oldStream)
                    oldStream.Dispose();
        }

        /// <summary>
        /// Meldet den Namen der angezeigten Datei ohne Dateierweiterung und Verzeichnisname.
        /// </summary>
        public string Name
        {
            get
            {
                // Attach to the stream
                var stream = m_Stream;

                // Report
                if (stream == null)
                    return string.Empty;
                else
                    return stream.Name;
            }
        }

        /// <summary>
        /// Beendet alle Aktivitäten dieses Zugriffsmoduls.
        /// </summary>
        protected override void OnDispose()
        {
            // Forward - will stop the feed
            base.OnDispose();

            // Finished with the file
            using (m_Stream)
                m_Stream = null;
        }

        /// <summary>
        /// Ermittelt den nächsten Datenblock von Transport Stream Paketen
        /// zur Übertragung in den DirectShow Graphen.
        /// </summary>
        /// <param name="video">Abfrage der Videodaten.</param>
        /// <returns>Zwischenspeicher, dessen erste Bytes zu verwenden sind.</returns>
        protected override byte[] GetNextChunk( bool video )
        {
            // Audio only
            if (video)
                return base.GetNextChunk( video );

            // Check the stream time offset
            long? offset = StreamTimeOffset;

            // Do not send any data - we are more than one second ahead
            if (offset.GetValueOrDefault() >= 10000000)
                return null;

            // Get the next chunk
            byte[] chunk = base.GetNextChunk( video );

            // Fill up
            while ((chunk == null) && IsRunning)
            {
                // Load the stream
                var stream = m_Stream;
                if (stream == null)
                    break;

                // Number of bytes
                int n;

                // Load - protect against changing the file position while reading
                lock (stream)
                    n = stream.Read( m_Buffer, 0, m_Buffer.Length );

                // File is at the end - at least for now
                if (n < 1)
                    break;

                // Push into file
                AddPayload( m_Buffer, 0, n );

                // Forward
                chunk = base.GetNextChunk( video );
            }

            // Report as is
            return chunk;
        }

        /// <summary>
        /// Liest oder setzt die aktuelle (relative) Position in der Datei.
        /// </summary>
        public double Position
        {
            get
            {
                // Attach to the stream
                var stream = m_Stream;

                // Already done
                if (stream == null)
                    return 0;

                // Data
                long pos, len;

                // Proctect
                lock (stream)
                {
                    // Load
                    pos = stream.Position;
                    len = stream.Length;
                }

                // Empty
                if (pos < 0)
                    return 0;
                if (len < 1)
                    return 0;

                // Save check
                if (pos > len)
                    return 1;
                else
                    return pos * 1.0 / len;
            }
            set
            {
                // Validate
                if ((value < 0.0) || (value > 1.0))
                    throw new ArgumentOutOfRangeException( "value" );

                // Attach to the stream
                var stream = m_Stream;

                // None
                if (stream == null)
                    return;

                // Protect against reading the file while moving the pointer
                lock (stream)
                    stream.Position = (long) (value * stream.Length);
            }
        }

        /// <summary>
        /// Liest oder setzt die aktuelle (absolute) Position in der Datei.
        /// </summary>
        public long AbsolutePosition
        {
            get
            {
                // Attach to the stream
                var stream = m_Stream;

                // Already done
                if (stream == null)
                    return 0;

                // Safe report
                lock (stream)
                    return stream.Position;
            }
            set
            {
                // Validate
                if (value < 0)
                    throw new ArgumentOutOfRangeException( "value" );

                // Attach to the stream
                var stream = m_Stream;

                // None
                if (stream == null)
                    return;

                // Protect against reading the file while moving the pointer
                lock (stream)
                    if (stream.Position > stream.Length)
                        throw new ArgumentOutOfRangeException( "value" );
                    else
                        stream.Position = value;
            }
        }
    }
}
