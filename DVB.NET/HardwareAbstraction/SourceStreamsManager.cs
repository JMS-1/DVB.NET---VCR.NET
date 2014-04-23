using System;
using System.Collections.Generic;
using System.IO;
using JMS.DVB.SI;
using JMS.DVB.TS;


namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt eine Datei, die vom <see cref="SourceStreamsManager"/> erzeugt
    /// wurde.
    /// </summary>
    [Serializable]
    public class FileStreamInformation
    {
        /// <summary>
        /// Der volle Pfad der erzeugten Datei.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Das in der Datei verwendete Bildformat.
        /// </summary>
        public VideoTypes VideoType { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public FileStreamInformation()
        {
        }

        /// <summary>
        /// Meldet einen Anzeigetest zu Testzwecken.
        /// </summary>
        /// <returns>Der Anzeigetext.</returns>
        public override string ToString()
        {
            // Construct
            return string.Format( "{0} ({1})", FilePath, VideoType );
        }
    }

    /// <summary>
    /// Verwaltet den Zugriff auf eine aktive Quelle.
    /// </summary>
    public class SourceStreamsManager : IDisposable
    {
        /// <summary>
        /// Wird ausgelöst, bevor eine Quelle nach einer Konfigurationsänderung erneut aktiviert wird.
        /// </summary>
        public event Func<SourceStreamsManager, StreamSelection> BeforeRecreateStream;

        /// <summary>
        /// Das zu verwendende DVB.NET Gerät.
        /// </summary>
        public Hardware Hardware { get; private set; }

        /// <summary>
        /// Die Quelle, deren Datenströme betrachtet werden.
        /// </summary>
        public SourceIdentifier Source { get; private set; }

        /// <summary>
        /// Die Auswahl der Datenströme.
        /// </summary>
        public StreamSelection StreamSelection { get; private set; }

        /// <summary>
        /// Die aktuelle Aufzeichnung in einen <i>Transport Stream</i>.
        /// </summary>
        private Manager m_TransportStream;

        /// <summary>
        /// Meldet, wann die aktuelle Altivierung gestartet wurde.
        /// </summary>
        public DateTime? LastActivationTime { get; set; }

        /// <summary>
        /// Eine eindeutige textuelle Beschreibung der aktuellen Aufzeichnung bezüglich
        /// der verwendeten Datenströme.
        /// </summary>
        private string m_CurrentSignature;

        /// <summary>
        /// Die technischen Informationen zur Quelle zum Zeitpunkt des Starts
        /// einer Aufzeichnung.
        /// </summary>
        private SourceInformation m_OriginalSettings;

        /// <summary>
        /// Der Dateiname, der bei der Erzeugung der Aufzeichnungsdatei verwendet wurde.
        /// </summary>
        private string m_OriginalPath;

        /// <summary>
        /// Verwaltet alle Dateien, die erzeugt wurden.
        /// </summary>
        private List<FileStreamInformation> m_AllFiles = new List<FileStreamInformation>();

        /// <summary>
        /// Die laufende Nummer der aktuellen Datei.
        /// </summary>
        private int m_FileCount = 0;

        /// <summary>
        /// Alle Datenströme, die aktiviert wurden.
        /// </summary>
        private List<Guid> m_Consumers = new List<Guid>();

        /// <summary>
        /// Optional ein zugehöriges Geräteprofil zur Referenz der Senderliste.
        /// </summary>
        public Profile Profile { get; private set; }

        /// <summary>
        /// Die als nächstes zu vergebende Nummer der internen Datenstromkennungen (PID) für den
        /// <i>Transport Stream</i>.
        /// </summary>
        public short NextStreamIdentifier { get; set; }

        /// <summary>
        /// Ein Offset für die Anzahl der bisher empfangenen Datenbytes <see cref="BytesReceived"/>.
        /// </summary>
        public long BytesBias { get; set; }

        /// <summary>
        /// Die aktuell verwendeten Teildatenströme beim Empfang.
        /// </summary>
        public StreamSelection ActiveSelection { get; private set; }

        /// <summary>
        /// Die zuletzt gewählte TCP/IP Zieladresse.
        /// </summary>
        private string m_LastStreamTarget;

        /// <summary>
        /// Gesetzt, wenn die Entschlüsselung aktiviert wurde.
        /// </summary>
        private bool m_Decrypting;

        /// <summary>
        /// Wird aktiviert, sobald die Systemuhr in eine Datei geschrieben wird.
        /// </summary>
        private Action<string, long, byte[]> m_WritePCRSink;

        /// <summary>
        /// Wird aufgerufen, wenn ein neuer <i>Transport Stream</i> angelegt wurde.
        /// </summary>
        public event Action<SourceStreamsManager> OnCreatedStream;

        /// <summary>
        /// Eine optionale Methode, die zu einer Art von Nutzdaten die Größe des zu verwendenden
        /// Zwischenspeichers für Dateien ermittelt.
        /// </summary>
        public Func<EPG.StreamTypes?, int?> FileBufferSizeChooser;

        /// <summary>
        /// Erzeugt eine neue Verwaltung.
        /// </summary>
        /// <param name="hardware">Das Gerät, auf dem die zugehörige Quellgruppe gerade aktiv ist.</param>
        /// <param name="profile">Optional das Geräteprofil mit der zugehörigen Senderliste.</param>
        /// <param name="source">Die Quelle, die zu betrachten ist.</param>
        /// <param name="selection">Die zu betrachtenden Datenströme.</param>
        /// <exception cref="ArgumentNullException">Ein Parameter wurde nicht angegeben.</exception>
        public SourceStreamsManager( Hardware hardware, Profile profile, SourceIdentifier source, StreamSelection selection )
        {
            // Validate
            if (null == hardware)
                throw new ArgumentNullException( "hardware" );
            if (null == source)
                throw new ArgumentNullException( "source" );
            if (null == selection)
                throw new ArgumentNullException( "selection" );

            // Remember all
            StreamSelection = selection;
            Hardware = hardware;
            Profile = profile;
            Source = source;
        }

        /// <summary>
        /// Startet das Auslesen der aktuellen Daten zur aktiven Quelle im Hintergrund.
        /// </summary>
        /// <returns>Eine Steuerinstanz zum asynchronen Zugriff auf die aktuellen Daten.</returns>
        public CancellableTask<SourceInformation> GetCurrentInformationAsync()
        {
            // Just forward
            return Hardware.GetSourceInformationAsync( Source, Profile );
        }

        /// <summary>
        /// Ermittelt die aktuellen Daten zur aktiven Quelle.
        /// </summary>
        /// <returns>Die neu ermittelten aktuellen Daten.</returns>
        public SourceInformation GetCurrentInformation()
        {
            // Just forward
            return GetCurrentInformationAsync().CancelAfter( 5000 ).Result;
        }

        /// <summary>
        /// Meldet die aktuell verwendeten Daten der Quelle.
        /// </summary>
        public SourceInformation ActiveInformation { get { return m_OriginalSettings; } }

        /// <summary>
        /// Meldet oder legt fest, ob Berichte über das Schreiben der Systemuhr
        /// in eine Datei erzeugt werden sollen.
        /// </summary>
        public Action<string, long, byte[]> OnWritingPCR
        {
            get { return m_WritePCRSink; }
            set
            {
                // Forward
                if (m_TransportStream != null)
                    m_TransportStream.OnWritingPCR = value;

                // Update cached
                m_WritePCRSink = value;
            }
        }

        /// <summary>
        /// Beginnt die Aufzeichnung in einen <i>Transport Stream</i> - optional als 
        /// Datei.
        /// </summary>
        /// <param name="filePath">Optional die Angabe eines Dateinames.</param>
        /// <returns>Gesetzt, wenn die Quelle bekannt ist und der Empfang aktiviert wurde.</returns>
        /// <exception cref="InvalidOperationException">Es ist bereits eine Aufzeichnung aktiv.</exception>
        public bool CreateStream( string filePath )
        {
            // Forward
            return CreateStream( filePath, null );
        }

        /// <summary>
        /// Beginnt die Aufzeichnung in einen <i>Transport Stream</i> - optional als 
        /// Datei.
        /// </summary>
        /// <param name="filePath">Optional die Angabe eines Dateinames.</param>
        /// <param name="info">Die aktuelle Konfiguration der Quelle oder <i>null</i>, wenn keine bekannt ist.</param>
        /// <returns>Gesetzt, wenn die Quelle bekannt ist und der Empfang aktiviert wurde.</returns>
        /// <exception cref="InvalidOperationException">Es ist bereits eine Aufzeichnung aktiv.</exception>
        public bool CreateStream( string filePath, SourceInformation info )
        {
            // Validate
            if (m_TransportStream != null)
                throw new InvalidOperationException();

            // Load
            if (info == null)
                info = GetCurrentInformation();

            // Remember settings
            m_OriginalSettings = info;
            m_OriginalPath = filePath;
            m_FileCount = 0;

            // Validate
            if (m_OriginalSettings == null)
                return false;

            // Forward
            CreateStream( NextStreamIdentifier, false );

            // Update signature
            m_CurrentSignature = CreateRecordingSignature( m_OriginalSettings );

            // Activate streaming
            if (!string.IsNullOrEmpty( m_LastStreamTarget ))
                StreamingTarget = m_LastStreamTarget;

            // Did it
            return true;
        }

        /// <summary>
        /// Meldet, ob eine Entschlüsselung bei <see cref="CreateStream(string, SourceInformation)"/> aktiviert wurde. Das
        /// Ergebnis ist <i>null</i>, wenn der Empfang der zugehörigen Quelle nicht aktiv ist.
        /// </summary>
        public bool? IsDecrypting
        {
            get
            {
                // Check mode
                if (m_TransportStream == null)
                    return null;
                else
                    return m_Decrypting;
            }
        }

        /// <summary>
        /// Liest oder setzt das Ziel für den Netzwerkversand. Die Angabe erfolgt
        /// in der Notation <i>Rechner:Port</i>.
        /// </summary>
        public string StreamingTarget
        {
            get
            {
                // None
                if (null == m_TransportStream)
                    return m_LastStreamTarget;

                // Check mode
                if (0 == m_TransportStream.TCPPort)
                    return null;

                // Combine
                return string.Format( "{0}:{1}", m_TransportStream.TCPClient, m_TransportStream.TCPPort );
            }
            set
            {
                // Check mode
                if (string.IsNullOrEmpty( value ))
                {
                    // Clear
                    if (null != m_TransportStream)
                        m_TransportStream.SetStreamTarget( string.Empty, 0 );
                }
                else
                {
                    // Split
                    var parts = value.Split( ':' );
                    if (2 != parts.Length)
                        throw new ArgumentException( value, "value" );

                    // Get port
                    ushort port;
                    if (!ushort.TryParse( parts[1], out port ))
                        throw new ArgumentException( value, "value" );

                    // Update
                    if (null != m_TransportStream)
                        m_TransportStream.SetStreamTarget( parts[0], port );
                }

                // Remember
                m_LastStreamTarget = value;
            }
        }

        /// <summary>
        /// Meldet die Anzahl der Bild- und Tondaten, die im aktuellen Datenstrom empfangen
        /// wurden.
        /// </summary>
        public long? CurrentAudioVideoBytes
        {
            get
            {
                // Report
                if (null == m_TransportStream)
                    return null;
                else
                    return m_TransportStream.AVLength;
            }
        }

        /// <summary>
        /// Meldet die gesamte Anzahl der bisher empfangenen Datenbytes.
        /// </summary>
        public long BytesReceived
        {
            get
            {
                // Report
                if (null == m_TransportStream)
                    return BytesBias;
                else
                    return BytesBias + m_TransportStream.Length;
            }
        }

        /// <summary>
        /// Meldet die Anzahl der Datenströme (PID), die für den Empfang benötigt werden.
        /// </summary>
        public int ConsumerCount
        {
            get
            {
                // Report
                if (null == m_TransportStream)
                    return 0;
                else
                    return m_TransportStream.StreamCount;
            }
        }

        /// <summary>
        /// Ermittelt anhand der aktuellen Konfiguration, welche Datenströme wie aufgezeichnet
        /// werden. Die gemeldete Zeichenkette hat keine weitere Bedeutung, nur dass sie zum
        /// schnellen Vergleich verwemdet werden kann.
        /// </summary>
        /// <param name="information">Die aktuellen Daten zur zugehörigen Quelle.</param>
        /// <returns>Ein Schlüssel passend zur aktuellen Aufzeichnung.</returns>
        private string CreateRecordingSignature( SourceInformation information )
        {
            // All keys - will be sorted to make sure that lists are ordered
            var flags = new List<int>();

            // Various offset - PIDs are 12 Bit only!
            const int Offset_VideoType = 0x10000;
            const int Offset_VideoPID = 0x20000;
            const int Offset_MP2PID = 0x30000;
            const int Offset_AC3PID = 0x40000;
            const int Offset_TTXPID = 0x50000;
            const int Offset_SUBPID = 0x60000;

            // Video first
            if (0 != information.VideoStream)
            {
                // Get the type                
                var videoType = (information.VideoType == VideoTypes.H264) ? EPG.StreamTypes.H264 : EPG.StreamTypes.Video13818;

                // Register
                flags.Add( Offset_VideoType + (int) videoType );
                flags.Add( Offset_VideoPID + (int) information.VideoStream );
            }

            // All MP2 audio
            foreach (var audio in information.AudioTracks)
                if (AudioTypes.MP2 == audio.AudioType)
                    if (StreamSelection.MP2Tracks.LanguageMode == LanguageModes.Primary)
                    {
                        // Add as is
                        flags.Add( Offset_MP2PID + (int) audio.AudioStream );

                        // Finish
                        break;
                    }
                    else if (StreamSelection.MP2Tracks.Contains( audio.Language ))
                    {
                        // Add as is
                        flags.Add( Offset_MP2PID + (int) audio.AudioStream );
                    }

            // All AC3 audio
            foreach (var audio in information.AudioTracks)
                if (AudioTypes.AC3 == audio.AudioType)
                    if (StreamSelection.AC3Tracks.LanguageMode == LanguageModes.Primary)
                    {
                        // Add as is
                        flags.Add( Offset_AC3PID + (int) audio.AudioStream );

                        // Finish
                        break;
                    }
                    else if (StreamSelection.AC3Tracks.Contains( audio.Language ))
                    {
                        // Add as is
                        flags.Add( Offset_AC3PID + (int) audio.AudioStream );
                    }

            // Videotext
            if (StreamSelection.Videotext)
                if (0 != information.TextStream)
                    flags.Add( Offset_TTXPID + (int) information.TextStream );

            // Subtitle streams
            var subtitles = new Dictionary<ushort, bool>();

            // Subtitles
            foreach (var subtitle in information.Subtitles)
                if (StreamSelection.SubTitles.LanguageMode == LanguageModes.Primary)
                {
                    // Add as is
                    subtitles[subtitle.SubtitleStream] = true;

                    // Finish
                    break;
                }
                else if (StreamSelection.SubTitles.Contains( subtitle.Language ))
                {
                    // Add as is
                    subtitles[subtitle.SubtitleStream] = true;
                }

            // Process all subtitles
            foreach (var subtitleStream in subtitles.Keys)
                flags.Add( Offset_SUBPID + (int) subtitleStream );

            // Create the key
            return (information.IsEncrypted ? "+" : "-") + string.Join( ".", flags.ConvertAll( f => f.ToString( "x5" ) ).ToArray() );
        }

        /// <summary>
        /// Meldet, ob der Datenempfang aktiviert wurde.
        /// </summary>
        public bool IsActive
        {
            get
            {
                // Check stream
                return (null != m_TransportStream);
            }
        }

        /// <summary>
        /// Prüft, ob sich an der Konfiguration der Quelle etwas verändert hat.
        /// </summary>
        /// <returns>Gesetzt, wenn nun der Empfang aktiv ist.</returns>
        public bool RetestSourceInformation()
        {
            // Forward
            return RetestSourceInformation( GetCurrentInformation() );
        }

        /// <summary>
        /// Prüft, ob sich an der Konfiguration der Quelle etwas verändert hat.
        /// </summary>
        /// <param name="information">Die zugehörige aktuelle Konfiguration der Quelle.</param>
        /// <returns>Gesetzt, wenn nun der Empfang aktiv ist.</returns>
        /// <exception cref="ArgumentNullException">Der angegebenen Konfiguration ist keine Quelle zugeordnet.</exception>
        /// <exception cref="ArgumentException">Die Konfiguration gehört zu einer anderen Quelle als der aktuellen.</exception>
        public bool RetestSourceInformation( SourceInformation information )
        {
            // Must get the current source information
            if (information == null)
            {
                // Close any active stream
                CloseStream();

                // Not active
                return false;
            }

            // Validate
            if (information.Source == null)
                throw new ArgumentNullException( "information.Source" );
            if (!information.Source.Equals( Source ))
                throw new ArgumentException( information.Source.ToString(), "information.Source" );

            // Nothing changed
            var signature = CreateRecordingSignature( information );
            if (m_TransportStream != null)
                if (signature.Equals( m_CurrentSignature ))
                    return true;

            // Get the streaming data
            var streamTarget = StreamingTarget;

            // Stop the current recording
            CloseStream();

            // Update all
            m_OriginalSettings = information;
            m_CurrentSignature = signature;

            // Fire notifications
            if (BeforeRecreateStream != null)
            {
                // Request new stream configuration
                var newSelection = BeforeRecreateStream( this );
                if (newSelection == null)
                    return false;

                // Remember
                StreamSelection = newSelection.Clone();
            }

            // Forward
            CreateStream( NextStreamIdentifier, true );

            // Reactivate streaming
            if (streamTarget != null)
                StreamingTarget = streamTarget;

            // Did it
            return true;
        }

        /// <summary>
        /// Beginnt die Aufzeichnung in einen <i>Transport Stream</i> - optional als 
        /// Datei.
        /// </summary>
        /// <param name="nextPID">Die erste Datenstromkennung (PID), die in der Aufzeichnungsdatei verwendet werden darf.</param>
        /// <param name="recreate">Gesetzt, wenn ein Neustart aufgrund veränderter Nutzdatenströme erforderlich wurde.</param>
        /// <exception cref="ArgumentException">Eine Aufzeichnung der angegebenen Quelle ist nicht möglich.</exception>
        private void CreateStream( short nextPID, bool recreate )
        {
            // Try to get the full name
            var filePath = m_OriginalPath;
            if (filePath != null)
                if (m_FileCount > 0)
                {
                    // Split off the parts
                    var name = Path.GetFileNameWithoutExtension( filePath );
                    var dir = Path.GetDirectoryName( filePath );
                    var ext = Path.GetExtension( filePath );

                    // Construct new name
                    filePath = Path.Combine( dir, string.Format( "{0} - {1}{2}", name, m_FileCount, ext ) );
                }

            // Try to decrypt
            if (m_Decrypting = m_OriginalSettings.IsEncrypted)
                try
                {
                    // Process
                    Hardware.Decrypt( Source );
                }
                catch
                {
                    // Ignore any error
                    m_Decrypting = false;
                }

            // Type of the video
            EPG.StreamTypes? videoType;

            // Video first
            if (m_OriginalSettings.VideoStream == 0)
                videoType = null;
            else
                videoType = (m_OriginalSettings.VideoType == VideoTypes.H264) ? EPG.StreamTypes.H264 : EPG.StreamTypes.Video13818;

            // Get the buffer size
            var bufferSize = (FileBufferSizeChooser == null) ? null : FileBufferSizeChooser( videoType );

            // Create the new stream
            m_TransportStream = new Manager( filePath, nextPID, bufferSize.GetValueOrDefault( Manager.DefaultBufferSize ) );

            // Attach PCR sink
            m_TransportStream.OnWritingPCR = m_WritePCRSink;

            // Report of the actually selected streams
            StreamSelection result = new StreamSelection();

            // Cleanup on any error
            try
            {
                // Video first
                if (videoType.HasValue)
                    AddConsumer( m_OriginalSettings.VideoStream, StreamTypes.Video, m_TransportStream.AddVideo( (byte) videoType.Value ) );

                // Select audio
                ProcessAudioSelection( AudioTypes.MP2, result.MP2Tracks, StreamSelection.MP2Tracks );
                ProcessAudioSelection( AudioTypes.AC3, result.AC3Tracks, StreamSelection.AC3Tracks );

                // Videotext
                if (StreamSelection.Videotext)
                    if (0 != m_OriginalSettings.TextStream)
                    {
                        // Register
                        AddConsumer( m_OriginalSettings.TextStream, StreamTypes.VideoText, m_TransportStream.AddTeleText() );

                        // Remember
                        result.Videotext = true;
                    }

                // Subtitle streams
                var subtitles = new Dictionary<ushort, List<EPG.SubtitleInfo>>();

                // Preset mode
                result.SubTitles.LanguageMode = LanguageModes.All;

                // All audio
                // Subtitles
                foreach (var subtitle in m_OriginalSettings.Subtitles)
                {
                    // Check for primary
                    if (StreamSelection.SubTitles.LanguageMode == LanguageModes.Primary)
                    {
                        // Attach to the list
                        AddSubtitleInformation( subtitle, subtitles );

                        // Copy over
                        result.SubTitles.LanguageMode = LanguageModes.Primary;

                        // Remember
                        result.SubTitles.Languages.Add( subtitle.Language );

                        // Done
                        break;
                    }

                    // Standard selection
                    if (StreamSelection.SubTitles.Contains( subtitle.Language ))
                    {
                        // Attach to the list
                        AddSubtitleInformation( subtitle, subtitles );

                        // Remember
                        result.SubTitles.Languages.Add( subtitle.Language );
                    }
                    else
                    {
                        // At least one is excluded
                        result.SubTitles.LanguageMode = LanguageModes.Selection;
                    }
                }

                // Clear flag if no audio is used
                if (LanguageModes.All == result.SubTitles.LanguageMode)
                    if (result.SubTitles.Languages.Count < 1)
                        result.SubTitles.LanguageMode = LanguageModes.Selection;

                // Process all subtitles
                foreach (var current in subtitles)
                    AddConsumer( current.Key, StreamTypes.SubTitle, m_TransportStream.AddSubtitles( current.Value.ToArray() ) );

                // See if program guide is requested
                bool epg = StreamSelection.ProgramGuide;

                // May want to disable
                if (epg)
                    if ((Hardware.Profile != null) && Hardware.Profile.DisableProgramGuide)
                        epg = false;
                    else if (Profile != null)
                        if (Profile.GetFilter( Source ).DisableProgramGuide)
                            epg = false;

                // EPG
                if (epg)
                {
                    // Activate dispatch
                    m_TransportStream.SetEPGMapping( Source.Network, Source.TransportStream, Source.Service );

                    // Start it
                    Hardware.AddProgramGuideConsumer( DispatchEPG );

                    // Remember
                    result.ProgramGuide = true;
                }

                // Counter
                int started = 0;
                try
                {
                    // Start all
                    foreach (var consumer in m_Consumers)
                        try
                        {
                            // Forward
                            Hardware.SetConsumerState( consumer, true );

                            // Count it
                            ++started;
                        }
                        catch (OutOfConsumersException)
                        {
                            // Translate
                            throw new OutOfConsumersException( m_Consumers.Count, started ) { RequestedSelection = result };
                        }
                }
                catch
                {
                    // Detach EPG
                    Hardware.RemoveProgramGuideConsumer( DispatchEPG );

                    // Cleanup on all errors
                    foreach (var consumer in m_Consumers)
                        try
                        {
                            // Remove
                            Hardware.SetConsumerState( consumer, null );
                        }
                        catch
                        {
                            // Ignore any error
                        }

                    // Forward
                    throw;
                }

                // Remember path
                if (filePath != null)
                    m_AllFiles.Add( new FileStreamInformation { FilePath = filePath, VideoType = m_OriginalSettings.VideoType } );

                // Remember the time
                LastActivationTime = DateTime.UtcNow;
            }
            catch
            {
                // Simply forget
                m_TransportStream.Dispose();
                m_TransportStream = null;

                // Forward
                throw;
            }

            // Get the next free PID
            NextStreamIdentifier = m_TransportStream.NextPID;

            // Remember the streams we use
            ActiveSelection = result;

            // Attach to clients
            var createNotify = OnCreatedStream;

            // Report
            if (createNotify != null)
                createNotify( this );
        }

        /// <summary>
        /// Trennt die Aufzeichungsdatei nahtlos.
        /// </summary>
        /// <param name="newPath">Die neue Aufzeichnungsdatei.</param>
        /// <returns>Gesetzt, wenn die Trennung erfolgreich angenommen wurde.</returns>
        public bool SplitFile( string newPath )
        {
            // No stream active
            if (m_TransportStream == null)
                return false;

            // Splitting files is not supported
            if (!m_TransportStream.CanSplitFile)
                return false;

            // No files so far - strange
            if (m_AllFiles.Count < 1)
                return false;

            // Load previous
            var last = m_AllFiles[m_AllFiles.Count - 1];

            // Safe split
            try
            {
                // Do the split
                m_TransportStream.SplitFile( newPath );

                // Create new entry
                m_AllFiles.Add( new FileStreamInformation { FilePath = newPath, VideoType = last.VideoType } );

                // Attach to clients
                var createNotify = OnCreatedStream;

                // Report
                if (createNotify != null)
                    createNotify( this );

                // Did it 
                return true;
            }
            catch
            {
                // Ignore any error - continue as before
                return false;
            }
        }

        /// <summary>
        /// Ergänzt die Informationen zu einer DVB Untertitelspur.
        /// </summary>
        /// <param name="subtitle">Die Untertitelspur.</param>
        /// <param name="subtitles">Alle bisher aufgenommenen Spuren.</param>
        private void AddSubtitleInformation( SubtitleInformation subtitle, Dictionary<ushort, List<EPG.SubtitleInfo>> subtitles )
        {
            // Attach to the list
            List<EPG.SubtitleInfo> list;
            if (!subtitles.TryGetValue( subtitle.SubtitleStream, out list ))
            {
                // Create new
                list = new List<EPG.SubtitleInfo>();

                // Remember it
                subtitles[subtitle.SubtitleStream] = list;
            }

            // Register
            list.Add(
                new EPG.SubtitleInfo
                    (
                        subtitle.Language.ToISOLanguage(),
                        (EPG.SubtitleTypes) subtitle.SubtitleType,
                        subtitle.CompositionPage,
                        subtitle.AncillaryPage
                    ) );
        }

        /// <summary>
        /// Ermittelt für eine Art von Tonspur alle aktiven Datenströme.
        /// </summary>
        /// <param name="type">Die gewünschte Art der Tonspur.</param>
        /// <param name="selected">Die aktive Auswahl der Tonspuren.</param>
        /// <param name="requested">Die gewünschten Sprachen.</param>
        private void ProcessAudioSelection( AudioTypes type, LanguageSelection selected, LanguageSelection requested )
        {
            // Get method for adding streams
            Func<string, StreamBase> addConsumer;
            if (type == AudioTypes.MP2)
                addConsumer = m_TransportStream.AddAudio;
            else
                addConsumer = m_TransportStream.AddDolby;

            // Preset mode
            selected.LanguageMode = LanguageModes.All;

            // All audio
            foreach (var audio in m_OriginalSettings.AudioTracks)
                if (type == audio.AudioType)
                {
                    // Check for primary
                    if (requested.LanguageMode == LanguageModes.Primary)
                    {
                        // Register
                        AddConsumer( audio.AudioStream, StreamTypes.Audio, addConsumer( audio.Language.ToISOLanguage() ) );

                        // Copy over
                        selected.LanguageMode = LanguageModes.Primary;

                        // Remember
                        selected.Languages.Add( audio.Language );

                        // Done
                        return;
                    }

                    // Regular test
                    if (requested.Contains( audio.Language ))
                    {
                        // Register
                        AddConsumer( audio.AudioStream, StreamTypes.Audio, addConsumer( audio.Language.ToISOLanguage() ) );

                        // Remember
                        selected.Languages.Add( audio.Language );
                    }
                    else
                    {
                        // Not all
                        selected.LanguageMode = LanguageModes.Selection;
                    }
                }

            // Clear flag if no audio is used
            if (LanguageModes.All == selected.LanguageMode)
                if (selected.Languages.Count < 1)
                    selected.LanguageMode = LanguageModes.Selection;
        }

        /// <summary>
        /// Meldet alle Dateien, die geöffnet wurden.
        /// </summary>
        public FileStreamInformation[] AllFiles
        {
            get
            {
                // Report
                return m_AllFiles.ToArray();
            }
        }

        /// <summary>
        /// Übermittelt die Daten zur Programmzeitschrift.
        /// </summary>
        /// <param name="table">Eine Tabelle der Programmzeitschrift.</param>
        private void DispatchEPG( EIT table )
        {
            // Be safe
            try
            {
                // Forward
                if (null != m_TransportStream)
                    m_TransportStream.AddEventTable( table.Table );
            }
            catch
            {
                // Ignore any error
            }
        }

        /// <summary>
        /// Meldet einen Datenstrom zum Datenempfang an.
        /// </summary>
        /// <param name="pid">Die gewünschte Datestromkennung (PID).</param>
        /// <param name="type">Die Art der Nutzdaten im Datenstrom.</param>
        /// <param name="stream">Der Empfänger der Daten.</param>
        private void AddConsumer( ushort pid, StreamTypes type, StreamBase stream )
        {
            // Register
            m_Consumers.Add( Hardware.AddConsumer( pid, type, stream.AddPayload ) );
        }

        /// <summary>
        /// Beendet die Aufzeichnung in eine Datei.
        /// </summary>
        public void CloseStream()
        {
            // Load
            using (var stream = m_TransportStream)
            {
                // Wipe out
                m_TransportStream = null;

                // Reset time
                LastActivationTime = null;

                // Forget current settings
                ActiveSelection = null;

                // Nothing more to do
                if (null == stream)
                    return;

                // Increment file counter
                ++m_FileCount;

                // Stop EPG receiver
                Hardware.RemoveProgramGuideConsumer( DispatchEPG );

                // Stop all consumers
                foreach (var consumer in m_Consumers)
                    try
                    {
                        // Try to stop the individual stream
                        Hardware.SetConsumerState( consumer, null );
                    }
                    catch
                    {
                        // Ignore any error
                    }

                // Forget consumers
                m_Consumers.Clear();

                // Send all the rest to the file (if any)
                stream.Flush();

                // Update byte count
                BytesBias += stream.Length;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung der Teildatenströme diese Quelle endgültig.
        /// </summary>
        public void Dispose()
        {
            // Be safe
            try
            {
                // Finish
                CloseStream();
            }
            catch
            {
                // Ignore any error
            }
        }

        #endregion
    }
}
