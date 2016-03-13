using System;
using JMS.DVB.Algorithms;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Beschreibt den Empfang einer Quelle.
    /// </summary>
    public class ActiveStream : IDisposable
    {
        /// <summary>
        /// Die eindeutige Kennung dieser Quelle. Dies Identifikation erlaubt es, eine Quelle mehrfach zu
        /// nutzen.
        /// </summary>
        public SourceIdenfierWithKey SourceKey { get; private set; }

        /// <summary>
        /// Die Verwaltung des Empfangs.
        /// </summary>
        public SourceStreamsManager Manager { get; private set; }

        /// <summary>
        /// Die ursprünglich angeforderte Aufzeichnungskonfiguration.
        /// </summary>
        public StreamSelection RequestedStreams { get; private set; }

        /// <summary>
        /// Der optionale Dateinamen, in dem die empfangenen Daten abgelegt werden.
        /// </summary>
        public string TargetPath { get; private set; }

        /// <summary>
        /// Gesetzt, sobald der Empfang aktiviert wurde.
        /// </summary>
        public bool FirstActivationDone { get; private set; }

        /// <summary>
        /// Die Anzahl der Versuche, die Entschlüsselung neu zu starten.
        /// </summary>
        private int m_DecryptionRestarts;

        /// <summary>
        /// Der Zeitpunkt, an dem das letzte Mal eine Prüfung ausgeführt wurde.
        /// </summary>
        private DateTime m_LastRetest = DateTime.MinValue;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="uniqueIdentifier">Die eindeutige Kennung dieses Datenstroms.</param>
        /// <param name="manager">Die Verwaltung des Empfangs.</param>
        /// <param name="targetPath">Ein optionaler Dateiname zur Ablage der empfangenen Daten.</param>
        /// <param name="originalSelection">Die ursprünglich angeforderte Konfiguration der Aufzeichnung.</param>
        /// <exception cref="ArgumentNullException">Es wurde keine Verwaltung angegeben</exception>
        public ActiveStream( Guid uniqueIdentifier, SourceStreamsManager manager, StreamSelection originalSelection, string targetPath )
        {
            // Validate
            if (manager == null)
                throw new ArgumentNullException( "manager" );

            // Remember
            SourceKey = new SourceIdenfierWithKey( uniqueIdentifier, manager.Source );
            RequestedStreams = originalSelection.Clone();
            TargetPath = targetPath;
            Manager = manager;
        }

        /// <summary>
        /// Deaktiviert die zugehörige Quelle.
        /// </summary>
        public void Close()
        {
            // Forward
            Manager.CloseStream();
        }

        /// <summary>
        /// Prüft, ob die Entschlüsselung funktioniert.
        /// </summary>
        /// <param name="interval">Das vom Profil her eingestellte Prüfintervall.</param>
        public void TestDecryption( TimeSpan interval )
        {
            // Not active
            if (Manager.ConsumerCount < 1)
                return;

            // See if it should be decrypted
            var info = Manager.ActiveInformation;
            if (info == null)
                return;
            if (!info.IsEncrypted)
                return;

            // No retry allowed
            if (m_DecryptionRestarts >= 3)
                return;

            // See if data is coming in
            if (Manager.CurrentAudioVideoBytes > 0)
                return;

            // See when we last tested
            var started = Manager.LastActivationTime;
            if (!started.HasValue)
                return;

            // Not so much
            if ((DateTime.UtcNow - started.Value) <= interval)
                return;

            // Count retries
            m_DecryptionRestarts += 1;

            // Reset decryption
            Manager.Hardware.Decrypt();

            // Restart the stream
            Manager.CloseStream();
        }

        /// <summary>
        /// Aktiviert die Verbraucherkontrolle für diese Quelle.
        /// </summary>
        /// <param name="source">Eine volle Referenz zur Quelle.</param>
        public void EnableOptimizer( SourceSelection source )
        {
            // Just register
            Manager.BeforeRecreateStream += manager =>
            {
                // Create a brand new optimizer
                var localOpt = new StreamSelectionOptimizer();

                // Add the one stream
                localOpt.Add( source, RequestedStreams );

                // Run the optimization
                if (localOpt.Optimize() == 1)
                    return localOpt.GetStreams( 0 );

                // Failed - activation is not possible
                return null;
            };
        }

        /// <summary>
        /// Erzeugt zum aktuellen Empfang einen Informationsblock.
        /// </summary>
        /// <returns>Die gewünschten Informationen.</returns>
        public StreamInformation CreateInformation()
        {
            // See if we are already stopped
            var manager = Manager;
            if (manager == null)
                return null;

            // Report state
            var activeSelection = manager.ActiveSelection;
            return
                new StreamInformation
                {
                    Streams = (activeSelection == null) ? null : activeSelection.Clone(),
                    IsDecrypting = manager.IsDecrypting.GetValueOrDefault( false ),
                    CurrentAudioVideoBytes = manager.CurrentAudioVideoBytes,
                    UniqueIdentifier = SourceKey.UniqueIdentifier,
                    StreamTarget = manager.StreamingTarget,
                    BytesReceived = manager.BytesReceived,
                    ConsumerCount = manager.ConsumerCount,
                    AllFiles = manager.AllFiles,
                    TargetPath = TargetPath,
                    Source = manager.Source,
                };
        }

        /// <summary>
        /// Meldet, ob für die zugehörige Quelle die Auswertung der Programmzeitschrift aktiv ist.
        /// </summary>
        public bool? IsProgamGuideActive
        {
            get
            {
                // Report
                var activeSelection = Manager.ActiveSelection;
                if (activeSelection == null)
                    return null;
                else
                    return activeSelection.ProgramGuide;
            }
        }

        /// <summary>
        /// Startet oder aktualisiert den Empfang der Quelle.
        /// </summary>
        /// <param name="interval">Das Interval zur Erkennung von deaktivierten Quellen.</param>
        public void Refresh( TimeSpan interval )
        {
            // Already stopped
            var manager = Manager;
            if (manager == null)
                return;

            // Get the source information
            var info = manager.GetCurrentInformationAsync().CancelAfter( 15000 ).Result;

            // Must start
            if (!FirstActivationDone)
            {
                // See if source information is available
                if (info == null)
                    return;

                // Start the stream
                FirstActivationDone = manager.CreateStream( TargetPath, info );
            }
            else
            {
                // Skip if it seems that the source has been gone
                if (info == null)
                    if ((DateTime.UtcNow - interval) < m_LastRetest)
                        return;

                // Update according to current station information
                manager.RetestSourceInformation( info );

                // Remember
                m_LastRetest = DateTime.UtcNow;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Quelle.
        /// </summary>
        public void Dispose()
        {
            // Free once
            using (Manager)
                Manager = null;
        }

        #endregion
    }
}
