using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Configuration;


namespace JMS.DVB.TS
{
    /// <summary>
    /// A single transport stream.
    /// </summary>
    public class Manager : IDisposable, IStreamConsumer2
    {
        /// <summary>
        /// Die Voreinstellung für die Größe des Zwischenspeichers beim Schreiben in Dateien.
        /// </summary>
        public const int DefaultBufferSize = 2000000;

        /// <summary>
        /// Delegate to receive any data in process.
        /// </summary>
        public Action<byte[]> InProcessConsumer = null;

        /// <summary>
        /// The delay the created PCR advances the corresponding PTS of
        /// a video sequence header.
        /// </summary>
        /// <remarks>
        /// The default value <i>90 * 1000</i> lets the PCR run one second
        /// before the PTS since the reference clock is 90kHz.
        /// </remarks>
        public static long PCRDelay = 90000;

        /// <summary>
        /// Kann gesetzt werden, um zu verhindern, dass die Systemzeit (PCR) aus dem
        /// H.264 Bildsignal abgeleitet wird.
        /// </summary>
        public static bool DisablePCRForHDTV = false;

        /// <summary>
        /// Kann gesetzt werden, um zu verhindern, dass die Systemzeit (PCR) aus dem
        /// MPEG2 Bildsignal abgeleitet wird.
        /// </summary>
        public static bool DisablePCRForSDTV = false;

        /// <summary>
        /// The video delay to use.
        /// </summary>
        /// <remarks>
        /// The default is <i>-90 * 666</i> which makes up 2 /3 seconds
        /// on the 90kHz PTS clock. A negative value makes sure that video
        /// data will be available before the corresponding audio data is
        /// put into the transport stream.
        /// </remarks>
        public static long VideoDelay = -60000;

        /// <summary>
        /// If active the PES length of video packets will be calculated.
        /// </summary>
        public static bool SetVideoLength = true;

        /// <summary>
        /// Set the service identification for EPG injection.
        /// </summary>
        public SourceIdentifier EPGMapping = null;

        /// <summary>
        /// Just in case we send EPG data into the stream.
        /// </summary>
        private int m_EPGCounter = 0;

        /// <summary>
        /// The number of hops multicasting can use - the default is <i>1</i> 
        /// restricting multicast packets to the local network.
        /// </summary>
        public static readonly int MulticastTTL = 1;

        /// <summary>
        /// Maximale Anzahl von aufgestauten UDP Paketen - etwa 10 MByte pro
        /// TS Datenstrom.
        /// </summary>
        public static readonly int MaxUDPQueueLength = 1250;

        /// <summary>
        /// Delay for this transport stream - will be doubled for HDTV.
        /// </summary>
        private long m_PCRDelay;

        /// <summary>
        /// Wird gesetzt um zu verhindern, dass aus dem H.264 Datenstrom die Zeitbasis (PCR)
        /// abgeleitet wird.
        /// </summary>
        private bool m_NoHDTVPCR;

        /// <summary>
        /// Wird gesetzt um zu verhindern, dass aus dem MPEG2 Datenstrom die Zeitbasis (PCR)
        /// abgeleitet wird.
        /// </summary>
        private bool m_NoSDTVPCR;

        /// <summary>
        /// Delay for this transport stream - will be doubled for HDTV.
        /// </summary>
        private long m_VideoDelay;

        private UDPStreaming m_UDPStream = new UDPStreaming( MulticastTTL, MaxUDPQueueLength );

        /// <summary>
        /// Set as soon as the first PCR arrived.
        /// </summary>
        private int m_PCRAvailable = 0;

        /// <summary>
        /// Collects full PES packets per stream. 
        /// </summary>
        /// <remarks>
        /// The map is indexed with the transport stream identifier.
        /// </remarks>
        private Hashtable m_Buffers = new Hashtable();

        /// <summary>
        /// The <see cref="PVASplitter"/> needs a PTS guidance since PVA
        /// only works with 32-Bit PTS.
        /// </summary>
        private PVASplitter m_Splitter = null;

        /// <summary>
        /// The stream which guides the <see cref="PVASplitter"/> to use
        /// the correct PTS.
        /// </summary>
        private short m_GuidePID = 0;

        /// <summary>
        /// Helper buffer holding <i>0xff</i> padding bytes for a whole transport stream
        /// packet.
        /// </summary>
        private static byte[] Padding = new byte[PacketSize];

        /// <summary>
        /// Full size of a transport stream packet - 188 bytes.
        /// </summary>
        public const int FullSize = 4 + PacketSize;

        /// <summary>
        /// Maximum payload size for a transport stream packet - 184 bytes.
        /// </summary>
        public const int PacketSize = 184;

        /// <summary>
        /// All our streams.
        /// </summary>
        private ArrayList m_Streams = new ArrayList();

        /// <summary>
        /// The <see cref="Tables.PAT"/> for this transport stream - there can be only
        /// a single program in it.
        /// </summary>
        private Tables.PAT ProgramAssociation;

        /// <summary>
        /// Next transport stream identifier for automatic generation.
        /// <seealso cref="AddStream"/>
        /// </summary>
        public short NextPID = 0x0200;

        /// <summary>
        /// The <see cref="Tables.PMT"/> for the only program in this transport stream.
        /// </summary>
        private Tables.PMT ProgramMap;

        /// <summary>
        /// The service description for the only program included.
        /// </summary>
        private Tables.SDT ServiceDescription;

        /// <summary>
        /// Number of bytes processed.
        /// </summary>
        private long m_Length = 0;

        /// <summary>
        /// Number of Audio/Video bytes processed.
        /// </summary>
        private long m_AVLength = 0;

        /// <summary>
        /// The number of transport stream packets sent after the last PAT/PMT.
        /// </summary>
        private int PacketCounter = 0;

