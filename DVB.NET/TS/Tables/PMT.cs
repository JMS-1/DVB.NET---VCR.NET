using System;
using JMS.DVB.EPG;
using System.Collections;
using JMS.DVB.EPG.Descriptors;
using System.Collections.Generic;


namespace JMS.DVB.TS.Tables
{
    /// <summary>
    /// Hold information about a single program in the transport stream.
    /// </summary>
    /// <remarks>
    /// Actually the table will include a couple of randomly choosen data 
    /// just to make the transport stream valid.
    /// </remarks>
    public class PMT : SITableBase
    {
        /// <summary>
        /// All streams added to this program.
        /// </summary>
        private List<short> m_Streams = new List<short>();

        /// <summary>
        /// The languages for the audio streams.
        /// </summary>
        private Dictionary<short, string> m_AudioNames = new Dictionary<short, string>();

        /// <summary>
        /// Maps each stream to the related <see cref="StreamTypes"/>.
        /// </summary>
        private Dictionary<short, StreamTypes> m_Type = new Dictionary<short, StreamTypes>();

        /// <summary>
        /// Maps each stream to the related AAC information.
        /// </summary>
        private Dictionary<short, ushort?> m_AAC = new Dictionary<short, ushort?>();

        /// <summary>
        /// Encoding if known.
        /// </summary>
        private Dictionary<short, byte> m_Encoding = new Dictionary<short, byte>();

        /// <summary>
        /// The transport stream identifier of the PCR.
        /// </summary>
        /// <remarks>
        /// Currently there is no PCR.
        /// </remarks>
        private short PCRPID = 0x1fff;

        /// <summary>
        /// Our unique program number.
        /// </summary>
        private short ProgrammNumber;

        /// <summary>
        /// Number of audio streams in this program - including <see cref="AC3Streams"/>.
        /// </summary>
        private int AudioStreams = 0;

        /// <summary>
        /// Number of teletext streams in this program - should be at most one.
        /// </summary>
        private int TextStreams = 0;

        /// <summary>
        /// DVB sub title streams in this program.
        /// </summary>
        private Dictionary<short, SubtitleInfo[]> DVBSubtitles = new Dictionary<short, SubtitleInfo[]>();

        /// <summary>
        /// Number of Dolby Digital (AC3) audio streams in this program.
        /// </summary>
        private int AC3Streams = 0;

        /// <summary>
        /// Unset to disable PCR generation.
        /// </summary>
        private bool m_AllowPCR = true;

        /// <summary>
        /// Create a new instance and bind it to the transport stream.
        /// </summary>
        /// <param name="pid">The related transport stream identifier.</param>
        /// <param name="program">The unique program number.</param>
        public PMT(short pid, short program)
            : base(pid)
        {
            // Remember
            ProgrammNumber = program;
        }

        /// <summary>
        /// A PMT has a table identifier of <i>2</i>.
        /// </summary>
        protected override byte TableIdentifier => 0x02;

        /// <summary>
        /// The optional field in the table header holds our program number.
        /// </summary>
        protected override short PrivateData => ProgrammNumber;

