using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace JMS.DVB.Administration.Tools
{
    /// <summary>
    /// Zeigt eine Auswahl von Quellgruppen an.
    /// </summary>
    public partial class GroupDisplay : UserControl
    {
        /// <summary>
        /// Die für die Anzeige zu nutzenden Farben.
        /// </summary>
        private static Color[] m_Colors = { Color.Yellow, Color.LightGreen, Color.LightSkyBlue, Color.Orange };

        /// <summary>
        /// Die zugehörige Auswahlkomponente.
        /// </summary>
        private SourceGroupSelector m_Selector;

        /// <summary>
        /// Alle Quellgruppen, die von dieser Anzeige berücksichtigt werden sollen.
        /// </summary>
        private Dictionary<SourceGroup, SourceSelection> m_AllRegistered = new Dictionary<SourceGroup, SourceSelection>();

        /// <summary>
        /// Alle zu bearbeitenden Gruppen.
        /// </summary>
        private List<SourceSelection> m_Groups;

        /// <summary>
        /// Die kleinste anzuzeigende Frequenz.
        /// </summary>
        public long m_MinFrequency { get; private set; }

        /// <summary>
        /// Die größte anzuzeigende Frequenz.
        /// </summary>
        public long m_MaxFrequency { get; private set; }

        /// <summary>
        /// Die aktuell zu verwendende Quellgruppe.
        /// </summary>
        private int m_CurrentGroup = 0;


        /// <summary>
        /// Erzeugt eine neue Quellgruppe.
        /// </summary>
        /// <param name="selector">Die zugehörige Auswahlkomponente.</param>
        public GroupDisplay( SourceGroupSelector selector )
        {
            // Remember
            m_Selector = selector;

            // Load designer stuff
            InitializeComponent();
        }

        /// <summary>
        /// Meldet an, dass später eine Quellgruppe auf dieser Anzeige zugeordnet werden soll.
        /// </summary>
        /// <param name="source">Die zugehörige Quelle.</param>
        public void Register( SourceSelection source )
        {
            // Remember
            m_AllRegistered[source.Group] = source;
        }

        /// <summary>
        /// Bearbeitet die nächste Quellgruppe.
        /// </summary>
        public void ProcessNext()
        {
            // Get it
            var source = m_Groups[m_CurrentGroup++];

            // Reset
            if (m_CurrentGroup >= m_Groups.Count)
                m_CurrentGroup = 0;

            // Select it
            source.SelectGroup();

            // Attach to the device and start getting best of 5
            var device = source.GetHardware();
            double best = 0;

            // Repeat a bit
            for (int repeat = 5; repeat-- > 0; Thread.Sleep( 100 ))
            {
                // Get signal information
                var signal = device.CurrentSignal;
                if (signal != null)
                    if (signal.Strength.HasValue)
                        if (signal.Strength.Value > best)
                            best = signal.Strength.Value;
            }

            // Don't believe in zeros
            if (best == 0)
                return;

            // Get horizontal position
            double hPos = (source.Group.Frequency - m_MinFrequency) * 1000.0 / (m_MaxFrequency - m_MinFrequency);

            // Get the vertical position
            double vPos = best * 1000.0 / 15;

            // Convert and correct
            int x = Math.Max( 0, Math.Min( 1000, (int) hPos ) );
            int y = Math.Max( 0, Math.Min( 1000, (int) vPos ) );

            // Process
            SetPoint( x, y, m_Colors[m_Selector.GetSubGroupIndex( source.Group ) % m_Colors.Length] );
        }

        /// <summary>
        /// Markiert einen Punkt.
        /// </summary>
        /// <param name="x">Horizontale Position.</param>
        /// <param name="y">Vertikale Position.</param>
        /// <param name="c">Zu verwendende Farbe.</param>
        private void SetPoint( int x, int y, Color c )
        {
            // Must switch
            if (InvokeRequired)
            {
                // Call self
                Invoke( new Action<int, int, Color>( SetPoint ), x, y, c );
            }
            else
            {
                // Attach to the image
                using (Graphics g = Graphics.FromImage( picPaint.Image ))
                using (SolidBrush b = new SolidBrush( c ))
                    g.FillEllipse( b, Math.Max( 0, Math.Min( 990, x - 5 ) ), 1000 - Math.Max( 10, y - 5 ), 10, 10 );

                // Refresh
                picPaint.Refresh();
            }
        }

        /// <summary>
        /// Bereitet die Anzeige vor.
        /// </summary>
        public void Prepare()
        {
            // Create list
            m_Groups = m_AllRegistered.Values.ToList();

            // Get bounds
            m_MinFrequency = m_Groups.Min( s => s.Group.Frequency );
            m_MaxFrequency = m_Groups.Max( s => s.Group.Frequency );
        }

        /// <summary>
        /// Initialisiert die Anzeige.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void GroupDisplay_Load( object sender, EventArgs e )
        {
            // Create image
            picPaint.Image = new Bitmap( 1000, 1000 );

            // Attach to it
            using (Graphics g = Graphics.FromImage( picPaint.Image ))
            {
                // Background
                using (SolidBrush black = new SolidBrush( Color.Black ))
                    g.FillRectangle( black, 0, 0, 1000, 1000 );

                // Draw lines
                using (Pen white = new Pen( Color.White ))
                {
                    // Change width
                    white.Width = 4;

                    // Process
                    for (int i = 0; ++i < 10; )
                        g.DrawLine( white, 0, 100 * i, 1000, 100 * i );
                }
            }
        }
    }
}
