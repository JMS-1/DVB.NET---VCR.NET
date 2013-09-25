using System;
using System.Net;
using System.Linq;
using System.Diagnostics;
using System.Configuration;
using System.Windows.Forms;

using JMS.DVB.Viewer;
using JMS.DVB.DirectShow;
using JMS.DVB.DeviceAccess.Enumerators;


namespace DVBNETViewer.Dialogs
{
    public partial class ProgramSettings : Form
    {
        private const string DecoderMonikerPrefix = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\";

        /// <summary>
        /// Beschreibt den Umfang einer Veränderung an den Einstellung.
        /// </summary>
        public enum ChangeTypes
        {
            /// <summary>
            /// Nur geringfügige Änderungen an den Einstellungen - weitermachen wie bisher.
            /// </summary>
            Minor,

            /// <summary>
            /// Das Bild neu aufbauen.
            /// </summary>
            Picture,

            /// <summary>
            /// Die Anwendung als Ganzes neu starten.
            /// </summary>
            Application
        }

        private bool m_Restart = false;
        private bool m_Rebuid = false;

        private PictureParameters m_Initial = null;
        private IViewerSite m_Viewer = null;

        public ProgramSettings( IViewerSite viewer )
        {
            // Remember
            m_Viewer = viewer;

            // Load picture parameters
            if (null != m_Viewer) m_Initial = m_Viewer.PictureParameters;

            // Set up self
            InitializeComponent();
        }

        private void ProgramSettings_Load( object sender, EventArgs e )
        {
            // Load settings
            var settings = Properties.Settings.Default;

            // Set priority
            switch (settings.Priority)
            {
                case ProcessPriorityClass.Idle: selPrio.SelectedIndex = 0; break;
                case ProcessPriorityClass.BelowNormal: selPrio.SelectedIndex = 1; break;
                case ProcessPriorityClass.Normal: selPrio.SelectedIndex = 2; break;
                case ProcessPriorityClass.AboveNormal: selPrio.SelectedIndex = 3; break;
                case ProcessPriorityClass.High: selPrio.SelectedIndex = 4; break;
                case ProcessPriorityClass.RealTime: selPrio.SelectedIndex = 5; break;
                default: selPrio.SelectedIndex = 4; break;
            }

            // Set OSD time
            selOSD.Value = settings.OSDLifeTime;

            // Set station type
            if (settings.UseTV)
                if (settings.UseRadio)
                    selType.SelectedIndex = 0;
                else
                    selType.SelectedIndex = 2;
            else if (settings.UseRadio)
                selType.SelectedIndex = 1;
            else
                selType.SelectedIndex = 0;

            // Set encryption mode
            if (settings.FreeTV)
                if (settings.PayTV)
                    selEnc.SelectedIndex = 0;
                else
                    selEnc.SelectedIndex = 2;
            else if (settings.PayTV)
                selEnc.SelectedIndex = 1;
            else
                selEnc.SelectedIndex = 0;

            // Set receiver data
            selPort.Value = settings.BroadcastPort;
            txMCast.Text = settings.BroadcastIP;

            // Set VCR data
            txURL.Text = settings.DVBNETViewer_FullServer_VCR30Server;

            // Set codec
            ckCyberlink.Checked = settings.UseCyberlinkCodec;
            ckHideCursor.Checked = settings.HideCursor;
            ckRemote.Checked = settings.UseRemote;

            // All lists
            LoadList( selMPEG2, DisplayGraph.VideoDecoderFilters, settings.MPEG2Decoder );
            LoadList( selH264, DisplayGraph.HDTVDecoderFilters, settings.H264Decoder );
            LoadList( selMP2, DisplayGraph.AudioDecoderFilters, settings.MP2Decoder );
            LoadList( selAC3, DisplayGraph.AC3DecoderFilters, settings.AC3Decoder );

            // Disable all
            selBrightness.Enabled = false;
            selSaturation.Enabled = false;
            selContrast.Enabled = false;
            ckOverwrite.Enabled = false;
            ckOverwrite.Checked = false;
            selHue.Enabled = false;

            // Prepare data
            if (null != m_Initial)
            {
                // Set the mode
                ckOverwrite.Checked = settings.OverwriteVideoSettings;

                // Show the current settings
                SetBar( selSaturation, m_Initial.Saturation, m_Initial.Saturation.Value );
                SetBar( selBrightness, m_Initial.Brightness, m_Initial.Brightness.Value );
                SetBar( selContrast, m_Initial.Contrast, m_Initial.Contrast.Value );
                SetBar( selHue, m_Initial.Hue, m_Initial.Hue.Value );

                // Disable all
                selBrightness.Enabled = ckOverwrite.Checked;
                selSaturation.Enabled = ckOverwrite.Checked;
                selContrast.Enabled = ckOverwrite.Checked;
                selHue.Enabled = ckOverwrite.Checked;

                // Enable
                ckOverwrite.Enabled = true;
            }
        }

