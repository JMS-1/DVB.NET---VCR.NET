using System;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

using JMS.DVB.TS.VideoText;


namespace JMS.DVB.DirectShow.UI
{
    /// <summary>
    /// Hilfsklasse zur Konstruktion eines OSD Bildes.
    /// </summary>
    public class OSDText : IDisposable
    {
        /// <summary>
        /// Die hellste Farbe in der Umrandung.
        /// </summary>
        public static Color BrightColor = Color.LightSteelBlue;

        /// <summary>
        /// Die dunkelste Farbe in der Umrandung.
        /// </summary>
        public static Color DarkColor = Color.DarkBlue;

        /// <summary>
        /// Der Winkel für die Farbanpassung.
        /// </summary>
        public static float BrightDarkGradient = 45.0f;

        /// <summary>
        /// Das Zeichenformat für den inneren Text.
        /// </summary>
        public static Font TextFont = new Font( "Courier New", 14.0f );

        /// <summary>
        /// Die Hintergrundfarbe für selektierte Textzeilen.
        /// </summary>
        public static Brush SelectColor = new SolidBrush( Color.Gray );

        /// <summary>
        /// Die Farbe des inneren Textes.
        /// </summary>
        public static Brush TextColor = new SolidBrush( Color.White );

        /// <summary>
        /// Das Zeichenformat für die Überschrift.
        /// </summary>
        public static Font HeadFont = new Font( "Arial", 20.0f );

        /// <summary>
        /// Der innere Zeichenbereich.
        /// </summary>
        private Rectangle m_Clipping;

        /// <summary>
        /// Schnittstelle zum Füllen des Bildes.
        /// </summary>
        private Graphics m_Graphics;

        /// <summary>
        /// Aktuelle Zeile.
        /// </summary>
        private float m_YPos = 5;

        /// <summary>
        /// Fenster, in dem das OSD dargestellt werden soll.
        /// </summary>
        private OverlayWindow m_Main;

        /// <summary>
        /// Zwischenspeicher für das OSD Bild.
        /// </summary>
        private Bitmap m_Bitmap;

        /// <summary>
        /// Relative Höhe des OSD Bildes, 1.0 würde den gesamten Bildschrim füllen. 
        /// </summary>
        private double m_Size;

        /// <summary>
        /// Echtes OSD Overlay benutzen, wenn möglich.
        /// </summary>
        private bool m_UseOverlay;

        /// <summary>
        /// Die Farbe, die durchsichtig sein soll.
        /// </summary>
        private Color? m_TransparentColor = null;

        /// <summary>
        /// Bereitet ein neues OSD vor.
        /// </summary>
        /// <param name="main">Schnittstelle zur tatsächlichen Darstellung des Inhaltes.</param>
        /// <param name="size">Relative Größe zwischen 0 und 1 - der maximalen OSD Größe.</param>
        /// <param name="headline">Überschrift für die OSD Darstellung.</param>
        /// <param name="transparent">Gesetzt, wenn eine transparente Anzeige erlaubt ist.</param>
        public OSDText( OverlayWindow main, double size, string headline, bool transparent )
        {
            // Remember
            m_Size = Math.Max( 0, Math.Min( 0.88, size ) );
            m_UseOverlay = transparent;
            m_Main = main;

            // Fill the background
            Initialize( headline );
        }

