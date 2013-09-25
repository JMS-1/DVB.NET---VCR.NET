using System;
using System.Drawing;
using System.Windows.Forms;

using JMS.DVB.TS.VideoText;


namespace JMS.DVB.DirectShow.UI
{
    /// <summary>
    /// Stellt eine Überlagerung dar.
    /// </summary>
    public partial class OverlayWindow : Form
    {
        /// <summary>
        /// Meldet, ob sich die Anzeige im Vor-Vista Modus befindet.
        /// </summary>
        public static bool UseLegacyOverlay = ((Environment.OSVersion.Platform != PlatformID.Win32NT) || (Environment.OSVersion.Version.Major < 6));

        /// <summary>
        /// Das Steuerelement, das als Bezugspunkt dient.
        /// </summary>
        private Control m_Reference;

        /// <summary>
        /// Das Formular zum Bezugspunkt.
        /// </summary>
        private Form m_ReferenceForm;

        /// <summary>
        /// Signatur einer Methode, an die alle Nachrichten weitergereicht werden sollen.
        /// </summary>
        /// <param name="message">Die empfangene Nachricht.</param>
        public delegate void MessageSink( ref Message message );

        /// <summary>
        /// Wird ausgelöst, wenn eine Eingabe erkannt wurde.
        /// </summary>
        public event MessageSink OnGotMessage;

        /// <summary>
        /// Die Informationen zur Größe des OSD.
        /// </summary>
        private RectangleF m_OSDSize;

        /// <summary>
        /// Erzeugt eine neue Überlagerung.
        /// </summary>
        /// <param name="reference">Dieses Steuerelement dient als Bezugspunkt.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Bezugspunkt angegeben.</exception>
        public OverlayWindow( Control reference )
        {
            // Validate
            if (reference == null)
                throw new ArgumentNullException( "reference" );

            // Remember
            m_ReferenceForm = reference.FindForm();
            m_Reference = reference;

            // Load designer stuff
            InitializeComponent();

            // Finish
            if (UseLegacyOverlay)
                picOSD.Dock = DockStyle.Fill;
            else
                TransparencyKey = TTXPage.TransparentColor.Color;

            // Attach location changes
            m_ReferenceForm.LocationChanged += AdaptChanges;
            m_ReferenceForm.SizeChanged += AdaptChanges;
            m_Reference.LocationChanged += AdaptChanges;
            m_Reference.SizeChanged += AdaptChanges;

            // Adjust to initial size
            AdaptChanges( m_Reference, EventArgs.Empty );
        }

        /// <summary>
        /// Bewegt dieses Fenstre zu einer neuen Position.
        /// </summary>
        /// <param name="rect">Die gewünschte neue Position in Bildschirmkoordinaten.</param>
        private void Reposition( Rectangle rect )
        {
            // Load self
            var self = RectangleToScreen( ClientRectangle );

            // Test
            if (Math.Abs( self.Left - rect.Left ) < 5)
                if (Math.Abs( self.Top - rect.Top ) < 5)
                    if (Math.Abs( self.Width - rect.Width ) < 5)
                        if (Math.Abs( self.Height - rect.Height ) < 5)
                            return;

            // Do it
            SetBounds( rect.Left, rect.Top, rect.Width, rect.Height );
        }

        /// <summary>
        /// Übernimmt Veränderung an der Position des Bezugselementes.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void AdaptChanges( object sender, EventArgs e )
        {
            // Get the target
            var rect = m_Reference.RectangleToScreen( m_Reference.ClientRectangle );

            // Z-Order
            if (TopMost != m_ReferenceForm.TopMost)
                TopMost = !TopMost;

            // Check mode
            if (UseLegacyOverlay)
            {
                // Read
                rect.Offset( (int) Math.Round( m_OSDSize.Left * rect.Width ), (int) Math.Round( m_OSDSize.Top * rect.Height ) );

                // Resize
                rect.Width = (int) Math.Round( m_OSDSize.Width * rect.Width );
                rect.Height = (int) Math.Round( m_OSDSize.Height * rect.Height );
            }

            // Position
            Reposition( rect );

            // Reinstall
            if (!UseLegacyOverlay)
                MoveInPlace();
        }

