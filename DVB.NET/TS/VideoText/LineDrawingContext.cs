using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

namespace JMS.DVB.TS.VideoText
{
    /// <summary>
    /// Diese Klasse ist in der Lage, eine einzelne Videotext Zeile zu zeichnen.
    /// </summary>
    public class LineDrawingContext
    {
        /// <summary>
        /// Zeichen für die normale Darstellung.
        /// </summary>
        public static Font NormalTextFont = new Font( "Courier New", 14.0f, FontStyle.Bold );

        /// <summary>
        /// Darstellungsverzerrung für normale Zeichen.
        /// </summary>
        private static readonly Matrix m_NormalTextMatrix = new Matrix( 1, 0, 0, 1, 0, 0 );

        /// <summary>
        /// Darstellungsverzerrung für doppelt breite Zeichen.
        /// </summary>
        private static readonly Matrix m_WideTextMatrix = new Matrix( 2, 0, 0, 1, 0, 0 );

        /// <summary>
        /// Darstellungsverzerrung für doppelt hohe Zeichen.
        /// </summary>
        private static readonly Matrix m_HighTextMatrix = new Matrix( 1, 0, 0, 2, 0, 0 );

        /// <summary>
        /// Darstellungsverzerrung für doppelte breite und hohe Zeichen.
        /// </summary>
        private static readonly Matrix m_LargeTextMatrix = new Matrix( 2, 0, 0, 2, 0, 0 );

        /// <summary>
        /// Die vom Videotext unterstützten Farben.
        /// </summary>
        public static readonly Dictionary<Color, Brush> Brushes = new Dictionary<Color, Brush>();

        /// <summary>
        /// Der primäre Zeichensatz (deutsch).
        /// </summary>
        private static string[] m_Charset;

        /// <summary>
        /// Die Seite, die gerade gezeichnet wird.
        /// </summary>
        internal readonly PageDrawingContext PageContext;

        /// <summary>
        /// Die aktuellen Konfiguration für die Anzeige eines Zeichens.
        /// </summary>
        private CharDrawingContext m_CurrentContext;

        /// <summary>
        /// Aktuelle horizontale Zeichenposition.
        /// </summary>
        private float m_X;

        /// <summary>
        /// Aktuelle vertikale Zeichenposition.
        /// </summary>
        private float m_Y;

        /// <summary>
        /// Gesetzt, wenn der primäre Zeichensatz verwendet werden soll.
        /// </summary>
        private bool m_UseG0;

        /// <summary>
        /// Gesetzt, sobald auf einer Untertitelseite die Box aktiviert wurde.
        /// </summary>
        private bool m_InsideSubtitleBox;

        /// <summary>
        /// Gesetzt, wenn Füllzeichen als Mosaik dargestellt werden sollen.
        /// </summary>
        private bool m_HoldMode;

        /// <summary>
        /// Zeichenmodus, wenn ein Füllzeichen als Mosaik ausgegeben wird.
        /// </summary>
        private CharDrawingContext m_HoldContext;

        /// <summary>
        /// Füllzeichen.
        /// </summary>
        private byte? m_HoldCode;

        /// <summary>
        /// Gesetzt, wenn mindestens ein Zeichen doppelte Höhe hat.
        /// </summary>
        private bool m_HasDoubleHeight;

        /// <summary>
        /// Bei doppelter Breite werden zwei Leerzeichen als eines ausgegeben.
        /// </summary>
        private bool m_PendingBlank;

        /// <summary>
        /// Initialisiert statische Hilfselemente, Nachschlagetabellen und so weiter.
        /// </summary>
        static LineDrawingContext()
        {
            // Create primary charset
            m_Charset = new string[128];

            // Fill primary charset - standard
            for (int i = 32; i < 128; ++i)
                m_Charset[i] = new string( (char) i, 1 );

            // Special characters (german)
            m_Charset[0x23] = "#";
            m_Charset[0x24] = "$";
            m_Charset[0x40] = "§";
            m_Charset[0x5b] = "Ä";
            m_Charset[0x5c] = "Ö";
            m_Charset[0x5d] = "Ü";
            m_Charset[0x60] = "°";
            m_Charset[0x7b] = "ä";
            m_Charset[0x7c] = "ö";
            m_Charset[0x7d] = "ü";
            m_Charset[0x7e] = "ß";
            m_Charset[0x7f] = "\x2588";

            // Colors
            Brushes[Color.Black] = new SolidBrush( Color.Black );
            Brushes[Color.Red] = new SolidBrush( Color.Red );
            Brushes[Color.Green] = new SolidBrush( Color.Lime );
            Brushes[Color.Yellow] = new SolidBrush( Color.Yellow );
            Brushes[Color.Blue] = new SolidBrush( Color.Blue );
            Brushes[Color.Magenta] = new SolidBrush( Color.Magenta );
            Brushes[Color.Cyan] = new SolidBrush( Color.Cyan );
            Brushes[Color.White] = new SolidBrush( Color.White );
        }