        /// <summary>
        /// Set when the PAT/PMT have been sent - reset after 1000 other packets are
        /// added to the transport stream.
        /// </summary>
        private bool PATSent = false;

        /// <summary>
        /// The output stream - typically a disk file.
        /// </summary>
        private Stream Target;

        /// <summary>
        /// Das aktuelle Ziel für alle Schreiboperationen.
        /// </summary>
        private DoubleBufferedFile BufferedTarget;

        /// <summary>
        /// Eine neue Datei, in die bei der nächsten Schreiboperation umgeschaltet werden soll.
        /// </summary>
        private DoubleBufferedFile PendingTarget;

        /// <summary>
        /// Currently active operation
        /// </summary>
        private IAsyncResult m_Writer = null;

        /// <summary>
        /// For the moment a synchronizer only.
        /// </summary>
        private object m_Queue = new object();

        /// <summary>
        /// Set when a HDTV video stream is added.
        /// </summary>
        private bool m_IsHDTV = false;

        /// <summary>
        /// Wird aktiviert, sobald die Systemuhr in eine Datei geschrieben wird.
        /// </summary>
        public Action<string, long, byte[]> OnWritingPCR;

        /// <summary>
        /// Die Größe des Zwischenspeichers für das Schreiben in Dateien.
        /// </summary>
        private int m_BufferSize = DefaultBufferSize;

        /// <summary>
        /// When set no data will be accepted in the corresponding streams.
        /// </summary>
        public bool IgnoreInput = false;

        /// <summary>
        /// Initialisiert statische Daten der Klasse.
        /// </summary>
        static Manager()
        {
            // Create padding
            for (int i = Padding.Length; i-- > 0; )
                Padding[i] = 0xff;

            // Check settings
            var videoLength = ConfigurationManager.AppSettings["TS.SetVideoLength"];
            var noHDTVPCR = ConfigurationManager.AppSettings["TS.DisableHDTVPCR"];
            var noSDTVPCR = ConfigurationManager.AppSettings["TS.DisableSDTVPCR"];
            var maxUDPQueue = ConfigurationManager.AppSettings["TS.MaxUDPQueue"];
            var videoDelay = ConfigurationManager.AppSettings["TS.VideoDelay"];
            var multiTTL = ConfigurationManager.AppSettings["Multicast.TTL"];
            var pcrDelay = ConfigurationManager.AppSettings["TS.PCRDelay"];

            // Overwrite
            if (!string.IsNullOrEmpty( pcrDelay ))
                PCRDelay = long.Parse( pcrDelay );
            if (!string.IsNullOrEmpty( videoDelay ))
                VideoDelay = long.Parse( videoDelay );
            if (!string.IsNullOrEmpty( multiTTL ))
                MulticastTTL = int.Parse( multiTTL );
            if (!string.IsNullOrEmpty( videoLength ))
                SetVideoLength = bool.Parse( videoLength );
            if (!string.IsNullOrEmpty( maxUDPQueue ))
                MaxUDPQueueLength = int.Parse( maxUDPQueue );
            if (!string.IsNullOrEmpty( noHDTVPCR ))
                DisablePCRForHDTV = bool.Parse( noHDTVPCR );
            if (!string.IsNullOrEmpty( noSDTVPCR ))
                DisablePCRForSDTV = bool.Parse( noSDTVPCR );
        }

        /// <summary>
        /// Create a transport stream with no attached physical file.
        /// </summary>
        public Manager()
            : this( (Stream) null )
        {
        }

        /// <summary>
        /// Create a transport stream on a <see cref="Stream"/>.
        /// </summary>
        /// <param name="target">Typically a disk file.</param>
        public Manager( Stream target )
            : this( target, 0 )
        {
        }

        /// <summary>
        /// Erzeugt einen neuen Datenstrom.
        /// </summary>
        /// <param name="path">Optional der volle Pfad zu einer Datei.</param>
        public Manager( string path )
            : this( path, 0 )
        {
        }

        /// <summary>
        /// Create a transport stream on a file.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="nextPID">Optional initial PID counter.</param>
        public Manager( string path, short nextPID )
            : this( path, nextPID, DefaultBufferSize )
        {
        }

        /// <summary>
        /// Erzeugt einen neuen Datenstrom.
        /// </summary>
        /// <param name="path">Optional der volle Pfad zu einer Datei.</param>
        /// <param name="nextPID">Die als nächstes zu verwendende Datenstromkennung.</param>
        /// <param name="bufferSize">Die Größe des zu verwendenden Zwischenspeichers.</param>
        /// <exception cref="ArgumentOutOfRangeException">Der Zwischenspeicher muss mindestens 1.000 Bytes groß sein.</exception>
        public Manager( string path, short nextPID, int bufferSize )
            : this( (Stream) null, nextPID )
        {
            // Validate
            if (bufferSize <= 1000)
                throw new ArgumentOutOfRangeException( "bufferSize" );

            // Remember
            m_BufferSize = bufferSize;

            // Open the file
            if (path != null)
                BufferedTarget = CreateBuffered( path );
        }

        /// <summary>
        /// Erzeugt einen neuen, doppelt gepufferten Bereich für das Schreiben in eine Datei.
        /// </summary>
        /// <param name="filePath">Der volle Pfad zur Datei.</param>
        /// <returns>Der gewünschte Speicherbereich.</returns>
        private DoubleBufferedFile CreateBuffered( string filePath )
        {
            // Process
            return new DoubleBufferedFile( filePath, m_BufferSize );
        }

        /// <summary>
        /// Prüft, ob eine nahtlose Auftrennung der Aufzeichnungsdatei unterstützt wird.
        /// </summary>
        public bool CanSplitFile
        {
            get
            {
                // Check mode of operation
                if (BufferedTarget != null)
                    if (Target == null)
                        return true;

                // Nope
                return false;
            }
        }