        /// <summary>
        /// Positioniert die Überlagerung.
        /// </summary>
        private void MoveInPlace()
        {
            // Process
            picOSD.SetBounds( (int) Math.Round( m_OSDSize.Left * Width ), (int) Math.Round( m_OSDSize.Top * Height ), (int) Math.Round( m_OSDSize.Width * Width ), (int) Math.Round( m_OSDSize.Height * Height ) );
        }

        /// <summary>
        /// Zeigt die Überlagerung an.
        /// </summary>
        /// <param name="bitmap">Im OSD anzuzeigende Daten.</param>
        /// <param name="left">Relative Position des OSD (linker Rand).</param>
        /// <param name="top">Relative Position des OSD (oberer Rand).</param>
        /// <param name="right">Relative Position des OSD (rechter Rand).</param>
        /// <param name="bottom">Relative Position des OSD (unterer Rand).</param>
        /// <param name="alpha">Transparenz des OSD. Befindet sich die Anwendung nicht im
        /// Vollbildmodus, wird dieser Parameter ignoriert und das OSD ist undurchsichtig.</param>
        /// <param name="transparent">Optional durchsichtige Farbe.</param>
        public void ShowOverlay( Bitmap bitmap, double left, double top, double right, double bottom, double? alpha, Color? transparent )
        {
            // Relative position
            m_OSDSize = new RectangleF( (float) left, (float) top, (float) (right - left), (float) (bottom - top) );

            // See if we are in full mode support
            if (!UseLegacyOverlay)
            {
                // Set transparency
                Opacity = alpha.GetValueOrDefault( 1 );

                // Set transparency key
                TransparencyKey = transparent.GetValueOrDefault( TTXPage.TransparentColor.Color );
                BackColor = TransparencyKey;
            }

            // Load
            using (picOSD.Image)
                picOSD.Image = new Bitmap( bitmap );

            // Call base
            if (!Visible)
                Show();

            // Update
            AdaptChanges( m_Reference, EventArgs.Empty );
        }

        /// <summary>
        /// Nimmt eine Videotextseite entgegen.
        /// </summary>
        /// <param name="page">Die gewünschte Videotextseite, eventuell leer.</param>
        /// <returns>Gesetzt, wenn eine Seite erfolgreich angezeigt wurde.</returns>
        public bool ShowPage( TTXPage page )
        {
            // Helper
            DigitManager digits;

            // Forward
            return ShowPage( page, out digits );
        }

        /// <summary>
        /// Nimmt eine Videotextseite entgegen.
        /// </summary>
        /// <param name="page">Die gewünschte Videotextseite, eventuell leer.</param>
        /// <param name="digits">Informationen zum Auftreten von Zahlen in der Darstellung.</param>
        /// <returns>Gesetzt, wenn eine Seite erfolgreich angezeigt wurde.</returns>
        public bool ShowPage( TTXPage page, out DigitManager digits )
        {
            // Safe create
            try
            {
                // Create the bitmap from the page
                RectangleF extend;
                using (var ttx = page.CreatePage( this, out digits, out extend ))
                {
                    // Default position
                    double left = GetNormalizedBorder( 0.8 ), top = 0.1;
                    double right = 1.0 - left, bottom = 1.0 - top;

                    // Get the transparency color
                    if (page.IsTransparent)
                    {
                        // There is nothing to show
                        if (extend.IsEmpty)
                            return true;

                        // Check mode
                        if (UseLegacyOverlay)
                        {
                            // Get size
                            double width = right - left, height = bottom - top;

                            // Correct against bounds
                            left += width * extend.Left;
                            top += height * extend.Top;

                            // Correct against bounds
                            right = left + width * extend.Width;
                            bottom = top + height * extend.Height;

                            // Get the range of interest
                            var srcRange =
                                new RectangleF
                                    (
                                        (float) (extend.Left * ttx.Width),
                                        (float) (extend.Top * ttx.Height),
                                        (float) (extend.Width * ttx.Width),
                                        (float) (extend.Height * ttx.Height)
                                    );

                            // Create new bitmap
                            using (var clipped = new Bitmap( ttx, (int) Math.Round( srcRange.Width ), (int) Math.Round( srcRange.Height ) ))
                            {
                                // Prepare to update
                                using (var dest = Graphics.FromImage( clipped ))
                                {
                                    // Get full ranges
                                    var destRange = new RectangleF( 0, 0, clipped.Width, clipped.Height );

                                    // Fill
                                    dest.DrawImage( ttx, destRange, srcRange, GraphicsUnit.Pixel );
                                }

                                // Simpy show up
                                ShowOverlay( clipped, left, top, right, bottom, null, null );
                            }
                        }
                        else
                        {
                            // Simpy show up
                            ShowOverlay( ttx, left, top, right, bottom, null, TTXPage.TransparentColor.Color );
                        }
                    }
                    else
                    {
                        // Simpy show up
                        ShowOverlay( ttx, left, top, right, bottom, null, null );
                    }
                }

                // Did it
                return true;
            }
            catch
            {
                // Reset
                digits = null;

                // Leave
                return false;
            }
        }