        /// <summary>
        /// Erzeugt eine neue Instanz zum Zeichnen einer Videotext Zeile.
        /// </summary>
        /// <param name="page"></param>
        internal LineDrawingContext( PageDrawingContext page )
        {
            // Remember
            PageContext = page;

            // Create helper
            m_CurrentContext = new CharDrawingContext( this );
        }

        /// <summary>
        /// Ermittelt das aktuelle Zeichenfeld.
        /// </summary>
        private Graphics Graphics
        {
            get
            {
                // Forward
                return PageContext.Graphics;
            }
        }

        /// <summary>
        /// Meldet die aktuelle Größe eines einzelnen Zeichens.
        /// </summary>
        private SizeF CharSize
        {
            get
            {
                // Forward
                return PageContext.NormalCharacterSize;
            }
        }

        /// <summary>
        /// Settz die Vordergrundfarbe ab dem nächsten Zeichen.
        /// </summary>
        /// <param name="color">Die neue Vorderdrundfarbe.</param>
        /// <param name="useG0">Gesetzt, wenn danach der primäre Zeichensatz verwendet werden soll.</param>
        private void SetForeground( Color color, bool useG0 )
        {
            // Platzhalter ausgeben
            DrawSpace();

            // Clear hold character
            if (m_UseG0 != useG0) m_HoldCode = null;

            // Farbe setzen und Zeichensatz umschalten
            m_CurrentContext.Foreground = Brushes[color];
            m_UseG0 = useG0;
        }

        /// <summary>
        /// Gibt den Platzhalter für eine Steuersequenz aus.
        /// </summary>
        private void DrawSpace()
        {
            // Check mode
            if (m_HoldMode && m_HoldCode.HasValue)
            {
                // Forward
                Draw( m_HoldCode.Value, m_CurrentContext );
            }
            else
            {
                // Forward
                Draw( 32, m_CurrentContext );
            }
        }

        /// <summary>
        /// Ändert die Zeichengröße.
        /// </summary>
        /// <param name="matrix">Zu verwendende Skalierung.</param>
        private void SetSize( Matrix matrix )
        {
            // Draw placeholder
            if (matrix != m_NormalTextMatrix)
                DrawSpace();

            // Terminate hold mode
            if (Graphics.Transform != matrix)
                m_HoldCode = null;

            // Change the scaling
            Graphics.Transform = matrix;

            // Draw placeholder
            if (matrix == m_NormalTextMatrix)
                DrawSpace();
        }

        /// <summary>
        /// Ändert die Hintergrundfarbe ab dem nächsten Zeichen.
        /// </summary>
        /// <param name="background">Die neue Hintergrundfarbe.</param>
        private void SetBackground( Brush background )
        {
            // Change background
            m_CurrentContext.Background = background;

            // Draw placeholder
            DrawSpace();
        }

