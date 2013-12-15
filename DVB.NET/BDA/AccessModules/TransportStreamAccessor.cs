using System;
using System.Collections.Generic;

using JMS.DVB.EPG;
using JMS.DVB.EPG.Tables;
using JMS.DVB.EPG.Descriptors;


namespace JMS.DVB.DirectShow.AccessModules
{
    /// <summary>
    /// Im Gegensatz zur <see cref="AudioVideoAccessor"/> wird diese Klasse nicht mit
    /// PES Datenströmen sondern mit einem <i>Transport Stream</i> befüllt. Dabei
    /// wird das Format von Bild- und Ton automatisch erkannt und auf Veränderungen
    /// überwacht. Zusätzlich kann die Programmzeitschrift und der Videotext extrahiert
    /// werden.
    /// </summary>
    public abstract class TransportStreamAccessor : AudioVideoAccessor
    {
        /// <summary>
        /// Nimmt Videotext Daten entgegen.
        /// </summary>
        private class TTXStreamConsumer : TS.IStreamConsumer
        {
            /// <summary>
            /// Die zugehörige Zugriffsinstanz.
            /// </summary>
            private TransportStreamAccessor m_Accessor;

            /// <summary>
            /// Erzeugt eine neue Instanz.
            /// </summary>
            /// <param name="accessor">Die Zugriffseinheit.</param>
            public TTXStreamConsumer( TransportStreamAccessor accessor )
            {
                // Remember
                m_Accessor = accessor;
            }

            #region IStreamConsumer Members

            /// <summary>
            /// Meldet, ob bereits eine Zeitbasis vorliegt.
            /// </summary>
            bool TS.IStreamConsumer.PCRAvailable
            {
                get
                {
                    // Always there
                    return true;
                }
            }

            /// <summary>
            /// Empfängt Nutzdaten.
            /// </summary>
            /// <param name="counter">Aktueller Zähler für <i>Transport Stream</i> Pakete.</param>
            /// <param name="pid">Die Datenstromkennung.</param>
            /// <param name="buffer">Ein Zwischenspeicher, in dem das Paket abgelegt ist.</param>
            /// <param name="start">Das erste Byte im Zwischenspeicher, dass zum Paket gehört.</param>
            /// <param name="packs">Die gesamte Anzahl von <i>Transport Stream</i> Paketen.</param>
            /// <param name="isFirst">Gesetzt, wenn dieses Paket einen Paketkopf enthält.</param>
            /// <param name="sizeOfLast">Die Anzahl der Bytes im letzten <i>Transport Stream</i> Paket.</param>
            /// <param name="pts">Optional die Zeitbasis für das Paket oder <i>-1</i>.</param>
            void TS.IStreamConsumer.Send( ref int counter, int pid, byte[] buffer, int start, int packs, bool isFirst, int sizeOfLast, long pts )
            {
                // Forward
                m_Accessor.AddTeletext( isFirst, buffer, start, (packs - 1) * TS.Manager.PacketSize + sizeOfLast, pts );
            }

            /// <summary>
            /// Meldet einen Synchronisationspunkt im Datenstrom.
            /// </summary>
            /// <param name="counter">Aktueller Zähler für <i>Transport Stream</i> Pakete.</param>
            /// <param name="pid">Die Datenstromkennung.</param>
            /// <param name="pts">Die aktuelle Zeitbasis des Datenstroms.</param>
            void TS.IStreamConsumer.SendPCR( int counter, int pid, long pts )
            {
                // Completly ignore
            }

            #endregion
        }

        /// <summary>
        /// Bechreibt eine einzelne Tonspur im Transport Stream.
        /// </summary>
        protected class AudioItem
        {
            /// <summary>
            /// Name der Tonspur.
            /// </summary>
            public readonly string Name;

            /// <summary>
            /// Gesetzt, wenn es sich um eine AC3 Tonspur handelt.
            /// </summary>
            public readonly bool AC3;

            /// <summary>
            /// Stromkennung der Tonspur.
            /// </summary>
            public readonly ushort PID;