        /// <summary>
        /// Meldet die relative horizontale Position eines Fensters im
        /// Bildbereich, so dass dieses das Seitenverhältnis 4:3 beibehält.
        /// </summary>
        /// <param name="width">Die gewünschte relative Breite des Fensters.</param>
        /// <returns>Der zugehörige linke Rand.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Es wurde eine unzulässige Breite angegeben.</exception>
        public double GetNormalizedBorder( double width )
        {
            // Validate
            if (width < 0)
                throw new ArgumentOutOfRangeException( "width" );
            if (width > 1)
                throw new ArgumentOutOfRangeException( "width" );

            // See if we can change
            if (m_Reference.Width > 0)
                if (m_Reference.Height > 0)
                {
                    // Get the correction factor - which is 1 for 4:3 and smaller on wide screen 
                    double corr = 4 / (3.0 * m_Reference.Width / m_Reference.Height);

                    // Get the width using the video ratio - 80% for 4:3 and smaller on wide screen
                    width *= corr;
                }

            // Report the horizontal shift - vertial will be bound to 80%
            return Math.Max( 0.1, (1.0 - width) / 2 );
        }

        /// <summary>
        /// Erzeugt ein neues Überlagerungsfenster.
        /// </summary>
        /// <param name="height">Relative Größe in Einheiten von <see cref="HeightBase"/>.</param>
        /// <param name="headline">Überschrift für die OSD Darstellung.</param>
        /// <param name="transparent">Gesetzt, wenn eine transparente Anzeige erlaubt ist.</param>
        /// <returns>Das gewünschte Fenster.</returns>
        public OSDText CreateOverlay( int height, string headline, bool transparent )
        {
            // Forward
            return new OSDText( this, height * 1.0 / HeightBase, headline, transparent );
        }

        /// <summary>
        /// Meldet die maximale Anzahl von Zeichen pro Zeile.
        /// </summary>
        public int MaximumCharactersPerLine
        {
            get
            {
                // Report
                return 65;
            }
        }

        /// <summary>
        /// Bezugszahl für Höhenangaben.
        /// </summary>
        public int HeightBase
        {
            get
            {
                // Report
                return 375;
            }
        }

        /// <summary>
        /// Die Rahmenhöhe in Einheiten von <see cref="HeightBase"/>.
        /// </summary>
        public int FrameHeight
        {
            get
            {
                // Report
                return 34;
            }
        }

        /// <summary>
        /// Die Höhe einer Zeile in Einheiten von <see cref="HeightBase"/>.
        /// </summary>
        public int LineHeight
        {
            get
            {
                // Report
                return 14;
            }
        }