        /// <summary>
        /// Stellt eine einzelne Videotext Seite dar
        /// </summary>
        /// <param name="line">Die Nummer der Zeile, beginnend mit der Überschrift in Zeile 0.</param>
        /// <param name="data">Die Videotext Daten für die Zeile.</param>
        /// <param name="prefix">Optionaler Vorabtext für die Zeile - die Darstellung erfolgt immer im
        /// Defaultformat.</param>
        public void Draw( int line, byte[] data, string prefix )
        {
            // Reset the context to the start of the line
            Graphics.Transform = m_NormalTextMatrix;

            // Reset state to the beginning of the indicated line
            m_Y = line * CharSize.Height;
            m_X = 0;

            // Reset all visual elements
            m_InsideSubtitleBox = false;
            m_HasDoubleHeight = false;
            m_PendingBlank = false;
            m_HoldMode = false;
            m_UseG0 = true;

            // Check data for double height characters
            foreach (TTXControlCodes code in data)
                if ((TTXControlCodes.DoubleHeight == code) || (TTXControlCodes.DoubleSize == code))
                {
                    // Remember
                    m_HasDoubleHeight = true;

                    // Check done
                    break;
                }

            // Draw prefix characters
            if (null != prefix)
                foreach (byte code in Encoding.ASCII.GetBytes( prefix ))
                    Draw( code, m_CurrentContext );

            // Draw codes
            foreach (TTXControlCodes code in data)
                switch (code)
                {
                    // Supported
                    case TTXControlCodes.AlphaBlack: SetForeground( Color.Black, true ); break;
                    case TTXControlCodes.AlphaBlue: SetForeground( Color.Blue, true ); break;
                    case TTXControlCodes.AlphaCyan: SetForeground( Color.Cyan, true ); break;
                    case TTXControlCodes.AlphaGreen: SetForeground( Color.Green, true ); break;
                    case TTXControlCodes.AlphaMagenta: SetForeground( Color.Magenta, true ); break;
                    case TTXControlCodes.AlphaRed: SetForeground( Color.Red, true ); break;
                    case TTXControlCodes.AlphaWhite: SetForeground( Color.White, true ); break;
                    case TTXControlCodes.AlphaYellow: SetForeground( Color.Yellow, true ); break;
                    case TTXControlCodes.BlackBackground: SetBackground( null ); break;
                    case TTXControlCodes.DoubleHeight: SetSize( m_HighTextMatrix ); break;
                    case TTXControlCodes.DoubleSize: SetSize( m_LargeTextMatrix ); break;
                    case TTXControlCodes.DoubleWidth: SetSize( m_WideTextMatrix ); break;
                    case TTXControlCodes.EndBox: DrawSpace(); m_InsideSubtitleBox = false; break;
                    case TTXControlCodes.HoldMosaic: m_HoldMode = true; DrawSpace(); break;
                    case TTXControlCodes.MosaicBlack: SetForeground( Color.Black, false ); break;
                    case TTXControlCodes.MosaicBlue: SetForeground( Color.Blue, false ); break;
                    case TTXControlCodes.MosaicCyan: SetForeground( Color.Cyan, false ); break;
                    case TTXControlCodes.MosaicGreen: SetForeground( Color.Green, false ); break;
                    case TTXControlCodes.MosaicMagenta: SetForeground( Color.Magenta, false ); break;
                    case TTXControlCodes.MosaicRed: SetForeground( Color.Red, false ); break;
                    case TTXControlCodes.MosaicWhite: SetForeground( Color.White, false ); break;
                    case TTXControlCodes.MosaicYellow: SetForeground( Color.Yellow, false ); break;
                    case TTXControlCodes.NewBackground: SetBackground( m_CurrentContext.Foreground ); break;
                    case TTXControlCodes.NormalSize: SetSize( m_NormalTextMatrix ); break;
                    case TTXControlCodes.ReleaseMosaic: DrawSpace(); m_HoldMode = false; break;
                    case TTXControlCodes.StartBox: DrawSpace(); m_InsideSubtitleBox = true; break;
                    // Unsupported
                    case TTXControlCodes.Conceal: DrawSpace(); break;
                    case TTXControlCodes.ContigousMosaic: DrawSpace(); break;
                    case TTXControlCodes.Escape: DrawSpace(); break;
                    case TTXControlCodes.Flash: DrawSpace(); break;
                    case TTXControlCodes.SeparatedMosaic: DrawSpace(); break;
                    case TTXControlCodes.Steady: DrawSpace(); break;
                    // Normal pass-through
                    default: Draw( (byte) code, m_CurrentContext ); break;
                }
        }

        /// <summary>
        /// Stellt ein Zeichen dar und verändert die horizontale Position entsprechend.
        /// </summary>
        /// <param name="code">Der Code des Zeichens.</param>
        /// <param name="context">Informationen über das Darstellungsformat.</param>
        private void Draw( byte code, CharDrawingContext context )
        {
            // Remember
            bool pendingBlank = m_PendingBlank;

            // Reset
            m_PendingBlank = false;

            // Check mode
            if (Graphics.Transform.Elements[0] == 2)
                if (code == 32)
                    if (!pendingBlank)
                    {
                        // Wait for pending blank
                        m_PendingBlank = true;

                        // Silent skip
                        return;
                    }

            // There is nothing to draw
            if (m_InsideSubtitleBox || !PageContext.IsTransparent)
            {
                // Attach to unscaled positions
                float realX = m_X / Graphics.Transform.Elements[0];
                float realY = m_Y / Graphics.Transform.Elements[3];

                // Set the background
                if (null != context.Background)
                    if (m_HasDoubleHeight && (Graphics.Transform.Elements[3] != 2))
                    {
                        // Erase line below
                        FillRectangle( context.Background, realX, realY, CharSize.Width, CharSize.Height * 2 );
                    }
                    else
                    {
                        // Draw as is
                        FillRectangle( context.Background, realX, realY, CharSize.Width, CharSize.Height );
                    }

                // Check mode and dispatch
                if (m_UseG0)
                {
                    // Normal character
                    DrawG0( code, realX, realY, context );
                }
                else
                {
                    // Eventually a mosaic character
                    DrawG1( code, realX, realY, context );
                }
            }

            // Advance position 
            m_X += Graphics.Transform.Elements[0] * CharSize.Width;
        }

