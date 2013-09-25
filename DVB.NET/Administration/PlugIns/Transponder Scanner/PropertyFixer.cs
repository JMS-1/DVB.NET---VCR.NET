using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace JMS.DVB.Administration.SourceScanner
{
    /// <summary>
    /// Über diesen Dialog können die Aspekte einzelner Quellen festgelegt werden.
    /// </summary>
    public partial class PropertyFixer : UserControl, IPlugInControl
    {
        /// <summary>
        /// Beschreibt die Auswahl einer Tonspur.
        /// </summary>
        private class AudioItem
        {
            /// <summary>
            /// Die zugehörigen Nutzdaten.
            /// </summary>
            public AudioInformation Information { get; private set; }

            /// <summary>
            /// Erzeugt eine neue Beschreibung.
            /// </summary>
            /// <param name="information">Die zu verwaltenden Daten.</param>
            public AudioItem( AudioInformation information )
            {
                // Remember
                Information = information;
            }

            /// <summary>
            /// Erzeugt einen Anzeigetext.
            /// </summary>
            /// <returns>Der Anzeigetext.</returns>
            public override string ToString()
            {
                // Construct
                return string.Format( "{0} {1} ({2})", Information.AudioType, Information.Language, Information.AudioStream );
            }
        }

        /// <summary>
        /// Die zugehörige Erweiterung.
        /// </summary>
        private SourceProperties m_PlugIn;

        /// <summary>
        /// Eine Methode, mit der das Administrationswerkzeug zur
        /// Aktualisierung der Schaltflächen aufgefordert werden kann.
        /// </summary>
        private IPlugInUISite m_Site;

        /// <summary>
        /// Das Anzeigeformat für die Anzahl der gesondert markierten Quellen.
        /// </summary>
        private string m_CountFormat;

        /// <summary>
        /// Wird gesetzt, sobald der Dialog initialisiert ist.
        /// </summary>
        private bool m_Running;

        /// <summary>
        /// Alle vorgenommenen Veränderungen.
        /// </summary>
        private Dictionary<SourceIdentifier, SourceModifier> m_Modifiers = new Dictionary<SourceIdentifier, SourceModifier>();

        /// <summary>
        /// Erzeugt einen neuen Dialog.
        /// </summary>
        /// <param name="configuration">Die Konfiguration für diese Auswahl.</param>
        /// <param name="site">Die Arbeitsumgebung der Erweiterung - üblicherweise das DVB.NET
        /// Administrationswerkzeug.</param>
        public PropertyFixer( SourceProperties configuration, IPlugInUISite site )
        {
            // Remember
            m_PlugIn = configuration;
            m_Site = site;

            // Fill data from designer
            InitializeComponent();

            // Remember
            m_CountFormat = lbGroups.Text;
        }

        /// <summary>
        /// Zeigt die Anzahl der gesondert behandelten Quellen.
        /// </summary>
        private void ShowCount()
        {
            // Just load
            lbGroups.Text = string.Format( m_CountFormat, lstSources.CheckedItems.Count );
        }

        /// <summary>
        /// Initialisiert den Dialog.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void PropertyFixer_Load( object sender, EventArgs e )
        {
            // Load name
            lbProfile.Text = string.Format( lbProfile.Text, m_PlugIn.Profile.Name );

            // Create a list
            List<ListViewItem> items = new List<ListViewItem>();

            // All sources which have modifications
            Dictionary<SourceIdentifier, bool> modified = new Dictionary<SourceIdentifier, bool>();

            // Find all
            foreach (SourceModifier modifier in m_PlugIn.Profile.ScanConfiguration.SourceDetails)
                modified[modifier] = true;

            // Fill in all
            foreach (GroupLocation location in m_PlugIn.Profile.Locations)
                foreach (SourceGroup group in location.Groups)
                    foreach (Station source in group.Sources)
                    {
                        // Create the item
                        ListViewItem item = new ListViewItem( string.Format( "{0} ({1})", source.FullName, source.Service ) );

                        // Attach the source and finish
                        item.Checked = modified.ContainsKey( source );
                        item.Tag = source;

                        // Remember
                        items.Add( item );
                    }

            // Fill
            lstSources.Items.AddRange( items.ToArray() );

            // Configure
            lstSources.Columns[0].AutoResize( ColumnHeaderAutoResizeStyle.ColumnContent );

            // Check for selection
            if (lstSources.CheckedItems.Count > 0)
                lstSources.CheckedItems[0].EnsureVisible();

            // Show hits
            ShowCount();

            // Active
            m_Running = true;

            // Reset selection
            lstSources_SelectedIndexChanged( lstSources, EventArgs.Empty );
        }

        /// <summary>
        /// Eine Auswahl wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void lstSources_ItemChecked( object sender, ItemCheckedEventArgs e )
        {
            // Not yet
            if (!m_Running)
                return;

            // Just update
            ShowCount();

            // Select this one
            e.Item.Selected = true;

            // Reload
            lstSources_SelectedIndexChanged( lstSources, EventArgs.Empty );
        }

        /// <summary>
        /// Meldet die aktuelle Änderungskonfiguration.
        /// </summary>
        private SourceModifier CurrentModifier
        {
            get
            {
                // Forward
                return m_Modifiers[(SourceIdentifier) lstSources.SelectedItems[0].Tag];
            }
        }

        /// <summary>
        /// Die Auswahl wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void lstSources_SelectedIndexChanged( object sender, EventArgs e )
        {
            // Block for changes
            m_Running = false;

            // Disable all
            selAudio.Enabled = false;
            selAudioType.Enabled = false;
            selAudioPID.Enabled = false;
            cmdAddAudio.Enabled = false;
            cmdDelAudio.Enabled = false;
            txAudioLanguage.Enabled = false;

            // Reset all
            txName.Text = null;
            txProvider.Text = null;
            selType.SelectedIndex = 0;
            ckEncrypted.CheckState = CheckState.Indeterminate;
            ckService.CheckState = CheckState.Indeterminate;
            selVideoPID.Value = selVideoPID.Maximum;
            selTextPID.Value = selTextPID.Maximum;
            ckEPG.CheckState = CheckState.Checked;
            selVideo.SelectedIndex = 0;
            ckAudio.CheckState = CheckState.Indeterminate;
            selAudio.Items.Clear();
            selAudioType.SelectedItem = null;
            selAudioPID.Value = selAudioPID.Maximum;
            txAudioLanguage.Text = null;

            // Disable
            grpData.Enabled = false;

            // Process the selected item
            foreach (ListViewItem item in lstSources.SelectedItems)
            {
                // Attach to source
                SourceIdentifier source = (SourceIdentifier) item.Tag;

                // Enable
                grpData.Enabled = item.Checked;

                // Check mode
                if (item.Checked)
                {
                    // Load modifier
                    SourceModifier modifier;
                    if (!m_Modifiers.TryGetValue( source, out modifier ))
                        modifier = m_PlugIn.Profile.GetFilter( source ).Clone();
                    else if (null == modifier)
                        modifier = new SourceModifier { Network = source.Network, TransportStream = source.TransportStream, Service = source.Service };

                    // Remember
                    m_Modifiers[source] = modifier;

                    // Load all
                    if (!string.IsNullOrEmpty( modifier.Name ))
                        txName.Text = modifier.Name;
                    if (!string.IsNullOrEmpty( modifier.Provider ))
                        txProvider.Text = modifier.Provider;
                    if (modifier.IsEncrypted.HasValue)
                        ckEncrypted.CheckState = modifier.IsEncrypted.Value ? CheckState.Checked : CheckState.Unchecked;
                    if (modifier.IsService.HasValue)
                        ckService.CheckState = modifier.IsService.Value ? CheckState.Checked : CheckState.Unchecked;
                    if (modifier.SourceType.HasValue)
                        selType.SelectedIndex = 1 + (int) modifier.SourceType.Value;
                    if (modifier.VideoType.HasValue)
                        selVideo.SelectedIndex = 1 + (int) modifier.VideoType.Value;
                    if (modifier.VideoStream.HasValue)
                        selVideoPID.Value = modifier.VideoStream.Value;
                    if (modifier.TextStream.HasValue)
                        selTextPID.Value = modifier.TextStream.Value;
                    ckEPG.Checked = !modifier.DisableProgramGuide;

                    // Audio
                    if (null != modifier.AudioStreams)
                    {
                        // Enable flag
                        ckAudio.CheckState = (modifier.AudioStreams.Length > 0) ? CheckState.Checked : CheckState.Unchecked;

                        // Load all
                        foreach (AudioInformation audio in modifier.AudioStreams)
                            selAudio.Items.Add( new AudioItem( audio ) );
                    }
                }
                else
                {
                    // Wipe out settings
                    m_Modifiers[source] = null;
                }

                // Done
                break;
            }

            // Back
            m_Running = true;
        }

        /// <summary>
        /// Der Name einer Quelle wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void txName_TextChanged( object sender, EventArgs e )
        {
            // Store
            if (m_Running)
                CurrentModifier.Name = txName.Text;
        }

        /// <summary>
        /// Der Name des Dienstanbieters wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void txProvider_TextChanged( object sender, EventArgs e )
        {
            // Store
            if (m_Running)
                CurrentModifier.Provider = txProvider.Text;
        }

        /// <summary>
        /// Das Bildformat wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selVideo_SelectedIndexChanged( object sender, EventArgs e )
        {
            // Store
            if (m_Running)
                switch (selVideo.SelectedIndex)
                {
                    case 0: CurrentModifier.VideoType = null; break;
                    case 1: CurrentModifier.VideoType = VideoTypes.Unknown; break;
                    case 2: CurrentModifier.VideoType = VideoTypes.NoVideo; break;
                    case 3: CurrentModifier.VideoType = VideoTypes.MPEG2; break;
                    case 4: CurrentModifier.VideoType = VideoTypes.H264; break;
                }
        }

        /// <summary>
        /// Die Verschüsselung wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ckEncrypted_CheckStateChanged( object sender, EventArgs e )
        {
            // Store
            if (m_Running)
                switch (ckEncrypted.CheckState)
                {
                    case CheckState.Indeterminate: CurrentModifier.IsEncrypted = null; break;
                    case CheckState.Checked: CurrentModifier.IsEncrypted = true; break;
                    case CheckState.Unchecked: CurrentModifier.IsEncrypted = false; break;
                }
        }

        /// <summary>
        /// Die Diensteigenschaft wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ckService_CheckStateChanged( object sender, EventArgs e )
        {
            // Store
            if (m_Running)
                switch (ckService.CheckState)
                {
                    case CheckState.Indeterminate: CurrentModifier.IsService = null; break;
                    case CheckState.Checked: CurrentModifier.IsService = true; break;
                    case CheckState.Unchecked: CurrentModifier.IsService = false; break;
                }
        }

        /// <summary>
        /// Die Integration der Programmzeitschrift wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ckEPG_CheckedChanged( object sender, EventArgs e )
        {
            // Store
            if (m_Running)
                CurrentModifier.DisableProgramGuide = !ckEPG.Checked;
        }

        /// <summary>
        /// Die Art des Datenstroms wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selType_SelectedIndexChanged( object sender, EventArgs e )
        {
            // Store
            if (m_Running)
                switch (selType.SelectedIndex)
                {
                    case 0: CurrentModifier.SourceType = null; break;
                    case 1: CurrentModifier.SourceType = SourceTypes.Unknown; break;
                    case 2: CurrentModifier.SourceType = SourceTypes.TV; break;
                    case 3: CurrentModifier.SourceType = SourceTypes.Radio; break;
                }
        }

        /// <summary>
        /// Die Datenstromkennung für das Bildsignal wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selVideoPID_ValueChanged( object sender, EventArgs e )
        {
            // Store
            if (m_Running)
                if (selVideoPID.Value == selVideoPID.Maximum)
                    CurrentModifier.VideoStream = null;
                else
                    CurrentModifier.VideoStream = (ushort) selVideoPID.Value;
        }

        /// <summary>
        /// Die Datenstromkennung für den Videotext wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selTextPID_ValueChanged( object sender, EventArgs e )
        {
            // Store
            if (m_Running)
                if (selTextPID.Value == selTextPID.Maximum)
                    CurrentModifier.TextStream = null;
                else
                    CurrentModifier.TextStream = (ushort) selTextPID.Value;
        }

        /// <summary>
        /// Die Auswahl der Tonspuren wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void ckAudio_CheckStateChanged( object sender, EventArgs e )
        {
            // Forward
            selAudio.Enabled = (ckAudio.CheckState != CheckState.Indeterminate);
            cmdAddAudio.Enabled = (ckAudio.CheckState == CheckState.Checked);

            // Wipe out
            if (m_Running)
                if (!cmdAddAudio.Enabled)
                {
                    // Forget
                    CurrentModifier.AudioStreams = selAudio.Enabled ? new AudioInformation[0] : null;

                    // Deselect all
                    selAudio.Items.Clear();

                    // Reload
                    selAudio_SelectedIndexChanged( selAudio, EventArgs.Empty );
                }
        }

        /// <summary>
        /// Die Daten einer Tonspur sollen angezeigt werden.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selAudio_SelectedIndexChanged( object sender, EventArgs e )
        {
            // Disable first
            selAudioType.Enabled = false;
            selAudioPID.Enabled = false;
            txAudioLanguage.Enabled = false;
            cmdDelAudio.Enabled = false;

            // Reset
            selAudioType.SelectedItem = null;
            selAudioPID.Value = selAudioPID.Maximum;
            txAudioLanguage.Text = null;

            // Get the selection
            AudioItem item = (AudioItem) selAudio.SelectedItem;
            if (null == item)
                return;

            // Fill in all
            switch (item.Information.AudioType)
            {
                case AudioTypes.MP2: selAudioType.SelectedIndex = 0; break;
                case AudioTypes.AC3: selAudioType.SelectedIndex = 1; break;
            }

            // Finish
            txAudioLanguage.Text = item.Information.Language;
            selAudioPID.Value = item.Information.AudioStream;
            cmdDelAudio.Enabled = true;
        }

        /// <summary>
        /// Entfernt eine Tonspur.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdDelAudio_Click( object sender, EventArgs e )
        {
            // Get the selection
            AudioItem item = (AudioItem) selAudio.SelectedItem;

            // Load list
            List<AudioInformation> audios = new List<AudioInformation>( CurrentModifier.AudioStreams );

            // Remove
            audios.Remove( item.Information );

            // Push back
            CurrentModifier.AudioStreams = audios.ToArray();

            // Remove item
            selAudio.Items.Remove( item );

            // Update
            selAudio_SelectedIndexChanged( selAudio, EventArgs.Empty );
        }

        /// <summary>
        /// Ergänzt eine neue Tonspur.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdAddAudio_Click( object sender, EventArgs e )
        {
            // Create new
            AudioInformation audio = new AudioInformation { AudioType = AudioTypes.MP2, AudioStream = 0, Language = "Deutsch" };

            // Create list
            List<AudioInformation> audios = new List<AudioInformation>();

            // Fill list
            if (null != CurrentModifier.AudioStreams)
                audios.AddRange( CurrentModifier.AudioStreams );

            // Append new
            audios.Add( audio );

            // Push back
            CurrentModifier.AudioStreams = audios.ToArray();

            // New item
            AudioItem item = new AudioItem( audio );

            // Push back
            selAudio.Items.Add( item );

            // Select it
            selAudio.SelectedItem = item;

            // Update
            selAudio_SelectedIndexChanged( selAudio, EventArgs.Empty );

            // Enable edit
            txAudioLanguage.Enabled = true;
            selAudioType.Enabled = true;
            selAudioPID.Enabled = true;
        }

        /// <summary>
        /// Meldet die Daten zur aktuellen Tonspur.
        /// </summary>
        public AudioInformation CurrentAudio
        {
            get
            {
                // Report
                return ((AudioItem) selAudio.SelectedItem).Information;
            }
        }

        /// <summary>
        /// Die Sprache der aktuellen Tonspur wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void txAudioLanguage_TextChanged( object sender, EventArgs e )
        {
            // Store
            if (txAudioLanguage.Enabled)
            {
                // Change
                CurrentAudio.Language = txAudioLanguage.Text;

                // Refresh
                selAudio.Invalidate();
            }
        }

        /// <summary>
        /// Die Art der aktuellen Tonspur wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selAudioType_SelectedIndexChanged( object sender, EventArgs e )
        {
            // Store
            if (selAudioType.Enabled)
            {
                // Change
                switch (selAudioType.SelectedIndex)
                {
                    case 0: CurrentAudio.AudioType = AudioTypes.MP2; break;
                    case 1: CurrentAudio.AudioType = AudioTypes.AC3; break;
                }

                // Refresh
                selAudio.Invalidate();
            }
        }

        /// <summary>
        /// Der Datenstrom der aktuellen Tonspur wurde verändert.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selAudioPID_ValueChanged( object sender, EventArgs e )
        {
            // Store
            if (selAudioPID.Enabled)
            {
                // Change
                CurrentAudio.AudioStream = (ushort) selAudioPID.Value;

                // Refresh
                selAudio.Invalidate();
            }
        }

        #region IPlugInControl Members

        /// <summary>
        /// Beginnt mit der Ausführung der Aufgabe.
        /// </summary>
        /// <returns>Gesetzt, wenn die Aufgabe synchron abgeschlossen wurde.</returns>
        bool IPlugInControl.Start()
        {
            // Attach to the configuration
            List<SourceModifier> modifiers = m_PlugIn.Profile.ScanConfiguration.SourceDetails;

            // Remove all entries we touched
            modifiers.RemoveAll( m => m_Modifiers.ContainsKey( m ) );

            // Add all modified
            foreach (KeyValuePair<SourceIdentifier, SourceModifier> modifier in m_Modifiers)
                if (null != modifier.Value)
                {
                    // Append it
                    modifiers.Add( modifier.Value );

                    // Apply to source
                    modifier.Value.ApplyTo( (Station) modifier.Key );
                }

            // Store
            m_PlugIn.Profile.Save();

            // Done
            return true;
        }

        /// <summary>
        /// Meldet, ob eine Aufgabe unterbrochen werden kann.
        /// </summary>
        bool IPlugInControl.CanCancel
        {
            get
            {
                // We can't 
                return false;
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

        #endregion
    }
}