        /// <summary>
        /// Beginnt bei nächster Gelegenheit mit dem Beschreiben einer neuen Datei.
        /// </summary>
        /// <param name="newFilePath">Der volle Pfad zur gewünschten Zieldatei.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Zieldatei angegeben.</exception>
        public void SplitFile( string newFilePath )
        {
            // Validate
            if (string.IsNullOrEmpty( newFilePath ))
                throw new ArgumentNullException( "newFilePath" );

            // Check mode of operation
            if (!CanSplitFile)
                throw new InvalidOperationException();

            // Create new file and install it
            using (Interlocked.Exchange( ref PendingTarget, CreateBuffered( newFilePath ) ))
            {
                // The new target is now installed and the previous one will be discarded unused - if any existed
            }
        }

        /// <summary>
        /// Create a transport stream on a <see cref="Stream"/>.
        /// </summary>
        /// <param name="target">Typically a disk file.</param>
        /// <param name="nextPID">Optional initial PID counter.</param>
        public Manager( Stream target, short nextPID )
        {
            // Fix delay
            m_NoHDTVPCR = DisablePCRForHDTV;
            m_NoSDTVPCR = DisablePCRForSDTV;
            m_VideoDelay = VideoDelay;
            m_PCRDelay = PCRDelay;

            // Use
            if ((nextPID >= 0x200) && (nextPID <= 0xf80))
                NextPID = nextPID;

            // Remember
            Target = target;

            // Create helper
            ProgramAssociation = new Tables.PAT();

            // Configure
            ProgramAssociation.ProgramStream = NextPID++;
            ProgramAssociation.ProgramNumber = NextPID++;

            // Create helper
            ProgramMap = new Tables.PMT( ProgramAssociation.ProgramStream, ProgramAssociation.ProgramNumber );
            ServiceDescription = new Tables.SDT( ProgramAssociation.NetworkIdentifier, ProgramAssociation.ProgramNumber );
        }

        /// <summary>
        /// Allow us to enforce changes on the PAT version stamp.
        /// </summary>
        public int PATVersion
        {
            get
            {
                // Forward
                return ProgramAssociation.TableVersion;
            }
            set
            {
                // Update
                ProgramAssociation.TableVersion = value;
            }
        }

        /// <summary>
        /// Send PAT/PMT to the transport stream if necessary.
        /// </summary>
        private void SendPAT()
        {
            // Must synchronize
            lock (m_Queue)
            {
                // Already done
                if (PATSent) return;

                // Reset
                PacketCounter = 0;
                PATSent = true;

                // Forward
                ProgramAssociation.Send( this );
                ProgramMap.Send( this );
                //ServiceDescription.Send(this);
            }
        }

        /// <summary>
        /// Send a table to the transport stream.
        /// </summary>
        /// <param name="counter">Individual packet counter.</param>
        /// <param name="pid">Transport stream identifier to use.</param>
        /// <param name="buffer">Full table data.</param>
        public void SendTable( ref int counter, int pid, byte[] buffer )
        {
            // Get the chunks
            int packs = (buffer.Length + PacketSize - 1) / PacketSize;
            int rest = buffer.Length % PacketSize;

            // Forward
            Send( ref counter, pid, buffer, 0, packs, true, (0 == rest) ? PacketSize : rest, true, -1 );
        }

        /// <summary>
        /// Create a new video stream and add it to this transport stream.
        /// </summary>
        /// <param name="encoding">Encoding of the video stream.</param>
        /// <returns>The newly created video stream instance.</returns>
        public VideoStream AddVideo( byte encoding )
        {
            // Forward
            return AddVideo( encoding, false );
        }

        /// <summary>
        /// Create a new video stream and add it to this transport stream.
        /// </summary>
        /// <param name="encoding">Encoding of the video stream.</param>
        /// <param name="noPCR">Set to disable PCR generation.</param>
        /// <returns>The newly created video stream instance.</returns>
        public VideoStream AddVideo( byte encoding, bool noPCR )
        {
            // Check mode
            var isH264 = (encoding == (byte) EPG.StreamTypes.H264);
            var forbidPCR = (m_NoHDTVPCR && isH264) || (m_NoSDTVPCR && !isH264);

            // Run
            bool isPCR;
            short pid = AddStream( StreamTypes.Video, encoding, noPCR || forbidPCR, false, null, out isPCR );

            // Create the correct type of stream
            VideoStream video;
            if (isH264)
            {
                // Remember
                m_IsHDTV = true;

                // H.264
                video = new HDTVStream( this, pid, isPCR );
            }
            else
            {
                // MPEG-2
                video = new VideoStream( this, pid, isPCR );
            }

            // Remember
            m_Streams.Add( video );

            // Report
            return video;
        }

        /// <summary>
        /// Report if this Transport Stream includes a H.264 video stream.
        /// </summary>
        public bool HasHDTVVideo
        {
            get
            {
                // Report
                return m_IsHDTV;
            }
        }

        /// <summary>
        /// Create a new audio stream and add it to this transport stream.
        /// </summary>
        /// <param name="name">The ISO name of the language for this audio stream.</param>
        /// <returns>The newly created audio stream instance.</returns>
        public AudioStream AddAudio( string name )
        {
            // Flag
            bool isPCR;

            // Run
            short pid = AddStream( StreamTypes.Audio, 255, false, false, null, out isPCR );

            // Set the name of the language
            if (!string.IsNullOrEmpty( name )) ProgramMap.SetAudioLanguage( pid, name );

            // Create
            AudioStream audio = new AudioStream( this, pid, isPCR );

            // Remember
            m_Streams.Add( audio );

            // Make it the guide
            if ((null != m_Splitter) && (0 == m_GuidePID)) m_GuidePID = pid;

            // Report
            return audio;
        }