            /// <summary>
            /// Erzeugt eine neue Information für eine Tonspur.
            /// </summary>
            /// <param name="entry">Informationen zur Tonspur.</param>
            /// <param name="ac3">Gesetzt, wenn es sich um eine AC3 Tonspur handelt.</param>
            /// <param name="index">Laufender Index dieser Tonspur.</param>
            public AudioItem( ProgramEntry entry, bool ac3, int index )
            {
                // Remember
                Name = string.Format( "{0}{2} [{1}]", entry.ProgrammeName.Trim(), 1 + index, ac3 ? " (AC3)" : string.Empty );
                PID = entry.ElementaryPID;
                AC3 = ac3;
            }
        }

        /// <summary>
        /// Signatur zur Benachrichtigung eines Clients über Veränderungen am eingehenden
        /// Datenstrom.
        /// </summary>
        /// <param name="restartGraph"></param>
        public delegate void StreamChangedHandler( bool restartGraph );

        /// <summary>
        /// Wird aktiviert, wenn der Direct Show Graph neu aufgebaut werden muss.
        /// </summary>
        public event StreamChangedHandler StreamChanged;

        /// <summary>
        /// Analysiert den eingehenden Transport Stream.
        /// </summary>
        private TS.TSParser m_TSParser = new TS.TSParser();

        /// <summary>
        /// Alle bekannten Namen von Audiokanälen.
        /// </summary>
        private volatile string[] m_AudioNames = { };

        /// <summary>
        /// Analyisert die <i>Program Association Table</i>. Es wird erwartet,
        /// dass der eingehende Transport Stream nur ein Programm enthält.
        /// </summary>
        private Parser m_PATParser = null;

        /// <summary>
        /// Analysiert die Programmzeitschrift.
        /// </summary>
        private EPG.Parser m_EPGParser = null;

        /// <summary>
        /// Analyisiert die <i>Program Map Table</i>. Es wird erwartet,
        /// dass der eingehende Transport Stream nur ein Programm enthält.
        /// </summary>
        private Parser m_PMTParser = null;

        /// <summary>
        /// Aktuelles Programm aus dem eingehenden Transport Stream.
        /// </summary>
        private ushort m_CurrentPMT = 0;

        /// <summary>
        /// Aktuelle Tonspur, die von eingehenden Transport Stream in den DirectShow
        /// Graphen durchgereicht werden soll.
        /// </summary>
        public int AudioIndex { get; set; }

        /// <summary>
        /// Aktueller Bilddatenstrom im eingehenden Transport Stream.
        /// </summary>
        private ushort m_Video = 0;

        /// <summary>
        /// Gesetzt, wenn das Bildsignal MPEG-4 ist.
        /// </summary>
        private bool m_MPEG4 = false;

        /// <summary>
        /// Aktueller Tondatenstrom im eingehenden Transport Stream.
        /// </summary>
        private ushort m_Audio = 0;

        /// <summary>
        /// Aktueller Videotext Strom im eingehenden Transport Stream.
        /// </summary>
        private ushort m_TTX = 0;

        /// <summary>
        /// Liest oder setzt die Informationen zur laufenden Aufzeichnung.
        /// </summary>
        public EventEntry CurrentEntry { get; set; }

        /// <summary>
        /// Liest oder setzt die Informationen zur nächsten Aufzeichnung.
        /// </summary>
        public EventEntry NextEntry { get; set; }

        /// <summary>
        /// Filter für die Programmzeitschrift.
        /// </summary>
        private ushort? m_CurrentService = null;

        /// <summary>
        /// Erzwingt einen Neustart des Graphen, selbst wenn die PIDs von Bild und
        /// Ton nicht verändert wurden.
        /// </summary>
        private bool m_RestartAV = true;

        /// <summary>
        /// Gesetzt, wenn das eingehende Tonsignal AC3 ist.
        /// </summary>
        private bool m_AC3 = false;

        /// <summary>
        /// Analyseeinheit für die Videotext Daten.
        /// </summary>
        private TTXStreamConsumer m_TTXConsumer;

        /// <summary>
        /// Datenstromumwandler für Videotext Daten.
        /// </summary>
        private TS.TTXStream m_TTXStream;