        /// <summary>
        /// Erzeugt den Hintergrund des OSD.
        /// </summary>
        /// <param name="headline">Überschrift für das OSD.</param>
        private void Initialize( string headline )
        {
            // Create the scratch bitmap
            m_Bitmap = CreateBitmap( 800, (int) (750 * m_Size) );

            // Create graphics
            m_Graphics = Graphics.FromImage( m_Bitmap );

            // Get the full rectangle
            Rectangle bitmap = new Rectangle( 0, 0, m_Bitmap.Width, m_Bitmap.Height );

            // Paint outer frame
            using (var frame = new LinearGradientBrush( bitmap, BrightColor, DarkColor, BrightDarkGradient ))
            {
                // Use a transparent color
                m_TransparentColor = TTXPage.TransparentColor.Color;

                // Make transparent
                using (Brush nothing = new SolidBrush( m_TransparentColor.Value ))
                {
                    // Fill it
                    m_Graphics.FillRectangle( nothing, bitmap );
                }

                // Prepare frame
                if (OverlayWindow.UseLegacyOverlay)
                    m_Graphics.FillRectangle( frame, bitmap );
                else
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        // Bound
                        const int arcSize = 40;

                        // Paint border
                        path.AddArc( 0, 0, arcSize, arcSize, 180, 90 );
                        path.AddLine( arcSize, 0, bitmap.Width - arcSize, 0 );
                        path.AddArc( bitmap.Width - arcSize, 0, arcSize, arcSize, 270, 90 );
                        path.AddLine( bitmap.Width, arcSize, bitmap.Width, bitmap.Height - arcSize );
                        path.AddArc( bitmap.Width - arcSize, bitmap.Height - arcSize, arcSize, arcSize, 0, 90 );
                        path.AddLine( bitmap.Width - arcSize, bitmap.Height, arcSize, bitmap.Height );
                        path.AddArc( 0, bitmap.Height - arcSize, arcSize, arcSize, 90, 90 );
                        path.AddLine( 0, bitmap.Height - arcSize, 0, arcSize );

                        // Fill inner
                        m_Graphics.FillPath( frame, path );
                    }
            }

            // Calculate the inner window
            m_Clipping = new Rectangle( 20, 45, m_Bitmap.Width - 40, m_Bitmap.Height - 65 );

            // Paint inner frame
            using (SolidBrush dark = new SolidBrush( System.Diagnostics.Debugger.IsAttached ? Color.DarkGreen : Color.Black ))
            {
                // Fill it
                m_Graphics.FillRectangle( dark, m_Clipping );
            }

            // Write the headline
            if (headline != null)
                using (SolidBrush black = new SolidBrush( Color.Black ))
                {
                    // Restrict
                    m_Graphics.SetClip( new Rectangle( 20, 0, m_Bitmap.Width - 40, m_Bitmap.Height ) );

                    // Paint
                    m_Graphics.DrawString( headline, HeadFont, black, m_Clipping.X, 10 );
                }

            // Install the clipping region
            m_Graphics.SetClip( m_Clipping );

            // Set position
            m_YPos += m_Clipping.Y;
        }

        /// <summary>
        /// Schreibt eine Textzeile in einem bestimmten Format in das OSD.
        /// </summary>
        /// <param name="font">Das gewünschte Zeichenformat.</param>
        /// <param name="selected">Gesetzt, wenn die Textzeile markiert dargestellt werden soll.</param>
        /// <param name="line">Die Textzeile.</param>
        private void WriteLine( Font font, bool selected, string line )
        {
            // Get the size
            SizeF textSize = m_Graphics.MeasureString( line, font );

            // Should be selected
            if (selected)
                m_Graphics.FillRectangle( SelectColor, m_Clipping.X, m_YPos, textSize.Width, textSize.Height );

            // Forward
            m_Graphics.DrawString( line, font, TextColor, m_Clipping.X, m_YPos );

            // Move down
            m_YPos += textSize.Height;
        }

        /// <summary>
        /// Zeigt einen Fortschrittsbalken an.
        /// </summary>
        /// <param name="percentage">Füllgrad des Balkens zwischen 0.0 und 1.0.</param>
        /// <param name="primaryColor">Gesetzt, wenn die primäre Farbe <see cref="Color.Blue"/> verwendet werden soll
        /// und nicht die sekundäre Farbe <see cref="Color.Red"/>.</param>
        public void ShowProgress( double percentage, bool primaryColor )
        {
            // Inner bar
            using (Brush inner = new SolidBrush( primaryColor ? Color.Blue : Color.Red ))
            {
                // Draw it
                m_Graphics.FillRectangle( inner, m_Clipping.X + 6, m_Clipping.Y + 6, (int) (percentage * (m_Clipping.Width - 12)), m_Clipping.Height - 12 );
            }
        }