        /// <summary>
        /// Erzeugt ein neues Überlagerungsfenster.
        /// </summary>
        /// <param name="lines">Die Anzahl der anzulegenden Zeilen.</param>
        /// <param name="headline">Überschrift für die OSD Darstellung.</param>
        /// <returns>Das gewünschte Fenster.</returns>
        public OSDText CreateTextOverlay( int lines, string headline )
        {
            // Forward
            return CreateTextOverlay( lines, headline, false );
        }

        /// <summary>
        /// Erzeugt ein neues Überlagerungsfenster.
        /// </summary>
        /// <param name="lines">Die Anzahl der anzulegenden Zeilen.</param>
        /// <param name="headline">Überschrift für die OSD Darstellung.</param>
        /// <param name="transparent">Gesetzt, wenn eine transparente Anzeige erlaubt ist.</param>
        /// <returns>Das gewünschte Fenster.</returns>
        public OSDText CreateTextOverlay( int lines, string headline, bool transparent )
        {
            // Forward
            return CreateOverlay( GetHeight( lines ), headline, transparent );
        }

        /// <summary>
        /// Berechnet die benötigte Höhe für eine Darstellung mit einer festen Anzahl von Zeilen.
        /// </summary>
        /// <param name="lines">Die gewünschte Anzahl von Zeilen.</param>
        /// <returns>Die benötigte relative Höhe.</returns>
        /// <exception cref="ArgumentException">Die gewünschte Anzahl von Zeilen kann nicht dargestellt werden.</exception>
        public int GetHeight( int lines )
        {
            // Validate
            if (lines < 1)
                throw new ArgumentException( lines.ToString(), "lines" );

            // Calculate
            int total = checked( lines * LineHeight + FrameHeight );

            // Report
            if (total > HeightBase)
                throw new ArgumentException( lines.ToString(), "lines" );
            else
                return total;
        }

        /// <summary>
        /// Ermittelt die maximale Anzahl von Zeilen, die dargestellt werden können.
        /// </summary>
        /// <returns>Die benötigte relative Höhe.</returns>
        public int MaximumNumberOfLines
        {
            get
            {
                // Calculate
                return (HeightBase - FrameHeight) / LineHeight;
            }
        }

        /// <summary>
        /// Übernimmt gewisse Tastatureingaben.
        /// </summary>
        /// <param name="m">Die Information zur Eingabe.</param>
        protected override void WndProc( ref Message m )
        {
            // Check client
            var sink = OnGotMessage;
            if (sink != null)
                sink( ref m );

            // Always forward
            base.WndProc( ref m );
        }

        /// <summary>
        /// Ermittelt eine Position im Videotextfenster.
        /// </summary>
        /// <returns>Die relative Position im Videotextbereich oder <i>null</i>, wenn
        /// dieser nicht getroffen wurde.</returns>
        public PointF? TeleTextHit
        {
            get
            {
                // Forward
                return GetTeleTextHit( MousePosition );
            }
        }

        /// <summary>
        /// Ermittelt eine Position im Videotextfenster.
        /// </summary>
        /// <param name="hit">Die aktuelle Position in Pixel.</param>
        /// <returns>Die relative Position im Videotextbereich oder <i>null</i>, wenn
        /// dieser nicht getroffen wurde.</returns>
        public PointF? GetTeleTextHit( Point hit )
        {
            // Not supported in legacy mode - will do a couple of resizes
            if (UseLegacyOverlay)
                return null;

            // Get the video text relative bounds
            double left = GetNormalizedBorder( 0.8 ), top = 0.1, width = 1.0 - 2 * left, height = 1.0 - 2 * top;

            // Rescale to window
            left *= Width;
            width *= Width;
            top *= Height;
            height *= Height;

            // Respect video window position
            left += Left;
            top += Top;

            // Respect hit position
            hit.Offset( -(int) Math.Round( left ), -(int) Math.Round( top ) );

            // Validate
            if (hit.X < 0)
                return null;
            if (hit.Y < 0)
                return null;
            if (hit.X >= width)
                return null;
            if (hit.Y >= height)
                return null;

            // Report
            return new PointF( (float) (hit.X / width), (float) (hit.Y / height) );
        }
    }
}
