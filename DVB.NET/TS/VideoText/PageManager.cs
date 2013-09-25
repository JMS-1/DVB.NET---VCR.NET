using System.Collections.Generic;

namespace JMS.DVB.TS.VideoText
{
    /// <summary>
    /// Verwaltet die Videotext Seiten einer Ausstrahlung.
    /// </summary>
    public class PageManager
    {
        /// <summary>
        /// Die zugehörige Analyseeinheit.
        /// </summary>
        private TTXParser m_Parser = new TTXParser();

        /// <summary>
        /// Alle vorgehaltenen Seiten.
        /// </summary>
        private Dictionary<int, TTXPage> m_Pages = new Dictionary<int, TTXPage>();

        /// <summary>
        /// Die Seite, die gerade angezeigt wird.
        /// </summary>
        private int? m_CurrentPage;

        /// <summary>
        /// Zur Benachrichtigung, wenn sich die aktuelle Seite verändert hat.
        /// </summary>
        public event TTXParser.PageHandler OnPageAvailable;

        /// <summary>
        /// Erzeugt eine neue Verwaltungseinheit.
        /// </summary>
        public PageManager()
        {
            // Install handler
            m_Parser.OnPage += PageAnalysed;
        }

        /// <summary>
        /// Nimmt eine analyiserte Seite entgegen.
        /// </summary>
        /// <param name="page">Die analysierte Seite.</param>
        private void PageAnalysed( TTXPage page )
        {
            // Replace in cache
            lock (m_Pages)
                if (m_Parser.EnableParser)
                    m_Pages[page.Page] = page;

            // Current page of interest
            int? currentPage = m_CurrentPage;

            // See if someone is interested
            if (currentPage.HasValue)
                if (currentPage.Value == page.Page)
                {
                    // Attach to handler
                    TTXParser.PageHandler handler = OnPageAvailable;

                    // Fire event
                    if (null != handler) handler( page );
                }
        }

        /// <summary>
        /// Beendet die Sammlung von Seiteninformationen.
        /// </summary>
        /// <param name="reset">Gesetzt, wenn alle vorgehaltenen Seiten entfernt werden sollen.</param>
        public void Deactivate( bool reset )
        {
            // Stop caching
            m_CurrentPage = null;

            // Stop parser
            m_Parser.EnableParser = false;

            // Clear all
            if (reset)
                lock (m_Pages)
                    m_Pages.Clear();
        }

        /// <summary>
        /// Setzt die Sammlung von Informationen fort und meldet eine bestimmte Seite, sobald diese 
        /// verfügbar ist.
        /// </summary>
        public int? CurrentPage
        {
            get
            {
                // Report
                return m_CurrentPage;
            }
            set
            {
                // Make cure that current page is not checked for
                m_CurrentPage = null;

                // Read the page
                TTXPage existing = null;
                if (value.HasValue)
                    if (null != OnPageAvailable)
                    {
                        // Try to load from cache
                        lock (m_Pages)
                            if (!m_Pages.TryGetValue( value.Value, out existing ))
                                existing = null;

                        // Create search page if not in cache
                        if (null == existing)
                            existing = new TTXPage( (value.Value / 100) % 8, value.Value % 100, 0, new byte[0] );

                        // Fire this one now
                        OnPageAvailable( existing );
                    }

                // Remember new settings
                m_CurrentPage = value;

                // Start parser - if not already done
                m_Parser.EnableParser = true;
            }
        }

        /// <summary>
        /// Fügt einen Auszug aus einem ES Videotext Datenstrom zur Analyse hinzu.
        /// </summary>
        /// <param name="startOfPacket">Gesetzt, wenn der PES Kopf am Anfang der Daten steht.</param>
        /// <param name="buf">Ein Puffer.</param>
        /// <param name="offset">Erstes zu nutzendes Byte im Puffer.</param>
        /// <param name="length">Anzahl der Bytes im Puffer.</param>
        public void AddPayload( bool startOfPacket, byte[] buf, int offset, int length )
        {
            // Simply forward
            AddPayload( startOfPacket, buf, offset, length, -1 );
        }

        /// <summary>
        /// Fügt einen Auszug aus einem ES Videotext Datenstrom zur Analyse hinzu.
        /// </summary>
        /// <param name="startOfPacket">Gesetzt, wenn der PES Kopf am Anfang der Daten steht.</param>
        /// <param name="buf">Ein Puffer.</param>
        /// <param name="offset">Erstes zu nutzendes Byte im Puffer.</param>
        /// <param name="length">Anzahl der Bytes im Puffer.</param>
        /// <param name="pts">Aktueller Zeitstempel.</param>
        public void AddPayload( bool startOfPacket, byte[] buf, int offset, int length, long pts )
        {
            // Simply forward
            m_Parser.AddPayload( startOfPacket, buf, offset, length, pts );
        }

        /// <summary>
        /// Meldet alle vorgehaltenen Seiten.
        /// </summary>
        public TTXPage[] Pages
        {
            get
            {
                // List
                List<TTXPage> pages = new List<TTXPage>();

                // Fill
                lock (m_Pages) pages.AddRange( m_Pages.Values );

                // Report
                return pages.ToArray();
            }
        }
    }
}