        /// <summary>
        /// Meldet die aktuelle Auswahl einer Liste.
        /// </summary>
        /// <param name="list">Ein der Auswahllisten für Decoder.</param>
        /// <returns>Die aktuelle Auswahl des Anwenders.</returns>
        private string ReadList( ComboBox list )
        {
            // Get the selection
            var info = list.SelectedItem as DecoderItem;
            if (info == null)
                return null;
            else
                return string.Format( "{0}{1}", DecoderMonikerPrefix, info.Information.UniqueName );
        }

        /// <summary>
        /// Füllt eine Auswahlliste mit Decodern.
        /// </summary>
        /// <param name="list">Die zu befüllende Liste.</param>
        /// <param name="items">Die Liste der Decoder in willkürlicher Reihenfolge.</param>
        /// <param name="selected">Der vorab ausgewählte Eintrag der Liste.</param>
        private void LoadList( ComboBox list, IDeviceOrFilterInformation[] items, string selected )
        {
            // Fill the list
            list.Items.AddRange( items.Select( i => new DecoderItem( i ) ).ToArray() );

            // Disable sorting
            list.Sorted = false;

            // Finally add the empty selector
            list.Items.Insert( 0, Properties.Resources.DefaultDecoder );

            // Check for selected item
            if (null != selected)
                if (selected.StartsWith( DecoderMonikerPrefix ))
                    try
                    {
                        // Rest should be a guid
                        string test = selected.Substring( DecoderMonikerPrefix.Length );

                        // Find it
                        for (int i = list.Items.Count; i-- > 1; )
                        {
                            // Attach to the item
                            var info = (DecoderItem) list.Items[i];

                            // Test
                            if (0 == string.Compare( test, info.Information.UniqueName, true ))
                            {
                                // Select
                                list.SelectedIndex = i;

                                // Done
                                return;
                            }
                        }
                    }
                    catch
                    {
                        // Ignore any error
                    }

            // Use default
            list.SelectedIndex = 0;
        }

        private ProcessPriorityClass CurrentPriority
        {
            get
            {
                // Check selection and report
                switch (selPrio.SelectedIndex)
                {
                    case 0: return ProcessPriorityClass.Idle;
                    case 1: return ProcessPriorityClass.BelowNormal;
                    case 2: return ProcessPriorityClass.Normal;
                    case 3: return ProcessPriorityClass.AboveNormal;
                    case 4: return ProcessPriorityClass.High;
                    case 5: return ProcessPriorityClass.RealTime;
                }

                // Default
                return ProcessPriorityClass.High;
            }
        }

