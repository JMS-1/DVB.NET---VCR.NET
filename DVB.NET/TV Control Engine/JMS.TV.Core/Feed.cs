using System;
using System.Collections.Generic;
using JMS.DVB;


namespace JMS.TV.Core
{
    /// <summary>
    /// Beschreibt einen einzelnen Sender - in der ersten Version wird es nur Fernsehsender
    /// geben.
    /// </summary>
    internal class Feed : IFeed
    {
        /// <summary>
        /// Gesetzt, wenn dieser Sender gerade vollständig angezeigt wird - Bild, Ton und Videotext.
        /// </summary>
        private bool m_primaryView;

        /// <summary>
        /// Gesetzt, wenn dieser Sender gerade vollständig angezeigt wird - Bild, Ton und Videotext.
        /// </summary>
        public bool IsPrimaryView
        {
            get
            {
                return m_primaryView;
            }
            set
            {
                // Must validate
                if (value && m_secondaryView)
                    throw new InvalidOperationException( "Sender kann nicht gleichzeitig primär und sekundär angezeigt werden" );
                else
                    m_primaryView = value;
            }
        }

        /// <summary>
        /// Gesetzt, wenn dieser Sender gerade als Bild-In-Bild (PiP) angezeigt wird.
        /// </summary>
        private bool m_secondaryView;

        /// <summary>
        /// Gesetzt, wenn dieser Sender gerade als Bild-In-Bild (PiP) angezeigt wird.
        /// </summary>
        public bool IsSecondaryView
        {
            get
            {
                return m_secondaryView;
            }
            set
            {
                // Must validate
                if (value && m_primaryView)
                    throw new InvalidOperationException( "Sender kann nicht gleichzeitig primär und sekundär angezeigt werden" );
                else
                    m_secondaryView = value;
            }
        }

        /// <summary>
        /// Die zugehörige Quelle.
        /// </summary>
        public SourceSelection Source { get; private set; }

        /// <summary>
        /// Alle Aufzeichnungen auf diesem Sender.
        /// </summary>
        private readonly HashSet<string> m_activeRecordings = new HashSet<string>( StringComparer.InvariantCultureIgnoreCase );

        /// <summary>
        /// Gesetzt, wenn dieser Sender benutzt wird.
        /// </summary>
        public bool IsActive { get { return m_primaryView || m_secondaryView || (m_activeRecordings.Count > 0); } }

        /// <summary>
        /// Gesetzt, wenn dieser Sender bei Bedarf abgeschaltet werden darf.
        /// </summary>
        public bool ReusePossible { get { return !m_primaryView && (m_activeRecordings.Count < 1); } }

        /// <summary>
        /// Meldet alle aktiven Aufzeichnungen für diesen Sender.
        /// </summary>
        public IEnumerable<string> Recordings { get { return m_activeRecordings; } }

        /// <summary>
        /// Prüft, ob eine bestimmte Aufzeichnung bereits aktiv ist.
        /// </summary>
        /// <param name="key">Die Identifikation der Aufzeichnung.</param>
        /// <returns>Gesetzt, wenn die angegebene Aufzeichnung aktiv ist.</returns>
        public bool IsRecording( string key ) { return m_activeRecordings.Contains( key ); }

        /// <summary>
        /// Beendet eine Aufzeichnung.
        /// </summary>
        /// <param name="key">Die Identifikation der Aufzeichnung.</param>
        /// <returns>Gesetzt, wenn die Aufzeichnung beendet wurde.</returns>
        public bool StopRecording( string key ) { return m_activeRecordings.Remove( key ); }

        /// <summary>
        /// Beginnt eine Aufzeichnung.
        /// </summary>
        /// <param name="key">Die Identifikation der Aufzeichnung.</param>
        public bool StartRecording( string key ) { return m_activeRecordings.Add( key ); }

        /// <summary>
        /// Das zugehörige Gerät.
        /// </summary>
        public readonly Device Device;

        /// <summary>
        /// Die Hintergrundaufgabe zum Einlesen der Quellinformationen.
        /// </summary>
        private CancellableTask<SourceInformation> m_reader;

        /// <summary>
        /// Meldet die Hintergrundaufgabe, mit der die Daten der zugehörigen Quelle ermittelt werden.
        /// </summary>
        public CancellableTask<SourceInformation> SourceInformationReader
        {
            get
            {

                // Create once only
                if (m_reader == null)
                    m_reader = Device.Provider.GetSourceInformationAsync( Device.Index, Source );

                return m_reader;
            }
        }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="source">Die zugehörige Quelle.</param>
        /// <param name="device">Das zugehörige Gerät.</param>
        public Feed( SourceSelection source, Device device )
        {
            Device = device;
            Source = source;
        }

        /// <summary>
        /// Beendet die Nutzung dieses Senders.
        /// </summary>
        public void RefreshSourceInformation()
        {
            if (m_reader != null)
                m_reader.Cancel();

            m_reader = null;
        }

        /// <summary>
        /// Meldet einen Anzeigenamen zu Testzwecken.
        /// </summary>
        /// <returns>Der gewünschte Anzeigename.</returns>
        public override string ToString()
        {
            // Operations
            var op = string.Empty;

            // Views
            if (IsPrimaryView)
                op = " primary";
            else if (IsSecondaryView)
                op = " secondary";

            // Recordings
            foreach (var recording in m_activeRecordings)
                op = string.Format( "{0} record({1})", op, recording );

            // Idle
            if (string.IsNullOrEmpty( op ))
                op = " (idle)";

            // Report
            return string.Format( "{0}{1}", Source.Source, op );
        }

        /// <summary>
        /// Löst eine Änderungsmeldung aus.
        /// </summary>
        /// <param name="sink">Der Empfänger der Meldung.</param>
        /// <param name="show">Der neue Zustand.</param>
        public void OnViewChanged( Action<IFeed, bool> sink, bool show )
        {
            Device.OnViewChanged( sink, this, show );
        }
    }
}