        /// <summary>
        /// Add a new Dolby Digital Audio Stream to this <i>Transport Stream</i>.
        /// </summary>
        /// <returns>The new data stream.</returns>
        public DolbyStream AddDolby()
        {
            // Forward
            return AddDolby( null );
        }

        /// <summary>
        /// Create a new Dobly Digital (AC3) audio stream and add it to this transport stream.
        /// </summary>
        /// <returns>The newly created AC3 audio stream instance.</returns>
        public DolbyStream AddDolby( string name )
        {
            // Flag
            bool isPCR;

            // Run
            short pid = AddStream( StreamTypes.Private, 255, false, false, null, out isPCR );

            // Set the name of the language
            if (!string.IsNullOrEmpty( name )) ProgramMap.SetAudioLanguage( pid, name );

            // Create
            DolbyStream dolby = new DolbyStream( this, pid, isPCR );

            // Remember
            m_Streams.Add( dolby );

            // Make it the guide
            if ((null != m_Splitter) && (0 == m_GuidePID)) m_GuidePID = pid;

            // Report
            return dolby;
        }

        /// <summary>
        /// Create a new teletext stream and add it to this transport stream.
        /// </summary>
        /// <returns>The newly created teletext stream instance.</returns>
        public TTXStream AddTeleText()
        {
            // Flag
            bool isPCR;

            // Run
            short pid = AddStream( StreamTypes.TeleText, 255, false, true, null, out isPCR );

            // Create
            TTXStream ttx = new TTXStream( this, pid, isPCR );

            // Remember
            m_Streams.Add( ttx );

            // Make it the guide
            if ((null != m_Splitter) && (0 == m_GuidePID)) m_GuidePID = pid;

            // Report
            return ttx;
        }

        /// <summary>
        /// Create a new stream holding DVB subtitles.
        /// </summary>
        /// <param name="info">Information on the contents of this subtitle stream.</param>
        /// <returns>The new subtitle stream.</returns>
        public SubtitleStream AddSubtitles( EPG.SubtitleInfo[] info )
        {
            // Flag
            bool isPCR;

            // Run
            short pid = AddStream( StreamTypes.SubTitles, 255, false, true, info, out isPCR );

            // Create
            SubtitleStream sub = new SubtitleStream( this, pid, isPCR );

            // Remember
            m_Streams.Add( sub );

            // Make it the guide
            if ((null != m_Splitter) && (0 == m_GuidePID)) m_GuidePID = pid;

            // Report
            return sub;
        }

        /// <summary>
        /// Create a new transport stream identifier for a new stream
        /// in this transport stream.
        /// </summary>
        /// <param name="type">Type of the stream.</param>
        /// <param name="encoding">Encoding type of the stream.</param>
        /// <param name="isPCR">Set if the stream will be the PCR reference.</param>
        /// <param name="noPTS">Set if the stream should not participate in PTS synchronisation.</param>
        /// <param name="info">Information on the contents of a subtitle stream.</param>
        /// <param name="noPCR">Set to disable PCR from PTS generation.</param>
        /// <returns>A randomly choosen but unique transport stream identifier.</returns>
        private short AddStream( StreamTypes type, byte encoding, bool noPCR, bool noPTS, EPG.SubtitleInfo[] info, out bool isPCR )
        {
            // Create pid
            short pid = NextPID++;

            // Make key
            int keyPID = pid;

            // Forward
            isPCR = ProgramMap.Add( type, encoding, pid, noPCR, info );

            // Reload
            lock (m_Queue)
            {
                // Force PAT change
                PATSent = false;

                // Load
                Packet buffers = (Packet) m_Buffers[keyPID];

                // Create new
                if (null == buffers)
                {
                    // Create new
                    buffers = new Packet( this, keyPID );

                    // Remember
                    m_Buffers[keyPID] = buffers;
                }

                // Set up
                if (StreamTypes.Video == type)
                    buffers.SetAudioVideo( true );
                else if ((StreamTypes.Audio == type) || (StreamTypes.Private == type))
                    buffers.SetAudioVideo( false );

                // May disable PTS synchronisation (e.g. for TeleText streams)
                buffers.IgnorePTS = noPTS;
            }

            // Report
            return pid;
        }

        /// <summary>
        /// Send a PCR to the transport stream.
        /// </summary>
        /// <param name="counter">Individual packet counter which will not be incremented.</param>
        /// <param name="pid">Related transport stream identifier.</param>
        /// <param name="pts">The PTS from a PES header used for PCR.</param>
        void IStreamConsumer.SendPCR( int counter, int pid, long pts )
        {
            // Remember
            if (0 == m_PCRAvailable)
                m_PCRAvailable = -pid;

            // See if its time to send the PAT and PMT
            SendPAT();

            // Validate
            if ((counter < 0) || (counter > 0xf))
                throw new ArgumentOutOfRangeException( "counter", counter, "only four bits allowed" );
            if ((pid < 0) || (pid >= 0x1fff))
                throw new ArgumentOutOfRangeException( "pid", pid, "only 13 bits allowed" );

            // Correct a bit (90kHz)
            long pcr = pts - m_PCRDelay;

            // Correct
            if (pcr < 0)
                pcr += Packet.PTSOverrun;
            else if (pcr >= Packet.PTSOverrun)
                pcr -= Packet.PTSOverrun;

            // Split
            byte pidh = (byte) (pid >> 8);
            byte pidl = (byte) (pid & 0xff);

            // Allocate data
            byte[] ts = new byte[FullSize];

            // Process the header
            ts[0] = 0x47;
            ts[1] = pidh;
            ts[2] = pidl;
            ts[3] = (byte) (0x20 | ((counter - 1) & 0x0f));

            // Process adaption control
            ts[4] = 0xb7;
            ts[5] = 0x10;
            ts[6] = (byte) ((pcr >> 25) & 0xff);
            ts[7] = (byte) ((pcr >> 17) & 0xff);
            ts[8] = (byte) ((pcr >> 9) & 0xff);
            ts[9] = (byte) ((pcr >> 1) & 0xff);
            ts[10] = (byte) (128 * (pcr & 0x01));
            ts[11] = 0x00;

            // Pad the rest
            Array.Copy( Padding, 0, ts, 12, ts.Length - 12 );

            // Enqueue to writer
            Enqueue( ts, pid, false, true, pts );
        }