        /// <summary>
        /// Stellt ein Zeichen aus dem primären Zeichensatz dar.
        /// </summary>
        /// <param name="code">Code des Zeichens.</param>
        /// <param name="x">Horizontale Position in der Seite, an der das Zeichen erscheinen soll.</param>
        /// <param name="y">Vertikale Position in der Seite, an der das Zeichen erscheinen soll.</param>
        /// <param name="context">Informationen über das Darstellungsformat.</param>
        private void DrawG0( byte code, float x, float y, CharDrawingContext context )
        {
            // Get the character to draw
            string ch = m_Charset[code];

            // Draw the one
            Graphics.DrawString( ch, NormalTextFont, context.Foreground, x, y );

            // Get the rectangle
            RectangleF rect =
                new RectangleF
                    (
                        x * Graphics.Transform.Elements[0],
                        y * Graphics.Transform.Elements[3],
                        CharSize.Width * Graphics.Transform.Elements[0] * (string.IsNullOrEmpty( ch ) ? 1 : ch.Length),
                        CharSize.Height * Graphics.Transform.Elements[3]
                    );

            // Report
            if ((ch != null) && !string.IsNullOrEmpty( ch.Trim( ' ' ) ))
                PageContext.ReportUsage( rect );

            // See if this is a number
            if (ch != null)
                if (ch.Length == 1)
                {
                    // Load
                    char dig = ch[0];

                    // Test
                    if ((dig >= '0') && (dig <= '9'))
                        PageContext.Digits.Add( dig - '0', rect );
                }
        }

        /// <summary>
        /// Stellt ein Zeichen aus dem Mosaik Zeichensatz dar.
        /// </summary>
        /// <param name="code">Code des Zeichens.</param>
        /// <param name="x">Horizontale Position in der Seite, an der das Zeichen erscheinen soll.</param>
        /// <param name="y">Vertikale Position in der Seite, an der das Zeichen erscheinen soll.</param>
        /// <param name="context">Informationen über das Darstellungsformat.</param>
        private void DrawG1( byte code, float x, float y, CharDrawingContext context )
        {
            // See if this is a mosaic character
            if ((code < '\x40') || (code > '\x5f'))
            {
                // Read mosaic bounds
                SizeF mosaic = PageContext.MosaicElementSize;

                // Test all bits
                if (0 != (0x01 & code)) FillRectangle( context.Foreground, x, y, mosaic.Width, mosaic.Height );
                if (0 != (0x02 & code)) FillRectangle( context.Foreground, x + mosaic.Width, y, mosaic.Width, mosaic.Height );
                if (0 != (0x04 & code)) FillRectangle( context.Foreground, x, y + mosaic.Height, mosaic.Width, mosaic.Height );
                if (0 != (0x08 & code)) FillRectangle( context.Foreground, x + mosaic.Width, y + mosaic.Height, mosaic.Width, mosaic.Height );
                if (0 != (0x10 & code)) FillRectangle( context.Foreground, x, y + 2 * mosaic.Height, mosaic.Width, mosaic.Height );
                if (0 != (0x40 & code)) FillRectangle( context.Foreground, x + mosaic.Width, y + 2 * mosaic.Height, mosaic.Width, mosaic.Height );

                // May want to remember as hold character
                m_HoldContext = m_CurrentContext.Clone();
                m_HoldCode = code;
            }
            else
            {
                // Normal character
                DrawG0( code, x, y, context );
            }
        }

        /// <summary>
        /// Füllt einen Bereich mit einer Farbe aus.
        /// </summary>
        /// <param name="color">Die gewünschte Farbe.</param>
        /// <param name="x">Die horizontale Position der linken oberen Ecke.</param>
        /// <param name="y">Die vertikale Position der linken oberen Ecke.</param>
        /// <param name="width">Die Breite.</param>
        /// <param name="height">Die Höhe.</param>
        private void FillRectangle( Brush color, float x, float y, float width, float height )
        {
            // Convert
            var rect = new RectangleF( x, y, width, height );

            // Forward
            Graphics.FillRectangle( color, rect );

            // Report
            if (!ReferenceEquals( color, TTXPage.TransparentColor ))
                PageContext.ReportUsage( rect );
        }

        /// <summary>
        /// Meldet, ob in dieser Zeile Zeichen doppelter Höhe enthalten sind.
        /// </summary>
        public bool HasDoubleHeight
        {
            get
            {
                // Report
                return m_HasDoubleHeight;
            }
        }
    }
}
