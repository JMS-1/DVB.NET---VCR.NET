using System;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// Mit dieser Klasse werden die SI Informationen zu einem Sender auf
    /// Veränderungen überwacht.
    /// </summary>
    public class PSIWatchDog : IDisposable
    {
        /// <summary>
        /// Signature für eine Methode, die über Veränderungen an einem Sender
        /// informiert wird.
        /// </summary>
        /// <param name="oldStation">Alte Senderdaten.</param>
        /// <param name="newStation">Neue Senderdaten.</param>
        public delegate void StationChangedHandler( Station oldStation, Station newStation );

        /// <summary>
        /// Überprüft nur das oberste Bit der Tabellenkennung.
        /// </summary>
        private static byte[] m_AllMask = { 0x80 };

        /// <summary>
        /// Akzeptiert alle Tabellen, deren oberstes Bit nicht gesetzt ist.
        /// </summary>
        private static byte[] m_AllData = { 0x00 };

        /// <summary>
        /// Das verwendete DVB.NET Gerät.
        /// </summary>
        private IDeviceProvider m_Device = null;

        /// <summary>
        /// Die überwachten Sender.
        /// </summary>
        private Dictionary<ushort, Station> m_Stations = new Dictionary<ushort, Station>();

        /// <summary>
        /// Die Helfer für die einzelnen PMTs.
        /// </summary>
        private Dictionary<ushort, Parser> m_Parsers = new Dictionary<ushort, Parser>();

        /// <summary>
        /// Der Helfer für die PAT.
        /// </summary>
        private Parser m_Parser = new Parser();

        /// <summary>
        /// Einklinkpunkt für Benachrichtigungen über Veränderungen an einem Sender.
        /// </summary>
        public event StationChangedHandler OnStationChanged;

        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        /// <param name="device">Das zu verwendende DVB.NET Gerät.</param>
        public PSIWatchDog( IDeviceProvider device )
        {
            // Remember
            m_Device = device;

            // Connect section output to analysis method
            m_Parser.SectionFound += AnalysePAT;
        }

        /// <summary>
        /// Trägt einen Sender zur Überwachung ein.
        /// </summary>
        /// <param name="station">Der zu überwachende Sender.</param>
        public void Add( Station station )
        {
            // Update
            lock (m_Stations) m_Stations[station.ServiceIdentifier] = station;
        }

        /// <summary>
        /// Wertet eine PAT aus.
        /// </summary>
        /// <param name="section">Die aktuelle PAT.</param>
        private void AnalysePAT( Section section )
        {
            // Already done
            if (null == m_Parser) return;

            // Verifiy section
            if ((null == section) || !section.IsValid) return;

            // Expect a PAT
            var pat = section.Table as Tables.PAT;

            // Verify table
            if ((null == pat) || !pat.IsValid) return;

            // Check all programs
            foreach (var service in pat.ProgramIdentifier)
                lock (m_Stations)
                {
                    // Load the station
                    Station station;
                    if (!m_Stations.TryGetValue( service.Key, out station )) continue;

                    // Explicitly deactivated
                    if (null == station) continue;

                    // Found the PMT for that station
                    if (m_Parsers.ContainsKey( service.Value )) continue;

                    // Create a new parser
                    var pmt = new Parser();

                    // Connect section notification
                    pmt.SectionFound += AnalysePMT;

                    // Remember
                    m_Parsers[service.Value] = pmt;

                    // Activate
                    m_Device.StartSectionFilter( service.Value, pmt.OnData, m_AllData, m_AllMask );
                }
        }

        /// <summary>
        /// Wertet eine PMT aus.
        /// </summary>
        /// <param name="section">Die aktuelle PAT.</param>
        private void AnalysePMT( Section section )
        {
            // Already done
            if (null == m_Parser) return;

            // Verifiy section
            if ((null == section) || !section.IsValid) return;

            // Expect a PMT
            Tables.PMT pmt = section.Table as Tables.PMT;

            // Verify table
            if ((null == pmt) || !pmt.IsValid) return;

            // The original station
            Station station;

            // Get the related station
            lock (m_Stations)
            {
                // Load
                if (!m_Stations.TryGetValue( pmt.ProgramNumber, out station )) return;

                // Explicitly disabled
                if (null == station) return;
            }

            // Create station from SI table
            Station newStation = pmt.CreateStation();

            // Compare
            if (station.VideoPID == newStation.VideoPID)
                if (station.VideoType == newStation.VideoType)
                    if (station.Encrypted == newStation.Encrypted)
                        if (station.TTXPID == newStation.TTXPID)
                            if (station.PCRPID == newStation.PCRPID)
                            {
                                // All audio
                                AudioInfo[] audios = station.AudioMap, newAudios = newStation.AudioMap;
                                if (audios.Length == newAudios.Length)
                                {
                                    // Index
                                    int ix = audios.Length;

                                    // Test
                                    while (ix-- > 0)
                                    {
                                        // Load
                                        AudioInfo audio = audios[ix], newAudio = newAudios[ix];

                                        // Compare PID and format only but ignore language
                                        if (audio.PID != newAudio.PID) break;
                                        if (audio.AC3 != newAudio.AC3) break;
                                    }

                                    // All subtitles
                                    if (ix < 0)
                                        if (station.DVBSubtitles.Count == newStation.DVBSubtitles.Count)
                                            for (ix = station.DVBSubtitles.Count; ix-- > 0; )
                                            {
                                                // Load to compare
                                                DVBSubtitleInfo sub = station.DVBSubtitles[ix], newSub = newStation.DVBSubtitles[ix];

                                                // For performance reasons compare PID only
                                                if (sub.PID != newSub.PID) break;
                                            }

                                    // All are equal
                                    if (ix < 0) return;
                                }
                            }

            // Adapt other settings
            newStation.TransportStreamIdentifier = station.TransportStreamIdentifier;
            newStation.NetworkIdentifier = station.NetworkIdentifier;
            newStation.CustomSettings.AddRange( station.CustomSettings );
            newStation.TransponderName = station.TransponderName;
            newStation.Name = station.Name;

            // Update to use new station
            Add( newStation );

            // Load handler
            StationChangedHandler callbacks = OnStationChanged;

            // Activate
            if (null != callbacks) callbacks( station, newStation );
        }

        /// <summary>
        /// Aktiviert die Überwachung der PAT eines DVB.NET Gerätes.
        /// </summary>
        public void Start()
        {
            // Connect parser to device
            m_Device.StartSectionFilter( 0x00, m_Parser.OnData, m_AllData, m_AllMask );
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Überwachung.
        /// </summary>
        public void Dispose()
        {
            // Cleanup
            lock (m_Stations)
            {
                // All
                m_Stations.Clear();
                m_Parsers.Clear();
                m_Parser = null;
            }
        }

        #endregion
    }
}
