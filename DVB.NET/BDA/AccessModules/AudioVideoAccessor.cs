using System;
using System.Threading;


namespace JMS.DVB.DirectShow.AccessModules
{
    /// <summary>
    /// Über dieses Zugriffsmodul werden Bild und Ton über <i>Elementary
    /// Streams</i> direkt in den Graphen eingespeist.
    /// </summary>
    public class AudioVideoAccessor : AccessModule
    {
        /// <summary>
        /// Zwischenspeicher zur Entkoppelung des eingehenden Audiostroms von
        /// der Anzeige im DirectShow Graphen. 
        /// </summary>
        private byte[] m_AudioBuffer = new byte[1000000];

        /// <summary>
        /// Aktuelle Anzahl von gesammelten Bytes aus dem eingehenden Audiostrom.
        /// </summary>
        private int m_AudioBufferPos = 0;

        /// <summary>
        /// Zwischenspeicher zur Entkoppelung des eingehenden Videostroms von
        /// der Anzeige im DirectShow Graphen. 
        /// </summary>
        private byte[] m_VideoBuffer = new byte[10000000];

        /// <summary>
        /// Aktuelle Anzahl von gesammelten Bytes aus dem eingehenden Audiostrom.
        /// </summary>
        private int m_VideoBufferPos = 0;

        /// <summary>
        /// Erzeugt ein neues Zugriffsmodul.
        /// </summary>
        public AudioVideoAccessor()
        {
            // Guess we are fed
            SetExternalFeed( true );
        }

        /// <summary>
        /// Audiodaten übernehmen.
        /// </summary>
        /// <param name="data">Datenblock.</param>
        public void AddAudio( byte[] data )
        {
            // Forward
            AddAudio( data, 0, data.Length );
        }

        /// <summary>
        /// Videodaten übernehmen.
        /// </summary>
        /// <param name="data">Datenblock.</param>
        public void AddVideo( byte[] data )
        {
            // Forward
            AddVideo( data, 0, data.Length );
        }

        /// <summary>
        /// Audiodaten übernehmen.
        /// </summary>
        /// <param name="data">Datenblock.</param>
        /// <param name="offset">Erstes zu nutzendes Bytes.</param>
        /// <param name="length">Anzahl der zu nutzenden Bytes.</param>
        public void AddAudio( byte[] data, int offset, int length )
        {
            // Forward
            if (Receive( data, offset, length, m_AudioBuffer, ref m_AudioBufferPos ))
                ReportAudioAvailable();
        }

        /// <summary>
        /// Videodaten übernehmen.
        /// </summary>
        /// <param name="data">Datenblock.</param>
        /// <param name="offset">Erstes zu nutzendes Bytes.</param>
        /// <param name="length">Anzahl der zu nutzenden Bytes.</param>
        public void AddVideo( byte[] data, int offset, int length )
        {
            // Forward
            if (Receive( data, offset, length, m_VideoBuffer, ref m_VideoBufferPos ))
                ReportVideoAvailable();
        }

        /// <summary>
        /// Überträgt Daten aus dem eingehenden Datenstrom in den
        /// internen Zwischenspeicher. Hat dieser einen vordefinierten minimalen
        /// Füllgrad erreicht, so wird ein Ereignis gesetzt.
        /// </summary>
        /// <param name="package">Datenpaket beliebieger Größe.</param>
        /// <param name="offset">Erstes zu nutzendes Bytes.</param>
        /// <param name="length">Anzahl der zu nutzenden Bytes.</param>
        /// <param name="buffer">Der betroffenen Zwischenspeicher.</param>
        /// <param name="bufferPos">Aktuelle Position im Zwischenspeicher.</param>
        /// <returns>Gesetzt, wenn nun neue Daten bereitstehen.</returns>
        private bool Receive( byte[] package, int offset, int length, byte[] buffer, ref int bufferPos )
        {
            // As long as necessary
            for (; ; )
            {
                // No one wants it
                if (!IsRunning)
                    return false;

                // Do nothing
                if (IsDisposing)
                    return false;

                // Read the item
                int currentPos = Thread.VolatileRead( ref bufferPos );

                // Discard on overflow
                if ((currentPos + length) > buffer.Length)
                    return (currentPos > 0);

                // Copy over
                Array.Copy( package, offset, buffer, currentPos, length );

                // Advance
                int newPos = currentPos + length;

                // Try to write back
                if (Interlocked.CompareExchange( ref bufferPos, newPos, currentPos ) == currentPos)
                    return (newPos > 0);
            }
        }

        /// <summary>
        /// Ermittelt den nächsten Speicherbereich.
        /// </summary>
        /// <seealso cref="Receive"/>
        /// <param name="buffer">Die Verwaltung des Bereichs.</param>
        /// <param name="bufferPos">Der aktuelle Füllgrad des Bereichs.</param>
        /// <returns>Der ermittelte Speicherbereich oder <i>null</i>.</returns>
        private byte[] GetNextChunk( byte[] buffer, ref int bufferPos )
        {
            // As long as necessary
            for (; ; )
            {
                // Already done
                if (IsDisposing)
                    return null;

                // Load
                int currentPos = Thread.VolatileRead( ref bufferPos );

                // Check for minimum buffer size
                if (currentPos < 1)
                    return null;

                // Create helper
                byte[] current = new byte[currentPos];

                // Fill
                Array.Copy( buffer, current, current.Length );

                // Adjust
                if (Interlocked.CompareExchange( ref bufferPos, 0, currentPos ) == currentPos)
                    return current;
            }
        }

        /// <summary>
        /// Ermittelt den nächsten Datenblock von Transport Stream Paketen
        /// zur Übertragung in den DirectShow Graphen.
        /// </summary>
        /// <param name="video">Abfrage der Videodaten.</param>
        /// <returns>Zwischenspeicher, dessen erste Bytes zu verwenden sind.</returns>
        protected override byte[] GetNextChunk( bool video )
        {
            // Check for the type of data to provide
            if (video)
                return GetNextChunk( m_VideoBuffer, ref m_VideoBufferPos );
            else
                return GetNextChunk( m_AudioBuffer, ref m_AudioBufferPos );
        }

        /// <summary>
        /// Entfernt alle bisher gesammelten Daten aus dem Zwischenspeicher.
        /// </summary>
        public override void ClearBuffers()
        {
            // Self
            Interlocked.Exchange( ref m_AudioBufferPos, 0 );
            Interlocked.Exchange( ref m_VideoBufferPos, 0 );

            // To base
            base.ClearBuffers();
        }
    }
}
