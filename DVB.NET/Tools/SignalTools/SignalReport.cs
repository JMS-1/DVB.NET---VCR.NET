using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

namespace JMS.DVB.Administration.Tools
{
    /// <summary>
    /// Auf diesem Formular wird eine Empfangsstatistik über alle Quellgruppen angezeigt.
    /// </summary>
    public partial class SignalReport : UserControl, IPlugInControl
    {
        /// <summary>
        /// Meldet die zugehörige administrative Erweiterung.
        /// </summary>
        public SignalOverview PlugIn { get; private set; }

        /// <summary>
        /// Die aktuelle administrative Umgebung.
        /// </summary>
        public IPlugInUISite AdminSite { get; private set; }

        /// <summary>
        /// Die Verteilung aller Quellgruppen auf die alternativen Anzeigen.
        /// </summary>
        private Dictionary<SourceGroupSelector, GroupDisplay> m_Displays = new Dictionary<SourceGroupSelector, GroupDisplay>();

        /// <summary>
        /// Führt die eigentliche Auswertung aus.
        /// </summary>
        private volatile Thread m_Worker;

        /// <summary>
        /// Erzeugt ein neues Anzeigelement.
        /// </summary>
        /// <param name="plugIn">Die zugehörige administrative Erweiterung.</param>
        /// <param name="site">Die aktuelle administrative Umgebung.</param>
        public SignalReport( SignalOverview plugIn, IPlugInUISite site )
        {
            // Remember
            AdminSite = site;
            PlugIn = plugIn;

            // Load designer stuff.
            InitializeComponent();

            // Update
            lbProfile.Text = string.Format( lbProfile.Text, PlugIn.Profile.Name );

            // Prepare all controls
            foreach (SourceSelection source in PlugIn.Profile.AllSources)
            {
                // Create selection
                SourceGroupSelector selector = SourceGroupSelector.Create( source );

                // Read display
                GroupDisplay display;
                if (!m_Displays.TryGetValue( selector, out display ))
                {
                    // Create new
                    display = new GroupDisplay( selector );

                    // Reconnect
                    selector.Display = display;

                    // Configure
                    display.Visible = true;
                    display.Dock = DockStyle.Fill;

                    // Attach to picture box
                    picView.Controls.Add( display );

                    // Remember
                    m_Displays[selector] = display;
                }

                // Register
                display.Register( source );
            }
        }

        /// <summary>
        /// Wird aufgerufen, sobald das Element angezeigt werden soll.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void SignalReport_Load( object sender, EventArgs e )
        {
            // Load all
            selGroup.Items.AddRange( m_Displays.Keys.ToArray() );

            // Enable
            selGroup.Enabled = (selGroup.Items.Count > 1);

            // Select the first
            if (selGroup.Items.Count > 0)
                selGroup.SelectedIndex = 0;

            // Fill
            selGroup_SelectionChangeCommitted( selGroup, EventArgs.Empty );
        }

        /// <summary>
        /// Prüft, ob der Anwender die Bearbeitung abgebrochen hat.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void updater_Tick( object sender, EventArgs e )
        {
            // Stop processing
            if (AdminSite.HasBeenCancelled)
                m_Worker = null;
        }

        /// <summary>
        /// Führt die eigentliche Auswertung durch.
        /// </summary>
        private void Worker()
        {
            // Be safe
            try
            {
                // Prepare all displays
                foreach (GroupDisplay display in m_Displays.Values)
                    display.Prepare();

                // As long as necessary
                using (HardwareManager.Open())
                    while (null != m_Worker)
                        foreach (GroupDisplay display in m_Displays.Values)
                        {
                            // Process
                            display.ProcessNext();

                            // Done
                            if (null == m_Worker)
                                break;

                            // Relax a bit
                            Thread.Sleep( 100 );

                            // Done
                            if (null == m_Worker)
                                break;
                        }
            }
            catch
            {
            }

            // Report
            Invoke( new Action( AdminSite.OperationDone ) );
        }

        /// <summary>
        /// Zeigt eine Art von Quellgruppen an.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selGroup_SelectionChangeCommitted( object sender, EventArgs e )
        {
            // Disable all
            foreach (GroupDisplay display in m_Displays.Values)
                display.Visible = false;

            // Get the selection
            SourceGroupSelector show = (SourceGroupSelector) selGroup.SelectedItem;

            // Select it
            if (null != show)
                show.Display.Visible = true;
        }

        #region IPlugInControl Members

        /// <summary>
        /// Prüft, ob eine Bearbeitung abgebrochen werden kann.
        /// </summary>
        bool IPlugInControl.CanCancel
        {
            get
            {
                // Yes, we can
                return true;
            }
        }

        /// <summary>
        /// Prüft, ob eine Ausführung möglich ist.
        /// </summary>
        /// <returns>Gesetzt, wenn <see cref="IPlugInControl.Start"/> aufgerufen werden darf.</returns>
        bool IPlugInControl.TestStart()
        {
            // Yes, we can
            return true;
        }

        /// <summary>
        /// Fordert dazu auf, diese Aufgabe auszuführen.
        /// </summary>
        /// <returns>Wird gesetzt, um einen synchronen Ablauf anzuzeigen.</returns>
        bool IPlugInControl.Start()
        {
            // Create worker
            m_Worker = new Thread( Worker );

            // Configure
            m_Worker.SetApartmentState( ApartmentState.STA );

            // Run it
            m_Worker.Start();

            // Start watchdog
            updater.Enabled = true;

            // Done
            return false;
        }

        #endregion
    }
}