        /// <summary>
        /// Rekonstruiert die Systemuhr aus einem elementaren Paket. Das Paket
        /// wird nicht auf Konsistenz geprüft.
        /// </summary>
        /// <param name="packet">Ein elementares Paket.</param>
        /// <returns>Die gewünschte Systemzeit.</returns>
        public static TimeSpan GetPCR( byte[] packet )
        {
            // Load parts
            long b0 = packet[6];
            long b1 = packet[7];
            long b2 = packet[8];
            long b3 = packet[9];
            long b4 = (packet[10] >> 7);

            // Merge
            long clockTicks = b4 + 2 * (b3 + 256 * (b2 + 256 * (b1 + 256 * b0)));

            // Calculate back from 90kHz clock
            return new TimeSpan( clockTicks * 1000 / 9 );
        }

        /// <summary>
        /// Report if a PCR has been sent to the stream.
        /// </summary>
        bool IStreamConsumer.PCRAvailable
        {
            get
            {
                // Report
                return (m_PCRAvailable > 0);
            }
        }

        /// <summary>
        /// Reports if all input should be discarded.
        /// </summary>
        bool IStreamConsumer2.IgnoreInput
        {
            get
            {
                // Forward
                return IgnoreInput;
            }
        }

        /// <summary>
        /// Send some data to the transport stream.
        /// </summary>
        /// <remarks>
        /// Padding will be done after the payload by simply filling the transport packing with
        /// <i>0xff</i>. Although this should be identical to using adaption fields just before
        /// the payloads some tools are not able to decode tables padded this way.
        /// </remarks>
        /// <param name="counter">The corresponding packet counter which will be updated.</param>
        /// <param name="pid">The transport stream identifier to use.</param>
        /// <param name="buffer">Data source.</param>
        /// <param name="start">First byte to send.</param>
        /// <param name="packs">Number of transport stream packets to send.</param>
        /// <param name="isFirst">Set if the first byte is the first byte of a PES header.</param>
        /// <param name="sizeOfLast">Number of bytes in the last packet - which may be padded.</param>
        /// <param name="pts">If not negative this is the PES headers PTS - <i>isFirst</i> will be set.</param>
        void IStreamConsumer.Send( ref int counter, int pid, byte[] buffer, int start, int packs, bool isFirst, int sizeOfLast, long pts )
        {
            // Forward
            Send( ref counter, pid, buffer, start, packs, isFirst, sizeOfLast, false, pts );
        }

        /// <summary>
        /// Send some data to the transport stream.
        /// </summary>
        /// <param name="counter">The corresponding packet counter which will be updated.</param>
        /// <param name="pid">The transport stream identifier to use.</param>
        /// <param name="buffer">Data source.</param>
        /// <param name="start">First byte to send.</param>
        /// <param name="packs">Number of transport stream packets to send.</param>
        /// <param name="isFirst">Set if the first byte is the first byte of a PES header.</param>
        /// <param name="sizeOfLast">Number of bytes in the last packet - which may be padded.</param>
        /// <param name="standardPadding">Set if padding will be done using an adpation field
        /// just before the payload data.</param>
        /// <param name="pts">If not negative this is the PES headers PTS - <i>isFirst</i> will be set.</param>
        private void Send( ref int counter, int pid, byte[] buffer, int start, int packs, bool isFirst, int sizeOfLast, bool standardPadding, long pts )
        {
            // Inform the splitter as soon as possible
            if ((pts >= 0) && (pid == m_GuidePID) && (null != m_Splitter))
                m_Splitter.GuidePTS = pts;

            // See if its time to send the PAT and PMT
            SendPAT();

            // Validate
            if ((counter < 0) || (counter > 0xf))
                throw new ArgumentOutOfRangeException( "counter", counter, "only four bits allowed" );
            if ((pid < 0) || (pid >= 0x1fff))
                throw new ArgumentOutOfRangeException( "pid", pid, "only 13 bits allowed" );
            if (null == buffer)
                throw new ArgumentNullException( "buffer" );
            if ((start < 0) || (start > buffer.Length))
                throw new ArgumentOutOfRangeException( "start", start, "exceeds buffer size" );
            if ((packs < 0) || (packs > (buffer.Length / PacketSize + 1)))
                throw new ArgumentOutOfRangeException( "packs", packs, "exceeds buffer size" );
            if ((sizeOfLast < 0) || (sizeOfLast > PacketSize))
                throw new ArgumentOutOfRangeException( "sizeOfLast", sizeOfLast, "exceeds packet size" );

            // Done
            if (0 == packs)
                return;

            // Check mode
            bool mustPad = (sizeOfLast < PacketSize);

            // Padding mode
            bool useSafePadding = (mustPad && !standardPadding && (sizeOfLast <= (PacketSize - 2)));

            // Calculate
            int end = start + (mustPad ? (packs - 1) : packs) * PacketSize + (mustPad ? sizeOfLast : 0);

            // Validate
            if (end > buffer.Length)
                throw new ArgumentOutOfRangeException( "packs", packs, "exceeds buffer size" );

            // Allocate data
            byte[] ts = new byte[packs * FullSize];

            // Split
            byte pidh = (byte) (pid >> 8);
            byte pidl = (byte) (pid & 0xff);

            // Flag
            if (isFirst)
                pidh |= 0x40;

            // Fill buffers
            for (int tsi = 0; start < end; start += PacketSize, tsi += PacketSize)
            {
                // Process the header
                ts[tsi++] = 0x47;
                ts[tsi++] = pidh;
                ts[tsi++] = pidl;
                ts[tsi++] = (byte) (0x10 | counter++);

                // Reset
                if (4 == tsi)
                    pidh &= 0x1f;

                // Correct
                counter &= 0x0f;

                // Check rest
                int rest = end - start;

                // Special mode
                bool last = (rest < PacketSize);

                // Check mode
                if (last && useSafePadding)
                {
                    // Activate adaption
                    ts[tsi - 1] |= 0x20;

                    // Padding size
                    int pad = PacketSize - rest - 2;

                    // Add adaption
                    ts[tsi + 0] = (byte) (pad + 1);
                    ts[tsi + 1] = 0x00;

                    // Pad first
                    Array.Copy( Padding, 0, ts, tsi + 2, pad );

                    // Data last
                    Array.Copy( buffer, start, ts, tsi + 2 + pad, rest );
                }
                else
                {
                    // Move in
                    Array.Copy( buffer, start, ts, tsi, (rest >= PacketSize) ? PacketSize : rest );

                    // Pad
                    if (last)
                        Array.Copy( Padding, 0, ts, tsi + rest, PacketSize - rest );
                }
            }

            // Must synchronize
            lock (m_Queue)
            {
                // Enqueue to writer
                Enqueue( ts, pid, isFirst, false, pts );

                // Count
                PacketCounter += packs;

                // Must resend
                if (PacketCounter > 200)
                    PATSent = false;
            }
        }