        /// <summary>
        /// Schreibt eine Textzeile in einem Zeichenstil in das OSD.
        /// </summary>
        /// <param name="style">Der gewünschte Zeichenstil.</param>
        /// <param name="selected">Gesetzt, wenn die Textzeile markiert dargestellt werden soll.</param>
        /// <param name="line">Die Textzeile.</param>
        public void WriteLine( FontStyle style, bool selected, string line )
        {
            // Show up
            if (FontStyle.Regular == style)
                WriteLine( TextFont, selected, line );
            else
                using (Font specialFont = new Font( TextFont, style ))
                    WriteLine( specialFont, selected, line );
        }

        /// <summary>
        /// Erzwingt einer Darstellung als Windows Fenster.
        /// </summary>
        public void DisableOverlay()
        {
            // Deactivate
            m_UseOverlay = false;
        }

        /// <summary>
        /// Aktiviert das erstellte OSD.
        /// </summary>
        private void Show()
        {
            // Get border
            var left = m_Main.GetNormalizedBorder( 0.96 );

            // Forward			
            m_Main.ShowOverlay( m_Bitmap, left, 0.9 - m_Size, 1.0 - left, 0.9, m_UseOverlay ? (double?) 0.8 : null, m_TransparentColor );
        }

        /// <summary>
        /// Erzeugt das OSD.
        /// </summary>
        /// <param name="width">Breite.</param>
        /// <param name="height">Höhe.</param>
        /// <returns>OSD Erzeugungsbereich.</returns>
        private Bitmap CreateBitmap( int width, int height )
        {
            // Process
            using (Graphics g = Graphics.FromHwnd( m_Main.Handle ))
                return new Bitmap( width, height, g );
        }

        /// <summary>
        /// Beende die Nutzung dieses ODS Bildes.
        /// </summary>
        private void Finish()
        {
            // Already done
            if (null == m_Graphics) return;

            // Shut down
            m_Graphics.Dispose();

            // Forget
            m_Graphics = null;
        }

        /// <summary>
        /// Zeichen, die einen Text nach Zeilen unterteilen.
        /// </summary>
        private static readonly char[] s_LineSplit = { '\n' };

        /// <summary>
        /// Zeichen, die eine Zeile nach Worten unterteilen.
        /// </summary>
        private static readonly char[] s_WordSplit = { ' ', '\t' };

        /// <summary>
        /// Erstellt eine Textzeile und schreibt diese in einem Zeichenstil in das OSD.
        /// </summary>
        /// <param name="style">Der gewünschte Zeichenstil.</param>
        /// <param name="selected">Gesetzt, wenn die Textzeile markiert dargestellt werden soll.</param>
        /// <param name="format">Format zur Konstruktion der Zeichenkette.</param>
        /// <param name="arguments">Parameter für das Format zur Konstruktion der Zeichenkette.</param>
        public void WriteLine( FontStyle style, bool selected, string format, params string[] arguments )
        {
            // Forward
            WriteLine( style, selected, string.Format( format, arguments ) );
        }

        /// <summary>
        /// Erstellt eine Textzeile und schreibt diese in einem Zeichenstil in das OSD.
        /// </summary>
        /// <param name="style">Der gewünschte Zeichenstil.</param>
        /// <param name="format">Format zur Konstruktion der Zeichenkette.</param>
        /// <param name="arguments">Parameter für das Format zur Konstruktion der Zeichenkette.</param>
        public void WriteLine( FontStyle style, string format, params string[] arguments )
        {
            // Forward
            WriteLine( style, false, format, arguments );
        }