        /// <summary>
        /// Create a PMT for the program.
        /// </summary>
        /// <remarks>
        /// There will be a lot of randomly choosen values just to make the transport
        /// stream valid. Each audio stream will be reported as <i>german</i> and 
        /// similiar defaults are attached to a teletext stream.
        /// </remarks>
        /// <returns>The table describing the related program.</returns>
        protected override byte[] CreateTable()
        {
            // Create buffer
            TableConstructor buffer = new TableConstructor();

            // Append to buffer
            buffer.Add((byte)(0xe0 | (PCRPID / 256)));
            buffer.Add((byte)(PCRPID & 0xff));
            buffer.Add(0xf0, 0x00);

            // All entries
            for (int ip = 0; ip < m_Streams.Count;)
            {
                // Load
                var pid = m_Streams[ip++];

                var aac = m_AAC[pid];
                var type = m_Type[pid];

                // Is teletext or audio
                bool ttx = (StreamTypes.TeleText == type);
                bool sub = !ttx && (StreamTypes.SubTitles == type);
                bool ac3 = !ttx && !sub && (StreamTypes.Private == type);
                bool aud = ac3 || (!ttx && !sub && (StreamTypes.Audio == type));

                // Append to buffer
                buffer.Add((ttx || sub) ? (byte)StreamTypes.Private : aac.HasValue ? (byte)EPG.StreamTypes.AAC : m_Encoding[pid]);
                buffer.Add((byte)(0xe0 | (pid / 256)));
                buffer.Add((byte)(pid & 0xff));
                buffer.Add(0xf0);

                // Length
                int lengthPos = buffer.CreateDynamicLength();

                // Create stream identifier
                buffer.Add(new StreamIdentifier((byte)ip));

                // Check for additional data
                if (ttx)
                {
                    // Create teletext descriptor
                    buffer.Add(new Teletext());
                }
                else if (sub)
                {
                    // Load descriptor list
                    SubtitleInfo[] subInfos = DVBSubtitles[pid];

                    // Create the descriptor
                    Subtitle subDescr = new Subtitle();

                    // Check mode
                    if ((null == subInfos) || (subInfos.Length < 1))
                    {
                        // Create a brand new pseudo entry
                        subDescr.Subtitles.Add(new SubtitleInfo("deu", EPG.SubtitleTypes.DVBNormal, 1, 1));
                    }
                    else
                    {
                        // Use as is
                        subDescr.Subtitles.AddRange(subInfos);
                    }

                    // Serialize to buffer
                    buffer.Add(subDescr);
                }
                else if (aud)
                {
                    // Load language
                    string language;
                    if (!m_AudioNames.TryGetValue(pid, out language)) language = "deu";

                    // Create language descriptor
                    ISOLanguage audioDescriptor = new ISOLanguage();

                    // Append language item
                    audioDescriptor.Languages.Add(new LanguageItem(language, ac3 ? EPG.AudioTypes.Undefined : EPG.AudioTypes.CleanEffects));

                    // Append to buffer
                    buffer.Add(audioDescriptor);

                    // Fill AC3 descriptor.
                    if (ac3)
                        buffer.Add(new AC3());

                    // Fill AAC descriptor.
                    if (aac.HasValue)
                    {
                        // Split parts.
                        var profileAndLevel = (byte)aac.Value;
                        var aacType = (byte)(aac.Value >> 8);

                        // Remember descriptor.
                        buffer.Add(new AAC(profileAndLevel, aacType == 255 ? (byte?)null : aacType));
                    }
                }

                // Finish
                buffer.SetDynamicLength(lengthPos);
            }

            // Report
            return buffer.ToArray();
        }

        /// <summary>
        /// Add a stream to this program.
        /// </summary>
        /// <param name="type">Type of the stream.</param>
        /// <param name="pid">Transport stream identifier of this stream.</param>
        /// <param name="encoding">Encoding used - <i>255</i> means that the encoding is
        /// unknown or irrelevant.</param>
        /// <returns>Set if the first video stream is added and it is used as the PCR
        /// source. Currently this function always reports <i>false</i> since no
        /// PCR will be registered in the transport stream for the only program.</returns>
        public bool Add(StreamTypes type, byte encoding, short pid) => Add(type, encoding, pid, false, null);

        /// <summary>
        /// Add a stream to this program.
        /// </summary>
        /// <param name="type">Type of the stream.</param>
        /// <param name="pid">Transport stream identifier of this stream.</param>
        /// <param name="noPCR">Set when adding a video stream to disable PCR 
        /// from PTS generation.</param>
        /// <param name="encoding">Encoding used - <i>255</i> means that the encoding is
        /// unknown or irrelevant.</param>
        /// <returns>Set if the first video stream is added and it is used as the PCR
        /// source. Currently this function always reports <i>false</i> since no
        /// PCR will be registered in the transport stream for the only program.</returns>
        public bool Add(StreamTypes type, byte encoding, short pid, bool noPCR) => Add(type, encoding, pid, noPCR, null, null);

