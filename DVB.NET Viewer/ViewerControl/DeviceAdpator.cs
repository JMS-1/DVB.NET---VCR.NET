using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

using JMS.DVB.TS;
using JMS.DVB.SI;
using JMS.DVB.Algorithms;
using JMS.DVB.DirectShow.AccessModules;


namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Diese Klasse realisiert den Zugriff auf eine lokale DVB.NET Hardware
    /// Abstraktion.
    /// </summary>
    public class DeviceAdpator : Adaptor
    {
        #region VideoText PES Parser

        /// <summary>
        /// Verbindet die PES Analyse mit der VideoText Analyse.
        /// </summary>
        private class TTXStreamConsumer : IStreamConsumer
        {
            /// <summary>
            /// Die zugehörige Zugriffsinstanz.
            /// </summary>
            public readonly DeviceAdpator Adaptor;

            /// <summary>
            /// Erzeugt eine neue Verbindungsinstanz.
            /// </summary>
            /// <param name="adaptor">Die zugehörige Zugriffsinstanz auf die lokale DVB Hardware.</param>
            public TTXStreamConsumer( DeviceAdpator adaptor )
            {
                // Remember
                Adaptor = adaptor;
            }

            #region IStreamConsumer Members

            /// <summary>
            /// Meldet, ob bereits PCR Daten verfügbar sind.
            /// </summary>
            bool IStreamConsumer.PCRAvailable
            {
                get
                {
                    // Make sure that analyses always runs
                    return true;
                }
            }

            /// <summary>
            /// Wertet ein PES Paket aus.
            /// </summary>
            /// <param name="counter">Der <i>Transport Stream</i> Paketzähler.</param>
            /// <param name="pid">Die Datenstromkennung dieses Datenstroms.</param>
            /// <param name="buffer">Die Daten, in denen das Paket enthalten ist.</param>
            /// <param name="start">Das erste Byte, das zum Paket gehört.</param>
            /// <param name="packs">Die Anzahl vollständiger <i>Transport Stream</i> Pakete.</param>
            /// <param name="isFirst">Gesetzt, wenn die Daten einen PES Kopf enthalten.</param>
            /// <param name="sizeOfLast">Anzahl der Datenbytes im letzten <i>Transport Stream</i> Paket.</param>
            /// <param name="pts">Die aktuelle Zeitbasis als <i>Presentation Time Stamp</i> - <i>-1</i> wird verwendet,
            /// wenn mit diesem Paket keine Zeitbasis verbunden ist.</param>
            void IStreamConsumer.Send( ref int counter, int pid, byte[] buffer, int start, int packs, bool isFirst, int sizeOfLast, long pts )
            {
                // The real processing
                Adaptor.AnalyseVideoText( isFirst, buffer, start, (packs - 1) * Manager.PacketSize + sizeOfLast, pts );
            }

            /// <summary>
            /// Überträgt die Systemzeit.
            /// </summary>
            /// <param name="counter">Der <i>Transport Stream</i> Paketzähler.</param>
            /// <param name="pid">Die Datenstromkennung dieses Datenstroms.</param>
            /// <param name="pts">Die aktuelle Zeitbasis, die in die Systemzeit umgesetzt werden soll.</param>
            void IStreamConsumer.SendPCR( int counter, int pid, long pts )
            {
                // No need to do this
            }

            #endregion
        }

        /// <summary>
        /// Schafft eine Verbindung zwischen dem PES Datenstrom und der VideoText Analyse.
        /// </summary>
        private TTXStreamConsumer m_TTXConnector;

        #endregion

        /// <summary>
        /// Analysiert die EPG Daten zur Anzeige der NVOD Dienste.
        /// </summary>
        private ServiceParser ServiceParser = null;

        /// <summary>
        /// Aktuelle Aufzeichnungsdatei.
        /// </summary>
        private SourceStreamsManager RecordingStream = null;

        /// <summary>
        /// Der aktuell ausgewählte Sender.
        /// </summary>
        private SourceSelection CurrentSelection = null;

        /// <summary>
        /// Die Informationen zu den Teildatenströmen.
        /// </summary>
        private SourceInformation CurrentSourceConfiguration = null;

        /// <summary>
        /// Das aktuell zugehörige NVOD Portal.
        /// </summary>
        private SourceSelection CurrentPortal = null;

        /// <summary>
        /// Der aktuellen NVOD Dienstes.
        /// </summary>
        private ServiceItem CurrentService = null;

        /// <summary>
        /// Die aktuell verwendete Tonspur.
        /// </summary>
        private AudioInformation CurrentAudio;

        /// <summary>
        /// Das zu verwendende DVB.NET Geräteprofil.
        /// </summary>
        private Profile Profile;

        /// <summary>
        /// Das zugehörige Gerät, zwischengespeichert zur schnellen Nutzung.
        /// </summary>
        private Hardware Device;

        /// <summary>
        /// Name der zuletzt aktivierten Tonspur.
        /// <seealso cref="SetAudio"/>
        /// </summary>
        private string m_LastAudio = null;

        /// <summary>
        /// Zugriff auf die Informationen zur Senderliste.
        /// </summary>
        public readonly IChannelInfo ChannelInfo;

        /// <summary>
        /// Zugriff auf die Einstellungen bei Verwendung einer lokalen
        /// DVB.NET Hardware.
        /// </summary>
        public readonly ILocalInfo LocalInfo;

        /// <summary>
        /// Vermittelt den Zugriff auf Informationen zum Netzwerkversand.
        /// </summary>
        public readonly IStreamInfo StreamInfo;

        /// <summary>
        /// Informationen zur Arbeitsumgebung.
        /// </summary>
        public readonly IGeneralInfo GeneralInfo;

        /// <summary>
        /// Die Kennung des Videodatenstrom.
        /// </summary>
        private Guid VideoId = Guid.Empty;

        /// <summary>
        /// Die Kennung des Tondatenstroms.
        /// </summary>
        private Guid AudioId = Guid.Empty;

        /// <summary>
        /// Die Kennung des Videotextdatenstroms.
        /// </summary>
        private Guid TextId = Guid.Empty;

        /// <summary>
        /// Es wird ein Neustart der Datenströme benötigt.
        /// </summary>
        private bool m_NeedRestart = false;

        /// <summary>
        /// Wartet auf neue Senderdaten.
        /// </summary>
        private CancellableTask<SourceInformation> m_InfoReader = null;

        /// <summary>
        /// Gesetzt, wenn auf die neuen Informationen zur Quellgruppe gewartet wird.
        /// </summary>
        private bool m_HasPendingGroupInformation = false;

        /// <summary>
        /// Erzeugte eine neue Zugriffsinstanz.
        /// </summary>
        /// <param name="profile">Zu verwendende DVB.NET Hardware Abstraktion.</param>
        /// <param name="main">Zugehörige Anwendung.</param>
        public DeviceAdpator( Profile profile, IViewerSite main )
            : base( main )
        {
            // Remember
            Profile = profile;

            // Attach to the device
            Device = HardwareManager.OpenHardware( Profile );

            // Create
            m_TTXConnector = new TTXStreamConsumer( this );

            // Load alternate interfaces
            GeneralInfo = (IGeneralInfo) main;
            ChannelInfo = (IChannelInfo) main;
            StreamInfo = (IStreamInfo) main;
            LocalInfo = (ILocalInfo) main;

            // Initialize core - DirectShow Graph feed directly from a Transport Stream
            SetAccessor( new AudioVideoAccessor() );
        }

        /// <summary>
        /// Meldet das Zugriffsmodul für die Datenübertragung in den
        /// DirectShow Graphen.
        /// </summary>
        public new AudioVideoAccessor Accessor
        {
            get
            {
                // Forward
                return (AudioVideoAccessor) base.Accessor;
            }
        }

        /// <summary>
        /// Erneuert die Liste der verfügbaren Sender.
        /// </summary>
        /// <remarks>
        /// Dabei werden die Einschränkungen des Anwenders bezüglich PayTV / FreeTV
        /// und Radio / Fernsehen berücksichtigt.
        /// </remarks>
        public override void LoadStations()
        {
            // Allow restriction on channels
            Favorites.EnableFavorites();

            // Encryption
            bool allowRadio = ChannelInfo.UseRadio;
            bool allowEncrypt = ChannelInfo.PayTV;
            bool allowFree = ChannelInfo.FreeTV;
            bool allowTV = ChannelInfo.UseTV;

            // Process all
            foreach (SourceSelection source in Profile.AllSourcesByDisplayName)
            {
                // Convert
                Station station = (Station) source.Source;

                // Check for encryption
                if (station.IsEncrypted || station.IsService)
                {
                    // See if this is allowed
                    if (!allowEncrypt && allowFree)
                        continue;
                }
                else
                {
                    // See if this is allowed
                    if (!allowFree && allowEncrypt)
                        continue;
                }

                // Check for type
                if (station.SourceType == SourceTypes.Radio)
                {
                    // See if this is allowed
                    if (!allowRadio && allowTV)
                        continue;
                }
                else if (station.SourceType == SourceTypes.TV)
                {
                    // See if this is allowed
                    if (!allowTV && allowRadio)
                        continue;
                }

                // Register
                Favorites.AddChannel( source.DisplayName, source );
            }

            // Finished
            Favorites.FillChannelList();
        }

        /// <summary>
        /// Gibt alle verbundenen Ressourcen frei. Insbesondere wird eine laufende
        /// Aufzeichnung beendet.
        /// </summary>
        protected override void OnDispose()
        {
            // Stop data receiption.
            StopReceivers( false );

            // Forget
            CurrentSourceConfiguration = null;
            CurrentSelection = null;
            CurrentAudio = null;
            CurrentEntry = null;
            NextEntry = null;

            // Reset EPG display
            ShowCurrentEntry();
        }

        /// <summary>
        /// Beendet die Datenübertragung in den DirectShow Graphen und zusätzlich
        /// eine eventuell laufende Aufzeichnung.
        /// </summary>
        /// <param name="pictureOnly">Gesetzt, wenn die Aufzeichnung selbst weiter laufen soll.</param>
        private void StopReceivers( bool pictureOnly )
        {
            // Stop all consumers we registered
            Device.RemoveProgramGuideConsumer( ReceiveEPG );
            Device.SetConsumerState( VideoId, null );
            Device.SetConsumerState( AudioId, null );
            Device.SetConsumerState( TextId, null );

            // Restart videotext caching from scratch
            VideoText.Deactivate( true );

            // Partial mode
            if (pictureOnly)
                return;

            // Stop reader
            using (m_InfoReader)
                m_InfoReader = null;

            // Reset flag
            m_HasPendingGroupInformation = false;

            // Stop portal parser
            if (null != ServiceParser)
            {
                // Stop it
                ServiceParser.Disable();

                // Forget it
                ServiceParser = null;
            }

            // Stop recording, too
            using (SourceStreamsManager recording = RecordingStream)
                RecordingStream = null;
        }

        /// <summary>
        /// Ändert den aktuellen Sender.
        /// </summary>
        /// <param name="context">Eine DVB.NET <see cref="SourceSelection"/>.</param>
        /// <returns>Name und Tonspur des neuen Senders oder <i>null</i>, wenn kein
        /// Senderwechsel suchgeführt wurde.</returns>
        public override string SetStation( object context )
        {
            // From scratch
            return SetStation( (SourceSelection) context, null, null );
        }

        /// <summary>
        /// Ändert den aktuellen Sender oder wählt einen NVOD Dienst zum aktuellen
        /// Portal aus.
        /// </summary>
        /// <param name="source">Eine DVB.NET <see cref="SourceSelection"/>.</param>
        /// <param name="portal">Optional ein Portal.</param>
        /// <param name="service">Optional ein Dienst des Portals.</param>
        /// <returns>Name und Tonspur des neuen Senders oder <i>null</i>, wenn kein
        /// Senderwechsel suchgeführt wurde.</returns>
        private string SetStation( SourceSelection source, SourceSelection portal, ServiceItem service )
        {
            // Stop data transmission at once
            Accessor.Stop();

            // Get rid of stream and recording
            StopReceivers( false );

            // Tune
            source.SelectGroup();

            // Check for encryption
            var station = (Station) source.Source;
            if (station.IsEncrypted)
                try
                {
                    // Request clean stream
                    Device.Decrypt( station );
                }
                catch
                {
                    // Ignore any error
                }

            // Retrieve the source information
            var info = source.GetSourceInformationAsync().Result;

            // Remember
            CurrentSourceConfiguration = info;
            CurrentSelection = source;
            CurrentService = service;
            CurrentPortal = portal;
            CurrentEntry = null;
            NextEntry = null;

            // Reset EPG display
            ShowCurrentEntry();

            // Store to settings if not a service
            if (CurrentPortal == null)
                LocalInfo.LocalStation = source.DisplayName;

            // Load audio
            ResetAudio();

            // Choose
            return LoadingDefaults ? null : SetAudio( null );
        }

        /// <summary>
        /// Lädt die Liste der Tonspuren.
        /// </summary>
        private void ResetAudio()
        {
            // Reset all
            Favorites.ClearAudioAndService();

            // All audio
            if (null != CurrentSourceConfiguration)
                for (int i = 0; i < CurrentSourceConfiguration.AudioTracks.Count; ++i)
                    Favorites.AddAudio( CurrentSourceConfiguration.AudioTracks[i].ToString(), 0 == i );

            // Finished
            Favorites.FillAudioList();
        }

        /// <summary>
        /// Zeigt Bild, Ton und Videotext an.
        /// </summary>
        /// <param name="audio">Die zu verwendende Tonspur.</param>
        private void Activate( AudioInformation audio )
        {
            // Stop current
            StopReceivers( true );

            // Reset anything in queue
            Accessor.ClearBuffers();

            // Mode of operation
            bool mpeg4 = ((0 != CurrentSourceConfiguration.VideoStream) && (VideoTypes.H264 == CurrentSourceConfiguration.VideoType)), ac3 = false;

            // Configure video
            if (0 != CurrentSourceConfiguration.VideoStream)
                VideoId = Device.AddConsumer( CurrentSourceConfiguration.VideoStream, StreamTypes.Video, Accessor.AddVideo );

            // Use default audio
            if (null == audio)
                if (CurrentSourceConfiguration.AudioTracks.Count > 0)
                    audio = CurrentSourceConfiguration.AudioTracks[0];

            // Configure audio
            if (null != audio)
            {
                // Remember the type
                ac3 = (AudioTypes.AC3 == audio.AudioType);

                // Create the filter
                AudioId = Device.AddConsumer( audio.AudioStream, StreamTypes.Audio, Accessor.AddAudio );
            }

            // Remember for update
            CurrentAudio = audio;

            // Start streaming
            Device.SetConsumerState( VideoId, true );
            Device.SetConsumerState( AudioId, true );

            // Check for video text
            if (0 != CurrentSourceConfiguration.TextStream)
            {
                // Create the PES analyser
                TTXStream stream = new TTXStream( m_TTXConnector, (short) CurrentSourceConfiguration.TextStream, false );

                // Create the filter
                TextId = Device.AddConsumer( CurrentSourceConfiguration.TextStream, StreamTypes.VideoText, stream.AddPayload );

                // Start the filter
                Device.SetConsumerState( TextId, true );
            }

            // Now restart data transmission
            Accessor.StartGraph( mpeg4, ac3 );

            // Enable service parser if we are allowed to to so
            if (!Profile.DisableProgramGuide)
                if (!Profile.GetFilter( CurrentSelection.Source ).DisableProgramGuide)
                    Device.AddProgramGuideConsumer( ReceiveEPG );
        }

        /// <summary>
        /// Ändert die aktuelle Tonspur.
        /// </summary>
        /// <param name="audio">Name der aktuellen Tonspur gemäß der Senderliste
        /// oder <i>null</i> für die bevorzugte Tonspur.</param>
        /// <returns>Name des aktuellen Senders und der aktuellen Tonspur.</returns>
        public override string SetAudio( string audio )
        {
            // Not possible
            if (null == CurrentSourceConfiguration)
                return (null == CurrentService) ? StationName : CurrentService.Name;

            // Ask favorites for default
            if (string.IsNullOrEmpty( audio ))
                audio = Favorites.GetPreferredAudio();

            // Always remember
            m_LastAudio = audio;

            // Remember if not a service
            if (null == CurrentPortal)
                LocalInfo.LocalAudio = audio;

            // Audio to use
            AudioInformation info = null;

            // Try default
            if (!string.IsNullOrEmpty( audio ))
                info = CurrentSourceConfiguration.AudioTracks.Find( a => 0 == string.Compare( audio, a.ToString(), true ) );

            // Start up
            Activate( info );

            // Enable service parser if we are allowed to to so
            if (!Profile.DisableProgramGuide)
                if (!Profile.GetFilter( CurrentSourceConfiguration.Source ).DisableProgramGuide)
                {
                    // Stop current
                    if (null != ServiceParser)
                        ServiceParser.Disable();

                    // Create new
                    ServiceParser = new ServiceParser( Profile, (null == CurrentPortal) ? CurrentSourceConfiguration.Source : CurrentPortal.Source );
                }

            // Report full name
            return string.Format( "{0} ({1})", (null == CurrentService) ? StationName : CurrentService.Name, (null == info) ? "-" : info.ToString() );
        }

        /// <summary>
        /// Meldet, ob ein Videotext Signal vorliegt.
        /// </summary>
        public override bool TTXAvailable
        {
            get
            {
                // Report
                return ((null != CurrentSourceConfiguration) && (0 != CurrentSourceConfiguration.TextStream));
            }
        }

        /// <summary>
        /// Nimmt eine EPG Tabelle entgegen.
        /// </summary>
        /// <param name="table"></param>
        private void ReceiveEPG( EIT table )
        {
            // Check for EPG entry
            if (null != CurrentSelection)
                ProcessEPG( table.Table.Section, CurrentSelection.Source );
        }

        /// <summary>
        /// Meldet den Namen des aktuellen Senders.
        /// </summary>
        protected override string StationName
        {
            get
            {
                // Report
                return CurrentSelection.DisplayName;
            }
        }

        /// <summary>
        /// Meldet den beim letzten Beenden der Anwendung aktiven Sender.
        /// </summary>
        protected override string DefaultStation
        {
            get
            {
                // Report
                return LocalInfo.LocalStation;
            }
        }

        /// <summary>
        /// Meldet die beim letzten Beender der Anwendung aktive Tonspur.
        /// </summary>
        protected override string DefaultAudio
        {
            get
            {
                // Report
                return LocalInfo.LocalAudio;
            }
        }

        /// <summary>
        /// Ermittelt alle NVOD Dienste zum aktuellen Portal.
        /// </summary>
        public override ServiceItem[] Services
        {
            get
            {
                // Load
                Dictionary<Station, string> map = (null == ServiceParser) ? null : ServiceParser.ServiceMap;

                // Create list
                List<ServiceItem> services = new List<ServiceItem>();

                // Fill
                if (null != map)
                    foreach (KeyValuePair<Station, string> current in map)
                        services.Add( new ServiceItem( current.Key, current.Value ) );

                // Sort
                services.Sort();

                // Report
                return services.ToArray();
            }
        }

        /// <summary>
        /// Wählt einen NVOD Dienst des aktuellen Portals zur Anzeige aus.
        /// <see cref="SetStation(SourceSelection, SourceSelection, ServiceItem)"/>
        /// </summary>
        /// <param name="service">Der gewünschte NVOD Dienst.</param>
        /// <returns>Der Name des NVOD Dienstes und die aktive Tonspur, wenn
        /// ein Wechsel des Senders möglich war.</returns>
        public override string SetService( ServiceItem service )
        {
            // Find the related station
            SourceSelection[] stations = Profile.FindSource( service.Identifier );

            // None found
            if (stations.Length < 1)
                return null;

            // Change
            if ((null != CurrentPortal) && CurrentPortal.Source.Equals( stations[0].Source ))
                return SetStation( stations[0], null, null );
            else
                return SetStation( stations[0], (null == CurrentPortal) ? CurrentSelection : CurrentPortal, service );
        }

        /// <summary>
        /// Trägt zusätzliche Konfigurationseinträge in die Liste der Anwendung ein.
        /// </summary>
        public override void FillOptions()
        {
            // Add option to choose directory
            Parent.AddOption( new OptionDisplay( Properties.Resources.SetDirectory, SetDirectory ) );
        }

        /// <summary>
        /// Wählt das Verzeichnis für Aufzeichnungsdateien aus.
        /// </summary>
        /// <remarks>
        /// Eine Auswahl erfolgt im Allgemeinen nur bei der ersten Aufzeichnung oder
        /// bei einem expliten Aufruf über den Konfigurationseintrag. Da hier ein 
        /// normaler Windows Dialog verwendet wird, wird der Vollbildmodus der Anwendung
        /// in jedem Fall vor dessen Anzeige beendet.
        /// <see cref="FillOptions"/>
        /// </remarks>
        private void SetDirectory()
        {
            // Ask user          
            using (GeneralInfo.ShowCursor())
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                // Configure
                dlg.Description = Properties.Resources.DirectoryDialogText;
                dlg.SelectedPath = LocalInfo.RecordingDirectory;
                dlg.ShowNewFolderButton = true;

                // Select
                if (DialogResult.OK == dlg.ShowDialog( Parent ))
                    LocalInfo.RecordingDirectory = dlg.SelectedPath;
            }
        }

        /// <summary>
        /// Meldet, ob gerade eine Aufzeichnung aktiv ist.
        /// </summary>
        public override bool IsRecording { get { return (RecordingStream != null); } }

        /// <summary>
        /// Meldet die bereits aufgezeichneten Bytes.
        /// </summary>
        public override long RecordedBytes { get { return (RecordingStream != null) ? RecordingStream.BytesReceived : 0; } }

        /// <summary>
        /// Prüft, ob ein Regionalsender sein Daten verändert hat.
        /// </summary>
        /// <param name="fine">Gesetzt für den Aufruf im Sekundenrythmus.</param>
        public override void KeepAlive( bool fine )
        {
            // Forward to base
            base.KeepAlive( fine );

            // No source
            if (CurrentSelection == null)
                return;

            // Start reader
            if (m_InfoReader == null)
            {
                // Make sure that service configuration is reloaded
                if (!m_HasPendingGroupInformation)
                {
                    // Once
                    m_HasPendingGroupInformation = true;

                    // Request new
                    Device.ResetInformationReaders();
                }

                // Check for match
                if (m_HasPendingGroupInformation)
                    if (null == Device.GetGroupInformation( 0 ))
                        return;
                    else
                        m_HasPendingGroupInformation = false;

                // Read in background
                m_InfoReader = CurrentSelection.GetSourceInformationAsync();
            }

            // Only coarse
            if (fine)
                return;

            // See if we are done
            if (!m_InfoReader.IsCompleted)
                return;

            // Read the information
            var info = m_InfoReader.Result;

            // Stop reader
            using (m_InfoReader)
                m_InfoReader = null;

            // Update recording
            if (RecordingStream != null)
            {
                // Forward
                RecordingStream.RetestSourceInformation( info );

                // Restart silently if necessary
                RestartStreams();
            }

            // See if something changed
            bool changed = CompareSourceInformation( info );

            // Use the new one
            CurrentSourceConfiguration = info;

            // Update
            if (!changed)
                return;

            // Refresh audio list
            ResetAudio();

            // Process
            Activate( null );

            // Report
            ShowMessage( Properties.Resources.PSIUpdate, Properties.Resources.PSIUpdateTitle, true );
        }

        /// <summary>
        /// Meldet, ob Bild, Ton und Videotext neu aufgebaut werden müssen.
        /// </summary>
        /// <param name="info">Vergleichswert.</param>
        /// <returns>Gesetzt, wenn eine relevante Veränderung erkannt wurde.</returns>
        private bool CompareSourceInformation( SourceInformation info )
        {
            // Always changed
            if (null == CurrentSourceConfiguration)
                return true;

            // Picture
            if (CurrentSourceConfiguration.VideoStream != info.VideoStream)
                return true;
            if (0 != CurrentSourceConfiguration.VideoStream)
                if (CurrentSourceConfiguration.VideoType != info.VideoType)
                    return true;

            // Videotext
            if (CurrentSourceConfiguration.TextStream != info.TextStream)
                return true;

            // Audio count
            if (CurrentSourceConfiguration.AudioTracks.Count != info.AudioTracks.Count)
                return true;

            // Compare all
            for (int i = CurrentSourceConfiguration.AudioTracks.Count; i-- > 0; )
            {
                // Load
                AudioInformation oldAudio = CurrentSourceConfiguration.AudioTracks[i], newAudio = info.AudioTracks[i];

                // Compare
                if (oldAudio.AudioStream != newAudio.AudioStream)
                    return true;
                if (oldAudio.AudioType != newAudio.AudioType)
                    return true;
                if (0 != string.Compare( oldAudio.Language, newAudio.Language, true ))
                    return true;
            }

            // No change
            return false;
        }

        /// <summary>
        /// Ermittelt eine möglicherweise eingeschränkte Datenstromauswahl.
        /// </summary>
        /// <param name="manager">Die Quelle, um die es geht.</param>
        /// <returns>Eine geeignete Auswahl.</returns>
        private StreamSelection GetOptimizedStreams( SourceStreamsManager manager )
        {
            // Not possible
            if (null == CurrentSelection)
                return null;

            // What we want to record
            StreamSelection selection = new StreamSelection();

            // Put it on
            selection.AC3Tracks.LanguageMode = LanguageModes.All;
            selection.MP2Tracks.LanguageMode = LanguageModes.All;
            selection.SubTitles.LanguageMode = LanguageModes.All;
            selection.ProgramGuide = true;
            selection.Videotext = true;

            // See if we are working on a limited device
            if (!Device.HasConsumerRestriction)
                return selection;

            // Stop picture for a moment
            Device.SetConsumerState( VideoId, false );
            Device.SetConsumerState( AudioId, false );
            Device.SetConsumerState( TextId, false );

            // Require restart
            m_NeedRestart = true;

            // Create a brand new optimizer
            StreamSelectionOptimizer localOpt = new StreamSelectionOptimizer();

            // Add the one stream
            localOpt.Add( CurrentSelection, selection );

            // Run the optimization
            localOpt.Optimize();

            // Report result
            return localOpt.GetStreams( 0 );
        }

        /// <summary>
        /// Beginnt oder beendet eine Aufzeichnung.
        /// <seealso cref="SetDirectory"/>
        /// </summary>
        public override void StartRecording()
        {
            // See if recording is running
            if (IsRecording)
            {
                // Stop it
                using (SourceStreamsManager manager = RecordingStream)
                    RecordingStream = null;

                // Force restart
                m_NeedRestart = true;

                // Restart silently
                RestartStreams();

                // Show up
                ShowMessage( Properties.Resources.RecordingFinished, Properties.Resources.RecordingTitle, true );

                // Done
                return;
            }

            // No channel
            if (null == CurrentSelection)
                return;

            // Must have directory
            if (string.IsNullOrEmpty( LocalInfo.RecordingDirectory ))
            {
                // Ask user
                SetDirectory();

                // No directory at all
                if (string.IsNullOrEmpty( LocalInfo.RecordingDirectory ))
                    return;
            }

            // Create the name
            string filename = string.Format( "{0:yyyy-MM-dd HH-mm-ss} {1}.ts", DateTime.Now, StationName );

            // Cleanup
            foreach (char bad in Path.GetInvalidFileNameChars())
                filename = filename.Replace( bad, '_' );

            // Extras
            filename = filename.Replace( '&', '_' );

            // Create the target
            RecordingStream = CurrentSelection.Open( GetOptimizedStreams( null ) );

            // Use for reconfigure
            if (Device.HasConsumerRestriction)
                RecordingStream.BeforeRecreateStream += GetOptimizedStreams;

            // Start it
            RecordingStream.CreateStream( Path.Combine( LocalInfo.RecordingDirectory, filename ) );

            // Restart silently
            RestartStreams();

            // Enable multicast broadcast
            if (!string.IsNullOrEmpty( StreamInfo.BroadcastIP ) && (StreamInfo.BroadcastPort > 0))
                RecordingStream.StreamingTarget = string.Format( "*{0}:{1}", StreamInfo.BroadcastIP, StreamInfo.BroadcastPort );

            // Show up
            ShowMessage( Properties.Resources.RecordingStarted, Properties.Resources.RecordingTitle, true );
        }

        /// <summary>
        /// Reaktiviert soviele Datenströme wie möglich.
        /// </summary>
        private void RestartStreams()
        {
            // Not necessary
            if (!m_NeedRestart)
                return;

            // Run once
            m_NeedRestart = false;

            // Never stopped
            if (!Device.HasConsumerRestriction)
                return;

            // As much as possible
            try
            {
                // On again
                Device.SetConsumerState( VideoId, true );
                Device.SetConsumerState( AudioId, true );
                Device.SetConsumerState( TextId, true );
            }
            catch
            {
            }
        }
    }
}