        /// <summary>
        /// Send all buffered data to the transport stream. 
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="withPCR">Set to flush PCR, too.</param>
        private void Flush( int pid, bool withPCR )
        {
            // Move full packet to synchronizer queue
            Enqueue( pid, withPCR );

            // Cleanup as much as possible
            for (bool processed = true; processed; processed = false)
            {
                // First get rid of all streams with no PTS or no data at all
                foreach (Packet buffers in m_Buffers.Values)
                    if (buffers.DequeueNoPTS())
                        processed = true;

                // Process as long as there are packets in each stream
                for (bool find = true; find; )
                {
                    // The minium PTS packet
                    Packet minHolder = null;

                    // Process all streams
                    foreach (Packet buffers in m_Buffers.Values)
                        if (!buffers.HasQueue)
                        {
                            // Can safely continue if this stream provides no PTS or we are not synchronizing this stream
                            if (buffers.PTSMissing || buffers.IgnorePTS)
                                continue;

                            // Can safely continue if this stream is currently not active
                            if (!buffers.IsActive)
                                continue;

                            // At least one queue is empty - stop right now
                            find = false;

                            // Done
                            break;
                        }
                        else if (null == minHolder)
                        {
                            // Use as is
                            minHolder = buffers;
                        }
                        else
                        {
                            // Load PTS
                            long test = buffers.FirstPTS, min = minHolder.FirstPTS;

                            // Correct for 0 pass - only one (video) can come out negative
                            if (test < Packet.PTSOverrun / 10)
                            {
                                // Must be some small absolute time
                                if (min > Packet.PTSOverrun / 2)
                                    test += Packet.PTSOverrun;
                            }
                            else if (min < Packet.PTSOverrun / 10)
                            {
                                // Must be some small absolute time
                                if (test > Packet.PTSOverrun / 2)
                                    min += Packet.PTSOverrun;
                            }

                            // Is a better hit
                            if (test < min)
                                minHolder = buffers;
                        }

                    // Done
                    if (!find || (null == minHolder))
                        break;

                    // Send data
                    if (minHolder.Dequeue())
                        processed = true;
                }

                // Finally see if queues become too long (PID is not receiving any data)
                foreach (Packet buffers in m_Buffers.Values)
                    if (buffers.DequeueOverflow())
                        processed = true;
            }
        }

        /// <summary>
        /// Send all buffered data to the transport stream. 
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="withPCR">Set to flush PCR, too.</param>
        private void Enqueue( int pid, bool withPCR )
        {
            // Attach to the list
            Packet buffers = (Packet) m_Buffers[pid];

            // Is empty
            if (null == buffers)
                return;

            // Collector packet
            Packet collect = null;

            // Create if not shutting down the stream
            if (!withPCR)
            {
                // Create new
                collect = new Packet( buffers );

                // Clone PTS from PES
                collect.PTS = buffers.PTS;
            }
            else
            {
                // Flush queue
                while (buffers.HasQueue) buffers.Dequeue();
            }

            // Mode
            bool isPCR = false;

            // Process all
            for (int ib = 0, imax = buffers.Count; ib < imax; )
            {
                // Load next
                byte[] buf = buffers[ib++];

                // Switch only
                if (null == buf)
                {
                    // Mark
                    isPCR = true;

                    // Next
                    continue;
                }

                // Last PCR belongs to next PES
                if (isPCR)
                    if (ib == imax)
                        if (!withPCR)
                        {
                            // Reset
                            buffers.Clear();

                            // Append as PCR
                            buffers.Add( null );
                            buffers.Add( buf );

                            // Remember collected data
                            if (collect.Count > 0)
                                buffers.Enqueue( collect );

                            // Finish
                            return;
                        }

                // Reset flag
                isPCR = false;

                // Check mode
                if (null == collect)
                {
                    // Send to file immediately to clenaup all buffers
                    SendBuffer( buf, buffers );
                }
                else
                {
                    // Save for later processing
                    collect.Add( buf );
                }
            }

            // Reset
            buffers.Clear();

            // Remember collected data
            if (null != collect) buffers.Enqueue( collect );
        }

