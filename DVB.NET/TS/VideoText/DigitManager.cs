using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Drawing;

namespace JMS.DVB.TS.VideoText
{
    /// <summary>
    /// Diese Klasse verwaltet die Ziffern auf einer Videotextseite.
    /// </summary>
    public class DigitManager
    {
        /// <summary>
        /// Die gesamte Breite der Seite als Bild.
        /// </summary>
        private int m_Width;

        /// <summary>
        /// Die gesamte Höhe der Seite als Bild.
        /// </summary>
        private int m_Height;

        /// <summary>
        /// Alle Zahlen.
        /// </summary>
        private List<int> m_Digits = new List<int>();

        /// <summary>
        /// Die zugehörigen Rahmen.
        /// </summary>
        private List<RectangleF> m_Positions = new List<RectangleF>();

        /// <summary>
        /// Erzeugt eine neue Verwaltung.
        /// </summary>
        /// <param name="page">Die zugehörige Bildseite.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Seite angegegen.</exception>
        public DigitManager( Bitmap page )
        {
            // Validate
            if (page == null)
                throw new ArgumentNullException( "page" );

            // Remember
            m_Width = page.Width;
            m_Height = page.Height;
        }

        /// <summary>
        /// Meldet eine Ziffer an.
        /// </summary>
        /// <param name="digit">Die dargestellte Ziffer.</param>
        /// <param name="at">Der Rahmen um die Ziffer.</param>
        public void Add( int digit, RectangleF at )
        {
            // Store
            if (m_Width > 0)
                if (m_Height > 0)
                {
                    // Number as is
                    m_Digits.Add( digit );

                    // Normalized rectangle
                    m_Positions.Add( new RectangleF( at.X / m_Width, at.Y / m_Height, at.Width / m_Width, at.Height / m_Height ) );
                }
        }

        /// <summary>
        /// Prüft, ob an einer bestimmten Position auf der Videotextseite eine Ziffer angezeigt wird.
        /// </summary>
        /// <param name="point">Die relative Position.</param>
        /// <param name="rect">Das die Zahl umschließende Rechteck.</param>
        /// <returns>Die zugehörige Ziffer oder <i>null</i>.</returns>
        public int? GetTTXDigitAt( PointF point, out RectangleF rect )
        {
            // Forward
            return GetTTXDigitAt( point.X, point.Y, out rect );
        }

        /// <summary>
        /// Prüft, ob an einer bestimmten Position auf der Videotextseite eine Ziffer angezeigt wird.
        /// </summary>
        /// <param name="x">Die relative horizontale Position.</param>
        /// <param name="y">Die relative vertikale Position.</param>
        /// <param name="rect">Das die Zahl umschließende Rechteck.</param>
        /// <returns>Die zugehörige Ziffer oder <i>null</i>.</returns>
        public int? GetTTXDigitAt( float x, float y, out RectangleF rect )
        {
            // Scan
            for (int i = m_Positions.Count; i-- > 0; )
            {
                // Load
                rect = m_Positions[i];

                // Test
                if (x >= rect.Left)
                    if (x <= rect.Right)
                        if (y >= rect.Top)
                            if (y <= rect.Bottom)
                                return m_Digits[i];
            }

            // Clear
            rect = RectangleF.Empty;

            // Not found
            return null;
        }

        /// <summary>
        /// Ermittelt eine dreistellige Seitennummer auf einer Videotextseite.
        /// </summary>
        /// <param name="point">Die relative Position, an der die Suche beginnen soll.</param>
        /// <returns>Die gefundene Seitennummer im Bereich 100 bis 899 oder <i>null</i>.</returns>
        public int? GetPageAt( PointF point )
        {
            // Forward
            return GetPageAt( point.X, point.Y );
        }

        /// <summary>
        /// Ermittelt eine dreistellige Seitennummer auf einer Videotextseite.
        /// </summary>
        /// <param name="x">Die relative horizontale Position, an der die Suche beginnen soll.</param>
        /// <param name="y">Die relative vertikale Position, an der die Suche beginnen soll.</param>
        /// <returns>Die gefundene Seitennummer im Bereich 100 bis 899 oder <i>null</i>.</returns>
        public int? GetPageAt( float x, float y )
        {
            // Find the hit
            RectangleF rect;
            int? dig = GetTTXDigitAt( x, y, out rect );

            // None
            if (!dig.HasValue)
                return null;

            // Get the center position
            float centerX = rect.X + rect.Width / 2, centerY = rect.Y + rect.Height / 2;

            // Create helper array
            int[] digits = { -1, -1, -1, dig.Value, -1, -1, -1 };

            // Forward fill
            for (int i = 4; i < 7; i++)
            {
                // Locate it
                RectangleF test;
                int? next = GetTTXDigitAt( centerX + (i - 3) * rect.Width, centerY, out test );

                // None
                if (!next.HasValue)
                    break;

                // Check parameters
                if (test.Width != rect.Width)
                    break;
                if (test.Height != rect.Height)
                    break;

                // Remember
                digits[i] = next.Value;
            }

            // Too long
            if (digits[6] != -1)
                return null;

            // Backward fill
            for (int i = 3; i-- > 0; )
            {
                // Locate it
                RectangleF test;
                int? next = GetTTXDigitAt( centerX - (3 - i) * rect.Width, centerY, out test );

                // None
                if (!next.HasValue)
                    break;

                // Check parameters
                if (test.Width != rect.Width)
                    break;
                if (test.Height != rect.Height)
                    break;

                // Remember
                digits[i] = next.Value;
            }

            // To long
            if (digits[0] != -1)
                return null;

            // Analyse result
            int start = Array.FindIndex( digits, d => d != -1 );
            int end = Array.FindLastIndex( digits, d => d != -1 );

            // Get the dimension
            if ((end - start) != 2)
                return null;

            // Create
            int page = digits[start + 2] + 10 * (digits[start + 1] + 10 * digits[start + 0]);

            // Check bounds
            if (page >= 100)
                if (page <= 899)
                    return page;

            // Not allowed
            return null;
        }
    }
}