        /// <summary>
        /// Signatur einer Methode zur Analyse eines Videotext PES Stroms.
        /// </summary>
        /// <param name="isFirst">Gesetzt, wenn die Daten mit dem PES Kopf beginnen.</param>
        /// <param name="data">Zwischenspeicher für Videotext Rohdaten.</param>
        /// <param name="offset">Erstes Byte des aktuellen Paketes.</param>
        /// <param name="length">Größe des aktuellen Paketes in Bytes.</param>
        /// <param name="pts">Der Zeitstempel zum aktuellen Paket.</param>
        public delegate void VideotextRawDataHandler( bool isFirst, byte[] data, int offset, int length, long pts );

        /// <summary>
        /// Optionaler Empfänger für Videotextdaten.
        /// </summary>
        public VideotextRawDataHandler OnVideotextData;

        /// <summary>
        /// Initialisiert eine Instanz.
        /// </summary>
        protected TransportStreamAccessor()
        {
            // Analyseeinheit erzeugen
            m_TTXConsumer = new TTXStreamConsumer( this );

            // Videotext PES Analysator erzeugen
            m_TTXStream = new TS.TTXStream( m_TTXConsumer, 0, false );

            // Install parser
            m_PATParser = new Parser();

            // Register receiver
            m_PATParser.SectionFound += ProcessPAT;

            // Connect to parser
            m_TSParser.SetFilter( 0, true, m_PATParser.OnData );

            // Install EPG parser
            m_EPGParser = new Parser();

            // Register receiver
            m_EPGParser.SectionFound += ProcessEPG;

            // Connect to parser
            m_TSParser.SetFilter( 0x12, true, m_EPGParser.OnData );
        }

        /// <summary>
        /// Beendet alle Aktivitäten dieses Zugriffsmoduls.
        /// </summary>
        protected override void OnDispose()
        {
            // Parser
            using (TS.TSParser parser = m_TSParser)
                m_TSParser = null;

            // Forward
            base.OnDispose();
        }

        /// <summary>
        /// Leer die Zwischenspeicher.
        /// </summary>
        public override void ClearBuffers()
        {
            // Names
            m_AudioNames = new string[0];

            // To base
            base.ClearBuffers();
        }

        /// <summary>
        /// Prüft, ob in der Programmzeitschrift Daten zur aktuellen Aufzeichnung vorliegen.
        /// </summary>
        /// <param name="section">Eintrag aus der Programmzeitschrift.</param>
        private void ProcessEPG( Section section )
        {
            // Not valid
            if (null == section)
                return;
            if (!section.IsValid)
                return;

            // Get the table
            EIT eit = section.Table as EIT;

            // Not valid
            if (null == eit)
                return;
            if (!eit.IsValid)
                return;

            // See if filter is active
            int? service = m_CurrentService;

            // Only current events are of interest
            if (!service.HasValue)
                return;
            if (service.Value != eit.ServiceIdentifier)
                return;

            // Set flags
            bool gotCurrent = false, gotNext = false;

            // Find all
            foreach (EventEntry entry in eit.Entries)
            {
                // Check for current
                if (!gotCurrent)
                    if (EventStatus.Running == entry.Status)
                    {
                        // Remember
                        CurrentEntry = entry;
                        gotCurrent = true;

                        // Done
                        if (gotNext)
                            break;
                        else
                            continue;
                    }

                // Check for next
                if (!gotNext)
                    if (EventStatus.NotRunning == entry.Status)
                    {
                        // Remember
                        NextEntry = entry;
                        gotNext = true;

                        // Done
                        if (gotCurrent)
                            break;
                    }
            }
        }

        /// <summary>
        /// Versucht, aus der aktuellen <i>Program Access Table</i> das Programm des
        /// eingehenden Transport Streams zu ermitteln.
        /// </summary>
        /// <remarks>
        /// Nur das erste Programm wird berücksichtigt.
        /// </remarks>
        /// <param name="pat">Eine SI Tabelle.</param>
        private void ProcessPAT( Section pat )
        {
            // Not active
            if (!IsRunning)
                return;

            // Validate
            if (!pat.IsValid)
                return;

            // Attach to table
            PAT table = pat.Table as PAT;

            // Validate
            if (null == table)
                return;
            if (!table.IsValid)
                return;
            if (null == table.ProgramIdentifier)
                return;

            // Get the first
            IEnumerator<KeyValuePair<ushort, ushort>> programEnum = table.ProgramIdentifier.GetEnumerator();
            if (!programEnum.MoveNext())
                return;

            // Compare current PMT
            if (m_CurrentPMT == programEnum.Current.Value)
                return;

            // Remove the audio names
            m_AudioNames = new string[0];

            // Stop current PMT
            if (0 != m_CurrentPMT)
                m_TSParser.RemoveFilter( m_CurrentPMT );

            // Re-create parser
            m_PMTParser = new Parser();

            // Connect to handler
            m_PMTParser.SectionFound += ProcessPMT;

            // Change
            m_CurrentPMT = programEnum.Current.Value;
            m_CurrentService = null;
            CurrentEntry = null;
            NextEntry = null;

            // Create PMT
            m_TSParser.SetFilter( m_CurrentPMT, true, m_PMTParser.OnData );
        }

