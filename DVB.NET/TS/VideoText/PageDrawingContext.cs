using System;
using System.Drawing;
using System.Collections.Generic;

namespace JMS.DVB.TS.VideoText
{
    /// <summary>
    /// Hilfsklasse zum Aufbau einer Videotext Seite.
    /// </summary>
    internal class PageDrawingContext : IDisposable
    {
        /// <summary>
        /// Die Zeichenfläche für die Videotext Seite.
        /// </summary>
        private Bitmap m_Bitmap;

        /// <summary>
        /// Die Größe eines einzelnen Zeichens.
        /// </summary>
        public readonly SizeF NormalCharacterSize;

        /// <summary>
        /// Die Größe eines Mosaic Bausteins - ein Zeichen hat 2 Bausteine horizontal und drei vertikal.
        /// </summary>
        public readonly SizeF MosaicElementSize;

        /// <summary>
        /// Das Zeichenfeld als GDI Instanz.
        /// </summary>
        private Graphics m_Graphics;

        /// <summary>
        /// Gesetzt, wenn der Hintergrund transparent gezeichnet werden soll.
        /// </summary>
        public readonly bool IsTransparent;

        /// <summary>
        /// Alle Zahlen auf dieser Seite.
        /// </summary>
        private List<KeyValuePair<RectangleF, int>> m_Digits = new List<KeyValuePair<RectangleF, int>>();

        /// <summary>
        /// Der Hintergrund der Seite.
        /// </summary>
        public readonly Brush Background;

        /// <summary>
        /// Die Verwaltung aller Ziffern auf der Seite.
        /// </summary>
        public readonly DigitManager Digits;

        /// <summary>
        /// Die gesamte benutzte Fläche.
        /// </summary>
        private RectangleF m_Extend;

        /// <summary>
        /// Erzeugt eine neue Instanz der Hilfsklasse.
        /// </summary>
        /// <param name="g">Die Zeichenumgebung, in der die Videotext Seite integriert werden muss.</param>
        /// <param name="isTransparent">Gesetzt, wenn die Seite durchscheinend sein soll.</param>
        public PageDrawingContext( Graphics g, bool isTransparent )
        {
            // Remember 
            IsTransparent = isTransparent;

            // Measure the size of a letter and a mosaic brick
            NormalCharacterSize = g.MeasureString( "X", LineDrawingContext.NormalTextFont );
            MosaicElementSize = new SizeF( NormalCharacterSize.Width / 2, NormalCharacterSize.Height / 3 );

            // Create the bitmap
            m_Bitmap = new Bitmap( (int) (40 * NormalCharacterSize.Width), (int) (26 * NormalCharacterSize.Height), g );

            // Create digit manager
            Digits = new DigitManager( m_Bitmap );

            // Create the drawing backbone
            m_Graphics = Graphics.FromImage( m_Bitmap );

            // Remember background
            Background = isTransparent ? TTXPage.TransparentColor : LineDrawingContext.Brushes[Color.Black];

            // Prepare the page
            m_Graphics.FillRectangle( Background, 0, 0, m_Bitmap.Width, m_Bitmap.Height );
        }

        /// <summary>
        /// Meldet den Zeichenbereich.
        /// </summary>
        public Graphics Graphics
        {
            get
            {
                // Report
                return m_Graphics;
            }
        }

        /// <summary>
        /// Zeichnet eine Videotext Zeile.
        /// </summary>
        /// <param name="line">Nummer der Zeile von 1 bis 25.</param>
        /// <param name="data">Daten zur Zeile.</param>
        /// <returns>Informationen zur ausgegebenen Zeile.</returns>
        public LineDrawingContext DrawLine( int line, byte[] data )
        {
            // Forward
            return DrawLine( line, data, null );
        }

        /// <summary>
        /// Zeichnet die Videotext Überschrift.
        /// </summary>
        /// <param name="prefix">Präfix mit Daten zu Seite.</param>
        /// <param name="data">Daten der Überschrift.</param>
        /// <returns>Informationen zur ausgegebenen Zeile.</returns>
        public LineDrawingContext DrawHeader( string prefix, byte[] data )
        {
            // Forward
            return DrawLine( 0, data, prefix );
        }

        /// <summary>
        /// Zeichnet eine Videotext Zeile.
        /// </summary>
        /// <param name="line">Die Nummer der Zeile von 0 bis 25.</param>
        /// <param name="data">Die Daten zur Zeile.</param>
        /// <param name="prefix">Optionale Präfixdaten zur Zeile.</param>
        /// <returns>Informationen zur ausgegebenen Zeile.</returns>
        private LineDrawingContext DrawLine( int line, byte[] data, string prefix )
        {
            // Create context
            LineDrawingContext context = new LineDrawingContext( this );

            // Process
            context.Draw( line, data, prefix );

            // Report result
            return context;
        }

        /// <summary>
        /// Meldet, dass ein bestimmter Bereich beschrieben wurde.
        /// </summary>
        /// <param name="rect">Der Bereich, der beschrieben wurde.</param>
        internal void ReportUsage( RectangleF rect )
        {
            // Check mode
            if (m_Extend.IsEmpty)
            {
                // All of it
                m_Extend = rect;
            }
            else
            {
                // New bounds
                var left = Math.Min( m_Extend.Left, rect.Left );
                var right = Math.Max( m_Extend.Right, rect.Right );
                var top = Math.Min( m_Extend.Top, rect.Top );
                var bottom = Math.Max( m_Extend.Bottom, rect.Bottom );

                // Update
                m_Extend = new RectangleF( left, top, right - left, bottom - top );
            }
        }

        /// <summary>
        /// Meldet die aktuelle Nutzung der Zeichenfläche.
        /// </summary>
        internal RectangleF Extend
        {
            get
            {
                // Check mode
                if (m_Extend.IsEmpty)
                    return new RectangleF( 0, 0, 0, 0 );
                else
                    return
                        new RectangleF
                            (
                                Math.Max( 0, Math.Min( 1, m_Extend.Left / m_Bitmap.Width ) ),
                                Math.Max( 0, Math.Min( 1, m_Extend.Top / m_Bitmap.Height ) ),
                                Math.Max( 0, Math.Min( 1, m_Extend.Width / m_Bitmap.Width ) ),
                                Math.Max( 0, Math.Min( 1, m_Extend.Height / m_Bitmap.Height ) )
                            );
            }
        }

        /// <summary>
        /// Meldet die aktuelle Zeichenfläche.
        /// </summary>
        /// <returns>Die vorbereitete Videotextseite.</returns>
        public Bitmap Detach()
        {
            try
            {
                // Report
                return m_Bitmap;
            }
            finally
            {
                // Do not cleanup
                m_Bitmap = null;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Hilfsklasse endgültig und gibt alle verbundenen
        /// GDI Ressourcen frei.
        /// </summary>
        void IDisposable.Dispose()
        {
            // Cleanup
            if (null != m_Graphics)
            {
                // Release resources
                m_Graphics.Dispose();

                // Forget
                m_Graphics = null;
            }
            if (null != m_Bitmap)
            {
                // Release resources
                m_Bitmap.Dispose();

                // Forget
                m_Bitmap = null;
            }
        }

        #endregion
    }
}
