using System;
using JMS.DVB;
using System.IO;
using System.Xml;
using System.Text;
using JMS.DVB.EPG;
using JMS.DVB.Satellite;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.ChannelManagement
{
    /// <summary>
    /// A complete receiver configuration usful for scanning.
    /// </summary>
    [Serializable]
    public class ReceiverConfiguration
    {
        private class DefaultReporter : Postprocessing.IPostprocessingReport, IDisposable
        {
            private class FinishStation : IDisposable
            {
                private DefaultReporter m_Reporter;

                public FinishStation(DefaultReporter reporter)
                {
                    // Remember
                    m_Reporter = reporter;
                }

                #region IDisposable Members

                public void Dispose()
                {
                    // Forward
                    if (null != m_Reporter)
                    {
                        // Send
                        m_Reporter.m_Station = null;

                        // Forget
                        m_Reporter = null;
                    }
                }

                #endregion
            }

            private StringBuilder m_Buffer = new StringBuilder();
            private Station m_Station = null;

            public DefaultReporter()
            {
            }

            public override string ToString()
            {
                // Report
                return m_Buffer.ToString();
            }

            #region IDisposable Members

            public void Dispose()
            {
            }

            #endregion

            #region IPostprocessingReport Members

            public IDisposable BeginUpdate(Station station)
            {
                // Remember
                m_Station = station;

                // Create cleanup helper
                return new FinishStation(this);
            }

            public void Report(Postprocessing.PostprocessingReportModes mode, string format, params object[] args)
            {
                // Test mode
                if ((null == args) || (args.Length < 1))
                    Report(mode, "{0}", format);
                else if (null == m_Station)
                    m_Buffer.AppendFormat("{0} {1} {2}\n", DateTime.Now, mode, string.Format(format, args));
                else
                    m_Buffer.AppendFormat("{0} {1} {2}={3} {4}\n", DateTime.Now, mode, m_Station, m_Station.FullName, string.Format(format, args));
            }

            #endregion
        }

        /// <summary>
        /// Eine spezielle Transponderklasse zur Markierung der Aktualisierungskarte.
        /// </summary>
        private class DisableOnlyMarker : Transponder
        {
            /// <summary>
            /// Die einzige Instanz dieser Klasse.
            /// </summary>
            public static readonly DisableOnlyMarker Instance = new DisableOnlyMarker();

            /// <summary>
            /// Erzeugt eine neue Instanz.
            /// </summary>
            private DisableOnlyMarker()
            {
            }

            /// <summary>
            /// Gibt eine Beschreibung aus.
            /// </summary>
            /// <param name="format">Das Format wird ignoriert.</param>
            /// <returns>Das Ergebnis ist immer die leere Zeichenkette.</returns>
            public override string ToString(string format)
            {
                // Report
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public const string CurrentSchemaVersion = "3.1";

        /// <summary>
        /// 
        /// </summary>
        public static string ScanConfiguration = null;

        /// <summary>
        /// Inform client on LNB currently scanned.
        /// </summary>
        /// <param name="lnb">The LNB to be scanned.</param>
        /// <returns>Set to continue scanning.</returns>
        public delegate bool LNBInformationHandler(DiSEqC lnb);

        /// <summary>
        /// Inform client on transponder currently scanned.
        /// </summary>
        /// <param name="transponder">The transponder to be scanned.</param>
        /// <returns>Set to continue scanning.</returns>
        public delegate bool TransponderInformationHandler(Transponder transponder);

        /// <summary>
        /// Inform client on a station successfully scanned.
        /// </summary>
        /// <param name="station">The station found.</param>
        /// <returns>Set to continue scanning.</returns>
        public delegate bool StationFoundHandler(Station station);

        /// <summary>
        /// The cached configuration file holding all transponder settings.
        /// </summary>
        [XmlIgnore]
        private ConfigurationFile.ConfigurationCollection m_Configuration = null;

        /// <summary>
        /// The type of the DVB source.
        /// </summary>
        public FrontendType FrontendType = FrontendType.Unknown;

        /// <summary>
        /// The version of the schema.
        /// </summary>
        public string SchemaVersion = null;

        /// <summary>
        /// The DiSEqC configuration.
        /// </summary>
        [XmlIgnore]
        private List<DiSEqCItem> m_LNBSettings = new List<DiSEqCItem>();

        /// <summary>
        /// The related channel manager.
        /// </summary>
        [XmlIgnore]
        private ChannelManager m_Channels;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        public ReceiverConfiguration()
        {
            // Create channel manager
            m_Channels = new ChannelManager(null);

            // Fill
            foreach (DiSEqC lnb in m_Channels.DiSEqCConfiguration)
            {
                // Remember
                m_LNBSettings.Add(new DiSEqCItem(lnb));
            }
        }

        /// <summary>
        /// Create a new instance with the current schema attached.
        /// </summary>
        /// <returns>New configuration instance.</returns>
        public static ReceiverConfiguration CreateNew()
        {
            // Create
            ReceiverConfiguration config = new ReceiverConfiguration();

            // Set version
            config.SchemaVersion = CurrentSchemaVersion;

            // Report
            return config;
        }

        /// <summary>
        /// The DiSEqC configuration.
        /// </summary>
        [XmlElement("Receiver")]
        public DiSEqCItem[] Receivers
        {
            get
            {
                // Report
                return m_LNBSettings.ToArray();
            }
            set
            {
                // Test
                if (null == value) throw new ArgumentNullException("value");
                if (4 != value.Length) throw new ArgumentOutOfRangeException("value");

                // Reset
                m_LNBSettings = new List<DiSEqCItem>();

                // Change
                if (null != value) m_LNBSettings.AddRange(value);

                // Connect
                SyncDiSEqC();
            }
        }

        /// <summary>
        /// Synchronize channel list and DiSEqC configuration.
        /// </summary>
        private void SyncDiSEqC()
        {
            // Not possible
            if ((null == m_Channels) || (null == m_LNBSettings)) return;

            // One by one
            m_Channels.DiSEqCConfiguration[0] = (m_LNBSettings.Count > 0) ? m_LNBSettings[0].LNB : new DiSEqCNone();
            m_Channels.DiSEqCConfiguration[1] = (m_LNBSettings.Count > 1) ? m_LNBSettings[1].LNB : new DiSEqCNone();
            m_Channels.DiSEqCConfiguration[2] = (m_LNBSettings.Count > 2) ? m_LNBSettings[2].LNB : new DiSEqCNone();
            m_Channels.DiSEqCConfiguration[3] = (m_LNBSettings.Count > 3) ? m_LNBSettings[3].LNB : new DiSEqCNone();
        }

        /// <summary>
        /// The channel list.
        /// </summary>
        public ChannelManager Channels
        {
            get
            {
                // Report
                return m_Channels;
            }
            set
            {
                // Change
                m_Channels = value;

                // Connect
                SyncDiSEqC();
            }
        }

        /// <summary>
        /// Serialize the configuration to a stream.
        /// </summary>
        /// <param name="stream">Target stream.</param>
        public void Save(Stream stream)
        {
            // Create serializer
            XmlSerializer serializer = new XmlSerializer(GetType(), "http://jochen-manns.de/DVB.NET/Receivers");

            // Create settings
            XmlWriterSettings settings = new XmlWriterSettings();

            // Configure settings
            settings.Encoding = Encoding.Unicode;
            settings.Indent = true;

            // Create writer and process
            using (XmlWriter writer = XmlWriter.Create(stream, settings)) serializer.Serialize(writer, this);
        }

        /// <summary>
        /// Serialize the configuration to a file.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="mode">Mode to open the file.</param>
        public void Save(string path, FileMode mode)
        {
            // Forward
            using (FileStream stream = new FileStream(path, mode, FileAccess.Write, FileShare.None)) Save(stream);
        }

        /// <summary>
        /// Load a configuration from a file.
        /// </summary>
        /// <param name="path">Full path to the file.</param>
        /// <returns>The reconstructed configuration instance.</returns>
        public static ReceiverConfiguration Load(string path)
        {
            // Forward
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) return Load(stream);
        }

        /// <summary>
        /// Load a configuration from a stream.
        /// </summary>
        /// <param name="stream">The stream holding the instance.</param>
        /// <returns>The reconstructed configuration instance.</returns>
        public static ReceiverConfiguration Load(Stream stream)
        {
            // Create deserializer
            XmlSerializer deserializer = new XmlSerializer(typeof(ReceiverConfiguration), "http://jochen-manns.de/DVB.NET/Receivers");

            // Process
            return (ReceiverConfiguration)deserializer.Deserialize(stream);
        }

        /// <summary>
        /// 
        /// </summary>
        public ConfigurationMap ConfigurationMap
        {
            get
            {
                // Load file
                if (null == m_Configuration) m_Configuration = new ConfigurationFile.ConfigurationCollection();

                // Create map
                ConfigurationMap map = new ConfigurationMap();

                // Check type
                switch (FrontendType)
                {
                    case FrontendType.Satellite: map.Load(m_Configuration.DVBSReceivers); break;
                    case FrontendType.Cable: map.Load(m_Configuration.DVBCReceivers); break;
                    case FrontendType.Terrestrial: map.Load(m_Configuration.DVBTReceivers); break;
                }

                // Report - empty for unsupported frontend types
                return map;
            }
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ReceiverConfiguration Clone()
        {
            // Create stream
            using (MemoryStream stream = new MemoryStream())
            {
                // Copy in
                Save(stream);

                // Reset
                stream.Seek(0, SeekOrigin.Begin);

                // Recreate
                return Load(stream);
            }
        }
    }
}
