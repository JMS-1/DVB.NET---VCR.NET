using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;


namespace JMS.DVB.TS.VideoText
{
    /// <summary>
    /// Der Inhalt einer Videotext Seite.
    /// </summary>
    public class TTXPage
    {
        /// <summary>
        /// Die Transparenzfarbe.
        /// </summary>
        public static readonly SolidBrush TransparentColor = new SolidBrush( Color.FromArgb( 0, 0, 127 ) );

        /// <summary>
        /// Die Magazinnummer der Seite (0 bis 7).
        /// </summary>
        public readonly int Magazine;

        /// <summary>
        /// Die Seitennummer im Magazin (0 bis 99).
        /// </summary>
        public readonly int RelativePage;

        /// <summary>
        /// Die übliche Seitennummer (100 bis 899).
        /// </summary>
        public readonly int Page;

        /// <summary>
        /// Die Teilseite.
        /// </summary>
        public readonly int SubPage;

        /// <summary>
        /// Die Kopfzeile der Seite.
        /// </summary>
        public readonly byte[] Header;

        /// <summary>
        /// Enthält die Daten der einzelnen Zeilen.
        /// </summary>
        private Dictionary<int, byte[]> m_Lines = new Dictionary<int, byte[]>();

        /// <summary>
        /// Seite löschen.
        /// </summary>
        public bool Erase = false;

        /// <summary>
        /// Seite blinken lassen.
        /// </summary>
        public bool Flash = false;

        /// <summary>
        /// Das ist eine Untertitelseite.
        /// </summary>
        public bool IsSubtitle = false;

        /// <summary>
        /// Kopfzeile ausblenden.
        /// </summary>
        public bool NoHeader = false;

        /// <summary>
        /// Die Seite hat sich seit der letzten Darstellung verändert.
        /// </summary>
        public bool Updated = false;

        /// <summary>
        /// Die Seite wurde unterbrochen.
        /// </summary>
        public bool OutOfSequence = false;

        /// <summary>
        /// Die Seite soll nicht dargestellt werden.
        /// </summary>
        public bool Hide = false;

        /// <summary>
        /// ?
        /// </summary>
        public bool MagazineSerial = false;

        /// <summary>
        /// Auswahl eines nationalen Zeichensatzes.
        /// </summary>
        public int NationalOptions = 0;

        /// <summary>
        /// Der zugehörige Zeitstempel, sofern bekannt.
        /// </summary>
        public long TimeStamp = -1;

        /// <summary>
        /// Optional Informationen zur aktuellen Eingabe durch den Anwender.
        /// </summary>
        public string Feedback { get; set; }

        /// <summary>
        /// Erzeugt eine Videotextseite.
        /// </summary>
        /// <param name="magazine">Die Nummer des Magazins (0 bis 7).</param>
        /// <param name="page">Die Nummer der Seite im Magazin (0 bis 99).</param>
        /// <param name="subcode">Die Nummer der Teilseite.</param>
        /// <param name="header">Die Kopfzeile der Seite.</param>
        public TTXPage( int magazine, int page, int subcode, byte[] header )
        {
            // Remember
            Magazine = magazine;
            RelativePage = page;
            SubPage = subcode;
            Header = header;

            // Get page
            Page = 100 * Magazine + RelativePage;

            // Correct
            if (Page < 100) Page += 800;
        }

        /// <summary>
        /// Liest oder setzt eine einzelne Zeile der Seite.
        /// </summary>
        /// <param name="line">Die gewünschte Zeile.</param>
        /// <returns>Der Inhalt der gewünschten Zeile.</returns>
        public byte[] this[int line]
        {
            get
            {
                // Try to load
                byte[] result;
                if (m_Lines.TryGetValue( line, out result )) return result;

                // Empty line
                return null;
            }
            set
            {
                // Remember
                m_Lines[line] = value;
            }
        }

