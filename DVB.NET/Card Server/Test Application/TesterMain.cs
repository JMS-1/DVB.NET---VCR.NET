using System;
using JMS.DVB;
using System.Xml;
using System.Text;
using System.Linq;
using System.Drawing;
using JMS.DVB.CardServer;
using System.Windows.Forms;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace CardServerTester
{
    /// <summary>
    /// Das Hauptfenster der Testanwendung.
    /// </summary>
    public partial class TesterMain : Form
    {
        /// <summary>
        /// Der aktuell aktive <i>Card Server</i>.
        /// </summary>
        public ServerImplementation CurrentServer { get; private set; }

        /// <summary>
        /// Der aktuelle Aufruf an den <i>Card Server</i>.
        /// </summary>
        public IAsyncResult CurrentRequest { get; private set; }

        /// <summary>
        /// Wertet das Ergebnis der aktuellen Anfrage aus.
        /// </summary>
        private Action<object> m_ResultProcessor;

        /// <summary>
        /// Erzweugt das Hauptfenster der Testanwendung.
        /// </summary>
        public TesterMain()
        {
            // Load designer stuff
            InitializeComponent();

            // Load all profiles
            selProfiles.Items.AddRange( Array.ConvertAll( ProfileManager.AllProfiles, p => new ProfileItem( p ) ) );

            // Load last profile in use
            SelectProfile( Properties.Settings.Default.ProfileName );

            // See if user has a preferred profile
            if (selProfiles.SelectedItem == null)
                SelectProfile( UserProfile.ProfileName );

            // Load other flags
            ckInProcess.Checked = Properties.Settings.Default.UseInMemory;
        }

        /// <summary>
        /// Wählt ein bestimmtes Geräteprofil aus.
        /// </summary>
        /// <param name="name">Der Anzeigename des gewünschten Pofils.</param>
        private void SelectProfile( string name )
        {
            // None
            if (string.IsNullOrEmpty( name ))
                return;

            // Try to locate
            Profile preferred = ProfileManager.FindProfile( name );
            if (null == preferred)
                return;

            // Scan
            foreach (ProfileItem item in selProfiles.Items)
                if (item.Profile == preferred)
                {
                    // Select
                    selProfiles.SelectedItem = item;

                    // Process
                    selProfiles_SelectionChangeCommitted( selProfiles, EventArgs.Empty );

                    // Done
                    break;
                }
        }

        /// <summary>
        /// Aktualisiert die Oberfläche.
        /// </summary>
        private void UpdateGUI()
        {
            // Check mode
            if (null == CurrentServer)
            {
                // Set GUI
                cmdStart.Enabled = (null != selProfiles.SelectedItem);
                grpConfig.Enabled = true;
                cmdStop.Enabled = false;
            }
            else if ((null != CurrentRequest) && !CurrentRequest.IsCompleted)
            {
                // Set GUI
                grpConfig.Enabled = false;
                cmdStart.Enabled = false;
                cmdStop.Enabled = false;
            }
            else
            {
                // Set GUI
                cmdStop.Enabled = !pnlState.Visible;
                grpConfig.Enabled = false;
                cmdStart.Enabled = false;

                // Load request and processor
                Action<object> processor = m_ResultProcessor;
                IAsyncResult request = CurrentRequest;

                // Reset
                m_ResultProcessor = null;
                CurrentRequest = null;

                // Load request
                if (null != request)
                    try
                    {
                        // Load
                        object result = ServerImplementation.EndRequest<object>( request );

                        // Process
                        if (null != processor)
                            processor( result );
                    }
                    catch (Exception ex)
                    {
                        // Report
                        MessageBox.Show( this, ex.Message, Text );
                    }
            }
        }

        /// <summary>
        /// Wertet das Aktivieren einer neuen Quelle aus.
        /// </summary>
        /// <param name="result">Die Informationen zum Empfang.</param>
        private void ProcessNewStream( object result )
        {
            // Change type
            StreamInformation[] infos = (StreamInformation[]) result;

            // Process
            LoadStreams( infos, null );
        }

        /// <summary>
        /// Wird aufgerufen, wenn der Anwender versucht, dieses Fenster zu schliessen.
        /// </summary>
        /// <param name="e">Wird geeignet aktualisiert.</param>
        protected override void OnClosing( CancelEventArgs e )
        {
            // Set mode
            if (null != CurrentServer)
                e.Cancel = true;

            // Forward.
            base.OnClosing( e );
        }

        /// <summary>
        /// Es wurde ein geräteprofil ausgewählt.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wir dignoriert.</param>
        private void selProfiles_SelectionChangeCommitted( object sender, EventArgs e )
        {
            // Get the selection
            ProfileItem item = (ProfileItem) selProfiles.SelectedItem;

            // Remember
            Properties.Settings.Default.ProfileName = (null == item) ? null : item.Profile.Name;
            Properties.Settings.Default.Save();

            // Load station selection list
            selStation.Items.Clear();

            // Try to resolve profile
            if (null != item)
                selStation.Items.AddRange( SourceItem.GetSourceItems( item.Profile ) );

            // Refresh
            UpdateGUI();
        }

        /// <summary>
        /// Die Auswahl der Betriebsart wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ckInProcess_CheckedChanged( object sender, EventArgs e )
        {
            // Remember
            Properties.Settings.Default.UseInMemory = ckInProcess.Checked;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Erzeugt einen neuen <i>Card Server</i>.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdStart_Click( object sender, EventArgs e )
        {
            // Attach to the current profile selection
            ProfileItem item = (ProfileItem) selProfiles.SelectedItem;
            if (null == item)
                return;

            // Be safe
            try
            {
                // Create server
                if (ckInProcess.Checked)
                    CurrentServer = ServerImplementation.CreateInMemory();
                else
                    CurrentServer = ServerImplementation.CreateOutOfProcess();

                // Attach the profile
                CurrentRequest = CurrentServer.BeginSetProfile( item.Profile.Name, ckRestart.Checked, false, false );

                // Reset the flag
                ckRestart.Checked = false;
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, ex.Message, Text );

                // Reset
                if (null != CurrentServer)
                    cmdStop_Click( cmdStop, EventArgs.Empty );
            }

            // Refresh
            UpdateGUI();
        }

        /// <summary>
        /// Beendet die aktuelle <i>Card Server Instanz</i>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdStop_Click( object sender, EventArgs e )
        {
            // Forget outstanding request
            CurrentRequest = null;

            // Be safe
            try
            {
                // Forward
                CurrentServer.Dispose();
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, ex.Message, Text );
            }
            finally
            {
                // Forget
                CurrentServer = null;
            }

            // Refresh
            UpdateGUI();
        }

        /// <summary>
        /// Wird periodisch aufgerufen.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ticker_Tick( object sender, EventArgs e )
        {
            // Just update
            UpdateGUI();
        }

        /// <summary>
        /// Aktiviert eine Quellgruppe (Transponder).
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdTune_Click( object sender, EventArgs e )
        {
            // Attach to source
            SourceItem item = (SourceItem) selStation.SelectedItem;
            if (null == item)
                return;

            // Be safe
            try
            {
                // Forward
                CurrentRequest = CurrentServer.BeginSelect( item.Selection.SelectionKey );
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, ex.Message, Text );
            }

            // Update GUI
            UpdateGUI();
        }

        /// <summary>
        /// Generelle Freigabe von Anfragen an den aktuellen <i>Card Server</i>.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdStop_EnabledChanged( object sender, EventArgs e )
        {
            // Forward
            selStation_SelectionChangeCommitted( selStation, EventArgs.Empty );
        }

        /// <summary>
        /// Es wurde ein Sender ausgewählt.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selStation_SelectionChangeCommitted( object sender, EventArgs e )
        {
            // Just update
            cmdTune.Enabled = cmdStop.Enabled && (null != selStation.SelectedItem);
            cmdSetZapping.Enabled = cmdTune.Enabled;
            cmdUnReceive.Enabled = cmdTune.Enabled;
            cmdStartPSI.Enabled = cmdStop.Enabled;
            cmdStartEPG.Enabled = cmdStop.Enabled;
            cmdStopPSI.Enabled = cmdStop.Enabled;
            cmdReceive.Enabled = cmdTune.Enabled;
            cmdEPGEnd.Enabled = cmdStop.Enabled;
            cmdStream.Enabled = cmdTune.Enabled;
            cmdUnAll.Enabled = cmdStop.Enabled;
            cmdState.Enabled = cmdStop.Enabled;
        }

        /// <summary>
        /// Aktiviert den Empfang einer Quelle.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdReceive_Click( object sender, EventArgs e )
        {
            // Attach to source
            SourceItem item = (SourceItem) selStation.SelectedItem;
            if (null == item)
                return;

            // Be safe
            try
            {
                // Ask user what to do
                using (StartRecording dlg = new StartRecording())
                {
                    // Load it
                    dlg.LoadItems( selStation.Items, item );

                    // Show up
                    if (DialogResult.OK != dlg.ShowDialog( this ))
                        return;

                    // Forward
                    CurrentRequest = CurrentServer.BeginAddSources( dlg.GetSources( txDir.Text ) );

                    // Add result processor
                    m_ResultProcessor = ProcessNewStream;
                }
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, ex.Message, Text );
            }

            // Update GUI
            UpdateGUI();
        }

        /// <summary>
        /// Stoppt den Empfang einer Quelle.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdUnReceive_Click( object sender, EventArgs e )
        {
            // Attach to source
            SourceItem item = (SourceItem) selStation.SelectedItem;
            if (null == item)
                return;

            // Be safe
            try
            {
                // Forward
                CurrentRequest = CurrentServer.BeginRemoveSource( item.Selection.Source );
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, ex.Message, Text );
            }

            // Update GUI
            UpdateGUI();
        }

        /// <summary>
        /// Beendet den Empfang für alle Quellen.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdUnAll_Click( object sender, EventArgs e )
        {
            // Be safe
            try
            {
                // Forward
                CurrentRequest = CurrentServer.BeginRemoveAllSources();
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, ex.Message, Text );
            }

            // Update GUI
            UpdateGUI();
        }

        /// <summary>
        /// Zeigt den Zustand des <i>Card Servers</i> an.
        /// </summary>
        /// <param name="result">Gesamtinformationen zum Zustand.</param>
        private void ProcessState( object result )
        {
            // Convert
            ServerInformation info = (ServerInformation) result;

            // Reconstruct the selection
            SourceSelection selection = new SourceSelection { SelectionKey = info.Selection };

            // Fill
            txLocation.Text = ((null == selection) || (null == selection.Location)) ? null : selection.Location.ToString();
            txGroup.Text = ((null == selection) || (null == selection.Group)) ? null : selection.Group.ToString();
            txProfile.Text = (null == selection) ? null : selection.ProfileName;
            txEPGItems.Text = info.CurrentProgramGuideItems.ToString();
            epgProgress.Visible = info.ProgramGuideProgress.HasValue;
            txPSISources.Text = info.UpdateSourceCount.ToString();
            psiProgress.Visible = info.UpdateProgress.HasValue;

            // Set error mode
            txLocation.BackColor = info.HasGroupInformation ? txProfile.BackColor : Color.Yellow;
            txGroup.BackColor = txLocation.BackColor;

            // Show 
            pnlState.Visible = true;

            // Set the progress values
            if (epgProgress.Visible)
                epgProgress.Value = Math.Max( 0, Math.Min( epgProgress.Maximum, (int) (info.ProgramGuideProgress.Value * epgProgress.Maximum) ) );
            if (psiProgress.Visible)
                psiProgress.Value = Math.Max( 0, Math.Min( psiProgress.Maximum, (int) (info.UpdateProgress.Value * psiProgress.Maximum) ) );

            // Reset service list
            selServices.Items.Clear();

            // Fill service list
            if (null != info.Services)
            {
                // Helper
                List<string> services = new List<string>();

                // Fill
                foreach (ServiceInformation service in info.Services)
                    services.Add( string.Format( "{0} ({1})", service.UniqueName, service.Service ) );

                // Sort and use
                selServices.Items.AddRange( services.OrderBy( s => int.Parse( s.Split( ',' )[0] ) ).ToArray() );
            }

            // Finish service list
            selServices.Enabled = (selServices.Items.Count > 0);

            // Select first
            if (selServices.Enabled)
                selServices.SelectedIndex = 0;

            // Load streams
            LoadStreams( info.Streams, selection );
        }

        /// <summary>
        /// Bearbeitet das Ergebnis einer Aktualisierung der Programmzeitschrift.
        /// </summary>
        /// <param name="result">Die Liste der einzelnen Einträge.</param>
        private void ProcessEPG( object result )
        {
            // Convert
            ProgramGuideItem[] items = (ProgramGuideItem[]) result;

            // Ask user
            if (DialogResult.OK != dlgEPG.ShowDialog( this ))
                return;

            // Be safe
            try
            {
                // Create configuration
                XmlWriterSettings settings = new XmlWriterSettings { Indent = true, Encoding = Encoding.Unicode };

                // Create serializer
                XmlSerializer serializer = new XmlSerializer( items.GetType() );

                // Store
                using (XmlWriter writer = XmlWriter.Create( dlgEPG.FileName, settings ))
                    serializer.Serialize( writer, items );
            }
            catch (Exception e)
            {
                // Report
                MessageBox.Show( this, e.Message, Text );
            }
        }

        /// <summary>
        /// Füllt die Informationen über die aktiven Quellen.
        /// </summary>
        /// <param name="streams">Die Liste der aktiven Quellen.</param>
        /// <param name="selection">Die aktive Quellgruppe.</param>
        private void LoadStreams( IEnumerable<StreamInformation> streams, SourceSelection selection )
        {
            // Reset
            lstStreams.Items.Clear();

            // Find the profile
            Profile profile = (null == selection) ? null : selection.GetProfile();

            // Process all
            foreach (StreamInformation stream in streams)
            {
                // Create the item
                ListViewItem item = lstStreams.Items.Add( stream.Source.ToString() );

                // No profile
                if (null != profile)
                    foreach (SourceSelection match in profile.FindSource( stream.Source ))
                    {
                        // Check for location match
                        if (null != selection.Location)
                            if (!Equals( selection.Location, match.Location ))
                                continue;

                        // Check for group match
                        if (null != selection.Group)
                            if (!Equals( selection.Group, match.Group ))
                                continue;

                        // Update name
                        item.Text = match.DisplayName;
                        // Skip the rest
                        break;
                    }

                // Add additional data
                item.SubItems.Add( (null == stream.Streams) ? null : string.Join( ", ", stream.Streams.MP2Tracks.Languages.ToArray() ) );
                item.SubItems.Add( (null == stream.Streams) ? null : string.Join( ", ", stream.Streams.AC3Tracks.Languages.ToArray() ) );
                item.SubItems.Add( (null == stream.Streams) ? null : string.Join( ", ", stream.Streams.SubTitles.Languages.ToArray() ) );
                item.SubItems.Add( (null == stream.Streams) ? null : (stream.Streams.Videotext ? "x" : null) );
                item.SubItems.Add( (null == stream.Streams) ? null : (stream.Streams.ProgramGuide ? "x" : null) );
                item.SubItems.Add( string.Format( "{0} ({1})", stream.BytesReceived, stream.CurrentAudioVideoBytes ) );
                item.SubItems.Add( stream.IsDecrypting ? "x" : null );
                item.SubItems.Add( stream.StreamTarget );
                item.SubItems.Add( stream.TargetPath );
                item.SubItems.Add( stream.ConsumerCount.ToString() );
                item.SubItems.Add( string.Join( ", ", stream.AllFiles.Select( f => string.Format( "{0} ({1})", f.FilePath, f.VideoType ) ).ToArray() ) );
            }

            // Set columns
            foreach (ColumnHeader column in lstStreams.Columns)
            {
                // First
                column.AutoResize( ColumnHeaderAutoResizeStyle.ColumnContent );

                // Remember
                int w = column.Width;

                // Second
                column.AutoResize( ColumnHeaderAutoResizeStyle.HeaderSize );

                // Both
                column.Width = Math.Max( column.Width, w );
            }
        }

        /// <summary>
        /// Ermittelt den aktuellen Zustand des <i>Card Servers</i>.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdState_Click( object sender, EventArgs e )
        {
            // Be safe
            try
            {
                // Forward
                CurrentRequest = CurrentServer.BeginGetState();

                // Install processor
                m_ResultProcessor = ProcessState;
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, ex.Message, Text );
            }

            // Update GUI
            UpdateGUI();
        }

        /// <summary>
        /// Wählt ein Aufzeichnungsverzeichnis aus.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdSelDir_Click( object sender, EventArgs e )
        {
            // Preset
            if (!string.IsNullOrEmpty( Properties.Settings.Default.Directory ))
                selFolder.SelectedPath = Properties.Settings.Default.Directory;

            // Ask user
            if (DialogResult.OK != selFolder.ShowDialog( this ))
                return;

            // Load
            txDir.Text = selFolder.SelectedPath;

            // Just remember
            Properties.Settings.Default.Directory = selFolder.SelectedPath;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Aktiviert oder deaktivert den Netzwerkversand.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdStream_Click( object sender, EventArgs e )
        {
            // Attach to source
            SourceItem item = (SourceItem) selStation.SelectedItem;
            if (null == item)
                return;

            // Be safe
            try
            {
                // Load
                string target = txStream.Text.Trim();

                // Forward
                CurrentRequest = CurrentServer.BeginSetStreamTarget( item.Selection.Source, string.IsNullOrEmpty( target ) ? null : target );
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, ex.Message, Text );
            }

            // Update GUI
            UpdateGUI();
        }

        /// <summary>
        /// Beginnt die Sammlung für die Programmzeitschrift.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdStartEPG_Click( object sender, EventArgs e )
        {
            // Attach to source
            SourceItem item = (SourceItem) selStation.SelectedItem;

            // Be safe
            try
            {
                // List of sources
                List<SourceIdentifier> sources = new List<SourceIdentifier>();

                // Add the one
                if (null != item)
                    sources.Add( item.Selection.Source );

                // Start
                CurrentRequest = CurrentServer.BeginStartEPGCollection( sources.ToArray(), EPGExtensions.PREMIEREDirect | EPGExtensions.PREMIERESport | EPGExtensions.FreeSatUK );
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, ex.Message, Text );
            }

            // Update GUI
            UpdateGUI();
        }

        /// <summary>
        /// Beendet die aktuelle Sammlung für die Programmzeitschrift.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdEPGEnd_Click( object sender, EventArgs e )
        {
            // Be safe
            try
            {
                // Start
                CurrentRequest = CurrentServer.BeginEndEPGCollection();

                // Install processor
                m_ResultProcessor = ProcessEPG;
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, ex.Message, Text );
            }

            // Update GUI
            UpdateGUI();
        }

        /// <summary>
        /// Versteckt die Zustandsanzeige wieder.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void pnlState_Click( object sender, EventArgs e )
        {
            // Hide
            pnlState.Visible = false;

            // Refresh
            UpdateGUI();
        }

        /// <summary>
        /// Beginnt die Aktualisierung der Quellen (Sendersuchlauf).
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdStartPSI_Click( object sender, EventArgs e )
        {
            // Be safe
            try
            {
                // Start
                CurrentRequest = CurrentServer.BeginStartScan();
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, ex.Message, Text );
            }

            // Update GUI
            UpdateGUI();
        }

        /// <summary>
        /// Aktualisiert die Anzeige nach einem Sendersuchlauf mit gespeichertem Profil.
        /// </summary>
        /// <param name="state">Wird ignoriert.</param>
        private void ProcessScan( object state )
        {
            // Reload profiles
            ProfileManager.Refresh();

            // Get the selection
            ProfileItem item = (ProfileItem) selProfiles.SelectedItem;
            if (null == item)
                return;

            // Find
            Profile profile = ProfileManager.FindProfile( item.Profile.Name );
            if (null == profile)
                return;

            // Reload
            item.Profile = profile;

            // Load station selection list
            selStation.Items.Clear();
            selStation.Items.AddRange( SourceItem.GetSourceItems( item.Profile ) );
        }

        /// <summary>
        /// Beendet die Aktualisierung der Quellen (Sendersuchlauf).
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdPSIStop_Click( object sender, EventArgs e )
        {
            // Be safe
            try
            {
                // Ask user
                switch (MessageBox.Show( this, Properties.Resources.Ask_SaveProfile, Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question ))
                {
                    case DialogResult.Yes: CurrentRequest = CurrentServer.BeginEndScan( true ); m_ResultProcessor = ProcessScan; break;
                    case DialogResult.No: CurrentRequest = CurrentServer.BeginEndScan( null ); break;
                    default: break;
                }
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, ex.Message, Text );
            }

            // Update GUI
            UpdateGUI();
        }

        /// <summary>
        /// Aktiviert eine einzelne Quelle für den <i>Zapping Modus</i>.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdSetZapping_Click( object sender, EventArgs e )
        {
            // Attach to source
            SourceItem item = (SourceItem) selStation.SelectedItem;
            if (null == item)
                return;

            // Be safe
            try
            {
                // Forward
                CurrentRequest = CurrentServer.BeginSetZappingSource( item.Selection.SelectionKey, txStream.Text );

                // Install processor
                m_ResultProcessor = ProcessState;
            }
            catch (Exception ex)
            {
                // Report
                MessageBox.Show( this, ex.Message, Text );
            }

            // Update GUI
            UpdateGUI();
        }
    }
}