        /// <summary>
        /// Add a stream to this program.
        /// </summary>
        /// <param name="type">Type of the stream.</param>
        /// <param name="pid">Transport stream identifier of this stream.</param>
        /// <param name="noPCR">Set when adding a video stream to disable PCR 
        /// from PTS generation.</param>
        /// <param name="encoding">Encoding used - <i>255</i> means that the encoding is
        /// unknown or irrelevant.</param>
        /// <param name="info">Extended information on the contents of a subtitle stream.</param>
        /// <returns>Set if the first video stream is added and it is used as the PCR
        /// source. Currently this function always reports <i>false</i> since no
        /// PCR will be registered in the transport stream for the only program.</returns>
        public bool Add(StreamTypes type, byte encoding, short pid, bool noPCR, SubtitleInfo[] info) => Add(type, encoding, pid, noPCR, info, null);

        /// <summary>
        /// Add a stream to this program.
        /// </summary>
        /// <param name="type">Type of the stream.</param>
        /// <param name="pid">Transport stream identifier of this stream.</param>
        /// <param name="noPCR">Set when adding a video stream to disable PCR 
        /// from PTS generation.</param>
        /// <param name="encoding">Encoding used - <i>255</i> means that the encoding is
        /// unknown or irrelevant.</param>
        /// <param name="info">Extended information on the contents of a subtitle stream.</param>
        /// <param name="aac">Optional AAC profile, level and type.</param>
        /// <returns>Set if the first video stream is added and it is used as the PCR
        /// source. Currently this function always reports <i>false</i> since no
        /// PCR will be registered in the transport stream for the only program.</returns>
        public bool Add(StreamTypes type, byte encoding, short pid, bool noPCR, SubtitleInfo[] info, ushort? aac)
        {
            // Validate
            if ((pid < 0) || (pid >= 0x1fff)) throw new ArgumentOutOfRangeException("pid", pid, "only 13 bits allowed");

            // Must be unique
            if (m_Streams.Contains(pid)) throw new ArgumentException("Duplicate PID " + pid.ToString(), "pid");

            // Disable PCR generation
            if (!m_AllowPCR) noPCR = true;

            // Carries PCR
            bool isPCR = false;

            // Remember
            m_Streams.Add(pid);

            // Connect
            m_AAC[pid] = aac;
            m_Encoding[pid] = (255 == encoding) ? (byte)type : encoding;
            m_Type[pid] = type;

            // Count
            switch (type)
            {
                case StreamTypes.Audio: ++AudioStreams; break;
                case StreamTypes.Private: ++AudioStreams; ++AC3Streams; break;
                case StreamTypes.TeleText: ++TextStreams; break;
                case StreamTypes.SubTitles: DVBSubtitles[pid] = info; break;
                case StreamTypes.Video: break;
            }

            // Enforce PCR on first stream
            if (!noPCR) isPCR = (0x1fff == PCRPID);

            // Load PCR
            if (isPCR) PCRPID = pid;

            // Require new
            Changed();

            // Report
            return isPCR;
        }

        /// <summary>
        /// Associate an audio stream with a language.
        /// </summary>
        /// <param name="pid">The internal stream identifier.</param>
        /// <param name="isoName">The three-letter ISO name of the language.</param>
        public void SetAudioLanguage(short pid, string isoName)
        {
            // Remember
            if (!string.IsNullOrEmpty(isoName))
                if (!isoName.StartsWith("#"))
                    m_AudioNames[pid] = isoName;
        }

        /// <summary>
        /// Do not generate a PCR.
        /// </summary>
        public void DisablePCR()
        {
            // Do not generate a PCR
            m_AllowPCR = false;

            // Forget all
            PCRPID = 0x1fff;

            // Require new
            Changed();
        }

        /// <summary>
        /// Meldet die Anzahl der zugeordneten Datenströme.
        /// </summary>
        public int Count
        {
            get
            {
                // Report the current number of streams
                return m_Streams.Count;
            }
        }
    }
}