        /// <summary>
        /// Erzeugt eine textuelle Darstellung dieser Seite (nur die Kopfzeile).
        /// </summary>
        /// <returns>Eine Kurzbeschreibung der Seite.</returns>
        public override string ToString()
        {
            // Report
            return string.Format( "{0} {1}", UniqueIdentifier, TTXParser.GetDebugString( Header ) );
        }

        /// <summary>
        /// Erstellt eine eindeutige Identifikation dieser Seite aus Seitenummer <see cref="Page"/> und
        /// Nummer der Unterseite <see cref="SubPage"/>.
        /// </summary>
        public string UniqueIdentifier
        {
            get
            {
                // Report
                return string.Format( "{0}/{1:0000}", Page, SubPage );
            }
        }

        /// <summary>
        /// Meldet, ob <see cref="CreatePage(Control, out DigitManager, out RectangleF)"/> ein transparentes Bild erstellt - die Transparenzfarbe
        /// ist dann <see cref="TransparentColor"/>.
        /// </summary>
        public bool IsTransparent
        {
            get
            {
                // Report
                return (IsSubtitle || Flash);
            }
        }

        /// <summary>
        /// Erzeugt die aktuelle Seite als <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="holder">Ein Anzeigeelement als Referenz für die Parameter der <see cref="Bitmap"/>.</param>
        /// <returns>Eine Darstellung der aktuellen Videotext Seite.</returns>
        public Bitmap CreatePage( Control holder )
        {
            // Helper
            DigitManager digits;
            RectangleF extend;

            // Forward
            return CreatePage( holder, out digits, out extend );
        }

        /// <summary>
        /// Erzeugt die aktuelle Seite als <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="holder">Ein Anzeigeelement als Referenz für die Parameter der <see cref="Bitmap"/>.</param>
        /// <param name="digits">Alle auf der Seite verwendeten Zahlen.</param>
        /// <returns>Eine Darstellung der aktuellen Videotext Seite.</returns>
        public Bitmap CreatePage( Control holder, out DigitManager digits )
        {
            // Helper
            RectangleF extend;

            // Forward
            return CreatePage( holder, out digits, out extend );
        }

        /// <summary>
        /// Erzeugt die aktuelle Seite als <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="holder">Ein Anzeigeelement als Referenz für die Parameter der <see cref="Bitmap"/>.</param>
        /// <param name="digits">Alle auf der Seite verwendeten Zahlen.</param>
        /// <param name="extend">Beschreibt den tatsächlich relevanten Teil der Seite.</param>
        /// <returns>Eine Darstellung der aktuellen Videotext Seite.</returns>
        public Bitmap CreatePage( Control holder, out DigitManager digits, out RectangleF extend )
        {
            // Create device context
            using (var referenceContext = Graphics.FromHwnd( holder.Handle ))
            using (var pageDraw = new PageDrawingContext( referenceContext, IsTransparent ))
            {
                // Process header
                if (!IsTransparent)
                    if (!NoHeader)
                    {
                        // Get the prefix for the headline
                        string prefix;
                        if (!string.IsNullOrEmpty( Feedback ))
                            prefix = string.Format( "{0}       ", Feedback ).Substring( 0, 8 );
                        else if (SubPage < 1)
                            prefix = string.Format( "{0:000}     ", Page );
                        else
                            prefix = string.Format( "{0:000}/{1:00}  ", Page, SubPage ).Substring( 0, 8 );

                        // Fill the headline
                        pageDraw.DrawHeader( prefix, Header );
                    }

                // Fill the body
                if (!Hide)
                    for (int i = 0; ++i < 26; )
                    {
                        // Line data
                        byte[] line;
                        if (!m_Lines.TryGetValue( i, out line ))
                            continue;

                        // Fill the line
                        LineDrawingContext context = pageDraw.DrawLine( i, line );

                        // Skip line if double height characters are used
                        if (context.HasDoubleHeight)
                            i++;
                    }

                // Attach to digit manager of context
                digits = pageDraw.Digits;
                extend = pageDraw.Extend;

                // Report
                return pageDraw.Detach();
            }
        }
    }
}