        /// <summary>
        /// Send the buffer to the target streams.
        /// </summary>
        /// <param name="buf">Full buffer to send.</param>
        /// <param name="source">The originator of the buffer.</param>
        /// <returns>Set if buffer is not empty.</returns>
        internal bool SendBuffer( byte[] buf, Packet source )
        {
            // Validate
            Debug.Assert( (buf.Length % FullSize) == 0 );

            // Count
            m_Length += buf.Length;

            // Count sometimes
            if (source != null)
            {
                // Count bytes for audio and video only
                if (source.IsAudioOrVideo)
                    m_AVLength += buf.Length;

                // See if PCR is now available
                if (source.PID == -m_PCRAvailable)
                    m_PCRAvailable = source.PID;
            }

            // Has file output
            if (null != Target)
            {
                // Must synchronize
                WaitDisk();

                // Start an asynchronous write operation
                m_Writer = Target.BeginWrite( buf, 0, buf.Length, null, null );
            }

            // Has buffered output
            if (null != BufferedTarget)
            {
                // See if anyone is interested in PCR reports
                var pcrReport = OnWritingPCR;

                // Check mode
                if (pcrReport != null)
                    if ((buf[3] & 0x20) != 0)
                        if (buf[4] >= 1)
                            if ((buf[5] & 0x10) != 0)
                                pcrReport( BufferedTarget.FilePath, BufferedTarget.TotalBytesWritten, buf );

                // Send to file
                BufferedTarget.Write( buf, 0, buf.Length );
            }

            // See if there is a file switch pending
            var pendingSwitch = Interlocked.Exchange( ref PendingTarget, null );

            // Activate the new one - this will enforce a flush on the previous file and may lead to corruption
            if (pendingSwitch != null)
                using (BufferedTarget)
                    BufferedTarget = pendingSwitch;

            // Nothing to do
            if (buf.Length < 1)
                return false;

            // Forward to optional stream
            m_UDPStream.Send( buf );

            // Read consumer
            var consumer = InProcessConsumer;

            // Finally send to in-process consumer
            if (null != consumer)
                consumer( buf );

            // Found some
            return true;
        }

        /// <summary>
        /// Send the indicated buffer to the target stream.
        /// </summary>
        /// <param name="buf">The full buffer which may be queued for later processing.</param>
        /// <param name="pid">The transport stream identifier to use.</param>
        /// <param name="isFirst">Set if the first packet starts at a PES header.</param>
        /// <param name="isPCR">Set if this is a PCR report.</param>
        /// <param name="pts">If not negative this is the PES headers PTS - <i>isFirst</i> will be set.</param>
        private void Enqueue( byte[] buf, int pid, bool isFirst, bool isPCR, long pts )
        {
            // Check mode
            bool isTSInfo = ((ProgramAssociation.PID == pid) || (ProgramMap.PID == pid) || (ServiceDescription.PID == pid) || (0x12 == pid));

            // Must serialize access
            lock (m_Queue)
            {
                // Direct mode
                if (isTSInfo)
                {
                    // Forward
                    SendBuffer( buf, null );

                    // Done
                    return;
                }

                // Attach to buffer
                Packet buffers = (Packet) m_Buffers[pid];

                // Send complete PES packet collected
                if (isFirst) Flush( pid, false );

                // Create once
                if (null == buffers) return;

                // Update PTS for the next packet
                buffers.PTS = pts;

                // PCR mark
                if (isPCR) buffers.Add( null );

                // Data
                buffers.Add( buf );
            }
        }

        /// <summary>
        /// Wait for the current asyncrhonous operation to finish.
        /// </summary>
        private void WaitDisk()
        {
            // Nothing to do
            if (null == m_Writer) return;

            // Wait
            if (!m_Writer.IsCompleted) m_Writer.AsyncWaitHandle.WaitOne();

            // Finish
            if (null != Target) Target.EndWrite( m_Writer );

            // Reset
            m_Writer = null;
        }

        /// <summary>
        /// Flush all outstanding data to the target stream.
        /// </summary>
        /// <returns>
        /// Report about the operation.
        /// </returns>
        public string Flush()
        {
            // Buffer
            StringBuilder stat = new StringBuilder();

            // Must serialize access
            lock (m_Queue)
            {
                // Send all
                foreach (DictionaryEntry ent in m_Buffers)
                {
                    // Retrieve
                    int pid = (int) ent.Key;
                    Packet buffers = (Packet) ent.Value;

                    // Send
                    Flush( pid, true );
                }

                // Create report
                foreach (DictionaryEntry ent in m_Buffers)
                {
                    // Retrieve
                    int pid = (int) ent.Key;
                    Packet buffers = (Packet) ent.Value;

                    // Separate
                    if (stat.Length > 0) stat.Append( "\r\n" );

                    // Add report
                    stat.AppendFormat( "{0}: MaxQueue={1} Overflow(s)={2}", pid, buffers.MaxQueueLength, buffers.Overflows );
                }

                // Forward
                if (null != m_Splitter) m_Splitter.Dispose();

                // Reset
                m_Splitter = null;
                m_Buffers.Clear();
                m_GuidePID = 0;

                // Forward
                if (null != Target)
                {
                    // Must synchronize
                    WaitDisk();

                    // Forward
                    Target.Flush();
                }

                // Forward
                if (null != BufferedTarget)
                    BufferedTarget.Flush();
            }

            // Report
            return stat.ToString();
        }