        /// <summary>
        /// Ermittelt aus der aktuellen <i>Program Map Table</i> die Informationen zum
        /// Porgramm des eingehenden Transport Streams. 
        /// </summary>
        /// <remarks>
        /// Dabei wird das Bild- und das gewünschte Tonsignal ermittelt und in den Transport
        /// Stream für den DirectShow Graphen übertragen. Dieser kann so mit festen PIDs
        /// arbeiten, wodurch die weitere Handhabung im DVB.NET DirectShow Graphen vereinfacht wird.
        /// Man kann die Arbeit dieser Methode daher als Auswahl der Tonspur und PID Mapping
        /// beschreiben.
        /// </remarks>
        /// <param name="pmt">Eine SI Tabelle.</param>
        private void ProcessPMT( Section pmt )
        {
            // Not active
            if (!IsRunning)
                return;

            // Validate
            if (!pmt.IsValid)
                return;

            // Attach to table
            PMT table = pmt.Table as PMT;

            // Validate
            if (null == table)
                return;
            if (!table.IsValid)
                return;
            if (null == table.ProgramEntries)
                return;
            if (table.ProgramEntries.Length < 1)
                return;

            // Remember
            m_CurrentService = table.ProgramNumber;

            // All audio
            List<AudioItem> audios = new List<AudioItem>();

            // Video and audio
            ushort video = 0, audio = 0, ttx = 0;
            bool mpeg4 = false, ac3 = false;

            // Process all programs
            foreach (ProgramEntry program in table.ProgramEntries)
                if ((program.StreamType == StreamTypes.Audio11172) || (program.StreamType == StreamTypes.Audio13818))
                {
                    // Remember MP2
                    audios.Add( new AudioItem( program, false, audios.Count ) );
                }
                else if ((program.StreamType == StreamTypes.Video13818) || (program.StreamType == StreamTypes.H264))
                {
                    // Only first video
                    if (0 != video)
                        continue;

                    // Remember
                    mpeg4 = (StreamTypes.H264 == program.StreamType);
                    video = program.ElementaryPID;
                }
                else if (program.StreamType == StreamTypes.PrivateData)
                {
                    // Modes
                    bool ac3Found = false, ttxFound = false;

                    // Scan descriptors
                    foreach (Descriptor descriptor in program.Descriptors)
                        if (!ac3Found && (descriptor is AC3))
                        {
                            // Add to list
                            audios.Add( new AudioItem( program, true, audios.Count ) );

                            // Done
                            ac3Found = true;
                        }
                        else if (!ttxFound && (descriptor is Teletext))
                        {
                            // Remember
                            ttx = program.ElementaryPID;

                            // Done
                            ttxFound = true;
                        }
                }

            // Always update name list
            m_AudioNames = audios.ConvertAll<string>( item => item.Name ).ToArray();

            // Read the audio index
            int audioIndex = AudioIndex;

            // Reset to default
            if ((audioIndex < 0) || (audioIndex >= audios.Count)) audioIndex = 0;

            // Check result
            if (audioIndex < audios.Count)
            {
                // Load item
                AudioItem item = audios[AudioIndex];

                // Copy all
                audio = item.PID;
                ac3 = item.AC3;
            }

            // Check changes
            bool videoDecChanged = (mpeg4 != m_MPEG4) || m_RestartAV;
            bool videoChanged = (video != m_Video) || m_RestartAV;
            bool audioChanged = (audio != m_Audio) || m_RestartAV;
            bool audioDecChanged = (ac3 != m_AC3) || m_RestartAV;
            bool ttxChanged = (ttx != m_TTX) || m_RestartAV;

            // No change at all
            if (!videoChanged)
                if (!videoDecChanged)
                    if (!audioChanged)
                        if (!audioDecChanged)
                            if (!ttxChanged)
                                return;

            // Discard old
            if (null != m_TSParser)
            {
                // De-register
                if (videoChanged && (0 != m_Video))
                    m_TSParser.RemoveFilter( m_Video );
                if (audioChanged && (0 != m_Audio))
                    m_TSParser.RemoveFilter( m_Audio );
                if (ttxChanged && (0 != m_TTX))
                    m_TSParser.RemoveFilter( m_TTX );
            }

            // Check for enforcement
            bool first = ((0 == m_Video) && (0 == m_Audio)) || m_RestartAV;

            // Change
            AudioIndex = audioIndex;
            m_RestartAV = false;
            m_Video = video;
            m_Audio = audio;
            m_MPEG4 = mpeg4;
            m_AC3 = ac3;
            m_TTX = ttx;

            // Register new
            if (null != m_TSParser)
            {
                // De-register
                if (videoChanged && (0 != m_Video))
                    m_TSParser.SetFilter( m_Video, false, AddVideo );
                if (audioChanged && (0 != m_Audio))
                    m_TSParser.SetFilter( m_Audio, false, AddAudio );
                if (ttxChanged && (0 != m_TTX))
                    m_TSParser.SetFilter( m_TTX, false, m_TTXStream.AddPayload );
            }

            // Attach to client
            var streamChanged = StreamChanged;
            if (streamChanged != null)
                streamChanged( first || videoDecChanged || audioDecChanged );
        }

