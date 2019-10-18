using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace JMS.DVB.Administration.Tools
{
    /// <summary>
    /// Auf diesem Formular werden die Empfangsdaten zu einer Quelle angezeigt.
    /// </summary>
    public partial class StreamDisplay : UserControl, IPlugInControl
    {
        /// <summary>
        /// Meldet die zugehörige administrative Erweiterung.
        /// </summary>
        public SourceStreamsAndStrength PlugIn { get; private set; }

        /// <summary>
        /// Die aktuelle administrative Umgebung.
        /// </summary>
        public IPlugInUISite AdminSite { get; private set; }

        /// <summary>
        /// Verwaltet den Zugriff auf die DVB.NET Geräte.
        /// </summary>
        private IDisposable m_HardwareManager;

        /// <summary>
        /// Die aktiven Datenströme.
        /// </summary>
        private List<StreamItem> m_Streams = new List<StreamItem>();

        /// <summary>
        /// Die Standardfarbe der Auswahlliste.
        /// </summary>
        private Color m_Background;

        /// <summary>
        /// Die Farbe zur Anzeige der Signalinformationen.
        /// </summary>
        private Color m_Signal;

        /// <summary>
        /// Erzeugt ein neues Anzeigelement.
        /// </summary>
        /// <param name="plugIn">Die zugehörige administrative Erweiterung.</param>
        /// <param name="site">Die aktuelle administrative Umgebung.</param>
        public StreamDisplay( SourceStreamsAndStrength plugIn, IPlugInUISite site )
        {
            // Remember
            AdminSite = site;
            PlugIn = plugIn;

            // Load designer stuff.
            InitializeComponent();

            // Remember color
            m_Background = lstStreams.BackColor;
            m_Signal = txSignal.BackColor;

            // Hide
            lbSignal.Visible = PlugIn.Profile.GetSafeRestrictions().ProvidesSignalInformation;
            txSignal.Visible = lbSignal.Visible;

            // Finish
            lbProfile.Text = string.Format( lbProfile.Text, PlugIn.Profile.Name );
        }

        /// <summary>
        /// Wird aufgerufen, sobald das Element angezeigt werden soll.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void StreamDisplay_Load( object sender, EventArgs e )
        {
            // Create all
            List<SourceItem> items = SourceItem.CreateSortedListFromProfile( PlugIn.Profile );

            // Load
            selSource.Items.AddRange( items.ToArray() );

            // Try to find the last selection
            if (null != PlugIn.LastSource)
                selSource.SelectedItem = items.Find( s => s.Source.CompareTo( PlugIn.LastSource ) );

            // Update
            selSource_SelectionChangeCommitted( selSource, EventArgs.Empty );
        }

        /// <summary>
        /// Beendet alle Datenströme.
        /// </summary>
        private void CloseStreams()
        {
            // Process all
            try
            {
                // Forward to all
                foreach (StreamItem stream in m_Streams)
                    stream.Close();
            }
            catch
            {
                // Shutdown hardware
                using (IDisposable hardware = m_HardwareManager)
                    m_HardwareManager = null;
            }
            finally
            {
                // Reset
                m_Streams.Clear();
            }
        }

        /// <summary>
        /// Der Anwender hat eine neue Quelle ausgewählt.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selSource_SelectionChangeCommitted( object sender, EventArgs e )
        {
            // Reset color
            lstStreams.BackColor = m_Background;
            txSignal.BackColor = m_Signal;
            txSignal.Text = null;

            // Stop all activities
            CloseStreams();

            // Clear list
            lstStreams.Items.Clear();

            // Attach to selection
            SourceItem source = (SourceItem) selSource.SelectedItem;

            // Nothing to do
            if (null == source)
                return;

            // Remember
            PlugIn.LastSource = source.Source;

            // Be as safe as possible
            try
            {
                // Make sure that hardware is controlled
                if (m_HardwareManager == null)
                    m_HardwareManager = HardwareManager.Open();

                // Select the source
                source.Source.SelectGroup();

                // Get the stream information
                var info = source.Source.GetSourceInformationAsync().CancelAfter( 15000 ).Result;
                if (info == null)
                    return;

                // Attach to the hardware
                Hardware device = source.Source.GetHardware();

                // Silent decrypt
                if (info.IsEncrypted)
                    try
                    {
                        // Process
                        device.Decrypt( source.Source.Source );

                        // Did it
                        lstStreams.BackColor = Color.Green;
                    }
                    catch
                    {
                        // Failed
                        lstStreams.BackColor = Color.Red;
                    }

                // Video
                if (info.VideoType == VideoTypes.MPEG2)
                    m_Streams.Add( new StreamItem( device, info.VideoStream, StreamTypes.Video, "SDTV", false ) );
                else if (info.VideoType == VideoTypes.H264)
                    m_Streams.Add( new StreamItem( device, info.VideoStream, StreamTypes.Video, "HDTV", true ) );

                // Audio
                foreach (AudioInformation audio in info.AudioTracks)
                    m_Streams.Add( new StreamItem( device, audio.AudioStream, StreamTypes.Audio, string.Format( "{0} {1}", audio.AudioType, audio.Language ) ) );

                // DVB subtitles
                foreach (SubtitleInformation sub in info.Subtitles)
                    m_Streams.Add( new StreamItem( device, sub.SubtitleStream, StreamTypes.SubTitle, string.Format( "{0} {1}", sub.SubtitleType, sub.Language ) ) );

                // Videotext
                if (0 != info.TextStream)
                    m_Streams.Add( new StreamItem( device, info.TextStream, StreamTypes.VideoText, "TTX" ) );
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, ex.Message, string.Format( Properties.Resources.Exception_Source_Title, source ) );
            }

            // Add to list
            lstStreams.Items.AddRange( m_Streams.ToArray() );

            // Set column widths
            foreach (ColumnHeader column in lstStreams.Columns)
            {
                // First width
                column.AutoResize( ColumnHeaderAutoResizeStyle.HeaderSize );

                // Remember
                int width1 = column.Width;

                // Second width
                column.AutoResize( ColumnHeaderAutoResizeStyle.ColumnContent );

                // Remember
                int width2 = column.Width;

                // Reset
                column.AutoResize( ColumnHeaderAutoResizeStyle.None );

                // Set width
                column.Width = Math.Max( width1, width2 );
            }
        }

        /// <summary>
        /// Aktualisiert die Anzeige der Datenströme.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void updater_Tick( object sender, EventArgs e )
        {
            // None
            if (m_Streams.Count < 1)
                return;

            // Forward to all
            foreach (StreamItem stream in m_Streams)
                stream.Update();

            // Signal information available?
            if (!txSignal.Visible)
                return;

            // Reset color
            txSignal.BackColor = m_Signal;

            // Attach to selection
            SourceItem source = (SourceItem) selSource.SelectedItem;

            // Load information
            var signal = ((null == source) || (null == m_HardwareManager)) ? null : source.Source.GetHardware().CurrentSignal;

            // Check mode
            if (null == signal)
            {
                // Warn
                txSignal.BackColor = Color.Yellow;
                txSignal.Text = null;
            }
            else
            {
                // Not locked
                if (signal.Locked.HasValue)
                    if (!signal.Locked.Value)
                        txSignal.BackColor = Color.Red;

                // Report
                txSignal.Text = string.Format( "{0}dB / {1}%", signal.Strength.HasValue ? signal.Strength.ToString() : "-", signal.Quality.HasValue ? ((int) (100 * signal.Quality)).ToString() : "-" );
            }
        }

        #region IPlugInControl Members

        /// <summary>
        /// Prüft, ob eine Bearbeitung abgebrochen werden kann.
        /// </summary>
        bool IPlugInControl.CanCancel
        {
            get
            {
                // Dummy - we are using synchronous processing.
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
        /// <returns>Immer gesetzt um einen synchronen Ablauf anzuzeigen.</returns>
        bool IPlugInControl.Start()
        {
            // Done
            return true;
        }

        #endregion

    }
}