        /// <summary>
        /// Start all PID filters attached to this transport stream.
        /// </summary>
        public void StartFilters()
        {
            // Unlock splitter if no guide can be found
            if (m_GuidePID == 0)
                if (m_Splitter != null)
                    m_Splitter.GuidePTS = -1;
        }

        /// <summary>
        /// Stop all PID filters attached to this transport stream.
        /// </summary>
        public void StopFilters()
        {
            // Stop streaming
            SetStreamTarget( "localhost", 0 );

            // Forward to splitter
            if (m_Splitter != null)
                m_Splitter.Stop();
        }

        /// <summary>
        /// Report the current file size.
        /// </summary>
        public long Length
        {
            get
            {
                // Synchronized report
                lock (m_Queue) return m_Length;
            }
        }

        /// <summary>
        /// Report the current number of processes audio or video bytes.
        /// </summary>
        public long AVLength
        {
            get
            {
                // Synchronized report
                lock (m_Queue) return m_AVLength;
            }
        }

        /// <summary>
        /// Change the TCP/IP UDP receiver of this transport stream.
        /// </summary>
        /// <param name="client">Name of the client system. If the format <i>*IP Address</i> is
        /// used multicast addressing is assumed.</param>
        /// <param name="port">TCP/IP UDP Port to use.</param>
        public void SetStreamTarget( string client, int port )
        {
            // Forward
            m_UDPStream.SetStreamTarget( client, port );
        }

        /// <summary>
        /// Get the TCP/IP client to receive the stream.
        /// </summary>
        public string TCPClient
        {
            get
            {
                // Forward
                return m_UDPStream.Client;
            }
        }

        /// <summary>
        /// Get the TCP/IP UDP port to send the stream to.
        /// </summary>
        public int TCPPort
        {
            get
            {
                // Forward
                return m_UDPStream.Port;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public PVASplitter PVASplitter
        {
            get
            {
                // Report
                return m_Splitter;
            }
            set
            {
                // Remember
                m_Splitter = value;

                // Reset guide
                m_GuidePID = 0;
            }
        }

        /// <summary>
        /// Meldet den aktuell benutzen Versazu zwischen Bild und Ton.
        /// </summary>
        public long ActiveVideoDelay
        {
            get
            {
                // Report
                return m_VideoDelay;
            }
        }

        /// <summary>
        /// Meldet die Anzahl der aktiven Datenströme.
        /// </summary>
        public int StreamCount
        {
            get
            {
                // Report
                return ProgramMap.Count;
            }
        }

        /// <summary>
        /// Ergänzt einen Eintrag der Programmzeitschrift.
        /// </summary>
        /// <param name="section">Ein beliebiger Eintrag der Programmzeitschrift.</param>
        public void AddEventTable( EPG.Section section )
        {
            // Forward
            if (null != section)
                if (section.IsValid)
                    AddEventTable( section.Table as EPG.Tables.EIT );
        }

        /// <summary>
        /// Ergänzt einen Eintrag der Programmzeitschrift.
        /// </summary>
        /// <param name="eit">Ein beliebiger Eintrag der Programmzeitschrift.</param>
        public void AddEventTable( EPG.Tables.EIT eit )
        {
            // Ignore until PCR is available
            if (m_PCRAvailable <= 0)
                return;

            // Load the current mapping
            var mapping = EPGMapping;

            // Not enabled
            if (null == mapping)
                return;

            // None
            if ((null == eit) || !eit.IsValid)
                return;

            // Compare - by service first because normally the rest is equal
            if (mapping.Service != eit.ServiceIdentifier)
                return;
            if (mapping.Network != eit.OriginalNetworkIdentifier)
                return;
            if (mapping.TransportStream != eit.TransportStreamIdentifier)
                return;

            // Update the table
            eit.OriginalNetworkIdentifier = (ushort) ProgramAssociation.NetworkIdentifier;
            eit.TransportStreamIdentifier = 1;
            eit.ServiceIdentifier = (ushort) ProgramAssociation.ProgramNumber;

            // Has running or nearly running?
            if (!Array.Exists( eit.Entries, entry => (EPG.EventStatus.Running == entry.Status) || (EPG.EventStatus.NotRunning == entry.Status) ))
                return;

            // Try to recreate
            byte[] table = eit.Section.CreateSITable();

            // Inject
            if (null != table)
                SendTable( ref m_EPGCounter, 0x12, table );
        }

        /// <summary>
        /// Legt den Filter für die Programmzeitschrift fest.
        /// </summary>
        /// <param name="network">Die originale Netzwerkkenung des zugehörigen Dienstes.</param>
        /// <param name="transportStream">Die Kennung des <i>Transport Streams</i>.</param>
        /// <param name="service">Die eindeutige Kennung des Dienstes zu dieser Datei.</param>
        public void SetEPGMapping( ushort network, ushort transportStream, ushort service )
        {
            // Forward
            EPGMapping = new SourceIdentifier( network, transportStream, service );
        }

        #region IDisposable Members

        /// <summary>
        /// Call <see cref="Flush()"/> and the <see cref="Stream.Close"/> the target
        /// stream.
        /// </summary>
        public void Dispose()
        {
            // Forward to all
            foreach (StreamBase stream in m_Streams)
                stream.Dispose();

            // Forget
            m_Streams.Clear();

            // Cleanup
            Flush();

            // Finish
            if (null != Target)
            {
                // Done
                Target.Close();

                // Forget
                Target = null;
            }

            // Check file targets
            using (PendingTarget)
                PendingTarget = null;
            using (BufferedTarget)
                BufferedTarget = null;

            // Release socket
            SetStreamTarget( "localhost", 0 );
        }

        #endregion
    }
}