        private void cmdSave_Click( object sender, EventArgs e )
        {
            // Validate
            if (!string.IsNullOrEmpty( txMCast.Text ))
                try
                {
                    // Parse
                    var test = IPAddress.Parse( txMCast.Text );

                    // Load
                    byte first = test.GetAddressBytes()[0];

                    // Check
                    if ((first < 224) || (first > 239))
                        throw new Exception();
                }
                catch
                {
                    // Report
                    MessageBox.Show( Properties.Resources.BadMulticast );

                    // Do not leave
                    DialogResult = DialogResult.None;

                    // Done
                    return;
                }

            // Attach to settngs
            var settings = Properties.Settings.Default;

            // Check for changes
            settings.SettingChanging += SettingChanging;
            try
            {
                // Copy general
                settings.DVBNETViewer_FullServer_VCR30Server = txURL.Text;
                settings.UseCyberlinkCodec = ckCyberlink.Checked;
                settings.UseRadio = (selType.SelectedIndex < 2);
                settings.BroadcastPort = (ushort) selPort.Value;
                settings.UseTV = (selType.SelectedIndex != 1);
                settings.FreeTV = (selEnc.SelectedIndex != 1);
                settings.PayTV = (selEnc.SelectedIndex < 2);
                settings.HideCursor = ckHideCursor.Checked;
                settings.OSDLifeTime = (int) selOSD.Value;
                settings.UseRemote = ckRemote.Checked;
                settings.Priority = CurrentPriority;
                settings.BroadcastIP = txMCast.Text;

                // Load decoders
                settings.MPEG2Decoder = ReadList( selMPEG2 );
                settings.H264Decoder = ReadList( selH264 );
                settings.MP2Decoder = ReadList( selMP2 );
                settings.AC3Decoder = ReadList( selAC3 );

                // Copy picture parameters
                if (ckOverwrite.Enabled)
                {
                    // All parameters as is
                    var current = m_Viewer.PictureParameters;

                    // Copy
                    settings.OverwriteVideoSettings = ckOverwrite.Checked;
                    settings.VideoSaturation = current.Saturation.Value;
                    settings.VideoBrightness = current.Brightness.Value;
                    settings.VideoContrast = current.Contrast.Value;
                    settings.VideoHue = current.Hue.Value;

                    // Do not reset
                    m_Initial = null;
                }
            }
            finally
            {
                // Remove handler
                settings.SettingChanging -= SettingChanging;
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn sich ein Konfigurationswert verändert hat.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Beschreibt die Änderung.</param>
        private void SettingChanging( object sender, SettingChangingEventArgs e )
        {
            // Load
            object oldValue = Properties.Settings.Default[e.SettingName], newValue = e.NewValue;
            string oldString = oldValue as string, newString = newValue as string;

            // Special reset
            if (oldString != null)
                if (oldString.Length < 1)
                    oldValue = null;
            if (newString != null)
                if (newString.Length < 1)
                    newValue = null;

            // None set
            if (oldValue == null)
                if (newValue == null)
                    return;

            // Check for change
            if (Equals( oldValue, e.NewValue ))
                return;

            // Check for quality of change
            switch (e.SettingName)
            {
                // Require restart
                case "DVBNETViewer_FullServer_VCR30Server": m_Restart = true; break;
                case "BroadcastPort": m_Restart = true; break;
                case "BroadcastIP": m_Restart = true; break;
                case "UseRemote": m_Restart = true; break;
                case "UseRadio": m_Restart = true; break;
                case "FreeTV": m_Restart = true; break;
                case "UseTV": m_Restart = true; break;
                case "PayTV": m_Restart = true; break;
                // Require rebuild only
                case "UseCyberlinkCodec": m_Rebuid = true; break;
                case "MPEG2Decoder": m_Rebuid = true; break;
                case "H264Decoder": m_Rebuid = true; break;
                case "MP2Decoder": m_Rebuid = true; break;
                case "AC3Decoder": m_Rebuid = true; break;
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn der Einstellungsdialog geschlossen wurde.
        /// </summary>
        /// <param name="e">Wird ignoriert.</param>
        protected override void OnClosed( EventArgs e )
        {
            // Reset
            if (m_Initial != null)
                m_Viewer.PictureParameters = m_Initial;

            // Forward
            base.OnClosed( e );
        }

        private void ckOverwrite_CheckedChanged( object sender, EventArgs e )
        {
            // Not active
            if (!ckOverwrite.Enabled) return;

            // Reset all
            if (!ckOverwrite.Checked)
            {
                // Create helper
                PictureParameters current = m_Viewer.PictureParameters;

                // Back to the defaults
                SetBar( selSaturation, current.Saturation, m_Initial.Saturation.Default );
                SetBar( selBrightness, current.Brightness, m_Initial.Brightness.Default );
                SetBar( selContrast, current.Contrast, m_Initial.Contrast.Default );
                SetBar( selHue, current.Hue, m_Initial.Hue.Default );

                // Reset picture
                m_Viewer.PictureParameters = current;
            }

            // Disable all
            selBrightness.Enabled = ckOverwrite.Checked;
            selSaturation.Enabled = ckOverwrite.Checked;
            selContrast.Enabled = ckOverwrite.Checked;
            selHue.Enabled = ckOverwrite.Checked;
        }

        private void SetBar( TrackBar bar, PictureParameters.ParameterSet parameter, float value )
        {
            // Remember
            parameter.Value = value;

            // Get the relative position
            double relVal = (value - parameter.Minimum) / (parameter.Maximum - parameter.Minimum);

            // Set slider
            bar.Value = (int) (bar.Minimum + relVal * (bar.Maximum - bar.Minimum));

            // Activate slider
            bar.Enabled = true;
        }

        private void SetBar( TrackBar bar, PictureParameters.ParameterSet parameter, PictureParameters parameters )
        {
            // Load the relative value
            double relVal = (bar.Value - bar.Minimum) * 1.0 / (bar.Maximum - bar.Minimum);

            // Copy to value
            parameter.Value = (float) (parameter.Minimum + relVal * (parameter.Maximum - parameter.Minimum));

            // Change picture 
            m_Viewer.PictureParameters = parameters;
        }

        private void selContrast_ValueChanged( object sender, EventArgs e )
        {
            // Not active
            if (!selContrast.Enabled) return;

            // Load current parameters
            PictureParameters current = m_Viewer.PictureParameters;

            // Process
            SetBar( selContrast, current.Contrast, current );
        }

        private void selSaturation_ValueChanged( object sender, EventArgs e )
        {
            // Not active
            if (!selSaturation.Enabled) return;

            // Load current parameters
            PictureParameters current = m_Viewer.PictureParameters;

            // Process
            SetBar( selSaturation, current.Saturation, current );
        }

        private void selBrightness_ValueChanged( object sender, EventArgs e )
        {
            // Not active
            if (!selBrightness.Enabled) return;

            // Load current parameters
            PictureParameters current = m_Viewer.PictureParameters;

            // Process
            SetBar( selBrightness, current.Brightness, current );
        }

        private void selHue_ValueChanged( object sender, EventArgs e )
        {
            // Not active
            if (!selHue.Enabled) return;

            // Load current parameters
            PictureParameters current = m_Viewer.PictureParameters;

            // Process
            SetBar( selHue, current.Hue, current );
        }

        /// <summary>
        /// Meldet, wie umfangreich die vorgenommenen Veränderungen waren.
        /// </summary>
        public ChangeTypes ChangeType
        {
            get
            {
                // Find out
                return m_Restart ? ChangeTypes.Application : (m_Rebuid ? ChangeTypes.Picture : ChangeTypes.Minor);
            }
        }
    }
}