        /// <summary>
        /// Nimmt Videotext Daten entgegen.
        /// </summary>
        /// <param name="isFirst">Gesetzt, wenn die Daten mit dem PES Kopf beginnen.</param>
        /// <param name="data">Zwischenspeicher für Videotext Rohdaten.</param>
        /// <param name="offset">Erstes Byte des aktuellen Paketes.</param>
        /// <param name="length">Größe des aktuellen Paketes in Bytes.</param>
        /// <param name="pts">Der aktuelle Zeitstempel zu diesem Paket.</param>
        private void AddTeletext( bool isFirst, byte[] data, int offset, int length, long pts )
        {
            // Load current callback
            VideotextRawDataHandler handler = OnVideotextData;

            // Dispatch
            if (handler != null)
                handler( isFirst, data, offset, length, pts );
        }

        /// <summary>
        /// Aktiviert die Übertragung der Daten in den Direct Show Graphen.
        /// </summary>
        public void SetDecoder()
        {
            // Prepare graph
            StartGraph( m_MPEG4, m_AC3 );
        }

        /// <summary>
        /// Überträgt keine Eingangsdaten mehr in den Direct Show Graphen.
        /// </summary>
        public override void Stop()
        {
            // Forward
            base.Stop();

            // Make sure that graph is restarted
            m_RestartAV = true;
        }

        /// <summary>
        /// Meldet die Namen der Tonspuren in dieser Datei.
        /// </summary>
        /// <returns></returns>
        public string[] GetAudioNames()
        {
            // Report
            return m_AudioNames;
        }

        /// <summary>
        /// Überträgt Daten zur Verarbeitung an den Transport Stream Parser.
        /// </summary>
        /// <param name="buffer">Zwischenspeicher.</param>
        /// <param name="offset">Erstes zu übertragendes Byte.</param>
        /// <param name="length">Anzahl der zu übertragenden Bytes.</param>
        /// <returns>Gesetzt, wenn die Daten übertragen wurden.</returns>
        protected bool AddPayload( byte[] buffer, int offset, int length )
        {
            // Already shut down
            if (null == m_TSParser)
                return false;

            // Push into pipe
            m_TSParser.AddPayload( buffer, offset, length );

            // Processed
            return true;
        }

        /// <summary>
        /// Meldet, ob ein Videotext Signal vorliegt.
        /// </summary>
        public bool TTXAvailable
        {
            get
            {
                // Report
                return (m_TTX != 0);
            }
        }
    }
}