        /// <summary>
        /// Erstellt eine Textzeile und schreibt in das OSD.
        /// </summary>
        /// <param name="format">Format zur Konstruktion der Zeichenkette.</param>
        /// <param name="arguments">Parameter für das Format zur Konstruktion der Zeichenkette.</param>
        public void WriteLine( string format, params string[] arguments )
        {
            // Forward
            WriteLine( FontStyle.Regular, false, format, arguments );
        }

        /// <summary>
        /// Erstellt eine Textzeile und schreibt diese in das OSD.
        /// </summary>
        /// <param name="selected">Gesetzt, wenn die Textzeile markiert dargestellt werden soll.</param>
        /// <param name="format">Format zur Konstruktion der Zeichenkette.</param>
        /// <param name="arguments">Parameter für das Format zur Konstruktion der Zeichenkette.</param>
        public void WriteLine( bool selected, string format, params string[] arguments )
        {
            // Forward
            WriteLine( FontStyle.Regular, selected, format, arguments );
        }

        /// <summary>
        /// Schreibt eine Textzeile in das OSD.
        /// </summary>
        /// <param name="line">Die gewünschte Textzeile.</param>
        public void WriteLine( string line )
        {
            // Forward
            WriteLine( FontStyle.Regular, false, line );
        }

        /// <summary>
        /// Schreibt eine Textzeile in einem bestimmten Zeichenstil in das OSD.
        /// </summary>
        /// <param name="style">Der gewünschte Stil.</param>
        /// <param name="line">Die Textzeile.</param>
        public void WriteLine( FontStyle style, string line )
        {
            // Forward
            WriteLine( style, false, line );
        }

        /// <summary>
        /// Schreibt eine Textzeile in das OSD.
        /// </summary>
        /// <param name="selected">Gesetzt, wenn die Textzeile markiert dargestellt werden soll.</param>
        /// <param name="line">Die gewünschte Textzeile.</param>
        public void WriteLine( bool selected, string line )
        {
            // Forward
            WriteLine( FontStyle.Regular, selected, line );
        }

        /// <summary>
        /// Gibt einen Text mit Zeilenumbruch aus.
        /// </summary>
        /// <param name="limit">Die maximale Anzahl von Zeichen pro Zeile.</param>
        /// <param name="text">Der gewünschte Text.</param>
        /// <exception cref="ArgumentException">Die Anzahl der Zeichen muss positiv sein.</exception>
        public void WriteWrappedText( int limit, string text )
        {
            // Validate
            if (limit < 1)
                throw new ArgumentException( limit.ToString(), "limit" );

            // No text at all
            if (text == null)
                return;

            // Current collector
            var collect = new StringBuilder( text.Length );

            // Lines
            foreach (var line in text.Replace( "\r", "\n" ).Split( s_LineSplit ))
            {
                // See if we have written at least one line
                bool feed = false;

                // Reset
                collect.Length = 0;

                // All words
                foreach (var word in line.Split( s_WordSplit, StringSplitOptions.RemoveEmptyEntries ))
                {
                    // Old length
                    int len = collect.Length;

                    // Just separate
                    if (collect.Length > 0)
                        collect.Append( ' ' );

                    // New data
                    collect.Append( word );

                    // Fits in
                    if (collect.Length < limit)
                        continue;

                    // Send the previous stuff
                    if (len > 0)
                    {
                        // Reset
                        collect.Length = len;

                        // Send
                        WriteLine( collect.ToString() );

                        // Did some
                        feed = true;
                    }

                    // Reset
                    collect.Length = 0;

                    // Load
                    collect.Append( word );
                }

                // Feed one empty line
                if (collect.Length > 0)
                    WriteLine( collect.ToString() );
                else if (!feed)
                    WriteLine( " " );
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung des OSD Bildes und zeigt es im zugehörigen Fenster an.
        /// </summary>
        public void Dispose()
        {
            // Finish changes
            Finish();

            // Show self
            if (null != m_Main)
            {
                // Paint
                Show();

                // Forget
                m_Main = null;
            }

            // Destroy
            using (m_Bitmap)
                m_Bitmap = null;
        }

        #endregion
    }
}
