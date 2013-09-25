using System;
using System.Threading;

using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DirectShow.Filters
{
    /// <summary>
    /// Mit Hilfe dieser Klasse werden <i>Elementary Streams</i> für Bild und Ton
    /// in einen Direct Show Graphen eingespielt.
    /// </summary>
    internal class ESInjector : Injector
    {
        #region Datenstromanalyse

        /// <summary>
        /// Hilfsklasse zur Analyse von Datenströmen.
        /// </summary>
        /// <remarks>
        /// Diese Klasse wird mit einer PES Analysinstanz verbunden und hat die Aufgabe,
        /// einen <see cref="ESInjector"/> mit dem ES sowie den passenden Zeitinformationen
        /// zu versorgen. Bei der Erzeugung wird die aktuelle Änderungsnummer der
        /// <see cref="ESInjector"/> Steuerinstanz festgehalten und bei der Übertragung
        /// des Datenstroms verwendet, um einen sauberen Sprung beim Wechseln von
        /// Datenströmen zu erlauben. Für jede Wechsel wird eine neue <see cref="_StreamConsumer"/>
        /// Instanz angelegt.
        /// </remarks>
        private class _StreamConsumer : TS.IStreamConsumer, IDisposable
        {
            /// <summary>
            /// Stellt die Verbindung zum Graphen her.
            /// </summary>
            private volatile ESInjector m_Master;

            /// <summary>
            /// Zugehörige Quelle für den Datenstrom.
            /// </summary>
            private volatile TS.StreamBase m_Stream;

            /// <summary>
            /// Der Änderungszähler, zu dem diese Instanz gehört.
            /// </summary>
            private int m_Version;

            /// <summary>
            /// Erzeugt einen neuen Analysator.
            /// </summary>
            /// <param name="master">Die zugehörige Instanz zur Einspielung der
            /// Pakete in den Graphen.</param>
            /// <param name="majorType">Die Hauptart des Datenstromformates.</param>
            /// <param name="minorType">Die Variante des Datenstromformates.</param>
            public _StreamConsumer( ESInjector master, Guid majorType, Guid minorType )
            {
                // Remember
                m_Master = master;

                // Get the current version
                m_Version = m_Master.CurrentVersion;

                // Stream depends on media type
                if (majorType == Constants.KSDATAFORMAT_TYPE_VIDEO)
                {
                    // Create video stream
                    if (minorType == Constants.KSDATAFORMAT_SUBTYPE_MPEG2_VIDEO)
                        m_Stream = new TS.VideoStream( this, 514, true );
                    else
                        m_Stream = new TS.HDTVStream( this, 514, true );

                    // Make sure that length in PES header is accepted
                    ((TS.VideoStream) m_Stream).AcceptAnyLength = true;
                }
                else
                {
                    // Create audio stream
                    if (minorType == Constants.KSDATAFORMAT_SUBTYPE_MPEG2_AUDIO)
                        m_Stream = new TS.AudioStream( this, 515, false );
                    else
                        m_Stream = new TS.DolbyStream( this, 515, false );
                }
            }

            /// <summary>
            /// Meldet die verwenendete virtuelle Datenstromkennung.
            /// </summary>
            public short? PID
            {
                get
                {
                    // Attach to stream
                    var stream = m_Stream;
                    if (stream == null)
                        return null;
                    else
                        return stream.PID;
                }
            }

            /// <summary>
            /// Überträgt Nutzdaten zur Analyse.
            /// </summary>
            /// <param name="version">Der Änderungszähler, zu dem die Nutzdaten gehören.</param>
            /// <param name="buffer">Der Zwischenspeicher mit den Nutzdaten.</param>
            /// <param name="start">Das erste zu benutzende Byte im Zwischenspeicher.</param>
            /// <param name="length">Die Anzahl der im Zwischenspeicher zu benutzenden Bytes.</param>
            public void AddPayload( int version, byte[] buffer, int start, int length )
            {
                // Check it
                if (version != m_Version)
                    return;

                // Attach to stream
                TS.StreamBase stream = m_Stream;

                // Report
                if (stream != null)
                    stream.AddPayload( buffer, start, length );
            }

            #region IDisposable Members

            /// <summary>
            /// Beendet die Nutzung dieser Instanz endgültig.
            /// </summary>
            public void Dispose()
            {
                // Forget stream
                using (TS.StreamBase stream = m_Stream)
                    m_Stream = null;

                // Forget master
                m_Master = null;
            }

            #endregion

            #region IStreamConsumer Members

            /// <summary>
            /// Meldet, ob bereits eine Zeitbasis vorliegt.
            /// </summary>
            bool TS.IStreamConsumer.PCRAvailable
            {
                get
                {
                    // PCR is not handled
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
                var master = m_Master;
                if (master != null)
                    master.Send( m_Version, ref counter, pid, buffer, start, packs, isFirst, sizeOfLast, pts );
            }

            /// <summary>
            /// Meldet einen Synchronisationspunkt im Datenstrom.
            /// </summary>
            /// <param name="counter">Aktueller Zähler für <i>Transport Stream</i> Pakete.</param>
            /// <param name="pid">Die Datenstromkennung.</param>
            /// <param name="pts">Die aktuelle Zeitbasis des Datenstroms.</param>
            void TS.IStreamConsumer.SendPCR( int counter, int pid, long pts )
            {
                // Forward
                var master = m_Master;
                if (master != null)
                    master.SendPCR( m_Version, counter, pid, pts );
            }

            #endregion
        }

        #endregion

        /// <summary>
        /// Der mittlerste aller möglichen Zeitstempel (2^33 / 2).
        /// </summary>
        private const long CenterPTS = 0x100000000;

        /// <summary>
        /// Ein Viertel des maximal möglichen Zeitstempels (2^33 / 4).
        /// </summary>
        private const long QuarterPTS = CenterPTS / 2;

        /// <summary>
        /// Drei Viertel des maximal möglichen Zeitstempels (2^33 - 2^33 / 4).
        /// </summary>
        private const long LastQuarterPTS = 3 * QuarterPTS;

        /// <summary>
        /// Hilfseinheit zur kontrollieren Analyse des eingehenden Datenstroms.
        /// </summary>
        private volatile _StreamConsumer m_Stream;

        /// <summary>
        /// Zugehöriger Filter.
        /// </summary>
        private TSFilter m_Filter;

        /// <summary>
        /// Zugehöriger Direct Show <see cref="Pin"/>.
        /// </summary>
        private LivePin m_Pin;

        /// <summary>
        /// Zuletzt in den Graphen übertragener Zeitstempel.
        /// </summary>
        private volatile object m_Time = null;

        /// <summary>
        /// Zuletzt ausgewerteter Zeitstempel.
        /// </summary>
        private long m_PreviousPTS = -1;

        /// <summary>
        /// Gesamtkorrektur des Zeitstempels.
        /// </summary>
        private long m_PTSOffset = 0;

        /// <summary>
        /// Gesetzt, wenn Sprünge des Zeitstempels überwacht werden sollen.
        /// </summary>
        private bool m_WatchPTSSkip = true;

        /// <summary>
        /// Letzter Zeitstempel, der in den Graphen übertragen wurde.
        /// </summary>
        private TimeSpan? m_LastPTS = null;

        /// <summary>
        /// Zuerst in den Graphen übertragener Zeitstempel.
        /// </summary>
        private long? m_FirstTime = null;

        /// <summary>
        /// Referenzzeit, als der erste Zeitstempel in den Graphen übertragen wurde.
        /// </summary>
        private long? m_FirstClock = null;

        /// <summary>
        /// Die Hauptart des Datenstromformates.
        /// </summary>
        private Guid m_MajorType;

        /// <summary>
        /// Die Unterart des Datenstromformates.
        /// </summary>
        private Guid m_MinorType;

        /// <summary>
        /// Gesetzt, wenn ein neuer Datenstrom beginnt.
        /// </summary>
        private volatile bool m_Reset = true;

        /// <summary>
        /// Gesetzt, sobald zum ersten Mal ein Zeitstempel übertragen wurde.
        /// </summary>
        private bool m_TimeSent = false;

        /// <summary>
        /// Gesetzt, wenn ein Synchronisationspunkt erkannt wurde.
        /// </summary>
        private bool m_SendPCR = false;

        /// <summary>
        /// Zählt Neustarts.
        /// </summary>
        private volatile int m_Version = Interlocked.Increment( ref m_VersionCounter );

        /// <summary>
        /// Vergibt eindeutige Versionsnummern.
        /// </summary>
        private static int m_VersionCounter;

        /// <summary>
        /// Erzeugt eine neue Einspieleinheit.
        /// </summary>
        /// <param name="filter">Der zugehörige Filter.</param>
        /// <param name="name">Der Name dieser Einheit.</param>
        /// <param name="type">Die Beschreibung des Datenstromformates.</param>
        public ESInjector( TSFilter filter, string name, MediaType type )
            : base( 1000, 8 * 1024 )
        {
            // Remember
            m_MajorType = type.MajorType;
            m_MinorType = type.SubType;
            m_Filter = filter;

            // Create the pin
            m_Pin = new LivePin( filter, name, type );

            // Connect sink to injector base
            SetSink( m_Pin.Receive );

            // Create stream
            CreateConsumer();
        }

        /// <summary>
        /// Meldet einen Anzeigetext für diese Instanz.
        /// </summary>
        /// <returns>Der gewünschte Anzeigetext.</returns>
        public override string ToString()
        {
            // Create
            return string.Format( "({0},{1})", m_MajorType, m_MinorType );
        }

        /// <summary>
        /// Erzeugt eine neue Analyseeinheit.
        /// </summary>
        private void CreateConsumer()
        {
            // Create
            m_Stream = new _StreamConsumer( this, m_MajorType, m_MinorType );
        }

        /// <summary>
        /// Überträgte PES Rohdaen in den DirectShow Graphen.
        /// </summary>
        /// <param name="buffer">Zwischenspeicher für die Daten - mit <i>null</i>
        /// wird das Ende des Datenstroms angekündigt.</param>
        /// <param name="offset">Erstes Byte im Zwischenspeicher, dass zu nutzen ist.</param>
        /// <param name="length">Anzahl der Bytes im Ziwschenspeicher, die zu nutzen sind.</param>
        public void Inject( byte[] buffer, int offset, int length )
        {
            // Forward
            Inject( m_Version, buffer, offset, length );
        }

        /// <summary>
        /// Überträgte PES Rohdaen in den DirectShow Graphen.
        /// </summary>
        /// <param name="version">Die aktuelle Version, zu der diese Daten gehören.</param>
        /// <param name="buffer">Zwischenspeicher für die Daten - mit <i>null</i>
        /// wird das Ende des Datenstroms angekündigt.</param>
        /// <param name="offset">Erstes Byte im Zwischenspeicher, dass zu nutzen ist.</param>
        /// <param name="length">Anzahl der Bytes im Ziwschenspeicher, die zu nutzen sind.</param>
        public void Inject( int version, byte[] buffer, int offset, int length )
        {
            // Attach to stream
            _StreamConsumer stream = m_Stream;

            // See if this is a shutdown request
            if (buffer == null)
                Inject( buffer, offset, length, true, null );
            else if (stream != null)
                stream.AddPayload( version, buffer, offset, length );
        }

        /// <summary>
        /// Die aktuelle Anzahl der Neustarts.
        /// </summary>
        public int CurrentVersion
        {
            get
            {
                // Report
                return m_Version;
            }
        }

        /// <summary>
        /// Meldet die <i>Direct Show</i> Schnittstelle, über die der
        /// von dieser Komponente erzeugte Datenstrom angeboten wird.
        /// </summary>
        public LivePin Pin
        {
            get
            {
                // Report
                return m_Pin;
            }
        }

        /// <summary>
        /// Meldet den relativen Versatz zwischen dem in den Graphen eingespielten
        /// und dem bereits angezeigten Material. Die Einheit entspricht <see cref="DateTime.Ticks"/>.
        /// </summary>
        public long? StreamTimeOffset
        {
            get
            {
                // Load starter items
                long? firstTime = m_FirstTime, firstClock = m_FirstClock;

                // Not yet
                if (!firstTime.HasValue)
                    return null;
                if (!firstClock.HasValue)
                    return null;

                // Attach to the filter
                TSFilter filter = m_Filter;

                // None
                if (filter == null)
                    return null;

                // Attach to the graph
                DisplayGraph graph = filter.DisplayGraph;

                // None
                if (graph == null)
                    return null;

                // Load current items
                long? time = (long?) m_Time, clock = graph.SystemClock;

                // Not possible
                if (!time.HasValue)
                    return null;
                if (!clock.HasValue)
                    return null;

                // Get deltas
                return (time.Value - firstTime.Value) - (clock.Value - firstClock.Value);
            }
        }

        /// <summary>
        /// Meldet den letzten Zeitstempel, der verwendet wurde. Gemeldet wird der vollständige
        /// Zeitstempel, der auch bei Nulldurchgängen nach 1d2h30m44s weiterzählt.
        /// </summary>
        public TimeSpan? LastPTS
        {
            get
            {
                // Report
                return m_LastPTS;
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn eine neue Speicherverwaltung im Graphen aktiv ist.
        /// </summary>
        /// <param name="allocator">Die neue Speicherverwaltung.</param>
        protected override void OnAllocatorChanged( NoMarshalComObjects.MemoryAllocator allocator )
        {
            // Base first
            base.OnAllocatorChanged( allocator );

            // Forward to pin
            if (null != m_Pin)
                m_Pin.SetMemAllocator( (allocator == null) ? IntPtr.Zero : allocator.ComInterface );
        }

        /// <summary>
        /// Beginnt neu mit der Datenstromanalyse.
        /// </summary>
        public override void Clear()
        {
            // Forward to base
            base.Clear();

            // Stop receiving data
            using (_StreamConsumer stream = m_Stream)
                m_Stream = null;

            // New version
            m_Version = Interlocked.Increment( ref m_VersionCounter );

            // Reset all on next packet
            m_Reset = true;

            // Reset receiver
            CreateConsumer();
        }

        /// <summary>
        /// Meldet die aktuelle virtuelle Datenstromkennung.
        /// </summary>
        private short? PID
        {
            get
            {
                // Attach to the stream
                _StreamConsumer stream = m_Stream;

                // Report
                if (stream == null)
                    return null;
                else
                    return stream.PID;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        public override void Dispose()
        {
            // Stop receiving data
            using (_StreamConsumer stream = m_Stream)
                m_Stream = null;

            // Forward to base
            base.Dispose();

            // Forward to pin
            if (null != m_Pin)
            {
                // Forward
                m_Pin.Dispose();

                // Forget
                m_Pin = null;
            }
        }

        #endregion

        #region IStreamConsumer Members

        /// <summary>
        /// Diese Methode nimmt ein PES Paket entgegeben und gibt es als ES Datenstrom an den Graphen
        /// weiter.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In einem DVB.NET Filtergraphen ist immer die Tonspur dafür verantwortlich, die Zeitbasis 
        /// zu lieferen. Dieser Ansatz wurde gewählt, da im Gegensatz zum Bild die Zeitstempel (PTS)
        /// der Tonspur immer fortlaufend sind - beim Bild wird zwischen DTS und PTS unterschieden und
        /// der DTS (Decoding Time Stamp), der von DVB.NET verwendet wird, orientiert sich an der
        /// physikalischen Bildstruktur (B-Frames immer nach den referenzierten I und P) im GOP 
        /// und nicht an der präsentierten.
        /// </para>
        /// <para>
        /// Da der ursprüngliche Zeitversatz der Einsangsdaten nicht bekannt ist und Bilddaten
        /// früher in den Graph geschickt werden müssen, da die Dekodierung deutlich länger 
        /// dauert, manipuliert diese Methode die dem Graphen gemeldete Zeitbasis. Zudem wird
        /// sichergestellt, dass die Zeitbasis im Graphen immer positive Werte annimmt. Dazu
        /// wird auf Basis des ersten Zeitstempels der Tonspur ein Offsetwert festgehalten,
        /// der um <see cref="TSFilter.AVDelay"/> Millisekunden vor dem ersten Zeitstempel
        /// der Tonspur liegt. Die Voreinstellung beträgt 2 Sekunden, so dass auch Datenströme
        /// korrekt wiedergegeben werden können, bei denen Bild und Ton ursprünglich um 2
        /// Sekunden versetzt sind (typisch sind 2/3 Sekunden) und deren Zeitbasis bei 0
        /// beginnt (sehr unüblich, wenn nicht manipuliert). Der Nachteil der Methode ist,
        /// dass die Darstellung von Bild und Ton nach Starten des Graphen um etwa 2 
        /// Sekunden verzögert wird.
        /// </para>
        /// <para>
        /// Die <see cref="TSFilter.LastPTS"/> Methode meldet im Allgemeinen den letzten
        /// Zeitstempel der Tonspur. Die Methode hier hat einen Mechanismus von Nulldurchgängen
        /// (2^33 / 90000 Sekunden, ungefähr 26h30m44s). Der Algorithmus ist allerdings
        /// recht einfach gehalten und wird nicht in allen Fällen funktionieren. Im
        /// ungünstigsten Fall kann es zu Rucklern oder gar zum Stillstand der Anzeige
        /// kommen.
        /// </para>
        /// <para>
        /// Passend zum ersten Zeitstempel der Tonspur wird auch die absolute Zeitbasis
        /// des Graphen vermerkt. Über <see cref="StreamTimeOffset"/> ist es dann möglich,
        /// zu dem Zeitpunkt festzustellen, welcher relative Versatz zwischen eingefütteter
        /// Bild- und Tonspur aktuell bestehlt. Wird etwa der Graph zu schnell beschickt,
        /// so kann der interne Mechanismus dafür sorgen, dass das Einspielen des Bildsignals
        /// blockiert wird. DVB.NET benutzt diese Information, um beim Abspielen von Dateien
        /// das Einspielen der Daten kurzzeitig einzustellen, bis der Graph wieder Bilddaten
        /// entgegennimmt.
        /// </para>
        /// </remarks>
        /// <param name="version">Die Änderungsnummer, zu der dieser Aufruf gehört.</param>
        /// <param name="counter">Wird ignoriert.</param>
        /// <param name="pid">Wird ignoriert.</param>
        /// <param name="buffer">Der Datenbereich, in dem das aktuelle Paket abgelegt ist.</param>
        /// <param name="start">Der Index des ersten Bytes im Datenbereich, das zu dem Paket gehört.</param>
        /// <param name="packs">Die Anzahl der <i>Transport Stream</i> Einheiten im Paket.</param>
        /// <param name="isFirst">Gesetzt, wenn dieses Paket einen PES Kopf enthält.</param>
        /// <param name="sizeOfLast">Die Anzahl der Bytes in der letzten Einheit.</param>
        /// <param name="pts">Optional der Zeitstempel zu diesem Paket.</param>
        private void Send( int version, ref int counter, int pid, byte[] buffer, int start, int packs, bool isFirst, int sizeOfLast, long pts )
        {
            // Attach to the filter
            var filter = m_Filter;
            if (filter == null)
                return;

            // Load and reset flag
            var isPCR = m_SendPCR;
            m_SendPCR = false;

            // Wrong version - ignore
            if (version != m_Version)
                return;

            // Reset all
            if (m_Reset)
            {
                // Process
                m_WatchPTSSkip = true;
                m_FirstClock = null;
                m_PreviousPTS = -1;
                m_TimeSent = false;
                m_FirstTime = null;
                m_LastPTS = null;
                m_PTSOffset = 0;
                m_Reset = false;
                m_Time = null;
            }

            // Wrong version - ignore
            if (version != m_Version)
                return;

            // Get the overall length
            int length = TS.Manager.PacketSize * (packs - 1) + sizeOfLast;

            // Strip off PES header
            if (isFirst)
            {
                // Get the header length
                int header = 9 + buffer[start + 8];

                // Adjust
                start += header;
                length -= header;
            }

            // Nothing left
            if (length < 1)
                return;

            // The presentation time
            long? time = null;

            // See if pts is present
            if (pts != -1)
            {
                #region Erkennung von PTS Nulldurchgängen nach 2^33 / 90kHz Sekunden

                // Test for correction - first PTS can not be checked
                if (m_PreviousPTS != -1)
                    if (m_WatchPTSSkip)
                    {
                        // Check for 0 transmission
                        if (m_PreviousPTS >= LastQuarterPTS) // ==> is in Q4
                            if (pts < QuarterPTS) // ==> is in Q1
                            {
                                // Adjust offset
                                m_PTSOffset += 2 * CenterPTS;

                                // Disable detection until next center transition.
                                m_WatchPTSSkip = false;
                            }
                    }
                    else if (pts >= LastQuarterPTS) // ==> is in Q4
                    {
                        // Jumped back
                        pts -= 2 * CenterPTS;
                    }
                    else
                    {
                        // Reactivate watch dog
                        if ((pts >= QuarterPTS) && (pts < CenterPTS)) // ==> is in Q2
                            m_WatchPTSSkip = true;
                    }

                #endregion

                // Remember
                m_PreviousPTS = pts;

                // Correct - gives us consequetive time line
                pts += m_PTSOffset;

                // Map to regular time - pts is 90kHz and regular .NET unit is 100nsec
                m_LastPTS = new TimeSpan( pts * 1000 / 9 );

                // Convert to number representation
                time = m_LastPTS.Value.Ticks;
            }

            // See if we are the audio line
            bool isAudio = (PID.GetValueOrDefault() == 515);

            // Get the time
            if (time.HasValue)
                time = filter.GetStreamTime( time.Value, !m_FirstTime.HasValue && isAudio );
            else
                time = null;

            // Correct againt time base line
            if (time.HasValue)
            {
                // Remember
                m_TimeSent = true;

                // Load
                m_Time = time;

                // It is the first one
                if (!m_FirstTime.HasValue)
                {
                    // Set this
                    m_FirstTime = time;

                    // And the graph time
                    m_FirstClock = filter.SystemClock;
                }
            }
            else if (!m_TimeSent)
            {
                // Make sure that first injection uses a time stamp
                return;
            }

            // Check for sync point
            bool isSync = isFirst && isAudio;

            // For debug only
            //System.Diagnostics.Debug.WriteLine( string.Format( "{0} {1} {2}@{5} pts={3} ({4})", PID, isSync, time.HasValue ? new TimeSpan( time.Value ) : TimeSpan.Zero, (pts != -1) ? new TimeSpan( pts * 1000 / 9 ) : TimeSpan.Zero, length, new TimeSpan( filter.SystemClock.GetValueOrDefault() ) ) );

            // Forward
            Inject( buffer, start, length, isSync, time );
        }

        /// <summary>
        /// Meldet einen zeitlichen Bezugspunkt im Datenstrom.
        /// </summary>
        /// <param name="version">Die Änderungsnummer, zu der dieser Aufruf gehört.</param>
        /// <param name="counter">Der aktuelle TS Paketzähler.</param>
        /// <param name="pid">Die Datenstromkennung.</param>
        /// <param name="pts">Der aktuelle Bezugspunkt.</param>
        private void SendPCR( int version, int counter, int pid, long pts )
        {
            // Skip
            if (version != m_Version)
                return;

            // Report
            //System.Diagnostics.Debug.WriteLine( string.Format( "{0} PCR", PID ) );

            // Remember
            m_SendPCR = true;
        }

        #endregion
    }
}
